using System;
using System.Collections.Generic;
using SimpleJSON;
using UnityEngine;

namespace Xsolla;

public class StyleManager : Singleton<StyleManager>
{
	public enum Themes
	{
		Black,
		White
	}

	public enum BaseColor
	{
		bg_main,
		bg_top_menu,
		bg_footer,
		bg_left_menu,
		bg_shop_item,
		bg_item_btn,
		bg_pay_btn,
		bg_payment_method,
		b_recomended,
		b_best_deal,
		b_special_offer,
		b_normal,
		txt_top_menu,
		txt_left_menu,
		txt_footer,
		txt_title,
		txt_title_second,
		txt_item_name,
		txt_item_desc,
		txt_item_full_desc,
		txt_item_bonus,
		txt_paymen_system,
		txt_accent,
		txt_accent_2,
		txt_white,
		bg_input_field,
		b_input_field,
		bg_card_1,
		bg_card_2,
		bg_card_line,
		b_card,
		divider_1,
		divider_2,
		divider_3,
		selected,
		bonus,
		bg_ok,
		bg_error,
		link_on_notify
	}

	public Dictionary<BaseColor, Color32> colorsMap;

	public Themes CurrentTheme;

	public Color32 invisColor;

	private static BaseColor[] myColors = Enum.GetValues(typeof(BaseColor)) as BaseColor[];

	protected StyleManager()
	{
	}

	private void GetColors()
	{
		invisColor = new Color32(0, 0, 0, 1);
		string aKey = ((CurrentTheme != 0) ? "default" : "dark");
		JSONNode jSONNode = JSONNode.Parse((Resources.Load("Styles/theme") as TextAsset).text);
		JSONArray asArray = jSONNode["theme"][aKey]["colors"].AsArray;
		_ = jSONNode["colors_map"].AsArray;
		colorsMap = new Dictionary<BaseColor, Color32>(asArray.Count);
		for (int i = 0; i < asArray.Count; i++)
		{
			int hexVal = Convert.ToInt32(asArray[i].Value, 16);
			Color32 value = ToColor(hexVal);
			colorsMap.Add(myColors[i], value);
		}
	}

	public Color32 GetColor(BaseColor color)
	{
		if (colorsMap != null && colorsMap.ContainsKey(color))
		{
			return colorsMap[color];
		}
		return invisColor;
	}

	public Color32 ToColor(int HexVal)
	{
		byte r = (byte)((HexVal >> 16) & 0xFF);
		byte g = (byte)((uint)(HexVal >> 8) & 0xFFu);
		byte b = (byte)((uint)HexVal & 0xFFu);
		return new Color32(r, g, b, byte.MaxValue);
	}

	public void ChangeTheme(string newTheme)
	{
		Logger.Log("ChangeTheme " + newTheme);
		JSONNode jSONNode = JSONNode.Parse((Resources.Load("Styles/theme") as TextAsset).text);
		if ("dark".Equals(newTheme))
		{
			CurrentTheme = Themes.Black;
		}
		else if ("default".Equals(newTheme))
		{
			CurrentTheme = Themes.White;
		}
		else
		{
			newTheme = "dark";
		}
		JSONArray asArray = jSONNode["theme"][newTheme]["colors"].AsArray;
		_ = jSONNode["colors_map"].AsArray;
		colorsMap = new Dictionary<BaseColor, Color32>(asArray.Count);
		for (int i = 0; i < asArray.Count; i++)
		{
			int hexVal = Convert.ToInt32(asArray[i].Value, 16);
			Color32 value = ToColor(hexVal);
			colorsMap.Add(myColors[i], value);
		}
	}

	private void Awake()
	{
		GetColors();
	}

	private void Start()
	{
	}

	private void Update()
	{
	}
}
