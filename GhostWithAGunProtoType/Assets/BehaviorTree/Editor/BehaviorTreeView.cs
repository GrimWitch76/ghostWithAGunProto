using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine.UIElements;

namespace BT.BTEditor
{
    using Nodes;

    public class BehaviorTreeView : GraphView
    {
        public new class UxmlFactor : UxmlFactory<BehaviorTreeView, GraphView.UxmlTraits> { }
        public NodeScriptGenerator ScriptGenerator { get; internal set; }
        public Action<NodeView> OnNodeSelected;
        public List<ExposedProperty> ExposedProperties = new List<ExposedProperty>();
        public BehaviorTreeEditor BehaviorTreeEditor;
        public NodeSearchWindowProvider SearchWindowProvider;

        private BehaviorTree _tree;
        private List<NodeView> _nodesToCopy = new List<NodeView>();

        private Dictionary<Type, Type> _nodeTypeToNodeViewType = new Dictionary<Type, Type>()
    {
        {typeof(RootNode), typeof(RootNodeView) },
        {typeof(ActionNode), typeof(ActionNodeView) },
        {typeof(CompositeNode), typeof(CompositeNodeView) },
        {typeof(DecoratorNode), typeof(DecorateNodeView) },
    };

        private Dictionary<KeyCode, Action<List<ISelectable>>> _alignmentInputMethods = new Dictionary<KeyCode, Action<List<ISelectable>>>()
        {
            {KeyCode.H, BTViewNodesAligner.AlignHorizontally},
            {KeyCode.V, BTViewNodesAligner.AlignVertically},
            {KeyCode.UpArrow, BTViewNodesAligner.AlignUp},
            {KeyCode.DownArrow, BTViewNodesAligner.AlignDown},
            {KeyCode.LeftArrow, BTViewNodesAligner.AlignLeft},
            {KeyCode.RightArrow, BTViewNodesAligner.AlignRight},
        };


        #region Initialization
        public BehaviorTreeView()
        {
            Insert(0, new GridBackground());

            AddManipulators();

            AddStyleSheets();


            ScriptGenerator = new NodeScriptGenerator();
            SearchWindowProvider = new NodeSearchWindowProvider(this);
            nodeCreationRequest = context => SearchWindow.Open(new SearchWindowContext(context.screenMousePosition), SearchWindowProvider);

            Undo.undoRedoPerformed += OnUndoRedo;
        }

        public void Initialize(BehaviorTreeEditor behaviorTreeEditor)
        {
            BehaviorTreeEditor = behaviorTreeEditor;
            SetupZoom(ContentZoomer.DefaultMinScale, ContentZoomer.DefaultMaxScale);
        }

        private void AddStyleSheets()
        {
            string path = BehaviorTreeSettings.CORE_DIRECTORY + "/Editor/BehaviorTreeEditor.uss";
            var styleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>(path);
            styleSheets.Add(styleSheet);
        }

        private void AddManipulators()
        {
            this.RegisterCallback<KeyDownEvent>(OnKeyDown);
            this.AddManipulator(new ContentZoomer());
            this.AddManipulator(new ContentDragger());
            this.AddManipulator(new SelectionDragger());
            this.AddManipulator(new RectangleSelector());
        }
        #endregion

        private void OnUndoRedo()
        {
            PopulateView(_tree);
            AssetDatabase.SaveAssets();
        }

        private NodeView FindNodeView(BaseNode node)
        {
            //UnityEngine.Debug.Log(node.name);
            return GetNodeByGuid(node.GUID) as NodeView;
        }

        internal void PopulateView(BehaviorTree tree)
        {
            _tree = tree;

            graphViewChanged -= OnGraphViewChanged;
            DeleteElements(graphElements);
            graphViewChanged += OnGraphViewChanged;

            if (_tree.RootNode == null)
            {
                _tree.RootNode = _tree.CreateNode(typeof(RootNode)) as RootNode;
                _tree.RootNode.Position = Vector2.zero;
                EditorUtility.SetDirty(_tree);
                AssetDatabase.SaveAssets();
            }

            tree.Nodes.RemoveAll(n => n == null);

            // create the node view
            foreach(BaseNode node in tree.Nodes)
            {
                if (node == null)
                {
                    Debug.LogError("No node");
                    continue;
                }

                CreateNodeView(node, node.Position);
            }

            // create the edges
            tree.Nodes.ForEach(n =>
            {
                // if the node has children, then create the edge for the connection to those children
                if (n.HasChildren())
                {
                    List<BaseNode> children = n.GetChildren();

                    children.ForEach(c =>
                    {
                        NodeView parentView = FindNodeView(n);
                        NodeView childView = FindNodeView(c);

                        Edge edge = parentView.OutputPort.ConnectTo(childView.InputPort);
                        AddElement(edge);
                    });
                }
            });
        }

