using UnityEngine;

public class LabFireBlanket : LabTestObject
{
	public string _AnimSolidInCrucible;

	public string _AnimNoSolidInCrucible;

	protected override void OnObjectRelease(LabCrucible crucible)
	{
		base.OnObjectRelease(crucible);
		Animation component = GetComponent<Animation>();
		if (base.pCrucible.HasCategoryItem(LabItemCategory.SOLID) && !string.IsNullOrEmpty(_AnimSolidInCrucible))
		{
			component[_AnimSolidInCrucible].speed = 1f;
			component[_AnimSolidInCrucible].time = 0f;
			component.Play(_AnimSolidInCrucible);
		}
		else if (!string.IsNullOrEmpty(_AnimNoSolidInCrucible))
		{
			component[_AnimNoSolidInCrucible].speed = 1f;
			component[_AnimNoSolidInCrucible].time = 0f;
			component.Play(_AnimNoSolidInCrucible);
		}
	}

	protected override void OnObjectPickup()
	{
		base.OnObjectPickup();
		Animation component = GetComponent<Animation>();
		if (!string.IsNullOrEmpty(_AnimSolidInCrucible))
		{
			component[_AnimSolidInCrucible].speed = -5f;
			component[_AnimSolidInCrucible].time = component[_AnimSolidInCrucible].length;
			component.Play(_AnimSolidInCrucible);
		}
	}
}
