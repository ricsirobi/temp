using UnityEngine;

public class KAUiCreditsScroll : KAUI
{
	public UIFont _Font;

	public Vector2 _LabelSize;

	public float _TextScale = 50f;

	public float _BoldTextScale = 50f;

	public Color _BoldTextColor = Color.yellow;

	public Color _UnderlineTextColor = Color.cyan;

	public Color _TextColor = Color.white;

	public float _ScrollRate = 3f;

	public GameObject _AnchorObject;

	public GameObject _MessageObject;

	public float _TextStartOffset = -300f;

	public float _NextLineOffset = 2f;

	public float _ResetOffset = 30f;

	private GameObject mParent;

	private UtTableXMLReader mCreditsFile;

	private UtTable mCreditsTable;

	protected override void Start()
	{
		base.Start();
		RsResourceManager.Load(GameConfig.GetKeyData("CreditsFile"), CreditsListReady);
		if (AvAvatar.pToolbar != null)
		{
			UiToolbar component = AvAvatar.pToolbar.GetComponent<UiToolbar>();
			if (component != null && !string.IsNullOrEmpty(RsResourceManager.pLastLevel))
			{
				component._BackBtnLoadLevel = UiOptions._Credits_exitLevel;
			}
		}
	}

	public void CreditsListReady(string inURL, RsResourceLoadEvent inEvent, float inProgress, object inFile, object inUserData)
	{
		if (inEvent == RsResourceLoadEvent.COMPLETE)
		{
			string fileData = (string)inFile;
			mCreditsFile = new UtTableXMLReader();
			mCreditsFile.LoadString(fileData);
			StartCredits();
		}
	}

	public void StartCredits()
	{
		if (mCreditsFile == null)
		{
			return;
		}
		mCreditsTable = mCreditsFile["tnCredits"];
		int recordCount = mCreditsTable.GetRecordCount();
		int num = 0;
		int num2 = 0;
		mParent = new GameObject("Parent");
		mParent.layer = LayerMask.NameToLayer("3DNGUI");
		if (_AnchorObject != null)
		{
			mParent.transform.parent = _AnchorObject.transform;
		}
		else
		{
			mParent.transform.parent = base.gameObject.transform;
		}
		mParent.transform.localPosition = Vector3.zero;
		mParent.transform.localRotation = Quaternion.identity;
		for (int i = 0; i < recordCount; i++)
		{
			string value = mCreditsTable.GetValue<string>(mCreditsTable.GetFieldIndex("tfString"), i);
			float y = _TextStartOffset - (float)i * _NextLineOffset;
			UILabel uILabel = mParent.AddWidget<UILabel>();
			uILabel.gameObject.name = "Label " + i;
			uILabel.bitmapFont = _Font;
			uILabel.width = (int)_LabelSize.x;
			uILabel.height = (int)_LabelSize.y;
			uILabel.overflowMethod = UILabel.Overflow.ShrinkContent;
			uILabel.cachedTransform.localScale = new Vector3(_TextScale, _TextScale, 0f);
			uILabel.text = value;
			uILabel.cachedTransform.localPosition = new Vector3(0f, y, 0f);
			uILabel.color = _TextColor;
			if (value != "")
			{
				num = mCreditsTable.GetValue<int>(mCreditsTable.GetFieldIndex("tfBold"), i);
				num2 = mCreditsTable.GetValue<int>(mCreditsTable.GetFieldIndex("tfUnderLine"), i);
				if (num == 1 && num2 == 1)
				{
					uILabel.cachedTransform.localScale = new Vector3(_BoldTextScale, _BoldTextScale, 0f);
					uILabel.color = _BoldTextColor;
				}
				else if (num == 1)
				{
					uILabel.cachedTransform.localScale = new Vector3(_BoldTextScale, _BoldTextScale, 0f);
				}
				else if (num2 == 1)
				{
					uILabel.color = _UnderlineTextColor;
				}
			}
		}
	}

	protected override void Update()
	{
		base.Update();
		if (mParent != null)
		{
			Vector3 localPosition = mParent.transform.localPosition;
			localPosition.y += _ScrollRate;
			mParent.transform.localPosition = localPosition;
			if (localPosition.y > (float)mCreditsTable.GetRecordCount() * _NextLineOffset + _ResetOffset)
			{
				localPosition.y = _TextStartOffset;
				mParent.transform.localPosition = localPosition;
			}
		}
	}

	private void EndCredits()
	{
		Object.Destroy(base.gameObject);
		if (_MessageObject != null)
		{
			_MessageObject.SendMessage("EndScroll", SendMessageOptions.DontRequireReceiver);
		}
	}

	public override void OnClick(KAWidget inWidget)
	{
		base.OnClick(inWidget);
		if (inWidget.name.Contains("BtnBack"))
		{
			EndCredits();
		}
	}
}
