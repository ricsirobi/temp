using UnityEngine;

public class ObRacingPosition : MonoBehaviour
{
	public bool _IgnoreY;

	public Texture[] _PositionSprite;

	public float _SpriteSize = 0.05f;

	private int mCurIndex;

	private Vector3 mTarget;

	private MeshRenderer mMesh;

	private void Awake()
	{
		SetPosition(0);
	}

	private void LateUpdate()
	{
		if (AvAvatar.pAvatarCam != null && AvAvatar.pLevelState == AvAvatarLevelState.RACING)
		{
			mTarget = AvAvatar.AvatarCamPosition;
			if (_IgnoreY)
			{
				mTarget.y = base.transform.position.y;
			}
			base.transform.LookAt(mTarget);
			Camera component = AvAvatar.pAvatarCam.GetComponent<Camera>();
			Vector3 point = component.worldToCameraMatrix.MultiplyPoint(base.transform.position);
			if (point.z < 0f)
			{
				float num = 1f / (component.projectionMatrix.MultiplyPoint(point).z / (0f - point.z));
				num *= _SpriteSize;
				base.transform.localScale = new Vector3(num, num, 1f);
			}
		}
		else if (mMesh != null)
		{
			mMesh.enabled = false;
		}
	}

	public void SetPosition(int index)
	{
		mCurIndex = Mathf.Clamp(index, 0, _PositionSprite.Length);
		mMesh = base.gameObject.GetComponent<MeshRenderer>();
		if (mMesh != null && mMesh.material != null)
		{
			if (index == 0)
			{
				mMesh.enabled = false;
				return;
			}
			mMesh.enabled = true;
			mMesh.material.SetTexture("_MainTex", _PositionSprite[mCurIndex - 1]);
		}
	}

	public int GetPosition()
	{
		return mCurIndex;
	}
}
