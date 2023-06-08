using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using UnityEngine;

[Serializable]
public class LabItem
{
	public delegate void InitializedCallback();

	public class SortStateTemperature : IComparer<LabState>
	{
		private bool mAscending = true;

		public SortStateTemperature(bool inAscending)
		{
			mAscending = inAscending;
		}

		public int Compare(LabState x, LabState y)
		{
			if (x == null && y == null)
			{
				return 0;
			}
			if (x != null && y == null)
			{
				return 1;
			}
			if (x == null && y != null)
			{
				return -1;
			}
			if (x.Temperature < y.Temperature)
			{
				if (mAscending)
				{
					return -1;
				}
				return 1;
			}
			if (x.Temperature == y.Temperature)
			{
				return 0;
			}
			if (x.Temperature >= y.Temperature)
			{
				if (mAscending)
				{
					return 1;
				}
				return -1;
			}
			return 0;
		}
	}

	[XmlElement(ElementName = "Name")]
	public string Name;

	[XmlElement(ElementName = "DisplayNameText")]
	public LocaleString DisplayNameText;

	[XmlElement(ElementName = "Icon")]
	public string Icon;

	[XmlElement(ElementName = "Prefab")]
	public string Prefab;

	[XmlElement(ElementName = "ColliderGO")]
	public string ColliderGO;

	[XmlElement(ElementName = "HideItemsOnAdd")]
	public bool HideItemsOnAdd;

	[XmlElement(ElementName = "StopsCombustion")]
	public bool StopsCombustion;

	[XmlElement(ElementName = "DestroyOnInvalidDrop")]
	public bool DestroyOnInvalidDrop;

	[XmlElement(ElementName = "ShaderName")]
	public string ShaderName;

	[XmlElement(ElementName = "DefaultShaderProperty")]
	public LabShaderProperty[] DefaultShaderProperties;

	[XmlElement(ElementName = "DefaultShaderValues")]
	public LabShaderValue[] DefaultShaderValues;

	[XmlElement(ElementName = "Weight")]
	public float Weight;

	[XmlElement(ElementName = "FreezeRate")]
	public float FreezeRate;

	[XmlElement(ElementName = "WarmingRate")]
	public float WarmingRate = 1f;

	[XmlElement(ElementName = "CoolingRate")]
	public float CoolingRate;

	[XmlElement(ElementName = "MinTemperature")]
	public float MinTemperature;

	[XmlElement(ElementName = "MaxTemperature")]
	public float MaxTemperature;

	[XmlElement(ElementName = "Resistance")]
	public float Resistance;

	[XmlElement(ElementName = "CrucibleItem")]
	public string CrucibleItem;

	[XmlElement(ElementName = "ScaleTime")]
	public float ScaleTime;

	[XmlElement(ElementName = "Category")]
	public string Category;

	[XmlElement(ElementName = "Color")]
	public string ColorValue;

	[XmlElement(ElementName = "AcidityBracket")]
	public int AcidityBracket;

	[XmlElement(ElementName = "LiquidPriority")]
	public int LiquidPriority = -1;

	[XmlElement(ElementName = "ScaleX")]
	public float ScaleX = 1f;

	[XmlElement(ElementName = "ScaleY")]
	public float ScaleY = 1f;

	[XmlElement(ElementName = "ScaleZ")]
	public float ScaleZ = 1f;

	[XmlElement(ElementName = "CoolingDownSound")]
	public string CoolingDownSoundURL;

	[XmlElement(ElementName = "LabItemProperty")]
	public LabItemProperty[] Properties;

	[XmlElement(ElementName = "State")]
	public LabState[] States;

	[XmlElement(ElementName = "Combination")]
	public LabItemCombination[] Combinations;

	[XmlElement(ElementName = "LiquidSplashParticleColor")]
	public Color LiquidSplashParticleColor;

	[XmlElement(ElementName = "BlastsToReachMaxTemp")]
	public int BlastsToReachMaxTemp = 3;

