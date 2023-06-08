using UnityEngine;

public class ObSticky : MonoBehaviour
{
	public float _Radius = 10f;

	public float _LerpTime = 0.25f;

	public bool _StickyOnGrounded;

	public float _StickyAfterSeconds = 2f;

	public float _MinCollectDistance = 0.2f;

	private bool mIsGrounded;

	private float mElapsedTime;

	private void Start()
	{
	}

	private void Update()
	{
		mElapsedTime += Time.deltaTime;
		if ((_StickyOnGrounded && !mIsGrounded) || (!_StickyOnGrounded && mElapsedTime < _StickyAfterSeconds) || !(AvAvatar.pObject != null))
		{
			return;
		}
		Vector3 b = AvAvatar.pObject.GetComponent<Collider>().ClosestPointOnBounds(base.transform.position);
		float num = Vector3.Distance(base.transform.position, b);
		float num2 = Vector3.Distance(base.transform.position, AvAvatar.position);
		if (num <= _MinCollectDistance || num2 <= _MinCollectDistance)
		{
			ObCollect component = GetComponent<ObCollect>();
			if (component != null)
			{
				component.OnItemCollected();
			}
		}
		else if (num2 <= _Radius)
		{
			base.transform.position = Vector3.Lerp(base.transform.position, AvAvatar.position, _LerpTime);
		}
	}

	private void OnCollisionEnter(Collision collision)
	{
		if (collision.collider.gameObject == AvAvatar.pObject)
		{
			ObCollect component = GetComponent<ObCollect>();
			if (component != null)
			{
				component.OnItemCollected();
			}
		}
	}
}
