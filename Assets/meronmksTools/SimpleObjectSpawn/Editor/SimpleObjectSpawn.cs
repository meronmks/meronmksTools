using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;
using UnityEngine.Serialization;

#if VRC_SDK_VRCSDK3
using VRC.SDK3.Avatars.Components;
using VRC.SDK3.Avatars.ScriptableObjects;
#endif

namespace MeronmksTools
{
    public class SimpleObjectSpawn : EditorWindow
    {
        [SerializeField] private Transform g_targetTransform;
        [SerializeField] private Transform g_rootTransform;
        [SerializeField] private AnimatorController g_animatorController;
        private string g_createAnimPath = "Animators";
        private string g_createClipName = "generateAnimClip";
        private string g_parameterName = "ParameterName";
        private string g_layerName = "LayerName";
        private bool g_defalutStateOn = true;
        private bool g_inverse = false;
        private bool g_shareParamererName = true;
        private bool g_shareLayerName = true;

#if VRC_SDK_VRCSDK3
        [SerializeField] private VRCExpressionsMenu g_exMenu;
        [SerializeField] private VRCExpressionParameters g_exParameters;
        private bool g_exParameterSave = false;
        private bool g_shareExMenuName = false;
        private string g_exMenuName = "New Control";
#endif
        
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
            EditorGUIUtility.labelWidth = 250f;
            bool isDisabled = false;
            
            SerializedObject so = new SerializedObject(this);
            so.Update();
            SerializedProperty sp_g_TargetGameObject = so.FindProperty(nameof(g_targetTransform));
            EditorGUILayout.PropertyField(sp_g_TargetGameObject, new GUIContent("ターゲットのGameObject"), false);
            if (g_targetTransform == null)
            {
                EditorGUILayout.HelpBox("ターゲットのGameObjectが未設定です", MessageType.Error);
                isDisabled = true;
            }
            
            SerializedProperty sp_g_RootGameObject = so.FindProperty(nameof(g_rootTransform));
            EditorGUILayout.PropertyField(sp_g_RootGameObject, new GUIContent("AnimationのRootにするObject"), false);
            g_createClipName = EditorGUILayout.TextField("書き出すAnimationClipの名前", g_createClipName);
            if (string.IsNullOrEmpty(g_createClipName))
            {
                EditorGUILayout.HelpBox("AnimationClip名が未設定です", MessageType.Error);
                isDisabled = true;
            }
            
            SerializedProperty sp_g_animatorController = so.FindProperty(nameof(g_animatorController));
            EditorGUILayout.PropertyField(sp_g_animatorController, new GUIContent("書き出し先のAnimatorController"), false); 
            
            if (g_animatorController == null)
            {
                EditorGUILayout.HelpBox("Controllerが未設定です", MessageType.Error);
                isDisabled = true;
            }

            g_shareParamererName = EditorGUILayout.Toggle("AnimationClip名とパラメータ名を同じにする", g_shareParamererName);
            
            if(g_shareParamererName)
            {
                g_parameterName = g_createClipName;
            }

            using (new EditorGUI.DisabledScope(g_shareParamererName))
            {
                g_parameterName = EditorGUILayout.TextField("パラメータ名", g_parameterName);
            }
            
            if (string.IsNullOrEmpty(g_parameterName) && !g_shareParamererName)
            {
                EditorGUILayout.HelpBox("パラメータ名が未設定です", MessageType.Error);
                isDisabled = true;
            }

            g_shareLayerName = EditorGUILayout.Toggle("AnimationClip名とLayer名を同じにする", g_shareLayerName); 
            
            if(g_shareLayerName)
            {
                g_layerName = g_createClipName;
            }

            using (new EditorGUI.DisabledScope(g_shareLayerName))
            {
                g_layerName = EditorGUILayout.TextField("Layer名", g_layerName);
            }
            
            if (string.IsNullOrEmpty(g_layerName) && !g_shareLayerName)
            {
                EditorGUILayout.HelpBox("Layer名が未設定です", MessageType.Error);
                isDisabled = true;
            }

