using System.Collections.Generic;
using UnityEngine;

public class ElementMatchTerribleTerror : MonoBehaviour
{
	public GameObject _MyTarget;

	public float _WalkSpeed = 2f;

	private SplineControl mSpline;

	private GameObject mAnimObject;

	private bool isActive;

	private ElementMatchGame.ElementType mType;

	private Spline mSplineObj;

	private float mTargetAngle;

	private Vector3 eulers = Vector3.zero;

	public bool pReachedEnd
	{
		get
		{
			if (null != mSpline)
			{
				return mSpline.mEndReached;
			}
			return false;
		}
	}

	private void Start()
	{
		Init();
	}

	private void Init()
	{
		mAnimObject = base.transform.Find("TerribleTerror").gameObject;
		Animation component = mAnimObject.GetComponent<Animation>();
		component.wrapMode = WrapMode.Loop;
		if (null != _MyTarget)
		{
			mSpline = _MyTarget.GetComponent<SplineControl>();
		}
		else
		{
			Debug.LogError("ElementMatchTerribleTerror target not assigned");
		}
		component.Stop();
		mSpline.gameObject.SetActive(value: false);
		base.gameObject.SetActive(value: true);
	}

	public void Show(List<Transform> ctrlPts, Material newMaterial, ElementMatchGame.ElementType tileType)
	{
		Init();
		mSpline.gameObject.SetActive(value: true);
		base.gameObject.SetActive(value: true);
		mAnimObject.GetComponent<Animation>().Play("Walk");
		isActive = true;
		SkinnedMeshRenderer componentInChildren = GetComponentInChildren<SkinnedMeshRenderer>();
		if (null != componentInChildren)
		{
			Material material = componentInChildren.material;
			material = newMaterial;
			componentInChildren.material = material;
		}
		mSplineObj = new Spline(ctrlPts.Count + 2, looping: false, constSpeed: true, alignTangent: true, hasQ: false);
		int num = 0;
		ElementMatchGame elementMatchGame = (ElementMatchGame)TileMatchPuzzleGame.pInstance;
		mSplineObj.SetControlPoint(num, elementMatchGame._TerrorStart.position - Vector3.forward * 2f, Quaternion.identity, 0f);
		num++;
		foreach (Transform ctrlPt in ctrlPts)
		{
			mSplineObj.SetControlPoint(num, ctrlPt.position - Vector3.forward * 2f, Quaternion.identity, 0f);
			num++;
		}
		mSplineObj.SetControlPoint(num, elementMatchGame._TerrorEnd.position - Vector3.forward * 2f, Quaternion.identity, 0f);
		num++;
		mSplineObj.RecalculateSpline();
		mSpline.SetSpline(mSplineObj);
		mSpline.Looping = false;
		mSpline.ResetSplinePosition();
		mType = tileType;
	}

	public static float AngleSigned(Vector3 v1, Vector3 v2, Vector3 n)
	{
		return Mathf.Atan2(Vector3.Dot(n, Vector3.Cross(v1, v2)), Vector3.Dot(v1, v2)) * 57.29578f;
	}

	public void Update()
	{
		if (isActive && mSpline != null)
		{
			mSpline.MoveOnSpline(Time.deltaTime * _WalkSpeed);
			float num = _WalkSpeed * Time.deltaTime;
			base.transform.position = Vector3.MoveTowards(base.transform.position, _MyTarget.transform.position, num);
			mSpline.mSpline.GetSectionTimeInfo(mSpline.CurrentPos / mSpline.LinearLength, out var _, out var _, out var n2);
			Vector3 v = mSpline.mSpline.mNodes[n2].mPoint - base.transform.position;
			v.z = 0f;
			Vector3 up = base.transform.up;
			up.z = 0f;
			mTargetAngle = AngleSigned(up, v, Vector3.forward);
			eulers = new Vector3(0f, 0f, Mathf.MoveTowardsAngle(eulers.z, mTargetAngle, num * 20f));
			base.transform.eulerAngles = eulers;
			if (mSpline.mEndReached)
			{
				mSpline.gameObject.SetActive(value: false);
				isActive = false;
				base.gameObject.SetActive(value: false);
				mAnimObject.GetComponent<Animation>().Stop();
				ElementMatchGame elementMatchGame = (ElementMatchGame)TileMatchPuzzleGame.pInstance;
				base.transform.position = elementMatchGame._TerrorStart.position;
				base.transform.rotation = Quaternion.identity;
			}
		}
	}

	private void OnTriggerEnter(Collider other)
	{
		ElementMatchTilePiece component = other.transform.GetComponent<ElementMatchTilePiece>();
		if (null != component && mType == component._ElementType)
		{
			component.gameObject.SetActive(value: false);
		}
	}
}
