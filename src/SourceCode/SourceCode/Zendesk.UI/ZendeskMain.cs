using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using Zendesk.Common;
using Zendesk.Internal.Models.Common;
using Zendesk.Internal.Models.Core;
using Zendesk.Providers;

namespace Zendesk.UI;

public class ZendeskMain : MonoBehaviour
{
	public string appId;

	public string clientId;

	public bool autoPause;

	public UnityEvent pauseFunctionality;

	public UnityEvent resumeFunctionality;

	public string commonTags;

	public ZendeskHelpCenterProvider helpCenterProvider;

	public ZendeskLocales locale = ZendeskLocales.Unknown;

	public bool setLocaleFromSettings = true;

	public ZendeskSupportProvider supportProvider;

	public ZendeskLocalizationHandler zendeskLocalizationHandler;

	public ZendeskPauseHandler zendeskPauseHandler;

	public ZendeskUI zendeskUI;

	public ZendeskLinkHandler zendeskLinkHander;

	public string zendeskUrl;

	public ZendeskHtmlHandler zendeskHtmlHandler;

	public GameObject mainCanvasGO;

	[HideInInspector]
	public Canvas mainCanvas;

	private ZendeskAuthHandler zendeskAuthHandler;

	private ZendeskCore zendeskCore;

	private ZendeskErrorUI zendeskErrorUI;

	[HideInInspector]
	public InitialisationStatus InitialisationStatus { get; private set; }

	public ZendeskAuthHandler ZendeskAuthHandler => zendeskAuthHandler;

	private void Start()
	{
		InitialiseComponents();
		Init();
	}

	public void Init()
	{
		if (InitialisationStatus != InitialisationStatus.InProgress)
		{
			InitialisationStatus = InitialisationStatus.InProgress;
			StartCoroutine(zendeskCore.InitializeZendeskCore(CoreInitCallback, zendeskUrl, appId, clientId, zendeskLocalizationHandler.Locale));
		}
	}

	public void InitImmediate()
	{
		if (InitialisationStatus != InitialisationStatus.InProgress)
		{
			InitialisationStatus = InitialisationStatus.InProgress;
			StartCoroutine(zendeskCore.InitializeZendeskCore(CoreInitImmediateCallback, zendeskUrl, appId, clientId, zendeskLocalizationHandler.Locale));
		}
	}

	private void InitialiseComponents()
	{
		InitialisationStatus = InitialisationStatus.NotInitialised;
		zendeskHtmlHandler = GetComponent<ZendeskHtmlHandler>();
		SetLocaleISO();
		zendeskLocalizationHandler.ReadData();
		zendeskErrorUI = GetComponent<ZendeskErrorUI>();
		zendeskErrorUI.Init(zendeskLocalizationHandler);
		zendeskUI = GetComponent<ZendeskUI>();
		zendeskUI.InitWithErrorHandler(this, zendeskErrorUI);
		zendeskErrorUI.zendeskUI = zendeskUI;
		zendeskPauseHandler = base.gameObject.GetComponent<ZendeskPauseHandler>();
		zendeskPauseHandler.init(this);
		zendeskLinkHander = GetComponent<ZendeskLinkHandler>();
		zendeskLinkHander.Init(zendeskErrorUI);
		zendeskCore = base.gameObject.GetComponent<ZendeskCore>();
	}

