using UnityEngine;

namespace TCustomAttributes
{
    public class ChildByTagAttribute : PropertyAttribute
    {
        public string Tag { get; }

        public ChildByTagAttribute(string tag)
        {
            Tag = tag;
        }
    }
}