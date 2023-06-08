using System;
using System.Collections;
using UnityEngine;

public class AvatarRacing : MonoBehaviour
{
	[Serializable]
	public class EndGameAchievements
	{
		public int _Pos;

		public int _AchievementID;
	}

	public enum Type
	{
		USER,
		MMO,
		END
	}

	public enum RacingMessage
	{
		WRONG_WAY,
		FINAL_LAP
	}

	protected Type mType;

	protected int mCurrentLap;

	protected int mNextExpectedCheckPoint;

	protected bool mIsReady;

	public bool _GameCompleted;

	protected float mRaceTime;

	protected float mLapTime;

	protected float mBestLapTime;

	protected int mPosition;

	private CarCamera mCamera;

	private Transform mAvatarTransform;

	private GameObject mUiGenericDBRaceComplete;

	private GameObject mMiniMapDisplay;

	private float mDistanceForward;

	private float mDistanceUpdateTimer;

	private float mLastLapTime;

	private GameModes pGameMode;

	private RacingMMOClient mRacingMMOClient;

	private LevelManager mLevelManager;

	public GameObject pMiniMapDisplay
	{
		set
		{
			mMiniMapDisplay = value;
		}
	}

	public bool pIsReady
	{
		get
		{
			return mIsReady;
		}
		set
		{
			mIsReady = value;
		}
	}

	public Type pType
	{
		get
		{
			return mType;
		}
		set
		{
			mType = value;
		}
	}

	public int pLapsCompleted
	{
		get
		{
			return mCurrentLap;
		}
		set
		{
			mCurrentLap = value;
		}
	}

	public bool pGameCompleted
	{
		get
		{
			return _GameCompleted;
		}
		set
		{
			_GameCompleted = value;
		}
	}

	public float pRaceTime
	{
		get
		{
			return mRaceTime;
		}
		set
		{
			mRaceTime = value;
		}
	}

	public float pBestLapTime
	{
		get
		{
			return mBestLapTime;
		}
		set
		{
			mBestLapTime = value;
		}
	}

	public UiRacingMessage pUiRacingMessage
	{
		get
		{
			if (mLevelManager != null && mType == Type.USER)
			{
				return mLevelManager.pUiInGame;
			}
			return null;
		}
	}

	public UiRaceTrackHUD pUiRaceTrackHUD
	{
		get
		{
			if (mLevelManager != null && mType == Type.USER)
			{
				return mLevelManager.pUiRaceHUD;
			}
			return null;
		}
	}

	public float pDistanceCovered
	{
		get
		{
			return mDistanceForward;
		}
		set
		{
			mDistanceForward = value;
		}
	}

	public int pPosition
	{
		get
		{
			return mPosition;
		}
		set
		{
			if (pUiRaceTrackHUD != null)
			{
				if (mPosition != value)
				{
					mPosition = value;
					if (pGameMode == GameModes.SURVIVAL)
					{
						pUiRaceTrackHUD.SetPosition(mPosition);
					}
					else
					{
						pUiRaceTrackHUD.SetPosition(mPosition);
					}
				}
			}
			else
			{
				mPosition = value;
			}
		}
	}

	private void Start()
	{
		Restart();
		mAvatarTransform = base.transform;
		AvAvatar.pLevelState = AvAvatarLevelState.RACING;
	}

	public void Init(RacingMMOClient racingMMOClient, LevelManager levelManager)
	{
		mRacingMMOClient = racingMMOClient;
		mLevelManager = levelManager;
	}

	private void OnEnable()
	{
		if (mLevelManager != null)
		{
			mRaceTime = mLevelManager.pElapsedTime;
		}
		RacingCheckpoint.OnHit += OnCheckpointHit;
	}

	private void OnDisable()
	{
		RacingCheckpoint.OnHit -= OnCheckpointHit;
	}

	private void Update()
	{
		SetRacePosition(pPosition);
		if (!(mLevelManager == null) && mLevelManager.pIsInGame)
		{
			DistanceUpdate();
			if (mType == Type.USER)
			{
				UpdateUI();
			}
			if (Application.isEditor && Input.GetKeyDown(KeyCode.L))
			{
				LapCompleted();
			}
		}
	}

	private void SetRacePosition(int index)
	{
		AvAvatarController component = GetComponent<AvAvatarController>();
		if (component != null && mLevelManager != null)
		{
			float num = mLevelManager.pPlayerData.Count;
			if (mLevelManager._FlyingBoost != null && num > 1f && index > 0 && index < mLevelManager._FlyingBoost.Length)
			{
				component.pFlyingPositionBoost = mLevelManager._FlyingBoost[index - 1];
			}
			else
			{
				component.pFlyingPositionBoost = 0f;
			}
		}
		SetPositionSprite(index);
	}

	public void SetPositionSprite(int index)
	{
		AvAvatarController component = GetComponent<AvAvatarController>();
		if (component == null)
		{
			return;
		}
		Transform transform = component.transform.Find("RacingPositionSprite");
		if (!(transform == null))
		{
			ObRacingPosition component2 = transform.GetComponent<ObRacingPosition>();
			if (!(component2 == null) && component2.GetPosition() != index)
			{
				component2.SetPosition(index);
			}
		}
	}

