using UnityEngine;

public class AvatarBlink : MonoBehaviour
{
	public float _BlinkLength = 0.1f;

	private float mEndBlink;

	private float mNextBlink;

	protected Renderer mMeshRenderer;

	public Renderer pMeshRenderer => mMeshRenderer;

	protected virtual void Awake()
	{
		mMeshRenderer = GetComponent<MeshRenderer>();
		if (mMeshRenderer == null)
		{
			mMeshRenderer = GetComponent<SkinnedMeshRenderer>();
		}
	}

	private void Update()
	{
		if (!(mMeshRenderer == null))
		{
			if (Time.time > mNextBlink)
			{
				mEndBlink = Time.time + _BlinkLength;
				mNextBlink = mEndBlink + Random.Range(1f, 6f);
				mMeshRenderer.enabled = true;
			}
			else if (Time.time > mEndBlink)
			{
				mMeshRenderer.enabled = false;
				mEndBlink = mNextBlink;
			}
		}
	}

	public void ResetBlink()
	{
		mNextBlink = 0f;
	}
}
