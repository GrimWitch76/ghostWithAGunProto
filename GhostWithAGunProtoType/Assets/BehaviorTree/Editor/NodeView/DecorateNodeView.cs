using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

namespace BT.BTEditor
{
    public class DecorateNodeView : NodeView
    {
        [HideInInspector]
        public override string StyleClassName { get; set; } = "decorator";

        protected override void CreateInputPort()
        {
            CreatePort(Orientation.Vertical, Direction.Input, Port.Capacity.Single);
        }

        protected override void CreateOutputPort()
        {
            CreatePort(Orientation.Vertical, Direction.Output, Port.Capacity.Single);
        }
    }
}
