using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace BT.BTEditor
{
    public class ToolBarMenuView : VisualElement
    {
        private ToolbarMenu _dropDownMenu;
        private Toolbar _toolBar;

        public new class UxmlFactory : UxmlFactory<ToolBarMenuView, UxmlTraits> { }

        public void Initialize(BehaviorTreeEditor behaviorTreeEditor)
        {

        }

        public ToolBarMenuView()
        {
            Initialize();
        }

        public void Initialize()
        {

            if (_toolBar == null)
            {
                _toolBar = new Toolbar();
            }

            if(_dropDownMenu == null)
            {
                _dropDownMenu = new ToolbarMenu();
                _dropDownMenu.text = "File";
            }

            _dropDownMenu.menu.MenuItems().Clear();

            // Add items to the dropdown
            _dropDownMenu.menu.AppendAction("Create new behavior tree", CreateNewBehaviorTree);
            AddSeparator();
            _dropDownMenu.menu.AppendAction("Open", OpenBehaviorTree);
            AddRecentlyOpenedOptions();
            _dropDownMenu.menu.AppendAction("Save", (DropdownMenuAction action) => { Debug.Log("Action 3 selected"); }, DropdownMenuAction.AlwaysDisabled);
            AddSeparator();
            _dropDownMenu.menu.AppendAction("Settings", (DropdownMenuAction action) => { Debug.Log("Action 3 selected"); }, DropdownMenuAction.AlwaysDisabled);
            AddSeparator();
            _dropDownMenu.menu.AppendAction("Close", CloseWindow);

            // Add the dropdown menu to the toolbar
            _toolBar.Add(_dropDownMenu);

            // Add the toolbar to the main container
            Add(_toolBar);
        }

        private void CloseWindow(DropdownMenuAction obj)
        {
            BehaviorTreeEditor.CloseWindow();
        }

        private void AddSeparator()
        {
            _dropDownMenu.menu.AppendSeparator();
        }

        private void CreateNewBehaviorTree(DropdownMenuAction action)
        {
            string folderPath = "Assets/YourDefaultFolder"; // Update with your desired default folder

            string path = EditorUtility.SaveFilePanelInProject(
                "Create New Behavior Tree",
                "NewBehaviorTree",
                "asset",
                "Please enter a file name to save the behavior tree to",
                folderPath
            );

            if (!string.IsNullOrEmpty(path))
            {
                BehaviorTree newBehaviorTree = ScriptableObject.CreateInstance<BehaviorTree>();

                AssetDatabase.CreateAsset(newBehaviorTree, path);
                AssetDatabase.SaveAssets();

                EditorUtility.FocusProjectWindow();
                Selection.activeObject = newBehaviorTree;

                EditorUtility.FocusProjectWindow();
                BehaviorTreeEditor.OpenBehaviorTree(newBehaviorTree);
            }
        }

        private void OpenBehaviorTree(DropdownMenuAction action)
        {
            string folderPath = "Assets/"; // Update with your specific folder path

            string path = EditorUtility.OpenFilePanelWithFilters("Select Behavior Tree", folderPath, new string[] { "Behavior Tree", "asset" });

            if (!string.IsNullOrEmpty(path))
            {
                // Load the selected asset
                string assetPath = "Assets" + path.Substring(Application.dataPath.Length);
                Object selectedAsset = AssetDatabase.LoadAssetAtPath<Object>(assetPath);

                if (selectedAsset is BehaviorTree behaviorTree)
                {
                    // Perform your custom action here
                    Debug.Log("BehaviorTree selected: " + selectedAsset.name);
                    BehaviorTreeEditor.OpenBehaviorTree(behaviorTree);
                }
                else
                {
                    EditorUtility.DisplayDialog("Error", "You can only open behavior tree files.", "OK");
                }
            }
        }

        private void AddRecentlyOpenedOptions()
        {
            List<BehaviorTree> lastBTs = BehaviorTreeSettings.GetSettings().GetRecentBehaviorTrees();

            if(lastBTs == null || lastBTs.Count == 0)
            {
                _dropDownMenu.menu.AppendAction("Open Recent", null, DropdownMenuAction.Status.Disabled);
                return;
            }

            for (int i = 0; i < lastBTs.Count; i++)
            {
                BehaviorTree bt = lastBTs[i];
                _dropDownMenu.menu.AppendAction($"Open Recent/{bt.name}", (DropdownMenuAction action) => { AddRecentBehaviorTree(bt); });
            }
        }

        private void AddRecentBehaviorTree(BehaviorTree behaviorTree)
        {
            if(behaviorTree == null)
            {
                EditorUtility.DisplayDialog("Error", "Selected Behavior Tree doesn't exist", "OK");
                BehaviorTreeSettings.GetSettings().ClearNullBehaviorTrees();
                Initialize();
                return;
            }

            EditorUtility.FocusProjectWindow();
            Selection.activeObject = behaviorTree;

            EditorUtility.FocusProjectWindow();
            BehaviorTreeEditor.OpenBehaviorTree(behaviorTree);
        }
    }
}
