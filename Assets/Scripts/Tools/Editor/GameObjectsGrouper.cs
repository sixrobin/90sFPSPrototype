namespace RSLib
{
	using UnityEngine;
	using UnityEditor;
	using Extensions;

	sealed class GameObjectsGrouperMenu
	{
		const string SHORTCUT = "%#q";

		[MenuItem ("GameObject/Group Objects " + SHORTCUT, true)]
		static bool CheckSelectionCount ()
		{
			return Selection.gameObjects.Length > 1;
		}

		[MenuItem ("GameObject/Group Objects " + SHORTCUT)]
		public static void LaunchObjectGrouper ()
		{
			GamebjectsGrouperEditor.LaunchGrouper ();
		}
	}

	sealed class GamebjectsGrouperEditor : EditorWindow
	{
		const string FULL_HIERARCHY_CHANGE = "full object hierarchy change";
		const string CREATE_UNDO = "Create ";

		GameObject[] _selection;
		string _groupName;
		bool _averagePos;
		bool _yReset;

		public static void LaunchGrouper ()
		{
			GetWindow<GamebjectsGrouperEditor> ("Group selection").Show ();
		}

		Vector3 GetSelectionAveragePosition ()
		{
			Vector3 averagePosition = Vector3.zero;
			foreach (GameObject child in _selection)
				averagePosition += child.transform.position;
			return averagePosition /= _selection.Length;
		}

		void GroupSelection ()
		{
			if (_selection.Length < 2)
			{
				EditorUtility.DisplayDialog ("Grouper warning", "You need to select at least 2 objects to group !", "OK");
				return;
			}
			if (string.IsNullOrEmpty (_groupName))
			{
				EditorUtility.DisplayDialog ("Grouper warning", "You must provide a name for the group !", "OK");
				return;
			}

			GameObject groupParent = new GameObject (_groupName);
			groupParent.transform.SetParent (_selection[0].transform.parent);
			groupParent.transform.SetSiblingIndex (_selection[0].transform.GetSiblingIndex ());
			if (_averagePos)
				groupParent.transform.position = _yReset ? GetSelectionAveragePosition ().WithY (0) : GetSelectionAveragePosition ();

			foreach (GameObject child in _selection)
			{
				Undo.RegisterFullObjectHierarchyUndo (child, FULL_HIERARCHY_CHANGE);
				child.transform.SetParent (groupParent.transform);
			}

			Undo.RegisterCreatedObjectUndo (groupParent, CREATE_UNDO + groupParent.name);
			Selection.activeObject = groupParent;
		}

		void OnGUI ()
		{
			_selection = Selection.gameObjects;

			EditorGUILayout.BeginHorizontal ();
			GUILayout.Space (10);
			EditorGUILayout.BeginVertical ();
			GUILayout.Space (10);

			EditorGUILayout.LabelField ("SELECTED : " + _selection.Length, EditorStyles.boldLabel);

			GUILayout.Space (10);

			EditorGUILayout.LabelField ("Group Name", EditorStyles.boldLabel);
			_groupName = EditorGUILayout.TextField (_groupName);

			GUILayout.Space (10);
			EditorGUILayout.EndVertical ();
			EditorGUILayout.EndHorizontal ();

			EditorGUILayout.BeginHorizontal ();
			GUILayout.Space (10);
			_averagePos = EditorGUILayout.Toggle ("Average position", _averagePos);
			EditorGUILayout.EndHorizontal ();

			if (_averagePos)
			{
				EditorGUILayout.BeginHorizontal ();
				GUILayout.Space (10);
				_yReset = EditorGUILayout.Toggle ("Keep Y at 0", _yReset);
				EditorGUILayout.EndHorizontal ();
			}

			GUILayout.Space (10);

			if (GUILayout.Button ("Group Selection", GUILayout.Height (45), GUILayout.ExpandWidth (true)))
				GroupSelection ();

			Repaint ();
		}
	}
}