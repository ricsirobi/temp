using UnityEngine;

public class UiCompass : KAUI
{
	[Range(1f, 10f)]
	public float _CompassStrength = 1f;

	public Transform _CompassNeedle;

	public Transform _NotationsHolder;

	public string _DirectionMarker = "PfMarkerNorth";

	private Quaternion mNeedleRotationVectorInQuat = Quaternion.identity;

	private Quaternion mPrevRotation = Quaternion.identity;

	private Vector3 mPrevPosition = Vector3.zero;

	private Transform mDirectionMarker;

	private MagneticZoneHandler mMagenticZoneHandlerRef;

	public static GameObject mInstance;

	public static void ToggleCompass()
	{
		if (mInstance != null)
		{
			Object.Destroy(mInstance);
			return;
		}
		KAUICursorManager.SetDefaultCursor("Loading");
		RsResourceManager.LoadAssetFromBundle(GameConfig.GetKeyData("CompassAsset"), OnCompassLoaded, typeof(GameObject));
	}

	public static void OnCompassLoaded(string inURL, RsResourceLoadEvent inLoadEvent, float inProgress, object inObject, object inUserData)
	{
		switch (inLoadEvent)
		{
		case RsResourceLoadEvent.COMPLETE:
			KAUICursorManager.SetDefaultCursor("Arrow");
			Object.Instantiate((GameObject)inObject).name = ((GameObject)inObject).name;
			break;
		case RsResourceLoadEvent.ERROR:
			Debug.LogError("Error loading Compass!" + inURL);
			KAUICursorManager.SetDefaultCursor("Arrow");
			break;
		}
	}

	protected override void Awake()
	{
		base.Awake();
		mInstance = base.gameObject;
		if (AvAvatar.pToolbar != null)
		{
			base.transform.parent = AvAvatar.pToolbar.transform;
		}
	}

	protected override void Start()
	{
		base.Start();
		GameObject gameObject = GameObject.Find(_DirectionMarker);
		if (gameObject == null)
		{
			Object.Destroy(base.gameObject);
		}
		if (gameObject != null)
		{
			mDirectionMarker = gameObject.transform;
			mMagenticZoneHandlerRef = mDirectionMarker.GetComponent<MagneticZoneHandler>();
		}
	}

	public override void OnClick(KAWidget item)
	{
		base.OnClick(item);
		if (item.name == "CloseBtn")
		{
			Object.Destroy(base.gameObject);
		}
	}

	protected override void Update()
	{
		if (AvAvatar.mTransform != null && mDirectionMarker != null)
		{
			Transform mTransform = AvAvatar.mTransform;
			if (mTransform.rotation != mPrevRotation || mTransform.position != mPrevPosition)
			{
				mPrevRotation = mTransform.rotation;
				mPrevPosition = mTransform.position;
				float needleAngle = GetNeedleAngle(mTransform, mDirectionMarker);
				float num = 0f;
				foreach (MagneticZoneInfo zone in mMagenticZoneHandlerRef.Zones)
				{
					if (Vector3.Distance(mTransform.position, zone.transform.position) <= zone._Radius && zone._MagneticPower > num)
					{
						num = zone._MagneticPower;
						needleAngle = GetNeedleAngle(mTransform, zone.transform);
					}
				}
				Vector3 euler = new Vector3(_CompassNeedle.rotation.eulerAngles.x, _CompassNeedle.rotation.eulerAngles.y, needleAngle);
				mNeedleRotationVectorInQuat = Quaternion.Euler(euler);
			}
			Quaternion rotation = _CompassNeedle.rotation;
			_CompassNeedle.rotation = Quaternion.Lerp(_CompassNeedle.rotation, mNeedleRotationVectorInQuat, _CompassStrength * Time.deltaTime);
			if (rotation != _CompassNeedle.rotation && _NotationsHolder != null)
			{
				foreach (Transform item in _NotationsHolder)
				{
					item.localRotation = Quaternion.Euler(new Vector3(item.localEulerAngles.x, item.localEulerAngles.y, 0f - _CompassNeedle.localEulerAngles.z));
				}
			}
		}
		base.Update();
	}

	public float GetNeedleAngle(Transform inSourceTranform, Transform inTargetTranform)
	{
		Quaternion quaternion = Quaternion.LookRotation(inTargetTranform.localPosition - inSourceTranform.localPosition);
		return inSourceTranform.rotation.eulerAngles.y - quaternion.eulerAngles.y;
	}
}