        /// <summary>
        /// Called every time the graph view changes or an edge is created. If there are elements of type node view that were deleted in the editor, we delete them from the scriptable object tree.
        /// </summary>
        /// <param name="graphViewChange"></param>
        /// <returns></returns>
        private GraphViewChange OnGraphViewChanged(GraphViewChange graphViewChange)
        {
            if (graphViewChange.elementsToRemove != null)
            {
                graphViewChange.elementsToRemove.ForEach(e =>
                {
                    if (e is NodeView nodeView)
                    {
                        if (nodeView != null)
                        {
                            if (nodeView.Node is RootNode)
                            {
                                EditorUtility.DisplayDialog("Warning", "The Root node cannot be deleted. Reopen the behavior tree and connect the root node back.", "OK");
                            }

                            _tree.DeleteNode(nodeView.Node);
                        }
                    }

                    if (e is Edge edge)
                    {
                        NodeView parentView = edge.output.node as NodeView;
                        NodeView childView = edge.input.node as NodeView;
                        parentView.Node.RemoveChild(childView.Node);
                    }
                });
            }

            if (graphViewChange.edgesToCreate != null)
            {
                var edges = graphViewChange.edgesToCreate;

                for (int i = 0; i < edges.Count; i++)
                {
                    NodeView parentView = edges[i].output.node as NodeView;
                    NodeView childView = edges[i].input.node as NodeView;

                    if (childView != null)
                    {
                        parentView.Node.AddChild(childView.Node);
                    }
                }
            }

            // if any elements were moved, sort the children in the list so that they're performed in order (composite nodes only)
            if (graphViewChange.movedElements != null)
            {
                nodes.ForEach(n =>
                {
                    NodeView view = n as NodeView;
                    view.SortChildren();
                });
            }

            return graphViewChange;
        }

        private NodeView CreateNodeView(BaseNode node, Vector2 position)
        {
            NodeView nodeView = null;
            Type nodeType = node.GetType();
            Type nodeViewType = null;

            foreach (var key in _nodeTypeToNodeViewType.Keys)
            {
                if (key.IsAssignableFrom(nodeType))
                {
                    nodeViewType = _nodeTypeToNodeViewType[key];
                    break;
                }
            }

            if (nodeViewType != null)
            {
                nodeView = Activator.CreateInstance(nodeViewType) as NodeView;
            }

            nodeView.Initialize(node, this);
            nodeView.SetPosition(new Rect(position, Vector2.zero));
            // subscribe so that we call the action whenever the node is selected
            nodeView.OnNodeSelected = OnNodeSelected;
            AddElement(nodeView);

            CheckForExistingConnections(nodeView);
            return nodeView;
        }

        /// <summary>
        /// Checks if the node created was the result of a port drag and drop and connect the new node to the parent/child that was dragged
        /// </summary>
        /// <param name="node"></param>
        private void CheckForExistingConnections(NodeView node)
        {
            if (node.Node is RootNode)
            {
                return;
            }

            if (node.Node is ActionNode)
            {
                ConnectNodeToDraggedFromParent(node);
                return;
            }

            ConnectNodeToDraggedFromParent(node);
            ConnectNodeToDraggedFromChild(node);
        }

        #region Connecting nodes to nodes that were dragged from
        private NodeView _parentNodeDraggedFrom;
        private NodeView _childNodeDraggedFrom;

        public void SetParentNodeDraggedFrom(NodeView node)
        {
            _parentNodeDraggedFrom = node;
        }

        public void SetChildNodeDraggedFrom(NodeView node)
        {
            _childNodeDraggedFrom = node;
        }

        private void ConnectNodeToDraggedFromParent(NodeView node)
        {
            if (_parentNodeDraggedFrom == null)
            {
                return;
            }

            _parentNodeDraggedFrom.Node.AddChild(node.Node);
            _parentNodeDraggedFrom.SortChildren();
            _parentNodeDraggedFrom = null;
            PopulateView(_tree);
        }

        private void ConnectNodeToDraggedFromChild(NodeView node)
        {
            if (_childNodeDraggedFrom == null)
            {
                return;
            }

            node.Node.AddChild(_childNodeDraggedFrom.Node);
            _childNodeDraggedFrom = null;
            PopulateView(_tree);
        }
        #endregion

        public void UpdateNodeStates()
        {
            nodes.ForEach(n =>
            {
                NodeView view = n as NodeView;
                view.UpdateState();
            });
        }


