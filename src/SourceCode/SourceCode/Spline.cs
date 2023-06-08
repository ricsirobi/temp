using UnityEngine;

public class Spline
{
	public SplineNode[] mNodes;

	public int mNumNodes;

	public float mLinearLength;

	public bool mLooping;

	public bool mConstSpeed;

	public bool mAlignTangent;

	public bool mHasQ;

	public int mNumSample = 3000;

	public Spline(int numCtrolPts, bool looping, bool constSpeed, bool alignTangent, bool hasQ)
	{
		mLooping = looping;
		mConstSpeed = constSpeed;
		mAlignTangent = alignTangent;
		mNumNodes = numCtrolPts;
		mNodes = new SplineNode[mNumNodes];
		mHasQ = hasQ;
	}

	public void SetControlPoint(int idx, Vector3 cpoint, Quaternion cqut, float t)
	{
		mNodes[idx] = new SplineNode();
		mNodes[idx].mPoint = cpoint;
		mNodes[idx].mQuat = cqut;
		mNodes[idx].mTime = t;
	}

	public Spline(Vector3[] ctrlPoints, Quaternion[] ctrlQuats, float[] ctrlTimes, bool looping, bool constSpeed, bool alignTangent)
	{
		mLooping = looping;
		mConstSpeed = constSpeed;
		mAlignTangent = alignTangent;
		if (ctrlQuats == null)
		{
			mHasQ = false;
		}
		else
		{
			mHasQ = ctrlQuats.Length != 0;
		}
		mNumNodes = ctrlPoints.Length;
		mNodes = new SplineNode[mNumNodes];
		int num = 0;
		for (num = 0; num < mNumNodes; num++)
		{
			mNodes[num] = new SplineNode();
			mNodes[num].mPoint = ctrlPoints[num];
			if (ctrlQuats != null && ctrlQuats.Length > num)
			{
				mNodes[num].mQuat = ctrlQuats[num];
			}
			if (ctrlTimes != null && ctrlTimes.Length > num)
			{
				mNodes[num].mTime = ctrlTimes[num];
			}
		}
		RecalculateSpline();
	}

	public void GeneratePath(Transform start, Transform end, float turnRadius, float minDist, bool endAlign, float moveHeight)
	{
		mLooping = false;
		mConstSpeed = true;
		if (mNumNodes < 10)
		{
			mNumNodes = 10;
			mNodes = new SplineNode[mNumNodes];
		}
		Vector3[] array = new Vector3[10];
		float[] array2 = new float[10] { 0f, 0.1f, 0.16f, 0.2f, 0.5f, 0.75f, 0.8f, 0.85f, 0.9f, 1f };
		Vector3 vector = new Vector3(0f, 0f, 0f);
		Vector3 vector2 = new Vector3(0f, 0f, 0f);
		Vector3 vector3 = new Vector3(0f, 0f, 0f);
		Vector3 vector4 = new Vector3(0f, 0f, 0f);
		vector = start.position;
		vector.y = moveHeight;
		vector2 = end.position;
		vector2.y = moveHeight;
		vector3 = start.forward;
		vector3.y = 0f;
		vector3.Normalize();
		vector4 = end.forward;
		vector4.y = 0f;
		vector4.Normalize();
		array[0] = vector;
		array[9] = vector2;
		array[1] = GetHeartPoint(vector, vector2, vector3, 1f * turnRadius);
		array[2] = GetHeartPoint(array[1], vector2, array[1] - array[0], turnRadius);
		array[3] = GetHeartPoint(array[2], vector2, array[2] - array[1], turnRadius);
		if (endAlign)
		{
			array[8] = vector2 + vector4 * (-1f * turnRadius);
			array[7] = GetHeartPoint(array[8], vector, array[8] - array[9], 1f * turnRadius);
			array[6] = GetHeartPoint(array[7], vector, array[7] - array[8], 1f * turnRadius);
			array[5] = GetHeartPoint(array[6], vector, array[6] - array[7], 1f * turnRadius);
			array[4] = (array[3] + array[5]) * 0.5f;
			mNumNodes = 10;
		}
		else
		{
			array2[4] = 1f;
			mNumNodes = 5;
			array[4] = vector2;
		}
		int num = 0;
		for (num = 0; num < mNumNodes; num++)
		{
			SetControlPoint(num, array[num], Quaternion.identity, array2[num]);
		}
		RecalculateSpline();
	}

