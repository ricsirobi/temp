using UnityEngine;

public class CTFuse : PhysicsObject
{
	private CTDynamite mDynamite;

	private CTBlowTorch mTorch;

	private void Start()
	{
		mDynamite = base.transform.parent.gameObject.GetComponent<CTDynamite>();
	}

	private void OnTriggerStay2D(Collider2D other)
	{
		if (other.name.Contains("FlameCollider"))
		{
			SendSignal(other);
		}
	}

	private void Update()
	{
	}

	private void SendSignal(Collider2D torch)
	{
		mTorch = torch.transform.parent.GetComponent<CTBlowTorch>();
		if (mDynamite == null)
		{
			mDynamite = Object.FindObjectOfType<CTDynamite>();
		}
		else if (mDynamite.GetReady() && mTorch.pIsOn)
		{
			mDynamite.LightFuse();
		}
	}
}
