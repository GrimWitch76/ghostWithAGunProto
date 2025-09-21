using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BT
{
    public class NodeDesciptionAttribute : Attribute
    {
        public string Description { get; }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="description">A default description for the node which can be read by hovering over the node. The description can be overriden through the editor per instance of the node.</param>
        public NodeDesciptionAttribute(string description) => Description = description;
    }
}
