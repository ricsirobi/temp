using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.UI;
using Zendesk.Internal.Models.Support;
using Zendesk.UI;

public class DragonsZendeskSupport : MonoBehaviour
{
	public static DragonsZendeskSupport pInstance;

	public string _RegexFilter;

	public int _RegexValidMatchThreshold;

	public MinMax _FakeSendWaitRange;

	[SerializeField]
	private List<SupportFieldData> m_SupportFieldData = new List<SupportFieldData>();

	[SerializeField]
	private List<PlatformTypeData> m_PlatformData = new List<PlatformTypeData>();

	[SerializeField]
	private ZendeskSupportUI m_ZendeskSupportUI;

	private DragonsZendeskCaptcha mDragonsZendeskCaptcha;

	public CanvasGroup _SupportFormCanvasGroup;

	public ScrollRect _SupportFormScrollRect;

	private LogConsole mLogConsole;

	private string mTempFilePath;

	private void Awake()
	{
		if (pInstance == null)
		{
			pInstance = this;
		}
	}

	private void Start()
	{
		mLogConsole = Object.FindObjectOfType<LogConsole>();
		mDragonsZendeskCaptcha = pInstance.GetComponent<DragonsZendeskCaptcha>();
		mDragonsZendeskCaptcha.Init(OnZendeskCaptchaSuccess, OnCaptchaOpened, OnCaptchaClosed);
	}

	public void AttachAndSubmit()
	{
		if (m_ZendeskSupportUI.ValidateRequestForm())
		{
			CaptchaChallenge();
		}
	}

	private void CaptchaChallenge()
	{
		mDragonsZendeskCaptcha.OpenCaptcha();
		mDragonsZendeskCaptcha.ClearInput();
	}

	public void OnCaptchaOpened()
	{
		if ((bool)_SupportFormCanvasGroup)
		{
			_SupportFormCanvasGroup.interactable = false;
		}
		if ((bool)_SupportFormScrollRect)
		{
			_SupportFormScrollRect.enabled = false;
		}
	}

	public void OnCaptchaClosed()
	{
		if ((bool)_SupportFormCanvasGroup)
		{
			_SupportFormCanvasGroup.interactable = true;
		}
		if ((bool)_SupportFormScrollRect)
		{
			_SupportFormScrollRect.enabled = true;
		}
	}

	private void OnZendeskCaptchaSuccess()
	{
		mDragonsZendeskCaptcha.CloseCaptcha();
		string path = Application.persistentDataPath + "/Player.log";
		if (Application.platform == RuntimePlatform.MetroPlayerX64 || Application.platform == RuntimePlatform.MetroPlayerX86)
		{
			path = Application.persistentDataPath;
			path = path.Replace("LocalState", "TempState/UnityPlayer.log");
		}
		if (mLogConsole != null && Application.isMobilePlatform && !Application.isEditor)
		{
			path = mLogConsole.pFullPath;
		}
		if (File.Exists(path))
		{
			mTempFilePath = CopyFile(path);
			if (!m_ZendeskSupportUI.attachmentPaths.Contains(mTempFilePath))
			{
				m_ZendeskSupportUI.attachmentPaths.Add(mTempFilePath);
				UtDebug.Log("Attachment added to Zendesk Ticket. Total count: " + m_ZendeskSupportUI.attachmentPaths.Count);
			}
		}
		string text = new List<ZendeskCustomTextFieldScript>(m_ZendeskSupportUI.gameObject.GetComponentsInChildren<ZendeskCustomTextFieldScript>()).Find((ZendeskCustomTextFieldScript t) => t.idCustomField == m_ZendeskSupportUI.requestForm.messageId).inputField.text;
		m_ZendeskSupportUI.SendRequest(DeleteTempFile, IsSpamMessage(text), _FakeSendWaitRange);
	}

	private bool IsSpamMessage(string inMessage)
	{
		return Regex.Matches(inMessage, _RegexFilter).Count < _RegexValidMatchThreshold;
	}

	private string CopyFile(string path)
	{
		string text = Application.persistentDataPath + "/TempLog.txt";
		File.Copy(path, text, overwrite: true);
		return text;
	}

