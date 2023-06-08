using System.Collections.Generic;
using UnityEngine;

public class UDTUtilities
{
	public class UDTLevelInfo
	{
		public float _StartPercent;

		public string _StarSprite;

		public string _StarFrameSprite;

		public float _NumStars;

		public UDTLevelInfo(float inStartPercent, string inSprite, string inFrameSprite, float inNumStars)
		{
			_StartPercent = inStartPercent;
			_StarSprite = inSprite;
			_StarFrameSprite = inFrameSprite;
			_NumStars = inNumStars;
		}
	}

	internal class UDTListSort : IComparer<UDTLevelInfo>
	{
		public int Compare(UDTLevelInfo inFirst, UDTLevelInfo inSecond)
		{
			if (inFirst._StartPercent > inSecond._StartPercent)
			{
				return 1;
			}
			if (inSecond._StartPercent > inFirst._StartPercent)
			{
				return -1;
			}
			return 0;
		}
	}

	public static List<UDTLevelInfo> _UDTStarsInfo = new List<UDTLevelInfo>(new UDTLevelInfo[23]
	{
		new UDTLevelInfo(0f, "", "IcoDWDragonsVeterancyBlank02", 1f),
		new UDTLevelInfo(0.1f, "IcoDWDragonsVeterancy02", "IcoDWDragonsVeterancyBlank02", 0.5f),
		new UDTLevelInfo(0.5f, "IcoDWDragonsVeterancy02", "IcoDWDragonsVeterancyBlank02", 1f),
		new UDTLevelInfo(1.25f, "IcoDWDragonsVeterancy02", "IcoDWDragonsVeterancyBlank02", 1.5f),
		new UDTLevelInfo(2f, "IcoDWDragonsVeterancy02", "IcoDWDragonsVeterancyBlank02", 2f),
		new UDTLevelInfo(3.5f, "IcoDWDragonsVeterancy02", "IcoDWDragonsVeterancyBlank02", 2.5f),
		new UDTLevelInfo(5f, "IcoDWDragonsVeterancy02", "IcoDWDragonsVeterancyBlank02", 3f),
		new UDTLevelInfo(7.5f, "IcoDWDragonsVeterancy01", "IcoDWDragonsVeterancyBlank01", 1f),
		new UDTLevelInfo(10f, "IcoDWDragonsVeterancy01", "IcoDWDragonsVeterancyBlank01", 1.5f),
		new UDTLevelInfo(13.5f, "IcoDWDragonsVeterancy01", "IcoDWDragonsVeterancyBlank01", 2f),
		new UDTLevelInfo(17f, "IcoDWDragonsVeterancy01", "IcoDWDragonsVeterancyBlank01", 2.5f),
		new UDTLevelInfo(21.5f, "IcoDWDragonsVeterancy01", "IcoDWDragonsVeterancyBlank01", 3f),
		new UDTLevelInfo(26f, "IcoDWDragonsVeterancy04", "IcoDWDragonsVeterancyBlank04", 1f),
		new UDTLevelInfo(31.5f, "IcoDWDragonsVeterancy04", "IcoDWDragonsVeterancyBlank04", 1.5f),
		new UDTLevelInfo(37f, "IcoDWDragonsVeterancy04", "IcoDWDragonsVeterancyBlank04", 2f),
		new UDTLevelInfo(43.5f, "IcoDWDragonsVeterancy04", "IcoDWDragonsVeterancyBlank04", 2.5f),
		new UDTLevelInfo(50f, "IcoDWDragonsVeterancy04", "IcoDWDragonsVeterancyBlank04", 3f),
		new UDTLevelInfo(57.5f, "IcoDWDragonsVeterancy03", "IcoDWDragonsVeterancyBlank03", 1f),
		new UDTLevelInfo(65f, "IcoDWDragonsVeterancy03", "IcoDWDragonsVeterancyBlank03", 1.5f),
		new UDTLevelInfo(73.5f, "IcoDWDragonsVeterancy03", "IcoDWDragonsVeterancyBlank03", 2f),
		new UDTLevelInfo(82f, "IcoDWDragonsVeterancy03", "IcoDWDragonsVeterancyBlank03", 2.5f),
		new UDTLevelInfo(91f, "IcoDWDragonsVeterancy03", "IcoDWDragonsVeterancyBlank03", 3f),
		new UDTLevelInfo(100f, "IcoDWDragonsVeterancy03", "", 1f)
	});

	public static void UpdateUDTStars(Transform inParent, UserAchievementInfo[] inAchievementInfo, string inStarObjectNamePrefix, string inStarFramePrefix = "")
	{
		UserAchievementInfo userAchievementInfoByType = UserRankData.GetUserAchievementInfoByType(inAchievementInfo, 12);
		UpdateUDTStars(inParent, userAchievementInfoByType, inStarObjectNamePrefix, inStarFramePrefix);
	}

	public static void UpdateUDTStars(Transform inParent, string inStarObjectNamePrefix, string inStarFramePrefix = "")
	{
		UserAchievementInfo userAchievementInfoByType = UserRankData.GetUserAchievementInfoByType(12);
		UpdateUDTStars(inParent, userAchievementInfoByType, inStarObjectNamePrefix, inStarFramePrefix);
	}

	public static void UpdateUDTStars(Transform inParent, UserAchievementInfo inAchievementInfo, string inStarObjectNamePrefix, string inStarFramePrefix = "")
	{
		int num = 0;
		if (inAchievementInfo != null && inAchievementInfo.AchievementPointTotal.HasValue)
		{
			num = inAchievementInfo.AchievementPointTotal.Value;
		}
		UpdateUDTStars(inParent, inStarObjectNamePrefix, num, inStarFramePrefix);
	}

