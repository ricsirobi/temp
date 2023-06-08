using UnityEngine;

public class ObPressurePlatePattern : ObPressurePlate
{
	public Renderer _Renderer;

	public ParticleSystem[] _ParticleSystems;

	public Renderer _ShapeRenderer;

	public void Init(Color color, Material inMaterial = null)
	{
		if ((bool)_Renderer)
		{
			_Renderer.material.color = color;
		}
		if (_ParticleSystems.Length != 0)
		{
			ParticleSystem[] particleSystems = _ParticleSystems;
			for (int i = 0; i < particleSystems.Length; i++)
			{
				ParticleSystem.MainModule main = particleSystems[i].main;
				main.startColor = color;
				color = new Color(color.r, color.g, color.b, 255f);
			}
		}
		if ((bool)_ShapeRenderer)
		{
			_ShapeRenderer.material = inMaterial;
		}
	}

	public void Reset()
	{
		mOneWayDone = false;
		SwitchOff();
		mOneWayDone = false;
	}
}
