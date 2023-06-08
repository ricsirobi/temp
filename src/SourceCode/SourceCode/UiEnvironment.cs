using System.Globalization;
using KA.Framework;
using UnityEngine;
using UnityEngine.UI;

public class UiEnvironment : MonoBehaviour
{
	public Dropdown _DropDown;

	private string mBuildServerType;

	private string mPrevBuildType;

	private string mDropDownBuild = "DropDownBuild";

	public void Awake()
	{
		if (PlayerPrefs.GetInt(mDropDownBuild, 0) == 1)
		{
			PlayerPrefs.DeleteKey(mDropDownBuild);
			PlayerPrefs.DeleteKey("BuildServerType");
		}
		ProductConfig.pServerType = "";
		base.gameObject.SetActive(value: false);
		if (Application.isEditor && !ProductSettings.pInstance._EnableEnvironmentSelectionInEditor)
		{
			ProductStartup.pState = ProductStartup.State.UNINITIALIZED;
			SetServerType();
			base.gameObject.SetActive(value: false);
		}
		EnableDebugLog();
	}

	public void Start()
	{
		mBuildServerType = PlayerPrefs.GetString("BuildServerType", "qa");
		mPrevBuildType = mBuildServerType;
		PlayerPrefs.SetString("BuildServerType", mBuildServerType);
		PlayerPrefs.SetInt(mDropDownBuild, 1);
		SetServerType();
		_DropDown.value = GetDropDownValue();
	}

	public int GetDropDownValue()
	{
		if (mBuildServerType == "dev")
		{
			return 0;
		}
		if (mBuildServerType == "qa")
		{
			return 1;
		}
		if (mBuildServerType == "staging")
		{
			return 2;
		}
		if (mBuildServerType == "live")
		{
			return 3;
		}
		return 1;
	}

	public void SetServerType()
	{
		if (mBuildServerType == "dev")
		{
			ProductConfig.pServerType = "D";
		}
		else if (mBuildServerType == "qa")
		{
			ProductConfig.pServerType = "Q";
		}
		else if (mBuildServerType == "staging")
		{
			ProductConfig.pServerType = "S";
		}
		else if (mBuildServerType == "live")
		{
			ProductConfig.pServerType = "";
		}
	}

	public void DropdownValueChanged(Dropdown change)
	{
		if (change.value == 0)
		{
			mBuildServerType = "dev";
		}
		else if (change.value == 1)
		{
			mBuildServerType = "qa";
		}
		else if (change.value == 2)
		{
			mBuildServerType = "staging";
		}
		else if (change.value == 3)
		{
			mBuildServerType = "live";
		}
		SetServerType();
		PlayerPrefs.SetString("BuildServerType", mBuildServerType);
		PlayerPrefs.Save();
	}

	public void OnClickOk()
	{
		ProductStartup.pState = ProductStartup.State.UNINITIALIZED;
		if (mPrevBuildType != mBuildServerType && !Application.isEditor)
		{
			Caching.ClearCache();
		}
	}

	private void EnableDebugLog()
	{
		string @string = PlayerPrefs.GetString("SAVEMASK");
		string string2 = PlayerPrefs.GetString("SAVELEVEL");
		if (!string.IsNullOrEmpty(@string))
		{
			UtDebug._Mask = uint.Parse(@string, NumberStyles.Number);
		}
		if (!string.IsNullOrEmpty(string2))
		{
			UtDebug._Level = int.Parse(string2, NumberStyles.Number);
		}
	}
}
