using UnityEngine;

public class ObPlatformCheck : MonoBehaviour
{
	public bool _DestroyOnMobile;

	public bool _DestroyOnWeb;

	private void Awake()
	{
		if (_DestroyOnMobile && UtPlatform.IsMobile())
		{
			Object.Destroy(base.gameObject);
		}
		if (_DestroyOnWeb && !UtPlatform.IsMobile())
		{
			Object.Destroy(base.gameObject);
		}
	}
}
