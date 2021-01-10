namespace DoomlikeEditor
{
	using UnityEngine;
	using UnityEditor;

    public class NavigationStaticFlagEditor
	{
		[MenuItem("Doomlike/Set Ground as Navigation Static")]
		public static void ResetTrainingWorkshops()
		{
			Transform[] sceneTransforms = GameObject.FindObjectsOfType<Transform>();

			for (int i = sceneTransforms.Length - 1; i >= 0; --i)
				if (sceneTransforms[i].name.Contains("Ground"))
                {
					StaticEditorFlags flags = GameObjectUtility.GetStaticEditorFlags(sceneTransforms[i].gameObject);
					flags |= StaticEditorFlags.NavigationStatic;
					GameObjectUtility.SetStaticEditorFlags(sceneTransforms[i].gameObject, flags);

					Doomlike.ConsoleProLogger.LogEditor($"Setting <b>{sceneTransforms[i].name}</b> as Navigation Static.", sceneTransforms[i].gameObject);
                }
		}
	}
}