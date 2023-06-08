using UnityEngine;
using UnityEngine.Rendering;

public class CreateShadows : KAMonoBase
{
	public Transform shadowPrefab;

	public bool adjustRotation = true;

	public float shadowScaleFactor = 0.4572f;

	public bool _GenerateShadowsOnStart;

	public bool _UpdateShadowPosition;

	private bool mShadowsOn;

	private void Start()
	{
		if (_GenerateShadowsOnStart)
		{
			GenerateShadows(_GenerateShadowsOnStart);
		}
	}

	private void Update()
	{
		if (_UpdateShadowPosition && mShadowsOn)
		{
			UpdateShadowPositions();
		}
	}

	public void TurnOnShadow()
	{
		GenerateShadows(onStart: true);
	}

	private void GenerateShadows(bool onStart)
	{
		if (mShadowsOn)
		{
			return;
		}
		int num = 16384;
		num = ~num;
		Transform transform = base.transform;
		mShadowsOn = true;
		RaycastHit hitInfo = default(RaycastHit);
		if (Physics.Raycast(transform.position, Vector3.down, out hitInfo, 5f, num))
		{
			Transform transform2 = Object.Instantiate(shadowPrefab, transform.position, Quaternion.identity);
			transform2.name = "CollectibleShadow";
			transform2.position = hitInfo.point + Vector3.up * 0.05f;
			transform2.rotation = Quaternion.LookRotation(Vector3.forward, hitInfo.normal);
			transform2.RotateAround(transform2.position, hitInfo.normal, transform.eulerAngles.y);
			transform2.parent = transform;
			AdjustShadow obj = transform2.GetComponent(typeof(AdjustShadow)) as AdjustShadow;
			obj.initPosition = transform2.position;
			obj.initRotation = transform2.rotation;
			obj.normal = hitInfo.normal;
			obj.parentInitRotation = transform.eulerAngles;
			obj.shadowScaleFactor = shadowScaleFactor;
			obj.adjustRotation = adjustRotation;
			if (base.renderer != null && !base.renderer.enabled)
			{
				transform2.GetComponent<Renderer>().enabled = false;
			}
		}
		Component[] componentsInChildren = base.transform.GetComponentsInChildren(typeof(Renderer));
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			((Renderer)componentsInChildren[i]).shadowCastingMode = ShadowCastingMode.Off;
		}
	}

	private void UpdateShadowPositions()
	{
		int num = 16384;
		num = ~num;
		Transform transform = base.transform;
		if (transform.gameObject.name != "CollectibleShadow")
		{
			RaycastHit hitInfo = default(RaycastHit);
			if (Physics.Raycast(transform.position, Vector3.down, out hitInfo, 5f, num))
			{
				Transform transform2 = transform.Find("CollectibleShadow");
				transform2.position = hitInfo.point + Vector3.up * 0.05f;
				(transform2.GetComponent(typeof(AdjustShadow)) as AdjustShadow).initPosition = transform2.position;
			}
		}
	}
}
