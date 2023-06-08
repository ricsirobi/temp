using System;

[Serializable]
public class Fish
{
	public string _Name;

	public int _ItemID;

	public string _AssetPath;

	[NonSerialized]
	public float _Weight = 3f;

	[NonSerialized]
	public int _Rank = 1;

	[NonSerialized]
	public float _FinalProbability;

	private float mMinWeight;

	private float mMaxWeight;

	public float _AppearanceProbablility;

	public int _NormalStateChance = 4;

	public int _IdleStateChance = 2;

	public int _FastStateChance = 4;

	public FishingZone.FishStateData[] _StateData = new FishingZone.FishStateData[3];

	public float _LeftInterpolationModifier = 1f;

	public float _RightInterpolationModifier = 1f;

	public int _AchievementTaskID;

	public int _AchievementClanTaskID;

	public int _XPAchievementTaskID;

	public LocaleString[] _FishFacts;

	public string pName => _Name;

	public int pItemID => _ItemID;

	public bool LoadData(ItemData item)
	{
		_Name = item.ItemName;
		_ItemID = item.ItemID;
		_AssetPath = item.AssetName;
		return true;
	}
}
