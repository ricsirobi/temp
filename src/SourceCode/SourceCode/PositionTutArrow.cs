using System;
using UnityEngine;

public class PositionTutArrow : MonoBehaviour
{
	[Serializable]
	public class AnchorOffsets
	{
		public UIAnchor.Side _Anchor;

		public Vector3 _Position;

		public Vector3 _Angle;
	}

	public AnchorOffsets[] _Offsets;

	public KAWidget _TargetObject;

	private KAUIDragObject mDragObject;

	private void Start()
	{
		if (_TargetObject != null)
		{
			mDragObject = _TargetObject.gameObject.GetComponent<KAUIDragObject>();
			return;
		}
		base.enabled = false;
		Debug.LogError("_TargetObject is not assigned for " + base.name);
	}

	private void Update()
	{
		if (!(mDragObject != null) || !mDragObject.pIsReady)
		{
			return;
		}
		AnchorOffsets[] offsets = _Offsets;
		foreach (AnchorOffsets anchorOffsets in offsets)
		{
			if (anchorOffsets._Anchor == _TargetObject.pAnchor.side)
			{
				UtUtilities.AttachToAnchor(_TargetObject.pUI.gameObject, base.gameObject, anchorOffsets._Anchor);
				Vector3 localPosition = _TargetObject.GetPosition() + anchorOffsets._Position;
				localPosition.z = -1f;
				base.transform.localPosition = localPosition;
				base.transform.localRotation = Quaternion.Euler(anchorOffsets._Angle);
			}
		}
		base.enabled = false;
	}
}
