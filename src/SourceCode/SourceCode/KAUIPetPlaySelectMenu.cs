using System;
using System.Collections.Generic;
using UnityEngine;

public class KAUIPetPlaySelectMenu : KAUISelectMenu
{
	protected KAUIPetPlaySelect mPetUI;

	private ItemPrefabResData m3DData = new ItemPrefabResData();

	private ItemTextureResData mTexData = new ItemTextureResData();

	private bool mLoadingData;

	protected KAWidget mLoadingItem;

	public int[] _CategoryIDs = new int[2] { 276, 327 };

	public List<PetFoodItems> _PetFoodList;

	[NonSerialized]
	public PetToy mToy;

	public float _BallInHandScale = 1f;

	public float _FrisbeeInHandScale = 0.5f;

	public AudioClip _Morph;

	public AudioClip _TooTired;

	public AudioClip _TooLittle;

	public AudioClip _TooBig;

	public AudioClip _Burp;

	public float _DropToyTime = 10f;

	private float mDropToyTimer;

	private bool mInitInvItems;

	private bool mUpdatedInv;

	public override void Initialize(KAUI parentInt)
	{
		mCategoryIDs = _CategoryIDs;
		mPetUI = (KAUIPetPlaySelect)_ParentUi;
		mPetUI.pPlayMenu = this;
		base.Initialize(_ParentUi);
		EnableFidgetAnimation(enable: false);
	}

	private void InventorySaveEventHandler(bool inSuccess, object inUserData)
	{
		mUpdatedInv = true;
	}

	protected override void Update()
	{
		base.Update();
		if (!mInitInvItems)
		{
			if (!TutorialManager.HasTutorialPlayed("InitFoodItems"))
			{
				if (CommonInventoryData.pIsReady)
				{
					mInitInvItems = true;
					if (_PetFoodList.Count > 0)
					{
						TutorialManager.MarkTutorialDone("InitFoodItems");
						foreach (PetFoodItems petFood in _PetFoodList)
						{
							CommonInventoryData.pInstance.AddItem(petFood.ItemID, petFood.Quantity);
						}
						CommonInventoryData.pInstance.Save(InventorySaveEventHandler, null);
					}
					else
					{
						mUpdatedInv = true;
					}
				}
			}
			else
			{
				mInitInvItems = true;
				mUpdatedInv = true;
			}
		}
		if (mLoadingData && m3DData.IsDataLoaded() && mTexData.IsDataLoaded())
		{
			mLoadingData = false;
			mPetUI.SetState(KAUIState.INTERACTIVE);
			On3DdataReady();
		}
		if (!(mPetUI.pObjectInHand != null) || (!mPetUI.pObjectAttachToMouse && !_RemoveWhenSelected))
		{
			return;
		}
		PetToyType pObjectinHandType = mPetUI.pObjectinHandType;
		if ((pObjectinHandType == PetToyType.BRUSH || pObjectinHandType == PetToyType.CHEWTOY || pObjectinHandType == PetToyType.FOOD) && mDropToyTimer > 0f)
		{
			mDropToyTimer -= Time.deltaTime;
			if (mDropToyTimer <= 0f)
			{
				mPetUI.DropObject(lookatcam: true);
			}
		}
		if ((((!UtPlatform.IsMobile() && !UtPlatform.IsWSA()) || !(GetClickedItem() == null)) && (!UtPlatform.IsStandAlone() || !(KAUI.GetGlobalMouseOverItem() == null))) || !Input.GetMouseButtonUp(0) || !(Input.mousePosition.y >= 0f) || !(Input.mousePosition.y <= (float)Screen.height) || !(Input.mousePosition.x >= 0f) || !(Input.mousePosition.x <= (float)Screen.width) || CanThrowToy(mPetUI.pObjectinHandType) || mPetUI.pObjectinHandType == PetToyType.LASER)
		{
			return;
		}
		Ray ray = mPetUI._Camera.ScreenPointToRay(Input.mousePosition);
		if (mPetUI.pPet.collider.Raycast(ray, out var _, 300f))
		{
			ApplyObject();
			return;
		}
		if (mPetUI.pObjectinHandType == PetToyType.SOAP)
		{
			PetSoap component = mPetUI.pObjectInHand.GetComponent<PetSoap>();
			if (component != null && component._IsPetBrushed)
			{
				RemoveSelectedInventoryItem();
			}
		}
		else if (mPetUI.pObjectinHandType == PetToyType.BRUSH)
		{
			PetBrush component2 = mPetUI.pObjectInHand.GetComponent<PetBrush>();
			if (component2 != null && component2._IsPetBrushed)
			{
				mPetUI.pPet.UpdateActionMeters(PetActions.BRUSH, 1f, doUpdateSkill: true);
				RemoveSelectedInventoryItem();
			}
			mPetUI.pPet.AIActor.PlayCustomAnim("Gulp");
		}
		mPetUI.DropObject(lookatcam: true);
	}

	private bool CanThrowToy(PetToyType t)
	{
		if (t != PetToyType.BALL && t != PetToyType.FRISBEE)
		{
			return t == PetToyType.BOOMERANG;
		}
		return true;
	}

