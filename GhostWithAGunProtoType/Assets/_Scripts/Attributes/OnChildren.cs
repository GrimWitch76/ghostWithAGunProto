using UnityEngine;

namespace TCustomAttributes
{
    /// <summary>
    /// Attribute to get a reference to a component in the children of the GameObject.
    /// </summary>
    public class OnChildren : PropertyAttribute
    {
        /// <summary>
        /// If true, the attribute will add a child gameobject with the required component if it doesn't exist.
        /// </summary>
        public bool ForceReference { get; }
        public int InstanceIndex { get; }

        public OnChildren(bool forceReference = false, int instanceIndex = 0)
        {
            ForceReference = forceReference;
            InstanceIndex = instanceIndex;
        }
    }
}
