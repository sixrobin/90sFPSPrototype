namespace RSTools
{
	using System;
	using UnityEngine;
	using UnityEditor;

	sealed class GameObjectsSorter
	{
		const string SHORTCUT = "%&s";

		[MenuItem ("GameObject/Sort by Names " + SHORTCUT, true)]
		static bool CheckSelectionCount ()
		{
			return Selection.gameObjects.Length > 1;
		}

		[MenuItem ("GameObject/Sort by Names " + SHORTCUT)]
		public static void SortObjectsByName ()
		{
			GameObject[] selection = Selection.gameObjects;
			Array.Sort (selection, delegate (GameObject a, GameObject b) { return a.name.CompareTo (b.name); });

			foreach (GameObject go in Selection.gameObjects)
				go.transform.SetSiblingIndex (Array.IndexOf (selection, go));
		}
	}
}