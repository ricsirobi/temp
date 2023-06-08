using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Zendesk.UI;

public class ScrollRectExpanded : ScrollRect
{
	public override void OnEndDrag(PointerEventData eventData)
	{
		if (base.content.GetComponent<RectTransform>().localPosition.y <= -250f)
		{
			GetComponentInParent<ZendeskUI>().RefreshPage();
		}
		base.OnEndDrag(eventData);
	}
}
