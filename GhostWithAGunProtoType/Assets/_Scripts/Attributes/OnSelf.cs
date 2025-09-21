using UnityEngine;

namespace TCustomAttributes
{
    /// <summary>
    /// Attribute to get a reference to a component in the GameObject itself.
    /// </summary>
    public class OnSelf : PropertyAttribute
    {
        /// <summary>
        /// If true, the attribute will add the component to the GameObject if it doesn't exist.
        /// </summary>
        public bool ForceReference { get; }

        public OnSelf(bool forceReference = false)
        {
            ForceReference = forceReference;
        }
    }
}