	public float GetPosQuatByTime(float t, out Vector3 pos, out Quaternion quat)
	{
		float num = t;
		pos = Vector3.zero;
		quat = Quaternion.identity;
		if (num < 0f)
		{
			num = 0f;
		}
		if (num > 1f)
		{
			num = 1f;
		}
		if (mNumNodes < 2)
		{
			return num;
		}
		if (GetSectionTimeInfo(num, out var rt, out var n, out var n2))
		{
			if (mHasQ)
			{
				quat = CalculateQuat(mNodes[n].mQuat, mNodes[n2].mQuat, rt);
			}
			pos = Calculate3D(n, n2, rt);
		}
		if (mAlignTangent)
		{
			float num2 = 0.02f;
			if (mLinearLength >= 1f)
			{
				num2 = 1f / mLinearLength;
			}
			float num3 = num + num2;
			if (num3 > 1f)
			{
				num3 = num - num2;
			}
			if (num3 < 0f)
			{
				num3 = 0f;
			}
			if (GetSectionTimeInfo(num3, out rt, out n, out n2))
			{
				Vector3 vector = Calculate3D(n, n2, rt);
				if (num3 > num)
				{
					vector -= pos;
				}
				else
				{
					vector = pos - vector;
				}
				if (vector.magnitude > 0.001f)
				{
					vector.Normalize();
					quat = Quaternion.LookRotation(vector);
				}
			}
		}
		return num;
	}

	public float GetPosQuatByDist(float d, out Vector3 pos, out Quaternion quat)
	{
		float t = d / mLinearLength;
		Vector3 pos2 = new Vector3(0f, 0f, 0f);
		Quaternion quat2 = new Quaternion(0f, 0f, 0f, 0f);
		t = GetPosQuatByTime(t, out pos2, out quat2);
		pos = pos2;
		quat = quat2;
		return t * mLinearLength;
	}

	public float GetClosestPoint(Vector3 pos, float pPos)
	{
		return 0f;
	}

	public void GenerateTangents()
	{
		mHasQ = false;
		mAlignTangent = true;
		Vector3 pos = new Vector3(0f, 0f, 0f);
		Quaternion quat = new Quaternion(0f, 0f, 0f, 0f);
		for (int i = 0; i < mNumNodes; i++)
		{
			GetPosQuatByTime(mNodes[i].mTime, out pos, out quat);
			mNodes[i].mQuat = quat;
		}
		mHasQ = true;
		mAlignTangent = false;
	}

	public void RecalculateSpline()
	{
		int num = 0;
		int num2 = 0;
		int num3 = mNumNodes - 1;
		for (int i = 0; i < mNumNodes; i++)
		{
			if (!mConstSpeed)
			{
				continue;
			}
			if (mLooping)
			{
				if (i == 0)
				{
					mNodes[i].mTime = 1f;
				}
				else
				{
					mNodes[i].mTime = (float)i / (float)mNumNodes;
				}
			}
			else
			{
				mNodes[i].mTime = (float)i / (float)(mNumNodes - 1);
			}
		}
		for (int i = 0; i < mNumNodes; i++)
		{
			num = i;
			num2 = ((i != num3) ? (i + 1) : 0);
			if (i == 0)
			{
				mNodes[num].mTimeFactor = mNodes[num2].mTime;
			}
			else
			{
				mNodes[num].mTimeFactor = mNodes[num2].mTime - mNodes[num].mTime;
			}
			mNodes[num].mTimeFactor = 1f / mNodes[num].mTimeFactor;
		}
		if (mLooping)
		{
			mNodes[num3].mTimeFactor = mNodes[0].mTime - mNodes[num3].mTime;
			mNodes[num3].mTimeFactor = 1f / mNodes[num3].mTimeFactor;
		}
		CalculateLinearLength(mNumSample, mConstSpeed);
	}

