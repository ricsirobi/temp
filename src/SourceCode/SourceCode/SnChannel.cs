using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

[Serializable]
[RequireComponent(typeof(AudioSource))]
public class SnChannel : KAMonoBase, SnIChannel
{
	[Serializable]
	public class Pool
	{
		private string mName;

		private List<SnChannel> mChannels;

		public string pName
		{
			get
			{
				return mName;
			}
			set
			{
				mName = value;
			}
		}

		public List<SnChannel> pChannels
		{
			get
			{
				return mChannels;
			}
			set
			{
				mChannels = value;
			}
		}

		public Pool(string inName)
		{
			mChannels = new List<SnChannel>();
			mName = inName;
		}

		public bool AddChannel(SnChannel inChannel)
		{
			if (mChannels == null)
			{
				return false;
			}
			mChannels.Add(inChannel);
			if (mTurnedOffPools != null)
			{
				if (mName == "DEFAULT_POOL")
				{
					inChannel.SetVolume(pSoundGroupVolume);
					if (pTurnOffSoundGroup)
					{
						Mute(mute: true);
					}
				}
				else
				{
					PoolInfo poolInfo = mTurnedOffPools.Find((PoolInfo p) => p._Name.Equals(mName));
					if (poolInfo != null)
					{
						switch (poolInfo._Group)
						{
						case PoolGroup.MUSIC:
							inChannel.SetVolume(pMusicGroupVolume);
							if (pTurnOffMusicGroup)
							{
								Mute(mute: true);
							}
							break;
						case PoolGroup.SOUND:
							inChannel.SetVolume(pSoundGroupVolume);
							if (pTurnOffSoundGroup)
							{
								Mute(mute: true);
							}
							break;
						}
					}
				}
			}
			UtDebug.Log("Channel added - channel: " + inChannel.pName + ", pool: " + inChannel.pPool, 4u);
			return true;
		}

		public SnChannel AcquireChannel(int inPriority, bool inForce)
		{
			bool flag = true;
			float num = 0f;
			int num2 = 0;
			int index = 0;
			SnChannel snChannel = null;
			for (int i = 0; i < mChannels.Count; i++)
			{
				if (mChannels[i] != null)
				{
					if (!mChannels[i].pInUse)
					{
						snChannel = mChannels[i];
						break;
					}
					if (mChannels[i].pPriority < num2 || flag)
					{
						num2 = mChannels[i].pPriority;
						num = mChannels[i].pUseTime;
						index = i;
						flag = false;
					}
					else if (mChannels[i].pPriority == num2 && mChannels[i].pUseTime < num)
					{
						num = mChannels[i].pUseTime;
						index = i;
					}
				}
			}
			if (snChannel == null && (inForce || num2 < inPriority) && mChannels.Count > 0)
			{
				snChannel = mChannels[index];
			}
			if (snChannel != null)
			{
				snChannel.Acquire();
				snChannel.pPriority = inPriority;
			}
			return snChannel;
		}

		public void ReleaseChannels()
		{
			foreach (SnChannel mChannel in mChannels)
			{
				if (mChannel != null && mChannel.pInUse)
				{
					mChannel.Release();
				}
			}
		}

		public SnChannel FindChannel(string inName)
		{
			foreach (SnChannel mChannel in mChannels)
			{
				if (mChannel != null && mChannel.pName == inName)
				{
					return mChannel;
				}
			}
			return null;
		}

		public void SetVolume(float inVolume)
		{
			foreach (SnChannel mChannel in mChannels)
			{
				if (mChannel != null)
				{
					mChannel.pVolume = inVolume;
				}
			}
		}

		public void SetPosition(Vector3 inPosition)
		{
			foreach (SnChannel mChannel in mChannels)
			{
				if (mChannel != null)
				{
					mChannel.pTransform.position = inPosition;
				}
			}
		}

		public void Play()
		{
			foreach (SnChannel mChannel in mChannels)
			{
				if (mChannel != null && mChannel.pInUse)
				{
					mChannel.Play();
				}
			}
		}

		public void Pause()
		{
			foreach (SnChannel mChannel in mChannels)
			{
				if (mChannel != null)
				{
					mChannel.Pause();
				}
			}
		}

		public void Stop()
		{
			foreach (SnChannel mChannel in mChannels)
			{
				if (mChannel != null)
				{
					mChannel.Stop();
				}
			}
		}

		public void Mute(bool mute)
		{
			foreach (SnChannel mChannel in mChannels)
			{
				if (mChannel != null)
				{
					mChannel.Mute(mute);
				}
			}
		}

		public void Kill()
		{
			foreach (SnChannel mChannel in mChannels)
			{
				if (mChannel != null)
				{
					mChannel.Kill();
				}
			}
		}

		public void Update()
		{
			if (mChannels == null)
			{
				return;
			}
			foreach (SnChannel mChannel in mChannels)
			{
				if (mChannel != null && mChannel.pInUse)
				{
					mChannel.Tick();
				}
			}
		}
	}

	public string _Pool = "";

	public int _Priority;

	public float _RolloffBegin;

	public float _RolloffEnd = float.PositiveInfinity;

	public float _RolloffMinVolume;

	public float _RolloffMaxVolume = 1f;

	public bool _UseRolloffDistance;

	public bool _Persistent;

	public bool _ReleaseOnStop = true;

	public bool _FollowListener;

	public GameObject _EventTarget;

	public SnTriggerList _Triggers;

	public AudioClip[] _ClipQueue;

	public SnTriggerList[] _TriggerQueue;

	public bool _LoopQueue;

	public bool _RandomQueue;

	public bool _Draw;

	private bool mDelayedStart;

	private float mUseTime = -1f;

	private SnEventType mLastEvent;

	private float mLastTriggerTime = -1f;

	private int mLastTrigger = -1;

	private int mCurrentClip = -1;

	private float mRolloffDifference;

	private string mStreamClip;

	private SnIStream mStreamInstance;

	private SnSettings mDefaultSettings;

	private float mVolume = 1f;

	private bool mAppPaused;

	private static List<PoolInfo> mTurnedOffPools = new List<PoolInfo>();

	private static bool mLoadedTurnOffMusicGroupStatus = false;

	private static bool mTurnOffMusicGroup = false;

	private static bool mLoadedTurnOffSoundGroupStatus = false;

	private static bool mTurnOffSoundGroup = false;

	private static bool mLoadedMusicGroupVolumeStatus = false;

