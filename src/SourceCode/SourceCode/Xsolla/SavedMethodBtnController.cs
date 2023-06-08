using System;
using UnityEngine;
using UnityEngine.UI;

namespace Xsolla;

public class SavedMethodBtnController : MonoBehaviour
{
	public Image _iconMethod;

	public Text _nameMethod;

	public Text _nameType;

	public Button _btnMethod;

	public GameObject _btnDelete;

	public GameObject _toggleObj;

	public Toggle _toggle;

	public Text _btnDeleteName;

	public GameObject _self;

	private XsollaSavedPaymentMethod _method;

	public void setMethod(XsollaSavedPaymentMethod pMethod)
	{
		_method = pMethod;
	}

	public XsollaSavedPaymentMethod getMethod()
	{
		return _method;
	}

	public void setNameMethod(string pNameMethod)
	{
		_nameMethod.text = pNameMethod;
	}

	public void setNameType(string pNametype)
	{
		_nameType.text = pNametype;
	}

	public void setMethodBtn(bool pState)
	{
		_btnMethod.enabled = pState;
	}

	public void setDeleteBtn(bool pState)
	{
		_btnDelete.SetActive(pState);
	}

	public void setToggleObj(bool pState, Action<string, bool> pActionChange)
	{
		_toggleObj.SetActive(pState);
		_toggle.onValueChanged.RemoveAllListeners();
		_toggle.onValueChanged.AddListener(delegate(bool value)
		{
			if (value)
			{
				pActionChange(_method.GetKey(), value);
			}
		});
	}

	public bool getToggleState()
	{
		return _toggle.isOn;
	}

	public void setToggleState(bool pState)
	{
		_toggle.isOn = pState;
	}

	public void setDeleteBtnName(string pName)
	{
		_btnDeleteName.text = pName;
	}

	public Button getBtnDelete()
	{
		return _btnDelete.GetComponent<Button>();
	}
}
