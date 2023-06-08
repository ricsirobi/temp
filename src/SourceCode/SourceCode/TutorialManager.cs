using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class TutorialManager : MonoBehaviour
{
	public class Tutorial
	{
		public string mTutorialID;

		public string mTableID;

		public string mSound;

		public uint mFlags;

		public uint mRecord;

		public string mWaitInterfaceID;

		public string mWaitItemID;

		public string mResetInterfaceID;

		public string mResetItemID;

		public uint mMouseUpKillMode;

		public bool mBaseFlag;

		public Tutorial mTutorialManager;

		public GameObject mCallBackObj;

		public UtTableXMLReader mTableReader;

		public Tutorial()
		{
			mTutorialID = null;
			mTableID = null;
			mFlags = 0u;
			mRecord = 0u;
			mSound = null;
			mWaitInterfaceID = null;
			mWaitItemID = null;
			mResetInterfaceID = null;
			mResetItemID = null;
			mMouseUpKillMode = 0u;
		}
	}

	private const uint TU_FLAG_SOUND_INTERRUPTABLE = 1u;

	private const uint TU_FLAG_SOUND_WAITFOREND = 2u;

	private const uint TU_FLAG_SOUND_WAITFORDIALOG = 4u;

	private const uint TU_FLAG_MARK_DONE = 268435456u;

	private const uint TU_FLAG_SOUND_WAITFORSTART = 536870912u;

	private const uint TU_FLAG_WAITFORTUTORIAL = 1073741824u;

	public const uint tkmMouseNext = 1u;

	public const uint tkmSpaceNext = 2u;

	public const uint tkmAnyNext = 3u;

	public const uint tkmMouseKill = 4u;

	public const uint tkmSpaceKill = 8u;

	public const uint tkmAnyKill = 12u;

	public const uint tkmNoResetInput = 268435456u;

	private const string ActionShow = "SHOW";

	private const string ActionHide = "HIDE";

	private const string ActionEnable = "ENABLE";

	private const string ActionDisable = "DISABLE";

	private const string ActionFlash = "FLASH";

	private const string ActionStill = "STILL";

	private const string ActionWait = "WAIT";

	private const string ActionOneFrame = "ONEFRAME";

	private const string ActionInteractive = "INTERACTIVE";

	private const string ActionNotInteractive = "NOTINTERACTIVE";

	private static AssetBundle mAssetBundle;

	private static string mAssetBundleURL;

	public static bool mPlaying;

	private static List<Tutorial> mTutorialArray = new List<Tutorial>();

	public static Tutorial mTutorial = null;

	private static GameObject mInstance = null;

	private static SnChannel mChannel;

	private static bool mKillState = false;

	public static bool mIgnoreCompletedCheck = false;

	public static bool _Save = true;

	private void Awake()
	{
		mInstance = base.gameObject;
	}

	private void Update()
	{
		uint num = 0u;
		if (Input.GetButtonUp("MouseLeft") || Input.GetButtonUp("MouseRight"))
		{
			num = 1u;
		}
		if (Input.GetKeyUp("space"))
		{
			num |= 2u;
		}
		if (num != 0)
		{
			ActOnInput(num, forceEnd: false);
		}
		for (int i = 0; i < mTutorialArray.Count; i++)
		{
			Tutorial tutorial = mTutorialArray[i];
			if (tutorial.mBaseFlag)
			{
				tutorial.mBaseFlag = false;
				tutorial.mRecord++;
				PlayTutorial(tutorial, 0u);
			}
		}
	}

	private static void ActOnInput(uint input, bool forceEnd)
	{
		for (int i = 0; i < mTutorialArray.Count; i++)
		{
			if (mTutorialArray[i].mMouseUpKillMode > 3 || forceEnd)
			{
				input <<= 2;
				if ((mTutorialArray[i].mMouseUpKillMode & input) != 0 || forceEnd)
				{
					Tutorial tutorial = mTutorialArray[i];
					if (mTutorialArray.Count == 0)
					{
						mPlaying = false;
					}
					else if (i < mTutorialArray.Count && (tutorial.mFlags & 0x40000000u) != 0)
					{
						PlayTutorial(tutorial, 0u);
					}
					if ((tutorial.mFlags & 0x10000000u) != 0)
					{
						MarkTutorialDone(tutorial.mTutorialID);
					}
					OnTutorialDone(tutorial);
					KillTutorial(tutorial);
					if (SnChannel.IsValid(mChannel))
					{
						SnChannel snChannel = mChannel;
						mChannel = null;
						snChannel.Stop();
					}
					mTutorialArray.RemoveAt(i);
				}
			}
			else if ((mTutorialArray[i].mMouseUpKillMode & input) != 0 && SnChannel.IsValid(mChannel))
			{
				SnChannel snChannel2 = mChannel;
				mChannel = null;
				snChannel2.Stop();
			}
		}
	}

	private static void OnTutorialDone(Tutorial tutorial)
	{
		if (tutorial.mCallBackObj != null)
		{
			tutorial.mCallBackObj.BroadcastMessage("OnTutorialDone", tutorial.mTutorialID, SendMessageOptions.DontRequireReceiver);
		}
	}

	public static void StopTutorials()
	{
		ActOnInput(12u, forceEnd: true);
		if (SnChannel.IsValid(mChannel))
		{
			SnChannel snChannel = mChannel;
			mChannel = null;
			snChannel.Stop();
		}
	}

	public static bool StartTutorial(TextAsset ta, string inTutorial, bool bMarkDone, uint mouseUpKillMode, GameObject callBackObj)
	{
		if (!mIgnoreCompletedCheck && ProductData.TutorialComplete(inTutorial))
		{
			return false;
		}
		if ((mouseUpKillMode & 0x10000000) == 0)
		{
			Input.ResetInputAxes();
		}
		Tutorial tutorial = new Tutorial();
		tutorial.mTutorialID = inTutorial;
		tutorial.mFlags = 0u;
		tutorial.mRecord = 0u;
		tutorial.mSound = null;
		tutorial.mWaitInterfaceID = null;
		tutorial.mWaitItemID = null;
		tutorial.mMouseUpKillMode = mouseUpKillMode;
		tutorial.mCallBackObj = callBackObj;
		tutorial.mTableReader = new UtTableXMLReader();
		tutorial.mTableReader.LoadString(ta.text);
		mKillState = false;
		if (bMarkDone)
		{
			tutorial.mFlags |= 268435456u;
		}
		if (tutorial.mTutorialID != null)
		{
			mTutorialArray.Add(tutorial);
			mPlaying = true;
			mTutorial = tutorial;
			mAssetBundle = null;
			mAssetBundleURL = "";
			PlayTutorial(mTutorial, 0u);
			return true;
		}
		return false;
	}

	private static void AssetBundleReady(string inURL, RsResourceLoadEvent inEvent, float inProgress, object inFile, object inUserData)
	{
		if (inEvent == RsResourceLoadEvent.COMPLETE && inURL == mAssetBundleURL)
		{
			mAssetBundle = (AssetBundle)inFile;
			if (mAssetBundle != null)
			{
				PlayTutorial(mTutorial, 0u);
			}
			else
			{
				Debug.LogError(" Missing asset bundle: " + inURL + " !!");
			}
		}
	}

	public static void GetAssetBundle(Tutorial inTutorial, uint record)
	{
		UtTable utTable = inTutorial.mTableReader[inTutorial.mTutorialID];
		if (utTable == null)
		{
			Debug.LogError("Tutorial table [" + inTutorial.mTutorialID + "] not found ");
			return;
		}
		int fieldIndex = utTable.GetFieldIndex("tfSound");
		string value = utTable.GetValue<string>(fieldIndex, (int)record);
		if (value == null)
		{
			Debug.LogError("ERROR :: tfSound [" + inTutorial.mTutorialID + "] not found ");
			return;
		}
		string[] array = value.Split('/');
		string inURL = "RS_DATA/" + array[0];
		mAssetBundle = null;
		mAssetBundleURL = inURL;
		RsResourceManager.Load(inURL, AssetBundleReady);
	}

	public static void PlayTutorial(Tutorial inTutorial, uint state)
	{
		UtTable utTable = inTutorial.mTableReader[inTutorial.mTutorialID];
		while (inTutorial.mRecord < utTable.GetRecordCount())
		{
			int fieldIndex = utTable.GetFieldIndex("tfFlags");
			uint value = utTable.GetValue<uint>(fieldIndex, (int)inTutorial.mRecord);
			fieldIndex = utTable.GetFieldIndex("tfSound");
			string value2 = utTable.GetValue<string>(fieldIndex, (int)inTutorial.mRecord);
			string text = null;
			if (value2 != null)
			{
				text = value2.Split('/')[1];
			}
			fieldIndex = utTable.GetFieldIndex("tfInterface");
			string value3 = utTable.GetValue<string>(fieldIndex, (int)inTutorial.mRecord);
			fieldIndex = utTable.GetFieldIndex("tfButton");
			string value4 = utTable.GetValue<string>(fieldIndex, (int)inTutorial.mRecord);
			fieldIndex = utTable.GetFieldIndex("tfAction");
			string value5 = utTable.GetValue<string>(fieldIndex, (int)inTutorial.mRecord);
			if (!string.IsNullOrEmpty(value5) && value3 == null)
			{
				Debug.LogError("ERROR :: tfInterface in [" + inTutorial.mTutorialID + " record #= " + inTutorial.mRecord + "] not found ");
			}
			if ((value & 2u) != 0)
			{
				if (state == 4)
				{
					inTutorial.mFlags &= 4294967293u;
				}
				else if (inTutorial.mSound != null && inTutorial.mSound != text)
				{
					inTutorial.mFlags |= 2u;
					break;
				}
			}
			else if ((value & 4u) != 0)
			{
				if (state == 4)
				{
					inTutorial.mFlags &= 4294967291u;
				}
				else
				{
					SnChannel snChannel = SnChannel.FindChannel("VO_Pool", "VOChannel");
					if (snChannel.pAudioSource.clip != null && snChannel.pAudioSource.clip.name != text)
					{
						inTutorial.mFlags |= 4u;
						break;
					}
				}
			}
			if (state == 1)
			{
				inTutorial.mFlags &= 3758096383u;
			}
			else if (text != null)
			{
				if (mAssetBundle == null)
				{
					GetAssetBundle(inTutorial, inTutorial.mRecord);
					break;
				}
				inTutorial.mSound = text;
				state = 0u;
				AudioClip audioClip = (AudioClip)mAssetBundle.LoadAsset(text);
				if (audioClip != null)
				{
					mChannel = SnChannel.Play(audioClip, "VO_Pool", 1, inForce: false, mInstance);
				}
				else
				{
					Debug.LogError("Tutorial Audio Clip, " + text + " not found in asset bundle");
				}
			}
			if ((inTutorial.mFlags & 0x20000000u) != 0)
			{
				break;
			}
			if (value5 == "WAIT")
			{
				inTutorial.mWaitInterfaceID = value3;
				inTutorial.mWaitItemID = value4;
				break;
			}
			if (value5 == "ONEFRAME")
			{
				inTutorial.mBaseFlag = true;
				break;
			}
			DoAction(value3, value4, value5);
			inTutorial.mRecord++;
		}
		if (inTutorial.mRecord != utTable.GetRecordCount())
		{
			return;
		}
		if ((inTutorial.mFlags & 0x10000000u) != 0)
		{
			MarkTutorialDone(inTutorial.mTutorialID);
		}
		OnTutorialDone(inTutorial);
		int i;
		for (i = 0; i < mTutorialArray.Count; i++)
		{
			if (mTutorialArray[i].mTutorialID == inTutorial.mTutorialID)
			{
				mTutorialArray.RemoveAt(i);
				break;
			}
		}
		if (mTutorialArray.Count == 0)
		{
			mPlaying = false;
		}
		else if (i < mTutorialArray.Count)
		{
			Tutorial tutorial = mTutorialArray[i];
			if ((tutorial.mFlags & 0x40000000u) != 0)
			{
				PlayTutorial(tutorial, 0u);
			}
		}
	}

	public static bool IsTutorialPlaying()
	{
		return mPlaying;
	}

	public static bool IsPlaying(string inTutorial)
	{
		for (int i = 0; i < mTutorialArray.Count; i++)
		{
			if (mTutorialArray[i].mTutorialID == inTutorial)
			{
				return true;
			}
		}
		return false;
	}

	private static void ResetTutorial(ref Tutorial inTutorial)
	{
		inTutorial.mRecord = 0u;
		inTutorial.mSound = null;
		inTutorial.mWaitInterfaceID = null;
		inTutorial.mWaitItemID = null;
		if (inTutorial.mBaseFlag)
		{
			inTutorial.mBaseFlag = false;
		}
		PlayTutorial(inTutorial, 0u);
	}

	private static void KillTutorial(string inTutorial, bool bMarkDone)
	{
		int i;
		for (i = 0; i < mTutorialArray.Count; i++)
		{
			if (mTutorialArray[i].mTutorialID == inTutorial)
			{
				Tutorial tutorial = mTutorialArray[i];
				mTutorialArray.RemoveAt(i);
				if (bMarkDone)
				{
					MarkTutorialDone(tutorial.mTutorialID);
				}
				KillTutorial(tutorial);
				break;
			}
		}
		mKillState = true;
		if (mTutorialArray.Count == 0)
		{
			mPlaying = false;
		}
		else if (i < mTutorialArray.Count)
		{
			Tutorial tutorial2 = mTutorialArray[i];
			if ((tutorial2.mFlags & 0x40000000u) != 0)
			{
				PlayTutorial(tutorial2, 0u);
			}
		}
	}

	private static void KillTutorial(Tutorial inTutorial)
	{
		if (inTutorial.mBaseFlag)
		{
			inTutorial.mBaseFlag = false;
		}
		UtTable utTable = inTutorial.mTableReader[inTutorial.mTutorialID];
		inTutorial.mRecord--;
		while (inTutorial.mRecord != uint.MaxValue)
		{
			int fieldIndex = utTable.GetFieldIndex("tfAction");
			if (utTable.GetValue<string>(fieldIndex, (int)inTutorial.mRecord) == "FLASH")
			{
				fieldIndex = utTable.GetFieldIndex("tfInterface");
				string value = utTable.GetValue<string>(fieldIndex, (int)inTutorial.mRecord);
				fieldIndex = utTable.GetFieldIndex("tfButton");
				string value2 = utTable.GetValue<string>(fieldIndex, (int)inTutorial.mRecord);
				DoAction(value, value2, "STILL");
			}
			inTutorial.mRecord--;
		}
	}

	public static void MarkTutorialDone(string inTutorial)
	{
		if (_Save)
		{
			ProductData.AddTutorial(inTutorial);
		}
	}

	public static bool HasTutorialPlayed(string inTutorial)
	{
		if (mIgnoreCompletedCheck)
		{
			return false;
		}
		return ProductData.TutorialComplete(inTutorial);
	}

	private static void DoAction(string interfaceID, string itemID, string action)
	{
	}

	private static bool StartTutorialAfterPrev(ref string inTutorial, bool bMarkDone, uint mouseUpKillMode)
	{
		Tutorial tutorial = new Tutorial();
		tutorial.mTutorialID = inTutorial;
		tutorial.mFlags = 0u;
		tutorial.mRecord = 0u;
		tutorial.mSound = null;
		tutorial.mWaitInterfaceID = null;
		tutorial.mWaitItemID = null;
		tutorial.mMouseUpKillMode = mouseUpKillMode;
		if (bMarkDone)
		{
			tutorial.mFlags |= 268435456u;
		}
		if (inTutorial != null)
		{
			mTutorialArray.Add(tutorial);
			tutorial.mFlags |= 1073741824u;
			return true;
		}
		return false;
	}

	private void OnSnEvent(SnEvent snEvent)
	{
		if ((snEvent.mType == SnEventType.END || snEvent.mType == SnEventType.STOP) && SnChannel.IsValid(mChannel) && snEvent.mChannel == mChannel.pName)
		{
			mChannel = null;
		}
		if (!mKillState)
		{
			SndCallback((uint)snEvent.mType, snEvent.mClip.name);
		}
	}

	private static void SndCallback(uint state, string soundID)
	{
		for (int i = 0; i < mTutorialArray.Count; i++)
		{
			switch (state)
			{
			case 3u:
			case 4u:
				if (soundID == mTutorialArray[i].mSound)
				{
					mTutorialArray[i].mSound = null;
					if ((mTutorialArray[i].mFlags & 2u) != 0)
					{
						PlayTutorial(mTutorialArray[i], 4u);
					}
					return;
				}
				if ((mTutorialArray[i].mFlags & 4u) != 0)
				{
					PlayTutorial(mTutorialArray[i], 4u);
				}
				break;
			case 1u:
				if (soundID == mTutorialArray[i].mSound && (mTutorialArray[i].mFlags & 0x20000000u) != 0)
				{
					PlayTutorial(mTutorialArray[i], 1u);
					return;
				}
				break;
			}
		}
	}
}
