using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using HtmlAgilityPack;
using UnityEngine;
using Zendesk.UI;

public class ZendeskHtmlHandler : MonoBehaviour
{
	private ZendeskLinkHandler linkHandler;

	private string lastText = "";

	private List<ZendeskHtmlComponent> zendeskHtmlComponents = new List<ZendeskHtmlComponent>();

	private ZendeskUI zendeskUI;

	private HtmlDocument htmlDoc;

	private int line;

	private void Start()
	{
		linkHandler = GetComponent<ZendeskLinkHandler>();
		zendeskUI = GetComponent<ZendeskUI>();
	}

	private void Init(string html)
	{
		zendeskHtmlComponents = new List<ZendeskHtmlComponent>();
		htmlDoc = new HtmlDocument();
		htmlDoc.LoadHtml(ParseHtmlInitial(html));
		lastText = "";
	}

	public List<ZendeskHtmlComponent> ParseHtml(string html)
	{
		try
		{
			Init(html);
			foreach (HtmlNode item in htmlDoc.DocumentNode.Descendants())
			{
				string text = item.NodeType.ToString().ToUpper();
				if (!(text == "TEXT"))
				{
					if (!(text == "ELEMENT"))
					{
						continue;
					}
					switch (item.Name.ToUpper())
					{
					case "IFRAME":
						foreach (HtmlAttribute item2 in (IEnumerable<HtmlAttribute>)item.Attributes)
						{
							if (item2.Name.ToUpper().Equals("SRC"))
							{
								string videoURL = GetVideoURL(item2.Value);
								string text2 = zendeskUI.embededVideoString + " (extlink) ";
								AddLink(text2, videoURL);
							}
						}
						break;
					case "A":
						foreach (HtmlAttribute item3 in (IEnumerable<HtmlAttribute>)item.Attributes)
						{
							if (!item3.Name.ToUpper().Equals("HREF"))
							{
								continue;
							}
							string text3 = "";
							if (item.ParentNode != null)
							{
								if (item.ParentNode.Name.ToUpper().Equals("LI"))
								{
									continue;
								}
								if (!PreviousTextIsSameLinkText(lastText, ClearHtmlText(item.InnerText)))
								{
									text3 += ClearHtmlText(item.InnerText);
									if (!linkHandler.IsInternalLink(item3.Value))
									{
										text3 += " (extlink) ";
									}
									AddLink(text3, item3.Value);
									break;
								}
								try
								{
									zendeskHtmlComponents.RemoveAll((ZendeskHtmlComponent a) => a.text == ClearHtmlText(item.InnerText));
									text3 += ClearHtmlText(item.InnerText);
									if (!linkHandler.IsInternalLink(item3.Value))
									{
										text3 += " (extlink) ";
									}
									AddLink(text3, item3.Value);
								}
								catch
								{
								}
								break;
							}
							if (!PreviousTextIsSameLinkText(lastText, ClearHtmlText(item.InnerText)))
							{
								text3 += ClearHtmlText(item.InnerText);
								if (!linkHandler.IsInternalLink(item3.Value))
								{
									text3 += " (extlink) ";
								}
								AddLink(text3, item3.Value);
							}
							break;
						}
						break;
					case "STRONG":
					case "B":
					{
						string newTextBold = item.InnerHtml;
						if (!PreviousTextContains(lastText, ClearHtmlText(newTextBold), "b"))
						{
							if (!PreviousTextIsSameLinkText(lastText, ClearHtmlText(newTextBold)) || (PreviousTextIsSameLinkText(lastText, ClearHtmlText(newTextBold)) && line != item.Line))
							{
								if (!item.ParentNode.Name.ToUpper().Equals("LI") && !item.ParentNode.Name.ToUpper().Equals("H1") && !item.ParentNode.Name.ToUpper().Equals("H2") && !item.ParentNode.Name.ToUpper().Equals("H3") && !item.ParentNode.Name.ToUpper().Equals("H4") && !item.ParentNode.Name.ToUpper().Equals("H5"))
								{
									AddText("<b>" + ClearHtmlText(newTextBold) + "</b>");
								}
							}
							else
							{
								ZendeskHtmlComponent zendeskHtmlComponent2 = zendeskHtmlComponents.Where((ZendeskHtmlComponent a) => !string.IsNullOrEmpty(a.text) && a.text.Contains(ClearHtmlText(newTextBold))).FirstOrDefault();
								zendeskHtmlComponent2.text = "<b>" + ClearHtmlText(zendeskHtmlComponent2.text) + "</b>";
								lastText = zendeskHtmlComponent2.text;
							}
						}
						line = item.Line;
						break;
					}
					case "I":
					{
						string newTextItalics = item.InnerHtml;
						PreviousTextContains(lastText, ClearHtmlText(newTextItalics), "i");
						if (!PreviousTextIsSameLinkText(lastText, ClearHtmlText(newTextItalics)) || (PreviousTextIsSameLinkText(lastText, ClearHtmlText(newTextItalics)) && line != item.Line))
						{
							if (!item.ParentNode.Name.ToUpper().Equals("LI") && !item.ParentNode.Name.ToUpper().Equals("H1") && !item.ParentNode.Name.ToUpper().Equals("H2") && !item.ParentNode.Name.ToUpper().Equals("H3") && !item.ParentNode.Name.ToUpper().Equals("H4") && !item.ParentNode.Name.ToUpper().Equals("H5"))
							{
								AddText("<i>" + ClearHtmlText(newTextItalics) + "</i>");
							}
						}
						else
						{
							ZendeskHtmlComponent zendeskHtmlComponent = zendeskHtmlComponents.Where((ZendeskHtmlComponent a) => !string.IsNullOrEmpty(a.text) && a.text.Contains(ClearHtmlText(newTextItalics))).FirstOrDefault();
							zendeskHtmlComponents.Remove(zendeskHtmlComponent);
							zendeskHtmlComponent.text = "<i>" + ClearHtmlText(zendeskHtmlComponent.text) + "</i>";
							zendeskHtmlComponents.Add(zendeskHtmlComponent);
							lastText = zendeskHtmlComponent.text;
						}
						line = item.Line;
						break;
					}
					case "BR":
						AddLineBreak();
						break;
					case "IMG":
						AddImage(item);
						break;
					case "LI":
						HandleHtmlList(item.ChildNodes);
						break;
					case "H1":
						if (item.InnerText.ToLower() == "\n")
						{
							AddLineBreak();
						}
						else if (!string.IsNullOrEmpty(item.InnerText) && !lastText.Equals(item.InnerText) && !PreviousTextIsSameLinkText(lastText, ClearHtmlText(item.InnerText)))
						{
							AddHeader(item.InnerText, "h1");
						}
						break;
					case "H2":
						if (item.InnerText.ToLower() == "\n")
						{
							AddLineBreak();
						}
						else if (!string.IsNullOrEmpty(item.InnerText) && !lastText.Equals(item.InnerText) && !PreviousTextIsSameLinkText(lastText, ClearHtmlText(item.InnerText)))
						{
							AddHeader(item.InnerText, "h2");
						}
						break;
					case "H3":
						if (item.InnerText.ToLower() == "\n")
						{
							AddLineBreak();
						}
						else if (!string.IsNullOrEmpty(item.InnerText) && !lastText.Equals(item.InnerText) && !PreviousTextIsSameLinkText(lastText, ClearHtmlText(item.InnerText)))
						{
							AddHeader(item.InnerText, "h3");
						}
						break;
					case "H4":
						if (item.InnerText.ToLower() == "\n")
						{
							AddLineBreak();
						}
						else if (!string.IsNullOrEmpty(item.InnerText) && !lastText.Equals(item.InnerText) && !PreviousTextIsSameLinkText(lastText, ClearHtmlText(item.InnerText)))
						{
							AddHeader(item.InnerText, "h4");
						}
						break;
					}
				}
				else if (item.InnerText.ToLower() == "\n")
				{
					AddLineBreak();
				}
				else if (!string.IsNullOrEmpty(item.InnerText) && !lastText.Equals(item.InnerText) && !PreviousTextIsSameLinkText(lastText, ClearHtmlText(item.InnerText)) && !PreviousTextContains(lastText, ClearHtmlText(item.InnerText), item.ParentNode.Name) && (item.ParentNode == null || !item.ParentNode.Name.ToUpper().Equals("LI")) && (item.ParentNode.ParentNode == null || !item.ParentNode.ParentNode.Name.ToUpper().Equals("LI")))
				{
					AddText(item.InnerText);
				}
			}
			return zendeskHtmlComponents;
		}
		catch (Exception ex)
		{
			throw ex;
		}
	}

