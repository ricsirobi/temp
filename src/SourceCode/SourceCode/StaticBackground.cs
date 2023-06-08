using UnityEngine;

[ExecuteInEditMode]
[AddComponentMenu("Image Effects/Static Background")]
public class StaticBackground : MonoBehaviour
{
	public Texture2D background;

	private void OnPreRender()
	{
		if (background != null)
		{
			Graphics.Blit(background, RenderTexture.active);
		}
	}
}