	protected PetToyType GetType(ItemData item)
	{
		return (PetToyType)Enum.Parse(typeof(PetToyType), item.GetAttribute("Type", PetToyType.UNKNOWN.ToString()), ignoreCase: true);
	}

	public void OnPettingStart()
	{
		if (mPetUI.pCurrentHomeIdx == 0 && mPetUI.pOnGlass)
		{
			mPetUI.SetHome(0, onGlass: false);
		}
		mPetUI.pNextHomeTimerPaused = true;
	}

	public void OnPettingEnd()
	{
		if (mPetUI.pCurrentHomeIdx == 0 && mPetUI.pOnGlass)
		{
			mPetUI.SetHome(0, onGlass: false);
		}
		mPetUI.pNextHomeTimerPaused = false;
	}

	public virtual void RemoveSelectedInventoryItem()
	{
		UtDebug.LogWarning("RemoveSelectedInventoryItem");
		if ((MyRoomsIntLevel.pInstance == null || !MyRoomsIntLevel.pInstance.IsOthersMyRoomsInt()) && mPetUI != null && mPetUI.pSelectedItem != null)
		{
			UtDebug.LogWarning("pSelectedItem");
			KAUISelectItemData kAUISelectItemData = (KAUISelectItemData)mPetUI.pSelectedItem.GetUserData();
			if (CommonInventoryData.pInstance.FindItem(kAUISelectItemData._ItemID) != null)
			{
				UtDebug.LogWarning("RemoveItem");
				CommonInventoryData.pInstance.RemoveItem(kAUISelectItemData._ItemID, updateServer: true);
				ReloadMenu();
			}
		}
	}

	public virtual void ApplyObject()
	{
		mPetUI.pPet.StopLookAtObject();
		mPetUI.pObjectAttachToMouse = false;
		mPetUI.pObjectInHand.transform.localScale = Vector3.one;
		switch (mPetUI.pObjectinHandType)
		{
		case PetToyType.FOOD:
			mPetUI.pPet.DoEatFood(mPetUI.pObjectInHand, FetchItemDataFromSelectedItem());
			break;
		case PetToyType.CHEWTOY:
			if (!mPetUI.pPet.DoChewToy(mPetUI.pObjectInHand))
			{
				mPetUI.DropObject(lookatcam: false);
			}
			break;
		}
		mPetUI.pObjectInHand = null;
	}

	private ItemData FetchItemDataFromSelectedItem()
	{
		return ((KAUISelectItemData)mPetUI.pSelectedItem.GetUserData())?._ItemData;
	}

	protected virtual void On3DdataReady()
	{
		KAUISelectItemData kAUISelectItemData = (KAUISelectItemData)mLoadingItem.GetUserData();
		PetToyType type = GetType(kAUISelectItemData._ItemData);
		PetActions action = GetAction(type);
		if (type == PetToyType.BALL || type == PetToyType.BOOMERANG || type == PetToyType.FRISBEE || type == PetToyType.LASER)
		{
			mPetUI.pPet.SetCanBePetted(t: false);
			if (!mPetUI.pPet.IsActionAllowed(action))
			{
				mPetUI.pPet.SetBrush(null);
				mPetUI.pPet.DoAction(null, Character_Action.userAction8);
				mPetUI.pPet.PlayAnim("Refuse", 0, 1f, 1);
				return;
			}
		}
		GameObject gameObject = UnityEngine.Object.Instantiate(m3DData._Prefab.gameObject);
		if (mTexData._Texture != null)
		{
			UtUtilities.SetObjectTexture(gameObject, 0, mTexData._Texture);
		}
		Collider component = gameObject.GetComponent<Collider>();
		if (component != null)
		{
			Physics.IgnoreCollision(mPetUI.pPet.collider, component);
		}
		mPetUI.pPet.SetTOW(null);
		mPetUI.pPet.SetBrush(null);
		switch (type)
		{
		case PetToyType.FOOD:
			mPetUI.pPet.SetCanBePetted(t: false);
			mPetUI.HoldObject(gameObject, type, attach: true);
			gameObject.transform.localScale = new Vector3(0.25f, 0.25f, 0.25f);
			if (mPetUI.pCurrentHomeIdx == 0 && mPetUI.pOnGlass)
			{
				mPetUI.SetHome(0, onGlass: false);
			}
			mDropToyTimer = _DropToyTime;
			break;
		case PetToyType.BRUSH:
			mPetUI.pPet.SetCanBePetted(t: true);
			mPetUI.pPet.EnablePetting(t: true);
			mPetUI.HoldObject(gameObject, type, attach: true);
			gameObject.transform.localScale = Vector3.one * 0.3f;
			gameObject.transform.rotation = mPetUI._Camera.transform.rotation;
			gameObject.transform.Rotate(-20f, -70f, 20f);
			mPetUI.pPet.SetBrush(gameObject.transform);
			gameObject.GetComponent<PetBrush>().Initialize(mPetUI);
			if (mPetUI._AllowMovement && (mPetUI.pCurrentHomeIdx != 0 || mPetUI.pOnGlass))
			{
				mPetUI.SetHome(0, onGlass: true);
			}
			break;
		case PetToyType.BALL:
		case PetToyType.LASER:
		case PetToyType.FRISBEE:
		case PetToyType.BOOMERANG:
			mPetUI.pPet.SetCanBePetted(t: false);
			mPetUI.HoldObject(gameObject, type, attach: true);
			mToy = gameObject.GetComponent<PetToy>();
			mToy.Initialize(mPetUI);
			mToy.transform.localScale = ((type == PetToyType.BALL) ? (Vector3.one * _BallInHandScale) : new Vector3(0.2f, 0.2f, 0.2f));
			mPetUI.SetHome(0, onGlass: true);
			mPetUI.pNextHomeTimerPaused = true;
			break;
		case PetToyType.CHEWTOY:
		case PetToyType.SOAP:
		case PetToyType.SPACETOY:
			break;
		}
	}

