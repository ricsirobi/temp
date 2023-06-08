using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using Zendesk.Internal.Models.Common;
using Zendesk.Internal.Models.Core;
using Zendesk.Internal.Models.HelpCenter;
using Zendesk.Internal.Models.Support;

namespace Zendesk.UI.HelpCenter;

public class ZendeskHelpCenterUI : MonoBehaviour
{
	public bool showArticleVotes;

	public GameObject myHelpCenterPanel;

	public GameObject articleRenderPanel;

	public GameObject categoryPanel;

	public GameObject helpCenterView;

	public GameObject helpCenterCategoryView;

	public GameObject categoryPrefab;

	public GameObject sectionPrefab;

	public GameObject backButtonContainer;

	public GameObject helpCenterScrollView;

	public GameObject articleTitle;

	public GameObject footerPanel;

	public Text myHelpCenterFooterContactUsButtonText;

	public GameObject articleListPanel;

	public GameObject articleListContentOverviewPrefab;

	public GameObject articleListView;

	public GameObject headerContainer;

	public Text headerTitle;

	public GameObject headerPanel;

	private string selectedCategory = "";

	private ZendeskErrorUI zendeskErrorUI;

	private ZendeskMain zendeskMain;

	private SectionsResponse sectionsResponse;

	private CategoriesResponse categoriesResponse;

	private ZendeskUI zendeskUI;

	private ZendeskArticleRender zendeskArticleRender;

	private ZendeskSettings zendeskSettings;

	private ZendeskLocalizationHandler zendeskLocalizationHandler;

	private GameObject zendeskCanvas;

	public void OpenHelpCenter()
	{
		try
		{
			if (!zendeskCanvas.activeSelf)
			{
				zendeskCanvas.SetActive(value: true);
			}
			if (zendeskMain != null && zendeskSettings != null)
			{
				base.gameObject.SetActive(value: true);
				if (zendeskSettings.HelpCenter.Enabled)
				{
					InitHelpCenter();
					ShowArticlesWindow();
				}
				else
				{
					zendeskUI.AddScreenBackState(myHelpCenterPanel, ZendeskScreen.MyHelpCenter);
					zendeskErrorUI.NavigateError(zendeskMain.zendeskLocalizationHandler.translationGameObjects["usdk_help_center_is_not_enabled"], true, false, "hide");
				}
			}
			else
			{
				zendeskUI.AddScreenBackState(myHelpCenterPanel, ZendeskScreen.MyHelpCenter);
				zendeskErrorUI.NavigateError(null, true, true);
			}
		}
		catch
		{
			zendeskErrorUI.NavigateError(null, true, true);
		}
	}

	private void OpenMainHelpCenter()
	{
		try
		{
			base.gameObject.SetActive(value: true);
		}
		catch (Exception ex)
		{
			throw ex;
		}
	}

	public void InitWithErrorHandler(ZendeskMain zendeskMain, ZendeskUI zendeskUI, ZendeskErrorUI zendeskErrorUI)
	{
		this.zendeskMain = zendeskMain;
		this.zendeskUI = zendeskUI;
		this.zendeskErrorUI = zendeskErrorUI;
		zendeskCanvas = this.zendeskUI.zendeskCanvas;
	}

	public void Init(ZendeskMain zendeskMain, ZendeskUI zendeskUI, ZendeskSettings zendeskSettings, ZendeskErrorUI zendeskErrorUI)
	{
		this.zendeskMain = zendeskMain;
		this.zendeskUI = zendeskUI;
		zendeskCanvas = this.zendeskUI.zendeskCanvas;
		this.zendeskErrorUI = zendeskErrorUI;
		this.zendeskSettings = zendeskSettings;
		zendeskLocalizationHandler = zendeskMain.GetComponent<ZendeskLocalizationHandler>();
		articleRenderPanel.GetComponent<ZendeskArticleRender>().Init(zendeskMain, zendeskErrorUI, zendeskLocalizationHandler);
		SetHelpCenterStrings();
	}

	private void SetHelpCenterStrings()
	{
		myHelpCenterFooterContactUsButtonText.text = zendeskMain.zendeskLocalizationHandler.translationGameObjects["usdk_sample_button_request"];
	}

	private void ShowArticlesWindow()
	{
		try
		{
			zendeskUI.AddScreenBackState(myHelpCenterPanel, ZendeskScreen.MyHelpCenter);
			LoadArticles();
		}
		catch (Exception ex)
		{
			throw ex;
		}
	}

	public void ShowArticlesWindowCached()
	{
		try
		{
			if (helpCenterView.transform.childCount > 0)
			{
				zendeskUI.AddScreenBackState(myHelpCenterPanel, ZendeskScreen.MyHelpCenter);
				zendeskUI.ShowBackButton(backButtonContainer);
				articleRenderPanel.SetActive(value: false);
				articleListPanel.SetActive(value: false);
				footerPanel.SetActive(value: true);
				helpCenterScrollView.SetActive(value: true);
			}
			else
			{
				OpenHelpCenter();
			}
		}
		catch (Exception ex)
		{
			throw ex;
		}
	}

