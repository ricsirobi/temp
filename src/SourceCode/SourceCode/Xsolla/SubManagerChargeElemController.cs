using UnityEngine;
using UnityEngine.UI;

namespace Xsolla;

public class SubManagerChargeElemController : MonoBehaviour
{
	public Text mDateLabel;

	public Text mPaymentType;

	public Text mAmount;

	public GameObject mDevider;

	public void init(string pDate, string pPaymentType, string pAmount, bool pIsTitle = false)
	{
		mDevider.SetActive(pIsTitle);
		mDateLabel.text = pDate;
		mPaymentType.text = pPaymentType;
		mAmount.text = pAmount;
		mDateLabel.fontStyle = (pIsTitle ? FontStyle.Bold : FontStyle.Normal);
		mPaymentType.fontStyle = (pIsTitle ? FontStyle.Bold : FontStyle.Normal);
		mAmount.fontStyle = (pIsTitle ? FontStyle.Bold : FontStyle.Normal);
	}
}
