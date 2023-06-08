using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Xsolla;

public class SubManagerDetailChargesPartController : MonoBehaviour
{
	public Text mPartTitle;

	public GameObject mDetailsContainer;

	public GameObject mMorePanel;

	public Text mShowMoreLabel;

	public Button mShowMoreBtn;

	private const string mChargeItemTextPrefab = "Prefabs/Screens/SubsManager/Simple/ChargeItem";

	private XsollaUtils mUtils;

	private XsollaManagerSubDetails mSubDetail;

	private List<GameObject> mListCharges;

	private bool mShowState;

	public void init(XsollaManagerSubDetails pSubDetail, XsollaUtils pUtils)
	{
		mUtils = pUtils;
		mSubDetail = pSubDetail;
		mListCharges = new List<GameObject>();
		mPartTitle.text = mUtils.GetTranslations().Get("user_subscription_charges_title");
		string pDate = mUtils.GetTranslations().Get("virtualstatus_check_time");
		string pPaymetntype = mUtils.GetTranslations().Get("user_subscription_payment_header");
		string pAmount = mUtils.GetTranslations().Get("user_subscription_payment");
		addChargeElem(pDate, pPaymetntype, pAmount, pIsTitle: true);
		mSubDetail.mCharges.Sort((XsollaSubDetailCharge charge1, XsollaSubDetailCharge charge2) => (charge1.mDateCreate < charge2.mDateCreate) ? 1 : (-1));
		mMorePanel.SetActive(mSubDetail.mCharges.Count > 4);
		mShowMoreBtn.onClick.AddListener(OnMoreBtnAction);
		foreach (XsollaSubDetailCharge mCharge in mSubDetail.mCharges)
		{
			mListCharges.Add(addChargeElem(StringHelper.DateFormat(mCharge.mDateCreate), mCharge.mPaymentMethod, mCharge.mCharge.ToString()));
		}
		ShowList(mShowState);
	}

	private GameObject addChargeElem(string pDate, string pPaymetntype, string pAmount, bool pIsTitle = false)
	{
		GameObject obj = Object.Instantiate(Resources.Load("Prefabs/Screens/SubsManager/Simple/ChargeItem")) as GameObject;
		SubManagerChargeElemController component = obj.GetComponent<SubManagerChargeElemController>();
		component.init(pDate, pPaymetntype, pAmount, pIsTitle);
		component.transform.SetParent(mDetailsContainer.transform);
		return obj;
	}

	private void OnMoreBtnAction()
	{
		ShowList(!mShowState);
	}

	private void ShowList(bool pMore)
	{
		if (pMore)
		{
			mListCharges.ForEach(delegate(GameObject obj)
			{
				obj.SetActive(value: true);
			});
		}
		else if (mListCharges.Count > 4)
		{
			mListCharges.GetRange(3, mListCharges.Count - 3).ForEach(delegate(GameObject obj)
			{
				obj.SetActive(value: false);
			});
		}
		mShowMoreLabel.text = mUtils.GetTranslations().Get(pMore ? "user_subscription_expand_link_expanded" : "user_subscription_expand_link");
		mShowState = pMore;
	}
}
