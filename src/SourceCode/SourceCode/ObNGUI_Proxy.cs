using UnityEngine;

public class ObNGUI_Proxy : MonoBehaviour
{
	public UIWidget _Widget;

	public Vector3 _Scale = Vector3.one;

	private Transform mWidgetTransform;

	private Transform mTransform;

	private GameObject mWidgetGameObject;

	private static Transform mNGUI_3D_Manager;

	public static Transform pNGUI_3D_Manager
	{
		get
		{
			if (mNGUI_3D_Manager != null)
			{
				return mNGUI_3D_Manager;
			}
			GameObject obj = Object.Instantiate((GameObject)RsResourceManager.LoadAssetFromResources("_NGUI_3D"));
			obj.name = "_NGUI_3D";
			mNGUI_3D_Manager = obj.transform;
			Object.DontDestroyOnLoad(obj);
			return mNGUI_3D_Manager;
		}
		set
		{
			mNGUI_3D_Manager = value;
		}
	}

	public void Awake()
	{
		mTransform = base.transform;
		if (_Widget != null)
		{
			mWidgetTransform = _Widget.transform;
			mWidgetTransform.parent = pNGUI_3D_Manager;
			mWidgetTransform.localScale = _Scale;
			mWidgetGameObject = _Widget.gameObject;
			mWidgetGameObject.layer = pNGUI_3D_Manager.gameObject.layer;
		}
		UpdateData();
	}

	public void OnDisable()
	{
		UpdateData();
		if (mWidgetGameObject != null)
		{
			mWidgetGameObject.SetActive(value: false);
		}
	}

	public void OnEnable()
	{
		UpdateData();
		if (mWidgetGameObject != null)
		{
			mWidgetGameObject.SetActive(value: true);
		}
	}

	public void UpdateData()
	{
		if (!(mWidgetTransform == null) && !(mTransform == null))
		{
			mWidgetTransform.position = mTransform.position;
			mWidgetTransform.rotation = mTransform.rotation;
		}
	}

	public void OnDestroy()
	{
		if (_Widget != null)
		{
			Object.Destroy(_Widget.gameObject);
		}
	}

	public void SetText(string Text)
	{
		if (!(_Widget == null))
		{
			UILabel uILabel = (UILabel)_Widget;
			if (!(uILabel == null))
			{
				uILabel.text = Text;
			}
		}
	}

	public string GetText()
	{
		if (_Widget == null)
		{
			return null;
		}
		UILabel uILabel = (UILabel)_Widget;
		if (uILabel == null)
		{
			return null;
		}
		return uILabel.text;
	}

	public void SetSpriteName(string SpriteName)
	{
		if (!(_Widget == null))
		{
			UISprite uISprite = (UISprite)_Widget;
			if (!(uISprite == null))
			{
				uISprite.spriteName = SpriteName;
			}
		}
	}

	public void SetVisible(bool Visible)
	{
		if (_Widget != null && _Widget.enabled != Visible)
		{
			_Widget.enabled = Visible;
		}
	}

	public bool GetVisible()
	{
		if (!(_Widget != null))
		{
			return false;
		}
		return _Widget.enabled;
	}

	public void SetColor(Color inColor)
	{
		if (_Widget != null)
		{
			_Widget.color = inColor;
		}
	}
}
