using UnityEngine;

public class EasyWaterAnimateUnderwater : MonoBehaviour
{
	public Vector2 distort1Speed;

	public Vector2 distort2Speed;

	private Vector2 distortionMap1UV;

	private Vector2 distortionMap2UV;

	private void Start()
	{
		distort1Speed /= 10f;
		distort2Speed /= 10f;
	}

	private void Update()
	{
		distortionMap1UV.x = Time.time * distort1Speed.x;
		distortionMap1UV.y = Time.time * distort1Speed.y;
		distortionMap2UV.x = Time.time * distort2Speed.x;
		distortionMap2UV.y = Time.time * distort2Speed.y;
		GetComponent<Renderer>().material.SetTextureOffset("_DistortionMap1", distortionMap1UV);
		GetComponent<Renderer>().material.SetTextureOffset("_DistortionMap2", distortionMap2UV);
	}
}