	public void LoadIndividualArticle(Article article)
	{
		try
		{
			zendeskUI.StartLoading(headerPanel);
			zendeskUI.AddScreenBackState(articleRenderPanel, ZendeskScreen.ArticleScreen, article);
			articleRenderPanel.SetActive(value: true);
			helpCenterScrollView.SetActive(value: false);
			articleListPanel.SetActive(value: false);
			footerPanel.SetActive(value: false);
			articleTitle.GetComponent<Text>().text = ClearHtmlText(article.Title);
			articleRenderPanel.GetComponent<ZendeskArticleRender>().RenderArticle(article);
			zendeskUI.ShowBackButton(backButtonContainer);
		}
		catch (Exception ex)
		{
			throw ex;
		}
	}

	public void LoadIndividualArticle(long articleId, string locale)
	{
		try
		{
			if (articleId == 0L)
			{
				OpenMainHelpCenter();
				zendeskMain.zendeskUI.zendeskSupportUI.CloseSupportUI();
				OpenHelpCenter();
				zendeskUI.ShowBackButton(backButtonContainer);
			}
			else
			{
				zendeskUI.StartLoading(headerPanel);
				zendeskMain.helpCenterProvider.GetArticle(GetArticleCallback, articleId, locale);
			}
		}
		catch (Exception ex)
		{
			throw ex;
		}
	}

	private void GetArticleCallback(ZendeskResponse<ArticleResponse> articleResponse)
	{
		try
		{
			if (articleResponse.IsError)
			{
				zendeskErrorUI.NavigateError(null, true, true);
				return;
			}
			OpenMainHelpCenter();
			zendeskMain.zendeskUI.zendeskSupportUI.CloseSupportUI();
			zendeskUI.AddScreenBackState(articleRenderPanel, ZendeskScreen.ArticleScreen, articleResponse.Result.Article);
			articleRenderPanel.SetActive(value: true);
			helpCenterScrollView.SetActive(value: false);
			footerPanel.SetActive(value: false);
			articleTitle.GetComponent<Text>().text = ClearHtmlText(articleResponse.Result.Article.Title);
			articleRenderPanel.GetComponent<ZendeskArticleRender>().RenderArticle(articleResponse.Result.Article);
			zendeskUI.ShowBackButton(backButtonContainer);
			headerTitle.text = articleResponse.Result.Article.Title;
		}
		catch
		{
			zendeskErrorUI.NavigateError(null, true, true);
		}
		finally
		{
			zendeskUI.FinishLoading();
		}
	}

	private void LoadArticles()
	{
		try
		{
			zendeskUI.StartLoading(headerPanel);
			zendeskMain.helpCenterProvider.GetCategories(GetCategoriesCallback);
		}
		catch (Exception ex)
		{
			throw ex;
		}
	}

	private void GetCategoriesCallback(ZendeskResponse<CategoriesResponse> categories)
	{
		try
		{
			if (categories.IsError)
			{
				if (categories.ErrorResponse.Status == 404)
				{
					zendeskErrorUI.NavigateError(zendeskMain.zendeskLocalizationHandler.translationGameObjects["usdk_help_center_is_not_enabled"], true, false, "hide");
					return;
				}
				zendeskErrorUI.NavigateError(null, true, true);
				zendeskUI.FinishLoading();
			}
			else
			{
				categoriesResponse = categories.Result;
				footerPanel.SetActive(value: true);
				ShowCategories();
			}
		}
		catch (Exception ex)
		{
			zendeskUI.FinishLoading();
			throw ex;
		}
	}

	private void GetSectionsCallback(ZendeskResponse<SectionsResponse> sections)
	{
		try
		{
			if (sections.IsError)
			{
				zendeskErrorUI.NavigateError(null, true, true);
				return;
			}
			zendeskUI.ShowBackButton(backButtonContainer);
			articleRenderPanel.SetActive(value: false);
			articleListPanel.SetActive(value: false);
			footerPanel.SetActive(value: true);
			helpCenterScrollView.SetActive(value: true);
			sectionsResponse = sections.Result;
			RenderMyHelpCenter();
		}
		catch (Exception ex)
		{
			throw ex;
		}
		finally
		{
			zendeskUI.FinishLoading();
		}
	}

	public void LoadArticlesFromSection(long sectionId)
	{
		try
		{
			zendeskUI.StartLoading(headerPanel);
			zendeskUI.AddScreenBackState(articleListPanel, ZendeskScreen.ArticleListScreen, sectionId);
			zendeskMain.helpCenterProvider.GetArticlesBySectionId(GetArticlesFromSectionCallback, sectionId);
			zendeskUI.ShowBackButton(backButtonContainer);
			headerTitle.text = "";
			if (sectionsResponse != null && sectionsResponse.Sections.Count > 0 && sectionsResponse.Sections.FirstOrDefault((Section a) => a.Id == sectionId) != null)
			{
				headerTitle.text = sectionsResponse.Sections.Where((Section a) => a.Id == sectionId).FirstOrDefault().Name;
			}
		}
		catch (Exception ex)
		{
			throw ex;
		}
	}

