using UnityEngine;

public class ProximitySphere : MonoBehaviour
{
	public float _ScaleFactor = 2.3f;

	public Renderer _SphereRenderer;

	public void SetColor(string property, Color inColor)
	{
		if (_SphereRenderer != null && _SphereRenderer.material.HasProperty(property))
		{
			_SphereRenderer.material.SetColor(property, inColor);
		}
	}

	public void ScaleWithFactor(float radius)
	{
		base.transform.localScale = Vector3.one * radius * _ScaleFactor;
	}
}
