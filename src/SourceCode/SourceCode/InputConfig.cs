using System;

[Serializable]
public class InputConfig
{
	public SceneGroup _Scenes;

	public PlatformGroup _Platforms;

	public KeyConfig[] _Keys;

	public TouchConfig[] _Touches;

	public JoystickConfig _Joystick;

	public AxisConfig[] _AxisConf;

	public UIButtonConfig[] _UIConf;

	public Axis _Axis;
}
