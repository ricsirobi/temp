using UnityEngine;

namespace ShatterToolkit.Helpers;

[RequireComponent(typeof(ShatterTool))]
public class PieceRemover : MonoBehaviour
{
	public int startAtGeneration = 3;

	public float timeDelay = 5f;

	public bool whenOutOfViewOnly = true;

	protected ShatterTool shatterTool;

	protected Renderer renderer;

	protected float timeSinceInstantiated;

	public void Start()
	{
		shatterTool = GetComponent<ShatterTool>();
		renderer = GetComponent<Renderer>();
	}

	public void Update()
	{
		if (shatterTool.Generation >= startAtGeneration)
		{
			timeSinceInstantiated += Time.deltaTime;
			if (timeSinceInstantiated >= timeDelay && (!whenOutOfViewOnly || !renderer.isVisible))
			{
				Object.Destroy(base.gameObject);
			}
		}
	}
}
