using UnityEngine;

public class MMOAvatarAnim : MonoBehaviour
{
	private Animator mAnim;

	private AvAvatarController mAC;

	private Transform mTrans;

	private Vector3 mPrevPosition;

	private float maxSpeed;

	private float mCurVelocity;

	private void Start()
	{
		mAnim = GetComponent<Animator>();
		mAC = base.transform.parent.gameObject.GetComponent<AvAvatarController>();
		mTrans = base.transform.parent.transform;
	}

	private void FixedUpdate()
	{
		if (!(mAnim == null) && !(mAC == null))
		{
			maxSpeed = mAC.pMaxForwardSpeed;
			Vector3 vector = mTrans.position - mPrevPosition;
			mPrevPosition = mTrans.position;
			float num = vector.y / Time.deltaTime;
			vector.y = 0f;
			float num2 = vector.magnitude / Time.deltaTime;
			bool flag = false;
			if (num == 0f || (mAC.OnGround() && mAnim.GetBool("bFall")))
			{
				mAnim.SetBool("bJump", value: false);
				mAnim.SetBool("bFall", value: false);
			}
			else if (num > 6f)
			{
				mAnim.SetBool("bJump", value: true);
				mAnim.SetBool("bFall", value: false);
			}
			else if (num < -5f)
			{
				mAnim.SetBool("bFall", value: true);
				flag = true;
			}
			if (num2 > maxSpeed)
			{
				num2 = maxSpeed;
			}
			num2 /= maxSpeed;
			mCurVelocity = Mathf.Lerp(mCurVelocity, num2, 0.2f);
			mAnim.SetFloat("fSpeed", flag ? 0f : mCurVelocity);
		}
	}
}
