using System;
using System.Collections.Generic;
using UnityEngine;

public class UiTitanInfo : KAUI
{
	[Serializable]
	public class TitanPetSprite
	{
		public int _TypeID;

		public Texture _Sprite;
	}

	public delegate void OnTitanInfoHandled();

	public TitanPetSprite[] _TitanPetSprites;

	public KAUIMenu _TitanInfoMenu;

	private KAWidget mBtnClose;

	private KAWidget mIconDragon;

	private OnTitanInfoHandled mCallback;

	private RaisedPetStage mPetStage = RaisedPetStage.TITAN;

	public OnTitanInfoHandled pCallback
	{
		set
		{
			mCallback = value;
		}
	}

	protected override void Start()
	{
		base.Start();
		KAUI.SetExclusive(this);
		mPetStage = RaisedPetStage.TITAN;
		mBtnClose = FindItem("BtnClose");
		mIconDragon = FindItem("IcoDragonPic");
		if (mIconDragon != null && SanctuaryManager.pCurPetInstance != null)
		{
			SanctuaryManager.pInstance.TakePicture(SanctuaryManager.pCurPetInstance.gameObject, base.gameObject, inSendPicture: false);
		}
		PopulateAvailableTitans();
	}

	private void PopulateAvailableTitans()
	{
		int ageIndex = RaisedPetData.GetAgeIndex(mPetStage);
		List<SanctuaryPetTypeInfo> list = new List<SanctuaryPetTypeInfo>();
		SanctuaryPetTypeInfo[] petTypes = SanctuaryData.pInstance._PetTypes;
		foreach (SanctuaryPetTypeInfo sanctuaryPetTypeInfo in petTypes)
		{
			if (sanctuaryPetTypeInfo._AgeData.Length == ageIndex + 1)
			{
				list.Add(sanctuaryPetTypeInfo);
			}
		}
		if (list.Count <= 0 || !(_TitanInfoMenu != null))
		{
			return;
		}
		foreach (SanctuaryPetTypeInfo petType in list)
		{
			KAWidget kAWidget = _TitanInfoMenu.AddWidget(_TitanInfoMenu._Template.name);
			kAWidget.SetText(petType._NameText.GetLocalizedString());
			KAWidget kAWidget2 = kAWidget.FindChildItem("IcoDragonPic");
			if (kAWidget2 != null)
			{
				UITexture uITexture = kAWidget2.GetUITexture();
				TitanPetSprite titanPetSprite = Array.Find(_TitanPetSprites, (TitanPetSprite x) => x._TypeID == petType._TypeID);
				if (uITexture != null && titanPetSprite != null && titanPetSprite._Sprite != null)
				{
					uITexture.mainTexture = titanPetSprite._Sprite;
				}
			}
		}
	}

	private void OnPetPictureDone(object inImage)
	{
		if (inImage != null && mIconDragon != null)
		{
			mIconDragon.SetTexture(inImage as Texture);
		}
	}

	public override void OnClick(KAWidget inItem)
	{
		base.OnClick(inItem);
		if (inItem == mBtnClose)
		{
			if (mCallback != null)
			{
				mCallback();
			}
			KAUI.RemoveExclusive(this);
			UnityEngine.Object.Destroy(base.gameObject);
		}
	}
}
