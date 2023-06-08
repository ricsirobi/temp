using UnityEngine;
using UnityEngine.EventSystems;

namespace JSGames.UI;

public class UICreditsScroll : UI
{
	public Font _Font;

	public Vector2 _LabelSize;

	public UIWidget _labelTemplate;

	public float _TextScale = 50f;

	public float _BoldTextScale = 50f;

	public Color _BoldTextColor = Color.yellow;

	public Color _UnderlineTextColor = Color.cyan;

	public Color _TextColor = Color.white;

	public float _ScrollRate = 3f;

	public GameObject _AnchorObject;

	public GameObject _MessageObject;

	public float _TextStartOffset;

	public float _NextLineOffset = 2f;

	public float _ResetOffset = 30f;

	private UtTableXMLReader mCreditsFile;

	private UtTable mCreditsTable;

	protected override void OnVisibleChanged(bool newVisible)
	{
		base.OnVisibleChanged(newVisible);
		if (newVisible && mCreditsFile == null)
		{
			RsResourceManager.Load(GameConfig.GetKeyData("CreditsFile"), CreditsListReady);
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

	protected override void OnClick(UIWidget widget, PointerEventData eventData)
	{
		base.OnClick(widget, eventData);
		if (widget == _BackButton)
		{
			Object.Destroy(base.gameObject);
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
		for (int i = 0; i < recordCount; i++)
		{
			string value = mCreditsTable.GetValue<string>(mCreditsTable.GetFieldIndex("tfString"), i);
			float y = _TextStartOffset - (float)i * _NextLineOffset;
			UIWidget uIWidget = _labelTemplate.Duplicate(autoAddToSameParent: true);
			uIWidget.transform.SetParent(_AnchorObject.transform);
			uIWidget.name = "Label " + i;
			if (_Font != null)
			{
				uIWidget._Text.font = _Font;
			}
			uIWidget.pRectTransform.sizeDelta = new Vector2((int)_LabelSize.x, (int)_LabelSize.y);
			uIWidget.pText = value;
			uIWidget.pRectTransform.localPosition = new Vector3(0f, y, 0f);
			uIWidget.pTextColor = _TextColor;
			if (value != "")
			{
				num = mCreditsTable.GetValue<int>(mCreditsTable.GetFieldIndex("tfBold"), i);
				num2 = mCreditsTable.GetValue<int>(mCreditsTable.GetFieldIndex("tfUnderLine"), i);
				if (num == 1 && num2 == 1)
				{
					uIWidget.pTextColor = _BoldTextColor;
				}
				else if (num == 1)
				{
					uIWidget.pRectTransform.sizeDelta = new Vector3(_BoldTextScale, _BoldTextScale, 0f);
				}
				else if (num2 == 1)
				{
					uIWidget.pTextColor = _UnderlineTextColor;
				}
			}
		}
		_labelTemplate.pVisible = false;
	}

	protected override void Update()
	{
		base.Update();
		if (pVisible && _AnchorObject != null && mCreditsTable != null)
		{
			Vector3 localPosition = _AnchorObject.transform.localPosition;
			localPosition.y += _ScrollRate;
			_AnchorObject.transform.localPosition = localPosition;
			if (localPosition.y > (float)mCreditsTable.GetRecordCount() * _NextLineOffset + _ResetOffset)
			{
				localPosition.y = _TextStartOffset;
				_AnchorObject.transform.localPosition = localPosition;
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

	protected override void OnVisibleInHierarchyChanged(bool newVisibleInHierarchy)
	{
		base.OnVisibleInHierarchyChanged(newVisibleInHierarchy);
		if (newVisibleInHierarchy && _AnchorObject != null && mCreditsTable != null)
		{
			Vector3 localPosition = _AnchorObject.transform.localPosition;
			localPosition.y = _TextStartOffset;
			_AnchorObject.transform.localPosition = localPosition;
		}
	}
}