	public void ShowCategories()
	{
		try
		{
			headerTitle.text = zendeskMain.zendeskLocalizationHandler.translationGameObjects["usdk_help_center_label"];
			categoryPanel.SetActive(value: true);
			helpCenterScrollView.SetActive(value: false);
			ClearExistingHelpCenterPage();
			zendeskUI.AddScreenBackState(myHelpCenterPanel, ZendeskScreen.MyHelpCenterCategories);
			zendeskUI.ShowBackButton(backButtonContainer);
			foreach (Zendesk.Internal.Models.HelpCenter.Category category in categoriesResponse.Categories)
			{
				GameObject obj = UnityEngine.Object.Instantiate(categoryPrefab, helpCenterCategoryView.transform);
				ClearHtmlText(category.Name);
				obj.GetComponent<ZendeskArticleListOverview>().Init(category, this, zendeskErrorUI, OnCategorySelected);
			}
		}
		catch (Exception ex)
		{
			throw ex;
		}
		finally
		{
			zendeskUI.FinishLoading();
		}
	}

	private void OnCategorySelected(Zendesk.Internal.Models.HelpCenter.Category category)
	{
		categoryPanel.SetActive(value: false);
		helpCenterScrollView.SetActive(value: true);
		ClearExistingHelpCenterPage();
		zendeskUI.AddScreenBackState(myHelpCenterPanel, ZendeskScreen.MyHelpCenter);
		zendeskUI.ShowBackButton(backButtonContainer);
		zendeskUI.StartLoading(headerPanel);
		selectedCategory = category.Name;
		headerTitle.text = selectedCategory;
		zendeskMain.helpCenterProvider.GetSections(GetSectionsCallback, null, category.Id);
	}

	private void RenderMyHelpCenter()
	{
		try
		{
			ClearExistingHelpCenterPage();
			long num = 0L;
			List<Zendesk.Internal.Models.HelpCenter.Category> categories = categoriesResponse.Categories;
			List<Section> sections = sectionsResponse.Sections;
			foreach (Zendesk.Internal.Models.HelpCenter.Category category in categories)
			{
				foreach (Section item in sections.FindAll((Section x) => x.CategoryId == category.Id))
				{
					string categoryText = ClearHtmlText(category.Name);
					string sectionText = ClearHtmlText(item.Name);
					GameObject obj = UnityEngine.Object.Instantiate(sectionPrefab, helpCenterView.transform);
					if (num == category.Id)
					{
						categoryText = null;
					}
					num = category.Id;
					obj.GetComponent<ZendeskArticleListOverview>().Init(categoryText, sectionText, item.Id, this, zendeskErrorUI);
				}
			}
		}
		catch (Exception ex)
		{
			throw ex;
		}
		finally
		{
			zendeskUI.FinishLoading();
		}
	}

	private void GetArticlesFromSectionCallback(ZendeskResponse<ArticlesResponse> articles)
	{
		try
		{
			ClearArticleListPanel();
			articleRenderPanel.SetActive(value: false);
			helpCenterScrollView.SetActive(value: false);
			articleListPanel.SetActive(value: true);
			footerPanel.SetActive(value: true);
			if (articles.IsError)
			{
				throw new Exception();
			}
			List<Article> articles2 = articles.Result.Articles;
			if (articles2.Count <= 0)
			{
				return;
			}
			foreach (Article article in articles2)
			{
				GameObject obj = UnityEngine.Object.Instantiate(articleListContentOverviewPrefab, articleListView.transform);
				article.Author = articles.Result.Users.Where((User a) => a.Id == article.AuthorId).FirstOrDefault();
				obj.GetComponent<ZendeskArticleUIContainer>().Init(article, zendeskErrorUI, this);
			}
		}
		catch
		{
			zendeskErrorUI.NavigateError(null, true, true);
		}
		finally
		{
			zendeskUI.FinishLoading();
		}
	}

	private void ClearExistingHelpCenterPage()
	{
		try
		{
			foreach (Transform item in helpCenterView.transform)
			{
				UnityEngine.Object.Destroy(item.gameObject);
			}
			foreach (Transform item2 in helpCenterCategoryView.transform)
			{
				UnityEngine.Object.Destroy(item2.gameObject);
			}
		}
		catch (Exception ex)
		{
			throw ex;
		}
	}

	private void ClearArticleListPanel()
	{
		try
		{
			foreach (Transform item in articleListView.transform)
			{
				UnityEngine.Object.Destroy(item.gameObject);
			}
		}
		catch (Exception ex)
		{
			throw ex;
		}
	}

	public string ClearHtmlText(string text)
	{
		return zendeskMain.zendeskHtmlHandler.ClearHtmlText(text);
	}

	public void OverrideHelpCenterLocale(string locale)
	{
		zendeskSettings.HelpCenter.Locale = locale;
	}

	private void InitHelpCenter()
	{
		headerTitle.text = zendeskMain.zendeskLocalizationHandler.translationGameObjects["usdk_help_center_label"];
		zendeskUI.ShowBackButton(backButtonContainer);
		articleRenderPanel.SetActive(value: false);
		articleListPanel.SetActive(value: false);
		helpCenterScrollView.SetActive(value: false);
	}
}
