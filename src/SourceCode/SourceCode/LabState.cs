using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using UnityEngine;

[Serializable]
public class LabState
{
	[XmlElement(ElementName = "Name")]
	public string Name;

	[XmlElement(ElementName = "Temperature")]
	public float Temperature;

	[XmlElement(ElementName = "ObjectName")]
	public string[] ObjectNames;

	[XmlElement(ElementName = "FireObjectName")]
	public string FireObjectName;

	[XmlElement(ElementName = "ParticleSound")]
	public string ParticleSound;

	[XmlElement(ElementName = "ReboundTemperature")]
	public float ReboundTemperature;

	[XmlElement(ElementName = "SubstanceProperty")]
	public LabSubstanceProperty[] Properties;

	[XmlElement(ElementName = "ShaderName")]
	public string ShaderName;

	[XmlElement(ElementName = "LabShaderTexture")]
	public LabShaderProperty[] ShaderProperties;

	[XmlElement(ElementName = "ShaderInterpolation")]
	public LabShaderInterpolation[] Interpolations;

	[XmlElement(ElementName = "ScaleOnTransition")]
	public bool ScaleOnTransition;

	[XmlElement(ElementName = "ScaleOnTransitionTime")]
	public float ScaleOnTransitionTime;

	[XmlElement(ElementName = "ScaleOnTransitionTimeSec")]
	public float ScaleOnTransitionTimeSec;

	[XmlElement(ElementName = "PauseStateUpdate")]
	public bool PauseStateUpdate;

	[XmlElement(ElementName = "Animation")]
	public string Animation = string.Empty;

	private float mMaxTemperature;

	private float mMinTemperature;

	private bool mInitialized;

	[XmlIgnore]
	public LabTextureLoader mShaderTextureLoader;

	private List<GameObject> mAdditionObjects;

	private GameObject mFireObject;

	private AudioClip mParticleAudioClip;

	private LabItem mItem;

	private int mBundleUpdateCount;

	private int mParticleLoadingCount;

	[XmlIgnore]
	public float pMaxTemperature => mMaxTemperature;

	[XmlIgnore]
	public float pMinTemperature => mMinTemperature;

	[XmlIgnore]
	public List<GameObject> pAdditionObjects => mAdditionObjects;

	[XmlIgnore]
	public GameObject pFireObject => mFireObject;

	[XmlIgnore]
	public AudioClip pParticleAudioClip => mParticleAudioClip;

	[XmlIgnore]
	public bool pInitialized => mInitialized;

	public void Initialize(float inMinTemperature, float inMaxTemperature, LabItem inLabItem)
	{
		mItem = inLabItem;
		mMaxTemperature = inMaxTemperature;
		mMinTemperature = inMinTemperature;
		bool flag = false;
		if (ObjectNames != null && ObjectNames.Length != 0)
		{
			string[] objectNames = ObjectNames;
			foreach (string text in objectNames)
			{
				if (!string.IsNullOrEmpty(text) && text.Split('/').Length == 3)
				{
					mParticleLoadingCount++;
				}
			}
			objectNames = ObjectNames;
			foreach (string text2 in objectNames)
			{
				if (!string.IsNullOrEmpty(text2))
				{
					string[] array = text2.Split('/');
					if (array.Length == 3)
					{
						RsResourceManager.LoadAssetFromBundle(array[0] + "/" + array[1], array[2], OnAdditionPrefabLoaded, typeof(GameObject));
						flag = true;
					}
				}
			}
		}
		if (!flag)
		{
			mBundleUpdateCount++;
			BundleEvent();
		}
		bool flag2 = false;
		if (!string.IsNullOrEmpty(FireObjectName))
		{
			string[] array2 = FireObjectName.Split('/');
			if (array2.Length == 3)
			{
				RsResourceManager.LoadAssetFromBundle(array2[0] + "/" + array2[1], array2[2], OnFirePrefabLoaded, typeof(GameObject));
				flag2 = true;
			}
		}
		if (!flag2)
		{
			mBundleUpdateCount++;
			BundleEvent();
		}
		bool flag3 = false;
		if (!string.IsNullOrEmpty(ParticleSound))
		{
			string[] array3 = ParticleSound.Split('/');
			if (array3.Length == 3)
			{
				RsResourceManager.LoadAssetFromBundle(array3[0] + "/" + array3[1], array3[2], OnParticleSoundLoaded, typeof(AudioClip));
				flag3 = true;
			}
		}
		if (!flag3)
		{
			mBundleUpdateCount++;
			BundleEvent();
		}
		if (ShaderProperties == null || ShaderProperties.Length == 0)
		{
			mBundleUpdateCount++;
			BundleEvent();
			return;
		}
		for (int j = 0; j < ShaderProperties.Length; j++)
		{
			if (ShaderProperties[j] != null && ShaderProperties[j].Type == LabShaderDataType.Texture && !string.IsNullOrEmpty(ShaderProperties[j].Name) && !string.IsNullOrEmpty(ShaderProperties[j].Value))
			{
				if (mShaderTextureLoader == null)
				{
					mShaderTextureLoader = new LabTextureLoader();
				}
				mShaderTextureLoader.AddTextureData(ShaderProperties[j].Name, ShaderProperties[j].Value);
			}
		}
		if (mShaderTextureLoader != null)
		{
			mShaderTextureLoader.Load(OnShaderTextureLoaded);
		}
	}

