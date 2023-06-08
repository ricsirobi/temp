using System;
using UnityEngine;

[Serializable]
public class AvAvatarStateData
{
	public AvAvatarSubState _State;

	public float _Acceleration = 10f;

	public float _RotSpeed = 100f;

	public float _RestThreshold = 0.1f;

	public float _WalkThreshold = 3f;

	public float _WalkAnimScale = 0.3f;

	public float _RunAnimScale = 0.2f;

	public float _MaxForwardSpeed = 6f;

	public float _MinForwardSpeed;

	public float _MaxBackwardSpeed = 2.5f;

	public float _MaxAirSpeed = 4.25f;

	public float _Gravity = -16f;

	public float _MaxForwardBackwardTilt;

	public float _MaxSidewaysTilt;

	public float _PushPower;

	public float _FidgetTimeMin = 8f;

	public float _FidgetTimeMax = 12f;

	public float _Height = 0.35f;

	public AvAvatarCamParams _AvatarCameraParams;

	public Vector3 _JumpBack = Vector3.zero;

	public AvAvatarJump _JumpValues;

	public AvAvatarStateAnims _StateAnims;

	public AvAvatarStateData Clone()
	{
		AvAvatarStateData avAvatarStateData = new AvAvatarStateData();
		avAvatarStateData._State = _State;
		avAvatarStateData._Acceleration = _Acceleration;
		avAvatarStateData._RotSpeed = _RotSpeed;
		avAvatarStateData._RestThreshold = _RestThreshold;
		avAvatarStateData._WalkThreshold = _WalkThreshold;
		avAvatarStateData._WalkAnimScale = _WalkAnimScale;
		avAvatarStateData._RunAnimScale = _RunAnimScale;
		avAvatarStateData._MaxForwardSpeed = _MaxForwardSpeed;
		avAvatarStateData._MinForwardSpeed = _MinForwardSpeed;
		avAvatarStateData._MaxBackwardSpeed = _MaxBackwardSpeed;
		avAvatarStateData._MaxAirSpeed = _MaxAirSpeed;
		avAvatarStateData._Gravity = _Gravity;
		avAvatarStateData._MaxForwardBackwardTilt = _MaxForwardBackwardTilt;
		avAvatarStateData._MaxSidewaysTilt = _MaxSidewaysTilt;
		avAvatarStateData._PushPower = _PushPower;
		avAvatarStateData._FidgetTimeMin = _FidgetTimeMin;
		avAvatarStateData._FidgetTimeMax = _FidgetTimeMax;
		avAvatarStateData._Height = _Height;
		avAvatarStateData._JumpBack = _JumpBack;
		avAvatarStateData._AvatarCameraParams = new AvAvatarCamParams();
		avAvatarStateData._AvatarCameraParams._FocusHeight = _AvatarCameraParams._FocusHeight;
		avAvatarStateData._AvatarCameraParams._IgnoreCollision = _AvatarCameraParams._IgnoreCollision;
		avAvatarStateData._AvatarCameraParams._MaxCameraDistance = _AvatarCameraParams._MaxCameraDistance;
		avAvatarStateData._AvatarCameraParams._Polar = _AvatarCameraParams._Polar;
		avAvatarStateData._AvatarCameraParams._Speed = _AvatarCameraParams._Speed;
		avAvatarStateData._JumpValues = new AvAvatarJump();
		avAvatarStateData._JumpValues._FallAnim = _JumpValues._FallAnim;
		avAvatarStateData._JumpValues._JumpAnim = _JumpValues._JumpAnim;
		avAvatarStateData._JumpValues._JumpAnimData = _JumpValues._JumpAnimData;
		avAvatarStateData._JumpValues._LandAnim = _JumpValues._LandAnim;
		avAvatarStateData._JumpValues._MaxJumpHeight = _JumpValues._MaxJumpHeight;
		avAvatarStateData._JumpValues._MaxJumpTime = _JumpValues._MaxJumpTime;
		avAvatarStateData._JumpValues._MinJumpHeight = _JumpValues._MinJumpHeight;
		avAvatarStateData._JumpValues._NoJumpAnimData = _JumpValues._NoJumpAnimData;
		avAvatarStateData._JumpValues._SuperJumpAnim = _JumpValues._SuperJumpAnim;
		avAvatarStateData._JumpValues._SuperJumpAnimData = _JumpValues._SuperJumpAnimData;
		avAvatarStateData._JumpValues._TimeSinceOnGround = _JumpValues._TimeSinceOnGround;
		avAvatarStateData._StateAnims = _StateAnims;
		return avAvatarStateData;
	}
}
