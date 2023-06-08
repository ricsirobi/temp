using UnityEngine;

public class UiActionsMenu : KAUIMenu
{
	private ActionsDisplay mSelected;

	private float mTimeElapsed;

	public AudioClip _LockedAudio;

	public AudioClip _NoRideAudio;

	public float _NextClickTime = 0.1f;

	public Vector2 _IconSize = new Vector2(32f, 32f);

	private KAWidget mMenuBkg;

	protected override void Start()
	{
		base.Start();
		mTimeElapsed = _NextClickTime;
		SetVisibility(inVisible: false);
	}

	public override void ClearItems()
	{
		base.ClearItems();
		SetVisibility(inVisible: false);
	}

	public void AddEmoticons()
	{
		if (EmoticonActionData._Bundle == null)
		{
			return;
		}
		mSelected = ActionsDisplay.EMOTICONS;
		ClearItems();
		EmoticonActionData.Emoticon[] emoticons = EmoticonActionData._EmoticonActionData.Emoticons;
		foreach (EmoticonActionData.Emoticon emoticon in emoticons)
		{
			Texture2D inTexture = (Texture2D)EmoticonActionData._Bundle.LoadAsset(emoticon.Icon);
			KAWidget kAWidget = AddWidget(typeof(KAButton), inTexture, Shader.Find("Unlit/Transparent Colored"), emoticon.ID.ToString());
			kAWidget.transform.parent = mCurrentGrid.transform;
			kAWidget.transform.localScale = new Vector3(_IconSize.x, _IconSize.y, 1f);
			kAWidget.GetComponent<BoxCollider>().size = new Vector3(kAWidget.GetUITexture().localSize.x, kAWidget.GetUITexture().localSize.y, 1f);
			UtUtilities.SetLayerRecursively(kAWidget.gameObject, mCurrentGrid.gameObject.layer);
			if (emoticon.Locked && !SubscriptionInfo.pIsMember)
			{
				KAWidget kAWidget2 = AddWidget("lock");
				kAWidget2.SetVisibility(inVisible: true);
				kAWidget.AddChild(kAWidget2);
			}
		}
		SetVisibility(inVisible: true);
	}

	public void AddActions()
	{
		if (EmoticonActionData._Bundle == null)
		{
			return;
		}
		mSelected = ActionsDisplay.ACTIONS;
		ClearItems();
		EmoticonActionData.Action[] actions = EmoticonActionData._EmoticonActionData.Actions;
		foreach (EmoticonActionData.Action action in actions)
		{
			Texture2D inTexture = (Texture2D)EmoticonActionData._Bundle.LoadAsset(action.Icon);
			KAWidget kAWidget = AddWidget(typeof(KAButton), inTexture, Shader.Find("Unlit/Transparent Colored"), action.ID.ToString());
			Vector3 localPosition = kAWidget.transform.localPosition;
			kAWidget.transform.parent = mCurrentGrid.transform;
			kAWidget.transform.localScale = new Vector3(_IconSize.x, _IconSize.x, 1f);
			kAWidget.GetComponent<BoxCollider>().size = new Vector3(kAWidget.GetUITexture().localSize.x, kAWidget.GetUITexture().localSize.y, 1f);
			UtUtilities.SetLayerRecursively(kAWidget.gameObject, mCurrentGrid.gameObject.layer);
			kAWidget.transform.localPosition = localPosition;
			if (action.Locked && !SubscriptionInfo.pIsMember)
			{
				KAWidget kAWidget2 = AddWidget("lock");
				kAWidget2.SetVisibility(inVisible: true);
				kAWidget.AddChild(kAWidget2);
			}
		}
		SetVisibility(inVisible: true);
	}

	protected override void Update()
	{
		base.Update();
		if (mTimeElapsed < _NextClickTime)
		{
			mTimeElapsed += Time.deltaTime;
		}
	}

	public override void OnClick(KAWidget item)
	{
		base.OnClick(item);
		int selectedItemIndex = GetSelectedItemIndex();
		if (mTimeElapsed >= _NextClickTime && selectedItemIndex >= 0)
		{
			if (mSelected.Equals(ActionsDisplay.EMOTICONS))
			{
				if (EmoticonActionData._EmoticonActionData.Emoticons[selectedItemIndex].Locked && !SubscriptionInfo.pIsMember)
				{
					SnChannel.Play(_LockedAudio, "VO_Pool", inForce: true);
					return;
				}
				mTimeElapsed = 0f;
				AvAvatar.pObject.SendMessage("OnEmote", int.Parse(item.name));
				if (MainStreetMMOClient.pInstance != null)
				{
					MainStreetMMOClient.pInstance.SendEmoticon(int.Parse(item.name));
				}
			}
			else if (mSelected.Equals(ActionsDisplay.ACTIONS))
			{
				if (AvAvatar.IsPlayerOnRide())
				{
					if (_NoRideAudio != null)
					{
						SnChannel.Play(_NoRideAudio, "VO_Pool", inForce: true);
					}
					return;
				}
				if (EmoticonActionData._EmoticonActionData.Actions[selectedItemIndex].Locked && !SubscriptionInfo.pIsMember)
				{
					SnChannel.Play(_LockedAudio, "VO_Pool", inForce: true);
					return;
				}
				mTimeElapsed = 0f;
				AvAvatar.pObject.SendMessage("OnAction", int.Parse(item.name));
				if (MainStreetMMOClient.pInstance != null)
				{
					MainStreetMMOClient.pInstance.SendAction(int.Parse(item.name));
				}
			}
		}
		SetSelectedItem(null);
	}
}
