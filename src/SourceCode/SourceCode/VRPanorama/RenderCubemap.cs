using UnityEngine;
using UnityEngine.Rendering;

namespace VRPanorama;

public class RenderCubemap : MonoBehaviour
{
	public int cubemapSize = 1024;

	public Camera cam;

	private RenderTexture rtex;

	private void Start()
	{
		UpdateCubemap();
	}

	private void LateUpdate()
	{
		UpdateCubemap();
	}

	private void UpdateCubemap()
	{
		if (!rtex)
		{
			rtex = new RenderTexture(cubemapSize, cubemapSize, 0);
			rtex.dimension = TextureDimension.Cube;
			rtex.hideFlags = HideFlags.HideAndDontSave;
			rtex.autoGenerateMips = false;
			GetComponent<Renderer>().sharedMaterial.SetTexture("_Cube", rtex);
		}
		cam.transform.position = base.transform.position;
		cam.RenderToCubemap(rtex, 63);
	}

	private void OnDisable()
	{
		Object.DestroyImmediate(rtex);
	}
}
