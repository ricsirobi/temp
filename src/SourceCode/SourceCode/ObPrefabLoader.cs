using UnityEngine;

public class ObPrefabLoader : MonoBehaviour
{
	public string _PrefabName;

	public void Start()
	{
		if (!string.IsNullOrEmpty(_PrefabName))
		{
			GameObject gameObject = Object.Instantiate((GameObject)RsResourceManager.LoadAssetFromResources(_PrefabName));
			if (gameObject != null)
			{
				gameObject.transform.parent = base.transform;
				gameObject.transform.localPosition = Vector3.zero;
				gameObject.transform.localRotation = Quaternion.identity;
			}
		}
	}
}
