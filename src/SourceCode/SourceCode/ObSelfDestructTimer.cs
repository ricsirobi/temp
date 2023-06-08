using UnityEngine;

public class ObSelfDestructTimer : MonoBehaviour
{
	public float _SelfDestructTime = 1f;

	public SpawnPool _Pool;

	public virtual void OnEnable()
	{
		Invoke("DestroySelf", _SelfDestructTime);
	}

	public void OnDisable()
	{
		CancelInvoke("DestroySelf");
	}

	public void DestroySelf()
	{
		if (_Pool != null)
		{
			_Pool.Despawn(base.transform);
		}
		else
		{
			Object.Destroy(base.gameObject);
		}
	}
}
