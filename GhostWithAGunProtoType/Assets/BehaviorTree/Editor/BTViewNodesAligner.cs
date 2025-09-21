using System.Collections.Generic;
using System.Linq;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

namespace BT.BTEditor
{
    public class BTViewNodesAligner
    {
        public static void AlignVertically(List<ISelectable> selection)
        {
            List<NodeView> nodesSelected = new List<NodeView>();
            selection.OfType<NodeView>().ToList().ForEach(n => nodesSelected.Add(n));

            if (nodesSelected.Count < 2)
            {
                return;
            }

            Vector2 size = nodesSelected[0].GetPosition().size;
            Vector2 position = nodesSelected[0].GetPosition().position;

            for (int i = 1; i < nodesSelected.Count; i++)
            {
                var node = nodesSelected[i];
                Vector2 nodeSize = node.GetPosition().size;

                Vector2 newPosition = new Vector2(node.GetPosition().position.x, position.y - (nodeSize.y - size.y) / 2);
                nodesSelected[i].SetPosition(new Rect(newPosition, nodesSelected[i].GetPosition().size));
            }
        }

        public static void AlignHorizontally(List<ISelectable> selection)
        {
            List<NodeView> nodesSelected = new List<NodeView>();
            selection.OfType<NodeView>().ToList().ForEach(n => nodesSelected.Add(n));

            if (nodesSelected.Count < 2)
            {
                return;
            }

            Vector2 size = nodesSelected[0].GetPosition().size;
            Vector2 position = nodesSelected[0].GetPosition().position;

            for (int i = 1; i < nodesSelected.Count; i++)
            {
                var node = nodesSelected[i];
                Vector2 nodeSize = node.GetPosition().size;

                Vector2 newPosition = new Vector2(position.x - (nodeSize.x - size.x) / 2, node.GetPosition().position.y);
                nodesSelected[i].SetPosition(new Rect(newPosition, nodesSelected[i].GetPosition().size));
            }
        }


        public static void AlignLeft(List<ISelectable> selection)
        {
            List<NodeView> nodesSelected = new List<NodeView>();
            selection.OfType<NodeView>().ToList().ForEach(n => nodesSelected.Add(n));

            if (nodesSelected.Count < 2)
            {
                return;
            }

            Vector2 position = nodesSelected.OrderBy(n => n.GetPosition().position.x).FirstOrDefault().GetPosition().position;

            for (int i = 0; i < nodesSelected.Count; i++)
            {
                var node = nodesSelected[i];
                Vector2 nodeSize = node.GetPosition().size;

                Vector2 newPosition = new Vector2(position.x, node.GetPosition().position.y);
                nodesSelected[i].SetPosition(new Rect(newPosition, nodesSelected[i].GetPosition().size));
            }
        }

        public static void AlignRight(List<ISelectable> selection)
        {
            List<NodeView> nodesSelected = new List<NodeView>();
            selection.OfType<NodeView>().ToList().ForEach(n => nodesSelected.Add(n));

            if (nodesSelected.Count < 2)
            {
                return;
            }

            var rightMostNode = nodesSelected.OrderBy(n => n.GetPosition().xMax).LastOrDefault();
            float xMax = rightMostNode.GetPosition().xMax;

            for (int i = 0; i < nodesSelected.Count; i++)
            {
                var node = nodesSelected[i];

                if (rightMostNode == node)
                {
                    continue;
                }

                Vector2 nodeSize = node.GetPosition().size;

                Vector2 newPosition = new Vector2(xMax - nodeSize.x, node.GetPosition().position.y);
                nodesSelected[i].SetPosition(new Rect(newPosition, nodeSize));
            }
        }

        public static void AlignUp(List<ISelectable> selection)
        {
            List<NodeView> nodesSelected = new List<NodeView>();
            selection.OfType<NodeView>().ToList().ForEach(n => nodesSelected.Add(n));

            if (nodesSelected.Count < 2)
            {
                return;
            }

            float yPoint = nodesSelected.OrderBy(n => n.GetPosition().position.y).FirstOrDefault().GetPosition().position.y;

            for (int i = 0; i < nodesSelected.Count; i++)
            {
                var node = nodesSelected[i];
                Vector2 nodeSize = node.GetPosition().size;

                Vector2 newPosition = new Vector2(node.GetPosition().position.x, yPoint);
                nodesSelected[i].SetPosition(new Rect(newPosition, nodesSelected[i].GetPosition().size));
            }
        }

        public static void AlignDown(List<ISelectable> selection)
        {
            List<NodeView> nodesSelected = new List<NodeView>();
            selection.OfType<NodeView>().ToList().ForEach(n => nodesSelected.Add(n));

            if (nodesSelected.Count < 2)
            {
                return;
            }

            var downMostNode = nodesSelected.OrderBy(n => n.GetPosition().yMax).LastOrDefault();
            float yPoint = downMostNode.GetPosition().yMax;

            for (int i = 0; i < nodesSelected.Count; i++)
            {
                var node = nodesSelected[i];

                if (downMostNode == node)
                {
                    continue;
                }

                Vector2 nodeSize = node.GetPosition().size;

                Vector2 newPosition = new Vector2(node.GetPosition().position.x, yPoint - nodeSize.y);
                nodesSelected[i].SetPosition(new Rect(newPosition, nodeSize));
            }
        }
    }
}
