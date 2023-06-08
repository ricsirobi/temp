using JSGames.UI;

namespace JSGames.Resource;

public class BundleUIItemData : CoBundleItemData
{
	public JSGames.UI.UIWidget pWidget { get; set; }

	public BundleUIItemData()
	{
	}

	public BundleUIItemData(string tn, string sn)
		: base(tn, sn)
	{
	}

	public override void OnAllDownloaded()
	{
		base.OnAllDownloaded();
		JSGames.UI.UIWidget uIWidget = pWidget;
		if (uIWidget != null)
		{
			uIWidget.mainTexture = _ItemTextureData._Texture;
			uIWidget.name = _ItemTextureData._ResName;
			if (_ItemRVOData._Clip != null)
			{
				uIWidget._HoverEffects._Clip._AudioClip = _ItemRVOData._Clip;
				uIWidget._HoverEffects._Clip._Settings._Pool = "VO_Pool";
				uIWidget._HoverEffects._Clip._Settings._Priority = 0;
			}
			pIsReady = true;
			uIWidget.pVisible = true;
		}
	}
}