        #region Context menu
        /// <summary>
        /// Adds options for right-clicks
        /// </summary>
        /// <param name="evt"></param>
        public override void BuildContextualMenu(ContextualMenuPopulateEvent evt)
        {
            //base.BuildContextualMenu(evt);
            Vector2 mousePosition = GetLocalMousePosition(evt.mousePosition);

            evt.menu.AppendAction("Copy", a => CopyNode());
            bool pasteAvailable = _nodesToCopy.Count > 0;

            evt.menu.AppendAction("Paste",
                a => PasteNodes(mousePosition),
                a => pasteAvailable ? DropdownMenuAction.Status.Normal : DropdownMenuAction.Status.Disabled);

            bool duplicateAvailable = selection.OfType<NodeView>().ToList().Count > 0;

            evt.menu.AppendAction("Duplicate",
                a => DuplicateNodes(),
                a => duplicateAvailable ? DropdownMenuAction.Status.Normal : DropdownMenuAction.Status.Disabled);

            evt.menu.AppendAction("Align Vertically",
                a => BTViewNodesAligner.AlignVertically(selection));

            evt.menu.AppendAction("Align Horizontally",
                a => BTViewNodesAligner.AlignHorizontally(selection));

            if (selection.OfType<NodeView>().ToList().Count > 0)
            {
                evt.menu.AppendSeparator();
                evt.menu.AppendAction("Do something for node", null);
            }
        }

        public void CreateNode(Type type, Vector2 position)
        {
            BaseNode node = _tree.CreateNode(type);
            CreateNodeView(node, position);
        }

        public void CopyNode()
        {
            var nodesSelected = selection.OfType<NodeView>().ToList();
            _nodesToCopy.Clear();

            if (nodesSelected != null)
            {
                nodesSelected.ForEach(n => _nodesToCopy.Add(n));
            }
        }

        public void PasteNodes(Vector2 mousePosition, List<NodeView>? nodesToCopy = null)
        {
            if (nodesToCopy == null)
            {
                nodesToCopy = _nodesToCopy;
            }

            if (nodesToCopy == null || nodesToCopy.Count == 0)
            {
                return;
            }

            Vector2 middlePoint = Vector2.zero;
            nodesToCopy.ForEach(n => middlePoint += n.Node.Position);
            middlePoint /= nodesToCopy.Count;

            for (int i = 0; i < nodesToCopy.Count; i++)
            {
                Vector2 position = mousePosition + nodesToCopy[i].Node.Position - middlePoint;
                PasteNode(nodesToCopy[i], position);
            }
        }

        private NodeView PasteNode(NodeView nodeToCopy, Vector2 position)
        {
            BaseNode node = _tree.CreateNode(nodeToCopy.Node.GetType());
            EditorUtility.CopySerialized(nodeToCopy.Node, node);
            node.ClearChildren();
            node.GUID = GUID.Generate().ToString();
            return CreateNodeView(node, position);
        }

        public void DuplicateNodes()
        {
            List<NodeView> nodesSelected = new List<NodeView>();
            selection.OfType<NodeView>().ToList().ForEach(n => nodesSelected.Add(n));
            ClearSelection();

            if (nodesSelected.Count == 0)
            {
                return;
            }

            for (int i = 0; i < nodesSelected.Count; i++)
            {
                Vector2 position = nodesSelected[i].Node.Position + Vector2.one * 20;
                NodeView createdNode = PasteNode(nodesSelected[i], position);
                AddToSelection(createdNode);
            }
        }

        #region Shortcut
        private void OnKeyDown(KeyDownEvent evt)
        {
            if (evt.ctrlKey)
            {
                if (evt.keyCode == KeyCode.C)
                {
                    CopyNode();
                    evt.StopPropagation();
                }
                else if (evt.keyCode == KeyCode.V)
                {
                    if (_nodesToCopy != null && _nodesToCopy.Count > 0)
                    {
                        this.ClearSelection();

                        for (int i = 0; i < _nodesToCopy.Count; i++)
                        {
                            Vector2 position = _nodesToCopy[i].Node.Position + Vector2.one * 20;
                            NodeView createdNode = PasteNode(_nodesToCopy[i], position);
                            this.AddToSelection(createdNode);
                        }
                    }

                    evt.StopPropagation();
                }
                else if (evt.keyCode == KeyCode.D)
                {
                    DuplicateNodes();
                    evt.StopPropagation();
                }
            }
            // alignment shortcuts
            else if (evt.shiftKey)
            {
                if(_alignmentInputMethods.ContainsKey(evt.keyCode))
                {
                    _alignmentInputMethods[evt.keyCode].Invoke(selection);
                    evt.StopPropagation();
                }
            }
        }
        #endregion
        #endregion

        public override List<Port> GetCompatiblePorts(Port startPort, NodeAdapter nodeAdapter)
        {
            return ports.ToList().Where(ep =>
            ep.direction != startPort.direction && ep.node != startPort.node).ToList();
        }

        public void AddBlackboard(Blackboard blackboard)
        {
            if (blackboard == null)
            {
                return;
            }

            Add(blackboard);
        }

        #region Utilities
        public Vector2 GetLocalMousePosition(Vector2 mousePosition, bool isSearchWindow = false)
        {
            Vector2 worldMousePosition = mousePosition;

            if (isSearchWindow)
            {
                worldMousePosition -= BehaviorTreeEditor.position.position;
            }

            Vector2 localMousePosition = contentViewContainer.WorldToLocal(worldMousePosition);

            return localMousePosition;
        }
        #endregion
    }
}
