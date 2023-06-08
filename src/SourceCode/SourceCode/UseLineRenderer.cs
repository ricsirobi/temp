using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class UseLineRenderer : MonoBehaviour
{
	public static bool brokenChainsHolderCreated;

	private LineRenderer lineRend;

	public Material ropeMaterial;

	public float width;

	private List<Transform> chains;

	private GameObject brokenChainsHolder;

	private Transform tr;

	private bool started;

	private float startChainCount;

	private void Start()
	{
		tr = base.transform;
		if (!brokenChainsHolderCreated)
		{
			brokenChainsHolder = GameObject.Find("Broken Chains Holder");
			if (brokenChainsHolder == null)
			{
				brokenChainsHolder = new GameObject("Broken Chains Holder");
				brokenChainsHolderCreated = true;
			}
		}
		if (chains == null)
		{
			chains = new List<Transform>();
			foreach (Transform item in tr)
			{
				chains.Add(item);
			}
		}
		if (chains.Count > 0 && !GetComponent<LineRenderer>())
		{
			lineRend = base.gameObject.AddComponent<LineRenderer>();
			lineRend.positionCount = chains.Count;
			lineRend.material = ropeMaterial;
		}
		else
		{
			lineRend = GetComponent<LineRenderer>();
		}
		if ((bool)lineRend)
		{
			if (width <= 0f)
			{
				width = chains[0].GetComponent<Renderer>().bounds.size.x;
			}
			lineRend.startWidth = width;
			lineRend.endWidth = width;
		}
		startChainCount = chains.Count;
		started = true;
	}

	public void AddChain(Transform chain)
	{
		if (chains == null)
		{
			chains = new List<Transform>();
		}
		if (lineRend == null)
		{
			lineRend = GetComponent<LineRenderer>();
			if (!lineRend)
			{
				lineRend = base.gameObject.AddComponent<LineRenderer>();
				lineRend.material = ropeMaterial;
			}
		}
		chains.Add(chain);
		lineRend.positionCount = chains.Count;
		lineRend.SetPosition(chains.Count - 1, chains[chains.Count - 1].position);
		startChainCount = chains.Count;
	}

	private void Update()
	{
		if (tr.childCount < 1)
		{
			Object.Destroy(base.gameObject);
		}
		else if (chains != null && chains.Count > 0)
		{
			for (int i = 0; i < chains.Count; i++)
			{
				lineRend.SetPosition(i, chains[i].position);
			}
		}
	}

	public void Split(string name, bool newMatPerChunk)
	{
		if (!started)
		{
			Start();
		}
		if (chains == null || chains.Count <= 0)
		{
			return;
		}
		GameObject gameObject = new GameObject("new part");
		bool flag = false;
		foreach (Transform chain in chains)
		{
			if (!flag && name == chain.name)
			{
				flag = true;
			}
			if (flag)
			{
				HingeJoint2D component = chain.GetComponent<HingeJoint2D>();
				if (((bool)component && component.enabled && (bool)component.connectedBody) || (!component && (chain == chains[0] || chain == chains[chains.Count - 1])))
				{
					chain.parent = gameObject.transform;
					continue;
				}
				chain.parent = ((brokenChainsHolder != null) ? brokenChainsHolder.transform : null);
				chain.GetComponent<Collider2D>().isTrigger = true;
			}
		}
		chains.Clear();
		foreach (Transform item in tr)
		{
			chains.Add(item);
		}
		lineRend.positionCount = chains.Count;
		UseLineRenderer useLineRenderer = gameObject.AddComponent<UseLineRenderer>();
		if (newMatPerChunk)
		{
			Material material = new Material(ropeMaterial);
			float num = startChainCount / (startChainCount - (float)chains.Count - 1f);
			float x = ropeMaterial.mainTextureScale.x / num;
			material.mainTextureScale = new Vector2(x, ropeMaterial.mainTextureScale.y);
			useLineRenderer.ropeMaterial = material;
			useLineRenderer.width = width;
			if (chains.Count > 0)
			{
				Material material2 = new Material(ropeMaterial);
				float num2 = startChainCount / (float)chains.Count;
				float x2 = ropeMaterial.mainTextureScale.x / num2;
				material2.mainTextureScale = new Vector2(x2, ropeMaterial.mainTextureScale.y);
				lineRend.material = material2;
				ropeMaterial = material2;
			}
			startChainCount = chains.Count;
		}
		else
		{
			useLineRenderer.ropeMaterial = ropeMaterial;
		}
	}
}