	private void DeleteTempFile()
	{
		if (File.Exists(mTempFilePath))
		{
			File.Delete(mTempFilePath);
		}
	}

	public string FetchPrepopulatedString(string label)
	{
		SupportFieldType supportFieldType = SupportFieldType.None;
		string result = "";
		foreach (SupportFieldData supportFieldDatum in m_SupportFieldData)
		{
			if (supportFieldDatum._Label == label)
			{
				supportFieldType = supportFieldDatum._SupportFieldType;
				break;
			}
		}
		switch (supportFieldType)
		{
		case SupportFieldType.DeviceType:
			result = SystemInfo.deviceModel.ToString();
			break;
		case SupportFieldType.SoftwareVersion:
			result = SystemInfo.operatingSystem.ToString();
			break;
		case SupportFieldType.VikingName:
			if (AvatarData.pInstanceInfo.mInstance != null)
			{
				result = AvatarData.pInstanceInfo.mInstance.DisplayName;
			}
			break;
		case SupportFieldType.Email:
			if (UiLogin.pParentInfo != null)
			{
				result = UiLogin.pParentInfo.Email;
			}
			break;
		case SupportFieldType.Username:
			result = UiLogin.pUserName;
			break;
		}
		return result;
	}

	public int FetchDropDownIndex(string label, DropdownOptions[] options)
	{
		SupportFieldType supportFieldType = SupportFieldType.None;
		foreach (SupportFieldData supportFieldDatum in m_SupportFieldData)
		{
			if (supportFieldDatum._Label == label)
			{
				supportFieldType = supportFieldDatum._SupportFieldType;
				break;
			}
		}
		bool flag = false;
		if (supportFieldType == SupportFieldType.Platform)
		{
			List<string> list = options.Select((DropdownOptions a) => a.Tag).ToList();
			foreach (PlatformTypeData item in m_PlatformData)
			{
				switch (item._PlatformType)
				{
				case PlatformType.iOS:
					if (Application.platform == RuntimePlatform.IPhonePlayer)
					{
						return list.FindIndex((string a) => a == item._Tag) + 1;
					}
					break;
				case PlatformType.GooglePlay:
					if (Application.platform == RuntimePlatform.Android)
					{
						return list.FindIndex((string a) => a == item._Tag) + 1;
					}
					break;
				case PlatformType.PCStandaloneWeb:
					if (Application.platform == RuntimePlatform.WindowsPlayer || (Application.platform == RuntimePlatform.WindowsEditor && !flag))
					{
						return list.FindIndex((string a) => a == item._Tag) + 1;
					}
					break;
				case PlatformType.MobileWindowsStore:
					if (Application.platform == RuntimePlatform.MetroPlayerARM)
					{
						return list.FindIndex((string a) => a == item._Tag) + 1;
					}
					break;
				case PlatformType.PCWindowsStore:
					if (Application.platform == RuntimePlatform.MetroPlayerX64 || Application.platform == RuntimePlatform.MetroPlayerX86)
					{
						return list.FindIndex((string a) => a == item._Tag) + 1;
					}
					break;
				case PlatformType.PCSteam:
					if (Application.platform == RuntimePlatform.WindowsPlayer && flag)
					{
						return list.FindIndex((string a) => a == item._Tag) + 1;
					}
					break;
				case PlatformType.MacSteam:
					if ((Application.platform == RuntimePlatform.OSXEditor || Application.platform == RuntimePlatform.OSXPlayer) && flag)
					{
						return list.FindIndex((string a) => a == item._Tag) + 1;
					}
					break;
				case PlatformType.MacStandaloneWeb:
					if (Application.platform == RuntimePlatform.OSXEditor || Application.platform == RuntimePlatform.OSXPlayer)
					{
						return list.FindIndex((string a) => a == item._Tag) + 1;
					}
					break;
				default:
					return list.FindIndex((string a) => item._Tag == PlatformType.Other.ToString()) + 1;
				}
			}
			return 0;
		}
		return 0;
	}
}