	private PetActions GetAction(PetToyType t)
	{
		return t switch
		{
			PetToyType.BALL => PetActions.FETCHBALL, 
			PetToyType.LASER => PetActions.FOLLOWLASER, 
			PetToyType.BRUSH => PetActions.BRUSH, 
			_ => PetActions.UNKNOWN, 
		};
	}

	public override void OnClick(KAWidget item)
	{
		base.OnClick(item);
		KAUISelectItemData kAUISelectItemData = (KAUISelectItemData)item.GetUserData();
		PetToyType type = GetType(kAUISelectItemData._ItemData);
		if (type == PetToyType.BALL || type == PetToyType.LASER)
		{
			_LockedVO = _Morph;
		}
		PetActions action = GetAction(type);
		if (!mPetUI.pPet.IsActionAllowed(action) && (MyRoomsIntLevel.pInstance == null || !MyRoomsIntLevel.pInstance.IsOthersMyRoomsInt()))
		{
			if (mPetUI.pObjectInHand != null)
			{
				mPetUI.DropObject(lookatcam: false);
			}
			if (_TooTired != null)
			{
				SnChannel.Play(_TooTired, "VO_Pool", inForce: true, null);
			}
		}
	}

	public override void SelectItem(KAWidget item)
	{
		if (mPetUI.pIsAIBundleReady)
		{
			if (mPetUI.pObjectInHand != null)
			{
				mPetUI.DropObject(lookatcam: false);
			}
			mPetUI.pNextHomeTimerPaused = true;
			KAUISelectItemData kAUISelectItemData = (KAUISelectItemData)item.GetUserData();
			m3DData.Init(kAUISelectItemData._PrefResName);
			mTexData.Init(kAUISelectItemData._TextResName);
			m3DData.LoadData();
			mTexData.LoadData();
			mLoadingData = true;
			mLoadingItem = item;
			mPetUI.pSelectedItem = item;
			if (_RemoveWhenSelected)
			{
				RemoveWidget(item);
				mNeedPageCheck = true;
				mPetUI.AddWidget(item);
				item.SetInteractive(isInteractive: false);
				item.AttachToCursor(0f, 0f);
			}
		}
	}

	public virtual void OnDisable()
	{
		if (_RemoveWhenSelected)
		{
			ReloadMenu();
		}
		EnableFidgetAnimation(enable: true);
	}

	public void UnlockTOWItem()
	{
		for (int i = 0; i < GetNumItems(); i++)
		{
			KAWidget kAWidget = FindItemAt(i);
			KAUISelectItemData kAUISelectItemData = (KAUISelectItemData)kAWidget.GetUserData();
			if (GetType(kAUISelectItemData._ItemData) == PetToyType.TOW)
			{
				kAUISelectItemData._Locked = false;
				KAWidget kAWidget2 = kAWidget.FindChildItemAt(0);
				if (kAWidget2 != null)
				{
					kAWidget.RemoveChildItem(kAWidget2);
				}
			}
		}
	}

	public override void ChangeCategory(int c, bool forceChange)
	{
		mPetUI.pSelectedItem = null;
		base.ChangeCategory(c, forceChange);
	}

	protected void EnableFidgetAnimation(bool enable)
	{
		if (mPetUI != null && mPetUI.pPet != null)
		{
			mPetUI.pPet.SetFidgetOnOff(enable);
		}
	}

	private void OnEnable()
	{
		ReloadMenu();
		EnableFidgetAnimation(enable: false);
	}

	private void LockItem(KAUISelectItemData itemData)
	{
		RaisedPetStage raisedPetStage = RaisedPetStage.NONE;
		if (mPetUI.pPet != null)
		{
			raisedPetStage = mPetUI.pPet.pData.pStage;
		}
		switch (GetType(itemData._ItemData))
		{
		case PetToyType.BALL:
			itemData._Locked = raisedPetStage < RaisedPetStage.BABY;
			break;
		case PetToyType.LASER:
			itemData._Locked = raisedPetStage < RaisedPetStage.BABY;
			break;
		}
	}

	public override bool IsCurrentDataReady()
	{
		return mUpdatedInv;
	}
}