	private void CoreInitCallback(ZendeskResponse<ZendeskSettings> zendeskSettingsResponse)
	{
		try
		{
			if (zendeskSettingsResponse.IsError)
			{
				InitialisationStatus = InitialisationStatus.Failed;
				Debug.Log(zendeskSettingsResponse.ErrorResponse.Reason);
				return;
			}
			ZendeskLocalStorage.LoadSupportStorage();
			ZendeskSettings result = zendeskSettingsResponse.Result;
			if (base.gameObject.GetComponent<ZendeskAuthHandler>() == null)
			{
				zendeskAuthHandler = base.gameObject.AddComponent<ZendeskAuthHandler>();
			}
			zendeskAuthHandler.InitializeZendeskAuth(zendeskCore, zendeskLocalizationHandler.SetLocaleISOForUserAuth());
			if (result.HelpCenter.Enabled)
			{
				zendeskLocalizationHandler.ReadData(result.HelpCenter.Locale);
				if (base.gameObject.GetComponent<ZendeskHelpCenterProvider>() == null)
				{
					helpCenterProvider = base.gameObject.AddComponent<ZendeskHelpCenterProvider>();
					helpCenterProvider.Initialize(zendeskAuthHandler, zendeskCore, result);
				}
			}
			else
			{
				zendeskLocalizationHandler.ReadData();
			}
			if (base.gameObject.GetComponent<ZendeskSupportProvider>() == null)
			{
				supportProvider = base.gameObject.AddComponent<ZendeskSupportProvider>();
				supportProvider.Initialize(zendeskAuthHandler, zendeskCore, result, zendeskLocalizationHandler);
			}
			if (zendeskUI != null)
			{
				zendeskUI.Init(this, result, zendeskErrorUI, commonTags);
			}
			InitialisationStatus = InitialisationStatus.Initialised;
		}
		catch (Exception ex)
		{
			Debug.Log(ex.Message);
			InitialisationStatus = InitialisationStatus.Failed;
		}
		finally
		{
			if (base.gameObject.GetComponent<ZendeskErrorUI>() != null)
			{
				zendeskErrorUI.zendeskFullScreenErrorBackButtonContainer.GetComponent<Button>().interactable = true;
			}
		}
	}

	private void CoreInitImmediateCallback(ZendeskResponse<ZendeskSettings> zendeskSettingsResponse)
	{
		try
		{
			if (zendeskSettingsResponse.IsError)
			{
				InitialisationStatus = InitialisationStatus.Failed;
				Debug.Log(zendeskSettingsResponse.ErrorResponse.Reason);
				return;
			}
			ZendeskLocalStorage.LoadSupportStorage();
			ZendeskSettings result = zendeskSettingsResponse.Result;
			if (base.gameObject.GetComponent<ZendeskAuthHandler>() == null)
			{
				zendeskAuthHandler = base.gameObject.AddComponent<ZendeskAuthHandler>();
			}
			zendeskAuthHandler.InitializeZendeskAuth(zendeskCore, zendeskLocalizationHandler.SetLocaleISOForUserAuth());
			if (result.HelpCenter.Enabled)
			{
				zendeskLocalizationHandler.ReadData(result.HelpCenter.Locale);
				if (base.gameObject.GetComponent<ZendeskHelpCenterProvider>() == null)
				{
					helpCenterProvider = base.gameObject.AddComponent<ZendeskHelpCenterProvider>();
					helpCenterProvider.Initialize(zendeskAuthHandler, zendeskCore, result);
				}
			}
			else
			{
				zendeskLocalizationHandler.ReadData();
			}
			if (base.gameObject.GetComponent<ZendeskSupportProvider>() == null)
			{
				supportProvider = base.gameObject.AddComponent<ZendeskSupportProvider>();
				supportProvider.Initialize(zendeskAuthHandler, zendeskCore, result, zendeskLocalizationHandler);
			}
			if (zendeskUI != null)
			{
				zendeskUI.Init(this, result, zendeskErrorUI, commonTags);
				zendeskUI.RefreshPage();
			}
			InitialisationStatus = InitialisationStatus.Initialised;
		}
		catch (Exception ex)
		{
			InitialisationStatus = InitialisationStatus.Failed;
			Debug.Log(ex.Message);
		}
		finally
		{
			if (base.gameObject.GetComponent<ZendeskErrorUI>() != null)
			{
				zendeskErrorUI.zendeskFullScreenErrorBackButtonContainer.GetComponent<Button>().interactable = true;
			}
		}
	}

