using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace BT.BTEditor
{
    /// <summary>
    /// This class creates, adds style, and configure the drag and drop functionality of nodes' ports.
    /// </summary>
    public class NodeViewPort
    {
        private CustomEdgeConnectorListener _customEdgeConnectorListener;
        private EventCallback<MouseMoveEvent> _onPortDragMove;
        private EventCallback<MouseUpEvent> _onPortDragEnd;
        private static BehaviorTreeView _view;
        private NodeView _node;
        private Port _port;

        public NodeViewPort(NodeView node, BehaviorTreeView view, Orientation orientation, Direction direction, Port.Capacity capacity)
        {
            _view = view;
            _node = node;
            CreatePort(orientation, direction, capacity);
        }

        private void CreatePort(Orientation orientation, Direction direction, Port.Capacity capacity)
        {
            _port = _node.InstantiatePort(orientation, direction, capacity, typeof(bool));

            // position and size the inner layer of the port
            VisualElement connector = _port.Q<VisualElement>("connector");
            connector.pickingMode = PickingMode.Position;
            connector.style.height = 100;
            connector.style.width = 100;
            connector.style.borderBottomWidth = 0;
            connector.style.borderTopWidth = 0;

            if (direction == Direction.Output)
            {
                connector.style.borderBottomRightRadius = 0;
                connector.style.borderBottomLeftRadius = 0;
            }
            else
            {
                connector.style.borderTopRightRadius = 0;
                connector.style.borderTopLeftRadius = 0;
            }

            // position and size the outer layer of the port
            VisualElement cap = _port.Q<VisualElement>("cap");
            cap.pickingMode = PickingMode.Position;
            cap.style.height = 100;
            cap.style.width = 100;

            if (direction == Direction.Output)
            {
                cap.style.borderBottomRightRadius = 0;
                cap.style.borderBottomLeftRadius = 0;
            }
            else
            {
                cap.style.borderTopRightRadius = 0;
                cap.style.borderTopLeftRadius = 0;
            }

            Label label = _port.Q<Label>("type");
            label.RemoveFromHierarchy();

            if (direction == Direction.Input)
            {
                _port.style.flexDirection = FlexDirection.Column;
                _node.inputContainer.Add(_port);
                _node.InputPort = _port;
            }
            else
            {
                _port.style.flexDirection = FlexDirection.ColumnReverse;
                _node.outputContainer.Add(_port);
                _node.OutputPort = _port;
            }

            _port.RegisterCallback<MouseDownEvent>(evt => OnPortDragStart(evt, _port));

            _customEdgeConnectorListener = new CustomEdgeConnectorListener();
            EdgeConnector<Edge> edgeConnector = new EdgeConnector<Edge>(_customEdgeConnectorListener);
            _port.AddManipulator(edgeConnector);
        }

        private void OnPortDragStart(MouseDownEvent evt, Port port)
        {
            if (evt.button != 0) // Only handle left mouse button
            {
                return;
            }

            port.CaptureMouse();

            // we cache the lambdas so that we can unregister them later
            _onPortDragMove = e => OnPortDragMove(e);
            _onPortDragEnd = e => OnPortDragEnd(e);
            port.RegisterCallback(_onPortDragMove);
            port.RegisterCallback(_onPortDragEnd);
        }

        /// <summary>
        /// For moving the dragging around
        /// </summary>
        /// <param name="evt"></param>
        /// <param name="port"></param>
        private void OnPortDragMove(MouseMoveEvent evt)
        {

        }

        private void OnPortDragEnd(MouseUpEvent evt)
        {
            if (evt.button != 0)
            {
                return;
            }

            _port.ReleaseMouse();

            _port.UnregisterCallback(_onPortDragMove);
            _port.UnregisterCallback(_onPortDragEnd);

            if (_customEdgeConnectorListener.ConnectionSuccessful)
            {
                return;
            }

            Vector2 endPosition = _view.LocalToWorld(evt.mousePosition);

            if (!IsReleasedInEmptyArea(endPosition))
            {
                return;
            }

            OpenSearchMenu(endPosition);
        }

        private bool IsReleasedInEmptyArea(Vector2 position)
        {
            foreach (var port in _view.ports.ToList())
            {
                Rect portBounds = port.worldBound;

                if (portBounds.Contains(position))
                {
                    return false;
                }
            }

            return true;
        }

        private void OpenSearchMenu(Vector2 position)
        {
            if (_port.direction == Direction.Output)
            {
                _view.SetParentNodeDraggedFrom(_node);
            }
            else
            {
                _view.SetChildNodeDraggedFrom(_node);
            }

            Rect graphViewRect = _view.contentContainer.worldBound;

            // Adjust the position for maximized window vs. non-maximized
            Vector2 adjustedPosition;

            if (IsWindowMaximized())
            {
                Debug.Log("Maximized");
                // If the window is maximized, adjust the position normally
                adjustedPosition = position - graphViewRect.position;
            }
            else
            {
                adjustedPosition = position;
                // If the window is not maximized, apply a different adjustment (depending on your layout, you may need a different approach)
                //adjustedPosition = position - _view.LocalToWorld(Vector2.zero);
            }

            SearchWindowContext context = new SearchWindowContext(adjustedPosition);
            SearchWindow.Open(context, NodeSearchWindowProvider.Instance);
        }

        private bool IsWindowMaximized()
        {
            return EditorWindow.focusedWindow.maximized;
        }
    }
}