	public List<ZendeskHtmlComponent> ParseHtmlRequest(string html)
	{
		try
		{
			Init(html);
			foreach (HtmlNode item in htmlDoc.DocumentNode.Descendants())
			{
				string text = item.NodeType.ToString().ToUpper();
				if (!(text == "TEXT"))
				{
					if (!(text == "ELEMENT"))
					{
						continue;
					}
					switch (item.Name.ToUpper())
					{
					case "DIV":
						if (item != null && item.Attributes != null && item.Attributes.Count > 0 && item.Attributes.Contains("class") && item.Attributes.FirstOrDefault((HtmlAttribute a) => a.Name.ToUpper().Equals("CLASS")).Value.ToUpper().Equals("SIGNATURE"))
						{
							AddText(item.InnerText);
							return zendeskHtmlComponents;
						}
						break;
					case "A":
						foreach (HtmlAttribute item2 in (IEnumerable<HtmlAttribute>)item.Attributes)
						{
							if (!item2.Name.ToUpper().Equals("HREF"))
							{
								continue;
							}
							string text2 = "";
							if (item.ParentNode != null)
							{
								if (!item.ParentNode.Name.ToUpper().Equals("LI"))
								{
									if (!PreviousTextIsSameLinkText(lastText, ClearHtmlText(item.InnerText)))
									{
										text2 += ClearHtmlText(item.InnerText);
										if (!linkHandler.IsInternalLink(item2.Value))
										{
											text2 += " (extlink) ";
										}
										AddLink(text2, item2.Value);
										break;
									}
									try
									{
										zendeskHtmlComponents.RemoveAll((ZendeskHtmlComponent a) => a.text == ClearHtmlText(item.InnerText));
										text2 += ClearHtmlText(item.InnerText);
										if (!linkHandler.IsInternalLink(item2.Value))
										{
											text2 += " (extlink) ";
										}
										AddLink(text2, item2.Value);
									}
									catch
									{
									}
									break;
								}
								if ((item.ParentNode == null || item.ParentNode.Name.ToUpper().Equals("LI")) && item.ParentNode.ParentNode != null)
								{
									item.ParentNode.ParentNode.Name.ToUpper().Equals("LI");
								}
								continue;
							}
							if (!PreviousTextIsSameLinkText(lastText, ClearHtmlText(item.InnerText)))
							{
								text2 += ClearHtmlText(item.InnerText);
								if (!linkHandler.IsInternalLink(item2.Value))
								{
									text2 += " (extlink) ";
								}
								AddLink(text2, item2.Value);
							}
							break;
						}
						break;
					case "STRONG":
					case "B":
					{
						string newTextBold = RemoveFontTags(item.InnerHtml);
						if (!PreviousTextIsSameLinkText(lastText, ClearHtmlText(newTextBold)))
						{
							if (!item.ParentNode.Name.ToUpper().Equals("LI"))
							{
								AddText("<b>" + ClearHtmlText(newTextBold) + "</b>");
							}
							break;
						}
						ZendeskHtmlComponent zendeskHtmlComponent7 = zendeskHtmlComponents.Where((ZendeskHtmlComponent a) => !string.IsNullOrEmpty(a.text) && a.text.Contains(ClearHtmlText(newTextBold))).FirstOrDefault();
						zendeskHtmlComponents.Remove(zendeskHtmlComponent7);
						zendeskHtmlComponent7.text = "<b>" + ClearHtmlText(zendeskHtmlComponent7.text) + "</b>";
						zendeskHtmlComponents.Add(zendeskHtmlComponent7);
						lastText = zendeskHtmlComponent7.text;
						break;
					}
					case "I":
					case "EM":
					{
						string newTextItalics = RemoveFontTags(item.InnerHtml);
						if (!PreviousTextIsSameLinkText(lastText, ClearHtmlText(newTextItalics)))
						{
							if (!item.ParentNode.Name.ToUpper().Equals("LI"))
							{
								AddText("<i>" + ClearHtmlText(newTextItalics) + "</i>");
							}
							break;
						}
						ZendeskHtmlComponent zendeskHtmlComponent4 = zendeskHtmlComponents.Where((ZendeskHtmlComponent a) => !string.IsNullOrEmpty(a.text) && a.text.Contains(ClearHtmlText(newTextItalics))).FirstOrDefault();
						zendeskHtmlComponents.Remove(zendeskHtmlComponent4);
						zendeskHtmlComponent4.text = "<i>" + ClearHtmlText(zendeskHtmlComponent4.text) + "</i>";
						zendeskHtmlComponents.Add(zendeskHtmlComponent4);
						lastText = zendeskHtmlComponent4.text;
						break;
					}
					case "BR":
						AddLineBreak();
						break;
					case "LI":
						HandleHtmlList(item.ChildNodes);
						break;
					case "H1":
					{
						if (!PreviousTextIsSameLinkText(lastText, ClearHtmlText(item.InnerText)))
						{
							if (!item.ParentNode.Name.ToUpper().Equals("LI"))
							{
								AddHeader("<b>" + ClearHtmlText(item.InnerText) + "</b>", "h1");
							}
							break;
						}
						ZendeskHtmlComponent zendeskHtmlComponent5 = zendeskHtmlComponents.Where((ZendeskHtmlComponent a) => !string.IsNullOrEmpty(a.text) && a.text.Contains(ClearHtmlText(item.InnerText))).FirstOrDefault();
						zendeskHtmlComponents.Remove(zendeskHtmlComponent5);
						zendeskHtmlComponent5.text = "<b>" + ClearHtmlText(zendeskHtmlComponent5.text) + "</b>";
						zendeskHtmlComponents.Add(zendeskHtmlComponent5);
						lastText = zendeskHtmlComponent5.text;
						break;
					}
					case "H2":
					{
						if (!PreviousTextIsSameLinkText(lastText, ClearHtmlText(item.InnerText)))
						{
							if (!item.ParentNode.Name.ToUpper().Equals("LI"))
							{
								AddHeader("<b>" + ClearHtmlText(item.InnerText) + "</b>", "h2");
							}
							break;
						}
						ZendeskHtmlComponent zendeskHtmlComponent2 = zendeskHtmlComponents.Where((ZendeskHtmlComponent a) => !string.IsNullOrEmpty(a.text) && a.text.Contains(ClearHtmlText(item.InnerText))).FirstOrDefault();
						zendeskHtmlComponents.Remove(zendeskHtmlComponent2);
						zendeskHtmlComponent2.text = "<b>" + ClearHtmlText(zendeskHtmlComponent2.text) + "</b>";
						zendeskHtmlComponents.Add(zendeskHtmlComponent2);
						lastText = zendeskHtmlComponent2.text;
						break;
					}
					case "H3":
					{
						if (!PreviousTextIsSameLinkText(lastText, ClearHtmlText(item.InnerText)))
						{
							if (!item.ParentNode.Name.ToUpper().Equals("LI"))
							{
								AddHeader("<b>" + ClearHtmlText(item.InnerText) + "</b>", "h3");
							}
							break;
						}
						ZendeskHtmlComponent zendeskHtmlComponent6 = zendeskHtmlComponents.Where((ZendeskHtmlComponent a) => !string.IsNullOrEmpty(a.text) && a.text.Contains(ClearHtmlText(item.InnerText))).FirstOrDefault();
						zendeskHtmlComponents.Remove(zendeskHtmlComponent6);
						zendeskHtmlComponent6.text = "<b>" + ClearHtmlText(zendeskHtmlComponent6.text) + "</b>";
						zendeskHtmlComponents.Add(zendeskHtmlComponent6);
						lastText = zendeskHtmlComponent6.text;
						break;
					}
					case "H4":
					{
						if (!PreviousTextIsSameLinkText(lastText, ClearHtmlText(item.InnerText)))
						{
							if (!item.ParentNode.Name.ToUpper().Equals("LI"))
							{
								AddHeader("<b>" + ClearHtmlText(item.InnerText) + "</b>", "h4");
							}
							break;
						}
						ZendeskHtmlComponent zendeskHtmlComponent3 = zendeskHtmlComponents.Where((ZendeskHtmlComponent a) => !string.IsNullOrEmpty(a.text) && a.text.Contains(ClearHtmlText(item.InnerText))).FirstOrDefault();
						zendeskHtmlComponents.Remove(zendeskHtmlComponent3);
						zendeskHtmlComponent3.text = "<b>" + ClearHtmlText(zendeskHtmlComponent3.text) + "</b>";
						zendeskHtmlComponents.Add(zendeskHtmlComponent3);
						lastText = zendeskHtmlComponent3.text;
						break;
					}
					case "H5":
					{
						if (!PreviousTextIsSameLinkText(lastText, ClearHtmlText(item.InnerText)))
						{
							if (!item.ParentNode.Name.ToUpper().Equals("LI"))
							{
								AddHeader("<b>" + ClearHtmlText(item.InnerText) + "</b>", "h5");
							}
							break;
						}
						ZendeskHtmlComponent zendeskHtmlComponent = zendeskHtmlComponents.Where((ZendeskHtmlComponent a) => !string.IsNullOrEmpty(a.text) && a.text.Contains(ClearHtmlText(item.InnerText))).FirstOrDefault();
						zendeskHtmlComponents.Remove(zendeskHtmlComponent);
						zendeskHtmlComponent.text = "<b>" + ClearHtmlText(zendeskHtmlComponent.text) + "</b>";
						zendeskHtmlComponents.Add(zendeskHtmlComponent);
						lastText = zendeskHtmlComponent.text;
						break;
					}
					}
					continue;
				}
				if (item.InnerText.ToLower() == "\n")
				{
					AddLineBreak();
				}
				if ((item.ParentNode == null || item.ParentNode.Name.ToUpper().Equals("LI")) && (item.ParentNode.ParentNode == null || item.ParentNode.ParentNode.Name.ToUpper().Equals("LI")))
				{
					continue;
				}
				if (zendeskHtmlComponents != null && !zendeskHtmlComponents.Any())
				{
					AddText(ClearHtmlText(item.InnerText));
				}
				else if (zendeskHtmlComponents.LastOrDefault().zendeskHtmlComponentType == ZendeskHtmlComponentType.LINK)
				{
					if (!string.IsNullOrEmpty(item.InnerText) && zendeskHtmlComponents.LastOrDefault() != null && !string.IsNullOrEmpty(zendeskHtmlComponents.LastOrDefault().text) && !zendeskHtmlComponents.LastOrDefault().text.Equals(item.InnerText))
					{
						AddText(ClearHtmlText(item.InnerText));
					}
				}
				else if (zendeskHtmlComponents.LastOrDefault().zendeskHtmlComponentType != 0)
				{
					AddText(ClearHtmlText(item.InnerText));
				}
				else
				{
					AddLineBreak();
				}
			}
			return zendeskHtmlComponents;
		}
		catch (Exception ex)
		{
			throw ex;
		}
	}