	private void OnShaderTextureLoaded(bool inSuccess)
	{
		if (inSuccess)
		{
			mBundleUpdateCount++;
			BundleEvent();
		}
	}

	private void OnFirePrefabLoaded(string inURL, RsResourceLoadEvent inEvent, float inProgress, object inObject, object inUserData)
	{
		switch (inEvent)
		{
		case RsResourceLoadEvent.COMPLETE:
			if (mFireObject == null)
			{
				mFireObject = (GameObject)inObject;
			}
			mBundleUpdateCount++;
			BundleEvent();
			break;
		case RsResourceLoadEvent.ERROR:
			mBundleUpdateCount++;
			BundleEvent();
			break;
		}
	}

	private void OnAdditionPrefabLoaded(string inURL, RsResourceLoadEvent inEvent, float inProgress, object inObject, object inUserData)
	{
		switch (inEvent)
		{
		case RsResourceLoadEvent.COMPLETE:
			if (mAdditionObjects == null)
			{
				mAdditionObjects = new List<GameObject>();
			}
			mAdditionObjects.Add((GameObject)inObject);
			mParticleLoadingCount = Mathf.Max(0, mParticleLoadingCount - 1);
			if (mParticleLoadingCount == 0)
			{
				mBundleUpdateCount++;
				BundleEvent();
			}
			break;
		case RsResourceLoadEvent.ERROR:
			mParticleLoadingCount = Mathf.Max(0, mParticleLoadingCount - 1);
			if (mParticleLoadingCount == 0)
			{
				mBundleUpdateCount++;
				BundleEvent();
			}
			break;
		}
	}

	private void OnParticleSoundLoaded(string inURL, RsResourceLoadEvent inEvent, float inProgress, object inObject, object inUserData)
	{
		switch (inEvent)
		{
		case RsResourceLoadEvent.COMPLETE:
			if (mParticleAudioClip == null)
			{
				mParticleAudioClip = (AudioClip)inObject;
			}
			mBundleUpdateCount++;
			BundleEvent();
			break;
		case RsResourceLoadEvent.ERROR:
			mBundleUpdateCount++;
			BundleEvent();
			break;
		}
	}

	private void BundleEvent()
	{
		if (mBundleUpdateCount >= 4)
		{
			mInitialized = true;
			if (mItem != null)
			{
				mItem.OnStateInitialized(this);
			}
		}
	}

	public LabSubstanceProperty GetProperty(string inProperty)
	{
		if (Properties == null || Properties.Length == 0)
		{
			return null;
		}
		LabSubstanceProperty[] properties = Properties;
		foreach (LabSubstanceProperty labSubstanceProperty in properties)
		{
			if (labSubstanceProperty != null && labSubstanceProperty.Name == inProperty)
			{
				return labSubstanceProperty;
			}
		}
		return null;
	}

	public void Unload()
	{
		if (ObjectNames != null && ObjectNames.Length != 0)
		{
			string[] objectNames = ObjectNames;
			foreach (string text in objectNames)
			{
				if (!string.IsNullOrEmpty(text) && text.Split('/').Length == 3)
				{
					RsResourceManager.Unload(text);
				}
			}
		}
		if (mAdditionObjects != null && mAdditionObjects.Count > 0)
		{
			for (int j = 0; j < mAdditionObjects.Count; j++)
			{
				mAdditionObjects[j] = null;
			}
		}
		if (!string.IsNullOrEmpty(FireObjectName))
		{
			string[] array = FireObjectName.Split('/');
			if (array != null && array.Length == 3)
			{
				RsResourceManager.Unload(FireObjectName);
			}
		}
		mFireObject = null;
		if (!string.IsNullOrEmpty(ParticleSound) && ParticleSound.Split('/').Length == 3)
		{
			RsResourceManager.Unload(ParticleSound);
		}
		mParticleAudioClip = null;
	}

	public Texture GetShaderTexture(string inTextureName)
	{
		if (mShaderTextureLoader != null)
		{
			return mShaderTextureLoader.GetTexture(inTextureName);
		}
		return null;
	}
}
