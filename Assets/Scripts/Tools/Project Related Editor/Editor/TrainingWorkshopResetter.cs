namespace DoomlikeEditor
{
    using Doomlike;
	using UnityEngine;
	using UnityEditor;

    public class TrainingWorkshopResetter
	{
		[MenuItem("Doomlike/Reset Training Workshops Scores")]
		public static void ResetTrainingWorkshops()
		{
			TrainingWorkshop[] workshops = GameObject.FindObjectsOfType<TrainingWorkshop>();
			ConsoleProLogger.LogEditor($"Resetting {workshops.Length} Training Workshops scores...");

			for (int i = workshops.Length - 1; i >= 0; --i)
				workshops[i].ResetScore();
		}
	}
}