	private string RemoveFontTags(string input)
	{
		string text = input;
		if (text.ToUpper().Contains("<B>"))
		{
			int num = text.ToUpper().IndexOf("<B>");
			int num2 = text.ToUpper().IndexOf("</B>", num);
			text = text.Substring(num + 3, num2 - num - 3);
		}
		if (text.ToUpper().Contains("<I>"))
		{
			int num3 = text.ToUpper().IndexOf("<I>");
			int num4 = text.ToUpper().IndexOf("</I>", num3);
			text = text.Substring(num3 + 3, num4 - num3 - 3);
		}
		return text;
	}

	public string ParseHtmlInitial(string html)
	{
		try
		{
			if (!string.IsNullOrEmpty(html))
			{
				html = Regex.Replace(html, "\\p{Cs}", "");
				html = Regex.Replace(html, "<em .*?>", "<i>");
				html = Regex.Replace(html, "<strong .*?>", "<b>");
				html = Regex.Replace(html, "<span .*?>", "");
				html = Regex.Replace(html, "<p .*?>", "");
				html = html.Replace("&amp;", "&");
				html = html.Replace("&nbsp;", " ");
				html = html.Replace("<strong>", "<b>");
				html = html.Replace("</strong>", "</b>");
				html = html.Replace("&lt;", "<");
				html = html.Replace("&gt;", ">");
				html = html.Replace("<em>", "<i>");
				html = html.Replace("</em>", "</i>");
				html = html.Replace("<p>", "");
				html = html.Replace("</p>", "<br>");
				html = html.Replace("<span>", "");
				html = html.Replace("</span>", "");
			}
		}
		catch (Exception ex)
		{
			throw ex;
		}
		return html;
	}

