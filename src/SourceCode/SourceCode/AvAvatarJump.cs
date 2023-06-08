using System;

[Serializable]
public class AvAvatarJump
{
	public string _JumpAnim;

	public string _SuperJumpAnim;

	public string _FallAnim;

	public string _LandAnim;

	public float _TimeSinceOnGround;

	public float _MinJumpHeight;

	public float _MaxJumpHeight;

	public float _MaxJumpTime;

	public JumpAnimData[] _SuperJumpAnimData;

	public JumpAnimData[] _JumpAnimData;

	public JumpAnimData[] _NoJumpAnimData;
}
