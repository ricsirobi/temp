using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Zendesk.Internal.Models.Common;
using Zendesk.Internal.Models.Core;
using Zendesk.Internal.Models.HelpCenter;
using Zendesk.Internal.Models.Support;
using Zendesk.UI.HelpCenter;

namespace Zendesk.UI;

public class ZendeskUI : MonoBehaviour
{
	private ZendeskErrorUI zendeskErrorUI;

	private ZendeskMain zendeskMain;

	[SerializeField]
	private Text openSupportString;

	[SerializeField]
	private Text createRequestString;

	[SerializeField]
	private Text openHelpCenterString;

	private GameObject loadingBarInstantiated;

	[SerializeField]
	public GameObject zendeskCanvas;

	[SerializeField]
	public GameObject zendeskButtonsCanvas;

	[SerializeField]
	public GameObject zendeskSupportUIGO;

	[SerializeField]
	public GameObject zendeskHelpCenterUIGO;

	[SerializeField]
	public bool useAndroidBackButtonViaZendesk;

	[SerializeField]
	public GameObject loadingBar;

	[SerializeField]
	public bool useZendeskUI = true;

	[SerializeField]
	public bool useZendeskUIButtons = true;

	public List<ZendeskScreenRecord> backStateGO = new List<ZendeskScreenRecord>();

	[HideInInspector]
	public ZendeskSupportUI zendeskSupportUI;

	[HideInInspector]
	public ZendeskHelpCenterUI zendeskHelpCenterUI;

	[HideInInspector]
	public string embededVideoString;

	public Action<bool> OnSupportUIClose;

	public void InitWithErrorHandler(ZendeskMain zendeskMain, ZendeskErrorUI zendeskErrorUI)
	{
		if (useZendeskUI)
		{
			this.zendeskMain = zendeskMain;
			this.zendeskErrorUI = zendeskErrorUI;
			zendeskSupportUI = zendeskSupportUIGO.GetComponent<ZendeskSupportUI>();
			zendeskSupportUI.InitWithErrorHandler(zendeskMain, this, zendeskErrorUI);
			zendeskHelpCenterUI = zendeskHelpCenterUIGO.GetComponent<ZendeskHelpCenterUI>();
			zendeskHelpCenterUI.InitWithErrorHandler(zendeskMain, this, zendeskErrorUI);
			SetCommonStrings();
			if (!useZendeskUIButtons)
			{
				UnityEngine.Object.Destroy(zendeskButtonsCanvas);
				return;
			}
			SetSampleButtonStrings();
			zendeskButtonsCanvas.SetActive(value: true);
		}
		else
		{
			UnityEngine.Object.Destroy(zendeskButtonsCanvas);
			UnityEngine.Object.Destroy(zendeskCanvas);
			UnityEngine.Object.Destroy(base.gameObject.GetComponent<ZendeskErrorUI>());
			UnityEngine.Object.Destroy(this);
		}
	}

	public void Init(ZendeskMain zendeskMain, ZendeskSettings zendeskSettings, ZendeskErrorUI zendeskErrorUI, string commonTags = "")
	{
		try
		{
			if (useZendeskUI)
			{
				this.zendeskErrorUI = zendeskErrorUI;
				zendeskSupportUI = zendeskSupportUIGO.GetComponent<ZendeskSupportUI>();
				zendeskSupportUI.Init(zendeskMain, this, zendeskSettings, zendeskErrorUI, commonTags);
				zendeskHelpCenterUI = zendeskHelpCenterUIGO.GetComponent<ZendeskHelpCenterUI>();
				zendeskHelpCenterUI.Init(zendeskMain, this, zendeskSettings, zendeskErrorUI);
				SetCommonStrings();
				if (!useZendeskUIButtons)
				{
					UnityEngine.Object.Destroy(zendeskButtonsCanvas);
					return;
				}
				SetSampleButtonStrings();
				zendeskButtonsCanvas.SetActive(value: true);
			}
			else
			{
				UnityEngine.Object.Destroy(zendeskCanvas);
				UnityEngine.Object.Destroy(zendeskButtonsCanvas);
				UnityEngine.Object.Destroy(base.gameObject.GetComponent<ZendeskErrorUI>());
				UnityEngine.Object.Destroy(this);
			}
		}
		catch (Exception ex)
		{
			throw ex;
		}
	}