	private static Transform GetTransform(Transform inParent, string inTransformName, bool inUseNGUIProxy)
	{
		Transform transform = inParent.Find(inTransformName);
		if (inUseNGUIProxy && transform != null)
		{
			ObNGUI_Proxy component = transform.GetComponent<ObNGUI_Proxy>();
			if (component != null)
			{
				transform = component._Widget.transform;
			}
		}
		return transform;
	}

	public static void UpdateUDTStars(Transform inParent, string inStarObjectNamePrefix, float inUDTPoints, string inStarFramePrefix = "", bool inUseNGUIProxy = false)
	{
		HideAllStars(inParent, inStarObjectNamePrefix, inStarFramePrefix, inUseNGUIProxy);
		UserRank userRankByTypeAndValue = UserRankData.GetUserRankByTypeAndValue(12, int.MaxValue);
		if (userRankByTypeAndValue == null)
		{
			return;
		}
		int value = userRankByTypeAndValue.Value;
		_UDTStarsInfo.Sort(new UDTListSort());
		UDTLevelInfo uDTLevelInfo = GetUDTLevelInfo(inUDTPoints / (float)value * 100f);
		if (uDTLevelInfo == null)
		{
			return;
		}
		if (uDTLevelInfo._StartPercent <= 0f)
		{
			ShowEmptyStars(uDTLevelInfo, inParent, inStarFramePrefix, inUseNGUIProxy);
			return;
		}
		int num = Mathf.CeilToInt(uDTLevelInfo._NumStars);
		float num2 = uDTLevelInfo._NumStars;
		if (inUDTPoints < (float)value)
		{
			ShowEmptyStars(uDTLevelInfo, inParent, inStarFramePrefix, inUseNGUIProxy);
			for (int i = 1; i <= num; i++)
			{
				Transform transform = GetTransform(inParent, inStarObjectNamePrefix + $"{i:d2}", inUseNGUIProxy);
				Transform transform2 = GetTransform(inParent, inStarFramePrefix + $"{i:d2}", inUseNGUIProxy);
				if (!(transform != null))
				{
					continue;
				}
				if (transform2 != null)
				{
					UISprite component = transform2.GetComponent<UISprite>();
					if (component != null)
					{
						component.fillAmount = 1f;
						component.spriteName = uDTLevelInfo._StarFrameSprite;
					}
				}
				UISprite component2 = transform.GetComponent<UISprite>();
				if (component2 != null)
				{
					component2.fillAmount = Mathf.Clamp01(num2);
					component2.spriteName = uDTLevelInfo._StarSprite;
					num2 -= 1f;
				}
			}
			return;
		}
		Transform transform3 = GetTransform(inParent, inStarObjectNamePrefix + $"{0:d2}", inUseNGUIProxy);
		if (transform3 != null)
		{
			UISprite component3 = transform3.GetComponent<UISprite>();
			if (component3 != null)
			{
				component3.fillAmount = Mathf.Clamp01(1f);
				component3.spriteName = uDTLevelInfo._StarSprite;
				num2 -= 1f;
			}
		}
	}

	private static UDTLevelInfo GetUDTLevelInfo(float inUDTPercent)
	{
		UDTLevelInfo result = null;
		if (inUDTPercent >= _UDTStarsInfo[_UDTStarsInfo.Count - 1]._StartPercent)
		{
			result = _UDTStarsInfo[_UDTStarsInfo.Count - 1];
		}
		else
		{
			for (int i = 0; i < _UDTStarsInfo.Count - 1; i++)
			{
				if (_UDTStarsInfo[i]._StartPercent <= inUDTPercent && _UDTStarsInfo[i + 1]._StartPercent > inUDTPercent)
				{
					result = _UDTStarsInfo[i];
					break;
				}
			}
		}
		return result;
	}

	private static void HideAllStars(Transform inParent, string inStarObjectNamePrefix, string inStarFramePrefix = "", bool inUseNGUIProxy = false)
	{
		int num = 0;
		Transform transform = GetTransform(inParent, inStarObjectNamePrefix + $"{num:d2}", inUseNGUIProxy);
		Transform transform2 = GetTransform(inParent, inStarFramePrefix + $"{num:d2}", inUseNGUIProxy);
		while (transform != null)
		{
			UISprite component = transform.GetComponent<UISprite>();
			if (component != null)
			{
				component.fillAmount = 0f;
			}
			if (transform2 != null)
			{
				UISprite component2 = transform2.GetComponent<UISprite>();
				if (component2 != null)
				{
					component2.fillAmount = 0f;
				}
			}
			num++;
			transform = GetTransform(inParent, inStarObjectNamePrefix + $"{num:d2}", inUseNGUIProxy);
			transform2 = GetTransform(inParent, inStarFramePrefix + $"{num:d2}", inUseNGUIProxy);
		}
	}

	private static void ShowEmptyStars(UDTLevelInfo inLevel, Transform inParent, string inStarFramePrefix, bool inUseNGUIProxy = false)
	{
		int num = 1;
		Transform transform = GetTransform(inParent, inStarFramePrefix + $"{num:d2}", inUseNGUIProxy);
		while (transform != null)
		{
			UISprite component = transform.GetComponent<UISprite>();
			if (component != null)
			{
				component.fillAmount = 1f;
				component.spriteName = inLevel._StarFrameSprite;
			}
			num++;
			transform = GetTransform(inParent, inStarFramePrefix + $"{num:d2}", inUseNGUIProxy);
		}
	}
}
