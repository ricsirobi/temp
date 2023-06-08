using System.Collections;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.UI;

namespace JSGames.UI;

public class UIEditBox : UIWidget
{
	[SerializeField]
	private string _RegularExpression;

	private InputField mInputField;

	private bool mInitialized;

	public InputField pInputField
	{
		get
		{
			return mInputField;
		}
		set
		{
			mInputField = value;
		}
	}

	public override string pText
	{
		get
		{
			return mInputField.text;
		}
		set
		{
			mInputField.text = value;
		}
	}

	protected override void Start()
	{
		base.Start();
		StartCoroutine("SetCaretAsLastSibling");
	}

	private IEnumerator SetCaretAsLastSibling()
	{
		yield return new WaitForEndOfFrame();
		if (_Background != null)
		{
			_Background.transform.GetChild(0).SetAsLastSibling();
		}
	}

	public bool IsValidText()
	{
		return Regex.IsMatch(pText, _RegularExpression);
	}

	public override void Initialize(UI parentUI, UIWidget parentWidget)
	{
		base.Initialize(parentUI, parentWidget);
		if (!mInitialized)
		{
			mInitialized = true;
			mInputField = GetComponent<InputField>();
			_Text = mInputField.textComponent;
			mInputField.onEndEdit.AddListener(OnEndEdit);
			mInputField.onValueChanged.AddListener(OnValueChanged);
			if (!string.IsNullOrEmpty(_RegularExpression))
			{
				mInputField.onValidateInput = OnValidateInput;
			}
		}
	}

	[ContextMenu("Fill Effect References")]
	protected override void FillAllEffectReferences()
	{
		base.FillAllEffectReferences();
	}

	protected void OnEndEdit(string text)
	{
		if (base.pEventTarget != null)
		{
			base.pEventTarget.TriggerOnEdit(this, text);
		}
	}

	protected char OnValidateInput(string oldValue, int index, char c)
	{
		if (!Regex.IsMatch(oldValue + c, _RegularExpression))
		{
			return '\0';
		}
		return c;
	}

	protected void OnValueChanged(string text)
	{
		if (base.pEventTarget != null)
		{
			base.pEventTarget.TriggerOnValueChanged(this, text);
		}
	}
}
