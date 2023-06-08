using UnityEngine;

public class ObClickableShowPict : ObClickable
{
	public LocaleString _MessageText;

	public LocaleString _NoTextOrImageText;

	public Texture _Picture;

	public Vector2 _Position;

	public Vector2 _Size;

	public int _Priority;

	public AudioClip _Sound;

	public string _PrefabName = "PfUiGenericDB";

	private GameObject mUiGenericDBAsset;

	private GameObject mUiGenericDB;

	private SnChannel mChannel;

	public override void OnActivate()
	{
		if (!AvAvatar.pToolbar.activeInHierarchy)
		{
			return;
		}
		base.OnActivate();
		KAUIGenericDB kAUIGenericDB = CreateDialog();
		if (kAUIGenericDB != null)
		{
			AvAvatar.pState = AvAvatarState.PAUSED;
			AvAvatar.SetUIActive(inActive: false);
			if (!string.IsNullOrEmpty(_MessageText._Text))
			{
				kAUIGenericDB.SetTextByID(_MessageText._ID, _MessageText._Text, interactive: true);
			}
			else if (!(_Picture != null))
			{
				kAUIGenericDB.SetTextByID(_NoTextOrImageText._ID, _NoTextOrImageText._Text, interactive: false);
			}
		}
		if (_Sound != null)
		{
			mChannel = SnChannel.Play(_Sound, "VO_Pool", inForce: true);
		}
	}

	protected virtual KAUIGenericDB CreateDialog()
	{
		if (mUiGenericDBAsset == null)
		{
			mUiGenericDBAsset = (GameObject)RsResourceManager.LoadAssetFromResources(_PrefabName);
		}
		mUiGenericDB = Object.Instantiate(mUiGenericDBAsset);
		KAUIGenericDB obj = (KAUIGenericDB)mUiGenericDB.GetComponent("KAUIGenericDB");
		obj._MessageObject = base.gameObject;
		obj._CloseMessage = "OnClose";
		obj._TextMessage = "OnTextClick";
		obj.SetButtonVisibility(inYesBtn: false, inNoBtn: false, inOKBtn: false, inCloseBtn: true);
		return obj;
	}

	public void OnTextClick()
	{
		if (_Sound != null)
		{
			mChannel = SnChannel.Play(_Sound, "VO_Pool", inForce: true);
		}
	}

	public void OnClose()
	{
		mUiGenericDB.SetActive(value: false);
		Object.Destroy(mUiGenericDB);
		AvAvatar.pState = AvAvatarState.IDLE;
		AvAvatar.SetUIActive(inActive: true);
		if (mChannel != null)
		{
			mChannel.Stop();
		}
	}

	public virtual void SetMessageText(string inText)
	{
		_MessageText._Text = inText;
	}

	public virtual void SetMessageText(LocaleString inText)
	{
		_MessageText = inText;
	}
}
