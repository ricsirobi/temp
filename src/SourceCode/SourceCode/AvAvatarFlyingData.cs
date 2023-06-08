using System;

[Serializable]
public class AvAvatarFlyingData
{
	public float _RollTurnRate = 1.3f;

	public float _RollDampRate = 1.8f;

	public float _YawTurnRate = 2f;

	public float _YawTurnFactor = 0.3f;

	public float _PitchTurnRate = 1.5f;

	public float _PitchDampRate = 1.5f;

	public MinMax _Speed = new MinMax(4f, 16f);

	public float _Acceleration = 1.5f;

	public float _ManualFlapAccel = 2f;

	public float _ManualFlapTimer = 1f;

	public float _SpeedDampRate = 10f;

	public float _BrakeDecel = 0.5f;

	public float _ClimbAccelRate = 0.2f;

	public float _DiveAccelRate = 1f;

	public float _SpeedModifierOnCollision = 1f;

	public float _BounceOnCollision = 3f;

	public float _GlideDownMultiplier = 0.5f;

	public float _GravityModifier;

	public float _GravityClimbMultiplier = 1f;

	public float _GravityDiveMultiplier = 1f;

	public float _FlyingMaxUpPitch = 40f;

	public float _FlyingMaxDownPitch = 50f;

	public float _GlidingMaxUpPitch = 30f;

	public float _GlidingMaxDownPitch = 50f;

	public float _MaxRoll = 40f;

	public float _FlyingPositionBoostFactor = 1f;

	public AvAvatarFlyingData Clone()
	{
		AvAvatarFlyingData obj = new AvAvatarFlyingData
		{
			_RollTurnRate = _RollTurnRate,
			_RollDampRate = _RollDampRate,
			_YawTurnRate = _YawTurnRate,
			_YawTurnFactor = _YawTurnFactor,
			_PitchTurnRate = _PitchTurnRate,
			_PitchDampRate = _PitchDampRate,
			_Acceleration = _Acceleration,
			_ManualFlapAccel = _ManualFlapAccel,
			_ManualFlapTimer = _ManualFlapTimer,
			_SpeedDampRate = _SpeedDampRate,
			_BrakeDecel = _BrakeDecel,
			_ClimbAccelRate = _ClimbAccelRate,
			_DiveAccelRate = _DiveAccelRate,
			_GlideDownMultiplier = _GlideDownMultiplier,
			_FlyingMaxUpPitch = _FlyingMaxUpPitch,
			_FlyingMaxDownPitch = _FlyingMaxDownPitch,
			_GlidingMaxUpPitch = _GlidingMaxUpPitch,
			_GlidingMaxDownPitch = _GlidingMaxDownPitch,
			_MaxRoll = _MaxRoll,
			_SpeedModifierOnCollision = _SpeedModifierOnCollision,
			_BounceOnCollision = _BounceOnCollision,
			_FlyingPositionBoostFactor = _FlyingPositionBoostFactor
		};
		MinMax speed = new MinMax(_Speed.Min, _Speed.Max);
		obj._Speed = speed;
		return obj;
	}
}
