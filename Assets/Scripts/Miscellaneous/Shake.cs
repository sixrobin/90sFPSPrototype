namespace Doomlike
{
	using System;
	using UnityEngine;

	public enum CoordinateAxes : byte
	{
		X = 1,
		Y = 2,
		Z = 4,
		XY = X | Y,
		XZ = X | Z,
		YZ = Y | Z,
		XYZ = X | Y | Z
	}

	public class Shake
	{
		[System.Serializable]
		public struct ShakeSettings
		{
			public CoordinateAxes PosAxes;
			public CoordinateAxes RotAxes;
			public float Speed;
			public float Radius;
			public float XRotMax;
			public float YRotMax;
			public float ZRotMax;

			public static ShakeSettings Default => new ShakeSettings()
			{
				PosAxes = CoordinateAxes.XY,
				RotAxes = CoordinateAxes.XYZ,
				Speed = 15,
				Radius = 0.3f,
				XRotMax = 15,
				YRotMax = 15,
				ZRotMax = 15
			};
		}

		public float Multiplier { get; private set; } = 1f;

		float _trauma;
		public float Trauma
		{
			get => _trauma;
			private set => _trauma = Mathf.Clamp01(value);
		}

		public ShakeSettings Settings { get; private set; }

		public Shake()
		{
			Settings = ShakeSettings.Default;
		}

		public Shake(ShakeSettings settings)
		{
			Settings = settings;
		}

		/// <summary>
		/// Sets the shake settings.
		/// </summary>
		/// <param name="settings"> The settings to use. </param>
		public void SetSettings(ShakeSettings settings)
		{
			Settings = settings;
		}

		public void SetMultiplier(float value)
		{
			Multiplier = Mathf.Max(0, value);
		}

		/// <summary>
		/// Sets the trauma value (clamped between 0 and 1).
		/// </summary>
		/// <param name="value"> The new trauma value. </param>
		public void SetTrauma(float value)
		{
			Trauma = value;
		}

		/// <summary>
		/// Adds a given value to the current trauma (clamped between 0 and 1).
		/// </summary>
		/// <param name="amount"> Amount to add. </param>
		public void AddTrauma(float amount)
		{
			Trauma += amount;
		}

		/// <summary>
		/// Evaluates and returns the shake values, based on a given transform.
		/// </summary>
		/// <param name="transform"> The transform to shake. </param>
		/// <returns> The shake values. </returns>
		public Tuple<Vector3, Quaternion> Evaluate(Transform transform)
		{
			if (Trauma == 0)
				return null;

			Vector3 offsetPos = Vector3.zero;

			if ((Settings.PosAxes & CoordinateAxes.X) == CoordinateAxes.X)
				offsetPos += transform.right * (Mathf.PerlinNoise(Time.time * Settings.Speed, 0) - 0.5f) * 2;
			if ((Settings.PosAxes & CoordinateAxes.Y) == CoordinateAxes.Y)
				offsetPos += transform.up * (Mathf.PerlinNoise(0, (Time.time + 5) * Settings.Speed) - 0.5f) * 2;
			if ((Settings.PosAxes & CoordinateAxes.Z) == CoordinateAxes.Z)
				offsetPos += transform.forward * (Mathf.PerlinNoise(0, (Time.time + 10) * Settings.Speed) - 0.5f) * 2;

			float sqrTrauma = Trauma * Trauma;
			offsetPos *= Settings.Radius * sqrTrauma * Multiplier;

			Quaternion offsetRot = Quaternion.Euler
			(
				(Settings.RotAxes & CoordinateAxes.X) != CoordinateAxes.X ? 0 : (Mathf.PerlinNoise(Time.time * Settings.Speed, 0) - 0.5f) * 2 * Settings.XRotMax * sqrTrauma * Multiplier,
				(Settings.RotAxes & CoordinateAxes.Y) != CoordinateAxes.Y ? 0 : (Mathf.PerlinNoise(Time.time * Settings.Speed + 2, 0) - 0.5f) * 2 * Settings.YRotMax * sqrTrauma * Multiplier,
				(Settings.RotAxes & CoordinateAxes.Z) != CoordinateAxes.Z ? 0 : (Mathf.PerlinNoise(Time.time * Settings.Speed + 4, 0) - 0.5f) * 2 * Settings.ZRotMax * sqrTrauma * Multiplier
			);

			Trauma -= Time.unscaledDeltaTime;
			if (Trauma < 0)
				Trauma = 0;

			return new Tuple<Vector3, Quaternion>(offsetPos, offsetRot);
		}
	}
}