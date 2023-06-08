using System.Collections.Generic;
using UnityEngine;

namespace Xsolla;

public class UserProfileController : MonoBehaviour
{
	private List<UserProfileBtnController> listBtn;

	public UserProfileController()
	{
		listBtn = new List<UserProfileBtnController>();
	}

	public void AddBtn(UserProfileBtnController pBtn)
	{
		listBtn.Add(pBtn);
	}
}
