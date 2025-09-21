using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.Callbacks;
using UnityEditor.Experimental.GraphView;

namespace BT.BTEditor
{
    public class BehaviorTreeEditor : EditorWindow
    {
        private BehaviorTreeView _treeView;
        private InspectorView _inspectorView;
        public static BehaviorTree SelectedBehaviorTree;
        private static BehaviorTreeEditor Window;

        private MiniMap _minimap;
        private Toggle _minimapToggle;
        //private Toggle _blackboardToggle;
        private CustomBlackboard _blackboard;
        private static Texture _tabIcon;

        private const string MINIMAP_TOGGLE_NAME = "minimap-toggle";
        private const string BLACKBOARD_TOGGLE_NAME = "blackboard-toggle";
        private const string BEHAVIOR_TREE_EDITOR_UXML_PATH = "Editor/BehaviorTreeEditor.uxml";
        private const string BEHAVIOR_TREE_EDITOR_USS_PATH = "Editor/BehaviorTreeEditor.uss";
        private const string BEHAVIOR_TREE_WINDOW_TITLE = "Behavior Tree Editor";


        /// <summary>
        /// Responsible for opening the BT editor if a BehaviorTreeSO is opened.
        /// </summary>
        /// <param name="instanceID"></param>
        /// <param name="line"></param>
        /// <returns></returns>
        [OnOpenAsset]
        public static bool OnOpenAsset(int instanceID, int line)
        {
            Object selectedAsset = Selection.activeObject;

            if (selectedAsset is BehaviorTree selectedBehaviorTree)
            {
                EditorWindow currentWindow = EditorWindow.focusedWindow;

                if (currentWindow != null && currentWindow.GetType().Name == "ProjectBrowser")
                {
                    OpenBehaviorTree(selectedBehaviorTree);
                    return true;
                }
            }

            return false;
        }

        [MenuItem("Tanklike/Behavior Tree Editor")]
        public static void OpenEditor()
        {
            string name = $"{BEHAVIOR_TREE_WINDOW_TITLE} - NAME";
            _tabIcon = (Texture2D)EditorGUIUtility.Load("T_Choices.png");


            if (Window == null)
            {
                //Window = this;
                Window = GetWindow<BehaviorTreeEditor>();
            }

            if (!HasOpenInstances<BehaviorTreeEditor>())
            {
                //Window.titleContent = new GUIContent(BEHAVIOR_TREE_WINDOW_TITLE);
                Window.Show();
            }

            Window.titleContent = new GUIContent(name, _tabIcon);

            Window.rootVisualElement.Clear();
            Window.GenerateAllVisual();
        }

        public static void OpenBehaviorTree(BehaviorTree behaviorTree)
        {
            if (behaviorTree == null)
            {
                Debug.LogError("Attampting to open a BT but no BT passed.");
                return;
            }

            Selection.activeObject = behaviorTree;
            EditorUtility.FocusProjectWindow();
            OpenEditor();
        }

        public static void CloseWindow()
        {
            Window.Close();
        }

        #region Graph and elements creation
        private void GenerateAllVisual()
        {
            SetBehaviorTree();

            if (SelectedBehaviorTree == null)
            {
                Debug.Log("Opening attempted");
                return;
            }

            CreateBehaviorTreeFromUXML();
            AddStyleSheets();
            CacheInspectorView();

            _treeView.OnNodeSelected = OnNodeSelectionChanged;

            OnSelectionChange();

            GenerateMinimap();
            GenerateBlackBoard();
            LoadViewData();

            _treeView.PopulateView(SelectedBehaviorTree);
        }


