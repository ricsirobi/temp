using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Zendesk.Internal.Models.Common;
using Zendesk.Internal.Models.Core;

namespace Zendesk.UI;

public class ZendeskErrorUI : MonoBehaviour
{
	public ZendeskErrorEvent zendeskErrorEvent;

	public GameObject zendeskErrorToastUIGO;

	public GameObject zendeskErrorUIGO;

	public GameObject zendeskFullScreenErrorTitleContainer;

	public GameObject zendeskFullScreenErrorBackButtonContainer;

	public GameObject zendeskFullScreenErrorReloadButtonContainer;

	public GameObject zendeskFullScreenErrorExitButtonContainer;

	public Text zendeskErrorFullScreenTitleText;

	public Text zendeskErrorFullScreenDescriptionText;

	public Text zendeskErrorFullScreenButtonText;

	public Text zendeskErrorFullScreenExitButtonText;

	[HideInInspector]
	public ZendeskUI zendeskUI;

	private static List<GameObject> errorToasts = new List<GameObject>();

	private ZendeskLocalizationHandler zendeskLocalizationHandler;

	private string authScreenErrorTitle;

	private string authScreenErrorBody;

	private string jwtIntegratorError;

	public void Init(ZendeskLocalizationHandler localizationHandler)
	{
		zendeskLocalizationHandler = localizationHandler;
		SetStrings();
	}

	private void SetStrings()
	{
		zendeskErrorFullScreenTitleText.text = zendeskLocalizationHandler.translationGameObjects["usdk_generic_error_message_title"];
		zendeskErrorFullScreenDescriptionText.text = zendeskLocalizationHandler.translationGameObjects["usdk_generic_error_message_body_content"];
		zendeskErrorFullScreenButtonText.text = zendeskLocalizationHandler.translationGameObjects["usdk_generic_error_button"];
		zendeskErrorFullScreenExitButtonText.text = zendeskLocalizationHandler.translationGameObjects["usdk_back_button_label"];
		authScreenErrorTitle = zendeskLocalizationHandler.translationGameObjects["usdk_jwt_error_message"];
		authScreenErrorBody = zendeskLocalizationHandler.translationGameObjects["usdk_error_page_message_new_design"];
		jwtIntegratorError = zendeskLocalizationHandler.translationGameObjects["usdk_jwt_token_integrator_error"];
	}

	public void NavigateError(string content, bool? isFullScreen = false, bool? isButtonReloads = true, string fullScreenErrorTitle = null)
	{
		zendeskErrorEvent.Invoke(content, isFullScreen.Value, isButtonReloads.Value, fullScreenErrorTitle);
	}

	public void NavigateError<T>(ZendeskResponse<T> response, bool? isFullScreen = false, bool? isButtonReloads = true, string fullScreenErrorTitle = null)
	{
		if (response.IsError)
		{
			if (response.ErrorResponse != null && response.ErrorResponse.Reason != null)
			{
				zendeskErrorEvent.Invoke(response.ErrorResponse.Reason, isFullScreen.Value, isButtonReloads.Value, fullScreenErrorTitle);
			}
		}
		else
		{
			zendeskErrorEvent.Invoke("", isFullScreen.Value, isButtonReloads.Value, fullScreenErrorTitle);
		}
	}

	public void ZendeskError(string content, bool isFullScreen, bool isButtonReloads, string fullScreenErrorTitle = null)
	{
		if (isFullScreen)
		{
			SetStrings();
			if (!string.IsNullOrEmpty(content))
			{
				zendeskErrorFullScreenDescriptionText.text = content;
			}
			if (fullScreenErrorTitle != null)
			{
				if (fullScreenErrorTitle.Trim().Equals("hide"))
				{
					zendeskFullScreenErrorTitleContainer.SetActive(value: false);
				}
				else
				{
					zendeskFullScreenErrorTitleContainer.SetActive(value: true);
					zendeskErrorFullScreenTitleText.text = fullScreenErrorTitle;
				}
			}
			else
			{
				zendeskFullScreenErrorTitleContainer.SetActive(value: true);
			}
			zendeskErrorUIGO.SetActive(value: true);
			if (!isButtonReloads)
			{
				zendeskFullScreenErrorReloadButtonContainer.SetActive(value: false);
				zendeskFullScreenErrorExitButtonContainer.SetActive(value: true);
				zendeskFullScreenErrorBackButtonContainer.SetActive(value: false);
			}
			else
			{
				zendeskFullScreenErrorReloadButtonContainer.SetActive(value: true);
				zendeskFullScreenErrorExitButtonContainer.SetActive(value: false);
				zendeskUI.ShowBackButton(zendeskFullScreenErrorBackButtonContainer);
			}
		}
		else
		{
			ShowToast(content);
		}
	}

	private void ShowToast(string content)
	{
		if (zendeskUI.backStateGO.Count > 0)
		{
			GameObject screenGO = zendeskUI.backStateGO[zendeskUI.backStateGO.Count - 1].ScreenGO;
			GameObject gameObject = UnityEngine.Object.Instantiate(zendeskErrorToastUIGO, screenGO.transform);
			errorToasts.Add(gameObject);
			gameObject.SetActive(value: true);
			gameObject.transform.GetComponentInChildren<Text>().text = string.Format(content);
		}
	}

	public static void AfterDestroyToast(GameObject g)
	{
		if (errorToasts.Contains(g))
		{
			errorToasts.Remove(g);
		}
	}

	public void DestroyZendeskErrorToast()
	{
		try
		{
			foreach (GameObject errorToast in errorToasts)
			{
				UnityEngine.Object.Destroy(errorToast);
			}
			errorToasts.Clear();
		}
		catch (Exception ex)
		{
			throw ex;
		}
	}

	public bool IfAuthError(ErrorResponse zendeskErrorResponse, ZendeskAuthType zendeskAuthType)
	{
		if (zendeskErrorResponse != null && zendeskErrorResponse.IsAuthError)
		{
			if (zendeskAuthType.Value == ZendeskAuthType.Jwt.Value)
			{
				Debug.Log(jwtIntegratorError);
			}
			NavigateError(authScreenErrorBody, true, false, authScreenErrorTitle);
			return true;
		}
		return false;
	}
}
