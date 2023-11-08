namespace DoomlikeEditor
{
	using UnityEngine;
	using UnityEditor;

	public class PostProcessEnabler
	{
		[MenuItem("Tools/Post Process/Enable")]
		public static void EnablePostProcessEnablerEditMode()
		{
			Debug.Log("Enabling Post Process layers...");

			UnityEngine.Rendering.PostProcessing.PostProcessLayer[] postProcessLayers = GameObject.FindObjectsOfType<UnityEngine.Rendering.PostProcessing.PostProcessLayer>();
			for (int i = postProcessLayers.Length - 1; i >= 0; --i)
				postProcessLayers[i].enabled = true;
		}

		[MenuItem("Tools/Post Process/Disable")]
		public static void DisablePostProcessEnablerEditMode()
		{
			Debug.Log("Disabling Post Process layers...");

			UnityEngine.Rendering.PostProcessing.PostProcessLayer[] postProcessLayers = GameObject.FindObjectsOfType<UnityEngine.Rendering.PostProcessing.PostProcessLayer>();
			for (int i = postProcessLayers.Length - 1; i >= 0; --i)
				postProcessLayers[i].enabled = false;
		}
	}
}