            g_defalutStateOn = EditorGUILayout.Toggle("デフォルトの遷移先StateをONにする", g_defalutStateOn);
            g_inverse = EditorGUILayout.Toggle("Stateと変数のON/OFFを逆にする", g_inverse);
            g_createAnimPath = EditorGUILayout.TextField("書き出し先フォルダ", g_createAnimPath);
            if (GUILayout.Button(new GUIContent("フォルダ選択"), EditorStyles.miniButton))
            {
                var path = EditorUtility.OpenFolderPanel("書き出し先フォルダ選択", Application.dataPath, string.Empty);
                if (!string.IsNullOrEmpty(path))
                {
                    g_createAnimPath = GetAssetsPath(path);
                }
            }
            if (new Regex(".*/$").IsMatch(g_createAnimPath))
            {
                g_createAnimPath = g_createAnimPath.Remove(g_createAnimPath.Length - 1);
            }

#if VRC_SDK_VRCSDK3
            SerializedProperty sp_g_exMenu = so.FindProperty(nameof(g_exMenu));
            EditorGUILayout.PropertyField(sp_g_exMenu, new GUIContent("書き出し先VRCExpressionsMenu"), false);
            if (g_exMenu != null)
            {
                if (g_exMenu.controls.Count >= 8)
                {
                    EditorGUILayout.HelpBox("メニューに空きがありません", MessageType.Error);
                    isDisabled = true;
                }
                else
                {
                    EditorGUILayout.HelpBox($"使用中のメニュー枠：{g_exMenu.controls.Count}/8", MessageType.Info);
                }
                
            }
            g_shareExMenuName = EditorGUILayout.Toggle("AnimationClip名とExメニュー名を同じにする", g_shareExMenuName);
            if(g_shareExMenuName)
            {
                g_exMenuName = g_createClipName;
            }

            using (new EditorGUI.DisabledScope(g_shareExMenuName))
            {
                g_exMenuName = EditorGUILayout.TextField("Exメニュー名", g_exMenuName);
            }
            
            SerializedProperty sp_g_exParameters = so.FindProperty(nameof(g_exParameters));
            EditorGUILayout.PropertyField(sp_g_exParameters, new GUIContent("書き出し先VRCExpressionParameters"), false);
            if (g_exParameters != null)
            {
                if (g_exParameters.CalcTotalCost() >= 256)
                {
                    EditorGUILayout.HelpBox($"メモリが足りません", MessageType.Error);
                    isDisabled = true;
                }
                else
                {
                    EditorGUILayout.HelpBox($"使用中のメモリ：{g_exParameters.CalcTotalCost()}/256", MessageType.Info);
                }
                
            }
            g_exParameterSave = EditorGUILayout.Toggle("アバターを着替えてもパラメータをSaveする", g_exParameterSave);
#endif
            var guid = AssetDatabase.AssetPathToGUID($"Assets/{g_createAnimPath}/{g_createClipName}.anim");
            if (!string.IsNullOrEmpty(guid))
            {
                EditorGUILayout.HelpBox("出力先に同じ名前のAnimationClipが存在します。内容が上書きされるので注意してください。", MessageType.Warning);
            }
            
            using (new EditorGUI.DisabledScope(isDisabled))
            {
                if (GUILayout.Button(new GUIContent("Generate!"), EditorStyles.miniButton))
                {
                    Generate();
                }
            }
            
            so.ApplyModifiedProperties();
        }