	public string ClearHtmlText(string text)
	{
		try
		{
			if (!string.IsNullOrEmpty(text))
			{
				text = Regex.Replace(text, "\\p{Cs}", "");
				text = text.Replace("&amp;", "&");
				text = text.Replace("&nbsp;", " ");
				text = text.Replace("<strong>", "<b>");
				text = text.Replace("</strong>", "</b>");
				text = text.Replace("<em>", "<i>");
				text = text.Replace("</em>", "</i>");
				text = text.Replace("&lt;", "<");
				text = text.Replace("&gt;", ">");
				text = text.Replace("<span>", "");
				text = text.Replace("</span>", "");
			}
		}
		catch (Exception ex)
		{
			throw ex;
		}
		return text;
	}

	public string RemoveLineBreaks(string text)
	{
		try
		{
			if (!string.IsNullOrEmpty(text))
			{
				text = text.Replace("\n", "");
			}
		}
		catch (Exception ex)
		{
			throw ex;
		}
		return text;
	}

	private bool PreviousTextIsSameLinkText(string lastText, string currentText)
	{
		try
		{
			if (!string.IsNullOrEmpty(lastText) && !string.IsNullOrEmpty(currentText))
			{
				return lastText.EndsWith(currentText + "</a>") || lastText.EndsWith(currentText + "</b>") || lastText.EndsWith(currentText + "</i>") || lastText.EndsWith(currentText + "</b></i>") || lastText.EndsWith(currentText + "</i></b>") || lastText.EndsWith(currentText + " (extlink) ") || lastText.StartsWith("  - " + currentText) || lastText.StartsWith("      - " + currentText);
			}
			return false;
		}
		catch (Exception ex)
		{
			throw ex;
		}
	}