	public void BackButton()
	{
		try
		{
			if (backStateGO.Count <= 1)
			{
				ZendeskInitCheck();
				backStateGO.Clear();
				zendeskErrorUI.zendeskErrorUIGO.SetActive(value: false);
				return;
			}
			ZendeskScreenRecord zendeskScreenRecord = backStateGO[backStateGO.Count - 1];
			ScreenControlsBeforeClose(zendeskScreenRecord);
			if (!ZendeskInitCheck())
			{
				backStateGO.Remove(zendeskScreenRecord);
				return;
			}
			ZendeskScreenRecord zendeskScreenRecord2 = backStateGO[backStateGO.Count - 2];
			zendeskErrorUI.DestroyZendeskErrorToast();
			backStateGO.Remove(zendeskScreenRecord);
			backStateGO.Remove(zendeskScreenRecord2);
			zendeskErrorUI.zendeskErrorUIGO.SetActive(value: false);
			if (zendeskScreenRecord.ZendeskScreen.ScreenType != zendeskScreenRecord2.ZendeskScreen.ScreenType)
			{
				if (zendeskScreenRecord.ZendeskScreen.ScreenType == ScreenType.Support)
				{
					zendeskSupportUIGO.SetActive(value: false);
				}
				else if (zendeskScreenRecord.ZendeskScreen.ScreenType == ScreenType.HelpCenter)
				{
					zendeskHelpCenterUIGO.SetActive(value: false);
				}
			}
			string text = "";
			if (zendeskScreenRecord2.Parameter != null)
			{
				text = zendeskScreenRecord2.Parameter.ToString();
			}
			switch (zendeskScreenRecord2.ZendeskScreen.Screen)
			{
			case Screen.CreateTicket:
				zendeskSupportUIGO.SetActive(value: true);
				zendeskSupportUI.SubmitNewRequest(text);
				break;
			case Screen.TicketResponse:
				zendeskSupportUIGO.SetActive(value: true);
				if (zendeskScreenRecord2.Parameter != null && !string.IsNullOrEmpty(text))
				{
					new Request().Id = text;
				}
				zendeskSupportUI.RetrieveTicketResponseData((Request)zendeskScreenRecord2.Parameter, clearLastCommentDatetime: true, refreshFooter: false);
				break;
			case Screen.MyTickets:
				zendeskSupportUIGO.SetActive(value: true);
				zendeskSupportUI.BackToMyTicketsScreen();
				break;
			case Screen.MyHelpCenterCategories:
				zendeskHelpCenterUIGO.SetActive(value: true);
				zendeskHelpCenterUI.ShowCategories();
				break;
			case Screen.MyHelpCenter:
				zendeskHelpCenterUIGO.SetActive(value: true);
				zendeskHelpCenterUI.ShowArticlesWindowCached();
				break;
			case Screen.Article:
			{
				zendeskHelpCenterUIGO.SetActive(value: true);
				Article article = null;
				if (zendeskScreenRecord2 != null && zendeskScreenRecord2.Parameter != null)
				{
					article = (Article)zendeskScreenRecord2.Parameter;
				}
				zendeskHelpCenterUI.LoadIndividualArticle(article);
				break;
			}
			case Screen.ArticleList:
			{
				zendeskHelpCenterUIGO.SetActive(value: true);
				long sectionId = (long)zendeskScreenRecord2.Parameter;
				zendeskHelpCenterUI.LoadArticlesFromSection(sectionId);
				break;
			}
			}
		}
		catch
		{
			zendeskErrorUI.NavigateError(null, true, true);
		}
	}

	public void AddScreenBackState(GameObject goScreen, ZendeskScreen screenEnum, object parameter = null)
	{
		try
		{
			backStateGO.Add(new ZendeskScreenRecord
			{
				ScreenGO = goScreen,
				ZendeskScreen = screenEnum,
				Parameter = parameter
			});
		}
		catch (Exception ex)
		{
			throw ex;
		}
	}

	public void RefreshPage()
	{
		try
		{
			if (Application.internetReachability == NetworkReachability.NotReachable)
			{
				zendeskErrorUI.NavigateError(null, true, true);
				return;
			}
			AddScreenBackState(null, ZendeskScreen.Empty);
			BackButton();
		}
		catch
		{
			zendeskErrorUI.NavigateError(null, true, true);
		}
	}

	private void ScreenControlsBeforeClose(ZendeskScreenRecord zendeskScreenRecord)
	{
		zendeskSupportUI.ticketConfirmationMessageCoroutines.Clear();
		if (zendeskScreenRecord != null && zendeskScreenRecord.ZendeskScreen != null && zendeskScreenRecord.ZendeskScreen.Screen == Screen.CreateTicket)
		{
			HideAllValidationsInCreateRequestForm();
		}
	}

