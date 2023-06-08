using UnityEngine;

public class FPSSimple : MonoBehaviour
{
	public Rect _Position = new Rect(10f, 10f, 200f, 96f);

	public Color _Color = Color.white;

	public Font _Font;

	public float _Size = 1.5f;

	private float mElapsedTime;

	private int mMinFps;

	private int mMaxFps;

	private int mFps;

	private int mFrameCount;

	private float mMBSize = 1048576f;

	private GUIStyle myStyle = new GUIStyle();

	private void Start()
	{
		Object.Destroy(base.gameObject);
	}

	private void Update()
	{
		mElapsedTime += Time.deltaTime;
		if (mElapsedTime >= 1f)
		{
			mElapsedTime = 0f;
			mFps = mFrameCount;
			mFrameCount = 0;
			if (mFps < mMinFps)
			{
				mMinFps = mFps;
			}
			if (mFps > mMaxFps)
			{
				mMaxFps = mFps;
			}
		}
		else
		{
			mFrameCount++;
		}
	}

	private void OnGUI()
	{
		GUI.depth = -1;
		string text = "FPS : " + mFps + " \nFree : " + (float)UtMobileUtilities.GetFreeMemory() / mMBSize + " \nInUse : " + (float)UtMobileUtilities.GetMemoryInUse() / mMBSize + " \n CUPUSage: " + UtMobileUtilities.GetCPUTime();
		if (_Font != null)
		{
			GUI.Box(_Position, text, myStyle);
			myStyle.font = _Font;
			return;
		}
		Matrix4x4 matrix = GUI.matrix;
		GUI.matrix = Matrix4x4.TRS(Vector3.zero, Quaternion.identity, Vector3.one * _Size);
		GUI.color = _Color;
		GUI.Label(_Position, text);
		GUI.matrix = matrix;
	}
}
