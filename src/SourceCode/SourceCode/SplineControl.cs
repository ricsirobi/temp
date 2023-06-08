using System;
using UnityEngine;

public class SplineControl : KAMonoBase
{
	public Transform SplineObject;

	public bool _Draw;

	public float[] ControlTime;

	public bool Looping;

	public bool ConstantSpeed = true;

	public bool AlignTangent;

	public float CurrentPos;

	public float Speed;

	public bool SmoothMovement = true;

	public float SmoothFactor = 0.1f;

	public bool GroundCheck;

	public float GroundCheckStartHeight = 2f;

	public float GroundCheckDist = 20f;

	public bool FlipDirOnBackward = true;

	public float LinearLength;

	public bool IsLocal;

	public Spline mSpline;

	[NonSerialized]
	public bool mEndReached;

	private void SortTransformsByName(ref Transform[] t)
	{
		int num = t.Length;
		int num2 = 0;
		int num3 = 0;
		Transform transform = null;
		for (num2 = 0; num2 < num; num2++)
		{
			for (num3 = num2 + 1; num3 < num; num3++)
			{
				if (string.Compare(t[num3].name, t[num2].name) < 0)
				{
					transform = t[num2];
					t[num2] = t[num3];
					t[num3] = transform;
				}
			}
		}
	}

	public void ResetSpline()
	{
		GameObject gameObject = SplineObject.gameObject;
		if (SplineObject.childCount <= 0)
		{
			return;
		}
		mSpline = new Spline(SplineObject.childCount, Looping, ConstantSpeed, AlignTangent, hasQ: true);
		int num = 0;
		Transform[] t = new Transform[SplineObject.childCount];
		foreach (Transform item in gameObject.transform)
		{
			t[num] = item;
			num++;
		}
		SortTransformsByName(ref t);
		num = 0;
		Transform transform2 = null;
		if (IsLocal && t.Length != 0)
		{
			Transform obj = t[0];
			Transform parent = obj.parent;
			transform2 = obj;
			while (parent != null && parent != base.transform.parent)
			{
				transform2 = parent;
				parent = parent.parent;
			}
			if (parent == base.transform.parent)
			{
				transform2.parent = null;
			}
			else
			{
				transform2 = null;
			}
		}
		Transform[] array = t;
		foreach (Transform transform3 in array)
		{
			float num2 = 0f;
			num2 = ((ControlTime != null && ControlTime.Length > num) ? ControlTime[num] : ((!Looping) ? ((float)num / (float)(SplineObject.childCount - 1)) : ((num != 0) ? ((float)num / (float)SplineObject.childCount) : 1f)));
			mSpline.SetControlPoint(num, transform3.position, transform3.rotation, num2);
			num++;
		}
		if (transform2 != null)
		{
			transform2.parent = base.transform.parent;
		}
		mSpline.RecalculateSpline();
		LinearLength = mSpline.mLinearLength;
		mEndReached = false;
	}

	public virtual void Start()
	{
		if (SplineObject != null && mSpline == null)
		{
			ResetSpline();
		}
	}

	public virtual void SetSpline(Spline sp)
	{
		mSpline = sp;
		CurrentPos = 0f;
		mEndReached = false;
		if (mSpline != null)
		{
			LinearLength = mSpline.mLinearLength;
		}
		else
		{
			LinearLength = 0f;
		}
	}

	public void ResetSplinePosition()
	{
		if (mSpline == null && SplineObject != null)
		{
			ResetSpline();
		}
		if (mSpline != null)
		{
			SetPosOnSpline(0f);
		}
	}

