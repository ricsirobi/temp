using UnityEngine;

public class UiClansCrestSelector : KAUI
{
	public int _DefaultStoreID = 104;

	public UiClansCrestSelectorMenu _Menu;

	public UITexture _CrestLogo;

	public UITexture _CrestBackground;

	private StoreData mStoreData;

	private KAWidget mCrest;

	private KAWidget mCrestFGColorBtn;

	private KAWidget mCrestBGColorBtn;

	private KAWidget mColorPalette;

	private KAWidget mColorSelector;

	private KAWidget mSelectedColorBtn;

	private KAWidget mOkBtn;

	private ClansCrestInfo mCrestInfo;

	private ClansCrestInfo mTempCrestInfo = new ClansCrestInfo();

	private bool mInitialized;

	private int mDefaultCrestIndex;

	public ClansCrestInfo pCrestInfo
	{
		get
		{
			return mCrestInfo;
		}
		set
		{
			mCrestInfo = value;
		}
	}

	protected override void Start()
	{
		mCrest = FindItem("ClanCrestTemplate");
		mCrestFGColorBtn = FindItem("CrestFGColorBtn");
		mCrestBGColorBtn = FindItem("CrestBGColorBtn");
		mColorPalette = FindItem("ColorPicker");
		mColorSelector = FindItem("ColorSelector");
		mOkBtn = FindItem("BtnOk");
		if (mCrestInfo == null)
		{
			mCrestInfo = new ClansCrestInfo();
			if (_CrestLogo != null)
			{
				mTempCrestInfo.ColorFG = (mCrestInfo.ColorFG = _CrestLogo.color);
			}
			if (_CrestBackground != null)
			{
				mTempCrestInfo.ColorBG = (mCrestInfo.ColorBG = _CrestBackground.color);
			}
		}
		else
		{
			mInitialized = true;
			if (_CrestLogo != null)
			{
				_CrestLogo.mainTexture = mCrestInfo.CrestIcon;
				_CrestLogo.color = mCrestInfo.ColorFG;
			}
			if (_CrestBackground != null)
			{
				_CrestBackground.color = mCrestInfo.ColorBG;
			}
			mTempCrestInfo.Copy(mCrestInfo);
		}
		mCrestFGColorBtn.pBackground.color = mCrestInfo.ColorFG;
		mCrestBGColorBtn.pBackground.color = mCrestInfo.ColorBG;
		base.Start();
		KAUICursorManager.SetDefaultCursor("Loading");
		SetInteractive(interactive: false);
		ItemStoreDataLoader.Load(_DefaultStoreID, OnStoreLoaded);
	}

	public void OnStoreLoaded(StoreData sd)
	{
		mStoreData = sd;
		PopulateMenu();
		if (mInitialized)
		{
			mCrest.SetVisibility(inVisible: true);
			KAUICursorManager.SetDefaultCursor("Arrow");
			SetInteractive(interactive: true);
		}
	}

	protected override void Update()
	{
		base.Update();
		if (mInitialized || _Menu.GetItemCount() <= 0)
		{
			return;
		}
		if (mDefaultCrestIndex < _Menu.GetItemCount())
		{
			KAWidget itemAt = _Menu.GetItemAt(mDefaultCrestIndex);
			if (itemAt != null)
			{
				ClansCrestSelectorData clansCrestSelectorData = (ClansCrestSelectorData)itemAt.GetUserData();
				if (clansCrestSelectorData != null)
				{
					if (clansCrestSelectorData.pIsReady)
					{
						Texture texture = clansCrestSelectorData.GetTexture();
						if (texture != null)
						{
							mInitialized = true;
							mTempCrestInfo.Logo = (mCrestInfo.Logo = clansCrestSelectorData._CrestUrl);
							mTempCrestInfo.CrestIcon = (mCrestInfo.CrestIcon = texture);
							if (_CrestLogo != null)
							{
								_CrestLogo.mainTexture = texture;
							}
						}
						else
						{
							mDefaultCrestIndex++;
						}
					}
				}
				else
				{
					mDefaultCrestIndex++;
				}
			}
			else
			{
				mDefaultCrestIndex++;
			}
		}
		else
		{
			mInitialized = true;
		}
		if (mInitialized)
		{
			mCrest.SetVisibility(inVisible: true);
			KAUICursorManager.SetDefaultCursor("Arrow");
			SetInteractive(interactive: true);
		}
	}

	private void OnDisable()
	{
		mOkBtn.SetDisabled(isDisabled: true);
		mTempCrestInfo.Copy(mCrestInfo);
		if (_CrestLogo != null)
		{
			_CrestLogo.mainTexture = mTempCrestInfo.CrestIcon;
			_CrestLogo.color = mTempCrestInfo.ColorFG;
		}
		if (_CrestBackground != null)
		{
			_CrestBackground.color = mTempCrestInfo.ColorBG;
		}
	}

