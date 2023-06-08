using UnityEngine;

public class RegisterPrompt : MonoBehaviour
{
	public float _WaitTime = 2f;

	private void ShowRegisterPrompt()
	{
		RegisterMsg registerMsg = MonetizationData.GetRegisterMsg("AppLaunch");
		if (registerMsg != null && GameUtilities.HasPromptFrequencyReached("APP_LAUNCH_COUNT", registerMsg.mFrequency))
		{
			GameUtilities.ShowRegisterMsg(registerMsg.mMessage.GetLocalizedString().Replace("{$}", registerMsg.mCredits.ToString()), base.gameObject);
		}
	}

	private void OnRegisterYes()
	{
		GameUtilities.LoadLoginLevel(showRegstration: true);
	}

	private void OnRegisterNo()
	{
		Object.Destroy(base.gameObject);
	}

	public void OnWaitListCompleted()
	{
		if (!UiLogin.pIsGuestUser || UiLogin.pGuestUserFirstLaunch || KAUI._GlobalExclusiveUI != null || RsResourceManager.pLastLevel != "ProfileSelectionDO")
		{
			Object.Destroy(base.gameObject);
		}
		else
		{
			ShowRegisterPrompt();
		}
	}
}
