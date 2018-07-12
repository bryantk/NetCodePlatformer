using UnityEditor;
using System.Reflection;

[CustomEditor(typeof(NCGameObject))]
public class NCGameObject_drawer : Editor
{

	void OnEnable()
	{
		NCGameObject myScript = target as NCGameObject;
		if (myScript.ID != 0) return;

		PropertyInfo inspectorModeInfo =
		typeof(UnityEditor.SerializedObject).GetProperty("inspectorMode", BindingFlags.NonPublic | BindingFlags.Instance);

		UnityEditor.SerializedObject serializedObject = new UnityEditor.SerializedObject(myScript.gameObject);

		inspectorModeInfo.SetValue(serializedObject, UnityEditor.InspectorMode.Debug, null);

		UnityEditor.SerializedProperty localIdProp = serializedObject.FindProperty("m_LocalIdentfierInFile");

		//Debug.Log ("found property: " + localIdProp.intValue);

		myScript.ID = localIdProp.intValue;
		//Important set the component to dirty so it won't be overriden from a prefab!
		UnityEditor.EditorUtility.SetDirty(this);

	}
}