	public void OnDrawGizmos(int np)
	{
		OnDrawGizmos(np, Matrix4x4.identity, Color.blue);
	}

	public void OnDrawGizmos(int np, Matrix4x4 tmat, Color c)
	{
		int num = 0;
		float num2 = 0f;
		float num3 = 1f / (float)np;
		Gizmos.color = c;
		GetPosQuatByDist(num2, out var pos, out var quat);
		Gizmos.matrix = tmat;
		for (num = 1; num <= np; num++)
		{
			num2 += num3;
			GetPosQuatByTime(num2, out var pos2, out var quat2);
			Gizmos.DrawLine(pos, pos2);
			pos = pos2;
			quat = quat2;
		}
		if (mHasQ)
		{
			Matrix4x4 matrix4x = default(Matrix4x4);
			for (num = 0; num < mNumNodes; num++)
			{
				matrix4x.SetTRS(mNodes[num].mPoint, mNodes[num].mQuat, Vector3.one);
				Gizmos.matrix = tmat * matrix4x;
				Gizmos.color = Color.red;
				Gizmos.DrawLine(Vector3.zero, Vector3.right);
				Gizmos.color = Color.green;
				Gizmos.DrawLine(Vector3.zero, Vector3.up);
				Gizmos.color = Color.blue;
				Gizmos.DrawLine(Vector3.zero, Vector3.forward);
			}
			return;
		}
		for (num = 0; num < mNumNodes; num++)
		{
			switch (num % 5)
			{
			case 0:
				Gizmos.color = Color.black;
				break;
			case 1:
				Gizmos.color = Color.white;
				break;
			case 2:
				Gizmos.color = Color.red;
				break;
			case 3:
				Gizmos.color = Color.green;
				break;
			case 4:
				Gizmos.color = Color.blue;
				break;
			}
			Gizmos.DrawWireCube(mNodes[num].mPoint, new Vector3(0.2f, 0.2f, 0.2f));
		}
	}

	private void CalculateLinearLength(float ns, bool normalizetime)
	{
		bool flag = mHasQ;
		float num = 1f / ns;
		float num2 = num;
		bool flag2 = mAlignTangent;
		mLinearLength = 0f;
		Vector3 b = mNodes[0].mPoint;
		mHasQ = false;
		mAlignTangent = false;
		int num3 = 1;
		for (; num2 < num + 1f; num2 += num)
		{
			GetPosQuatByTime(num2, out var pos, out var _);
			mLinearLength += Vector3.Distance(pos, b);
			if (num3 < mNumNodes && num2 >= mNodes[num3].mTime)
			{
				mNodes[num3].mDistance = mLinearLength;
				num3++;
			}
			b = pos;
		}
		if (mLooping)
		{
			mNodes[0].mTime = 1f;
			mNodes[0].mDistance = mLinearLength;
		}
		else
		{
			mNodes[0].mTime = 0f;
			mNodes[0].mDistance = 0f;
			mNodes[mNumNodes - 1].mDistance = mLinearLength;
		}
		if (normalizetime)
		{
			for (num3 = 1; num3 < mNumNodes; num3++)
			{
				mNodes[num3].mTime = mNodes[num3].mDistance / mLinearLength;
			}
			mNodes[0].mTimeFactor = 1f / mNodes[1].mTime;
			for (num3 = 1; num3 < mNumNodes - 1; num3++)
			{
				mNodes[num3].mTimeFactor = 1f / (mNodes[num3 + 1].mTime - mNodes[num3].mTime);
			}
			if (mLooping)
			{
				mNodes[mNumNodes - 1].mTimeFactor = 1f / (mNodes[0].mTime - mNodes[mNumNodes - 1].mTime);
			}
			else
			{
				mNodes[mNumNodes - 1].mTimeFactor = 0f;
			}
		}
		mHasQ = flag;
		mAlignTangent = flag2;
	}

