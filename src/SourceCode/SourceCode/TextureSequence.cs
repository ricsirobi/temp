using System.Collections;
using UnityEngine;

public class TextureSequence : MonoBehaviour
{
	public int MyTextureIndex;

	public Texture2D[] MyTextures;

	public float MyDelay = 0.03f;

	private void Start()
	{
		StartCoroutine(AnimateTextures());
	}

	private IEnumerator AnimateTextures()
	{
		while (true)
		{
			GetComponent<Light>().cookie = MyTextures[MyTextureIndex];
			yield return new WaitForSeconds(MyDelay);
			MyTextureIndex++;
			if (MyTextureIndex >= MyTextures.Length)
			{
				MyTextureIndex = 0;
			}
		}
	}
}
