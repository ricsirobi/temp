using System;
using UnityEngine;
using UnityEngine.UI;

namespace Xsolla;

public class UserProfileBtnController : MonoBehaviour
{
	public Text _title;

	public Button _btn;

	public void InitScreen(string pName, Action pAction)
	{
		_title.text = pName;
		_btn.onClick.AddListener(delegate
		{
			pAction();
		});
	}
}