	public bool GetSectionTimeInfo(float t, out float rt, out int n1, out int n2)
	{
		int num = 0;
		int num2 = 1;
		for (num2 = 1; num2 < mNumNodes; num2++)
		{
			if (mNodes[num2].mTime >= t)
			{
				num = num2 - 1;
				if (num == 0 && mLooping)
				{
					rt = t * mNodes[0].mTimeFactor;
				}
				else
				{
					rt = (t - mNodes[num].mTime) * mNodes[num].mTimeFactor;
				}
				n1 = num;
				n2 = num2;
				return true;
			}
		}
		if (mLooping && mNodes[0].mTime >= t)
		{
			num = mNumNodes - 1;
			num2 = 0;
			rt = (t - mNodes[num].mTime) * mNodes[num].mTimeFactor;
			n1 = num;
			n2 = num2;
			return true;
		}
		n1 = 0;
		n2 = 0;
		rt = 0f;
		return false;
	}

	private Vector3 Calculate3D(int n1, int n2, float t)
	{
		Vector3 vector = new Vector3(0f, 0f, 0f);
		Vector3 vector2 = new Vector3(0f, 0f, 0f);
		Vector3 vector3 = new Vector3(0f, 0f, 0f);
		Vector3 vector4 = new Vector3(0f, 0f, 0f);
		Vector3 vector5 = new Vector3(0f, 0f, 0f);
		Vector3 vector6 = new Vector3(0f, 0f, 0f);
		vector2 = mNodes[n1].mPoint;
		vector3 = mNodes[n2].mPoint;
		if (mLooping)
		{
			vector = ((n1 != 0) ? mNodes[n1 - 1].mPoint : mNodes[mNumNodes - 1].mPoint);
			vector5 = (vector3 - vector) * 0.5f;
			vector4 = ((n2 != mNumNodes - 1) ? mNodes[n2 + 1].mPoint : mNodes[0].mPoint);
			vector6 = (vector4 - vector2) * 0.5f;
		}
		else
		{
			if (n1 == 0)
			{
				vector = mNodes[n1].mPoint;
				vector5 = (vector3 - vector2) * 0.5f;
			}
			else
			{
				vector = mNodes[n1 - 1].mPoint;
				vector5 = (vector3 - vector) * 0.5f;
			}
			if (n2 == mNumNodes - 1)
			{
				vector4 = mNodes[n2].mPoint;
				vector6 = (vector3 - vector2) * 0.5f;
			}
			else
			{
				vector4 = mNodes[n2 + 1].mPoint;
				vector6 = (vector4 - vector2) * 0.5f;
			}
		}
		float num = t * t;
		float num2 = num * t;
		float num3 = num2 + num2 - num - num - num + 1f;
		float num4 = 0f - num2 - num2 + num + num + num;
		float num5 = num2 - num - num + t;
		float num6 = num2 - num;
		return vector2 * num3 + vector3 * num4 + vector5 * num5 + vector6 * num6;
	}

	private Quaternion CalculateQuat(Quaternion q1, Quaternion q2, float t)
	{
		return Quaternion.Slerp(q1, q2, t);
	}

	private Vector3 GetHeartPoint(Vector3 sp, Vector3 dp, Vector3 sdir0, float r)
	{
		Vector3 vector = dp - sp;
		Vector3 vector2 = sdir0;
		vector2.Normalize();
		vector.Normalize();
		float num = (Vector3.Dot(vector2, vector) + 1f) * 0.5f;
		num = 1f - num;
		num *= 0.7f;
		num += 0.3f;
		Vector3 vector3 = vector2 + vector;
		if (vector3.magnitude < 1E-05f)
		{
			vector3 = Vector3.Cross(vector, Vector3.up);
			vector3 *= 0.1f;
			vector3 = vector2 + vector3;
		}
		vector3.Normalize();
		vector3 *= r * num;
		return vector3 + sp;
	}
}
