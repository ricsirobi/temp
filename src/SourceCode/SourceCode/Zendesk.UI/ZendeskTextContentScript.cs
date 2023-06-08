using System;
using UnityEngine;
using UnityEngine.UI;

namespace Zendesk.UI;

public class ZendeskTextContentScript : MonoBehaviour
{
	public Text[] textContent;

	public void Init(string[] contentList)
	{
		try
		{
			for (int i = 0; i < contentList.Length; i++)
			{
				textContent[i].text = contentList[i];
			}
			base.gameObject.SetActive(value: true);
		}
		catch (Exception ex)
		{
			throw ex;
		}
	}
}
