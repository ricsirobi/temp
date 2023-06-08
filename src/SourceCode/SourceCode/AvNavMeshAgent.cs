using UnityEngine;
using UnityEngine.AI;

public class AvNavMeshAgent : MonoBehaviour
{
	private Vector3 mTargetPos;

	private GameObject mMessageObject;

	private NavMeshAgent mNavMeshAgent;

	private AvAvatarController mAvatarController;

	private Animator mAvatarAnimator;

	private AvAvatarAnim mAvatarAnim;

	public Vector3 pTargetPos
	{
		set
		{
			mTargetPos = value;
		}
	}

	public GameObject pMessageObject
	{
		set
		{
			mMessageObject = value;
		}
	}

	private void Awake()
	{
		mNavMeshAgent = GetComponent<NavMeshAgent>();
		mAvatarController = base.gameObject.GetComponent<AvAvatarController>();
		mAvatarAnim = mAvatarController.GetComponentInChildren<AvAvatarAnim>();
		mAvatarAnimator = mAvatarAnim.GetComponent<Animator>();
	}

	private void OnEnable()
	{
		mNavMeshAgent.enabled = true;
		if (mNavMeshAgent != null && mNavMeshAgent.pathStatus == NavMeshPathStatus.PathComplete)
		{
			EnableAvatarControlling(isEnable: false);
			mNavMeshAgent.destination = mTargetPos;
		}
		else
		{
			UtDebug.Log("Path cannot be reached!!!!");
			base.transform.position = mTargetPos;
		}
	}

	private void OnDisable()
	{
		if (mMessageObject != null)
		{
			mMessageObject.SendMessage("OnPathEndReached", base.gameObject);
		}
		if (mNavMeshAgent != null)
		{
			mNavMeshAgent.enabled = false;
		}
		EnableAvatarControlling(isEnable: true);
	}

	private void Update()
	{
		if (mNavMeshAgent.remainingDistance <= mNavMeshAgent.stoppingDistance)
		{
			base.enabled = false;
		}
	}

	private void EnableAvatarControlling(bool isEnable)
	{
		mAvatarAnim.enabled = isEnable;
		mAvatarController.enabled = isEnable;
	}

	private void LateUpdate()
	{
		if (mNavMeshAgent.remainingDistance > 0f)
		{
			mAvatarAnimator.SetFloat("fSpeed", 1f);
		}
	}
}
