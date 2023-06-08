using System;
using UnityEngine;

public class DragonTaxiTrigger : KAMonoBase
{
	public bool _Draw;

	public TaxiDragonData _TaxiDragonData;

	public TaxiUISceneData[] _LevelData;

	public float _MaxRadiusBoundary = 360f;

	public float _MaxYCylinderBoundary = 150f;

	public float _AvatarRotationBackSpeed = 9f;

	public string _ManagerBundlePath = "RS_DATA/PfDragonTaxiManagerDO.unity3d/PfDragonTaxiManagerDO";

	public LocaleString _TaxiLockedText = new LocaleString("You can not use Taxi now!");

	public float _DragonFlyDownTime = 2f;

	public float _DragonFlyDownXRot = 50f;

	private Vector3 mPrevAvatarPos = Vector3.zero;

	private Quaternion mTargetRot = Quaternion.identity;

	private bool mIsClickable;

	private float mDragonFlyDownTimer = -1f;

	private bool mCylinderYBoundaryHit;

	private KAUIGenericDB mKAUIGenericDB;

	private bool mIsLoadingMap;

	private bool mIsBoundaryTrigger;

	private UiWorldMap mUiWorldMap;

	private void Start()
	{
		mIsClickable = GetComponent<ObClickable>() != null;
	}

	private void OnActivate()
	{
		if (!TaxiLocked())
		{
			TriggerWorldMap(isBoundaryTrigger: false);
			AvAvatar.SetUIActive(inActive: false);
		}
		else
		{
			PopupDB("TaxiLocked", _TaxiLockedText);
		}
	}

	private void Update()
	{
		if (SanctuaryManager.pCurPetInstance == null || SanctuaryManager.pCurPetInstance.pAge < 3 || DragonTaxiManager.pIsReady || AvAvatar.pSubState != AvAvatarSubState.FLYING || mIsClickable)
		{
			return;
		}
		if (mTargetRot != Quaternion.identity)
		{
			ProcessAvatarRotation();
			return;
		}
		if (!mIsLoadingMap && mUiWorldMap == null)
		{
			float y = AvAvatar.GetPosition().y;
			if ((AvAvatar.GetPosition() - new Vector3(base.transform.position.x, y, base.transform.position.z)).magnitude >= _MaxRadiusBoundary && (mPrevAvatarPos - new Vector3(base.transform.position.x, y, base.transform.position.z)).magnitude < _MaxRadiusBoundary)
			{
				mCylinderYBoundaryHit = false;
				TriggerWorldMap(isBoundaryTrigger: true);
			}
			if (y - base.transform.position.y >= _MaxYCylinderBoundary && mPrevAvatarPos.y - base.transform.position.y < _MaxYCylinderBoundary)
			{
				TriggerWorldMap(isBoundaryTrigger: true);
				mCylinderYBoundaryHit = true;
			}
		}
		if (mDragonFlyDownTimer > -1f)
		{
			UpdateDragonFlyDownTimer();
		}
		mPrevAvatarPos = AvAvatar.GetPosition();
	}

	private void UpdateDragonFlyDownTimer()
	{
		mDragonFlyDownTimer -= Time.deltaTime;
		AvAvatarController component = AvAvatar.pObject.GetComponent<AvAvatarController>();
		if (component != null)
		{
			component._FlyingBone.localEulerAngles = new Vector3(_DragonFlyDownXRot, component._FlyingBone.localEulerAngles.y, component._FlyingBone.localEulerAngles.z);
		}
		if (mDragonFlyDownTimer < 0f)
		{
			mDragonFlyDownTimer = -1f;
		}
	}

	private void ProcessAvatarRotation()
	{
		if (!(AvAvatar.pObject != null))
		{
			return;
		}
		AvAvatar.mTransform.rotation = Quaternion.Slerp(AvAvatar.GetRotation(), mTargetRot, Time.deltaTime * _AvatarRotationBackSpeed);
		if (Mathf.Abs(AvAvatar.GetRotation().eulerAngles.y - mTargetRot.eulerAngles.y) < 0.2f)
		{
			AvAvatar.mTransform.rotation = mTargetRot;
			AvAvatar.pState = AvAvatar.pPrevState;
			mTargetRot = Quaternion.identity;
			if (mCylinderYBoundaryHit)
			{
				mDragonFlyDownTimer = _DragonFlyDownTime;
			}
		}
	}

	private void TriggerWorldMap(bool isBoundaryTrigger)
	{
		if (!mIsLoadingMap && !(mUiWorldMap != null))
		{
			mIsBoundaryTrigger = isBoundaryTrigger;
			mIsLoadingMap = true;
			AvAvatar.SetUIActive(inActive: false);
			AvAvatar.pState = AvAvatarState.PAUSED;
			KAUICursorManager.SetDefaultCursor("Loading");
			RsResourceManager.LoadAssetFromBundle(GameConfig.GetKeyData("WorldMapUIAsset"), OnWorldMapLoaded, typeof(GameObject));
		}
	}

