using UnityEngine;

public class MemoryProfilerInGame : MonoBehaviour
{
	[HideInInspector]
	public MemoryProfiler _MemoryProfiler = new MemoryProfiler();

	private void Start()
	{
		_MemoryProfiler._OwnerGO = base.gameObject;
	}

	private void Update()
	{
	}

	private void OnGUI()
	{
		GUILayout.BeginArea(new Rect(0f, 0f, Screen.width, Screen.height));
		_MemoryProfiler.RenderGUI();
		GUILayout.EndArea();
	}
}
