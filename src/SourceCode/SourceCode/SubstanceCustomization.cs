using System.Collections.Generic;
using UnityEngine;

public class SubstanceCustomization
{
	public const int _PairDataID = 2014;

	private const string TEXTURE = "T";

	private const string COLOR = "C";

	public static SubstanceCustomization pInstance;

	private PairData mPairData;

	private string mObjectName = string.Empty;

	public bool mIsDirty;

	private List<SubstanceInfo> mSubstancePart = new List<SubstanceInfo>();

	public static bool pIsReady
	{
		get
		{
			if (pInstance != null)
			{
				return pInstance.pPairData != null;
			}
			return false;
		}
	}

	public PairData pPairData => mPairData;

	public static void Init(string inName)
	{
		if (pInstance == null)
		{
			pInstance = new SubstanceCustomization();
			PairData.Load(2014, pInstance.SubstancePairDataHandler, null);
		}
		pInstance.mObjectName = inName;
		pInstance.ClearSubstanceParts();
	}

	private void SubstancePairDataHandler(bool success, PairData inData, object inUserData)
	{
		if (inData != null)
		{
			mPairData = inData;
		}
		else
		{
			UtDebug.LogError("PairData Loding Failed!!!!!!!");
		}
	}

	public bool IsPropertyAvailable(string inProperty)
	{
		if (mPairData != null && mPairData.pPairList != null)
		{
			string value = mPairData.GetValue(inProperty + mObjectName + "C");
			if (!value.Equals("LIST_NOT_VALID") && !value.Equals("___VALUE_NOT_FOUND___"))
			{
				return true;
			}
		}
		return false;
	}

	public void GetProperty(string inProperty, out Color inColor)
	{
		inColor = Color.white;
		if (mPairData != null && mPairData.pPairList != null)
		{
			string value = mPairData.GetValue(inProperty + mObjectName + "C");
			if (!value.Equals("LIST_NOT_VALID") && !value.Equals("___VALUE_NOT_FOUND___"))
			{
				HexUtil.HexToColor(value, out inColor);
			}
		}
	}

	public void GetProperty(string inProperty, string inRenderer, out string inAssetPath)
	{
		inAssetPath = "";
		if (mPairData != null && mPairData.pPairList != null)
		{
			string value = mPairData.GetValue(inProperty + mObjectName + inRenderer + "T");
			if (!value.Equals("LIST_NOT_VALID") && !value.Equals("___VALUE_NOT_FOUND___"))
			{
				inAssetPath = value;
			}
		}
	}

	public void SetTextureProperty(Material mat, string rsPath, string propName)
	{
		SubstanceInfo substanceInfo = new SubstanceInfo(rsPath, mat, propName);
		mSubstancePart.Add(substanceInfo);
		string[] array = rsPath.Split('/');
		if (array.Length > 2)
		{
			RsResourceManager.LoadAssetFromBundle(array[0] + "/" + array[1], array[2], substanceInfo.OnTextureLoaded, typeof(Texture2D));
		}
	}

	public void ClearSubstanceParts()
	{
		mSubstancePart.Clear();
	}

	public void SetProperty(Color inColor, string inKey)
	{
		if (mPairData != null)
		{
			mIsDirty = true;
			string inValue = HexUtil.ColorToHex(inColor);
			mPairData.SetValue(inKey + mObjectName + "C", inValue);
		}
	}

	public void SetProperty(string inAssetPath, string inKey, string inRendererName)
	{
		if (mPairData != null)
		{
			mIsDirty = true;
			mPairData.SetValue(inKey + mObjectName + inRendererName + "T", inAssetPath);
		}
	}

	public void SaveData()
	{
		if (mPairData != null && mIsDirty)
		{
			PairData.Save(2014);
			mIsDirty = false;
		}
	}

	public void FlushPairData()
	{
		PairData.DeleteData(2014);
	}
}