	[XmlElement(ElementName = "FuelTime")]
	public float FuelTime;

	[XmlElement(ElementName = "Interaction")]
	public LabInteraction[] Interactions;

	[XmlElement(ElementName = "Pickable")]
	public bool Pickable = true;

	[XmlElement(ElementName = "RotationX")]
	public float DefaultRotationX;

	[XmlElement(ElementName = "RotationY")]
	public float DefaultRotationY;

	[XmlElement(ElementName = "RotationZ")]
	public float DefaultRotationZ;

	[XmlElement(ElementName = "DefaultAdditions")]
	public LabAdditionObjectData[] DefaultAdditions;

	private Vector3 mDefaultRotation = Vector3.zero;

	private Vector3 mScale = Vector3.one;

	private int mDefaultObjectLoadedCount;

	private int mInitializedCount;

	[XmlIgnore]
	private InitializedCallback mInitializeCallback;

	private List<LabState> mSortedStates;

	private List<LabState> mSortedStatesBelowRoomTemp;

	private List<LabState> mSortedStatesAboveRoomTemp;

	private float mStartTemperature;

	[XmlIgnore]
	public bool mNoScaling;

	[XmlIgnore]
	public LabTextureLoader mShaderTextureLoader;

	private LabItemCategory mCategory = LabItemCategory.NONE;

	public Vector3 pDefaultRotation => mDefaultRotation;

	[XmlIgnore]
	public Vector3 pScale => mScale;

	[XmlIgnore]
	public bool pInitialized => mInitializedCount >= 4;

	[XmlIgnore]
	public AudioClip mCoolingDownClip { get; set; }

	[XmlIgnore]
	public float pStartTemperature
	{
		get
		{
			return mStartTemperature;
		}
		set
		{
			mStartTemperature = value;
		}
	}

	public List<LabState> pSortedStates => mSortedStates;

	[XmlIgnore]
	public List<LabState> pSortedStatesBelowRoomTemp => mSortedStatesBelowRoomTemp;

	[XmlIgnore]
	public List<LabState> pSortedStatesAboveRoomTemp => mSortedStatesAboveRoomTemp;

	public LabItemCategory pCategory => mCategory;

	public static LabItemCategory GetMappedItemCategory(string inCategory)
	{
		if (string.IsNullOrEmpty(inCategory))
		{
			return LabItemCategory.NONE;
		}
		return inCategory switch
		{
			"LIQUID" => LabItemCategory.LIQUID, 
			"SOLID" => LabItemCategory.SOLID, 
			"SOLIDPOWDER" => LabItemCategory.SOLID_POWDER, 
			"LIQUID_COMBUSTIBLE" => LabItemCategory.LIQUID_COMBUSTIBLE, 
			"SOLID_COMBUSTIBLE" => LabItemCategory.SOLID_COMBUSTIBLE, 
			"VOLATILE" => LabItemCategory.VOLATILE, 
			_ => LabItemCategory.NONE, 
		};
	}

	public LabState GetState(string inStateName)
	{
		if (string.IsNullOrEmpty(inStateName) || States == null)
		{
			return null;
		}
		LabState[] states = States;
		foreach (LabState labState in states)
		{
			if (labState != null && labState.Name == inStateName)
			{
				return labState;
			}
		}
		return null;
	}

