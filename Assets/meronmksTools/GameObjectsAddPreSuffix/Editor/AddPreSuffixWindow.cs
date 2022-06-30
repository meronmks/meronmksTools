using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace MeronmksTools
{
    public class AddPreSuffixWindow : EditorWindow
    {
        [SerializeField] private Transform g_RootGameObject;
        private String prefixText;
        private String suffixText;
        private bool isRootObjectRename = false;
        private bool isDisabled = true;
        
        [MenuItem("Tools/AddPreSuffix")]
        static void ShowWindow()
        {
            var window = EditorWindow.GetWindow<AddPreSuffixWindow>();
            window.titleContent.text = "AddPreSuffix";
            window.Show();
        }
    
        private void ChangeAllUnderObjectName(Transform transform)
        {
            foreach (Transform childTransform in transform)
            {
                Undo.RecordObject(childTransform.gameObject, $"AddPreSuffix:{g_RootGameObject.name}");
                childTransform.name = prefixText + childTransform.name + suffixText;
                ChangeAllUnderObjectName(childTransform);
            }
        }
        private void OnGUI()
        {
            g_RootGameObject = (Transform) EditorGUILayout.ObjectField("RootGameObject", g_RootGameObject, typeof(Transform), true);
            prefixText = EditorGUILayout.TextField("Prefix", prefixText);
            suffixText = EditorGUILayout.TextField("Suffix", suffixText);
            isRootObjectRename = EditorGUILayout.Toggle("Rootもリネーム対象とする", isRootObjectRename);
            if (g_RootGameObject == null)
            {
                EditorGUILayout.HelpBox("RootGameObjectが未設定です", MessageType.Error);
                isDisabled = true;
            }
            else
            {
                isDisabled = false;
            }
    
            if (String.IsNullOrWhiteSpace(prefixText) && String.IsNullOrWhiteSpace(suffixText))
            {
                EditorGUILayout.HelpBox("PrefixおよびSuffixが未設定です", MessageType.Warning);
            }
            using (new EditorGUI.DisabledScope(isDisabled))
            {
                if (GUILayout.Button("実行"))
                {
                    if (isRootObjectRename)
                    {
                        Undo.RecordObject(g_RootGameObject.gameObject, $"AddPreSuffix:{g_RootGameObject.name}");
                        g_RootGameObject.name = prefixText + g_RootGameObject.name + suffixText;
                    }
                    ChangeAllUnderObjectName(g_RootGameObject);
                    Debug.Log("リネーム完了！");
                }
            }
        }
    }
}

