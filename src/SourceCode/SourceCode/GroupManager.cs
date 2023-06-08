using UnityEngine;

public class GroupManager : MonoBehaviour
{
	public GameObject[] _Groups;

	public bool _NoCounter;

	public int _NumberRequired = -1;

	public Texture2D _CounterBkg;

	public Texture2D _CounterBkgCompleted;

	public KAUI _CounterInt;

	public int _CounterX = 400;

	public int _CounterY = 50;

	public GUISkin _CounterSkin;

	public AudioClip[] _FindPromptVO;

	public AudioClip[] _AllFoundVO;

	public GameObject _MessageObject;

	public int pCurGroup = -1;

	public int pNumCollected;

	public int pNumRequired;

	public void SetCounter(int num0)
	{
		if (_CounterInt == null)
		{
			_CounterInt = AvAvatar.pToolbar.GetComponent<UiToolbar>();
		}
		if (_CounterBkg == null || _NoCounter)
		{
			return;
		}
		int num = num0;
		if (num < 0)
		{
			num = 0;
		}
		KAWidget kAWidget = _CounterInt.FindItem(_CounterBkg.name);
		if (kAWidget == null)
		{
			kAWidget = _CounterInt.AddWidget(typeof(KAButton), _CounterBkg, Shader.Find("Unlit/Transparent Colored"), "Counter");
			kAWidget.SetPosition(_CounterX, _CounterY);
			kAWidget.SetState(KAUIState.NOT_INTERACTIVE);
			if (pNumRequired > 1)
			{
				KAEditBox kAEditBox = new GameObject(num.ToString()).AddComponent<KAEditBox>();
				kAEditBox.SetText(num.ToString());
				kAWidget.AddChild(kAEditBox);
			}
		}
		kAWidget.SetVisibility(inVisible: true);
		if (num == 0 && _CounterBkgCompleted != null)
		{
			kAWidget.SetTexture(_CounterBkgCompleted);
			((KAEditBox)kAWidget.FindChildItemAt(0)).SetVisibility(inVisible: false);
		}
		if (pNumRequired > 1)
		{
			((KAEditBox)kAWidget.FindChildItemAt(0)).SetText(num.ToString());
		}
	}

	public void RemoveCounter()
	{
		if (!(_CounterBkg == null) && !(_CounterInt == null))
		{
			KAWidget kAWidget = _CounterInt.FindItem(_CounterBkg.name);
			if (kAWidget != null)
			{
				kAWidget.SetVisibility(inVisible: false);
			}
		}
	}

	private int GetIndexFromName(string s)
	{
		string[] array = s.Split('_');
		if (array.Length >= 1)
		{
			return UtStringUtil.Parse(array[^1], -1);
		}
		return -1;
	}

	private void Start()
	{
		Component[] componentsInChildren = base.gameObject.GetComponentsInChildren(typeof(Transform));
		int num = 0;
		Component[] array = componentsInChildren;
		for (int i = 0; i < array.Length; i++)
		{
			if (((Transform)array[i]).parent == base.transform)
			{
				num++;
			}
		}
		_Groups = new GameObject[num];
		int num2 = 0;
		array = componentsInChildren;
		for (int i = 0; i < array.Length; i++)
		{
			Transform transform = (Transform)array[i];
			if (transform.parent == base.transform)
			{
				int indexFromName = GetIndexFromName(transform.name);
				if (indexFromName >= 0 && indexFromName < num)
				{
					_Groups[indexFromName] = transform.gameObject;
				}
				else
				{
					_Groups[num2] = transform.gameObject;
				}
				num2++;
			}
		}
	}

	public void SetCollectObjRegen(float t)
	{
		Component[] componentsInChildren = base.gameObject.GetComponentsInChildren(typeof(ObCollect));
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			((ObCollect)componentsInChildren[i])._RegenTime = t;
		}
	}

	private void SetCollectMsgObj(GameObject obj)
	{
		Component[] componentsInChildren = obj.GetComponentsInChildren(typeof(ObCollect));
		foreach (Component obj2 in componentsInChildren)
		{
			((ObCollect)obj2)._MessageObject = base.gameObject;
			((ObCollect)obj2).OnSetMessageOnly(flag: true);
		}
	}

	public virtual bool IsActiveLastGroup()
	{
		if (pCurGroup >= 0)
		{
			return pCurGroup == _Groups.Length - 1;
		}
		return false;
	}

	public virtual void LoadGroup(int idx, GameObject messageObject)
	{
		_MessageObject = messageObject;
		UnloadGroup();
		pCurGroup = idx;
		pNumCollected = 0;
		if (pCurGroup < 0)
		{
			pCurGroup = Random.Range(0, _Groups.Length);
		}
		_Groups[pCurGroup].SetActive(value: true);
		UtDebug.Log("Group " + base.name + " load group " + _Groups[pCurGroup].name);
		if (_NumberRequired < 0)
		{
			pNumRequired = _Groups[pCurGroup].transform.childCount;
		}
		else
		{
			pNumRequired = _NumberRequired;
		}
		SetCounter(pNumRequired);
		SetCollectMsgObj(_Groups[pCurGroup]);
	}

	public void UnloadGroup()
	{
		if (pCurGroup >= 0)
		{
			_Groups[pCurGroup].SetActive(value: false);
			pCurGroup = 0;
		}
	}

	public void EnableCollect(bool t)
	{
		Component[] componentsInChildren = base.gameObject.GetComponentsInChildren(typeof(ObCollect));
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			((ObCollect)componentsInChildren[i]).enabled = t;
		}
	}

	public virtual void Collect(GameObject obj)
	{
		ObCollect component = obj.GetComponent<ObCollect>();
		if (component != null)
		{
			component.OnItemCollected();
		}
		pNumCollected++;
		SetCounter(pNumRequired - pNumCollected);
		if (!(_MessageObject != null))
		{
			return;
		}
		_MessageObject.SendMessage("OnGroupCollect", this, SendMessageOptions.DontRequireReceiver);
		if (pNumCollected >= pNumRequired)
		{
			UnloadGroup();
			_MessageObject.SendMessage("OnGroupCollectDone", this);
			if (_AllFoundVO != null && !_NoCounter)
			{
				SnChannel.Play(_AllFoundVO, "VO_Pool", 0, inForce: true, null);
			}
		}
	}

	public bool CollectDone()
	{
		return pNumCollected >= pNumRequired;
	}
}
