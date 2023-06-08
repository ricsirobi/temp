using UnityEngine;

public class CTRopeHolder : PhysicsObject
{
	private Rigidbody2D[] chainLinks;

	private GameObject chainLinkGameObjectAtA;

	private GameObject chainLinkGameObjectAtB;

	private Vector2 mLastBodyPos;

	private Vector2 mLastBodyRot;

	private Transform mLastRopeTransform;

	private GameObject mRopeHolder;

	private DistanceJoint2D mJointA;

	private DistanceJoint2D mJointB;

	private JointInfo mJointInfoA;

	private JointInfo mJointInfoB;

	public Rigidbody2D BodyConnectedAtA;

	public Rigidbody2D BodyConnectedAtB;

	private void Start()
	{
		mRopeHolder = base.transform.GetChild(0).gameObject;
		mLastRopeTransform = base.transform;
		mRopeHolder.GetComponent<Rigidbody2D>().isKinematic = true;
		mRopeHolder.GetComponent<Rigidbody2D>().velocity = new Vector2(0f, 0f);
		chainLinks = mRopeHolder.gameObject.GetComponentsInChildren<Rigidbody2D>();
		if (BodyConnectedAtA != null)
		{
			mJointA = chainLinks[1].GetComponent<DistanceJoint2D>();
			if (mJointA != null)
			{
				mJointA.enabled = true;
				mJointA.connectedBody = BodyConnectedAtA;
				mJointInfoA = new JointInfo();
				mJointInfoA.connectedBody = mJointA.connectedBody;
				mJointInfoA.anchor = mJointA.anchor;
				mJointInfoA.connectedAnchor = mJointA.connectedAnchor;
				mJointInfoA.distance = mJointA.distance;
			}
			else
			{
				UtDebug.LogWarning("Rope end A not connected to any object");
			}
		}
		if (BodyConnectedAtB != null)
		{
			mJointB = chainLinks[chainLinks.Length - 1].GetComponent<DistanceJoint2D>();
			if (mJointB != null)
			{
				mJointB.enabled = true;
				mJointB.connectedBody = BodyConnectedAtB;
				mJointInfoB = new JointInfo();
				mJointInfoB.connectedBody = mJointB.connectedBody;
				mJointInfoB.anchor = mJointB.anchor;
				mJointInfoB.connectedAnchor = mJointB.connectedAnchor;
				mJointInfoB.distance = mJointB.distance;
			}
			else
			{
				UtDebug.LogWarning("Rope end B not connected to any object");
			}
		}
	}

	public override void OnCollisionEnter2D(Collision2D other)
	{
	}

	public override void Enable()
	{
		canMove = true;
		Rigidbody2D[] array = chainLinks;
		foreach (Rigidbody2D obj in array)
		{
			obj.gravityScale = gravity;
			obj.isKinematic = false;
			obj.velocity = new Vector2(0f, 0f);
			obj.freezeRotation = false;
		}
		chainLinks[0].isKinematic = true;
		chainLinks[0].freezeRotation = true;
		chainLinks[0].gravityScale = 0f;
	}

	public override void Reset()
	{
		base.rigidbody2D.isKinematic = true;
		base.transform.position = mLastRopeTransform.position;
		base.transform.eulerAngles = mLastRopeTransform.eulerAngles;
		Object.Destroy(mRopeHolder);
		mRopeHolder = null;
		RsResourceManager.LoadAssetFromBundle("RS_DATA/PfRopeHolder", "PfRopeHolder", OnLoaded, typeof(GameObject), inDontDestroy: true);
		base.gameObject.SetActive(value: false);
		canMove = false;
	}

	private void OnLoaded(string inURL, RsResourceLoadEvent inEvent, float inProgress, object inObject, object inUserData)
	{
		if (inObject != null && inEvent == RsResourceLoadEvent.COMPLETE && mRopeHolder == null)
		{
			mRopeHolder = Object.Instantiate((GameObject)inObject, base.transform.position, base.transform.rotation);
			mRopeHolder.transform.parent = base.transform;
			mRopeHolder.GetComponent<Rigidbody2D>().velocity = new Vector2(0f, 0f);
			mRopeHolder.GetComponent<Rigidbody2D>().gravityScale = 0f;
			base.gameObject.SetActive(value: true);
			chainLinks = mRopeHolder.gameObject.GetComponentsInChildren<Rigidbody2D>();
			Rigidbody2D[] array = chainLinks;
			foreach (Rigidbody2D obj in array)
			{
				obj.gravityScale = 0f;
				obj.isKinematic = true;
				obj.velocity = new Vector2(0f, 0f);
				obj.freezeRotation = true;
			}
			chainLinkGameObjectAtA = mRopeHolder.transform.GetChild(0).gameObject;
			chainLinkGameObjectAtB = mRopeHolder.transform.GetChild(chainLinks.Length - 2).gameObject;
			if (mJointInfoA != null)
			{
				chainLinkGameObjectAtA.AddComponent<DistanceJoint2D>();
				chainLinkGameObjectAtA.GetComponent<DistanceJoint2D>().connectedBody = mJointInfoA.connectedBody;
				chainLinkGameObjectAtA.GetComponent<DistanceJoint2D>().anchor = mJointInfoA.anchor;
				chainLinkGameObjectAtA.GetComponent<DistanceJoint2D>().connectedAnchor = mJointInfoA.connectedAnchor;
				chainLinkGameObjectAtA.GetComponent<DistanceJoint2D>().distance = mJointInfoA.distance;
			}
			if (mJointInfoB != null)
			{
				chainLinkGameObjectAtB.AddComponent<DistanceJoint2D>();
				chainLinkGameObjectAtB.GetComponent<DistanceJoint2D>().connectedBody = mJointInfoB.connectedBody;
				chainLinkGameObjectAtB.GetComponent<DistanceJoint2D>().anchor = mJointInfoB.anchor;
				chainLinkGameObjectAtB.GetComponent<DistanceJoint2D>().connectedAnchor = mJointInfoB.connectedAnchor;
				chainLinkGameObjectAtB.GetComponent<DistanceJoint2D>().distance = mJointInfoB.distance;
			}
		}
	}

	public void MakeLinksStatic()
	{
		chainLinks = mRopeHolder.gameObject.GetComponentsInChildren<Rigidbody2D>();
		if (chainLinks != null)
		{
			Rigidbody2D[] array = chainLinks;
			foreach (Rigidbody2D obj in array)
			{
				obj.gravityScale = 0f;
				obj.isKinematic = true;
				obj.velocity = new Vector2(0f, 0f);
				obj.freezeRotation = true;
			}
		}
		else
		{
			UtDebug.LogWarning("chain links not found");
		}
	}
}
