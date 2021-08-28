using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Linq;
using System.Reflection;

public class NewInspectorWindow
{

	[MenuItem("GameObject/NewInspectorWindow %l", false, 30)]
	private static void ShowGoInspector()
	{
		ShowInspector();
	}
	
	[MenuItem("Assets/NewInspectorWindow %l", false, 30)]
	private static void ShowPjInspector()
	{
		ShowInspector();
	}

	private static void ShowInspector()
	{
		var inspectorType = typeof(Editor).Assembly.GetType("UnityEditor.InspectorWindow");
		var inspectorInstance = ScriptableObject.CreateInstance(inspectorType) as EditorWindow;
		inspectorInstance.ShowUtility();

		var isLocked = inspectorType.GetProperty("isLocked", BindingFlags.Instance | BindingFlags.Public);
		isLocked.GetSetMethod().Invoke(inspectorInstance, new object[] { true });
	}
}