	private bool PreviousTextContains(string lastText, string currentText, string tag)
	{
		try
		{
			if (!string.IsNullOrEmpty(lastText) && !string.IsNullOrEmpty(currentText))
			{
				return lastText.Contains(currentText + "</" + tag + ">");
			}
			return false;
		}
		catch (Exception ex)
		{
			throw ex;
		}
	}

	private void AddLineBreak()
	{
		zendeskHtmlComponents.Add(new ZendeskHtmlComponent(ZendeskHtmlComponentType.LINEBREAK, null, null));
	}

	private void AddText(string text)
	{
		if (!string.IsNullOrEmpty(text.Trim()))
		{
			zendeskHtmlComponents.Add(new ZendeskHtmlComponent(ZendeskHtmlComponentType.TEXT, ClearHtmlText(text), null));
			lastText = ClearHtmlText(text);
		}
	}

	private void AddHeader(string text, string headerType)
	{
		zendeskHtmlComponents.Add(new ZendeskHtmlComponent(ZendeskHtmlComponentType.HEADER, ClearHtmlText(text), null, new Dictionary<ZendeskHtmlAttributeType, string> { 
		{
			ZendeskHtmlAttributeType.HEADER,
			headerType
		} }));
		lastText = ClearHtmlText(text);
	}

