using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace MeronmksTools
{
    public class AnimationGenerator : EditorWindow
    {
        [SerializeField] private Transform[] g_RootTransform;
        [SerializeField] private string g_CreateAnimPath;
        
        [MenuItem("Tools/AnimationGenerator")]
        private static void ShowWindow()
        {
            GetWindow<AnimationGenerator>(true, "AnimationGenerator", true);
        }

        private void OnGUI()
        {
            SerializedObject so = new SerializedObject(this);
            SerializedProperty sp_g_RootGameObject = so.FindProperty(nameof(g_RootTransform));
            EditorGUILayout.PropertyField(sp_g_RootGameObject, new GUIContent("RootGameObject"), true);

            if (GUILayout.Button(new GUIContent("Create!"), EditorStyles.miniButton, GUILayout.Width(100f)))
            {
                var animation = new AnimationClip();

                for (int i = 0; i < g_RootTransform.Length; i++)
                {
                    SkinnedMeshRenderer smr = g_RootTransform[i].GetComponent<SkinnedMeshRenderer>();
                    var mesh = smr.sharedMesh;
                    for (var j = 0; j < mesh.blendShapeCount; j++)
                    {
                        AnimationCurve curve = AnimationCurve.Linear(0.0f, smr.GetBlendShapeWeight(j), 0.0f, smr.GetBlendShapeWeight(j));
                        //animation.SetCurve(GetFullPath(g_RootTransform[i]), typeof(Transform), "localPosition.x", curve);
                        animation.SetCurve(GetFullPath(g_RootTransform[i]), typeof(SkinnedMeshRenderer), mesh.GetBlendShapeName(j), curve);
                    }
  
                }

                AssetDatabase.CreateAsset(animation, g_CreateAnimPath);
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