        private void SetBehaviorTree()
        {
            if (Selection.activeObject is BehaviorTree selectedBehaviorTree)
            {
                SelectedBehaviorTree = selectedBehaviorTree;
            }
            else
            {
                GameObject selectedGameObject = Selection.activeGameObject;

                if (selectedGameObject != null)
                {
                    if (selectedGameObject.TryGetComponent(out BehaviorTreeRunner selectedBTRunner))
                    {
                        SelectedBehaviorTree = selectedBTRunner.Tree;
                    }
                }
            }

            if (SelectedBehaviorTree == null)
            {
                SelectedBehaviorTree = BehaviorTreeSettings.GetSettings().LastSelectedBehaviorTree;
            }

            if (SelectedBehaviorTree == null)
            {
                return;
            }

            if (!Application.isPlaying)
            {
                BehaviorTreeSettings.GetSettings().SetBehaviorTree(SelectedBehaviorTree);
                BehaviorTreeSettings.GetSettings().AddRecentBehaviorTree(SelectedBehaviorTree);
            }

            SelectedBehaviorTree.Refresh();
            SelectedBehaviorTree.CreateBlackboardContainer();
        }

        private void CreateBehaviorTreeFromUXML()
        {
            string path = BehaviorTreeSettings.CORE_DIRECTORY + BEHAVIOR_TREE_EDITOR_UXML_PATH;
            VisualTreeAsset visualTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(path);

            if (visualTree == null)
            {
                Debug.LogError($"Couldn't find the UXML file in {path}");
                return;
            }

            visualTree.CloneTree(rootVisualElement);
            _treeView = rootVisualElement.Q<BehaviorTreeView>();

            _treeView.Initialize(this);
        }

        private void AddStyleSheets()
        {
            // the stylesheet can be added to a VisualElement.
            // this style will be applied to the VisualElement and all of its children.
            string path = BehaviorTreeSettings.CORE_DIRECTORY + BEHAVIOR_TREE_EDITOR_USS_PATH;
            var styleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>(path);
            rootVisualElement.styleSheets.Add(styleSheet);
        }

        private void CacheInspectorView()
        {
            _inspectorView = rootVisualElement.Q<InspectorView>();
        }

        #region Blackboard
        private void GenerateBlackBoard()
        {
            if (Application.isPlaying)
            {
                return;
            }

            if (_blackboard != null)
            {
                _blackboard.RemoveFromHierarchy();
                _blackboard = null;
            }

            _blackboard = new CustomBlackboard(_treeView, this);
            _blackboard.visible = IsBlackboardDisplayed();

            _treeView.AddBlackboard(_blackboard);

            //_blackboardToggle = rootVisualElement.Q<Toggle>(BLACKBOARD_TOGGLE_NAME);

            //_blackboardToggle.RegisterValueChangedCallback(evt =>
            //{
            //    _blackboard.visible = evt.newValue;
            //    SelectedBehaviorTree.IsBlackboardDisplayed = evt.newValue;
            //});

            LoadBlackboardVisibility();
        }

        private bool IsBlackboardDisplayed()
        {
            if (SelectedBehaviorTree != null)
            {
                return SelectedBehaviorTree.IsBlackboardDisplayed;
            }

            return false;
        }
        #endregion

        #region Minimap
        private void GenerateMinimap()
        {
            _minimap = new MiniMap() { anchored = false };
            _minimap.visible = IsMinimapDisplayed();

            _minimap.style.position = Position.Absolute;
            _minimap.style.top = 0;
            _minimap.style.right = 0;
            _minimap.style.width = 200;
            _minimap.style.height = 150;

            _treeView.Add(_minimap);

            // set up the minimap toggle
            _minimapToggle = rootVisualElement.Q<Toggle>(MINIMAP_TOGGLE_NAME);
            _minimapToggle.RegisterValueChangedCallback(evt =>
            {
                _minimap.visible = evt.newValue;
                SelectedBehaviorTree.IsMinimapDisplayed = evt.newValue;
            });

            LoadMinimapVisibility();
        }

        private bool IsMinimapDisplayed()
        {
            if (SelectedBehaviorTree != null)
            {
                return SelectedBehaviorTree.IsMinimapDisplayed;
            }

            return false;
        }
        #endregion
        #endregion

        #region Editor and play mode controls
        /// <summary>
        /// Allows us to control what happens when the editor and the play mode run and stop in the editor.
        /// </summary>
        private void OnEnable()
        {
            EditorApplication.playModeStateChanged -= OnPlayerModeStateChanged;
            EditorApplication.playModeStateChanged += OnPlayerModeStateChanged;

            if (Window == null)
            {
                Window = this;
            }

            // reinitialize the window after recompilation
            if (BehaviorTreeSettings.GetSettings().LastSelectedBehaviorTree != null)
            {
                var existingWindows = Resources.FindObjectsOfTypeAll<BehaviorTreeEditor>();
                OpenEditor();
            }
        }

