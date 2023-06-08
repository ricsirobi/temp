using System;
using System.Collections.Generic;
using UnityEngine;

public class FieldGuideMenuGrid : KAUIMenuGrid
{
	public float _VocabularyItemWidth = 200f;

	public float _VocabularyItemHeight = 40f;

	public float _SubTitleXOffset = 15f;

	public float _VocabularyXOffset = 15f;

	public float _TextBreakXOffset = 15f;

	public float _GeneralInfoXOffset = 15f;

	public float _ExperimentXOffset = 15f;

	public float _ImageXOffset = 30f;

	public float _ImageYOffset = 5f;

	public int _MaxImagesPerLine = 4;

	public FieldGuideChapter mChapter;

	protected override void GridArrangement()
	{
		if (mChapter == null)
		{
			return;
		}
		List<KAWidget> items = mMenu.GetItems();
		if (items == null || items.Count <= 0)
		{
			return;
		}
		float num = 0f;
		float num2 = 50f;
		KAWidget kAWidget = null;
		KAWidget kAWidget2 = null;
		KAWidget kAWidget3 = null;
		KAWidget kAWidget4 = null;
		List<KAWidget> list = new List<KAWidget>();
		List<KAWidget> list2 = new List<KAWidget>();
		List<KAWidget> list3 = new List<KAWidget>();
		foreach (KAWidget item in items)
		{
			if (item.name == "tbe" || item.name == "tbg" || item.name == "tbi")
			{
				list2.Add(item);
			}
			else if (kAWidget == null && item.name == "VocabWords")
			{
				kAWidget = item;
			}
			else if (kAWidget2 == null && item.name == "GenInfo")
			{
				kAWidget2 = item;
			}
			else if (kAWidget3 == null && item.name.Equals(mChapter.GeneralInformation.Data._Text + "_g"))
			{
				kAWidget3 = item;
			}
			else if (item.name.Contains("_img"))
			{
				list3.Add(item);
			}
			else if (kAWidget4 == null && item.name == "Experiments")
			{
				kAWidget4 = item;
			}
			else
			{
				list.Add(item);
			}
		}
		if (mChapter.Vocabularies != null)
		{
			num = _VocabularyXOffset;
			kAWidget.transform.localPosition = new Vector3(_SubTitleXOffset, 0f - num2, kAWidget.transform.localPosition.z);
			num2 += 50f;
			if (mChapter.Vocabularies.Length != 0)
			{
				int num3 = 0;
				FieldGuideVocabulary[] vocabularies = mChapter.Vocabularies;
				foreach (FieldGuideVocabulary vocabulary in vocabularies)
				{
					foreach (KAWidget item2 in list.FindAll((KAWidget s) => s.name.Equals(vocabulary.Data._Text + "_v")))
					{
						if (NGUITools.GetActive(item2.gameObject) || !hideInactive)
						{
							item2.transform.localPosition = new Vector3(num, 0f - num2, item2.transform.localPosition.z);
							num += _VocabularyItemWidth + _VocabularyXOffset;
							num3++;
							if (num3 >= maxPerLine && maxPerLine > 0)
							{
								num = _VocabularyXOffset;
								num3 = 0;
								num2 += _VocabularyItemHeight;
							}
						}
					}
				}
				num2 += 40f;
			}
		}
		KAWidget kAWidget5 = list2.Find((KAWidget s) => s.name.Equals("tbi"));
		if (kAWidget5 != null)
		{
			kAWidget5.transform.localPosition = new Vector3(_TextBreakXOffset, 0f - num2, kAWidget5.transform.localPosition.z);
			num2 += 50f;
		}
		if (mChapter.Images != null && mChapter.Images.Length != 0)
		{
			num = _ImageXOffset;
			int num4 = 0;
			foreach (KAWidget item3 in list3)
			{
				string[] separator = new string[2] { "/", "_" };
				string[] array = item3.name.Split(separator, StringSplitOptions.None);
				item3.SetTextureFromBundle(array[0] + "_" + array[1] + "/" + array[2], array[3]);
				Transform obj = item3.transform;
				float z = obj.localPosition.z;
				obj.localPosition = new Vector3(num, 0f - num2, z);
				Bounds bounds = NGUIMath.CalculateRelativeWidgetBounds(item3.transform);
				num4++;
				num += bounds.size.x + _ImageXOffset;
				if (num4 >= _MaxImagesPerLine && _MaxImagesPerLine != 0)
				{
					num = _ImageXOffset;
					num4 = 0;
					num2 += bounds.size.y + _ImageYOffset;
				}
			}
			num2 += 50f;
		}
		if (mChapter.GeneralInformation != null)
		{
			kAWidget5 = list2.Find((KAWidget s) => s.name.Equals("tbg"));
			if (kAWidget5 != null)
			{
				kAWidget5.transform.localPosition = new Vector3(_TextBreakXOffset, 0f - num2, kAWidget5.transform.localPosition.z);
				num2 += 20f;
			}
			if (kAWidget2 != null)
			{
				kAWidget2.transform.localPosition = new Vector3(_SubTitleXOffset, 0f - num2, kAWidget2.transform.localPosition.z);
				num2 += 50f;
			}
			if (kAWidget3 != null)
			{
				float z2 = kAWidget3.transform.localPosition.z;
				kAWidget3.transform.localPosition = new Vector3(_GeneralInfoXOffset, 0f - num2, z2);
				num2 += NGUIMath.CalculateRelativeWidgetBounds(kAWidget3.transform).size.y;
			}
			num2 += 30f;
		}
		if (mChapter.Experiments != null && mChapter.Experiments.Length != 0)
		{
			kAWidget5 = list2.Find((KAWidget s) => s.name.Equals("tbe"));
			if (kAWidget5 != null)
			{
				kAWidget5.transform.localPosition = new Vector3(_TextBreakXOffset, 0f - num2, kAWidget5.transform.localPosition.z);
				num2 += 20f;
			}
			if (kAWidget4 != null)
			{
				kAWidget4.transform.localPosition = new Vector3(_SubTitleXOffset, 0f - num2, kAWidget4.transform.localPosition.z);
				num2 += 50f;
			}
			FieldGuideItem[] experiments = mChapter.Experiments;
			foreach (FieldGuideItem experiment in experiments)
			{
				foreach (KAWidget item4 in list.FindAll((KAWidget s) => s.name.Equals(experiment.Data._Text + "_e")))
				{
					item4.transform.localPosition = new Vector3(_ExperimentXOffset, 0f - num2, item4.transform.localPosition.z);
					num2 += NGUIMath.CalculateRelativeWidgetBounds(item4.transform).size.y;
				}
			}
			num2 += 50f;
		}
		UIScrollView uIScrollView = NGUITools.FindInParents<UIScrollView>(base.gameObject);
		if (uIScrollView != null)
		{
			uIScrollView.UpdateScrollbars(recalculateBounds: true);
		}
	}
}
