using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace MeronmksTools
{
    public class MissingSearch : EditorWindow
    {
	    private static readonly GUIContent GC_Find = new GUIContent("Search");
	    private static List<AssetParameterData> missingList = new List<AssetParameterData>();

	    public class AssetParameterData {
		    public UnityEngine.Object obj { get; set; }  
		    public string targetName { get; set; }
	    }
	    
	    private static GUIStyle PingButtonStyle;
	    
	    private Vector2 scroll;
	    
        [MenuItem("Window/Missing Search")]
        private static void ShowWindow()
        {
            GetWindow<MissingSearch>(true, "Missing Search", true);
        }
        
	    private void OnGUI()
		{
			if (PingButtonStyle == null)
			{
				PingButtonStyle = new GUIStyle(EditorStyles.miniButton) { alignment = TextAnchor.MiddleLeft };
			}

			EditorGUILayout.BeginHorizontal();
			{
				if (GUILayout.Button(GC_Find, EditorStyles.miniButton, GUILayout.Width(100f)))
				{
					Find();
				}			
				GUILayout.FlexibleSpace();
				EditorGUILayout.Space();
			}
			EditorGUILayout.EndHorizontal();
			EditorGUILayout.Space();
			
			EditorGUILayout.BeginHorizontal ();
			EditorGUILayout.LabelField ("Asset", GUILayout.Width (200));
			EditorGUILayout.LabelField ("Path");
			EditorGUILayout.EndHorizontal ();
			
			scroll = EditorGUILayout.BeginScrollView (scroll);

			foreach (AssetParameterData data in missingList) {
				EditorGUILayout.BeginHorizontal ();
				EditorGUILayout.ObjectField (data.obj, data.obj.GetType (), true, GUILayout.Width (200));
				EditorGUILayout.TextField (data.targetName);
				EditorGUILayout.EndHorizontal ();
			}
			EditorGUILayout.EndScrollView ();
		}

		private void Find()
		{
			missingList.Clear();
			GameObject[] gos = Resources.FindObjectsOfTypeAll<GameObject>();

			foreach (var go in gos)
			{
				int count = 0;
				Component[] cos = go.GetComponents<Component>();
				foreach (var co in cos)
				{
					if (co == null)
					{
						Transform tr = go.transform.parent;
						AssetParameterData assetParameterData = new AssetParameterData()
						{
							targetName = "Component is Missing",
							obj = go
						};
						missingList.Add(assetParameterData);
						while (tr != null)
						{
							assetParameterData.targetName = string.Format("{0}/{1}", tr.name, assetParameterData.targetName);
							tr = tr.parent;
						}

						assetParameterData.targetName = assetParameterData.targetName;
					}
					else
					{
						SerializedObject sobj = new SerializedObject(co);
						SerializedProperty property = sobj.GetIterator();

						while (property.Next(true))
						{
							if (property.propertyType == SerializedPropertyType.ObjectReference &&
							    property.objectReferenceValue == null &&
							    property.objectReferenceInstanceIDValue != 0)
							{
								missingList.Add(new AssetParameterData()
								{
									obj = co,
									targetName = $"{co.GetType().Name}/{property.displayName}"
								});
							}
						}
					}
				}
			}
			
			missingList.Sort((a, b) => String.Compare(a.targetName, b.targetName, StringComparison.Ordinal));
		}

		// ------------------------------------------------------------------------------------------------------------------
    }
}