        private void OnDisable()
        {
            EditorApplication.playModeStateChanged -= OnPlayerModeStateChanged;
        }

        private void OnPlayerModeStateChanged(PlayModeStateChange obj)
        {
            switch (obj)
            {
                case PlayModeStateChange.EnteredEditMode:
                    OnSelectionChange();
                    break;
                case PlayModeStateChange.ExitingEditMode:
                    break;
                case PlayModeStateChange.EnteredPlayMode:
                    //OnSelectionChange();
                    OpenEditor();
                    //CacheLastBehaviorTree();
                    break;
                case PlayModeStateChange.ExitingPlayMode:
                    OnPlayModeExit();
                    break;
            }
        }

        //private void CacheLastBehaviorTree()
        //{
        //    if (SelectedBehaviorTree != null)
        //    {
        //        BehaviorTreeSettings.GetSettings().LastSelectedBehaviorTree = SelectedBehaviorTree;
        //    }
        //}

        private void OnPlayModeExit()
        {
            Window.rootVisualElement.Clear();

            //if (SelectedBehaviorTree == null)
            //{
            OpenBehaviorTree(BehaviorTreeSettings.GetSettings().LastSelectedBehaviorTree);
            //}
        }
        #endregion

        /// <summary>
        /// Whenever an asset in the project files is selected
        /// </summary>
        private void OnSelectionChange()
        {
            if (Selection.activeGameObject != null)
            {
                // if there is a selected gameObject, and that selection has a behavior tree runner, then do the checks
                if (Selection.activeGameObject.TryGetComponent(out BehaviorTreeRunner runner))
                {
                    if (runner.Tree == null)
                    {
                        return;
                    }

                    if (runner.Tree != SelectedBehaviorTree)
                    {
                        SelectedBehaviorTree = runner.Tree;
                        OpenBehaviorTree(SelectedBehaviorTree);
                    }
                }
            }
        }

        private void OnNodeSelectionChanged(NodeView node)
        {
            _inspectorView.UpdateSelection(node);
        }

        /// <summary>
        /// Gets called around 10 times a second.
        /// </summary>
        private void OnInspectorUpdate()
        {
            _treeView?.UpdateNodeStates();
            SaveViewData();
        }

        #region Save Data
        public void SaveData()
        {
            SavePopUpWindowsView();
            SaveViewData();
        }

        private void SavePopUpWindowsView()
        {
            SelectedBehaviorTree.IsBlackboardDisplayed = _blackboard.visible;
            SelectedBehaviorTree.IsMinimapDisplayed = _minimap.visible;
        }

        private void SaveViewData()
        {
            if (SelectedBehaviorTree == null || _treeView == null)
            {
                return;
            }

            SelectedBehaviorTree.ViewPosition = _treeView.viewTransform.position;
            SelectedBehaviorTree.ViewZoom = _treeView.viewTransform.scale;
        }
        #endregion

        #region Load Data
        public void LoadData()
        {
            LoadViewData();
        }

        private void LoadMinimapVisibility()
        {
            if (_minimap == null || SelectedBehaviorTree == null || _minimapToggle == null)
            {
                return;
            }

            _minimap.visible = SelectedBehaviorTree.IsMinimapDisplayed;
            _minimapToggle.value = _minimap.visible;
        }

        private void LoadBlackboardVisibility()
        {
            if (_blackboard == null || SelectedBehaviorTree == null /*|| _blackboardToggle == null*/)
            {
                return;
            }

            _blackboard.visible = true; //SelectedBehaviorTree.IsBlackboardDisplayed;
            //_blackboardToggle.value = _blackboard.visible;
        }

        private void LoadViewData()
        {
            if (SelectedBehaviorTree == null || _treeView == null)
            {
                return;
            }

            _treeView.viewTransform.position = SelectedBehaviorTree.ViewPosition;
            _treeView.viewTransform.scale = SelectedBehaviorTree.ViewZoom;
        }
        #endregion
    }
}