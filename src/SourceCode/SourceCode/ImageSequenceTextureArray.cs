using System.Collections;
using UnityEngine;

public class ImageSequenceTextureArray : MonoBehaviour
{
	private Object[] objects;

	private Texture[] textures;

	private Material goMaterial;

	private int frameCounter;

	private void Awake()
	{
		goMaterial = GetComponent<Renderer>().material;
	}

	private void Start()
	{
		objects = Resources.LoadAll("Sequence", typeof(Texture));
		textures = new Texture[objects.Length];
		for (int i = 0; i < objects.Length; i++)
		{
			textures[i] = (Texture)objects[i];
		}
	}

	private void Update()
	{
		StartCoroutine("PlayLoop", 0.04f);
		goMaterial.mainTexture = textures[frameCounter];
	}

	private IEnumerator PlayLoop(float delay)
	{
		yield return new WaitForSeconds(delay);
		frameCounter = ++frameCounter % textures.Length;
		StopCoroutine("PlayLoop");
	}

	private IEnumerator Play(float delay)
	{
		yield return new WaitForSeconds(delay);
		if (frameCounter < textures.Length - 1)
		{
			frameCounter++;
		}
		StopCoroutine("PlayLoop");
	}
}