	private static float mMusicGroupVolume;

	private static bool mLoadedSoundGroupVolumeStatus = false;

	private static float mSoundGroupVolume = 1f;

	public const string DEFAULT_POOL_NAME = "DEFAULT_POOL";

	public const uint LOG_MASK = 4u;

	private static AudioListener mListener = null;

	private static float mGlobalVolume = 1f;

	private static Pool mDefaultPool = null;

	private static List<Pool> mChannelPools = new List<Pool>();

	private static bool mInitialized = false;

	private static GameObject mChannelUnderConstruction = null;

	private static string mGlobalReductionPool = "";

	private static string mGlobalReductionChannel = "";

	private static float mGlobalVolumeReduction = 0f;

	private static SnChannel mGlobalReductionInstance = null;

	public string pName => base.gameObject.name;

	public string pPool => _Pool;

	public int pPriority
	{
		get
		{
			return _Priority;
		}
		set
		{
			_Priority = value;
		}
	}

	public float pUseTime => mUseTime;

	public bool pInUse => mUseTime != -1f;

	public bool pIsStreaming => mStreamClip != null;

	public bool pIsPlaying
	{
		get
		{
			if (mStreamInstance == null)
			{
				return base.audio.isPlaying;
			}
			return mStreamInstance.pPlaying;
		}
	}

	public string pClipName => GetClipName();

	public AudioClip pClip
	{
		get
		{
			return GetClip();
		}
		set
		{
			SetClip(value);
		}
	}

	public AudioSource pAudioSource => base.audio;

	public SnIStream pAudioStream => mStreamInstance;

	public GameObject pGameObject => base.gameObject;

	public Transform pTransform => base.transform;

	public float pVolume
	{
		get
		{
			return GetVolume();
		}
		set
		{
			SetVolume(value);
		}
	}

	public bool pLoop
	{
		get
		{
			return GetLoop();
		}
		set
		{
			SetLoop(value);
		}
	}

	public float pRolloffBegin
	{
		get
		{
			return _RolloffBegin;
		}
		set
		{
			if (_RolloffBegin != value)
			{
				_RolloffBegin = value;
				OnRolloffChanged();
			}
		}
	}

	public float pRolloffEnd
	{
		get
		{
			return _RolloffEnd;
		}
		set
		{
			if (_RolloffEnd != value)
			{
				_RolloffEnd = value;
				OnRolloffChanged();
			}
		}
	}

	public bool pUseRolloffDistance
	{
		get
		{
			return _UseRolloffDistance;
		}
		set
		{
			if (_UseRolloffDistance != value)
			{
				_UseRolloffDistance = value;
				OnRolloffChanged();
			}
		}
	}

	public bool pReleaseOnStop
	{
		get
		{
			return _ReleaseOnStop;
		}
		set
		{
			_ReleaseOnStop = value;
		}
	}

	public bool pFollowListener
	{
		get
		{
			return _FollowListener;
		}
		set
		{
			_FollowListener = value;
		}
	}

	public GameObject pEventTarget
	{
		get
		{
			return _EventTarget;
		}
		set
		{
			_EventTarget = value;
		}
	}

	public SnEventType pLastEvent => mLastEvent;

	public SnTriggerList pTriggers
	{
		get
		{
			return _Triggers;
		}
		set
		{
			_Triggers = value;
			mLastTrigger = -1;
			mLastTriggerTime = -1f;
		}
	}

	public bool pLoopQueue
	{
		get
		{
			return _LoopQueue;
		}
		set
		{
			_LoopQueue = value;
		}
	}

	public AudioClip[] pClipQueue
	{
		get
		{
			return _ClipQueue;
		}
		set
		{
			_ClipQueue = value;
			mCurrentClip = -1;
		}
	}

	public SnTriggerList[] pTriggerQueue
	{
		get
		{
			return _TriggerQueue;
		}
		set
		{
			_TriggerQueue = value;
		}
	}

	public SnSettings pDefaultSettings
	{
		get
		{
			return mDefaultSettings;
		}
		set
		{
			mDefaultSettings = value;
		}
	}

	public static List<PoolInfo> pTurnedOffPools => mTurnedOffPools;

	public static bool pTurnOffMusicGroup
	{
		get
		{
			if (!mLoadedTurnOffMusicGroupStatus)
			{
				mTurnOffMusicGroup = PlayerPrefs.GetInt("MusicMute", 1) == 0;
				mLoadedTurnOffMusicGroupStatus = true;
			}
			return mTurnOffMusicGroup;
		}
		set
		{
			PlayerPrefs.SetInt("MusicMute", (!value) ? 1 : 0);
			mTurnOffMusicGroup = value;
			mLoadedTurnOffMusicGroupStatus = true;
			TurnOffPools(value, PoolGroup.MUSIC);
		}
	}

	public static bool pTurnOffSoundGroup
	{
		get
		{
			if (!mLoadedTurnOffSoundGroupStatus)
			{
				mTurnOffSoundGroup = PlayerPrefs.GetInt("SoundMute", 1) == 0;
				mLoadedTurnOffSoundGroupStatus = true;
			}
			return mTurnOffSoundGroup;
		}
		set
		{
			PlayerPrefs.SetInt("SoundMute", (!value) ? 1 : 0);
			mTurnOffSoundGroup = value;
			mLoadedTurnOffSoundGroupStatus = true;
			TurnOffPools(value, PoolGroup.SOUND);
		}
	}

	public static float pMusicGroupVolume
	{
		get
		{
			if (!mLoadedMusicGroupVolumeStatus)
			{
				mMusicGroupVolume = PlayerPrefs.GetFloat("MusicLevel", 1f);
				mLoadedMusicGroupVolumeStatus = true;
			}
			return mMusicGroupVolume;
		}
		set
		{
			PlayerPrefs.SetFloat("MusicLevel", value);
			mMusicGroupVolume = value;
			SetVolumeForPoolGroup(value, PoolGroup.MUSIC);
			mLoadedMusicGroupVolumeStatus = true;
		}
	}

	public static float pSoundGroupVolume
	{
		get
		{
			if (!mLoadedSoundGroupVolumeStatus)
			{
				mSoundGroupVolume = PlayerPrefs.GetFloat("SoundLevel", 1f);
				mLoadedSoundGroupVolumeStatus = true;
			}
			return mSoundGroupVolume;
		}
		set
		{
			PlayerPrefs.SetFloat("SoundLevel", value);
			mSoundGroupVolume = value;
			SetVolumeForPoolGroup(value, PoolGroup.SOUND);
			mLoadedSoundGroupVolumeStatus = true;
		}
	}

