using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEditor;

namespace BT.BTEditor
{
    public class NodeSearchWindowProvider : ScriptableObject, ISearchWindowProvider
    {
        private BehaviorTreeView _graphView;
        private NodeScriptGenerator _scriptGenerator;
        private BehaviorTreeEditor _behaviorTreeEditor;
        private Texture2D _icon;
        public static NodeSearchWindowProvider Instance;
        private BehaviorTreeSearchDirectories _nodesDictionary;

        public NodeSearchWindowProvider(BehaviorTreeView treeView)
        {
            Instance = this;
            _graphView = treeView;
            _scriptGenerator = _graphView.ScriptGenerator;
            _behaviorTreeEditor = _graphView.BehaviorTreeEditor;
            _icon = new Texture2D(1, 1);
            _icon.SetPixel(0, 0, new Color(0, 0, 0, 0));
            _icon.Apply();
        }

        public void Initialize(BehaviorTreeEditor editor)
        {
            _scriptGenerator = _graphView.ScriptGenerator;
        }

        public List<SearchTreeEntry> CreateSearchTree(SearchWindowContext context)
        {
            List<SearchTreeEntry> tree = new List<SearchTreeEntry>();
            // Root node
            SearchTreeGroupEntry rootGroup = new SearchTreeGroupEntry(new GUIContent("Nodes Editor"), 0);
            tree.Add(rootGroup);

            if (_behaviorTreeEditor == null)
            {
                _behaviorTreeEditor = _graphView.BehaviorTreeEditor;
            }

            //if(_behaviorTreeEditor.BehaviorTree == null)
            if (BehaviorTreeEditor.SelectedBehaviorTree == null)
            {
                Debug.LogError("No bt");
            }

            //if (_behaviorTreeEditor != null && _behaviorTreeEditor.BehaviorTree != null)
            if (_behaviorTreeEditor != null && BehaviorTreeEditor.SelectedBehaviorTree != null)
            {
                GenerateNodeEntries(tree, rootGroup);
            }

            GenerateNodesScriptCreationEntries(tree);
            return tree;
        }

        private void GenerateNodeEntries(List<SearchTreeEntry> tree, SearchTreeGroupEntry rootGroup)
        {
            _nodesDictionary = GetMenuOptionsDictionary();

            _nodesDictionary.RefreshDictionary();

            var sortedDirectories = _nodesDictionary.Directories.OrderBy(d => d.Directory).ToList();

            // cache for tracking existing group paths
            var groupPaths = new Dictionary<string, SearchTreeGroupEntry>();

            groupPaths["Create Node"] = rootGroup;

            foreach (NodeSearchDirectory directory in sortedDirectories)
            {
                string[] branches = directory.Directory.Split('/');

                if (branches.Length == 0)
                {
                    continue;
                }

                string nodeName = branches.Last();
                string currentPath = "";

                // start from the root
                SearchTreeGroupEntry parentGroup = rootGroup;

                for (int i = 0; i < branches.Length - 1; i++)
                {
                    string category = branches[i];
                    currentPath = string.IsNullOrEmpty(currentPath) ? category : $"{currentPath}/{category}";

                    if (!groupPaths.TryGetValue(currentPath, out var group))
                    {
                        // create a new group if not existing
                        group = new SearchTreeGroupEntry(new GUIContent(category), parentGroup.level + 1);
                        tree.Add(group);
                        groupPaths[currentPath] = group;
                    }

                    // set the current group as parent for the next level
                    parentGroup = group;
                }

                SearchTreeEntry nodeEntry = new SearchTreeEntry(new GUIContent(nodeName, _icon))
                {
                    level = parentGroup.level + 1,
                    userData = directory.ClassName,
                };

                tree.Add(nodeEntry);
            }
        }

        private BehaviorTreeSearchDirectories GetMenuOptionsDictionary()
        {
            if (_nodesDictionary != null)
            {
                return _nodesDictionary;
            }

            _nodesDictionary = AssetDatabase.FindAssets($"t:{nameof(BehaviorTreeSearchDirectories)}")
                   .Select(guid => AssetDatabase.LoadAssetAtPath<BehaviorTreeSearchDirectories>(AssetDatabase.GUIDToAssetPath(guid)))
                   .FirstOrDefault();

            // if there is no dictionary, then create one
            if (_nodesDictionary == null)
            {
                string path = BehaviorTreeSettings.CORE_DIRECTORY;
                _nodesDictionary = ScriptableObject.CreateInstance<BehaviorTreeSearchDirectories>();
                AssetDatabase.CreateAsset(_nodesDictionary, path);
                AssetDatabase.SaveAssets();
            }

            return _nodesDictionary;
        }

        private void GenerateNodesScriptCreationEntries(List<SearchTreeEntry> tree)
        {
            var types = Enum.GetValues(typeof(NodeType));

            foreach (NodeType type in types)
            {
                string optionName = ObjectNames.NicifyVariableName(type.ToString());
                var nodeEntry = new SearchTreeEntry(new GUIContent($"Create new {optionName}", _icon))
                {
                    level = 1,
                    userData = type
                };

                tree.Add(nodeEntry);
            }
        }

        public bool OnSelectEntry(SearchTreeEntry searchTreeEntry, SearchWindowContext context)
        {
            Vector2 worldMousePosition = _graphView.GetLocalMousePosition(context.screenMousePosition, true);

            if (searchTreeEntry.userData is NodeType type)
            {
                _scriptGenerator.GenerateNodeScript(type);
                return true;
            }

            Type nodeType = Type.GetType($"{searchTreeEntry.userData as string}, Assembly-CSharp");
            //Type nodeType = Type.GetType($"{typeof(BaseNode).Namespace}.{searchTreeEntry.userData as string}, Assembly-CSharp");

            if (nodeType == null)
            {
                Debug.LogError($"Couldn't create the node {searchTreeEntry.userData}. Type mismatch.");
                return false;
            }

            _graphView.CreateNode(nodeType, worldMousePosition);

            return true;
        }
    }
}