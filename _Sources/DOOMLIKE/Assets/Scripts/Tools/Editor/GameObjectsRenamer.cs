namespace RSTools
{
	using UnityEngine;
	using UnityEditor;

	sealed class ObjectsRenamerMenu
	{
		const string SHORTCUT = "%&r";

		[MenuItem ("GameObject/Rename Objects " + SHORTCUT, true)]
		static bool CheckIfAtLeastOneObjectIsSelect ()
		{
			return Selection.gameObjects.Length > 0;
		}

		[MenuItem ("GameObject/Rename Objects " + SHORTCUT)]
		public static void RenameSelectedObjects ()
		{
			GameObjectsRenamerEditor.LaunchRenamer ();
		}
	}

	sealed class GameObjectsRenamerEditor : EditorWindow
	{
		const string UNDERSCORE = "_";

		GameObject[] _selection;
		string _prefix;
		string _nameBody;
		string _suffix;
		bool _numbering;

		public static void LaunchRenamer ()
		{
			GetWindow<GameObjectsRenamerEditor> ("Rename Objects").Show ();
		}

		void RenameSelection ()
		{
			if (_selection.Length == 0)
			{
				EditorUtility.DisplayDialog ("Renamer warning", "You must select at least 1 object to rename !", "OK");
				return;
			}
			if (string.IsNullOrEmpty (_nameBody))
			{
				EditorUtility.DisplayDialog ("Renamer warning", "You must provide at least the name to set !", "OK");
				return;
			}

			System.Array.Sort (_selection, delegate (GameObject a, GameObject b) { return a.name.CompareTo (b.name); });

			for (int i = 0; i < _selection.Length; i++)
			{
				string newName = string.Empty;
				if (!string.IsNullOrEmpty (_prefix))
					newName += _prefix;
				newName += (newName.Length != 0 ? UNDERSCORE : "") + _nameBody;
				if (!string.IsNullOrEmpty (_suffix))
					newName += UNDERSCORE + _suffix;
				if (_numbering)
					newName += UNDERSCORE + i.ToString ("000");

				_selection[i].name = newName;
			}
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
			EditorGUILayout.BeginVertical (EditorStyles.helpBox);
			GUILayout.Space (5);

			_prefix = EditorGUILayout.TextField ("Prefix : ", _prefix, EditorStyles.miniTextField, GUILayout.ExpandWidth (true));
			_nameBody = EditorGUILayout.TextField ("Name : ", _nameBody, EditorStyles.miniTextField, GUILayout.ExpandWidth (true));
			_suffix = EditorGUILayout.TextField ("Suffix : ", _suffix, EditorStyles.miniTextField, GUILayout.ExpandWidth (true));
			_numbering = EditorGUILayout.Toggle ("Add numbering ?", _numbering);

			GUILayout.Space (5);
			EditorGUILayout.EndVertical ();
			GUILayout.Space (10);

			if (GUILayout.Button ("Rename Selected GameObjects", GUILayout.Height (45), GUILayout.ExpandWidth (true)))
				RenameSelection ();

			EditorGUILayout.EndVertical ();
			GUILayout.Space (10);
			EditorGUILayout.EndHorizontal ();

			Repaint ();
		}
	}
}