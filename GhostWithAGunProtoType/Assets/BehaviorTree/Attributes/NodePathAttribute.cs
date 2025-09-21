using System;

namespace BT
{
    [AttributeUsage(AttributeTargets.Class, Inherited = false)]
    public class NodePathAttribute : Attribute
    {
        public string Path { get; }
        public NodePathAttribute(string path) => Path = path;
    }
}
