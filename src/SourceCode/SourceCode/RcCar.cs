using UnityEngine;

public class RcCar : PhysicsObject
{
	public float wheelInertia;

	public Rigidbody2D mainRigidbody;

	public ParticleSystem particle;

	public float speed;

	public HingeJoint2D[] wheels;

	private float signalStrength;

	private float currentSpeed;

	private bool activated;

	private ParticleSystem.EmissionModule emissionModule;

	private JointMotor2D motor;

	private Vector2 frontWheelPos;

	private Vector2 backWheelPos;

	private Vector2 lastBodyPos;

	private Vector2 lastBodyRot;

	private void Start()
	{
		emissionModule = particle.emission;
		lastBodyPos = mainRigidbody.transform.position;
		lastBodyRot = mainRigidbody.transform.eulerAngles;
		frontWheelPos = wheels[0].transform.position;
		backWheelPos = wheels[1].transform.position;
		motor = default(JointMotor2D);
		motor.maxMotorTorque = 1000f;
		mainRigidbody.inertia = inertia;
		wheels[0].GetComponent<Rigidbody2D>().inertia = wheelInertia;
		wheels[1].GetComponent<Rigidbody2D>().inertia = wheelInertia;
	}

	public void FixedUpdate()
	{
		if (activated)
		{
			if (signalStrength > 10f)
			{
				emissionModule.enabled = true;
			}
			else
			{
				emissionModule.enabled = false;
			}
			currentSpeed = (0f - speed) * signalStrength / 100f;
			motor.motorSpeed = currentSpeed;
			wheels[0].motor = motor;
			wheels[1].motor = motor;
		}
	}

	public override void Enable()
	{
		lastBodyPos = mainRigidbody.transform.position;
		lastBodyRot = mainRigidbody.transform.eulerAngles;
		frontWheelPos = wheels[0].transform.position;
		backWheelPos = wheels[1].transform.position;
		mainRigidbody.freezeRotation = false;
		wheels[0].GetComponent<Rigidbody2D>().freezeRotation = false;
		wheels[1].GetComponent<Rigidbody2D>().freezeRotation = false;
		mainRigidbody.gravityScale = gravity;
		activated = true;
	}

	public override void Reset()
	{
		mainRigidbody.gravityScale = 0f;
		activated = false;
		signalStrength = 0f;
		emissionModule.enabled = false;
		StopAllCoroutines();
		mainRigidbody.velocity = Vector2.zero;
		wheels[0].GetComponent<Rigidbody2D>().velocity = Vector2.zero;
		wheels[1].GetComponent<Rigidbody2D>().velocity = Vector2.zero;
		mainRigidbody.freezeRotation = false;
		wheels[0].GetComponent<Rigidbody2D>().freezeRotation = false;
		wheels[1].GetComponent<Rigidbody2D>().freezeRotation = false;
		mainRigidbody.transform.position = lastBodyPos;
		mainRigidbody.transform.eulerAngles = lastBodyRot;
		wheels[0].transform.position = frontWheelPos;
		wheels[1].transform.position = backWheelPos;
	}

	public void SetAcceloration(float signal)
	{
		signalStrength = signal;
		PlaySound(inForce: true);
	}
}
