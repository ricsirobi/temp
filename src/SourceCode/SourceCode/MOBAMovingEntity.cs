using UnityEngine;

public class MOBAMovingEntity : MOBAEntity
{
	[HideInInspector]
	[Replicated(1f)]
	public Vector3 rPos = Vector3.zero;

	[HideInInspector]
	[Replicated(5f)]
	public float rFor;

	[HideInInspector]
	[Replicated(0.25f)]
	public float rVel;

	public bool _Grounded = true;

	private float mPositionTime;

	private Quaternion mVisualRotation;

	public override void Init()
	{
		rPos = base.transform.position;
		rFor = base.transform.rotation.eulerAngles.y;
		mVisualRotation = base.transform.rotation;
		base.Init();
	}

	protected override void EntityUpdate(bool bIsAuthority)
	{
		mPositionTime += Time.deltaTime;
		Vector3 position = base.transform.position;
		if (bIsAuthority)
		{
			base.transform.localRotation = Quaternion.Euler(0f, rFor, 0f);
			rPos = position + base.transform.forward * rVel * Time.deltaTime;
		}
		float y = base.transform.rotation.eulerAngles.y;
		y = Mathf.LerpAngle(y, rFor, Time.deltaTime);
		base.transform.localRotation = Quaternion.Euler(0f, y, 0f);
		position += (rPos - position).normalized * rVel * Time.deltaTime;
		if (_Grounded && UtUtilities.GetGroundHeight(position + new Vector3(0f, 2f, 0f), 10f, out var groundHeight, out var normal) != null)
		{
			position.y = groundHeight;
			base.transform.position = position;
			Vector3 forward = base.transform.forward;
			if (Vector3.Dot(forward, normal) < 1f)
			{
				forward = Vector3.Cross(Vector3.Cross(normal, forward), normal).normalized;
				Quaternion b = Quaternion.LookRotation(forward, normal);
				mVisualRotation = Quaternion.Slerp(mVisualRotation, b, Time.deltaTime * 5f);
				base.transform.rotation = mVisualRotation;
			}
		}
	}

	public override void EntityReset(bool bIsAuthority)
	{
		base.EntityReset(bIsAuthority);
		rVel = 0f;
	}

	protected override void OnVariableChanged(string varName)
	{
		base.OnVariableChanged(varName);
		if (varName == "rPos")
		{
			mPositionTime = 0f;
			if ((base.transform.position - rPos).magnitude > 5f)
			{
				base.transform.position = rPos;
			}
		}
	}
}
