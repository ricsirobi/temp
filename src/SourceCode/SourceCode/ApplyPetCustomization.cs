using System.Collections.Generic;
using UnityEngine;

public class ApplyPetCustomization
{
	private string PRICOLOR = "_PrimaryColor";

	private string SECCOLOR = "_SecondaryColor";

	private string TERCOLOR = "_TertiaryColor";

	private const string SADDLE = "Saddle";

	private string mSkinApplied = string.Empty;

	private RaisedPetData mRaisedPetData;

	private Dictionary<string, SkinnedMeshRenderer> mRendererMap = new Dictionary<string, SkinnedMeshRenderer>();

	private SanctuaryPetAccData[] mSancPetAccData;

	private GameObject mPetObject;

	public void InitPetCustomization(RaisedPetData data, GameObject go, bool swapMesh = false)
	{
		mRaisedPetData = data;
		mPetObject = go;
		if (mRaisedPetData == null)
		{
			return;
		}
		SkinnedMeshRenderer[] componentsInChildren = mPetObject.GetComponentsInChildren<SkinnedMeshRenderer>();
		foreach (SkinnedMeshRenderer skinnedMeshRenderer in componentsInChildren)
		{
			mRendererMap[skinnedMeshRenderer.name] = skinnedMeshRenderer;
		}
		if (swapMesh)
		{
			LoadMeshData();
		}
		bool flag = false;
		if (mRaisedPetData.Accessories != null && mRaisedPetData.Accessories.Length != 0)
		{
			mSancPetAccData = new SanctuaryPetAccData[mRaisedPetData.Accessories.Length];
			int num = 0;
			RaisedPetAccessory[] accessories = mRaisedPetData.Accessories;
			foreach (RaisedPetAccessory raisedPetAccessory in accessories)
			{
				if (raisedPetAccessory.Type != "Hat" && !string.IsNullOrEmpty(raisedPetAccessory.Geometry))
				{
					mSancPetAccData[num] = new SanctuaryPetAccData(raisedPetAccessory, OnAccReady);
					mSancPetAccData[num].LoadResource();
					if (RaisedPetData.GetAccessoryType(mSancPetAccData[num].mAccData.Type) == RaisedPetAccType.Materials)
					{
						flag = true;
					}
					num++;
				}
			}
		}
		if (mRaisedPetData.GetAccessory(RaisedPetAccType.Materials) == null)
		{
			UpdateMaterials(null);
		}
		if (!flag)
		{
			ApplyGlow();
		}
	}

	private void OnAccReady(SanctuaryPetAccData adata)
	{
		SetAccessory(RaisedPetData.GetAccessoryType(adata.mAccData.Type), adata.mObj, adata.mTex);
	}

	private void SetAccessory(RaisedPetAccType atype, GameObject obj, Texture2D newTexture)
	{
		if (atype == RaisedPetAccType.Materials)
		{
			UpdateMaterials(obj);
			ApplyGlow();
		}
	}

	private void UpdateMaterials(GameObject materialsObject)
	{
		if (materialsObject != null)
		{
			DragonSkin component = materialsObject.GetComponent<DragonSkin>();
			if (component == null || component.name == mSkinApplied)
			{
				return;
			}
			SetSkinData(component);
		}
		UpdateShaders();
	}

	private void SetSkinData(DragonSkin dragonSkin)
	{
		Material[] array;
		Material[] array2;
		switch (mRaisedPetData.pStage)
		{
		case RaisedPetStage.BABY:
			array = dragonSkin._BabyMaterials;
			array2 = dragonSkin._BabyMaterials;
			break;
		case RaisedPetStage.TEEN:
			array = ((dragonSkin._TeenMaterials.Length == 0) ? dragonSkin._Materials : dragonSkin._TeenMaterials);
			array2 = ((dragonSkin._TeenLODMaterials.Length == 0) ? dragonSkin._LODMaterials : dragonSkin._TeenLODMaterials);
			break;
		case RaisedPetStage.TITAN:
			array = dragonSkin._TitanMaterials;
			array2 = dragonSkin._TitanLODMaterials;
			break;
		default:
			array = dragonSkin._Materials;
			array2 = dragonSkin._LODMaterials;
			break;
		}
		Mesh mesh = ((mRaisedPetData.pStage == RaisedPetStage.BABY) ? dragonSkin._BabyMesh : ((mRaisedPetData.pStage == RaisedPetStage.TEEN) ? ((!(dragonSkin._TeenMesh != null)) ? dragonSkin._Mesh : dragonSkin._TeenMesh) : ((mRaisedPetData.pStage != RaisedPetStage.TITAN) ? dragonSkin._Mesh : dragonSkin._TitanMesh)));
		foreach (string key in mRendererMap.Keys)
		{
			if (!dragonSkin.IsRendererAllowedToChange(key))
			{
				continue;
			}
			if (array != null && array.Length != 0)
			{
				if (key.Contains("LOD") && array2 != null && array2.Length != 0)
				{
					mRendererMap[key].materials = array2;
				}
				else
				{
					mRendererMap[key].materials = array;
				}
			}
			if (mesh != null)
			{
				mRendererMap[key].sharedMesh = mesh;
			}
		}
		mSkinApplied = dragonSkin.name;
		if (UtPlatform.IsEditor())
		{
			UtUtilities.ReAssignShader(mPetObject);
		}
	}

