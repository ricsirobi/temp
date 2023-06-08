using System.Collections;
using UnityEngine;

public class SoundWaitForLoad : MonoBehaviour
{
	private SnChannel m_SnChannel;

	private float m_MaxVolume;

	private void Awake()
	{
		m_SnChannel = GetComponent<SnChannel>();
		m_MaxVolume = m_SnChannel._RolloffMaxVolume;
		m_SnChannel._RolloffMaxVolume = 0f;
		StartCoroutine(WaitForLoadingScreen());
	}

	private IEnumerator WaitForLoadingScreen()
	{
		yield return new WaitUntil(() => !RsResourceManager.pLevelLoadingScreen);
		float t = 0f;
		while (t < m_MaxVolume)
		{
			yield return new WaitForSeconds(0.1f);
			t += 0.1f;
			m_SnChannel._RolloffMaxVolume = t;
		}
	}
}