	public void Initialize(InitializedCallback inCallback, float inStartTemperature)
	{
		mStartTemperature = inStartTemperature;
		mInitializedCount = 0;
		mInitializeCallback = (InitializedCallback)Delegate.Combine(mInitializeCallback, inCallback);
		mDefaultRotation = new Vector3(DefaultRotationX, DefaultRotationY, DefaultRotationZ);
		mScale = new Vector3(ScaleX, ScaleY, ScaleZ);
		mInitializedCount++;
		if (mSortedStates == null)
		{
			mSortedStates = new List<LabState>();
		}
		mSortedStates.Clear();
		if (mSortedStatesBelowRoomTemp == null)
		{
			mSortedStatesBelowRoomTemp = new List<LabState>();
		}
		mSortedStatesBelowRoomTemp.Clear();
		if (mSortedStatesAboveRoomTemp == null)
		{
			mSortedStatesAboveRoomTemp = new List<LabState>();
		}
		mSortedStatesAboveRoomTemp.Clear();
		if (States != null)
		{
			LabState[] states = States;
			foreach (LabState labState in states)
			{
				if (labState != null)
				{
					if (labState.Temperature - LabData.pInstance.RoomTemperature < 0f)
					{
						mSortedStatesBelowRoomTemp.Add(labState);
					}
					else
					{
						mSortedStatesAboveRoomTemp.Add(labState);
					}
				}
			}
		}
		mSortedStatesBelowRoomTemp.Sort(new SortStateTemperature(inAscending: false));
		mSortedStatesAboveRoomTemp.Sort(new SortStateTemperature(inAscending: true));
		if (States != null)
		{
			mSortedStates.AddRange(States);
		}
		mSortedStates.Sort(new SortStateTemperature(inAscending: true));
		if (mSortedStates.Count == 0)
		{
			mInitializedCount++;
			OnStateInitialized(null);
		}
		else
		{
			for (int j = 0; j < mSortedStates.Count; j++)
			{
				if (mSortedStates[j] == null)
				{
					continue;
				}
				if (mSortedStates[j].ReboundTemperature <= mSortedStates[j].Temperature)
				{
					if (j == mSortedStates.Count - 1 || mSortedStates[j + 1] == null)
					{
						mSortedStates[j].Initialize(mSortedStates[j].Temperature, MaxTemperature, this);
					}
					else
					{
						mSortedStates[j].Initialize(mSortedStates[j].Temperature, mSortedStates[j + 1].Temperature, this);
					}
				}
				else if (j == 0 || mSortedStates[j - 1] == null)
				{
					mSortedStates[j].Initialize(MinTemperature, mSortedStates[j].Temperature, this);
				}
				else
				{
					mSortedStates[j].Initialize(mSortedStates[j - 1].Temperature, mSortedStates[j].Temperature, this);
				}
			}
		}
		if (!string.IsNullOrEmpty(CoolingDownSoundURL))
		{
			string[] array = CoolingDownSoundURL.Split('/');
			if (array.Length == 3)
			{
				RsResourceManager.LoadAssetFromBundle(array[0] + "/" + array[1], array[2], OnCoolingDownLoaded, typeof(AudioClip));
			}
		}
		if (DefaultAdditions == null || DefaultAdditions.Length == 0)
		{
			mInitializedCount++;
			if (pInitialized && mInitializeCallback != null)
			{
				mInitializeCallback();
			}
		}
		else
		{
			mDefaultObjectLoadedCount = 0;
			LabAdditionObjectData[] defaultAdditions = DefaultAdditions;
			foreach (LabAdditionObjectData labAdditionObjectData in defaultAdditions)
			{
				bool flag = false;
				if (labAdditionObjectData != null)
				{
					flag = true;
					labAdditionObjectData.Initialize(OnAdditionLoaded);
				}
				if (flag)
				{
					continue;
				}
				mDefaultObjectLoadedCount++;
				if (mDefaultObjectLoadedCount >= DefaultAdditions.Length)
				{
					mInitializedCount++;
					if (pInitialized && mInitializeCallback != null)
					{
						mInitializeCallback();
					}
				}
			}
		}
		if (DefaultShaderProperties != null && DefaultShaderProperties.Length != 0)
		{
			for (int k = 0; k < DefaultShaderProperties.Length; k++)
			{
				if (DefaultShaderProperties[k] != null && DefaultShaderProperties[k].Type == LabShaderDataType.Texture && !string.IsNullOrEmpty(DefaultShaderProperties[k].Name) && !string.IsNullOrEmpty(DefaultShaderProperties[k].Value))
				{
					if (mShaderTextureLoader == null)
					{
						mShaderTextureLoader = new LabTextureLoader();
					}
					mShaderTextureLoader.AddTextureData(DefaultShaderProperties[k].Name, DefaultShaderProperties[k].Value);
				}
			}
			if (mShaderTextureLoader != null)
			{
				mShaderTextureLoader.Load(OnShaderTextureLoaded);
			}
		}
		else
		{
			mInitializedCount++;
			if (pInitialized && mInitializeCallback != null)
			{
				mInitializeCallback();
			}
		}
		mCategory = GetMappedItemCategory(Category);
	}

