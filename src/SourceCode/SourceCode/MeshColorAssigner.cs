using System;
using System.Collections;
using UnityEngine;

public class MeshColorAssigner : MonoBehaviour
{
	[Serializable]
	public class ColorData
	{
		public int Index;

		public string Key;

		public Color DefaultColor;
	}

	public MeshRenderer _Mesh;

	public string _MaterialName = "";

	public ColorData[] _ColorData;

	public int[] _SkipForTypes;

	private ObStatus mObStatus;

	private Material mMaterial;

	private void Start()
	{
		if (_Mesh != null)
		{
			mMaterial = Array.Find(_Mesh.materials, (Material t) => t.name.StartsWith(_MaterialName));
		}
		mObStatus = base.gameObject.GetComponent<ObStatus>();
		StartCoroutine(RegisterOnPetChanged());
		if (SanctuaryManager.pCurPetData == null)
		{
			SetReadyStaus(status: true);
		}
	}

	private void OnDestroy()
	{
		if (SanctuaryManager.pInstance != null)
		{
			SanctuaryManager.pInstance.OnPetChanged -= SetColor;
		}
	}

	private IEnumerator RegisterOnPetChanged()
	{
		while (SanctuaryManager.pInstance == null)
		{
			yield return null;
		}
		if (SanctuaryManager.pInstance != null)
		{
			SanctuaryManager.pInstance.OnPetChanged += SetColor;
			if ((bool)SanctuaryManager.pCurPetInstance)
			{
				SetColor(SanctuaryManager.pCurPetInstance);
			}
		}
	}

	private void SetColor(SanctuaryPet pet)
	{
		bool flag = Array.Find(_SkipForTypes, (int t) => t == pet.pData.PetTypeID) != 0;
		ColorData[] colorData = _ColorData;
		foreach (ColorData colorData2 in colorData)
		{
			if (mMaterial.HasProperty(colorData2.Key))
			{
				mMaterial.SetColor(colorData2.Key, flag ? colorData2.DefaultColor : pet.pData.GetColor(colorData2.Index));
			}
		}
		SetReadyStaus(status: true);
	}

	private void SetReadyStaus(bool status)
	{
		if (mObStatus != null)
		{
			mObStatus.pIsReady = status;
		}
	}
}
