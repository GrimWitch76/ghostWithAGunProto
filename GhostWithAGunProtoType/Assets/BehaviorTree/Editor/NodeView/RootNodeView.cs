using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

namespace BT.BTEditor
{
    public class RootNodeView : NodeView
    {
        [HideInInspector]
        public override string StyleClassName { get; set; } = "root";

        protected override void CreateInputPort()
        {

        }

        protected override void CreateOutputPort()
        {
            CreatePort(Orientation.Vertical, Direction.Output, Port.Capacity.Single);
        }
    }
}