	public override void SetVisibility(bool inVisible)
	{
		base.SetVisibility(inVisible);
		if (inVisible && mCrestFGColorBtn != null)
		{
			OnClick(mCrestFGColorBtn);
		}
	}

	private void PopulateMenu()
	{
		ItemData[] items = mStoreData._Items;
		foreach (ItemData obj in items)
		{
			KAWidget kAWidget = DuplicateWidget(_Menu._Template);
			kAWidget.SetVisibility(inVisible: true);
			_Menu.AddWidget(kAWidget);
			ClansCrestSelectorData userData = new ClansCrestSelectorData(obj.IconName);
			kAWidget.SetUserData(userData);
		}
	}

	public override void OnPressRepeated(KAWidget inWidget, bool inPressed)
	{
		base.OnPressRepeated(inWidget, inPressed);
		if (inPressed && !(mSelectedColorBtn == null) && inWidget == mColorPalette)
		{
			GameObject gameObject = inWidget.transform.Find("Background").gameObject;
			Vector2 vector = new Vector2(Input.mousePosition.x, Input.mousePosition.y);
			Vector3 vector2 = new Vector3(gameObject.GetComponent<UITexture>().localSize.x, gameObject.GetComponent<UITexture>().localSize.y, 1f);
			Vector3 position = gameObject.transform.position - vector2 * 0.5f;
			Vector3 position2 = gameObject.transform.position + vector2 * 0.5f;
			position = UICamera.currentCamera.WorldToScreenPoint(position);
			position2 = UICamera.currentCamera.WorldToScreenPoint(position2);
			float num = (vector.x - position.x) / (position2.x - position.x);
			float num2 = (vector.y - position.y) / (position2.y - position.y);
			Vector3 vector3;
			if (UtPlatform.IsMobile())
			{
				num = Mathf.Min(Mathf.Max(0f, num), 1f);
				num2 = Mathf.Min(Mathf.Max(0f, num2), 1f);
				vector3 = UICamera.currentCamera.ScreenToWorldPoint(new Vector3(position.x + num * (position2.x - position.x), position.y + num2 * (position2.y - position.y), 0f));
			}
			else
			{
				vector3 = UICamera.currentCamera.ScreenToWorldPoint(new Vector3(KAInput.mousePosition.x, KAInput.mousePosition.y, 0f));
			}
			mColorSelector.transform.position = new Vector3(vector3.x, vector3.y, mColorSelector.transform.position.z);
			Color pixelBilinear = ((Texture2D)inWidget.GetTexture()).GetPixelBilinear(num, num2);
			mSelectedColorBtn.pBackground.color = pixelBilinear;
			ApplyColor(pixelBilinear);
		}
	}

	public override void OnClick(KAWidget inWidget)
	{
		base.OnClick(inWidget);
		if (inWidget == mCrestFGColorBtn || inWidget == mCrestBGColorBtn)
		{
			mSelectedColorBtn = inWidget;
		}
		else if (inWidget == mOkBtn)
		{
			mOkBtn.SetDisabled(isDisabled: true);
			mCrestInfo.Copy(mTempCrestInfo);
			UiClans.pInstance._UiClansCreate.UpdateCrest(mCrestInfo);
			UiClans.pInstance._UiClansCreate.ShowCrestSelector(show: false);
		}
	}

	public void ApplyColor(Color inColor)
	{
		OnCrestModified();
		if (mSelectedColorBtn == mCrestFGColorBtn)
		{
			mTempCrestInfo.ColorFG = inColor;
			if (_CrestLogo != null)
			{
				_CrestLogo.color = inColor;
			}
		}
		else
		{
			mTempCrestInfo.ColorBG = inColor;
			if (_CrestBackground != null)
			{
				_CrestBackground.color = inColor;
			}
		}
	}

	public void OnCrestSelected(KAWidget inWidget)
	{
		if (!(inWidget != null))
		{
			return;
		}
		ClansCrestSelectorData clansCrestSelectorData = (ClansCrestSelectorData)inWidget.GetUserData();
		if (clansCrestSelectorData != null)
		{
			OnCrestModified();
			mTempCrestInfo.Logo = clansCrestSelectorData._CrestUrl;
			mTempCrestInfo.CrestIcon = clansCrestSelectorData.GetTexture();
			if (_CrestLogo != null)
			{
				_CrestLogo.mainTexture = clansCrestSelectorData.GetTexture();
			}
		}
	}

	private void OnCrestModified()
	{
		mOkBtn.SetDisabled(isDisabled: false);
	}
}