	private void InitCamera()
	{
		if (mCamera != null || mType != 0)
		{
			return;
		}
		Transform transform = base.transform.Find("Eye");
		Camera camera = null;
		if (transform != null)
		{
			camera = transform.GetComponent<Camera>();
		}
		if (camera == null)
		{
			return;
		}
		mCamera = camera.GetComponent(typeof(CarCamera)) as CarCamera;
		if (mCamera == null)
		{
			Debug.LogError("CarCamera component not found in Camera object.");
			return;
		}
		ParticleSystem componentInChildren = mCamera.GetComponentInChildren<ParticleSystem>();
		if (componentInChildren != null)
		{
			componentInChildren.gameObject.SetActive(value: false);
		}
	}

	public void PetReady(bool mount, PetSpecialSkillType type)
	{
		mIsReady = true;
	}

	protected void DistanceUpdate()
	{
		if (!_GameCompleted && mType == Type.USER)
		{
			mDistanceUpdateTimer += Time.deltaTime;
			if (!RacingManager.pIsSinglePlayer && !(MainStreetMMOClient.pInstance == null) && mDistanceUpdateTimer >= 1f)
			{
				mDistanceUpdateTimer = 0f;
				mDistanceForward = mLevelManager.GetRaceCompletedAmount(mAvatarTransform.position, mCurrentLap, mNextExpectedCheckPoint - 1);
				mRacingMMOClient.SendDistance(UserInfo.pInstance.UserID, mDistanceForward);
			}
		}
	}

	private void Restart()
	{
		mCurrentLap = 1;
		mPosition = 0;
		mNextExpectedCheckPoint = 0;
		mRaceTime = 0f;
		mLapTime = 0f;
		mBestLapTime = 0f;
		mLastLapTime = 0f;
		UpdateCheckpoints();
		HideMessage();
		if (!(mLevelManager == null) && !(mLevelManager.pUiInGame == null))
		{
			mLevelManager.pUiInGame.enabled = true;
			mLevelManager.pUiInGame.SetText(null, null, isAppend: false, visibility: false);
		}
	}

	private void UpdateCheckpoints()
	{
		if (mLevelManager == null || mLevelManager._RacingCheckpoints == null)
		{
			return;
		}
		if (mNextExpectedCheckPoint == 0 || mNextExpectedCheckPoint >= mLevelManager._RacingCheckpoints.Length)
		{
			if (mNextExpectedCheckPoint != 0)
			{
				LapCompleted();
			}
			mNextExpectedCheckPoint = 1;
		}
		else
		{
			mNextExpectedCheckPoint++;
		}
		for (int i = 0; i < mLevelManager._RacingCheckpoints.Length; i++)
		{
			mLevelManager._RacingCheckpoints[i].SetAsActiveCheckpoint(mLevelManager._RacingCheckpoints[i]._CheckpointID == mNextExpectedCheckPoint);
		}
	}

	public void OnCheckpointHit(RacingCheckpoint inCheckpoint)
	{
		if (AvAvatar.IsCurrentPlayer(base.gameObject))
		{
			if (mNextExpectedCheckPoint == inCheckpoint._CheckpointID)
			{
				inCheckpoint.CollectCheckpoint();
				UpdateCheckpoints();
			}
			else
			{
				Debug.Log("Hit wrong checkpoint");
			}
		}
	}

	public virtual void LapCompleted()
	{
		if (pGameCompleted || pGameMode == GameModes.COLLECTION_RACE || pType == Type.MMO)
		{
			return;
		}
		float num = 0f;
		if (mType == Type.USER)
		{
			num = mLapTime - mLastLapTime;
			SetBestLapTime(num);
		}
		UtDebug.Log("Lap completed = " + mCurrentLap + " : " + mLevelManager._NumLaps);
		mCurrentLap++;
		if (mCurrentLap > mLevelManager._NumLaps)
		{
			mCurrentLap = mLevelManager._NumLaps;
			OnGameComplete();
			if (pUiRacingMessage != null)
			{
				pUiRacingMessage.HideAll();
			}
			if (pUiRaceTrackHUD != null)
			{
				pUiRaceTrackHUD.SetUiVisibility(Visibility: false);
			}
		}
		else if (mCurrentLap == mLevelManager._NumLaps)
		{
			ShowRacingMessage(RacingMessage.FINAL_LAP);
			StartCoroutine("HideMessageAfterDelay", 2f);
		}
		if (pUiRacingMessage != null && mCurrentLap <= mLevelManager._NumLaps && !pGameCompleted)
		{
			pUiRacingMessage.SetLapTimeText(GameUtilities.FormatTime(num), visibility: true);
			pUiRacingMessage.FadeOutLapTextandTexture(1.5f, 2f);
		}
		if (pUiRaceTrackHUD != null && pGameMode != GameModes.TIMEATTACK)
		{
			pUiRaceTrackHUD.SetLap(mCurrentLap + " of " + mLevelManager._NumLaps);
		}
	}

