using System;
using UnityEngine;
using UnityEngine.UI;

namespace Xsolla;

public class LabelValueController : MonoBehaviour
{
	public Text mlabel;

	public Text mValue;

	public Text mActionLabel;

	public Button mActionBtn;

	public void init(string pLabel, string pValue, string pActionLabel = null, Action pAction = null)
	{
		mlabel.text = pLabel + ":";
		mValue.text = pValue;
		if (pAction != null)
		{
			mActionBtn.gameObject.SetActive(value: true);
			mActionLabel.text = pActionLabel;
			mActionBtn.onClick.AddListener(delegate
			{
				pAction();
			});
		}
	}
}
