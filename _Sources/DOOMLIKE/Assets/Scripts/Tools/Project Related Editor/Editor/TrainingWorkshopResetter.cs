namespace DoomlikeEditor
{
    using Doomlike;
	using UnityEngine;
	using UnityEditor;

    public class TrainingWorkshopResetter
	{
		[MenuItem("Tools/Doomlike/Reset Training Workshops Scores")]
		public static void ResetTrainingWorkshops()
		{
			TrainingWorkshop[] workshops = GameObject.FindObjectsOfType<TrainingWorkshop>();
			Debug.Log($"Resetting {workshops.Length} Training Workshops scores...");

			for (int i = workshops.Length - 1; i >= 0; --i)
				workshops[i].ResetScore();
		}
	}
}