	public static GameObject pChannelUnderConstruction => mChannelUnderConstruction;

	public static AudioListener pListener
	{
		get
		{
			return mListener;
		}
		set
		{
			mListener = value;
		}
	}

	public static float pGlobalVolume
	{
		get
		{
			return mGlobalVolume;
		}
		set
		{
			mGlobalVolume = value;
			SyncListenerWithGlobalVolume();
		}
	}

	public static string pDefaultPool
	{
		get
		{
			if (mDefaultPool != null)
			{
				return mDefaultPool.pName;
			}
			return null;
		}
		set
		{
			mDefaultPool = FindPool(value);
		}
	}

	public static List<Pool> pChannelPools => mChannelPools;

	public bool Acquire()
	{
		base.audio.Stop();
		if (mStreamInstance != null)
		{
			mStreamInstance.Stop();
		}
		bool num = mLastEvent == SnEventType.PLAY || mLastEvent == SnEventType.PAUSE;
		mDelayedStart = false;
		mUseTime = Time.time;
		AudioClip inClip = pClip;
		base.audio.clip = null;
		mStreamInstance = null;
		mStreamClip = null;
		_Priority = 0;
		ProcessChannelForGlobalVolumeReduction(this, inAcquire: true);
		GameObject eventTarget = _EventTarget;
		_EventTarget = null;
		mLastEvent = SnEventType.NONE;
		pTriggers = null;
		pClipQueue = null;
		if (num)
		{
			SendEvent(SnEventType.STOP, inClip, eventTarget);
		}
		return true;
	}

	public bool Release()
	{
		return DoRelease(inSendEvent: true);
	}

	private bool DoRelease(bool inSendEvent)
	{
		base.audio.Stop();
		if (mStreamInstance != null)
		{
			mStreamInstance.Stop();
		}
		bool num = (mLastEvent == SnEventType.PLAY || mLastEvent == SnEventType.PAUSE) && inSendEvent;
		mDelayedStart = false;
		mUseTime = -1f;
		AudioClip inClip = pClip;
		base.audio.clip = null;
		mStreamInstance = null;
		mStreamClip = null;
		_Priority = 0;
		ProcessChannelForGlobalVolumeReduction(this, inAcquire: false);
		GameObject eventTarget = _EventTarget;
		_EventTarget = null;
		mLastEvent = SnEventType.NONE;
		pTriggers = null;
		pClipQueue = null;
		if (num)
		{
			SendEvent(SnEventType.STOP, inClip, eventTarget);
		}
		return true;
	}

	public void ApplySettings(SnSettings inSettings)
	{
		float num = 1f;
		string poolName = (string.IsNullOrEmpty(inSettings._Pool) ? mDefaultPool.pName : inSettings._Pool);
		PoolInfo poolInfo = mTurnedOffPools.Find((PoolInfo obj) => obj._Name == poolName);
		if (poolInfo != null)
		{
			num = ((poolInfo._Group == PoolGroup.MUSIC) ? pMusicGroupVolume : pSoundGroupVolume);
		}
		pVolume = inSettings._Volume * num;
		base.audio.pitch = inSettings._Pitch;
		_RolloffMinVolume = inSettings._MinVolume;
		_RolloffMaxVolume = inSettings._MaxVolume;
		pLoop = inSettings._Loop;
		_Priority = inSettings._Priority;
		_ReleaseOnStop = inSettings._ReleaseOnStop;
		_FollowListener = inSettings._FollowListener;
		_EventTarget = inSettings._EventTarget;
		_RolloffBegin = inSettings._RolloffBegin;
		_RolloffEnd = inSettings._RolloffEnd;
		_UseRolloffDistance = inSettings._UseRolloffDistance;
		if (_UseRolloffDistance)
		{
			OnRolloffChanged();
		}
	}

	public void Play()
	{
		mDelayedStart = false;
		if (mCurrentClip == -1 && _ClipQueue != null && _ClipQueue.Length != 0)
		{
			mCurrentClip = 0;
			pClip = _ClipQueue[0];
			if (_TriggerQueue != null && _TriggerQueue.Length != 0)
			{
				pTriggers = _TriggerQueue[0];
			}
		}
		AudioClip audioClip = pClip;
		if (audioClip != null)
		{
			if (mStreamInstance != null)
			{
				UtDebug.Log("############################## STOPPING PREVIOUS STREAM INSTANCE FOR NEXT PLAY, POOL: " + _Pool, 4u);
				mStreamInstance.Stop();
				mStreamInstance = null;
			}
			if (pIsStreaming)
			{
				UtDebug.Log("############################## STREAMING PLAY: " + mStreamClip + ", POOL: " + _Pool, 4u);
				mStreamInstance = SnStream.Create(base.audio, this);
			}
			else
			{
				UtDebug.Log("############################## NON-STREAMING PLAY: " + audioClip.name + ", POOL: " + _Pool, 4u);
				base.audio.Play();
				SendEvent(SnEventType.PLAY, audioClip, _EventTarget);
			}
		}
		else
		{
			Debug.LogWarning("WARNING: Play called with null clip, channel: " + pName + ", pool: " + _Pool);
			SendEvent(SnEventType.PLAY, null, _EventTarget);
		}
	}

	public void Play(SnSettings inSettings)
	{
		ApplySettings(inSettings);
		Play();
	}

	public void Pause()
	{
		if (!pInUse)
		{
			return;
		}
		if (mLastEvent == SnEventType.PLAY || mDelayedStart)
		{
			mDelayedStart = false;
			if (pIsStreaming)
			{
				if (mStreamInstance != null)
				{
					mStreamInstance.pPaused = true;
				}
			}
			else
			{
				base.audio.Pause();
			}
			SendEvent(SnEventType.PAUSE, pClip, _EventTarget);
		}
		else if (pIsPlaying)
		{
			Debug.LogError("ERROR: Invalid channel state, channel: " + pName + ", playing with last event: " + mLastEvent);
		}
	}

	public void Stop()
	{
		GameObject eventTarget = _EventTarget;
		AudioClip inClip = pClip;
		mLastTriggerTime = -1f;
		mLastTrigger = -1;
		mDelayedStart = false;
		if (pIsStreaming)
		{
			if (mStreamInstance != null)
			{
				mStreamInstance.Stop();
			}
		}
		else
		{
			base.audio.Stop();
		}
		if (_ReleaseOnStop)
		{
			DoRelease(inSendEvent: false);
		}
		SendEvent(SnEventType.STOP, inClip, eventTarget);
	}

