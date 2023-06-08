using UnityEngine;

public class ObResetToMarker : MonoBehaviour
{
	public Transform[] _ObjectFilter;

	public string[] _ObjectStringFilter;

	public Transform _Marker;

	public string _MarkerName = "";

	public AudioClip _Sound;

	public AudioClip _VO;

	public bool _PlayTeleportFX;

	public float _DisableAvatarInputTime;

	public bool _Deactivate;

	public bool _RemoveSpeed;

	public bool _IgnoreTriggers = true;

	private float mDisableAvatarInputTimer;

	private void Start()
	{
		if (_MarkerName.Length > 0)
		{
			GameObject gameObject = GameObject.Find(_MarkerName);
			if (gameObject != null)
			{
				_Marker = gameObject.transform;
			}
		}
	}

	private void Update()
	{
		if (mDisableAvatarInputTimer > 0f)
		{
			mDisableAvatarInputTimer -= Time.deltaTime;
			if (mDisableAvatarInputTimer <= 0f)
			{
				AvAvatar.pInputEnabled = true;
			}
		}
	}

	private void Reset(Transform objectHit)
	{
		if (_RemoveSpeed)
		{
			Rigidbody component = objectHit.GetComponent<Rigidbody>();
			if (component != null)
			{
				component.velocity = new Vector3(0f, 0f, 0f);
				component.angularVelocity = new Vector3(0f, 0f, 0f);
			}
		}
		if (AvAvatar.IsCurrentPlayer(objectHit.gameObject))
		{
			AvAvatar.SetPosition(_Marker);
			if (_DisableAvatarInputTime > 0f)
			{
				mDisableAvatarInputTimer = _DisableAvatarInputTime;
				AvAvatar.pInputEnabled = false;
			}
			if (AvAvatar.pState == AvAvatarState.NONE)
			{
				MainStreetMMOClient.pInstance.SendUpdate(MMOAvatarFlags.SETPOSITION);
			}
		}
		else
		{
			objectHit.position = _Marker.position;
			objectHit.rotation = _Marker.rotation;
		}
		if (_PlayTeleportFX)
		{
			TeleportFx.PlayAt(_Marker.position);
		}
		else if (_Sound != null)
		{
			AudioSource.PlayClipAtPoint(_Sound, Camera.main.transform.position);
		}
		if (_VO != null)
		{
			SnChannel.Play(_VO, "VO_Pool", inForce: true, null);
		}
		if (_Deactivate)
		{
			objectHit.gameObject.SetActive(value: false);
		}
	}

	protected virtual void OnTriggerEnter(Collider other)
	{
		if (_IgnoreTriggers && other.isTrigger)
		{
			return;
		}
		Transform[] objectFilter = _ObjectFilter;
		foreach (Transform transform in objectFilter)
		{
			if (other.transform == transform)
			{
				Reset(transform);
				return;
			}
		}
		string[] objectStringFilter = _ObjectStringFilter;
		foreach (string text in objectStringFilter)
		{
			if (other.gameObject.name == text)
			{
				Reset(other.transform);
				break;
			}
		}
	}
}
