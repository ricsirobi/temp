using System;
using UnityEngine;

public class TwoInOneUtil : MonoBehaviour
{
	public enum TwoInOneMode
	{
		Tablet,
		NonTablet
	}

	[SerializeField]
	private bool _DebugSwitch;

	private TwoInOneMode? _mode;

	public const string WSA_HYBRID_DEVICE = "WSA_HYBRID_DEVICE";

	public static bool pIsHybridDevice;

	public event Action<TwoInOneMode> ModeChanged;

	protected void OnModeChanged(TwoInOneMode obj)
	{
		if (!_mode.HasValue || _mode.Value != obj)
		{
			_mode = obj;
			this.ModeChanged?.Invoke(obj);
			if (KAInput.pInstance != null)
			{
				KAInput.pInstance.pInputMode = ((_mode != TwoInOneMode.Tablet) ? KAInputMode.MOUSE : KAInputMode.TOUCH);
			}
		}
	}

	protected void Awake()
	{
		UnityEngine.Object.Destroy(base.gameObject);
	}
}
