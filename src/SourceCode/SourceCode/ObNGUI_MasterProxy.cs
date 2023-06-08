using UnityEngine;

public class ObNGUI_MasterProxy : MonoBehaviour
{
	private ObNGUI_Proxy[] Proxies;

	private int nProxies;

	private Transform mTransform;

	private Vector3 mLastPosition = Vector3.zero;

	private Quaternion mLastRotation = Quaternion.identity;

	private void Awake()
	{
		Proxies = base.gameObject.GetComponentsInChildren<ObNGUI_Proxy>(includeInactive: true);
		nProxies = Proxies.Length;
		mTransform = base.transform;
		mLastPosition = mTransform.position;
		mLastRotation = mTransform.rotation;
		UpdateData(bForce: true);
	}

	public void LateUpdate()
	{
		UpdateData(bForce: false);
	}

	public void OnDisable()
	{
		for (int i = 0; i < nProxies; i++)
		{
			Proxies[i].OnDisable();
		}
	}

	public void OnEnable()
	{
		for (int i = 0; i < nProxies; i++)
		{
			Proxies[i].OnEnable();
		}
	}

	public void UpdateData(bool bForce)
	{
		if (nProxies <= 0)
		{
			return;
		}
		Vector3 position = mTransform.position;
		Quaternion rotation = mTransform.rotation;
		bool flag = false;
		if (mLastPosition != position)
		{
			mLastPosition = position;
			flag = true;
		}
		if (mLastRotation != rotation)
		{
			mLastRotation = rotation;
			flag = true;
		}
		if (flag || bForce)
		{
			for (int i = 0; i < nProxies; i++)
			{
				Proxies[i].UpdateData();
			}
		}
	}
}
