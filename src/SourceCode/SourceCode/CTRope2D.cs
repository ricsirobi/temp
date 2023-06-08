using UnityEngine;

public class CTRope2D : PhysicsObject
{
	public int _NumOfJoints;

	public Vector3 _IndividualJointScale = new Vector3(0.5f, 0.5f, 0.1f);

	public Vector3 _InitialJointPosition = new Vector3(0f, 0f, 0f);

	public float _IndividualJointRigidMass = 200f;

	public float _IndividualJointGravityScale = 50f;

	public bool _CollideConnected = true;

	public bool _UseMotor = true;

	public float _MotorSpeed = 100f;

	public float _MaxMotorForce;

	public bool _UseAngleLimit = true;

	public float _FirstJointMinAngle;

	public float _FirstJointMaxAngle;

	public float _AngleOffset;

	public Sprite _Sprite;

	public GameObject _TargetGo;

	private Rigidbody2D m_PreviousRigidbody;

	private bool m_bIsMouseButtonDown;

	private void Start()
	{
		GameObject gameObject = null;
		Vector3 initialJointPosition = _InitialJointPosition;
		for (int i = 0; i < _NumOfJoints; i++)
		{
			gameObject = new GameObject
			{
				name = "Joint_" + i
			};
			gameObject.layer = LayerMask.NameToLayer("Rope");
			gameObject.transform.parent = base.gameObject.transform;
			gameObject.transform.localPosition = initialJointPosition;
			gameObject.transform.localScale = _IndividualJointScale;
			AddSpriteRenderer(gameObject);
			AddJoint(gameObject, _FirstJointMinAngle, _FirstJointMaxAngle);
			AddRigidBody(gameObject);
			initialJointPosition.y -= _IndividualJointScale.y;
			gameObject.AddComponent<BoxCollider2D>();
			m_PreviousRigidbody = gameObject.GetComponent<Rigidbody2D>();
			_FirstJointMinAngle -= _AngleOffset;
			_FirstJointMaxAngle += _AngleOffset;
		}
		ConnectToTarget();
	}

	public override void Enable()
	{
		base.rigidbody2D.freezeRotation = false;
		base.rigidbody2D.gravityScale = gravity;
		canMove = true;
	}

	private void AddSpriteRenderer(GameObject inCurrentGo)
	{
		inCurrentGo.AddComponent<SpriteRenderer>().sprite = _Sprite;
	}

	private void AddRigidBody(GameObject inCurrentGo)
	{
		Rigidbody2D component = inCurrentGo.GetComponent<Rigidbody2D>();
		component.mass = _IndividualJointRigidMass;
		component.gravityScale = _IndividualJointGravityScale;
		component.angularDrag = 0f;
	}

	private void AddJoint(GameObject inCurrentGo, float inMinAngle, float inMaxAngle)
	{
		HingeJoint2D hingeJoint2D = inCurrentGo.AddComponent<HingeJoint2D>();
		hingeJoint2D.enableCollision = _CollideConnected;
		if (m_PreviousRigidbody == null)
		{
			hingeJoint2D.connectedBody = base.gameObject.GetComponent<Rigidbody2D>();
			hingeJoint2D.anchor = new Vector2(_InitialJointPosition.x, 0.4f);
			hingeJoint2D.connectedAnchor = new Vector2(_InitialJointPosition.x, -0.4f);
		}
		else
		{
			hingeJoint2D.connectedBody = m_PreviousRigidbody;
			hingeJoint2D.anchor = new Vector2(_InitialJointPosition.x, 0.5f);
			hingeJoint2D.connectedAnchor = new Vector2(_InitialJointPosition.x, -0.5f);
		}
		hingeJoint2D.useMotor = _UseMotor;
		if (hingeJoint2D.useMotor)
		{
			hingeJoint2D.motor = new JointMotor2D
			{
				motorSpeed = _MotorSpeed,
				maxMotorTorque = _MaxMotorForce
			};
		}
		hingeJoint2D.useLimits = _UseAngleLimit;
		if (hingeJoint2D.useLimits)
		{
			hingeJoint2D.limits = new JointAngleLimits2D
			{
				min = inMinAngle,
				max = inMaxAngle
			};
		}
	}

	private void ConnectToTarget()
	{
		if (_TargetGo != null)
		{
			HingeJoint2D hingeJoint2D = _TargetGo.AddComponent<HingeJoint2D>();
			hingeJoint2D.connectedBody = m_PreviousRigidbody;
			hingeJoint2D.anchor = new Vector2(_InitialJointPosition.x, -0.4f);
			hingeJoint2D.connectedAnchor = new Vector2(_InitialJointPosition.x, -0.4f);
		}
	}

	private void Update()
	{
		if (!IsMouseButtonDown())
		{
			return;
		}
		RaycastHit2D raycastHit2D = Physics2D.Raycast(Camera.main.ScreenToWorldPoint(Input.mousePosition), Vector2.zero);
		if (raycastHit2D.collider != null)
		{
			HingeJoint2D component = raycastHit2D.collider.gameObject.GetComponent<HingeJoint2D>();
			if (component != null)
			{
				component.enabled = false;
			}
		}
	}

	private bool IsMouseButtonDown()
	{
		if (!m_bIsMouseButtonDown)
		{
			m_bIsMouseButtonDown = Input.GetMouseButtonDown(0);
		}
		else if (Input.GetMouseButtonUp(0))
		{
			m_bIsMouseButtonDown = false;
		}
		return m_bIsMouseButtonDown;
	}

	private void OnDestroy()
	{
		Transform[] componentsInChildren = base.gameObject.GetComponentsInChildren<Transform>();
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			if (componentsInChildren[i].gameObject.layer == LayerMask.NameToLayer("Rope"))
			{
				Object.Destroy(componentsInChildren[i].gameObject);
			}
		}
	}

	public override void Reset()
	{
		canMove = false;
		base.rigidbody2D.velocity = new Vector2(0f, 0f);
		base.rigidbody2D.gravityScale = 0f;
		base.rigidbody2D.freezeRotation = true;
		StopAllCoroutines();
		base.gameObject.SetActive(value: true);
	}
}
