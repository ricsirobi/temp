using System.Collections;
using JSGames.UI.Util;
using UnityEngine;
using UnityEngine.UI;

namespace JSGames.UI;

[RequireComponent(typeof(Text))]
public class TextLocalize : KAMonoBase
{
	[Tooltip("ID used for localization")]
	public int _TextID;

	private Text mText;

	protected void Awake()
	{
		mText = GetComponent<Text>();
	}

	protected void Start()
	{
		mText.supportRichText = true;
		StartCoroutine(ProcessLocaleText(mText.text));
	}

	private IEnumerator ProcessLocaleText(string defaultText)
	{
		yield return new WaitUntil(() => LocaleData.pIsReady);
		if (mText.text == defaultText)
		{
			string stringData = StringTable.GetStringData(_TextID, mText.text);
			mText.text = UIUtil.NGUIToUGUIConvert(stringData);
		}
	}
}
