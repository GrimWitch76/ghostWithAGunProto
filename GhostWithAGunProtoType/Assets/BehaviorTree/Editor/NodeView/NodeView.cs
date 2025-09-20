using System;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEditor.Experimental.GraphView;

namespace BT.BTEditor
{
    using Nodes;
    using System.Reflection;
    using static BT.Nodes.BaseNode;

    public abstract class NodeView : Node
    {
        public abstract string StyleClassName { get; set; }
        public Action<NodeView> OnNodeSelected;
        public BaseNode Node;
        public Port InputPort;
        public Port OutputPort;

        private Label _stateLabel;
        private static BehaviorTreeView _view;
        private NodeState _currentState = NodeState.NONE;
        private string _lastAddedClass = string.Empty;

        private const string NODE_VIEW_UXML_DIRECTORY = BehaviorTreeSettings.CORE_DIRECTORY + "Editor/NodeView/NodeView.uxml";
        private const string NODE_ICONS_PATH = BehaviorTreeSettings.CORE_DIRECTORY + "/Icons/NodesIcons/";

        public NodeView() : base(NODE_VIEW_UXML_DIRECTORY)
        {
        }

        public override void OnSelected()
        {
            base.OnSelected();

            OnNodeSelected?.Invoke(this);
        }

        #region Set up
        public virtual void Initialize(BaseNode node, BehaviorTreeView view)
        {
            _view = view;
            this.Node = node;
            this.viewDataKey = node.GUID;
            _stateLabel = this.Q<Label>("state-title");
            _stateLabel.visible = false;

            SetNodeInitialPosition();

            CreateInputPort();
            CreateOutputPort();

            VisualElement bg = this.Q<VisualElement>("title-background");

            bg.AddToClassList(StyleClassName);

            BindTitleLabelToName();
            BindTooltipToDescription();
            Stylize();
            SetClassNameLabel();

            SetDescriptionFromAttribute();
        }

        private void SetDescriptionFromAttribute()
        {
            NodeDesciptionAttribute attribute = Node.GetType().GetCustomAttribute<NodeDesciptionAttribute>();

            if (String.IsNullOrEmpty(Node.ViewDetails.Description) && attribute != null)
            {
                Node.ViewDetails.Description = attribute.Description;
            }
        }

        private void SetNodeInitialPosition()
        {
            style.left = Node.Position.x;
            style.top = Node.Position.y;
        }

        private void BindTitleLabelToName()
        {
            Label titleLabel = this.Q<Label>("title-label");

            if(titleLabel == null)
            {
                Debug.LogError("No title label");
                return;
            }

            if(Node == null)
            {
                Debug.LogError("No node passed");
                return;
            }

            titleLabel.bindingPath = "ViewDetails.Name";
            titleLabel.Bind(new SerializedObject(Node));

            if (Node.ViewDetails.Name.Length == 0)
            {
                string nodeName = Node.GetType().Name;
                //nodeName = nodeName.
                Node.ViewDetails.Name = nodeName;
                titleLabel.text = Node.ViewDetails.Name;
            }
        }

        private void BindTooltipToDescription()
        {
            RegisterCallback<MouseEnterEvent>(UpdateTooltip);
        }

        private void UpdateTooltip(MouseEnterEvent evt)
        {
            tooltip = Node.ViewDetails.Description;
        }

        protected abstract void CreateInputPort();
        protected abstract void CreateOutputPort();
        #endregion

        #region Utilities
        public override void SetPosition(Rect newPos)
        {
            base.SetPosition(newPos);

            Undo.RecordObject(Node, "Behavior Tree (Set Position)");

            Node.Position.x = newPos.xMin;
            Node.Position.y = newPos.yMin;

            EditorUtility.SetDirty(Node);
        }

        #region Ports
        protected void CreatePort(Orientation orientation, Direction direction, Port.Capacity capacity)
        {
            new NodeViewPort(this, _view, orientation, direction, capacity);
        }
        #endregion

        #region Sort children based on position
        public void SortChildren()
        {
            CompositeNode composite = Node as CompositeNode;

            if (composite)
            {
                composite.Children.Sort(SortByHorizontalPosition);
            }
        }

        private int SortByHorizontalPosition(BaseNode left, BaseNode right)
        {
            return left.Position.x < right.Position.x ? -1 : 1;
        }
        #endregion
        #endregion


        /// <summary>
        /// Updates the border of the node depending on its state if the application is running
        /// </summary>
        public void UpdateState()
        {
            if (!Application.isPlaying)
            {
                if (_stateLabel.visible)
                {
                    _stateLabel.visible = false;
                }

                return;
            }

            if (Node.State == _currentState)
            {
                return;
            }

            RemoveFromClassList(_lastAddedClass);

            NodeState state = Node.State;
            _currentState = state;
            _lastAddedClass = state.ToString().ToLower();
            _stateLabel.text = _lastAddedClass;
            AddToClassList(_lastAddedClass);
            _stateLabel.visible = true;
        }

        private void Stylize()
        {
            string path = NODE_ICONS_PATH;

            if (Node is SequenceNode)
            {
                path += "T_Sequence.png";
            }
            else if (Node is SelectorNode)
            {
                path += "T_Selector.png";
            }
            else if (Node is ParallelNode)
            {
                path += "T_Parallel.png";
            }
            else if (Node is LoopNode)
            {
                path += "T_Loop.png";
            }
            else if (Node is SucceederNode)
            {
                path += "T_Success.png";
            }
            else if (Node is FailureNode)
            {
                path += "T_Failure.png";
            }
            else if (Node is CooldownNode)
            {
                path += "T_Cooldown.png";
            }
            else if(Node is TimeoutNode)
            {
                path += "T_Timeout.png";
            }
            else if (Node is ConditionalCheckNode)
            {
                path += "T_Check.png";
            }
            else if (Node is ConditionalLoopNode)
            {
                path += "T_ConditionalCheck.png";
            }
            else if (Node is RootNode)
            {
                path += "T_Root.png";
            }
            else if (Node is InvertNode)
            {
                path += "T_Invert.png";
            }
            else if (Node is RepeatNode)
            {
                path += "T_Repeat.png";
            }
            else if (Node is BehaviorTreeNode)
            {
                path += "T_BehaviorTree.png";
            }
            else if (Node is ActionNode)
            {
                path += "T_Action.png";
            }
            else if (Node is ForceStateNode)
            {
                path += "T_ForceState.png";
            }

            Texture2D gradient = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
            VisualElement icon = this.Q<VisualElement>("node-icon");
            icon.style.backgroundImage = gradient;
        }

        private void SetClassNameLabel()
        {
            TextElement classText = this.Q<TextElement>("script-name");

            if(classText == null)
            {
                return;
            }

            string name = Node.GetType().Name.Replace("Node", string.Empty);
            name = name.Replace("AN", string.Empty);
            name = name.Replace("CN", string.Empty);
            name = name.Replace("DN", string.Empty);

            classText.text = name;
        }
    }

    public class CustomEdgeConnectorListener : IEdgeConnectorListener
    {
        private bool _connectionSuccessful;

        public bool ConnectionSuccessful => _connectionSuccessful;

        public void OnDropOutsidePort(Edge edge, Vector2 position)
        {
            // The drop was outside a port, so the connection is not successful
            _connectionSuccessful = false;
        }

        public void OnDrop(GraphView graphView, Edge edge)
        {
            // The drop was onto a valid port, check if both ends are connected
            if (edge.input != null && edge.output != null)
            {
                _connectionSuccessful = true;
            }
            else
            {
                _connectionSuccessful = false;
            }
        }
    }

}