	public void OnWorldMapLoaded(string inURL, RsResourceLoadEvent inEvent, float inProgress, object inObject, object inUserData)
	{
		switch (inEvent)
		{
		case RsResourceLoadEvent.COMPLETE:
		{
			GameObject gameObject = UnityEngine.Object.Instantiate((GameObject)inObject);
			gameObject.name = "PfUiWorldMap";
			mUiWorldMap = gameObject.GetComponent<UiWorldMap>();
			if (mUiWorldMap != null)
			{
				mUiWorldMap._MessageObject = base.gameObject;
			}
			KAUICursorManager.SetDefaultCursor("Arrow");
			mIsLoadingMap = false;
			break;
		}
		case RsResourceLoadEvent.ERROR:
			AvAvatar.pState = AvAvatarState.IDLE;
			AvAvatar.SetUIActive(inActive: true);
			KAUICursorManager.SetDefaultCursor("Arrow");
			mIsLoadingMap = false;
			break;
		}
	}

	private bool TaxiLocked()
	{
		TaxiUISceneData[] levelData = _LevelData;
		for (int i = 0; i < levelData.Length; i++)
		{
			if (UnlockManager.IsSceneUnlocked(levelData[i]._LoadLevel))
			{
				return false;
			}
		}
		return true;
	}

	private void PopupDB(string inDBName, LocaleString inStr)
	{
		mKAUIGenericDB = GameUtilities.CreateKAUIGenericDB("PfKAUIGenericDB", inDBName);
		mKAUIGenericDB.SetDestroyOnClick(isDestroy: true);
		mKAUIGenericDB.SetButtonVisibility(inYesBtn: false, inNoBtn: false, inOKBtn: true, inCloseBtn: false);
		mKAUIGenericDB._MessageObject = base.gameObject;
		mKAUIGenericDB.SetTextByID(inStr._ID, inStr._Text, interactive: false);
	}

	private void OnDrawGizmos()
	{
		if (_Draw)
		{
			Gizmos.color = Color.magenta;
			for (int i = 0; i < 360; i += 30)
			{
				Vector3 vector = new Vector3(base.transform.position.x + Mathf.Cos((float)i * (MathF.PI / 180f)) * _MaxRadiusBoundary, base.transform.position.y + _MaxYCylinderBoundary, base.transform.position.z + Mathf.Sin((float)i * (MathF.PI / 180f)) * _MaxRadiusBoundary);
				Vector3 to = new Vector3(base.transform.position.x + Mathf.Cos((float)(i + 30) * (MathF.PI / 180f)) * _MaxRadiusBoundary, base.transform.position.y + _MaxYCylinderBoundary, base.transform.position.z + Mathf.Sin((float)(i + 30) * (MathF.PI / 180f)) * _MaxRadiusBoundary);
				Gizmos.DrawLine(vector, to);
				vector.y = base.transform.position.y - _MaxYCylinderBoundary;
				to.y = base.transform.position.y - _MaxYCylinderBoundary;
				Gizmos.DrawLine(vector, to);
				to = vector;
				to.y = base.transform.position.y + _MaxYCylinderBoundary;
				Gizmos.DrawLine(vector, to);
			}
		}
	}

	private void OnMapClosed()
	{
		if (mIsBoundaryTrigger)
		{
			mTargetRot = Quaternion.Euler(0f, AvAvatar.GetRotation().eulerAngles.y + 180f, 0f);
			AvAvatar.pState = AvAvatarState.PAUSED;
		}
		mUiWorldMap = null;
		mIsBoundaryTrigger = false;
	}

	private void OnLocationSelected(string sceneName)
	{
		mUiWorldMap.Show(show: false);
		mUiWorldMap = null;
		mIsBoundaryTrigger = false;
		TaxiUISceneData taxiUISceneData = null;
		if (_LevelData != null && _LevelData.Length != 0)
		{
			TaxiUISceneData[] levelData = _LevelData;
			foreach (TaxiUISceneData taxiUISceneData2 in levelData)
			{
				if (sceneName == taxiUISceneData2._LoadLevel)
				{
					taxiUISceneData = taxiUISceneData2;
					break;
				}
			}
		}
		if (taxiUISceneData != null)
		{
			AvAvatar.pStartLocation = taxiUISceneData._StartMarker;
		}
		if (AvAvatar.pObject != null)
		{
			AvAvatarController component = AvAvatar.pObject.GetComponent<AvAvatarController>();
			if (component != null)
			{
				component.ResetAvatarState();
			}
		}
		RsResourceManager.LoadLevel(sceneName);
	}
}
