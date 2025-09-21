using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace BT
{
    [CustomEditor(typeof(BehaviorTreeSearchDirectories))]
    public class BehaviorTreeSearchDirectories_Editor : Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            BehaviorTreeSearchDirectories dictionary = (BehaviorTreeSearchDirectories)target;

            if(GUILayout.Button("Refresh"))
            {
                dictionary.RefreshDictionary();
            }

            if (GUILayout.Button("Reset Dictionary"))
            {
                dictionary.ResetDictionary();
            }
        }
    }
}
