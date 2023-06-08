using System;

[Serializable]
public class AvAvatarUWSwimmingData
{
	public float _RollTurnRate = 1.3f;

	public float _RollDampRate = 1.8f;

	public float _YawTurnRate = 2f;

	public float _YawTurnFactor = 0.3f;

	public float _PitchTurnRate = 1.5f;

	public float _PitchDampRate = 1.5f;

	public MinMax _Speed = new MinMax(4f, 16f);

	public float _Acceleration = 1.5f;

	public float _ManualBoostAccel = 2f;

	public float _ManualBoostTimer = 1f;

	public float _SpeedDampRate = 10f;

	public float _BrakeDecel = 0.5f;

	public float _ClimbAccelRate = 0.2f;

	public float _DiveAccelRate = 1f;

	public float _SpeedModifierOnCollision = 1f;

	public float _BounceOnCollision = 3f;

	public float _MaxUpPitch = 45f;

	public float _MaxDownPitch = 60f;

	public float _MaxRoll = 30f;

	public AvAvatarUWSwimmingData Clone()
	{
		AvAvatarUWSwimmingData obj = new AvAvatarUWSwimmingData
		{
			_RollTurnRate = _RollTurnRate,
			_RollDampRate = _RollDampRate,
			_PitchTurnRate = _PitchTurnRate,
			_PitchDampRate = _PitchDampRate,
			_Acceleration = _Acceleration,
			_ManualBoostAccel = _ManualBoostAccel,
			_ManualBoostTimer = _ManualBoostTimer,
			_SpeedDampRate = _SpeedDampRate,
			_BrakeDecel = _BrakeDecel,
			_ClimbAccelRate = _ClimbAccelRate,
			_DiveAccelRate = _DiveAccelRate,
			_MaxUpPitch = _MaxUpPitch,
			_MaxDownPitch = _MaxDownPitch,
			_MaxRoll = _MaxRoll,
			_SpeedModifierOnCollision = _SpeedModifierOnCollision,
			_BounceOnCollision = _BounceOnCollision
		};
		MinMax speed = new MinMax(_Speed.Min, _Speed.Max);
		obj._Speed = speed;
		return obj;
	}
}
