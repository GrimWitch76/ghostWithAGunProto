using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace BT
{
#if UNITY_EDITOR
    [CreateAssetMenu(menuName = "BT/Behavior Tree Settings")]
    public class BehaviorTreeSettings : ScriptableObject
    {
        public static BehaviorTreeSettings Instance;
        public const string CORE_DIRECTORY = "Assets/BehaviorTree/";
        public const string PATH = "Assets/BehaviorTree/Settings/Settings.asset";
        [field: SerializeField] public BehaviorTree LastSelectedBehaviorTree { get; private set; }
        [field: SerializeField] public List<BehaviorTree> RecentOpenedBehaviorTrees { get; set; }

        public void AddRecentBehaviorTree(BehaviorTree behaviorTree)
        {
            ClearNullBehaviorTrees();

            if (RecentOpenedBehaviorTrees.Contains(behaviorTree))
            {
                return;
            }

            RecentOpenedBehaviorTrees.Insert(0, behaviorTree);
        }

        /// <summary>
        /// Remove null recent Behavior Trees from the list.
        /// </summary>
        public void ClearNullBehaviorTrees()
        {
            RecentOpenedBehaviorTrees.RemoveAll(bt => bt == null);
        }

        public List<BehaviorTree> GetRecentBehaviorTrees()
        {
            return RecentOpenedBehaviorTrees;
        }

        public static BehaviorTreeSettings GetSettings()
        {
            if (Instance != null)
            {
                return Instance;
            }

            Instance = AssetDatabase.LoadAssetAtPath<BehaviorTreeSettings>(PATH);

            if (Instance == null)
            {
                Debug.LogError($"Couldn't find the settings in {PATH}");
                return null;
            }

            return Instance;
        }

        public void SetBehaviorTree(BehaviorTree behaviorTree)
        {
            LastSelectedBehaviorTree = behaviorTree;
            EditorUtility.SetDirty(this);
        }
    }
#endif
}