	public void Mute(bool mute)
	{
		base.audio.mute = mute;
	}

	public void Kill()
	{
		mLastTriggerTime = -1f;
		mLastTrigger = -1;
		if (pIsStreaming)
		{
			if (mStreamInstance != null)
			{
				mStreamInstance.Stop();
			}
		}
		else
		{
			base.audio.Stop();
		}
		DoRelease(inSendEvent: false);
	}

	public void Tick()
	{
		AudioClip audioClip = pClip;
		if (mDelayedStart)
		{
			if (audioClip != null)
			{
				UtDebug.Assert(!pIsPlaying, "DELAYED START IS TRUE AND SOUND IS PLAYING!!");
				if (pIsPlaying)
				{
					if (pIsStreaming && mStreamInstance != null)
					{
						mStreamInstance.Stop();
					}
					base.audio.Stop();
				}
				mDelayedStart = false;
				Play();
			}
			else
			{
				Debug.LogError("ERROR: Delayed start is set on a channel with no clip, channel: " + pName);
				mDelayedStart = false;
			}
		}
		else
		{
			if (mAppPaused)
			{
				return;
			}
			if (!pIsPlaying)
			{
				if (mLastEvent != SnEventType.PLAY)
				{
					return;
				}
				GameObject eventTarget = _EventTarget;
				SnEvent[] tickTriggerEvents = GetTickTriggerEvents();
				if (mCurrentClip != -1)
				{
					mCurrentClip++;
					if (_LoopQueue && mCurrentClip >= _ClipQueue.Length)
					{
						mCurrentClip = 0;
					}
					if (mCurrentClip < _ClipQueue.Length)
					{
						pClip = _ClipQueue[mCurrentClip];
						if (_TriggerQueue != null)
						{
							if (mCurrentClip < _TriggerQueue.Length)
							{
								pTriggers = _TriggerQueue[mCurrentClip];
							}
							else
							{
								pTriggers = null;
							}
						}
						if (pIsStreaming)
						{
							UtDebug.Log("############################## STREAMING PLAY: " + mStreamClip, 4u);
							mStreamInstance = SnStream.Create(base.audio, null);
						}
						else
						{
							UtDebug.Log("############################## NON-STREAMING PLAY: " + pClipName, 4u);
							base.audio.Play();
						}
						SendTriggerEvents(tickTriggerEvents, eventTarget);
						SendEvent(SnEventType.END, audioClip, eventTarget);
						SendEvent(SnEventType.PLAY, pClip, eventTarget);
					}
					else
					{
						if (_ReleaseOnStop)
						{
							DoRelease(inSendEvent: false);
						}
						SendTriggerEvents(tickTriggerEvents, eventTarget);
						SendEvent(SnEventType.END_QUEUE, audioClip, eventTarget);
					}
				}
				else
				{
					if (_ReleaseOnStop)
					{
						DoRelease(inSendEvent: false);
					}
					SendTriggerEvents(tickTriggerEvents, eventTarget);
					SendEvent(SnEventType.END, audioClip, eventTarget);
				}
			}
			else if (audioClip == null)
			{
				Kill();
			}
			else
			{
				SendTriggerEvents(GetTickTriggerEvents(), _EventTarget);
			}
		}
	}

	public void LoadTriggers(string inURL)
	{
		TextAsset textAsset = (TextAsset)RsResourceManager.LoadAssetFromBundle(inURL, typeof(TextAsset));
		if (!(textAsset != null))
		{
			return;
		}
		string text = null;
		List<SnTrigger> list = new List<SnTrigger>();
		using StringReader stringReader = new StringReader(textAsset.text);
		while ((text = stringReader.ReadLine()) != null)
		{
			text = text.Trim();
			string[] array = text.Split(' ');
			if (array.Length == 2)
			{
				SnTrigger snTrigger = new SnTrigger();
				snTrigger._Name = array[1];
				snTrigger._Time = float.Parse(array[0]) / 1000f;
				list.Add(snTrigger);
			}
		}
		if (list.Count > 0)
		{
			pTriggers = new SnTriggerList(list.ToArray());
		}
	}

	private void SaveCurrentToDefaultSettings()
	{
		new SnSettings
		{
			_Volume = pVolume,
			_Pitch = base.audio.pitch,
			_MinVolume = _RolloffMinVolume,
			_MaxVolume = _RolloffMaxVolume,
			_Loop = pLoop,
			_Priority = _Priority,
			_ReleaseOnStop = _ReleaseOnStop,
			_FollowListener = _FollowListener,
			_EventTarget = _EventTarget,
			_RolloffBegin = _RolloffBegin,
			_RolloffEnd = _RolloffEnd,
			_UseRolloffDistance = _UseRolloffDistance
		};
	}

	public void SendEvent(SnEventType inEventID, AudioClip inClip, GameObject inEventTarget)
	{
		if (mLastEvent == inEventID)
		{
			return;
		}
		mLastEvent = inEventID;
		if (!(inEventTarget != null))
		{
			return;
		}
		SnEvent snEvent = default(SnEvent);
		snEvent.mType = inEventID;
		snEvent.mPool = pPool;
		snEvent.mChannel = pName;
		snEvent.mTrigger = null;
		snEvent.mClip = inClip;
		try
		{
			inEventTarget.SendMessage("OnSnEvent", snEvent, SendMessageOptions.DontRequireReceiver);
		}
		catch (Exception ex)
		{
			Debug.LogError("EXCEPTION: " + ex.ToString());
		}
		try
		{
			switch (inEventID)
			{
			case SnEventType.PLAY:
				inEventTarget.SendMessage("OnSnPlay", snEvent, SendMessageOptions.DontRequireReceiver);
				break;
			case SnEventType.PAUSE:
				inEventTarget.SendMessage("OnSnPause", snEvent, SendMessageOptions.DontRequireReceiver);
				break;
			case SnEventType.STOP:
				inEventTarget.SendMessage("OnSnStop", snEvent, SendMessageOptions.DontRequireReceiver);
				break;
			case SnEventType.END:
				inEventTarget.SendMessage("OnSnEnd", snEvent, SendMessageOptions.DontRequireReceiver);
				break;
			case SnEventType.END_QUEUE:
				inEventTarget.SendMessage("OnSnEndQueue", snEvent, SendMessageOptions.DontRequireReceiver);
				break;
			}
		}
		catch (Exception ex2)
		{
			Debug.LogError("EXCEPTION: " + ex2.ToString());
		}
	}

