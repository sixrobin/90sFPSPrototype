namespace DoomlikeEditor
{
	using UnityEngine;
	using UnityEditor;

	public static class GameObjectExtensions
    {
		public static void SetPhysicMaterial(this GameObject go, PhysicMaterial pm)
		{
			if (go.TryGetComponent(out Collider collider))
				collider.material = pm;

			foreach (Transform child in go.transform)
				child.gameObject.SetPhysicMaterial(pm);
		}
	}

	sealed class PhysicMaterialRecursiveSetterMenu
	{
		[MenuItem("Tools/Physic Material Recursive Setter", true)]
		static bool CheckSelectionCount()
		{
			return Selection.gameObjects.Length > 0;
		}

		[MenuItem("Tools/Physic Material Recursive Setter")]
		public static void LaunchLayerSetter()
		{
			PhysicMaterialRecursiveSetterEditor.LaunchSetter();
		}
	}

	sealed class PhysicMaterialRecursiveSetterEditor : EditorWindow
	{
		GameObject[] _selection;
		PhysicMaterial _pm;

		public static void LaunchSetter()
		{
			EditorWindow window = GetWindow<PhysicMaterialRecursiveSetterEditor>("Set objet's children physic material");
			window.Show();
		}

		private void OnGUI()
		{
			_selection = Selection.gameObjects;

			EditorGUILayout.LabelField("Physic Material", EditorStyles.boldLabel);
			_pm = (PhysicMaterial)EditorGUILayout.ObjectField(_pm, typeof(PhysicMaterial), true);

			if (GUILayout.Button("Set objet's children physic material recursively", GUILayout.Height(45), GUILayout.ExpandWidth(true)))
            {
				foreach (GameObject selected in _selection)
                    selected.SetPhysicMaterial(_pm);

                UnityEngine.SceneManagement.Scene scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
                UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(scene);
			}

			Repaint();
		}
	}
}