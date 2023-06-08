using System.Collections;
using UnityEngine;

internal class AnimateTiledTexture : KAMonoBase
{
	public int columns = 2;

	public int rows = 2;

	public float framesPerSecond = 10f;

	private int index;

	private void Start()
	{
		StartCoroutine(updateTiling());
		Vector2 value = new Vector2(1f / (float)columns, 1f / (float)rows);
		base.renderer.sharedMaterial.SetTextureScale("_MainTex", value);
	}

	private IEnumerator updateTiling()
	{
		while (true)
		{
			index++;
			if (index >= rows * columns)
			{
				index = 0;
			}
			Vector2 value = new Vector2((float)index / (float)columns - (float)(index / columns), (float)(index / columns) / (float)rows);
			base.renderer.sharedMaterial.SetTextureOffset("_MainTex", value);
			yield return new WaitForSeconds(1f / framesPerSecond);
		}
	}
}
