using System;
using UnityEngine;

public class Breakable : MonoBehaviour
{
	public static AudioSource audioSrc;

	public float breakAngle = 30f;

	public float breakDistance = 1.3f;

	public bool newMatPerChunk = true;

	public bool limitBreakCount;

	public int maxBreakCount = 3;

	public AudioClip chainBreakSound;

	public GameObject breakFX;

	private HingeJoint2D joint;

	private HingeJoint2D connectedJoint;

	private Transform connectedObject;

	private Transform tr;

	private bool broken;

	private bool jointGot;

	private float time;

	[NonSerialized]
	public int breakCount;

	private void Awake()
	{
		if (audioSrc == null)
		{
			GameObject obj = new GameObject("Break Sound Player");
			obj.transform.position = Camera.main.transform.position;
			audioSrc = obj.AddComponent<AudioSource>();
			audioSrc.playOnAwake = false;
		}
	}

	private void OnEnable()
	{
		broken = false;
		jointGot = false;
		breakCount = 0;
		if (tr == null)
		{
			tr = base.transform;
		}
		if (joint == null)
		{
			joint = GetComponent<HingeJoint2D>();
		}
		if (!limitBreakCount)
		{
			maxBreakCount = breakCount + 1;
		}
		time = Time.time;
	}

	private void GetConnectedObject()
	{
		connectedObject = joint.connectedBody.transform;
		connectedJoint = connectedObject.GetComponent<HingeJoint2D>();
		jointGot = true;
	}

	private void FixedUpdate()
	{
		if (!(Time.time > time + 1f) || breakCount >= maxBreakCount || broken)
		{
			return;
		}
		if ((bool)joint && (bool)joint.connectedBody)
		{
			if (!jointGot)
			{
				GetConnectedObject();
			}
			if ((bool)connectedJoint)
			{
				Vector3.Angle(tr.up, connectedObject.up);
				_ = breakAngle;
			}
			if ((bool)connectedObject)
			{
				Vector3.Distance(tr.position, connectedObject.position);
				_ = breakDistance;
			}
		}
		if (!joint || (jointGot && (bool)joint && !joint.enabled))
		{
			broken = true;
		}
	}

	public void OnCollisionEnter2D(Collision2D other)
	{
		if (other.collider.gameObject.tag == "RopeCutter")
		{
			Break();
		}
	}

	private void Break()
	{
		if (limitBreakCount)
		{
			foreach (Transform item in tr.parent.transform)
			{
				item.GetComponent<Breakable>().breakCount++;
			}
		}
		if ((bool)joint)
		{
			joint.enabled = false;
			tr.GetComponent<Collider2D>().isTrigger = true;
			tr.GetComponent<Renderer>().enabled = false;
		}
		if ((bool)breakFX)
		{
			UnityEngine.Object.Destroy(UnityEngine.Object.Instantiate(breakFX, tr.position, Quaternion.identity), 1f);
		}
		if (!audioSrc.isPlaying)
		{
			audioSrc.enabled = true;
			audioSrc.clip = chainBreakSound;
			audioSrc.Play();
		}
		broken = true;
		if ((bool)joint && (bool)joint.transform.parent)
		{
			UseLineRenderer component = joint.transform.parent.GetComponent<UseLineRenderer>();
			if ((bool)component)
			{
				component.Split(joint.name, newMatPerChunk);
			}
		}
	}
}
