﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

public class SimpleObjectSpawn : EditorWindow
{
    [SerializeField] private Transform g_TargetTransform;
    [SerializeField] private Transform g_RootTransform;
    [SerializeField] private AnimatorController g_animatorController;
    private string g_CreateAnimPath = "Animators";
    private string g_CreateClipName = "generateAnimClip";
    private string g_ParameterName = "ParameterName";
    private string g_LayerName = "LayerName";
    private bool g_defalutStateOn = true;
    
    [MenuItem("Tools/SimpleObjectSpawn")]
    private static void ShowWindow()
    {
        var window = GetWindow<SimpleObjectSpawn>(false, "", true);
        var icon = EditorGUIUtility.IconContent("Animator Icon");
        icon.text = "SimpleObjectSpawn";
        window.titleContent = icon;
    }

    private void OnGUI()
    {
        bool isDisabled = false;
        
        SerializedObject so = new SerializedObject(this);
        so.Update();
        SerializedProperty sp_g_TargetGameObject = so.FindProperty(nameof(g_TargetTransform));
        EditorGUILayout.PropertyField(sp_g_TargetGameObject, new GUIContent("ターゲットのGameObject"), false);
        if (g_TargetTransform == null)
        {
            EditorGUILayout.HelpBox("ターゲットのGameObjectが未設定です", MessageType.Error);
            isDisabled = true;
        }
        
        SerializedProperty sp_g_RootGameObject = so.FindProperty(nameof(g_RootTransform));
        EditorGUILayout.PropertyField(sp_g_RootGameObject, new GUIContent("AnimのRootにするObject"), false);

        g_CreateClipName = EditorGUILayout.TextField("書き出すClip名", g_CreateClipName);
        if (string.IsNullOrEmpty(g_CreateClipName))
        {
            EditorGUILayout.HelpBox("Clip名が未設定です", MessageType.Error);
            isDisabled = true;
        }
        
        SerializedProperty sp_g_animatorController = so.FindProperty(nameof(g_animatorController));
        EditorGUILayout.PropertyField(sp_g_animatorController, new GUIContent("書き出し先のController"), false);
        if (g_animatorController == null)
        {
            EditorGUILayout.HelpBox("Controllerが未設定です", MessageType.Error);
            isDisabled = true;
        }
        
        g_ParameterName = EditorGUILayout.TextField("パラメータ名", g_ParameterName);
        if (string.IsNullOrEmpty(g_ParameterName))
        {
            EditorGUILayout.HelpBox("パラメータ名が未設定です", MessageType.Error);
            isDisabled = true;
        }
        
        g_LayerName = EditorGUILayout.TextField("Layer名", g_LayerName);
        if (string.IsNullOrEmpty(g_LayerName))
        {
            EditorGUILayout.HelpBox("Layer名が未設定です", MessageType.Error);
            isDisabled = true;
        }

        g_defalutStateOn = EditorGUILayout.Toggle("デフォの遷移先をOnにする", g_defalutStateOn);
        
        g_CreateAnimPath = EditorGUILayout.TextField("書き出し先のフォルダ", g_CreateAnimPath);
        if (GUILayout.Button(new GUIContent("フォルダ選択"), EditorStyles.miniButton))
        {
            var path = EditorUtility.OpenFolderPanel("書き出し先のフォルダ選択", Application.dataPath, string.Empty);
            if (!string.IsNullOrEmpty(path))
            {
                g_CreateAnimPath = GetAssetsPath(path);
            }
        }
        if (new Regex(".*/$").IsMatch(g_CreateAnimPath))
        {
            g_CreateAnimPath = g_CreateAnimPath.Remove(g_CreateAnimPath.Length - 1);
        }

        using (new EditorGUI.DisabledScope(isDisabled))
        {
            if (GUILayout.Button(new GUIContent("Generate!"), EditorStyles.miniButton))
            {
                var targetPath = $"Assets/{g_CreateAnimPath}";
                var animation = new AnimationClip();
                
                AnimationCurve curve = AnimationCurve.Linear(0f, 0f, 0.01f, 1f);
                animation.SetCurve(GetFullPath(g_TargetTransform), typeof(GameObject), "m_IsActive", curve);

                AssetDatabase.CreateAsset(animation, $"{targetPath}/{g_CreateClipName}.anim");

                var hitFindParameter = false;
                foreach (var parameter in g_animatorController.parameters)
                {
                    if (parameter.name.Equals(g_ParameterName))
                    {
                        hitFindParameter = true;
                    }
                }

                if (hitFindParameter == false)
                {
                    g_animatorController.AddParameter(g_ParameterName, AnimatorControllerParameterType.Bool);
                }
                
                g_animatorController.AddLayer(g_LayerName);

                // layer全体をコピーして編集後再セットしないと正しく反映されない　クソ
                var layers = g_animatorController.layers;
                var addLayerIndex = g_animatorController.layers.Length - 1;
                layers[addLayerIndex].defaultWeight = 1f;
                layers[addLayerIndex].blendingMode = AnimatorLayerBlendingMode.Override;
                g_animatorController.layers = layers;
                
                var stateMachine = layers[addLayerIndex].stateMachine;
                
                var states = new List<AnimatorState>();

                // State作成
                var onState = stateMachine.AddState("On", new Vector2(340, 0));
                onState.motion = animation;
                onState.writeDefaultValues = false;
                onState.speed = 1f;
                states.Add(onState);
                
                var offState = stateMachine.AddState("Off", new Vector2(340, 210));
                offState.motion = animation;
                offState.writeDefaultValues = false;
                offState.speed = -1f;
                states.Add(offState);
                
                // Transition作成
                if (g_defalutStateOn)
                {
                    stateMachine.defaultState = states[0];
                }
                else
                {
                    stateMachine.defaultState = states[1];
                }

                var transition1 = stateMachine.AddEntryTransition(states[0]);
                transition1.AddCondition(AnimatorConditionMode.If, 0f, g_ParameterName);
                var exitTransition1 = states[0].AddExitTransition();
                exitTransition1.AddCondition(AnimatorConditionMode.IfNot, 0f, g_ParameterName);
                exitTransition1.duration = 0f;
                
                var transition2 = stateMachine.AddEntryTransition(states[1]);
                transition2.AddCondition(AnimatorConditionMode.IfNot, 0f, g_ParameterName);
                var exitTransition2 = states[1].AddExitTransition();
                exitTransition2.AddCondition(AnimatorConditionMode.If, 0f, g_ParameterName);
                exitTransition2.duration = 0f;
                
                // 保存
                EditorUtility.SetDirty(g_animatorController);
                
                AssetDatabase.SaveAssets();
                
                if (EditorUtility.DisplayDialog("SimpleObjectSpawn", "正常に処理が完了しました", "OK"))
                {
                    
                }
            }
        }
        
        so.ApplyModifiedProperties();
    }
    
    private string GetFullPath(Transform transform)
    {
        string path = transform.name;
        var parent = transform.parent;

        while (parent)
        {
            if (transform == g_RootTransform || parent == g_RootTransform) break;
            path = $"{parent.name}/{path}";
            parent = parent.parent;
        }

        return path;
    }

    private string GetAssetsPath(string fullPath)
    {
        return fullPath.Replace($"{Application.dataPath}/","");
    }
}