	private void AddLink(string text, string link)
	{
		zendeskHtmlComponents.Add(new ZendeskHtmlComponent(ZendeskHtmlComponentType.LINK, text, link));
		lastText = ClearHtmlText(text);
	}

	private string GetVideoURL(string rawURL)
	{
		string text = rawURL;
		while (text.StartsWith("/"))
		{
			text = text.Remove(0, 1);
		}
		if (!text.StartsWith("http"))
		{
			text = "https://" + text;
		}
		return text;
	}

	private void AddImage(HtmlNode item)
	{
		string link = "";
		string text = "";
		foreach (HtmlAttribute item2 in (IEnumerable<HtmlAttribute>)item.Attributes)
		{
			if (item2.Name.ToUpper().Equals("SRC"))
			{
				link = item2.Value;
			}
			if (item2.Name.ToUpper().Equals("ALT"))
			{
				text = item2.Value;
			}
		}
		zendeskHtmlComponents.Add(new ZendeskHtmlComponent(ZendeskHtmlComponentType.IMAGE, text, link));
	}

	private void HandleHtmlList(HtmlNodeCollection htmlList)
	{
		foreach (HtmlNode item in (IEnumerable<HtmlNode>)htmlList)
		{
			string text = item.NodeType.ToString().ToUpper();
			if (!(text == "TEXT"))
			{
				if (!(text == "ELEMENT"))
				{
					continue;
				}
				AddBullet(item);
				switch (item.Name.ToUpper())
				{
				case "A":
					foreach (HtmlAttribute item2 in (IEnumerable<HtmlAttribute>)item.Attributes)
					{
						if (!item2.Name.ToUpper().Equals("HREF"))
						{
							continue;
						}
						string text2 = "";
						if (!PreviousTextIsSameLinkText(lastText, item.InnerText))
						{
							text2 += ClearHtmlText(item.InnerText);
							if (!linkHandler.IsInternalLink(item2.Value))
							{
								text2 += " (extlink) ";
							}
							AddLink(text2, item2.Value);
							AddLineBreak();
						}
					}
					break;
				case "B":
				{
					if (!PreviousTextIsSameLinkText(lastText, item.InnerText))
					{
						if (item.ParentNode.Name.ToUpper().Equals("LI"))
						{
							AddText("<b>" + ClearHtmlText(item.InnerText) + "</b>");
						}
						break;
					}
					ZendeskHtmlComponent zendeskHtmlComponent2 = zendeskHtmlComponents.Where((ZendeskHtmlComponent a) => !string.IsNullOrEmpty(a.text) && a.text.Contains(item.InnerText)).FirstOrDefault();
					zendeskHtmlComponents.Remove(zendeskHtmlComponent2);
					zendeskHtmlComponent2.text = "<b>" + ClearHtmlText(zendeskHtmlComponent2.text) + "</b>";
					zendeskHtmlComponents.Add(zendeskHtmlComponent2);
					lastText = zendeskHtmlComponent2.text;
					break;
				}
				case "I":
				{
					if (!PreviousTextIsSameLinkText(lastText, item.InnerText))
					{
						if (!item.ParentNode.Name.ToUpper().Equals("LI"))
						{
							AddText("<i>" + ClearHtmlText(item.InnerText) + "</i>");
						}
						break;
					}
					ZendeskHtmlComponent zendeskHtmlComponent3 = zendeskHtmlComponents.Where((ZendeskHtmlComponent a) => !string.IsNullOrEmpty(a.text) && a.text.Contains(item.InnerText)).FirstOrDefault();
					zendeskHtmlComponents.Remove(zendeskHtmlComponent3);
					zendeskHtmlComponent3.text = "<i>" + ClearHtmlText(zendeskHtmlComponent3.text) + "</i>";
					zendeskHtmlComponents.Add(zendeskHtmlComponent3);
					lastText = zendeskHtmlComponent3.text;
					break;
				}
				case "EM":
				{
					if (!PreviousTextIsSameLinkText(lastText, item.InnerText))
					{
						if (!item.ParentNode.Name.ToUpper().Equals("LI"))
						{
							AddText("<i>" + ClearHtmlText(item.InnerText) + "</i>");
						}
						break;
					}
					ZendeskHtmlComponent zendeskHtmlComponent = zendeskHtmlComponents.Where((ZendeskHtmlComponent a) => !string.IsNullOrEmpty(a.text) && a.text.Contains(item.InnerText)).FirstOrDefault();
					zendeskHtmlComponents.Remove(zendeskHtmlComponent);
					zendeskHtmlComponent.text = "<i>" + ClearHtmlText(zendeskHtmlComponent.text) + "</i>";
					zendeskHtmlComponents.Add(zendeskHtmlComponent);
					lastText = zendeskHtmlComponent.text;
					break;
				}
				case "BR":
					AddLineBreak();
					break;
				case "IMG":
					AddImage(item);
					break;
				case "H1":
					if (item.InnerText.ToLower() == "\n")
					{
						AddLineBreak();
					}
					else if (!PreviousTextIsSameLinkText(lastText, ClearHtmlText(item.InnerText)) && ((item.ParentNode != null && !item.ParentNode.Name.ToUpper().Equals("LI")) || (item.ParentNode.ParentNode != null && !item.ParentNode.ParentNode.Name.ToUpper().Equals("LI"))))
					{
						AddHeader("  - " + ClearHtmlText(item.InnerText), "H1");
						AddLineBreak();
					}
					break;
				case "H2":
					if (item.InnerText.ToLower() == "\n")
					{
						AddLineBreak();
					}
					else if (!PreviousTextIsSameLinkText(lastText, ClearHtmlText(item.InnerText)) && ((item.ParentNode != null && !item.ParentNode.Name.ToUpper().Equals("LI")) || (item.ParentNode.ParentNode != null && !item.ParentNode.ParentNode.Name.ToUpper().Equals("LI"))))
					{
						AddHeader("  - " + ClearHtmlText(item.InnerText), "H2");
						AddLineBreak();
					}
					break;
				case "H3":
					if (item.InnerText.ToLower() == "\n")
					{
						AddLineBreak();
					}
					else if (!PreviousTextIsSameLinkText(lastText, ClearHtmlText(item.InnerText)) && ((item.ParentNode != null && !item.ParentNode.Name.ToUpper().Equals("LI")) || (item.ParentNode.ParentNode != null && !item.ParentNode.ParentNode.Name.ToUpper().Equals("LI"))))
					{
						AddHeader("  - " + ClearHtmlText(item.InnerText), "H3");
						AddLineBreak();
					}
					break;
				case "H4":
					if (item.InnerText.ToLower() == "\n")
					{
						AddLineBreak();
					}
					else if (!PreviousTextIsSameLinkText(lastText, ClearHtmlText(item.InnerText)) && ((item.ParentNode != null && !item.ParentNode.Name.ToUpper().Equals("LI")) || (item.ParentNode.ParentNode != null && !item.ParentNode.ParentNode.Name.ToUpper().Equals("LI"))))
					{
						AddHeader("  - " + ClearHtmlText(item.InnerText), "H4");
						AddLineBreak();
					}
					break;
				}
			}
			else if (item.InnerText.ToLower() == "\n")
			{
				AddLineBreak();
			}
			else
			{
				AddBullet(item);
				if (!PreviousTextIsSameLinkText(lastText, ClearHtmlText(item.InnerText)) && ((item.ParentNode != null && !item.ParentNode.Name.ToUpper().Equals("LI")) || (item.ParentNode.ParentNode != null && !item.ParentNode.ParentNode.Name.ToUpper().Equals("LI"))))
				{
					AddText(ClearHtmlText(item.InnerText));
					AddLineBreak();
				}
			}
		}
	}

