using UnityEngine;

namespace Xsolla;

public class ScreenErrorController : ScreenBaseConroller<XsollaError>
{
	public LinearLayout linerLayout;

	public void Init()
	{
		linerLayout = GetComponent<LinearLayout>();
	}

	public override void InitScreen(XsollaTranslations translations, XsollaError error)
	{
		Init();
		DrawScreen(translations, error);
	}

	public void DrawScreen(XsollaError error)
	{
		linerLayout.AddObject(GetError(error));
		linerLayout.Invalidate();
	}

	private void DrawScreen(XsollaTranslations translations, XsollaError error)
	{
		linerLayout.AddObject(GetError(error));
		linerLayout.AddObject(GetButton(translations.Get(XsollaTranslations.FORM_CONTINUE), delegate
		{
			OnClickButton(error);
		}));
		linerLayout.AddObject(GetHelp(translations));
		linerLayout.Invalidate();
	}

	public void OnClickButton(XsollaError error)
	{
		OnErrorRecived(error);
		Object.Destroy(base.gameObject.GetComponentInParent<XsollaPaystationController>().gameObject);
	}
}
