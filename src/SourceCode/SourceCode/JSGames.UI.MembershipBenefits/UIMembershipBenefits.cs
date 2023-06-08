using UnityEngine;
using UnityEngine.EventSystems;

namespace JSGames.UI.MembershipBenefits;

public class UIMembershipBenefits : UI
{
	public UIWidget _CloseBtn;

	protected override void Start()
	{
		SetExclusive();
	}

	protected override void OnClick(UIWidget widget, PointerEventData eventData)
	{
		base.OnClick(widget, eventData);
		if (!(widget != _CloseBtn))
		{
			RemoveExclusive();
			Object.Destroy(base.gameObject);
		}
	}
}
