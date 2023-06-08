using System.Collections.Generic;
using UnityEngine;

public class LabSpectrumCrucible : LabCrucible
{
	private LabSpectrum mSpectrum;

	private Texture mOrginalSpectrumTexture;

	private Texture mOrginalSpectrumRaysTexture;

	private float mWarmingConstant;

	private Vector3 mSnapPosition = Vector3.zero;

	public LabSpectrumCrucible(ScientificExperiment inManager, LabSpectrum spectrum, float warmingConstant, Vector3 snapPosition)
		: base(inManager)
	{
		mSpectrum = spectrum;
		mOrginalSpectrumTexture = mSpectrum._Spectrum.material.mainTexture;
		mOrginalSpectrumRaysTexture = mSpectrum._SpectrumRays.material.mainTexture;
		mWarmingConstant = warmingConstant;
		mSnapPosition = snapPosition;
	}

	protected override void AddTestItemReal(LabTestObject inItemToAdd, ItemPositionOption inPositionOption, Vector3 inPosition, Quaternion inRotation, Vector3 inScale, LabItem inParentItem, List<TestItemLoader.ReplaceItemDetails> inReplaceObjects, CrucibleItemAddedCallback inCallback)
	{
		RemoveAllTestItems();
		base.AddTestItemReal(inItemToAdd, inPositionOption, inPosition, inRotation, inScale, inParentItem, inReplaceObjects, inCallback);
		if (inItemToAdd != null)
		{
			inItemToAdd.transform.position = new Vector3(mSnapPosition.x, inItemToAdd.transform.position.y, mSnapPosition.z);
			ApplySpectrumTextures(inItemToAdd);
			mManager._MainUI._SetCursorOnContextAction = false;
			inItemToAdd.pState = null;
		}
	}

	protected override void RecalculateTemperatureMultipliers()
	{
		base.RecalculateTemperatureMultipliers();
		mCurrentHeatMultiplier = mWarmingConstant;
	}

	protected override bool CanDragonGetExcited(LabTask task)
	{
		return task.PlayExciteAlways;
	}

	public override void Reset()
	{
		base.Reset();
		ResetCrucible();
	}

	public void ApplySpectrumTextures(LabTestObject inItem)
	{
		Texture shaderTexture = inItem.pTestItem.GetShaderTexture("Spectrum");
		Texture shaderTexture2 = inItem.pTestItem.GetShaderTexture("SpectrumRays");
		if (shaderTexture != null && shaderTexture2 != null)
		{
			mSpectrum._Spectrum.material.mainTexture = shaderTexture;
			mSpectrum._SpectrumRays.material.mainTexture = shaderTexture2;
		}
	}

	public override void RemoveTestItem(LabTestObject inTestItem, bool inDestroy = true)
	{
		base.RemoveTestItem(inTestItem, inDestroy);
		ResetCrucible();
	}

	public void ResetCrucible()
	{
		mSpectrum._Spectrum.material.mainTexture = mOrginalSpectrumTexture;
		mSpectrum._SpectrumRays.material.mainTexture = mOrginalSpectrumRaysTexture;
	}
}
