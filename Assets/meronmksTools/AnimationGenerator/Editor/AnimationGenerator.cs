using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;
using UnityEngine.Serialization;

namespace MeronmksTools
{
    public class AnimationGenerator : EditorWindow
    {
        [SerializeField] private Transform[] g_TargetTransform;
        private string g_CreateAnimPath = "Animations";
        private string g_CreateAnimName = "generateAnim";
        private bool isObjActive = true;
        private bool isTransFormPosEx = true;
        private bool isTransFormRotEx = true;
        private bool isTransFormScaEx = true;
        private bool isIgoneVrc_Shape = true;
        private bool isIgoneWeightZeroShape = false;
        
        [MenuItem("Tools/AnimationGenerator")]
        private static void ShowWindow()
        {
            var window = GetWindow<AnimationGenerator>(false, "", true);
            var icon = EditorGUIUtility.IconContent("Animation Icon");
            icon.text = "AnimationGenerator";
            window.titleContent = icon;
        }

        private void OnGUI()
        {
            bool isDisabled = false;
            
            SerializedObject so = new SerializedObject(this);
            SerializedProperty sp_g_RootGameObject = so.FindProperty(nameof(g_TargetTransform));
            EditorGUILayout.PropertyField(sp_g_RootGameObject, new GUIContent("ターゲットのGameObject"), true);
            if (g_TargetTransform == null || g_TargetTransform.Length == 0)
            {
                EditorGUILayout.HelpBox("ターゲットのGameObjectが未設定です", MessageType.Error);
                isDisabled = true;
            }
            
            g_CreateAnimName = EditorGUILayout.TextField("書き出すファイル名", g_CreateAnimName);
            if (string.IsNullOrEmpty(g_CreateAnimName))
            {
                EditorGUILayout.HelpBox("ファイル名が未設定です", MessageType.Error);
                isDisabled = true;
            }
            
            g_CreateAnimPath = EditorGUILayout.TextField("書き出し先のフォルダ", g_CreateAnimPath);
            if (new Regex(".*/$").IsMatch(g_CreateAnimPath))
            {
                g_CreateAnimPath = g_CreateAnimPath.Remove(g_CreateAnimPath.Length - 1);
            }
            
            isObjActive = EditorGUILayout.Toggle("Active状態を書き出す", isObjActive);
            isTransFormPosEx = EditorGUILayout.Toggle("Positionも書き出す", isTransFormPosEx);
            isTransFormRotEx = EditorGUILayout.Toggle("Rotationも書き出す", isTransFormRotEx);
            isTransFormScaEx = EditorGUILayout.Toggle("Scaleも書き出す", isTransFormScaEx);
            isIgoneVrc_Shape = EditorGUILayout.Toggle("vrc.vから始まるShapeを除外", isIgoneVrc_Shape);
            isIgoneWeightZeroShape = EditorGUILayout.Toggle("Weighが0のShapeを除外", isIgoneWeightZeroShape);
            
            using (new EditorGUI.DisabledScope(isDisabled))
            {
                if (GUILayout.Button(new GUIContent("Generate!"), EditorStyles.miniButton))
                {
                    var targetPath = $"Assets/{g_CreateAnimPath}";
                    var animation = new AnimationClip();

                    for (int i = 0; i < g_TargetTransform.Length; i++)
                    {
                        SkinnedMeshRenderer smr = g_TargetTransform[i].GetComponent<SkinnedMeshRenderer>();
                        
                        if (smr != null)
                        {
                            var mesh = smr.sharedMesh;
                            for (var j = 0; j < mesh.blendShapeCount; j++)
                            {
                                // vrc.vから始まるShapeを除外するか
                                if (isIgoneVrc_Shape && mesh.GetBlendShapeName(j).StartsWith("vrc.v", StringComparison.InvariantCultureIgnoreCase))
                                {
                                    continue;;
                                }
                                
                                // Weighが0のShapeを除外するか
                                if (isIgoneWeightZeroShape && smr.GetBlendShapeWeight(j) == 0f)
                                {
                                    continue;
                                }
                                // BlendShapeを書き出す
                                AnimationCurve curve = AnimationCurve.Linear(0f, smr.GetBlendShapeWeight(j), 0f, smr.GetBlendShapeWeight(j));
                                animation.SetCurve(GetFullPath(g_TargetTransform[i]), typeof(SkinnedMeshRenderer), $"blendShape.{mesh.GetBlendShapeName(j)}", curve);
                            }   
                        }
                        
                        // Active状態を書き出す
                        if (isObjActive)
                        {
                            var isActiveF = Convert.ToSingle(g_TargetTransform[i].gameObject.activeSelf);
                            AnimationCurve curve = AnimationCurve.Linear(0f, isActiveF, 0f, isActiveF);
                            animation.SetCurve(GetFullPath(g_TargetTransform[i]), typeof(GameObject), "m_IsActive", curve);
                        }
                        
                        // TransFormのPositionを書き出す
                        if (isTransFormPosEx)
                        {
                            AnimationCurve curvePX = AnimationCurve.Linear(0f, g_TargetTransform[i].localPosition.x, 0f, g_TargetTransform[i].localPosition.x);
                            AnimationCurve curvePY = AnimationCurve.Linear(0f, g_TargetTransform[i].localPosition.y, 0f, g_TargetTransform[i].localPosition.y);
                            AnimationCurve curvePZ = AnimationCurve.Linear(0f, g_TargetTransform[i].localPosition.z, 0f, g_TargetTransform[i].localPosition.z);
                            animation.SetCurve(GetFullPath(g_TargetTransform[i]), typeof(Transform), "localPosition.x", curvePX);
                            animation.SetCurve(GetFullPath(g_TargetTransform[i]), typeof(Transform), "localPosition.y", curvePY);
                            animation.SetCurve(GetFullPath(g_TargetTransform[i]), typeof(Transform), "localPosition.z", curvePZ);
                        }
                        
                        // TransFormのRotationを書き出す
                        if (isTransFormPosEx)
                        {
                            AnimationCurve curveRX = AnimationCurve.Linear(0f, g_TargetTransform[i].localRotation.x, 0f, g_TargetTransform[i].localRotation.x);
                            AnimationCurve curveRY = AnimationCurve.Linear(0f, g_TargetTransform[i].localRotation.y, 0f, g_TargetTransform[i].localRotation.y);
                            AnimationCurve curveRZ = AnimationCurve.Linear(0f, g_TargetTransform[i].localRotation.z, 0f, g_TargetTransform[i].localRotation.z);
                            animation.SetCurve(GetFullPath(g_TargetTransform[i]), typeof(Transform), "localRotation.x", curveRX);
                            animation.SetCurve(GetFullPath(g_TargetTransform[i]), typeof(Transform), "localRotation.y", curveRY);
                            animation.SetCurve(GetFullPath(g_TargetTransform[i]), typeof(Transform), "localRotation.z", curveRZ);
                        }
                        
                        // TransFormのScaleを書き出す
                        if (isTransFormPosEx)
                        {
                            AnimationCurve curveCX = AnimationCurve.Linear(0f, g_TargetTransform[i].localScale.x, 0f, g_TargetTransform[i].localScale.x);
                            AnimationCurve curveCY = AnimationCurve.Linear(0f, g_TargetTransform[i].localScale.y, 0f, g_TargetTransform[i].localScale.y);
                            AnimationCurve curveCZ = AnimationCurve.Linear(0f, g_TargetTransform[i].localScale.z, 0f, g_TargetTransform[i].localScale.z);
                            animation.SetCurve(GetFullPath(g_TargetTransform[i]), typeof(Transform), "localScale.x", curveCX);
                            animation.SetCurve(GetFullPath(g_TargetTransform[i]), typeof(Transform), "localScale.y", curveCY);
                            animation.SetCurve(GetFullPath(g_TargetTransform[i]), typeof(Transform), "localScale.z", curveCZ);
                        }
  
                    }

                    // 書き出す前に書き出し先のフォルダ存在確認
                    if (!AssetDatabase.IsValidFolder(targetPath))
                    {
                        // 無ければ[/]で分解して作る
                        var splitString = targetPath.Split('/');
                        var combinePath = splitString[0];
                        foreach (var dir in splitString.Skip(1))
                        {
                            // 作る前にフォルダの存在確認
                            if (!AssetDatabase.IsValidFolder($"{combinePath}/{dir}"))
                            {
                                AssetDatabase.CreateFolder(combinePath, dir);
                            }
                            combinePath += $"/{dir}";
                        }
                    }
                    
                    AssetDatabase.CreateAsset(animation, $"{targetPath}/{g_CreateAnimName}.anim");
                    AssetDatabase.Refresh();
                    Debug.Log($"{targetPath}/{g_CreateAnimName}.animに書き出ししました");
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
                if (parent == transform.root) break;
                path = $"{parent.name}/{path}";
                parent = parent.parent;
            }

            return path;
        }
    }
}
