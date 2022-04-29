using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace MeronmksTools
{
    public class AnimationGenerator : EditorWindow
    {
        [SerializeField] private Transform[] g_RootTransform;
        private string g_CreateAnimPath = "Assets/";
        private bool isObjActive = true;
        private bool isTransFormPosEx = true;
        private bool isTransFormRotEx = true;
        private bool isTransFormScaEx = true;
        private bool isIgoneVrc_Shape = true;
        
        [MenuItem("Tools/AnimationGenerator")]
        private static void ShowWindow()
        {
            GetWindow<AnimationGenerator>(true, "AnimationGenerator", true);
        }

        private void OnGUI()
        {
            bool isDisabled = false;
            
            SerializedObject so = new SerializedObject(this);
            SerializedProperty sp_g_RootGameObject = so.FindProperty(nameof(g_RootTransform));
            EditorGUILayout.PropertyField(sp_g_RootGameObject, new GUIContent("RootGameObject"), true);
            g_CreateAnimPath = EditorGUILayout.TextField("書き出し先のPath", g_CreateAnimPath);
            isObjActive = EditorGUILayout.Toggle("Active状態を書き出す", isObjActive);
            isTransFormPosEx = EditorGUILayout.Toggle("Positionも書き出す", isTransFormPosEx);
            isTransFormRotEx = EditorGUILayout.Toggle("Rotationも書き出す", isTransFormRotEx);
            isTransFormScaEx = EditorGUILayout.Toggle("Scaleも書き出す", isTransFormScaEx);
            isIgoneVrc_Shape = EditorGUILayout.Toggle("vrc.vから始まるShapeを除外", isIgoneVrc_Shape);
            
            if (g_RootTransform == null || g_RootTransform.Length == 0)
            {
                EditorGUILayout.HelpBox("RootGameObjectが未設定です", MessageType.Error);
                isDisabled = true;
            }

            using (new EditorGUI.DisabledScope(isDisabled))
            {
                if (GUILayout.Button(new GUIContent("Generate!"), EditorStyles.miniButton))
                {
                    var animation = new AnimationClip();

                    for (int i = 0; i < g_RootTransform.Length; i++)
                    {
                        SkinnedMeshRenderer smr = g_RootTransform[i].GetComponent<SkinnedMeshRenderer>();
                        
                        //
                        if (smr != null)
                        {
                            var mesh = smr.sharedMesh;
                            for (var j = 0; j < mesh.blendShapeCount; j++)
                            {
                                if (isIgoneVrc_Shape && mesh.GetBlendShapeName(j).StartsWith("vrc.v", StringComparison.InvariantCultureIgnoreCase))
                                {
                                    continue;;
                                }
                                // BlendShapeを書き出す
                                AnimationCurve curve = AnimationCurve.Linear(0.0f, smr.GetBlendShapeWeight(j), 0.0f, smr.GetBlendShapeWeight(j));
                                animation.SetCurve(GetFullPath(g_RootTransform[i]), typeof(SkinnedMeshRenderer), $"blendShape.{mesh.GetBlendShapeName(j)}", curve);
                            }   
                        }
                        
                        // Active状態を書き出す
                        if (isObjActive)
                        {
                            var isActiveF = Convert.ToSingle(g_RootTransform[i].gameObject.activeSelf);
                            AnimationCurve curve = AnimationCurve.Linear(0.0f, isActiveF, 0.0f, isActiveF);
                            animation.SetCurve(GetFullPath(g_RootTransform[i]), typeof(GameObject), "m_IsActive", curve);
                        }
                        
                        // TransFormのPositionを書き出す
                        if (isTransFormPosEx)
                        {
                            AnimationCurve curvePX = AnimationCurve.Linear(0.0f, g_RootTransform[i].localPosition.x, 0.0f, g_RootTransform[i].localPosition.x);
                            AnimationCurve curvePY = AnimationCurve.Linear(0.0f, g_RootTransform[i].localPosition.y, 0.0f, g_RootTransform[i].localPosition.y);
                            AnimationCurve curvePZ = AnimationCurve.Linear(0.0f, g_RootTransform[i].localPosition.z, 0.0f, g_RootTransform[i].localPosition.z);
                            animation.SetCurve(GetFullPath(g_RootTransform[i]), typeof(Transform), "localPosition.x", curvePX);
                            animation.SetCurve(GetFullPath(g_RootTransform[i]), typeof(Transform), "localPosition.y", curvePY);
                            animation.SetCurve(GetFullPath(g_RootTransform[i]), typeof(Transform), "localPosition.z", curvePZ);
                        }
                        
                        // TransFormのRotationを書き出す
                        if (isTransFormPosEx)
                        {
                            AnimationCurve curveRX = AnimationCurve.Linear(0.0f, g_RootTransform[i].localRotation.x, 0.0f, g_RootTransform[i].localRotation.x);
                            AnimationCurve curveRY = AnimationCurve.Linear(0.0f, g_RootTransform[i].localRotation.y, 0.0f, g_RootTransform[i].localRotation.y);
                            AnimationCurve curveRZ = AnimationCurve.Linear(0.0f, g_RootTransform[i].localRotation.z, 0.0f, g_RootTransform[i].localRotation.z);
                            animation.SetCurve(GetFullPath(g_RootTransform[i]), typeof(Transform), "localRotation.x", curveRX);
                            animation.SetCurve(GetFullPath(g_RootTransform[i]), typeof(Transform), "localRotation.y", curveRY);
                            animation.SetCurve(GetFullPath(g_RootTransform[i]), typeof(Transform), "localRotation.z", curveRZ);
                        }
                        
                        // TransFormのScaleを書き出す
                        if (isTransFormPosEx)
                        {
                            AnimationCurve curveCX = AnimationCurve.Linear(0.0f, g_RootTransform[i].localScale.x, 0.0f, g_RootTransform[i].localScale.x);
                            AnimationCurve curveCY = AnimationCurve.Linear(0.0f, g_RootTransform[i].localScale.y, 0.0f, g_RootTransform[i].localScale.y);
                            AnimationCurve curveCZ = AnimationCurve.Linear(0.0f, g_RootTransform[i].localScale.z, 0.0f, g_RootTransform[i].localScale.z);
                            animation.SetCurve(GetFullPath(g_RootTransform[i]), typeof(Transform), "localScale.x", curveCX);
                            animation.SetCurve(GetFullPath(g_RootTransform[i]), typeof(Transform), "localScale.y", curveCY);
                            animation.SetCurve(GetFullPath(g_RootTransform[i]), typeof(Transform), "localScale.z", curveCZ);
                        }
  
                    }

                    AssetDatabase.CreateAsset(animation, $"{g_CreateAnimPath}generateAnim.anim");
                    Debug.Log($"{g_CreateAnimPath}generateAnim.animに書き出ししました");
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
