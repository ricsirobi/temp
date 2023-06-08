using System.Collections;
using UnityEngine;

public class RopeCutter : MonoBehaviour
{
	public bool useCutterObject = true;

	public bool useTouch;

	public AudioClip cutSound;

	public GameObject cutFX;

	public bool limitCutCount;

	public int maxCutCount = 5;

	public bool limitCutPerObject;

	public int maxCutPerObject = 2;

	public bool newMatPerChunk = true;

	private bool cutting;

	private bool cut = true;

	private int cutCount;

	private float lastCutTime;

	private float trailRendTime;

	private float dragEndTime;

	private Transform tr;

	private Camera cam;

	private AudioSource audioSrc;

	private TrailRenderer trailRend;

	private Hashtable ropeHash = new Hashtable();

	private void Start()
	{
		tr = base.transform;
		cam = Camera.main;
		audioSrc = base.gameObject.AddComponent<AudioSource>();
		if (!useCutterObject)
		{
			trailRend = GetComponent<TrailRenderer>();
			trailRendTime = trailRend.time;
		}
		if (!limitCutCount)
		{
			maxCutCount = cutCount + 1;
		}
	}

	private void Update()
	{
		if (useCutterObject)
		{
			return;
		}
		if (useTouch ? (Input.touchCount > 0) : Input.GetMouseButton(0))
		{
			Vector3 position = ((!useTouch) ? cam.ScreenToWorldPoint(Input.mousePosition) : cam.ScreenToWorldPoint(Input.GetTouch(0).position));
			position.z = -1f;
			tr.position = position;
			cutting = true;
			if (Time.time > dragEndTime + 0.05f)
			{
				trailRend.time = trailRendTime;
			}
		}
		else
		{
			cutting = false;
			if (!useCutterObject)
			{
				trailRend.time = 0f;
			}
			dragEndTime = Time.time;
		}
	}

	private void OnTriggerExit2D(Collider2D col)
	{
		if (!(Time.time > lastCutTime + 0.2f) || col.isTrigger || cutCount >= maxCutCount || !cutting || !col.transform.parent || !(col.tag == "rope2D") || !col.GetComponent<HingeJoint2D>())
		{
			return;
		}
		if (limitCutPerObject)
		{
			int instanceID = col.transform.parent.GetInstanceID();
			if (ropeHash.ContainsKey(instanceID))
			{
				if ((int)ropeHash[instanceID] < maxCutPerObject - 1)
				{
					cut = true;
					ropeHash[instanceID] = (int)ropeHash[instanceID] + 1;
				}
				else
				{
					cut = false;
				}
			}
			else
			{
				ropeHash.Add(instanceID, 0);
				cut = true;
			}
		}
		if (!limitCutPerObject || (limitCutPerObject && cut))
		{
			if (!audioSrc.isPlaying)
			{
				audioSrc.clip = cutSound;
				audioSrc.Play();
			}
			col.GetComponent<HingeJoint2D>().enabled = false;
			col.isTrigger = true;
			col.GetComponent<Renderer>().enabled = false;
			if ((bool)cutFX)
			{
				Object.Destroy(Object.Instantiate(cutFX, col.transform.position, Quaternion.identity), 1f);
			}
			if ((bool)col.transform.parent && (bool)col.transform.parent.GetComponent<UseLineRenderer>())
			{
				col.transform.parent.GetComponent<UseLineRenderer>().Split(col.name, newMatPerChunk);
			}
			if (limitCutCount)
			{
				cutCount++;
			}
		}
		lastCutTime = Time.time;
	}

	private void OnTriggerEnter2D(Collider2D col)
	{
		if (!useCutterObject || !(Time.time > lastCutTime + 0.2f) || col.isTrigger || cutCount >= maxCutCount || !col.transform.parent || !(col.tag == "rope2D") || !col.GetComponent<HingeJoint2D>())
		{
			return;
		}
		if (limitCutPerObject)
		{
			int instanceID = col.transform.parent.GetInstanceID();
			if (ropeHash.ContainsKey(instanceID))
			{
				if ((int)ropeHash[instanceID] < maxCutPerObject - 1)
				{
					cut = true;
					ropeHash[instanceID] = (int)ropeHash[instanceID] + 1;
				}
				else
				{
					cut = false;
				}
			}
			else
			{
				ropeHash.Add(instanceID, 0);
				cut = true;
			}
		}
		if (!limitCutPerObject || (limitCutPerObject && cut))
		{
			if (!audioSrc.isPlaying)
			{
				audioSrc.clip = cutSound;
				audioSrc.Play();
			}
			col.GetComponent<HingeJoint2D>().enabled = false;
			col.isTrigger = true;
			col.GetComponent<Renderer>().enabled = false;
			if ((bool)cutFX)
			{
				Object.Destroy(Object.Instantiate(cutFX, col.transform.position, Quaternion.identity), 1f);
			}
			if ((bool)col.transform.parent && (bool)col.transform.parent.GetComponent<UseLineRenderer>())
			{
				col.transform.parent.GetComponent<UseLineRenderer>().Split(col.name, newMatPerChunk);
			}
			if (limitCutCount)
			{
				cutCount++;
			}
		}
		lastCutTime = Time.time;
	}
}
