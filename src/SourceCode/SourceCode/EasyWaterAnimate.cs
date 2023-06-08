using UnityEngine;

public class EasyWaterAnimate : MonoBehaviour
{
	public Vector2 texture1Speed;

	public Vector2 texture2Speed;

	public Vector2 bumpMap1Speed;

	public Vector2 bumpMap2Speed;

	public Vector2 distortSpeed;

	public Vector2 distort1Speed;

	public Vector2 distort2Speed;

	private Vector2 texture1UV;

	private Vector2 texture2UV;

	private Vector2 bumpMap1UV;

	private Vector2 bumpMap2UV;

	private Vector2 distortUV;

	private Vector2 distort1UV;

	private Vector2 distort2UV;

	private void Start()
	{
		texture1Speed /= 10f;
		texture2Speed /= 10f;
		bumpMap1Speed /= 10f;
		bumpMap2Speed /= 10f;
		distortSpeed = distort1Speed / 10f;
		distort1Speed /= 10f;
		distort2Speed /= 10f;
	}

	private void Update()
	{
		texture1UV.x = Time.time * texture1Speed.x;
		texture1UV.y = Time.time * texture1Speed.y;
		texture2UV.x = Time.time * texture2Speed.x;
		texture2UV.y = Time.time * texture2Speed.y;
		bumpMap1UV.x = Time.time * bumpMap1Speed.x;
		bumpMap1UV.y = Time.time * bumpMap1Speed.y;
		bumpMap2UV.x = Time.time * bumpMap2Speed.x;
		bumpMap2UV.y = Time.time * bumpMap2Speed.y;
		distortUV.x = Time.time * distort1Speed.x;
		distortUV.y = Time.time * distort1Speed.y;
		distort1UV.x = Time.time * distort1Speed.x;
		distort1UV.y = Time.time * distort1Speed.y;
		distort2UV.x = Time.time * distort2Speed.x;
		distort2UV.y = Time.time * distort2Speed.y;
		GetComponent<Renderer>().material.SetTextureOffset("_Texture1", texture1UV);
		GetComponent<Renderer>().material.SetTextureOffset("_Texture2", texture2UV);
		GetComponent<Renderer>().material.SetTextureOffset("_BumpMap1", bumpMap1UV);
		GetComponent<Renderer>().material.SetTextureOffset("_BumpMap2", bumpMap2UV);
		GetComponent<Renderer>().material.SetTextureOffset("_DistortionMap1", distort1UV);
		GetComponent<Renderer>().material.SetTextureOffset("_DistortionMap2", distort2UV);
	}
}
