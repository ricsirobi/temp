using UnityEngine;

public class ObRandomTextureSelector : MonoBehaviour
{
	public Texture2D[] _Textures;

	private void Awake()
	{
		int num = Random.Range(0, _Textures.Length);
		Texture2D texture2D = _Textures[num];
		if (texture2D != null)
		{
			UtUtilities.SetObjectTexture(base.gameObject, 0, texture2D);
		}
	}
}
