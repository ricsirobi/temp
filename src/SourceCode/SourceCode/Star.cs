using UnityEngine;

public class Star : ObjectBase
{
	public ParticleSystem particle;

	public Renderer mainRenderer;

	public AudioClip _Sound;

	private bool canCollect = true;

	private bool inPlayMode;

	private void OnTriggerEnter2D(Collider2D other)
	{
		if (inPlayMode && canCollect)
		{
			Collected();
		}
	}

	public override void Enable()
	{
		inPlayMode = true;
		base.collider2D.isTrigger = true;
	}

	public override void Reset()
	{
		inPlayMode = false;
		canCollect = true;
		mainRenderer.enabled = true;
	}

	private void Collected()
	{
		canCollect = false;
		mainRenderer.enabled = false;
		IMGLevelManager.pInstance.StarCollected();
		particle.Play();
		if (_Sound != null)
		{
			SnChannel.Play(_Sound, "", inForce: true);
		}
	}
}
