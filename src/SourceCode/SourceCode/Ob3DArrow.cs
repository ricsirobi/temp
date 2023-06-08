using UnityEngine;

public class Ob3DArrow : MonoBehaviour
{
	private bool mShow = true;

	private Renderer[] mRenderers;

	public GameObject pTarget;

	public Vector3 _TargetDirection;

	public float _ActivationDistY = 2f;

	public bool _ShowAlways;

	public bool pVisible => mShow;

	private void Update()
	{
		if (mShow)
		{
			base.transform.forward = Vector3.Lerp(base.transform.forward, _TargetDirection, Time.deltaTime * 5f);
		}
	}

	private void Start()
	{
		mRenderers = GetComponents<Renderer>();
		if (mRenderers == null || mRenderers.Length == 0)
		{
			mRenderers = GetComponentsInChildren<Renderer>();
		}
		Show(inShow: false);
	}

	public void Show(bool inShow)
	{
		if (mShow && pTarget != null && !_ShowAlways && base.transform.position.y - pTarget.transform.position.y < _ActivationDistY)
		{
			inShow = false;
		}
		if (inShow == mShow)
		{
			return;
		}
		mShow = inShow;
		if (mRenderers == null || mRenderers.Length == 0)
		{
			return;
		}
		Renderer[] array = mRenderers;
		foreach (Renderer renderer in array)
		{
			if (renderer != null)
			{
				renderer.enabled = mShow;
			}
		}
	}
}
