using System.IO;
using UnityEditor;
using UnityEngine;

namespace BT
{
    public enum NodeType
    {
        Action = 0,
        Composite = 1, 
        Decorator = 2,
        ConditionalCheck = 3, 
        ConditionalLoop = 4
    }

    public class NodeScriptGenerator
    {
        private const string ACTION_NODE = "using UnityEngine;\n\n" +
            "namespace BT.Nodes\n{\n\t" +
            "\t[NodePath(\"New Nodes/#SCRIPTNAME#\")]\n" +
            "\tpublic class #SCRIPTNAME# : ActionNode" +
            "\n\t{" +
            "\n\t\tprotected override NodeState OnStart()\n\t\t{\n\t\t\t// start logic\n\t\t\treturn NodeState.Running;\n\t\t}" +
            "\n\n\t\tprotected override NodeState OnUpdate()\n\t\t{\n\t\t\t// update logic\n\t\t\treturn NodeState.Success;\n\t\t}" +
            "\n\n\t\tprotected override void OnExit()\n\t\t{\n\t\t\t// stop logic\n\t\t}" +
            "\n\t}\n}";

        private const string COMPOSITE_NODE = "using UnityEngine;\n\n" +
            "namespace BT.Nodes\n{\n\t" +
            "public class #SCRIPTNAME# : CompositeNode" +
            "\n\t{" +
            "\n\t\tprotected override NodeState OnStart()\n\t\t{\n\t\t\t// start logic\n\t\t\treturn NodeState.Running;\n\t\t}" +
            "\n\n\t\tprotected override NodeState OnUpdate()\n\t\t{\n\t\t\t// update logic\n\t\t\treturn NodeState.Success;\n\t\t}" +
            "\n\n\t\tprotected override void OnExit()\n\t\t{\n\t\t\t// end logic\n\t\t}" +
            "\n\t}\n}";

        private const string DECORATOR_NODE = "using UnityEngine;\n\n" +
            "namespace BT.Nodes\n{\n\t" +
            "public class #SCRIPTNAME# : DecoratorNode" +
            "\n\t{" +
            "\n\t\tprotected override NodeState OnStart()\n\t\t{\n\t\t\t// start logic\n\t\t\treturn NodeState.Running;\n\t\t}" +
            "\n\n\t\tprotected override NodeState OnUpdate()\n\t\t{\n\t\t\t// update logic\n\t\t\treturn NodeState.Success;\n\t\t}" +
            "\n\n\t\tprotected override void OnExit()\n\t\t{\n\t\t\t// end logic\n\t\t}" +
            "\n\t}\n}";

        private const string CONDITIONAL_CHECK_NODE = "using UnityEngine;\n\n" +
            "namespace BT.Nodes\n{\n\t" +
            "public class #SCRIPTNAME# : ConditionalCheckNode" +
            "\n\t{" +
            "\n\t\tprotected override bool IsTrue()" +
            "\n\t\t{\n\t\t\t// condition\n\t\t\treturn true;\n\t\t}" +
            "\n\t}\n}";

        private const string CONDITIONAL_LOOP_NODE = "using UnityEngine;\n\n" +
            "namespace BT.Nodes\n{\n\t" +
            "public class #SCRIPTNAME# : ConditionalLoopNode" +
            "\n\t{" +
            "\n\t\tprotected override bool IsTrue()" +
            "\n\t\t{\n\t\t\t// condition. If true, the loop will go on, otherwise, it will stop.\n\t\t\treturn true;\n\t\t}" +
            "\n\t}\n}";

        public void GenerateNodeScript(NodeType type)
        {
            switch(type)
            {
                case NodeType.Action:
                    CreateScriptFromTemplate(ACTION_NODE);
                    break;
                case NodeType.Composite:
                    CreateScriptFromTemplate(COMPOSITE_NODE);
                    break;
                case NodeType.Decorator:
                    CreateScriptFromTemplate(DECORATOR_NODE);
                    break;
                case NodeType.ConditionalCheck:
                    CreateScriptFromTemplate(CONDITIONAL_CHECK_NODE);
                    break;
                case NodeType.ConditionalLoop:
                    CreateScriptFromTemplate(CONDITIONAL_LOOP_NODE);
                    break;
            }
        }

        private static void CreateScriptFromTemplate(string template)
        {
            // Prompt for file save location and name
            string path = EditorUtility.SaveFilePanel("Save Script", Application.dataPath, "NewNode", "cs");

            // Check if the user canceled the save operation
            if (string.IsNullOrEmpty(path))
            {
                return;
            }

            // Get the file name without extension to use as the script class name
            string fileName = Path.GetFileNameWithoutExtension(path);

            // Replace the placeholder with the actual class name
            string fileContent = template.Replace("#SCRIPTNAME#", fileName);

            // Write the content to the selected path
            File.WriteAllText(path, fileContent);

            // Convert the absolute path to a relative path for Unity's AssetDatabase
            string relativePath = "Assets" + path.Substring(Application.dataPath.Length);

            // Import the newly created script into the Unity project
            AssetDatabase.ImportAsset(relativePath);

            // Load the script asset
            MonoScript asset = AssetDatabase.LoadAssetAtPath<MonoScript>(relativePath);

            // Show the created asset in the project window
            ProjectWindowUtil.ShowCreatedAsset(asset);
        }

        private class DoCreateScriptAsset : UnityEditor.ProjectWindowCallback.EndNameEditAction
        {
            public string Template { get; set; }

            public override void Action(int instanceId, string pathName, string resourceFile)
            {
                var fileName = Path.GetFileNameWithoutExtension(pathName);
                var fileContent = Template.Replace("#SCRIPTNAME#", fileName);
                File.WriteAllText(pathName, fileContent);
                AssetDatabase.ImportAsset(pathName);
                var asset = AssetDatabase.LoadAssetAtPath<MonoScript>(pathName);
                ProjectWindowUtil.ShowCreatedAsset(asset);
            }
        }
    }
}