	public void CloseButton()
	{
		try
		{
			zendeskErrorUI.DestroyZendeskErrorToast();
			foreach (ZendeskScreenRecord item in backStateGO)
			{
				ScreenControlsBeforeClose(item);
			}
			backStateGO.Clear();
			zendeskHelpCenterUIGO.SetActive(value: false);
			zendeskSupportUIGO.SetActive(value: false);
			zendeskErrorUI.zendeskErrorUIGO.SetActive(value: false);
			OnSupportUIClose?.Invoke(obj: true);
			if (zendeskCanvas.activeSelf)
			{
				zendeskCanvas.SetActive(value: false);
			}
			if (useZendeskUI && useZendeskUIButtons)
			{
				zendeskButtonsCanvas.SetActive(value: true);
			}
		}
		catch
		{
			zendeskErrorUI.NavigateError(null, true, true);
		}
	}

	private void Update()
	{
		if (useAndroidBackButtonViaZendesk && Application.platform == RuntimePlatform.Android)
		{
			if (Input.GetKeyDown(KeyCode.Escape) && backStateGO.Count > 1)
			{
				BackButton();
			}
			else if (Input.GetKeyDown(KeyCode.Escape) && backStateGO.Count == 1)
			{
				CloseButton();
			}
		}
	}

	public void ShowBackButton(GameObject backButtonContainer)
	{
		try
		{
			if (backStateGO.Count > 2)
			{
				backButtonContainer.SetActive(value: true);
			}
			else
			{
				backButtonContainer.SetActive(value: false);
			}
		}
		catch (Exception ex)
		{
			throw ex;
		}
	}

	private void SetCommonStrings()
	{
		embededVideoString = zendeskMain.zendeskLocalizationHandler.translationGameObjects["usdk_help_center_article_view_media"];
	}

	private void SetSampleButtonStrings()
	{
		if (openSupportString != null)
		{
			openSupportString.text = zendeskMain.zendeskLocalizationHandler.translationGameObjects["usdk_sample_button_support"];
		}
		if (createRequestString != null)
		{
			createRequestString.text = zendeskMain.zendeskLocalizationHandler.translationGameObjects["usdk_get_in_touch_label_saddle_bar"];
		}
		if (openHelpCenterString != null)
		{
			openHelpCenterString.text = zendeskMain.zendeskLocalizationHandler.translationGameObjects["usdk_sample_button_helpcenter"];
		}
	}

	public void StartLoading(GameObject target)
	{
		try
		{
			if (loadingBarInstantiated != null)
			{
				UnityEngine.Object.Destroy(loadingBarInstantiated);
			}
			loadingBarInstantiated = UnityEngine.Object.Instantiate(loadingBar, target.transform);
			loadingBarInstantiated.SetActive(value: true);
		}
		catch
		{
		}
	}

	public void FinishLoading()
	{
		try
		{
			if (!(loadingBarInstantiated != null))
			{
				return;
			}
			if (loadingBarInstantiated.GetComponent<Animator>() != null && loadingBarInstantiated.GetComponent<Animator>().isActiveAndEnabled)
			{
				loadingBarInstantiated.GetComponent<Animator>().SetBool("isLoading", value: false);
				if (loadingBarInstantiated.GetComponent<Animator>().GetCurrentAnimatorStateInfo(0).IsName("FinishLoading"))
				{
					UnityEngine.Object.Destroy(loadingBarInstantiated);
				}
			}
			else
			{
				UnityEngine.Object.Destroy(loadingBarInstantiated);
			}
		}
		catch
		{
			UnityEngine.Object.Destroy(loadingBarInstantiated);
		}
	}

	private void HideAllValidationsInCreateRequestForm()
	{
		zendeskSupportUI.createRequestPanelEmailValidation.SetActive(value: false);
		zendeskSupportUI.createRequestPanelNameValidation.SetActive(value: false);
		zendeskSupportUI.createRequestPanelMessageValidation.SetActive(value: false);
	}

	public void OpenHelpCenter()
	{
		zendeskHelpCenterUI.OpenHelpCenter();
	}

	public void OpenSupport(string tags = "")
	{
		zendeskSupportUI.OpenSupportPanel(tags);
	}

	public void OpenCreateRequest(string tags = "")
	{
		zendeskSupportUI.SubmitNewRequest(tags);
	}

	private bool ZendeskInitCheck()
	{
		if (zendeskMain.InitialisationStatus != InitialisationStatus.Initialised)
		{
			zendeskErrorUI.zendeskFullScreenErrorBackButtonContainer.GetComponent<Button>().interactable = false;
			zendeskMain.InitImmediate();
			return false;
		}
		return true;
	}
}
