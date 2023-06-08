using UnityEngine;

public class UiNotification : KAUI
{
	public static UiNotification pInstance;

	public GameObject _MessageObject;

	public float _ExpireTime = 10f;

	public KAWidget _CloseBtn;

	public KAWidget _TxtAchievement;

	private float mExpireTime;

	protected override void Start()
	{
		base.Start();
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
	}

	public static void Show(string msg)
	{
		Show(msg, "PfUiNotification");
	}

	public static void ShowBig(string msg)
	{
		Show(msg, "PfUiNotificationBig");
	}

	private static bool Show(string msg, string PrefabName)
	{
		if (pInstance != null)
		{
			pInstance.CloseUI();
		}
		GameObject gameObject = (GameObject)RsResourceManager.LoadAssetFromResources(PrefabName);
		if (gameObject == null)
		{
			return false;
		}
		UiNotification uiNotification = (pInstance = Object.Instantiate(gameObject).GetComponent<UiNotification>());
		uiNotification.SetText(msg);
		uiNotification.SetPosition(400f, 0f);
		uiNotification.SlideIn();
		return true;
	}

	public void SetText(string text)
	{
		_TxtAchievement.SetInteractive(isInteractive: true);
		_TxtAchievement.SetText(text);
	}

	public void SetPosition(float posX, float posY)
	{
		base.transform.position += new Vector3(posX, posY, 0f);
	}

	public string GetText()
	{
		return _TxtAchievement.GetText();
	}

	public void DisplayIcon(string name)
	{
		KAWidget kAWidget = FindItem(name);
		if (kAWidget != null)
		{
			kAWidget.SetVisibility(inVisible: true);
		}
	}

	public void CloseUI()
	{
		pInstance = null;
		Object.Destroy(base.gameObject);
		if (_MessageObject != null)
		{
			_MessageObject.SendMessage("OnAlertClose", this, SendMessageOptions.DontRequireReceiver);
		}
	}

	protected override void Update()
	{
		base.Update();
		if (RsResourceManager.pLevelLoadingScreen)
		{
			CloseUI();
		}
		if (!AvAvatar.pInputEnabled || AvAvatar.pState == AvAvatarState.PAUSED || (AvAvatar.pToolbar != null && !AvAvatar.pToolbar.activeInHierarchy))
		{
			SetVisibility(inVisible: false);
		}
		else
		{
			SetVisibility(inVisible: true);
			mExpireTime += Time.deltaTime;
		}
		if (mExpireTime >= _ExpireTime)
		{
			CloseUI();
		}
	}

	public override void OnClick(KAWidget inWidget)
	{
		base.OnClick(inWidget);
		if (inWidget == _CloseBtn)
		{
			Input.ResetInputAxes();
			CloseUI();
		}
	}
}