	public virtual void SetPosOnSpline(float p)
	{
		CurrentPos = p;
		if (mSpline == null)
		{
			return;
		}
		CurrentPos = mSpline.GetPosQuatByDist(CurrentPos, out var pos, out var quat);
		if (Speed < 0f && FlipDirOnBackward && (mSpline.mHasQ || mSpline.mAlignTangent))
		{
			quat = Quaternion.Euler(quat.eulerAngles.x, quat.eulerAngles.y + 180f, quat.eulerAngles.z);
		}
		if (SmoothMovement)
		{
			if (IsLocal)
			{
				base.transform.localRotation = Quaternion.Slerp(quat, base.transform.rotation, 1f - SmoothFactor);
			}
			else
			{
				base.transform.rotation = Quaternion.Slerp(quat, base.transform.rotation, 1f - SmoothFactor);
			}
		}
		else if (mSpline.mHasQ || mSpline.mAlignTangent)
		{
			if (IsLocal)
			{
				base.transform.localRotation = quat;
			}
			else
			{
				base.transform.rotation = quat;
			}
		}
		if (IsLocal)
		{
			pos = base.transform.parent.localToWorldMatrix.MultiplyPoint3x4(pos);
		}
		if (GroundCheck)
		{
			pos.y += GroundCheckStartHeight;
			UtUtilities.GetGroundHeight(pos, GroundCheckDist, out var groundHeight);
			pos.y = groundHeight;
		}
		ValidateVector(ref pos);
		base.transform.position = pos;
	}

	public void ValidateVector(ref Vector3 ioVec)
	{
		bool flag = false;
		if (ioVec.x == float.PositiveInfinity)
		{
			ioVec.x = float.MaxValue;
			flag = true;
		}
		if (ioVec.y == float.PositiveInfinity)
		{
			ioVec.y = float.MaxValue;
			flag = true;
		}
		if (ioVec.z == float.PositiveInfinity)
		{
			ioVec.z = float.MaxValue;
			flag = true;
		}
		if (ioVec.x == float.NegativeInfinity)
		{
			ioVec.x = float.MinValue;
			flag = true;
		}
		if (ioVec.y == float.NegativeInfinity)
		{
			ioVec.y = float.MinValue;
			flag = true;
		}
		if (ioVec.z == float.NegativeInfinity)
		{
			ioVec.z = float.MinValue;
			flag = true;
		}
		if (flag)
		{
			UtDebug.Log("~~~~~~~~~~~~~~~~~~~~~~~~ Vector modified to eliminate infinity references: " + ioVec.ToString(), 200);
		}
	}

	public bool MoveOnSpline(float dist)
	{
		if (mSpline == null)
		{
			return false;
		}
		CurrentPos += dist;
		if (dist > 0f)
		{
			if (CurrentPos >= mSpline.mLinearLength)
			{
				if (Looping)
				{
					while (CurrentPos >= mSpline.mLinearLength)
					{
						CurrentPos -= mSpline.mLinearLength;
					}
				}
				else
				{
					mEndReached = true;
				}
			}
		}
		else if (CurrentPos <= 0f)
		{
			if (Looping)
			{
				while ((double)CurrentPos <= 0.0)
				{
					CurrentPos += mSpline.mLinearLength;
				}
			}
			else
			{
				mEndReached = true;
			}
		}
		SetPosOnSpline(CurrentPos);
		return mEndReached;
	}

	public virtual void OnDrawGizmos()
	{
		if (mSpline != null)
		{
			if (_Draw)
			{
				Color c = Color.blue;
				Matrix4x4 tmat = Matrix4x4.identity;
				if (IsLocal && base.transform.parent != null)
				{
					c = Color.yellow;
					tmat = base.transform.parent.transform.localToWorldMatrix;
				}
				if (mSpline.mLinearLength > 10f)
				{
					mSpline.OnDrawGizmos((int)(mSpline.mLinearLength * 2f), tmat, c);
				}
				else
				{
					mSpline.OnDrawGizmos(100, tmat, c);
				}
			}
		}
		else if (SplineObject != null)
		{
			Start();
		}
	}

	public virtual void Update()
	{
		if (mSpline != null)
		{
			if (Speed != 0f)
			{
				MoveOnSpline(Time.deltaTime * Speed);
			}
			else
			{
				mEndReached = false;
			}
		}
	}
}