	private void UpdateShaders()
	{
		if (mRaisedPetData == null)
		{
			return;
		}
		if (mRaisedPetData.Colors == null || mRaisedPetData.Colors.Length < 3)
		{
			SetColorsFromMaterial();
		}
		Color color = mRaisedPetData.GetColor(0);
		Color color2 = mRaisedPetData.GetColor(1);
		Color color3 = mRaisedPetData.GetColor(2);
		foreach (SkinnedMeshRenderer value in mRendererMap.Values)
		{
			Material[] materials = value.materials;
			foreach (Material material in materials)
			{
				if (material.HasProperty(PRICOLOR))
				{
					material.SetColor(PRICOLOR, color);
				}
				if (material.HasProperty(SECCOLOR))
				{
					material.SetColor(SECCOLOR, color2);
				}
				if (material.HasProperty(TERCOLOR))
				{
					material.SetColor(TERCOLOR, color3);
				}
			}
		}
	}

	private void ApplyGlow()
	{
		if (GlowManager.pInstance != null && mRaisedPetData.IsGlowRunning())
		{
			GlowManager.pInstance.ApplyGlow(mRaisedPetData.pGlowEffect.GlowColor, mRaisedPetData.RaisedPetID, mPetObject, new List<string> { "Saddle" });
		}
	}

	private void SetColorsFromMaterial()
	{
		Color color = Color.white;
		Color color2 = Color.white;
		Color color3 = Color.white;
		foreach (SkinnedMeshRenderer value in mRendererMap.Values)
		{
			Material[] materials = value.materials;
			foreach (Material material in materials)
			{
				if (material.HasProperty(PRICOLOR))
				{
					color = material.GetColor(PRICOLOR);
				}
				if (material.HasProperty(SECCOLOR))
				{
					color2 = material.GetColor(SECCOLOR);
				}
				if (material.HasProperty(TERCOLOR))
				{
					color3 = material.GetColor(TERCOLOR);
				}
			}
			if (color != Color.white)
			{
				break;
			}
		}
		Color[] colors = new Color[3] { color, color2, color3 };
		mRaisedPetData.SetColors(colors);
	}

	private void LoadMeshData()
	{
		string text = "";
		SanctuaryPetTypeInfo sanctuaryPetTypeInfo = SanctuaryData.FindSanctuaryPetTypeInfo(mRaisedPetData.PetTypeID);
		if (sanctuaryPetTypeInfo == null)
		{
			return;
		}
		int ageIndex = RaisedPetData.GetAgeIndex(mRaisedPetData.pStage);
		SantuayPetResourceInfo[] petResList = sanctuaryPetTypeInfo._AgeData[ageIndex]._PetResList;
		foreach (SantuayPetResourceInfo santuayPetResourceInfo in petResList)
		{
			if (santuayPetResourceInfo._Gender == mRaisedPetData.Gender)
			{
				text = santuayPetResourceInfo._Prefab;
				break;
			}
		}
		string[] array = text.Split('/');
		if (array.Length == 3)
		{
			RsResourceManager.LoadAssetFromBundle(array[0] + "/" + array[1], array[2], OnMeshLoaded, typeof(GameObject));
		}
	}

	private void OnMeshLoaded(string inURL, RsResourceLoadEvent inLoadEvent, float inProgress, object inObject, object inUserData)
	{
		KAUICursorManager.SetDefaultCursor("Arrow");
		switch (inLoadEvent)
		{
		case RsResourceLoadEvent.COMPLETE:
		{
			if (inObject == null)
			{
				break;
			}
			GameObject gameObject = Object.Instantiate(inObject as GameObject);
			SkinnedMeshRenderer[] componentsInChildren = gameObject.GetComponentsInChildren<SkinnedMeshRenderer>();
			foreach (SkinnedMeshRenderer skinnedMeshRenderer in componentsInChildren)
			{
				if (mRendererMap.ContainsKey(skinnedMeshRenderer.name))
				{
					mRendererMap[skinnedMeshRenderer.name].materials = skinnedMeshRenderer.materials;
					mRendererMap[skinnedMeshRenderer.name].sharedMesh = skinnedMeshRenderer.sharedMesh;
				}
			}
			Object.Destroy(gameObject);
			mSkinApplied = string.Empty;
			break;
		}
		case RsResourceLoadEvent.ERROR:
			Debug.LogError("Failed to load Avatar Equipment....");
			break;
		}
	}
}
