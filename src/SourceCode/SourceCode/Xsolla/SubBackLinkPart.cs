using System;
using UnityEngine;
using UnityEngine.UI;

namespace Xsolla;

public class SubBackLinkPart : MonoBehaviour
{
	public Text mBackLinkLabel;

	public Button mBackBtn;

	public Text mConfirmLabel;

	public GameObject mConfirmBtn;

	public void init(string pBackLabel, Action pBackAction, string pConfirmLabel = "", Action pConfirmAction = null)
	{
		mBackLinkLabel.text = pBackLabel;
		mBackBtn.onClick.AddListener(delegate
		{
			pBackAction();
		});
		if (pConfirmLabel != "")
		{
			mConfirmBtn.SetActive(value: true);
			mConfirmLabel.text = pConfirmLabel;
			mConfirmBtn.GetComponent<Button>().onClick.AddListener(delegate
			{
				pConfirmAction();
			});
		}
		else
		{
			mConfirmBtn.SetActive(value: false);
		}
	}
}