	private SnEvent[] GetTickTriggerEvents()
	{
		if (_Triggers != null)
		{
			SnEvent[] array = null;
			int num = 0;
			float num2 = base.audio.time;
			bool flag = false;
			bool flag2 = false;
			AudioClip audioClip = pClip;
			if (audioClip == null)
			{
				Debug.LogError("ERROR: Trigger events are not valid with a null clip reference!! channel = " + pName);
				mLastTrigger = -1;
				mLastTriggerTime = -1f;
				return null;
			}
			if (pLoop && mLastTriggerTime > num2)
			{
				num2 = audioClip.length;
				flag = true;
			}
			while (!flag2)
			{
				for (int i = mLastTrigger + 1; i < _Triggers.pLength && _Triggers[i]._Time < num2; i++)
				{
					if (array == null)
					{
						array = new SnEvent[_Triggers.pLength + 1];
					}
					array[num].mType = SnEventType.TRIGGER;
					array[num].mPool = pPool;
					array[num].mChannel = pName;
					array[num].mTrigger = _Triggers[i]._Name;
					array[num].mClip = audioClip;
					num++;
					mLastTrigger = i;
				}
				if (flag)
				{
					mLastTrigger = -1;
					mLastTriggerTime = -1f;
					num2 = base.audio.time;
					flag = false;
				}
				else
				{
					flag2 = true;
				}
			}
			if (num != 0)
			{
				array[num].mType = SnEventType.NONE;
			}
			mLastTriggerTime = num2;
			return array;
		}
		return null;
	}

	private void SendTriggerEvents(SnEvent[] inEvents, GameObject inEventTarget)
	{
		if (!(inEventTarget != null) || inEvents == null)
		{
			return;
		}
		for (int i = 0; i < inEvents.Length; i++)
		{
			SnEvent snEvent = inEvents[i];
			if (snEvent.mType == SnEventType.TRIGGER)
			{
				try
				{
					inEventTarget.SendMessage("OnSnEvent", snEvent, SendMessageOptions.DontRequireReceiver);
				}
				catch (Exception ex)
				{
					Debug.LogError("EXCEPTION: " + ex.ToString());
				}
				try
				{
					inEventTarget.SendMessage("OnSnTrigger", snEvent, SendMessageOptions.DontRequireReceiver);
				}
				catch (Exception ex2)
				{
					Debug.LogError("EXCEPTION: " + ex2.ToString());
				}
				continue;
			}
			break;
		}
	}

	private void OnRolloffChanged()
	{
		if (pUseRolloffDistance)
		{
			mRolloffDifference = pRolloffEnd - pRolloffBegin;
		}
	}

	private void ApplyRolloffToVolume(GameObject inListener)
	{
		float num = Vector3.Distance(base.transform.position, inListener.transform.position);
		float num2 = 1f;
		if (mRolloffDifference == 0f)
		{
			if (num > pRolloffBegin)
			{
				num2 = _RolloffMinVolume;
			}
			else
			{
				num2 = _RolloffMaxVolume;
			}
		}
		num2 = ((!(num > pRolloffBegin)) ? _RolloffMaxVolume : ((!(num < pRolloffEnd)) ? _RolloffMinVolume : Mathf.Lerp(_RolloffMaxVolume, _RolloffMinVolume, Mathf.InverseLerp(pRolloffBegin, pRolloffEnd, num))));
		if (mGlobalReductionInstance != null && mGlobalReductionInstance != this)
		{
			base.audio.volume = mVolume * mGlobalVolume * (1f - mGlobalVolumeReduction) * num2;
		}
		else
		{
			base.audio.volume = mVolume * mGlobalVolume * num2;
		}
	}

	private string GetClipName()
	{
		AudioClip clip = GetClip();
		if (clip != null)
		{
			return clip.name;
		}
		return null;
	}

	private AudioClip GetClip()
	{
		if (base.audio == null)
		{
			return null;
		}
		return base.audio.clip;
	}

	private void SetClip(AudioClip inClip)
	{
		if (base.audio.clip != inClip)
		{
			base.audio.clip = inClip;
		}
		if (inClip != null && (double)inClip.length < 0.002)
		{
			UtDebug.Log("++++++++++++++++++++++++++++++++++ Stream clip set to: " + inClip.name, 4u);
			mStreamClip = inClip.name;
		}
	}

	private float GetVolume()
	{
		return mVolume;
	}

	private void SetVolume(float inVolume)
	{
		mVolume = inVolume;
		SyncVolume();
	}

	private void SyncVolume()
	{
		if (mGlobalReductionInstance != null && mGlobalReductionInstance != this)
		{
			base.audio.volume = mVolume * mGlobalVolume * (1f - mGlobalVolumeReduction);
		}
		else
		{
			base.audio.volume = mVolume * mGlobalVolume;
		}
	}

	private bool GetLoop()
	{
		if (mStreamInstance != null)
		{
			return mStreamInstance.pLoop;
		}
		return base.audio.loop;
	}

	private void SetLoop(bool inLoop)
	{
		base.audio.loop = inLoop;
		if (mStreamInstance != null)
		{
			mStreamInstance.pLoop = inLoop;
		}
	}

	private void Awake()
	{
		if (pChannelUnderConstruction == base.gameObject)
		{
			return;
		}
		if (_Persistent)
		{
			if (FindChannel(_Pool, pName) != null)
			{
				base.audio.enabled = false;
				UnityEngine.Object.Destroy(base.gameObject);
				return;
			}
			UnityEngine.Object.DontDestroyOnLoad(base.gameObject);
		}
		if (!base.enabled)
		{
			Debug.LogError("ERROR: Adding a disabled channel is not a good idea. channel = " + pName);
		}
		if (AddChannel(this) && string.IsNullOrEmpty(_Pool))
		{
			_Pool = pDefaultPool;
		}
		mVolume = base.audio.volume;
		if (base.audio.playOnAwake && base.audio.clip != null)
		{
			pClip = base.audio.clip;
			mUseTime = Time.time;
			mDelayedStart = true;
			base.audio.playOnAwake = false;
			base.audio.Stop();
		}
		else if (base.audio.playOnAwake && _ClipQueue != null && _ClipQueue.Length != 0)
		{
			if (_RandomQueue)
			{
				UtUtilities.Shuffle(_ClipQueue);
			}
			Play();
		}
		if (_UseRolloffDistance)
		{
			OnRolloffChanged();
		}
	}

