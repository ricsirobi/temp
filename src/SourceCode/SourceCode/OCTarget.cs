using UnityEngine;

public class OCTarget : MonoBehaviour
{
	public Transform _ParticlePf;

	public GameObject _PrtObject;

	public AudioClip _HitSFX;

	private bool mHit;

	private void OnDisableParticle()
	{
		mHit = false;
		if (_PrtObject != null)
		{
			_PrtObject.transform.parent = null;
			Object.Destroy(_PrtObject);
			_PrtObject = null;
		}
	}

	private void PlayParticle(bool active)
	{
		mHit = active;
		if (!(_PrtObject == null))
		{
			_PrtObject.SetActive(active);
		}
	}

	private void OnCollisionEnter(Collision inCollision)
	{
		if (!mHit && inCollision.gameObject.GetComponent<ObAmmo>() != null)
		{
			if (_PrtObject == null && _ParticlePf != null)
			{
				_PrtObject = Object.Instantiate(_ParticlePf.gameObject);
				_PrtObject.transform.parent = base.transform;
				_PrtObject.transform.localScale = Vector3.one;
				_PrtObject.transform.localPosition = Vector3.zero;
				_PrtObject.transform.localRotation = Quaternion.identity;
			}
			PlayParticle(active: true);
			if (AvAvatar.pToolbar != null)
			{
				AvAvatar.pToolbar.SendMessage("Collect", base.gameObject);
			}
		}
	}
}
