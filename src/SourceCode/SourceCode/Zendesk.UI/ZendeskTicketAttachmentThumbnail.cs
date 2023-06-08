using System;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

namespace Zendesk.UI;

public class ZendeskTicketAttachmentThumbnail : MonoBehaviour
{
	public GameObject agentContentGO;

	public GameObject agentDeleteButtonGO;

	private ZendeskSupportUI zendeskSupportUI;

	private string maxSizeErrorMessage;

	private string maxResolutionErrorMessage;

	private string Path { get; set; }

	public void Init(string path, long maxAttachmentSize, ZendeskSupportUI zendeskSupportUI, string maxSizeErrorMessage, string maxResolutionErrorMessage)
	{
		try
		{
			Path = path;
			this.maxSizeErrorMessage = maxSizeErrorMessage;
			this.maxResolutionErrorMessage = maxResolutionErrorMessage;
			base.gameObject.SetActive(value: true);
			Texture2D texture2D = LoadImageFromPath(maxAttachmentSize, 4096);
			agentContentGO.GetComponent<RawImage>().texture = texture2D;
			agentContentGO.GetComponent<RectTransform>().sizeDelta = new Vector2(texture2D.width, texture2D.height);
			agentDeleteButtonGO.GetComponent<Button>().onClick.AddListener(delegate
			{
				zendeskSupportUI.AttachmentDeleteButtonClick(base.gameObject);
			});
		}
		catch (Exception ex)
		{
			throw ex;
		}
	}

	public string GetPath()
	{
		return Path;
	}

	public Texture2D LoadImageFromPath(long maxAttachmentSize, int maxRes)
	{
		Texture2D texture2D = new Texture2D(1, 1, TextureFormat.ARGB32, mipChain: true);
		if (File.Exists(Path))
		{
			byte[] array = File.ReadAllBytes(Path);
			if (array.Length > 20971520)
			{
				throw new Exception(maxSizeErrorMessage);
			}
			texture2D = new Texture2D(2, 2);
			texture2D.LoadImage(array);
			if (texture2D.height > maxRes || texture2D.width > maxRes)
			{
				throw new Exception(maxResolutionErrorMessage);
			}
		}
		return texture2D;
	}
}