	private void Start()
	{
		SaveCurrentToDefaultSettings();
		SyncListenerWithGlobalVolume();
	}

	private void Update()
	{
		Tick();
	}

	private void LateUpdate()
	{
		if (!_FollowListener && !_UseRolloffDistance)
		{
			return;
		}
		if (mListener != null && (!mListener.enabled || !mListener.gameObject.activeInHierarchy))
		{
			mListener = null;
		}
		if (mListener == null)
		{
			mListener = (AudioListener)UnityEngine.Object.FindObjectOfType(typeof(AudioListener));
		}
		if (mListener != null)
		{
			if (_FollowListener)
			{
				base.transform.position = mListener.gameObject.transform.position;
			}
			if (_UseRolloffDistance)
			{
				ApplyRolloffToVolume(mListener.gameObject);
			}
		}
	}

	public void OnApplicationPause(bool pause)
	{
		mAppPaused = pause;
	}

	private void OnEnable()
	{
		if (!_Persistent)
		{
			Pool pool = FindPool(_Pool);
			if (pool != null && !pool.pChannels.Contains(this))
			{
				pool.AddChannel(this);
			}
		}
	}

	private void OnDisable()
	{
		if (!_Persistent)
		{
			if (pInUse)
			{
				Release();
			}
			FindPool(_Pool)?.pChannels.Remove(this);
		}
	}

	private void OnDrawGizmos()
	{
		if (_Draw && _UseRolloffDistance)
		{
			if (_RolloffBegin != 0f)
			{
				Gizmos.color = Color.green;
				Gizmos.DrawWireSphere(base.transform.position, _RolloffBegin);
			}
			if (_RolloffEnd != 0f && _RolloffEnd != float.PositiveInfinity && _RolloffEnd > _RolloffBegin)
			{
				Gizmos.color = Color.red;
				Gizmos.DrawWireSphere(base.transform.position, _RolloffEnd);
			}
		}
	}

	private static Pool FindPool(string inName)
	{
		if (mChannelPools != null)
		{
			if (string.IsNullOrEmpty(inName))
			{
				return mDefaultPool;
			}
			foreach (Pool mChannelPool in mChannelPools)
			{
				if (mChannelPool.pName == inName)
				{
					return mChannelPool;
				}
			}
		}
		return null;
	}

	private static void ProcessChannelForGlobalVolumeReduction(SnChannel inChannel, bool inAcquire)
	{
		if (!string.IsNullOrEmpty(mGlobalReductionChannel) && inChannel.pName == mGlobalReductionChannel)
		{
			string text = pDefaultPool;
			if (text == null)
			{
				text = "DEFAULT_POOL";
			}
			bool flag = false;
			string text2 = inChannel.pPool;
			if (string.IsNullOrEmpty(text2) || text2 == text)
			{
				if (string.IsNullOrEmpty(mGlobalReductionPool) || mGlobalReductionPool == text)
				{
					flag = true;
				}
			}
			else if (text2 == mGlobalReductionPool)
			{
				flag = true;
			}
			if (flag)
			{
				if (inAcquire)
				{
					mGlobalReductionInstance = inChannel;
				}
				else
				{
					mGlobalReductionInstance = null;
				}
				foreach (Pool mChannelPool in mChannelPools)
				{
					if (mChannelPool == null)
					{
						continue;
					}
					foreach (SnChannel pChannel in mChannelPool.pChannels)
					{
						if (pChannel != null && pChannel != inChannel && pChannel.pInUse)
						{
							pChannel.SyncVolume();
						}
					}
				}
			}
		}
		inChannel.SyncVolume();
	}

	public static bool Initialize()
	{
		if (!mInitialized)
		{
			mInitialized = true;
			Pool item = new Pool("DEFAULT_POOL");
			mChannelPools.Add(item);
			mDefaultPool = item;
			UtDebug.Log("SnSoundInitialize - Sound system initialized.", 4u);
			SyncListenerWithGlobalVolume();
		}
		return mInitialized;
	}

	public static void SyncListenerWithGlobalVolume()
	{
		mGlobalReductionInstance = null;
		if (!string.IsNullOrEmpty(mGlobalReductionChannel))
		{
			SnChannel snChannel = FindChannel(mGlobalReductionPool, mGlobalReductionChannel);
			if (snChannel != null && snChannel.pInUse)
			{
				mGlobalReductionInstance = snChannel;
			}
		}
		foreach (Pool mChannelPool in mChannelPools)
		{
			if (mChannelPool == null)
			{
				continue;
			}
			foreach (SnChannel pChannel in mChannelPool.pChannels)
			{
				if (pChannel != null && pChannel.pInUse)
				{
					pChannel.SyncVolume();
				}
			}
		}
	}

	public static void SetGlobalVolumeReduction(string inPool, string inChannel, float inReduction)
	{
		mGlobalReductionPool = inPool;
		mGlobalReductionChannel = inChannel;
		mGlobalVolumeReduction = inReduction;
		SyncListenerWithGlobalVolume();
	}

	public static void ClearGlobalVolumeReduction()
	{
		mGlobalReductionPool = "";
		mGlobalReductionChannel = "";
		mGlobalVolumeReduction = 0f;
		SyncListenerWithGlobalVolume();
	}

	public static void SetVolumeForPool(float inVolume, string inPool)
	{
		FindPool(inPool)?.SetVolume(inVolume);
	}

	public static void SetVolumeForAll(float inVolume)
	{
		if (mChannelPools == null)
		{
			return;
		}
		foreach (Pool mChannelPool in mChannelPools)
		{
			mChannelPool.SetVolume(inVolume);
		}
	}

	public static void SetVolumeForPoolGroup(float volume, PoolGroup poolGroup)
	{
		if (mTurnedOffPools == null || mTurnedOffPools.Count == 0)
		{
			return;
		}
		foreach (PoolInfo mTurnedOffPool in mTurnedOffPools)
		{
			if (!string.IsNullOrEmpty(mTurnedOffPool._Name) && mTurnedOffPool._Group == poolGroup)
			{
				SetVolumeForPool(volume, mTurnedOffPool._Name);
			}
		}
	}

