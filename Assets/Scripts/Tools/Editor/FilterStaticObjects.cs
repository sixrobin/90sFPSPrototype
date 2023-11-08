namespace RSTools
{
	using UnityEngine;
	using UnityEditor;

	sealed class FilterStaticObjectMenu
	{
		[MenuItem ("Tools/Filter Static Objects")]
		public static void RenameSelectedObjects ()
		{
			FilterStaticObjectEditor.LaunchFilter ();
		}
	}

	sealed class FilterStaticObjectEditor : EditorWindow
	{
		StaticEditorFlags _flag;
		bool _include;

		public static void LaunchFilter ()
		{
			GetWindow<FilterStaticObjectEditor> ("Filter Static Objects").Show ();
		}

		void FilterSelection (bool include)
		{
			var gameObjects = FindObjectsOfType (typeof (GameObject));
			var gameObjectsArray = new GameObject[gameObjects.Length];

			int arrayPointer = 0;

			foreach (GameObject gameObject in gameObjects)
			{
				StaticEditorFlags flags = GameObjectUtility.GetStaticEditorFlags (gameObject);

				if (include ? (flags & _flag) != 0 : (flags & _flag) == 0)
				{
					gameObjectsArray[arrayPointer] = gameObject;
					arrayPointer += 1;
				}
			}

			Selection.objects = gameObjectsArray;
		}

		void OnGUI ()
		{
			EditorGUILayout.BeginHorizontal ();
			GUILayout.Space (10);
			EditorGUILayout.BeginVertical ();
			GUILayout.Space (10);

			EditorGUILayout.LabelField ("Flag to filter :", EditorStyles.boldLabel);
			var options = System.Enum.GetValues (typeof (StaticEditorFlags));
			_flag = (StaticEditorFlags)EditorGUILayout.EnumPopup (_flag);

			_include = EditorGUILayout.Toggle ("Include", _include);

			GUILayout.Space (10);

			if (GUILayout.Button ("Filter Selection", GUILayout.Height (45), GUILayout.ExpandWidth (true)))
			{
				FilterSelection (_include);
			}

			EditorGUILayout.EndVertical ();
			GUILayout.Space (10);
			EditorGUILayout.EndHorizontal ();

			Repaint ();
		}
	}
}