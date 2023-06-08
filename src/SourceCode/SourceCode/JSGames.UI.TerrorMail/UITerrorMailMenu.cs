using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace JSGames.UI.TerrorMail;

public class UITerrorMailMenu : UIMenu
{
	public ContentSizeFitter _Content;

	protected override void OnClick(UIWidget widget, PointerEventData eventData)
	{
		base.OnClick(widget, eventData);
		if (widget == _BackButton)
		{
			Object.Destroy(base.gameObject);
		}
	}

	public override void AddWidget(UIWidget widget)
	{
		base.AddWidget(widget);
		if ((bool)_Content)
		{
			widget.transform.SetParent(_Content.transform);
		}
	}
}