	public void TestConfiguration()
	{
		zendeskCore = base.gameObject.GetComponent<ZendeskCore>();
		zendeskErrorUI = GetComponent<ZendeskErrorUI>();
		SetLocaleISO();
		zendeskLocalizationHandler.ReadData();
		if (string.IsNullOrEmpty(appId))
		{
			Debug.Log(zendeskLocalizationHandler.translationGameObjects["usdk_fill_application_id"]);
		}
		else if (string.IsNullOrEmpty(clientId))
		{
			Debug.Log(zendeskLocalizationHandler.translationGameObjects["usdk_fill_client_id"]);
		}
		else if (string.IsNullOrEmpty(zendeskUrl))
		{
			Debug.Log(zendeskLocalizationHandler.translationGameObjects["usdk_fill_url"]);
		}
		else
		{
			StartCoroutine(zendeskCore.InitializeZendeskCore(CoreInitTestCallback, zendeskUrl, appId, clientId, zendeskLocalizationHandler.Locale));
		}
	}

	private void SetLocaleISO()
	{
		zendeskLocalizationHandler = base.gameObject.GetComponent<ZendeskLocalizationHandler>();
		if (setLocaleFromSettings)
		{
			zendeskLocalizationHandler.SetLocaleISO();
		}
		else
		{
			zendeskLocalizationHandler.SetLocaleISO((int)locale);
		}
	}

	private void CoreInitTestCallback(ZendeskResponse<ZendeskSettings> settings)
	{
		if (!settings.IsError)
		{
			Debug.Log(zendeskLocalizationHandler.translationGameObjects["usdk_success_connection_information"]);
			if (settings.Result.HelpCenter.Enabled)
			{
				Debug.Log(string.Format(zendeskLocalizationHandler.translationGameObjects["usdk_help_center_locale_success"], settings.Result.HelpCenter.Locale));
				zendeskLocalizationHandler.ReadData(settings.Result.HelpCenter.Locale);
			}
		}
		else
		{
			Debug.Log(zendeskLocalizationHandler.translationGameObjects["usdk_fail_connection_information"]);
		}
	}

	public bool IsUserAuthenticatedBefore()
	{
		return zendeskAuthHandler.isUserAuthenticatedBefore();
	}

	public string GetUserName()
	{
		return zendeskAuthHandler.GetUserName();
	}

	public string GetUserEmail()
	{
		return zendeskAuthHandler.GetUserEmail();
	}

	public void SetJwtUniqueIdentifierToken(string jwtUniqueUserId = null)
	{
		if (zendeskCore != null && zendeskCore.zendeskSettings != null && zendeskCore.zendeskSettings.Core != null && zendeskCore.zendeskSettings.Core.Authentication != ZendeskAuthType.Anonymous.Value)
		{
			if (string.IsNullOrEmpty(jwtUniqueUserId))
			{
				throw new Exception(zendeskLocalizationHandler.translationGameObjects["usdk_jwt_token_integrator_error"]);
			}
			zendeskAuthHandler.SetJwtToken(jwtUniqueUserId);
		}
	}

	public void SetAnonymousIdentity(string email = "", string username = "")
	{
		if (zendeskCore != null && zendeskCore.zendeskSettings != null && zendeskCore.zendeskSettings.Core != null && zendeskCore.zendeskSettings.Core.Authentication == ZendeskAuthType.Anonymous.Value && zendeskCore.zendeskSettings.Support.Conversations.Enabled && (!email.Equals(zendeskAuthHandler.GetUserEmail()) || !username.Equals(zendeskAuthHandler.GetUserName())) && !string.IsNullOrEmpty(email) && !string.IsNullOrEmpty(username))
		{
			zendeskAuthHandler.DeleteUserInfo();
			zendeskAuthHandler.SetUserEmail(email);
			zendeskAuthHandler.SetUserName(username);
		}
	}
}
