using UnityEngine;

public class FontInstance
{
	public string _FontBundlePath = "";

	public string _FontBundleName = "";

	public Font _Font;

	public bool pIsReady;

	public FontData _FontData;

	public UIFont _FontAtlas;

	public void FontAssetBundleReady(string inURL, RsResourceLoadEvent inEvent, float inProgress, object inObject, object inUserData)
	{
		switch (inEvent)
		{
		case RsResourceLoadEvent.COMPLETE:
			_FontAtlas = inObject as UIFont;
			LocaleData.ReportLoaded();
			pIsReady = true;
			RsResourceManager.SetDontDestroy(_FontBundlePath, inDontDestroy: true);
			break;
		case RsResourceLoadEvent.ERROR:
			pIsReady = true;
			LocaleData.ReportLoaded();
			UtDebug.LogError("Error: Unable to load font " + inURL);
			break;
		}
	}

	public void Init(string fres)
	{
		string[] array = fres.Split('/');
		if (array.Length != 3)
		{
			UtDebug.LogError("Bad locale font res = " + fres);
			pIsReady = true;
			return;
		}
		_FontBundlePath = array[0] + "/" + array[1];
		_FontBundleName = array[2];
		if (_FontBundlePath.Length == 0 || _FontBundleName.Length == 0)
		{
			UtDebug.LogError("Bad locale font res = " + fres);
			pIsReady = true;
		}
	}

	public void LoadRes()
	{
		RsResourceManager.LoadAssetFromBundle(_FontBundlePath, _FontBundleName, FontAssetBundleReady, typeof(UIFont), inDontDestroy: true);
	}
}