	private void OnShaderTextureLoaded(bool inSuccess)
	{
		if (inSuccess)
		{
			mInitializedCount++;
			if (pInitialized && mInitializeCallback != null)
			{
				mInitializeCallback();
			}
		}
	}

	public Texture GetShaderTexture(string inName)
	{
		if (!string.IsNullOrEmpty(inName) && mShaderTextureLoader != null)
		{
			return mShaderTextureLoader.GetTexture(inName);
		}
		return null;
	}

	private void OnCoolingDownLoaded(string inURL, RsResourceLoadEvent inEvent, float inProgress, object inObject, object inUserData)
	{
		if (inEvent == RsResourceLoadEvent.COMPLETE && mCoolingDownClip == null)
		{
			mCoolingDownClip = (AudioClip)inObject;
		}
	}

	private void OnAdditionLoaded(LabAdditionObjectData inData)
	{
		mDefaultObjectLoadedCount++;
		if (mDefaultObjectLoadedCount >= DefaultAdditions.Length)
		{
			mInitializedCount++;
			if (pInitialized && mInitializeCallback != null)
			{
				mInitializeCallback();
			}
		}
	}

	public void OnStateInitialized(LabState inState)
	{
		bool flag = true;
		if (mSortedStates != null && mSortedStates.Count > 0)
		{
			for (int i = 0; i < mSortedStates.Count; i++)
			{
				if (mSortedStates[i] == null || !mSortedStates[i].pInitialized)
				{
					flag = false;
					break;
				}
			}
		}
		if (flag)
		{
			mInitializedCount++;
			if (pInitialized && mInitializeCallback != null)
			{
				mInitializeCallback();
			}
		}
	}

	public LabInteraction GetInteraction(string inTestObjectName)
	{
		if (string.IsNullOrEmpty(inTestObjectName) || Interactions == null || Interactions.Length == 0)
		{
			return null;
		}
		LabInteraction[] interactions = Interactions;
		foreach (LabInteraction labInteraction in interactions)
		{
			if (labInteraction != null && labInteraction.Name == inTestObjectName)
			{
				return labInteraction;
			}
		}
		return null;
	}

	public bool CanInteractWith(string inTestObjectName)
	{
		if (string.IsNullOrEmpty(inTestObjectName) || Interactions == null || Interactions.Length == 0)
		{
			return false;
		}
		LabInteraction[] interactions = Interactions;
		foreach (LabInteraction labInteraction in interactions)
		{
			if (labInteraction != null && labInteraction.Name == inTestObjectName)
			{
				return true;
			}
		}
		return false;
	}

	public void Unload()
	{
		if (!string.IsNullOrEmpty(CoolingDownSoundURL) && CoolingDownSoundURL.Split('/').Length == 3)
		{
			RsResourceManager.Unload(CoolingDownSoundURL);
		}
		if (mCoolingDownClip != null)
		{
			UnityEngine.Object.Destroy(mCoolingDownClip);
		}
		mCoolingDownClip = null;
		if (mSortedStates == null || mSortedStates.Count == 0)
		{
			return;
		}
		foreach (LabState mSortedState in mSortedStates)
		{
			mSortedState.Unload();
		}
	}

	public string GetPropertyValueForKey(string inKeyName)
	{
		if (Properties != null)
		{
			for (int i = 0; i < Properties.Length; i++)
			{
				if (Properties[i].Key == inKeyName)
				{
					return Properties[i].Value;
				}
			}
		}
		return "";
	}
}
