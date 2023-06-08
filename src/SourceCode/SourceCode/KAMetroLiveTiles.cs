using UnityEngine;

public class KAMetroLiveTiles : MonoBehaviour
{
	public string[] _LiveTilesTexts;

	public string[] _LiveTilesIconsMedium;

	public string[] _LiveTilesIconsWide;

	public string[] _LiveTilesIconsLarge;

	private void Awake()
	{
		Object.Destroy(base.gameObject);
	}

	private void Start()
	{
	}
}
