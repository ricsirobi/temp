using System;
using UnityEngine;

public class KAUIStoreCategory : KAUI
{
	private static KAUIStoreCategory mInstance = null;

	public static bool _SetDefaultMenuItem = true;

	public static string _EnterSelection = "";

	public static string _ExitToScene = "";

	public static string _ExitToSceneMarkerName = "";

	public static bool _ShowItemID = false;

	public AudioClip[] _BaseIdleVOs;

	public string _ExitMarkerName = "";

	public GameObject _Camera;

	public GUISkin _ToolTipSkin;

	[NonSerialized]
	public KAUIStoreBase _StoreUI;

	[NonSerialized]
	public KAUIStoreCategoryMenu _Menu;

	[NonSerialized]
	public int _CurPage;

	[NonSerialized]
	public CoIdleManager _IdleMgr;

	private KAWidget mUp;

	private KAWidget mHome;

	private KAWidget mBkg;

	public static KAUIStoreCategory pInstance => mInstance;

	private void OnEnable()
	{
	}

	protected override void Start()
	{
		mInstance = this;
		base.Start();
		mUp = FindItem("btnUp");
		mHome = FindItem("btnHome");
		mBkg = FindItem("bkgPDA");
		_Menu = base.transform.root.GetComponentInChildren<KAUIStoreCategoryMenu>();
		_IdleMgr = base.transform.root.GetComponentInChildren<CoIdleManager>();
	}

	protected override void Update()
	{
		base.Update();
		if (_SetDefaultMenuItem && _Menu.pIsInitialized)
		{
			_SetDefaultMenuItem = false;
			CreateStoreItemInterface();
		}
	}

	public void HideCurUI()
	{
		if (_StoreUI != null)
		{
			_StoreUI.enabled = false;
			_StoreUI.SetVisibility(inVisible: false);
			_StoreUI = null;
		}
	}

	public void CreateStoreItemInterface()
	{
		HideCurUI();
		KAUIStore componentInChildren = base.transform.parent.GetComponentInChildren<KAUIStore>();
		componentInChildren.enabled = true;
		_StoreUI = componentInChildren;
		_StoreUI.SetVisibility(inVisible: true);
		_StoreUI.LoadSelectedStore();
		componentInChildren.EnableStoreMenu(inEnable: true);
	}

	public override void OnClick(KAWidget item)
	{
		base.OnClick(item);
		if (item == mHome)
		{
			HideCurUI();
			_Menu.SetCategories(_Menu._MenuItemData);
			_Menu.SetVisibility(inVisible: true);
			_Menu.pLastSelectedItem = null;
		}
	}

	public void SetBackGroundTexture(Texture tex)
	{
		if (mBkg != null)
		{
			mBkg.SetTexture(tex);
		}
	}

	public void EnableBackButton(bool bEnable)
	{
		if (mUp != null)
		{
			mUp.SetVisibility(bEnable);
		}
	}

	public void EnableBackGround(bool bEnable)
	{
		if (mBkg != null)
		{
			mBkg.SetVisibility(bEnable);
		}
	}
}
