using UnityEngine;

public class ShieldUvAnimation : MonoBehaviour
{
	public GameObject iShield;

	public float iSpeed;

	private Material mMaterial;

	private float mTime;

	private void Start()
	{
		mMaterial = iShield.GetComponent<Renderer>().material;
		mTime = 0f;
	}

	private void Update()
	{
		mTime += Time.deltaTime * iSpeed;
		mMaterial.SetFloat("_Offset", Mathf.Repeat(mTime, 1f));
	}
}
