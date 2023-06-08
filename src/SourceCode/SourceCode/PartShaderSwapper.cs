using UnityEngine;

public class PartShaderSwapper : MonoBehaviour
{
	private Shader mOriginalShader;

	public void SwapShader(Material material, Shader shader)
	{
		mOriginalShader = material.shader;
		material.shader = shader;
	}

	public void RestoreShader(Material material)
	{
		if (mOriginalShader != null)
		{
			material.shader = mOriginalShader;
		}
		mOriginalShader = null;
		Object.DestroyImmediate(this);
	}
}