	private void AddBullet(HtmlNode item)
	{
		if ((item.LinePosition == 0 || (item.ParentNode != null && item.ParentNode.FirstChild == item)) && !item.Name.ToUpper().Equals("BR") && !item.Name.ToUpper().Equals("UL"))
		{
			if (item.ParentNode != null && item.ParentNode.ParentNode != null && item.ParentNode.ParentNode.ParentNode != null && item.ParentNode.ParentNode.ParentNode.ParentNode != null && item.ParentNode.ParentNode.ParentNode.ParentNode.Name.ToUpper().Equals("UL"))
			{
				AddText("      - ");
			}
			else
			{
				AddText("  - ");
			}
		}
	}

	public string HandleHtmlComponents(List<ZendeskHtmlComponent> zendeskHtmlComponents)
	{
		try
		{
			string text = string.Empty;
			string text2 = string.Empty;
			foreach (ZendeskHtmlComponent zendeskHtmlComponent in zendeskHtmlComponents)
			{
				switch (zendeskHtmlComponent.zendeskHtmlComponentType)
				{
				case ZendeskHtmlComponentType.HEADER:
				case ZendeskHtmlComponentType.TEXT:
					if (!text2.Equals(zendeskHtmlComponent) && !PreviousTextIsSameLinkText(text2, ClearHtmlText(zendeskHtmlComponent.text)))
					{
						text += zendeskHtmlComponent.text;
						text2 = zendeskHtmlComponent.text;
					}
					break;
				case ZendeskHtmlComponentType.LINK:
					if (!text2.Equals(zendeskHtmlComponent))
					{
						text = text + "<a href=" + linkHandler.AddLink(zendeskHtmlComponent.link) + ">";
						text = text + zendeskHtmlComponent.text + "</a>";
					}
					text2 = zendeskHtmlComponent.text;
					break;
				case ZendeskHtmlComponentType.LINEBREAK:
					text += "\n";
					break;
				}
			}
			return ClearHtmlText(text);
		}
		catch (Exception ex)
		{
			throw ex;
		}
	}
}