	public static void SetPosition(string inPool, Vector3 inPosition)
	{
		FindPool(inPool)?.SetPosition(inPosition);
	}

	public static void PlayPool(string inPool)
	{
		FindPool(inPool)?.Play();
	}

	public static void PlayAll()
	{
		if (mChannelPools == null)
		{
			return;
		}
		foreach (Pool mChannelPool in mChannelPools)
		{
			mChannelPool.Play();
		}
	}

	public static void PausePool(string inPool)
	{
		FindPool(inPool)?.Pause();
	}

	public static void PauseAll()
	{
		if (mChannelPools == null)
		{
			return;
		}
		foreach (Pool mChannelPool in mChannelPools)
		{
			mChannelPool.Pause();
		}
	}

	public static void StopPool(string inPool)
	{
		FindPool(inPool)?.Stop();
	}

	public static void StopAll()
	{
		if (mChannelPools == null)
		{
			return;
		}
		foreach (Pool mChannelPool in mChannelPools)
		{
			mChannelPool.Stop();
		}
	}

	public static void MutePool(string inPool, bool mute)
	{
		FindPool(inPool)?.Mute(mute);
	}

	public static void MuteAll(bool mute)
	{
		if (mChannelPools == null)
		{
			return;
		}
		foreach (Pool mChannelPool in mChannelPools)
		{
			mChannelPool.Mute(mute);
		}
	}

	public static void KillPool(string inPool)
	{
		FindPool(inPool)?.Kill();
	}

	public static void KillAll()
	{
		if (mChannelPools == null)
		{
			return;
		}
		foreach (Pool mChannelPool in mChannelPools)
		{
			mChannelPool.Kill();
		}
	}

	public static bool AddChannel(SnChannel inChannel)
	{
		Initialize();
		Pool pool = FindPool(inChannel.pPool);
		if (pool == null)
		{
			pool = new Pool(inChannel.pPool);
			mChannelPools.Add(pool);
		}
		return pool.AddChannel(inChannel);
	}

	public static SnChannel AddChannel(SnSettings inSettings)
	{
		SnChannel snChannel = null;
		GameObject gameObject = new GameObject(inSettings._Channel);
		if ((bool)gameObject)
		{
			mChannelUnderConstruction = gameObject;
			snChannel = gameObject.AddComponent<SnChannel>();
			if ((bool)snChannel)
			{
				snChannel._Pool = inSettings._Pool;
				snChannel.ApplySettings(inSettings);
				if (AddChannel(snChannel))
				{
					if (string.IsNullOrEmpty(inSettings._Pool))
					{
						snChannel._Pool = pDefaultPool;
					}
				}
				else
				{
					snChannel = null;
				}
			}
			mChannelUnderConstruction = null;
		}
		return snChannel;
	}

	public static bool AddChannels(SnSettings[] inSettings)
	{
		bool result = false;
		for (int i = 0; i < inSettings.Length; i++)
		{
			if (!AddChannel(inSettings[i]))
			{
				result = false;
			}
		}
		return result;
	}

	public static SnChannel AcquireChannel()
	{
		return AcquireChannel(null, 0, inForce: false);
	}

	public static SnChannel AcquireChannel(int inPriority)
	{
		return AcquireChannel(null, inPriority, inForce: false);
	}

	public static SnChannel AcquireChannel(string inPool, bool inForce)
	{
		return AcquireChannel(inPool, 0, inForce);
	}

	public static SnChannel AcquireChannel(string inPool, int inPriority, bool inForce)
	{
		return FindPool(inPool)?.AcquireChannel(inPriority, inForce);
	}

	public static SnChannel AcquireChannel(string inPool, string inChannel, bool inForce)
	{
		return AcquireChannel(inPool, inChannel, 0, inForce);
	}

	public static SnChannel AcquireChannel(string inPool, string inChannel, int inPriority, bool inForce)
	{
		SnChannel snChannel = null;
		if (string.IsNullOrEmpty(inChannel))
		{
			snChannel = AcquireChannel(inPool, inPriority, inForce);
		}
		else
		{
			snChannel = FindChannel(inPool, inChannel);
			if (snChannel != null)
			{
				if (snChannel.pInUse && (!inForce || snChannel.pPriority >= inPriority))
				{
					snChannel = null;
				}
				else
				{
					snChannel.Acquire();
				}
			}
		}
		return snChannel;
	}

	public static void ReleaseChannels()
	{
		ReleaseChannels(null);
	}

	public static void ReleaseChannels(string inPool)
	{
		FindPool(inPool)?.ReleaseChannels();
	}

	public static void ReleaseAllChannels()
	{
		if (mChannelPools == null)
		{
			return;
		}
		foreach (Pool mChannelPool in mChannelPools)
		{
			mChannelPool.ReleaseChannels();
		}
	}

	public static SnChannel FindChannel(string inChannel)
	{
		return FindChannel(inChannel, null);
	}

	public static SnChannel FindChannel(string inPool, string inChannel)
	{
		return FindPool(inPool)?.FindChannel(inChannel);
	}

	public static bool IsValid(SnChannel inChannel)
	{
		if (inChannel != null)
		{
			if (inChannel.pAudioSource != null)
			{
				return true;
			}
			Debug.LogError("ERROR: Invalid channel state detected, AudioSource is not present on GameObject!!");
		}
		return false;
	}

	public static SnChannel Play(AudioClip inClip)
	{
		return Play(inClip, "", 0, inForce: false, null);
	}

	public static SnChannel Play(AudioClip inClip, int inPriority)
	{
		return Play(inClip, "", inPriority, inForce: false, null);
	}

	public static SnChannel Play(AudioClip inClip, bool inForce)
	{
		return Play(inClip, "", 0, inForce, null);
	}

	public static SnChannel Play(AudioClip inClip, int inPriority, bool inForce)
	{
		return Play(inClip, "", inPriority, inForce, null);
	}

	public static SnChannel Play(AudioClip inClip, string inPool, bool inForce)
	{
		return Play(inClip, inPool, 0, inForce, null);
	}

	public static SnChannel Play(AudioClip inClip, string inPool, int inPriority, bool inForce)
	{
		return Play(inClip, inPool, inPriority, inForce, null);
	}

	public static SnChannel Play(AudioClip inClip, string inPool, bool inForce, GameObject inEventTarget)
	{
		return Play(inClip, inPool, 0, inForce, inEventTarget);
	}

