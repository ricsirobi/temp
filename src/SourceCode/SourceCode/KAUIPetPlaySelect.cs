using System;
using UnityEngine;

public class KAUIPetPlaySelect : KAUISelect
{
	public static KAUIPetPlaySelect Instance;

	public Camera _Camera;

	public Transform _PetMarker;

	public Transform _ExitMarker;

	public Transform _PetExitMarker;

	public GameObject _SoapBubbles;

	public GameObject _BathBubbles;

	[HideInInspector]
	public GameObject _PetLastTransform;

	public Transform[] _HomeObjects;

	public float _NextHomeTime = 10f;

	public int _PlayWithMuttAchievementID = 73;

	public RaisedPetStage[] _BlockedAgesForGlassDown;

	public Vector3 _GlassDownCameraOffset = new Vector3(0f, 0.7f, -0.6f);

	public Vector3 _GlassDownCameraRotation = Vector3.zero;

	public Vector3 _CloseUpCameraOffset = new Vector3(0f, 0f, 0f);

	public Vector3 _CloseUpCameraRotation = Vector3.zero;

	public StoreLoader.Selection _StoreInfo;

	protected bool mInitialized;

	[NonSerialized]
	public bool pNextHomeTimerPaused;

	[NonSerialized]
	public int pCurrentHomeIdx;

	[NonSerialized]
	public SanctuaryPet pPet;

	[NonSerialized]
	public KAWidget pSelectedItem;

	[NonSerialized]
	public GameObject pObjectInHand;

	[NonSerialized]
	public PetToyType pObjectinHandType;

	[NonSerialized]
	public KAUIPetPlaySelectMenu pPlayMenu;

	[NonSerialized]
	public bool pObjectAttachToMouse;

	[NonSerialized]
	public bool pOnGlass;

	private Quaternion mGlassUpCameraRot;

	private Vector3 mGlassUpCameraPos;

	protected Camera mOldCam;

	protected bool mOldHover;

	protected bool mOldFly;

	private bool mIsAIBundleReady;

	private string mPrevAnim = "";

	public bool _AllowMovement = true;

	private KAWidget mStoreBtn;

	private KAWidget mTxtCoin;

	private KAWidget mTxtGem;

	private bool mPetPlayedAtleastOnce;

	public bool pIsAIBundleReady => mIsAIBundleReady;

	public bool pPetPlayedAtleastOnce
	{
		get
		{
			return mPetPlayedAtleastOnce;
		}
		set
		{
			mPetPlayedAtleastOnce = value;
		}
	}

	protected override void Start()
	{
		base.Start();
		mStoreBtn = FindItem("StoreBtn");
		mTxtCoin = FindItem("TxtCoinAmount");
		mTxtGem = FindItem("TxtGemAmount");
		mPetPlayedAtleastOnce = false;
		Money.AddNotificationObject(base.gameObject);
		Instance = this;
	}

	public void SnapToHome()
	{
		pPet.transform.position = _HomeObjects[pCurrentHomeIdx].position;
		pPet.transform.rotation = _HomeObjects[pCurrentHomeIdx].rotation;
	}

	public Transform GetHomeObject()
	{
		return _HomeObjects[pCurrentHomeIdx];
	}

	public bool SetHome(int hidx, bool onGlass)
	{
		hidx = 1;
		if (_HomeObjects == null || _HomeObjects.Length == 0)
		{
			return false;
		}
		pCurrentHomeIdx = hidx;
		return true;
	}