        private void Generate()
        {
            var targetPath = $"Assets/{g_createAnimPath}";
            var animation = new AnimationClip();
            
            AnimationCurve curve = AnimationCurve.Linear(0f, 0f, 0.01f, 1f);
            animation.SetCurve(GetFullPath(g_targetTransform), typeof(GameObject), "m_IsActive", curve);

            AssetDatabase.CreateAsset(animation, $"{targetPath}/{g_createClipName}.anim");

            var hitFindParameter = false;
            foreach (var parameter in g_animatorController.parameters)
            {
                if (parameter.name.Equals(g_parameterName))
                {
                    hitFindParameter = true;
                }
            }

            if (hitFindParameter == false)
            {
                g_animatorController.AddParameter(g_parameterName, AnimatorControllerParameterType.Bool);
            }
            
            g_animatorController.AddLayer(g_layerName);

            // layer全体をコピーして編集後再セットしないと正しく反映されない　クソ
            var layers = g_animatorController.layers;
            var addLayerIndex = g_animatorController.layers.Length - 1;
            layers[addLayerIndex].defaultWeight = 1f;
            layers[addLayerIndex].blendingMode = AnimatorLayerBlendingMode.Override;
            g_animatorController.layers = layers;
            
            var stateMachine = layers[addLayerIndex].stateMachine;
            
            var states = new List<AnimatorState>();

            // State作成
            var onState = stateMachine.AddState(g_layerName + "->ON", new Vector2(300, 60));
            onState.motion = animation;
            onState.writeDefaultValues = false;
            onState.speed = 1f;
            states.Add(onState);
            
            var offState = stateMachine.AddState(g_layerName + "->OFF", new Vector2(300, 160));
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
            if(!g_inverse)
            {
                transition1.AddCondition(AnimatorConditionMode.If, 0f, g_parameterName);
            }
            else
            {
                transition1.AddCondition(AnimatorConditionMode.IfNot, 0f, g_parameterName);
            }
            var exitTransition1 = states[0].AddExitTransition();
            if(!g_inverse)
            {
                exitTransition1.AddCondition(AnimatorConditionMode.IfNot, 0f, g_parameterName);
            }
            else
            {
                exitTransition1.AddCondition(AnimatorConditionMode.If, 0f, g_parameterName);
            }
            exitTransition1.duration = 0f;
            
            var transition2 = stateMachine.AddEntryTransition(states[1]);
            if(!g_inverse)
            {
                transition2.AddCondition(AnimatorConditionMode.IfNot, 0f, g_parameterName);
            }
            else
            {
                transition2.AddCondition(AnimatorConditionMode.If, 0f, g_parameterName);
            }
            var exitTransition2 = states[1].AddExitTransition();
            if(!g_inverse)
            {
                exitTransition2.AddCondition(AnimatorConditionMode.If, 0f, g_parameterName);
            }
            else
            {
                exitTransition2.AddCondition(AnimatorConditionMode.IfNot, 0f, g_parameterName);
            }
            exitTransition2.duration = 0f;

            stateMachine.entryPosition = new Vector2(20, 70);
            stateMachine.anyStatePosition = new Vector2(20, 170);
            stateMachine.exitPosition = new Vector2(620, 70);

            VRCParmGenerate();
            
            // 保存
            EditorUtility.SetDirty(g_animatorController);
            
            AssetDatabase.SaveAssets();
            
            if (EditorUtility.DisplayDialog("SimpleObjectSpawn", "正常に処理が完了しました", "OK"))
            {
                Debug.Log($"[SimpleObjectSpawn] targetPath:{targetPath}");
            }
        }

        private void VRCParmGenerate()
        {
#if VRC_SDK_VRCSDK3
            if (g_exParameters != null)
            {
                var isParmUniq = true;
                var parameter = new VRCExpressionParameters.Parameter();
                parameter.name = g_parameterName;
                parameter.valueType = VRCExpressionParameters.ValueType.Bool;
                parameter.defaultValue = 0;
                parameter.saved = g_exParameterSave;
                var parameters = new VRCExpressionParameters.Parameter[g_exParameters.parameters.Length + 1];
                for (var i = 0; i < g_exParameters.parameters.Length; i++)
                {
                    parameters[i] = g_exParameters.parameters[i];
                    if (g_exParameters.parameters[i].name.Equals(g_parameterName))
                    {
                        isParmUniq = false;
                    }
                }
                parameters.SetValue(parameter, parameters.Length - 1);
                if (isParmUniq)
                {
                    g_exParameters.parameters = parameters;
                }
                EditorUtility.SetDirty(g_exParameters);
            }

            if (g_exMenu != null)
            {
                var controls = g_exMenu.controls;
                var control = new VRCExpressionsMenu.Control();
                control.name = g_exMenuName;
                control.type = VRCExpressionsMenu.Control.ControlType.Toggle;
                control.parameter = new VRCExpressionsMenu.Control.Parameter(){name = g_parameterName};
                controls.Add(control);
                g_exMenu.controls = controls;
                EditorUtility.SetDirty(g_exMenu);
            }
#endif
        }
        
        private string GetFullPath(Transform transform)
        {
            string path = transform.name;
            var parent = transform.parent;

            while (parent)
            {
                if (transform == g_rootTransform || parent == g_rootTransform || transform.root == parent) break;
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

}

