using UnityEngine;

public class UiChooseADragon : KAUI
{
	public int _ToothlessItemID;

	public KAWidget _ToothlessWidget;

	public KAWidget _PlayerDragon;

	protected KAWidget mBackBtn;

	protected KAWidget mCloseBtn;

	protected bool mSetPlayerDragonData;

	private AvPhotoManager mPhotoManager;

	protected override void Start()
	{
		base.Start();
		mBackBtn = FindItem("BackBtn");
		mCloseBtn = FindItem("BtnDWDragonsClose");
		HeroPetData heroDragonFromID = SanctuaryData.GetHeroDragonFromID(_ToothlessItemID);
		AddDragonDetails(_ToothlessWidget, heroDragonFromID);
	}

	protected void SetPlayerDragonData()
	{
		if (SanctuaryManager.pCurPetInstance != null)
		{
			SanctuaryManager.pInstance.TakePicture(SanctuaryManager.pCurPetInstance.gameObject, base.gameObject, inSendPicture: false);
			HeroPetData heroDragonFromID = SanctuaryData.GetHeroDragonFromID(0);
			heroDragonFromID._Name = SanctuaryManager.pCurPetInstance.pData.Name;
			heroDragonFromID._TypeID = SanctuaryManager.pCurPetInstance.pTypeInfo._TypeID;
			heroDragonFromID._DragonClass = SanctuaryManager.pCurPetInstance.pTypeInfo._DragonClass;
			heroDragonFromID._DragonType = SanctuaryManager.pCurPetInstance.pTypeInfo._NameText.GetLocalizedString();
			heroDragonFromID._Age = SanctuaryManager.pCurPetInstance.pData.pStage;
		}
	}

	protected virtual void OnPetPictureDone(object inImage)
	{
		if (inImage != null)
		{
			KAWidget kAWidget = _PlayerDragon.FindChildItem("IcoDragonFlightTexture");
			if (kAWidget != null)
			{
				kAWidget.SetTexture(inImage as Texture);
			}
		}
	}

	protected void AddDragonDetails(KAWidget inWidget, HeroPetData pData)
	{
		if (inWidget == null || pData == null)
		{
			return;
		}
		DragonClassInfo dragonClassInfo = SanctuaryData.GetDragonClassInfo(pData._DragonClass);
		KAWidget kAWidget = inWidget.FindChildItem("TxtDragonName");
		if (kAWidget != null)
		{
			kAWidget.SetText(pData._Name);
			kAWidget.SetVisibility(inVisible: true);
		}
		kAWidget = inWidget.FindChildItem("TxtDragonClass");
		if (kAWidget != null)
		{
			string text = dragonClassInfo._InfoText.GetLocalizedString();
			if (text.Contains("_"))
			{
				text = text.Replace('_', ' ');
			}
			kAWidget.SetText(text);
			kAWidget.SetVisibility(inVisible: true);
		}
		kAWidget = inWidget.FindChildItem("IcoDragonClass");
		if (kAWidget != null && !string.IsNullOrEmpty(dragonClassInfo._IconSprite))
		{
			kAWidget.pBackground.UpdateSprite(dragonClassInfo._IconSprite);
			kAWidget.SetVisibility(inVisible: true);
		}
		kAWidget = inWidget.FindChildItem("TxtDragonType");
		if (kAWidget != null)
		{
			kAWidget.SetText(pData._DragonType);
			kAWidget.SetVisibility(inVisible: true);
		}
		kAWidget = inWidget.FindChildItem("TxtStrengthVal");
		if (kAWidget != null)
		{
			kAWidget.SetText(pData._Strength);
			kAWidget.SetVisibility(inVisible: true);
		}
		kAWidget = inWidget.FindChildItem("TxtSpeedVal");
		if (kAWidget != null)
		{
			kAWidget.SetText(pData._Speed);
			kAWidget.SetVisibility(inVisible: true);
		}
		kAWidget = inWidget.FindChildItem("TxtEnduranceVal");
		if (kAWidget != null)
		{
			kAWidget.SetText(pData._Endurance);
			kAWidget.SetVisibility(inVisible: true);
		}
		kAWidget = inWidget.FindChildItem("TxtArmorVal");
		if (kAWidget != null)
		{
			kAWidget.SetText(pData._Armor);
			kAWidget.SetVisibility(inVisible: true);
		}
		kAWidget = inWidget.FindChildItem("TxtFireVal");
		if (kAWidget != null)
		{
			kAWidget.SetText(pData._Fire);
			kAWidget.SetVisibility(inVisible: true);
		}
		kAWidget = inWidget.FindChildItem("TxtShotLimitVal");
		if (kAWidget != null)
		{
			kAWidget.SetText(pData._ShotLimit);
			kAWidget.SetVisibility(inVisible: true);
		}
		kAWidget = inWidget.FindChildItem("TxtWingSpanVal");
		if (kAWidget != null)
		{
			kAWidget.SetText(pData._WingSpan);
			kAWidget.SetVisibility(inVisible: true);
		}
		kAWidget = inWidget.FindChildItem("TxtLengthVal");
		if (kAWidget != null)
		{
			kAWidget.SetText(pData._Length);
			kAWidget.SetVisibility(inVisible: true);
		}
		kAWidget = inWidget.FindChildItem("TxtWeightVal");
		if (kAWidget != null)
		{
			kAWidget.SetText(pData._Weight);
			kAWidget.SetVisibility(inVisible: true);
		}
		kAWidget = inWidget.FindChildItem("IcoRider");
		if (kAWidget != null && !string.IsNullOrEmpty(pData._RiderSpriteName))
		{
			kAWidget.pBackground.UpdateSprite(pData._RiderSpriteName);
			kAWidget.SetVisibility(inVisible: true);
		}
		kAWidget = inWidget.FindChildItem("IcoDragonFlightTexture");
		if (kAWidget != null && !string.IsNullOrEmpty(pData._DragonSpriteName))
		{
			kAWidget.pBackground.UpdateSprite(pData._DragonSpriteName);
			kAWidget.SetVisibility(inVisible: true);
		}
	}

	public override void SetVisibility(bool visible)
	{
		base.SetVisibility(visible);
		if (visible && !mSetPlayerDragonData)
		{
			SetPlayerDragonData();
			HeroPetData heroDragonFromID = SanctuaryData.GetHeroDragonFromID(0);
			AddDragonDetails(_PlayerDragon, heroDragonFromID);
			mSetPlayerDragonData = true;
		}
	}

	public override void OnClick(KAWidget item)
	{
		base.OnClick(item);
		if (item.pParentWidget == _ToothlessWidget)
		{
			GauntletRailShootManager.pInstance._GauntletController.LoadToothless(LoadGame);
		}
		else if (item.pParentWidget == _PlayerDragon)
		{
			GauntletRailShootManager.pInstance._GauntletController.LoadPet(SanctuaryManager.pCurPetData, LoadGame);
		}
		else if (item == mBackBtn || item == mCloseBtn)
		{
			SetVisibility(visible: false);
			GauntletRailShootManager.pInstance._LevelSelectionScreen.SetVisibility(visible: true);
		}
	}

	protected virtual void LoadGame()
	{
		GauntletRailShootManager.pInstance.SetGameType(GSGameType.TRAINING);
		SetVisibility(visible: false);
	}
}
