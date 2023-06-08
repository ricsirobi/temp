using UnityEngine;

public class SilverLiningCirrusCloud
{
	private GameObject cloudTop;

	private GameObject cloudBottom;

	public SilverLiningCirrusCloud(Vector3 position, float size)
	{
		GameObject gameObject = GameObject.Find("CirrusClouds");
		GameObject gameObject2 = GameObject.Find("CirrusCloudPrefab");
		if (gameObject != null && gameObject2 != null)
		{
			cloudTop = Object.Instantiate(gameObject2, position, Quaternion.identity);
			cloudTop.transform.localScale = new Vector3(size, 1f, size);
			cloudTop.SetActive(value: true);
			cloudTop.GetComponent<Renderer>().material.renderQueue = 2002;
			cloudTop.GetComponent<MeshRenderer>().enabled = true;
			Quaternion rotation = Quaternion.AngleAxis(180f, new Vector3(1f, 0f, 0f));
			cloudBottom = Object.Instantiate(gameObject2, position, rotation);
			cloudBottom.transform.localScale = new Vector3(size, 1f, size);
			cloudBottom.SetActive(value: true);
			cloudBottom.GetComponent<Renderer>().material.renderQueue = 2002;
			cloudBottom.GetComponent<MeshRenderer>().enabled = true;
			cloudTop.transform.parent = gameObject.transform;
			cloudBottom.transform.parent = gameObject.transform;
		}
	}

	public void Destroy()
	{
		if (cloudTop != null)
		{
			Object.Destroy(cloudTop);
		}
		if (cloudBottom != null)
		{
			Object.Destroy(cloudBottom);
		}
	}
}
