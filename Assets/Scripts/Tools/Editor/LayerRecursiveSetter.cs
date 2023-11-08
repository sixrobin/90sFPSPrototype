namespace RSTools
{
	using UnityEngine;
	using UnityEditor;
	using RSLib.Extensions;

	sealed class LayerRecurviseSetterMenu
	{
		[MenuItem ("Tools/Layer Recursive Setter", true)]
		static bool CheckSelectionCount ()
		{
			return Selection.gameObjects.Length > 0;
		}

		[MenuItem ("Tools/Layer Recursive Setter")]
		public static void LaunchLayerSetter ()
		{
			LayerRecursiveSetterEditor.LaunchSetter ();
		}
	}

	sealed class LayerRecursiveSetterEditor : EditorWindow
	{
		GameObject[] _selection;
		string _layerName;

		public static void LaunchSetter ()
		{
			EditorWindow window = GetWindow<LayerRecursiveSetterEditor> ("Set objet's children layer");
			window.Show ();
		}

		void OnGUI ()
		{
			_selection = Selection.gameObjects;

			EditorGUILayout.LabelField ("Layer Name", EditorStyles.boldLabel);
			_layerName = EditorGUILayout.TextField (_layerName);

			if (GUILayout.Button ("Set objet's children layer recursively", GUILayout.Height (45), GUILayout.ExpandWidth (true)))
				foreach (GameObject selected in _selection)
					selected.SetChildrenLayers (LayerMask.NameToLayer (_layerName));

			Repaint ();
		}
	}
}