using System;
using UnityEngine;

namespace Zendesk.UI;

public class ZendeskPauseHandler : MonoBehaviour
{
	private static ZendeskMain zendeskMain;

	public void init(ZendeskMain main)
	{
		zendeskMain = main;
	}

	public void PauseApplication()
	{
		if (zendeskMain.autoPause)
		{
			Time.timeScale = 0f;
		}
	}

	public void ResumeApplication()
	{
		if (zendeskMain.autoPause)
		{
			Time.timeScale = 1f;
		}
	}

	internal void PauseApplicationInternal()
	{
		try
		{
			if (zendeskMain.autoPause)
			{
				zendeskMain.pauseFunctionality.Invoke();
			}
		}
		catch (Exception ex)
		{
			throw ex;
		}
	}

	internal void ResumeApplicationInternal()
	{
		try
		{
			if (zendeskMain.autoPause)
			{
				zendeskMain.resumeFunctionality.Invoke();
			}
		}
		catch (Exception ex)
		{
			throw ex;
		}
	}
}
