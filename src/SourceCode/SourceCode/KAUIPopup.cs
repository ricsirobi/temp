using System;
using UnityEngine;

[Serializable]
public class KAUIPopup : KAUI
{
	public string _ParentInterfaceName = "KAUiStoreSyncPopUp";

	public string _OpenBtnName = "";

	public string _CloseBtnName = "";

	public bool _UseMask;

	public Color _MaskColor = new Color(0.5f, 0.5f, 0.5f, 0.5f);

	private KAUI mParentInterface;

	protected bool mHasGameObject;

	[NonSerialized]
	public KAWidget mOpenItem;

	[NonSerialized]
	protected KAWidget mCloseItem;

	public KAUIPopup()
	{
	}

	public KAUIPopup(KAUI parentInt, KAWidget openItem, KAWidget closeItem, Texture bkTexture, int priority)
	{
		Initialize(parentInt, openItem, closeItem, bkTexture, priority);
	}

	public virtual void Initialize(KAUI parentInt, KAWidget openItem, KAWidget closeItem, Texture bkTexture, int priority)
	{
		mOpenItem = openItem;
		mCloseItem = closeItem;
		mParentInterface = parentInt;
		_ = mParentInterface != null;
	}

	public void Initialize(KAUI parentInt)
	{
		Initialize(parentInt, (parentInt != null) ? parentInt.FindItem(_OpenBtnName) : null, FindItem(_CloseBtnName), null, GetPriority());
	}

	protected override void Start()
	{
		KAUI parentInt = (KAUI)base.gameObject.GetComponent(_ParentInterfaceName);
		Initialize(parentInt);
		mHasGameObject = true;
		base.Start();
	}

	public override void SetVisibility(bool t)
	{
		base.SetVisibility(t);
		if (t)
		{
			KAUI.SetExclusive(this);
		}
		else
		{
			KAUI.RemoveExclusive(this);
		}
		if (mParentInterface != null)
		{
			if (_UseMask)
			{
				mParentInterface.SetState(t ? KAUIState.NOT_INTERACTIVE : KAUIState.INTERACTIVE);
			}
			else
			{
				mParentInterface.SetState(t ? KAUIState.DISABLED : KAUIState.INTERACTIVE);
			}
		}
	}

	public override void OnClick(KAWidget inWidget)
	{
		if (inWidget == mCloseItem)
		{
			SetVisibility(t: false);
		}
		base.OnClick(inWidget);
	}

	public void OnGUI()
	{
		GetVisibility();
	}
}