	public static SnChannel Play(AudioClip inClip, string inPool, int inPriority, bool inForce, GameObject inEventTarget)
	{
		SnChannel snChannel = AcquireChannel(inPool, inPriority, inForce);
		if (snChannel != null)
		{
			SnSettings snSettings = null;
			if (snChannel.pDefaultSettings != null)
			{
				snSettings = new SnSettings(snChannel.pDefaultSettings);
				snSettings._Priority = inPriority;
				snSettings._EventTarget = inEventTarget;
			}
			snChannel.pClip = inClip;
			snChannel.pEventTarget = inEventTarget;
			if (snSettings != null)
			{
				snChannel.Play(snSettings);
			}
			else
			{
				snChannel.Play();
			}
			return snChannel;
		}
		return null;
	}

	public static SnChannel Play(AudioClip inClip, string inChannel)
	{
		return Play(inClip, null, inChannel, 0, inForce: false, null);
	}

	public static SnChannel Play(AudioClip inClip, string inChannel, int inPriority)
	{
		return Play(inClip, null, inChannel, inPriority, inForce: false, null);
	}

	public static SnChannel Play(AudioClip inClip, string inPool, string inChannel, bool inForce)
	{
		return Play(inClip, inPool, inChannel, 0, inForce, null);
	}

	public static SnChannel Play(AudioClip inClip, string inPool, string inChannel, int inPriority, bool inForce)
	{
		return Play(inClip, inPool, inChannel, inPriority, inForce, null);
	}

	public static SnChannel Play(AudioClip inClip, string inPool, string inChannel, bool inForce, GameObject inEventTarget)
	{
		return Play(inClip, inPool, inChannel, 0, inForce, inEventTarget);
	}

	public static SnChannel Play(AudioClip inClip, string inPool, string inChannel, int inPriority, bool inForce, GameObject inEventTarget)
	{
		SnChannel snChannel = AcquireChannel(inPool, inChannel, inPriority, inForce);
		if (snChannel != null)
		{
			SnSettings snSettings = null;
			if (snChannel.pDefaultSettings != null)
			{
				snSettings = new SnSettings(snChannel.pDefaultSettings);
				snSettings._Priority = inPriority;
				snSettings._EventTarget = inEventTarget;
			}
			snChannel.pClip = inClip;
			snChannel.pEventTarget = inEventTarget;
			if (snSettings != null)
			{
				snChannel.Play(snSettings);
			}
			else
			{
				snChannel.Play();
			}
			return snChannel;
		}
		return null;
	}

	public static SnChannel Play(AudioClip inClip, SnSettings inSettings, bool inForce)
	{
		SnChannel snChannel = AcquireChannel(inSettings._Pool, inSettings._Channel, inSettings._Priority, inForce);
		if (snChannel != null)
		{
			snChannel.pClip = inClip;
			snChannel.Play(inSettings);
			return snChannel;
		}
		return null;
	}

	public static SnChannel Play(AudioClip inClip, SnSettings inSettings, SnTriggerList inTriggers, bool inForce)
	{
		SnChannel snChannel = AcquireChannel(inSettings._Pool, inSettings._Channel, inSettings._Priority, inForce);
		if (snChannel != null)
		{
			snChannel.pClip = inClip;
			snChannel.pTriggers = inTriggers;
			snChannel.Play(inSettings);
			return snChannel;
		}
		return null;
	}

	public static SnChannel Play(AudioClip[] inClipQueue, string inPool, int inPriority, bool inForce, GameObject inEventTarget)
	{
		SnChannel snChannel = AcquireChannel(inPool, inPriority, inForce);
		if (snChannel != null)
		{
			SnSettings snSettings = null;
			if (snChannel.pDefaultSettings != null)
			{
				snSettings = new SnSettings(snChannel.pDefaultSettings);
				snSettings._Priority = inPriority;
				snSettings._EventTarget = inEventTarget;
			}
			snChannel.pClipQueue = inClipQueue;
			snChannel.pEventTarget = inEventTarget;
			if (snSettings != null)
			{
				snChannel.Play(snSettings);
			}
			else
			{
				snChannel.Play();
			}
			return snChannel;
		}
		return null;
	}

	public static SnChannel Play(AudioClip[] inClipQueue, SnSettings inSettings, SnTriggerList[] inTriggerQueue, bool inForce)
	{
		SnChannel snChannel = AcquireChannel(inSettings._Pool, inSettings._Channel, inSettings._Priority, inForce);
		if (snChannel != null)
		{
			SnSettings snSettings = new SnSettings(inSettings);
			if (snSettings._Loop)
			{
				snSettings._Loop = false;
				snChannel.pLoopQueue = true;
			}
			else
			{
				snChannel.pLoopQueue = false;
			}
			snChannel.pClipQueue = inClipQueue;
			snChannel.pTriggerQueue = inTriggerQueue;
			snChannel.Play(snSettings);
			return snChannel;
		}
		return null;
	}

	public static void TurnOffPools(bool inOff, PoolGroup poolGroup)
	{
		if (mTurnedOffPools == null || mTurnedOffPools.Count == 0)
		{
			UtDebug.Log("############################## NO POOLS TO TURN OFF ", 4u);
			return;
		}
		foreach (PoolInfo mTurnedOffPool in mTurnedOffPools)
		{
			if (!string.IsNullOrEmpty(mTurnedOffPool._Name) && mTurnedOffPool._Group == poolGroup)
			{
				MutePool(mTurnedOffPool._Name, inOff);
			}
		}
		if (poolGroup == PoolGroup.SOUND)
		{
			MutePool("DEFAULT_POOL", inOff);
		}
	}

	public static void AddToTurnOffPools(PoolInfo[] poolInfo)
	{
		mTurnedOffPools.Clear();
		if (poolInfo == null || poolInfo.Length == 0)
		{
			return;
		}
		foreach (PoolInfo poolInfo2 in poolInfo)
		{
			if (!string.IsNullOrEmpty(poolInfo2._Name) && !mTurnedOffPools.Contains(poolInfo2))
			{
				mTurnedOffPools.Add(poolInfo2);
			}
		}
		TurnOffPools(pTurnOffMusicGroup, PoolGroup.MUSIC);
		TurnOffPools(pTurnOffSoundGroup, PoolGroup.SOUND);
		SetVolumeForPoolGroup(pMusicGroupVolume, PoolGroup.MUSIC);
		SetVolumeForPoolGroup(pSoundGroupVolume, PoolGroup.SOUND);
	}
}