	public virtual void HoldObject(GameObject obj, PetToyType otype, bool attach)
	{
		pObjectAttachToMouse = attach;
		pObjectInHand = obj;
		pObjectinHandType = otype;
		pObjectInHand.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);
	}

	public virtual void DropObject(bool lookatcam)
	{
		if (pPlayMenu.mToy != null)
		{
			pPlayMenu.mToy.OnDropObject();
		}
		pPlayMenu.mToy = null;
		if (pObjectInHand != null)
		{
			UnityEngine.Object.Destroy(pObjectInHand);
			pObjectInHand = null;
			if (pSelectedItem != null)
			{
				if (pPlayMenu._RemoveWhenSelected)
				{
					pSelectedItem.DetachFromCursor();
					RemoveWidget(pSelectedItem);
					pPlayMenu.AddWidget(pSelectedItem);
					pSelectedItem.SetPosition(0f, 0f);
					pSelectedItem.SetInteractive(isInteractive: true);
				}
				pSelectedItem = null;
			}
		}
		if (lookatcam && pPet != null)
		{
			pPet.SetLookAtObject(_Camera.transform, tween: true, Vector3.zero);
		}
		pObjectinHandType = PetToyType.UNKNOWN;
		pObjectAttachToMouse = false;
		if (pPet != null)
		{
			pPet.SetCanBePetted(t: true);
		}
	}

	public void SetCameraTransform(SanctuaryPet pet)
	{
	}

	public virtual void DoWakeUp()
	{
		pPet.PlayIdleAnimation();
	}

	public virtual void OnEnable()
	{
		mInitialized = false;
		if (pPet == null)
		{
			pPet = SanctuaryManager.pCurPetInstance;
		}
		if (pPet != null)
		{
			mPrevAnim = pPet._IdleAnimName;
			pPet.pWaterObject = null;
			mIsAIBundleReady = true;
		}
		Instance = this;
	}

	public void OnDisable()
	{
		Instance = null;
	}

	private void OnAIResEventHandler(string inURL, RsResourceLoadEvent inEvent, float inProgress, object inObject, object inUserData)
	{
		if (inEvent == RsResourceLoadEvent.COMPLETE)
		{
			mIsAIBundleReady = true;
		}
	}

	public override void OnClick(KAWidget item)
	{
		base.OnClick(item);
		if (item.name == "btnExit")
		{
			if (pPet != null)
			{
				pPet.AIActor.SetState(AISanctuaryPetFSM.NORMAL);
			}
			if (SanctuaryManager.pInstance.pPetMeter != null)
			{
				SanctuaryManager.pInstance.pPetMeter.AttachToToolbar();
			}
			if (MainStreetMMOClient.pInstance != null)
			{
				MainStreetMMOClient.pInstance.SetBusy(busy: false);
			}
			Input.ResetInputAxes();
			SetVisibility(inVisible: false);
			AvAvatar.SetActive(inActive: true);
			AvAvatar.pState = AvAvatarState.IDLE;
			if (MissionManager.pInstance != null)
			{
				MissionManager.pInstance.SetTimedTaskUpdate(inState: true);
			}
			base.transform.root.gameObject.SetActive(value: false);
			if (_ExitMarker != null)
			{
				AvAvatar.SetPosition(_ExitMarker);
			}
			DropObject(lookatcam: false);
			if (pPet != null)
			{
				pPet.SetTOW(null);
				pPet.SetCanBePetted(t: false);
				pPet.SetCamera(mOldCam);
				if (_PetExitMarker != null)
				{
					pPet.transform.position = _PetExitMarker.position;
					pPet.transform.rotation = _PetExitMarker.rotation;
					pPet.SetFollowAvatar(follow: false);
				}
				else if (_PetLastTransform != null)
				{
					pPet.transform.position = _PetLastTransform.transform.position;
					pPet.transform.rotation = _PetLastTransform.transform.rotation;
					UnityEngine.Object.Destroy(_PetLastTransform);
					_PetLastTransform = null;
					pPet.SetFollowAvatar(follow: true);
				}
				pPet._Move2D = true;
				pPet._ActionDoneMessageObject = null;
				pPet.RestoreScale();
				pPet.StopLookAtObject();
				pPet._Hover = mOldHover;
				pPet._CanFly = mOldFly;
				SetHome(1, onGlass: false);
				SanctuaryManager.pCheckPetAge = true;
				if (mPetPlayedAtleastOnce)
				{
					AwardPlayWithMuttAchievement();
				}
				pPet._IdleAnimName = mPrevAnim;
				pPet.PlayIdleAnimation();
			}
			UICursorManager.SetCursor("Arrow", showHideSystemCursor: true);
			if (PetPlayAreaLoader._MessageObject != null)
			{
				PetPlayAreaLoader._MessageObject.SendMessage("OnPlayAreaExit", SendMessageOptions.DontRequireReceiver);
			}
			SanctuaryManager.ResetToActivePet();
			if (RsResourceManager.pCurrentLevel == GameConfig.GetKeyData("PetPlayScene"))
			{
				AvAvatar.pStartLocation = AvAvatar.pSpawnAtSetPosition;
				UtUtilities.LoadLevel(PetPlayAreaLoader._ExitToScene);
			}
			else
			{
				UnityEngine.Object.Destroy(base.transform.root.gameObject);
			}
		}
		else if (item.name == "BtnStores" || item == mStoreBtn)
		{
			if (SanctuaryManager.pInstance.pPetMeter != null)
			{
				SanctuaryManager.pInstance.pPetMeter.gameObject.SetActive(value: false);
			}
			DropObject(lookatcam: true);
			pPet.SetTOW(null);
			pPet.SetCanBePetted(t: false);
			if (MissionManager.pInstance != null)
			{
				MissionManager.pInstance.SetTimedTaskUpdate(inState: false);
			}
			if (_StoreInfo != null)
			{
				StoreLoader.Load(setDefaultMenuItem: true, _StoreInfo._Category, _StoreInfo._Store, base.gameObject);
			}
			SetVisibility(inVisible: false);
		}
	}

	public void OnStoreClosed()
	{
		RestoreUI();
	}

	public void RestoreUI()
	{
		if (SanctuaryManager.pInstance.pPetMeter != null)
		{
			SanctuaryManager.pInstance.pPetMeter.gameObject.SetActive(value: true);
		}
		pPet.SetCanBePetted(t: true);
		if (MissionManager.pInstance != null)
		{
			MissionManager.pInstance.SetTimedTaskUpdate(inState: true, inForceUpdate: true);
		}
		pPlayMenu.ReloadMenu();
		_Camera.gameObject.SetActive(value: true);
		AvAvatar.SetActive(inActive: false);
		SetVisibility(inVisible: true);
		Instance = this;
	}

	public virtual void AwardPlayWithMuttAchievement()
	{
		UserAchievementTask.Set(_PlayWithMuttAchievementID);
	}

	public void SetPet(SanctuaryPet pet)
	{
		pPet = pet;
	}

	public override void PlayHelpDlg()
	{
		pPlayMenu.PlayIntro();
	}

	public void UpdatePet()
	{
		pPet = SanctuaryManager.pCurPetInstance;
		if (pPet != null)
		{
			pPet.SetCanBePetted(t: true);
			mOldHover = pPet._Hover;
			pPet._Hover = false;
			mOldFly = pPet._CanFly;
			pPet._CanFly = false;
			if (_PetMarker != null)
			{
				pPet.transform.position = _PetMarker.position;
				pPet.transform.rotation = _PetMarker.rotation;
				pPet.SetFollowAvatar(follow: false);
			}
			mOldCam = pPet.GetCamera();
			pPet.SetCamera(_Camera);
			pPet.SetState(Character_State.idle);
			pPet.SetPlayScale();
			pPet._Move2D = false;
			pPet._ActionDoneMessageObject = base.gameObject;
			pPet.SetLookAtObject(_Camera.transform, tween: true, Vector3.zero);
			SetHome(1, onGlass: false);
			pPlayMenu = (KAUIPetPlaySelectMenu)base.gameObject.GetComponent(typeof(KAUIPetPlaySelectMenu));
			pPlayMenu.PlayIntro();
			pPlayMenu.ReloadMenu();
		}
	}

	protected override void Update()
	{
		base.Update();
		if (!mInitialized)
		{
			if (pPet == null)
			{
				pPet = SanctuaryManager.pCurPetInstance;
			}
			if (pPet != null)
			{
				pPet.AIActor.SetState(AISanctuaryPetFSM.PET_PLAY);
				if (pPet.pIsMounted)
				{
					pPet.OnFlyDismountImmediate(AvAvatar.pObject);
				}
				pPet.SetCanBePetted(t: true);
				mOldHover = pPet._Hover;
				pPet._Hover = false;
				mOldFly = pPet._CanFly;
				pPet._CanFly = false;
				if (_PetMarker != null)
				{
					pPet.transform.position = _PetMarker.position;
					pPet.transform.rotation = _PetMarker.rotation;
					pPet.SetFollowAvatar(follow: false);
				}
				mOldCam = pPet.GetCamera();
				pPet.SetCamera(_Camera);
				pPet.SetState(Character_State.idle);
				pPet.SetPlayScale();
				pPet._Move2D = false;
				pPet._ActionDoneMessageObject = _MenuList[0].gameObject;
				if (SanctuaryManager.pInstance.pPetMeter != null)
				{
					SanctuaryManager.pInstance.pPetMeter.DetachFromToolbar();
				}
			}
			if (MainStreetMMOClient.pInstance != null)
			{
				MainStreetMMOClient.pInstance.SetBusy(busy: true);
			}
			SetVisibility(inVisible: true);
			mInitialized = true;
			AvAvatar.SetActive(inActive: false);
			if (pPet != null)
			{
				pPet.SetLookAtObject(_Camera.transform, tween: true, Vector3.zero);
			}
			if (pPet != null)
			{
				SetHome(1, onGlass: false);
			}
			SetBlockedGlassDown();
			if (MissionManager.pInstance != null)
			{
				MissionManager.pInstance.SetTimedTaskUpdate(inState: true, inForceUpdate: true);
			}
			pPlayMenu = (KAUIPetPlaySelectMenu)_MenuList[0];
			pPlayMenu.PlayIntro();
		}
		if (pObjectInHand != null && KAUI._GlobalExclusiveUI == null && pObjectAttachToMouse)
		{
			Vector3 mousePosition = Input.mousePosition;
			if (mousePosition.x >= 0f && mousePosition.x < (float)Screen.width && mousePosition.y >= 0f && mousePosition.y < (float)Screen.height)
			{
				mousePosition.z = 0.5f;
				pObjectInHand.transform.position = _Camera.ScreenToWorldPoint(mousePosition);
			}
		}
	}

	private void SetBlockedGlassDown()
	{
	}

	public void OnMoneyUpdated()
	{
		mTxtCoin.SetText(Money.pGameCurrency.ToString());
		mTxtGem.SetText(Money.pCashCurrency.ToString());
	}

	public void ReAttachToy(PetActions inAction)
	{
		pObjectAttachToMouse = true;
		if (!pPet.IsActionAllowed(inAction))
		{
			DropObject(lookatcam: true);
		}
	}
}
