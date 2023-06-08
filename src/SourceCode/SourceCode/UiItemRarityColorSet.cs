using JSGames.UI;
using UnityEngine;

public class UiItemRarityColorSet : KAMonoBase
{
	private static void SetItemBackgroundColor(Color color, UITexture texture)
	{
		if (texture != null)
		{
			texture.color = color;
			texture.pOrgColorTint = color;
		}
	}

	private static void SetItemBackgroundColor(Color color, UISprite sprite)
	{
		if (sprite != null)
		{
			sprite.color = color;
			sprite.pOrgColorTint = color;
		}
	}

	public static void SetItemBackgroundColor(ItemRarity rarity, JSGames.UI.UIWidget widget, string backgroundName = "Background")
	{
		SetItemBackgroundColor(InventorySetting.pInstance.GetItemRarityColor(rarity), widget, backgroundName);
	}

	public static void SetItemBackgroundColor(ItemRarity rarity, KAWidget widget, string backgroundName = "Background")
	{
		SetItemBackgroundColor(InventorySetting.pInstance.GetItemRarityColor(rarity), widget, backgroundName);
	}

	public static void SetItemBackgroundColor(Color color, KAWidget widget, string backgroundName = "Background")
	{
		if (widget == null)
		{
			return;
		}
		if (widget.pBackground != null)
		{
			SetItemBackgroundColor(color, widget.pBackground);
			return;
		}
		Transform transform = widget.transform.Find(backgroundName);
		if (transform != null)
		{
			UITexture component = transform.GetComponent<UITexture>();
			if (component != null)
			{
				SetItemBackgroundColor(color, component);
				return;
			}
			UISprite component2 = transform.GetComponent<UISprite>();
			if (component2 != null)
			{
				SetItemBackgroundColor(color, component2);
			}
		}
		else
		{
			UtDebug.LogError("Background not found for ::: " + widget.name);
		}
	}

	public static void SetItemBackgroundColor(Color color, JSGames.UI.UIWidget widget, string backgroundName = "Background")
	{
		if (widget == null)
		{
			return;
		}
		if (widget._Background != null)
		{
			widget._Background.color = color;
			return;
		}
		Transform transform = widget.transform.Find(backgroundName);
		if (transform != null)
		{
			UITexture component = transform.GetComponent<UITexture>();
			if (component != null)
			{
				SetItemBackgroundColor(color, component);
				return;
			}
			UISprite component2 = transform.GetComponent<UISprite>();
			if (component2 != null)
			{
				SetItemBackgroundColor(color, component2);
			}
		}
		else
		{
			UtDebug.LogError("Background not found for ::: " + widget.name);
		}
	}

	public static Color GetItemBackgroundColor(KAWidget widget, string colorWidgetName, string backgroundName = "Background")
	{
		Color result = Color.white;
		if (widget == null)
		{
			return result;
		}
		Transform transform = widget.transform.Find(colorWidgetName);
		if (transform != null)
		{
			Transform transform2 = transform.transform.Find(backgroundName);
			if (transform2 == null)
			{
				return result;
			}
			UITexture component = transform2.GetComponent<UITexture>();
			if (component != null)
			{
				result = component.color;
			}
			else
			{
				UISprite component2 = transform2.GetComponent<UISprite>();
				if (component2 != null)
				{
					result = component2.color;
				}
			}
		}
		return result;
	}
}
