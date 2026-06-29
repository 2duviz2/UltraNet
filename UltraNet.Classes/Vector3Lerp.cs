using UnityEngine;

namespace UltraNet.Classes;

public struct Vector3Lerp
{
	public Vector3 PreviousValue;

	public Vector3 TargetValue;

	public float Duration;

	public float StartTime;

	public Vector3 Grab()
	{
		return Vector3.Lerp(PreviousValue, TargetValue, (Time.realtimeSinceStartup - StartTime) / Duration);
	}

	public void Set(Vector3 value)
	{
		PreviousValue = ((StartTime == 0f) ? value : Grab());
		TargetValue = value;
		Duration = Mathf.Max(Time.realtimeSinceStartup - StartTime, PlayerFetcher.syncTime);
		StartTime = Time.realtimeSinceStartup;
	}
}
