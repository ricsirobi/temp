using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Xsolla;

public class ValidatorInputField : MonoBehaviour
{
	public InputField _input;

	public GameObject error;

	public Text errorText;

	public Graphic[] imagesToColor;

	public ValidatorFactory.ValidatorType[] types;

	protected int _limit;

	protected bool _isValid;

	private bool isActive;

	private bool isErrorShowed;

	private List<IValidator> validators;

	private string _errorMsg = "Invalid";

	private bool isOn;

	public override string ToString()
	{
		return $"[ValidatorInputField: _input={_input}, error={error}, errorText={errorText}, imagesToColor={imagesToColor}, types={types}, _limit={_limit}, _isValid={_isValid}, isActive={isActive}, isErrorShowed={isErrorShowed}, validators={validators}, _errorMsg={_errorMsg}, isOn={isOn}]";
	}

	private void Awake()
	{
		if (error != null)
		{
			error.gameObject.SetActive(value: false);
		}
		InitEventTrigger();
	}

	private void Start()
	{
		if (types != null && types.Length != 0)
		{
			ValidatorFactory.ValidatorType[] array = types;
			foreach (ValidatorFactory.ValidatorType type in array)
			{
				validators.Add(ValidatorFactory.GetByType(type));
			}
			SetErrorMsg(validators[0].GetErrorMsg());
		}
		_input.onValueChanged.AddListener(delegate(string s)
		{
			Validate(s);
		});
	}

	private void InitEventTrigger()
	{
		UnityEngine.EventSystems.EventTrigger eventTrigger = base.gameObject.AddComponent<UnityEngine.EventSystems.EventTrigger>();
		UnityEngine.EventSystems.EventTrigger.Entry entry = new UnityEngine.EventSystems.EventTrigger.Entry();
		entry.eventID = EventTriggerType.Select;
		entry.callback = new UnityEngine.EventSystems.EventTrigger.TriggerEvent();
		UnityAction<BaseEventData> call = OnSelected;
		entry.callback.AddListener(call);
		UnityEngine.EventSystems.EventTrigger.Entry entry2 = new UnityEngine.EventSystems.EventTrigger.Entry();
		entry2.eventID = EventTriggerType.Deselect;
		entry2.callback = new UnityEngine.EventSystems.EventTrigger.TriggerEvent();
		UnityAction<BaseEventData> call2 = OnDeselected;
		entry2.callback.AddListener(call2);
		eventTrigger.triggers = new List<UnityEngine.EventSystems.EventTrigger.Entry> { entry, entry2 };
	}

	public ValidatorInputField()
	{
		validators = new List<IValidator>();
	}

	public void AddValidator(IValidator validator)
	{
		validators.Add(validator);
	}

	public void SetErrorMsg(string msg)
	{
		_errorMsg = msg;
	}

	private void OnSelected(BaseEventData baseEvent)
	{
		ShowErrorText(!_isValid);
	}

	private void OnDeselected(BaseEventData baseEvent)
	{
		ShowErrorText(b: false);
		if (!isOn)
		{
			isOn = true;
		}
		UpdateBorderColor();
	}

	protected void ShowErrorText(bool b)
	{
		if (isOn && error != null && errorText != null)
		{
			errorText.text = _errorMsg;
			error.gameObject.SetActive(b);
			isErrorShowed = b;
		}
	}

	protected void UpdateBorderColor()
	{
		if (isOn)
		{
			Color color = ((!_isValid) ? ((Color)Singleton<StyleManager>.Instance.GetColor(StyleManager.BaseColor.bg_error)) : ((Color)Singleton<StyleManager>.Instance.GetColor(StyleManager.BaseColor.bg_ok)));
			Graphic[] array = imagesToColor;
			for (int i = 0; i < array.Length; i++)
			{
				array[i].color = color;
			}
		}
	}

	public bool IsValid()
	{
		if (isOn)
		{
			return _isValid;
		}
		isOn = true;
		return Validate("");
	}

	public bool Validate(string s)
	{
		_isValid = true;
		foreach (IValidator validator in validators)
		{
			if (!validator.Validate(s))
			{
				SetErrorMsg(validator.GetErrorMsg());
				_isValid = false;
			}
		}
		ShowErrorText(!_isValid);
		UpdateBorderColor();
		return _isValid;
	}
}
