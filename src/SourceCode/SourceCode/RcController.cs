using UnityEngine;

public class RcController : PhysicsObject
{
	public RcCar car;

	public Transform knob;

	public Transform body;

	public ParticleSystem particle;

	private float maxKnobMovement = 0.11f;

	private float signalStrength;

	private bool inPlayMode;

	private Vector2 knobStartDist;

	private Vector2 bodyStartPos;

	private ParticleSystem.EmissionModule mEmissionModule;

	private void Awake()
	{
		knobStartDist = knob.localPosition - body.localPosition;
		bodyStartPos = body.transform.localPosition;
		mEmissionModule = particle.emission;
	}

	public void FixedUpdate()
	{
		UpdateKnobPosition();
		CalculateSignalStrength();
		if (inPlayMode)
		{
			SendSignal();
		}
	}

	public override void Enable()
	{
		inPlayMode = true;
		body.GetComponent<Rigidbody2D>().gravityScale = gravity;
	}

	public override void Reset()
	{
		inPlayMode = false;
		body.GetComponent<Rigidbody2D>().gravityScale = 0f;
		body.GetComponent<Rigidbody2D>().velocity = Vector2.zero;
		body.localPosition = bodyStartPos;
		knob.localPosition = bodyStartPos + knobStartDist;
	}

	private void UpdateKnobPosition()
	{
		knob.transform.localPosition = new Vector2(body.localPosition.x + knobStartDist.x, knob.transform.localPosition.y);
		if (knob.localPosition.y - body.localPosition.y < knobStartDist.y)
		{
			knob.GetComponent<Rigidbody2D>().AddForce(Vector2.up * 5f);
			return;
		}
		knob.GetComponent<Rigidbody2D>().velocity = new Vector2(0f, 0f);
		knob.transform.localPosition = new Vector2(knob.transform.localPosition.x, body.localPosition.y + knobStartDist.y);
	}

	private void CalculateSignalStrength()
	{
		signalStrength = Mathf.Abs(knob.localPosition.y - body.localPosition.y - knobStartDist.y) / (maxKnobMovement / 100f);
		if (signalStrength > 1f)
		{
			mEmissionModule.enabled = true;
			return;
		}
		mEmissionModule.enabled = false;
		signalStrength = 0f;
	}

	private void SendSignal()
	{
		if (car == null)
		{
			car = Object.FindObjectOfType<RcCar>();
		}
		else
		{
			car.SetAcceloration(signalStrength);
		}
	}
}
