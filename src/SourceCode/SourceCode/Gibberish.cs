using UnityEngine;

public class Gibberish : MonoBehaviour
{
	public GibberishString[] _Strings;

	public string Get()
	{
		if (_Strings != null && _Strings.Length != 0)
		{
			int num = Random.Range(0, _Strings.Length);
			return StringTable.GetStringData(_Strings[num]._ID, _Strings[num]._Text);
		}
		return "";
	}
}