	private void SetBestLapTime(float inCurrLapTime)
	{
		if (mBestLapTime == 0f)
		{
			mBestLapTime = mLapTime;
		}
		if (inCurrLapTime <= mBestLapTime)
		{
			mBestLapTime = inCurrLapTime;
		}
		UtDebug.LogWarning("Curr Lap Time:: " + inCurrLapTime + " ::Best Lap Time:: " + mBestLapTime + " ::Lap Time:: " + mLapTime + " ::Last Lap Time:: " + mLastLapTime);
		mLastLapTime = mLapTime;
	}

	public void ShowRacingMessage(RacingMessage msgType)
	{
		if (pUiRacingMessage == null)
		{
			return;
		}
		switch (msgType)
		{
		case RacingMessage.WRONG_WAY:
			if (!pUiRacingMessage.pBkgWrongWay.GetVisibility())
			{
				pUiRacingMessage.pBkgWrongWay.SetVisibility(inVisible: true);
			}
			break;
		case RacingMessage.FINAL_LAP:
			if (!pUiRacingMessage.pBkgFinalLap.GetVisibility())
			{
				pUiRacingMessage.pBkgFinalLap.SetVisibility(inVisible: true);
			}
			break;
		}
	}

	public void HideMessage()
	{
		if (!(pUiRacingMessage == null))
		{
			pUiRacingMessage.pBkgWrongWay.SetVisibility(inVisible: false);
			pUiRacingMessage.pBkgFinalLap.SetVisibility(inVisible: false);
		}
	}

	public IEnumerator HideMessageAfterDelay(float delay)
	{
		yield return new WaitForSeconds(delay);
		HideMessage();
	}

	private void DestroyMessageDB()
	{
		pUiRacingMessage.SetVisibility(inVisible: false);
		RsResourceManager.DestroyLoadScreen();
		RsResourceManager.LoadLevel("DragonRacingDO");
		if (mUiGenericDBRaceComplete != null)
		{
			UnityEngine.Object.DestroyImmediate(mUiGenericDBRaceComplete);
		}
	}

	public void OnGameComplete()
	{
		if (_GameCompleted)
		{
			return;
		}
		_GameCompleted = true;
		base.enabled = false;
		AvAvatar.pAvatarCam.SetActive(value: true);
		if (mType != 0 || pGameMode != 0)
		{
			return;
		}
		if (mPosition <= 3)
		{
			OnWin();
		}
		else
		{
			OnLose();
		}
		mLevelManager._UiRaceResults.SetXPAndCoins(mPosition + 1, mLevelManager);
		mLevelManager.GameComplete();
		if (!RacingManager.pIsSinglePlayer && MainStreetMMOClient.pIsMMOEnabled)
		{
			if (mPosition == 1)
			{
				AchievementTask achievementTask = new AchievementTask(mLevelManager._FirstPlaceAchievementID);
				UserAchievementTask.Set(achievementTask, UserProfile.pProfileData.GetGroupAchievement(mLevelManager._FirstPlaceClanAchievementID));
			}
			mRacingMMOClient.SendDistance(UserInfo.pInstance.UserID, mDistanceForward);
			mRacingMMOClient.SendResult(mLevelManager.pElapsedTime, mDistanceForward, pLapsCompleted);
		}
		else
		{
			AchievementTask achievementTask2 = new AchievementTask(mLevelManager._SinglePlayerAchievementTaskID);
			UserAchievementTask.Set(achievementTask2, UserProfile.pProfileData.GetGroupAchievement(mLevelManager._SinglePlayerClanAchievementTaskID));
			mLevelManager._UiRaceResults.Initialize(null, mLevelManager);
		}
		KAInput.ShowJoystick(JoyStickPos.BOTTOM_LEFT, inShow: false);
	}

	public void UpdateUI()
	{
		if (!_GameCompleted)
		{
			float time = 0f;
			if (pGameMode == GameModes.SINGLERACE || pGameMode == GameModes.GRAND_PRIX)
			{
				pRaceTime += Time.deltaTime;
				mLevelManager.pElapsedTime = pRaceTime;
				mLapTime += Time.deltaTime;
				time = mRaceTime;
			}
			if (pUiRaceTrackHUD != null)
			{
				pUiRaceTrackHUD.SetTime(GameUtilities.FormatTime(time));
			}
		}
	}

	public virtual void OnWin()
	{
		if (pType == Type.USER && mCamera != null)
		{
			mCamera.pState = CarCamera.State.ORBIT;
		}
	}

	public virtual void OnLose()
	{
		if (pType == Type.USER && mCamera != null)
		{
			mCamera.pState = CarCamera.State.ORBIT;
		}
	}

	public void OnDestroy()
	{
		if (AvAvatar.IsCurrentPlayer(base.gameObject))
		{
			SetRacePosition(0);
			AvAvatar.pLevelState = AvAvatarLevelState.NORMAL;
		}
		else
		{
			SetPositionSprite(0);
		}
		if (mMiniMapDisplay != null)
		{
			UnityEngine.Object.Destroy(mMiniMapDisplay);
		}
	}
}
