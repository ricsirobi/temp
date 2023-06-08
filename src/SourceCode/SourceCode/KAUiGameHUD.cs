using System.Collections.Generic;
using UnityEngine;

public class KAUiGameHUD : KAUI
{
	public KAUiGameHUDItem _ItemNames;

	public string _EventObjectName = "";

	public GameObject _EventObject;

	public AudioClip _CountDownSFX;

	public float _HealthUpdatePerSec;

	public Vector2 _MessageMoveVector = Vector2.zero;

	public float _MessageFadeStartTime = 5f;

	public float _MessageFadeTime = 2f;

	public string _ExitLevel = "";

	public float _TimerDirection = 1f;

	public Color _TextMessageColor = Color.white;

	public float _GameOverScaleTime = 1f;

	public float _GameOverDelayTime = 2f;

	public AudioClip _GameOverSound;

	private float mCountDownTimer;

	private int mPreviousCountDownFrame = -1;

	protected int mScore;

	protected bool mTimerOn;

	protected float mTimer;

	protected List<float> mTimeMarkers;

	private int mCurTimerMarkerIdx;

	protected int mGoalCount;

	protected int mHealthCount;

	protected int mMaxHealthCount;

	private Vector2 mHealthScale;

	private bool mHealthUpdate;

	protected int mLives;

	private float mGameOverTimer;

	protected KAWidget mBackBtn;

	protected KAWidget mHelpBtn;

	protected KAWidget mFireBtn;

	protected KAWidget mCountDown;

	protected KAWidget mMessageText;

	protected KAWidget mScoreGroup;

	protected KAWidget mScoreItem;

	protected KAWidget mAccuracyItem;

	protected KAWidget mTimerGroup;

	protected KAWidget mTimerText;

	protected KAWidget mGoalGroup;

	protected KAWidget mGoalText;

	protected KAWidget mGoalCountText;

	protected KAWidget mGoalTargetText;

	protected KAWidget mGoalDescGroup;

	protected KAWidget mGoalDescText;

	protected KAWidget mHealthGroup;

	protected KAWidget mHealthItem;

	protected KAWidget mLivesGroup;

	protected KAWidget mLivesItem;

	protected KAWidget mQuestionGroup;

	protected KAWidget mQuestionText;

	protected KAWidget mGameOver;

	protected override void Start()
	{
		base.Start();
		mBackBtn = FindItem(_ItemNames._BackBtn);
		mHelpBtn = FindItem(_ItemNames._HelpBtn);
		mFireBtn = FindItem("FireBtn");
		mCountDown = FindItem(_ItemNames._CountDown);
		mMessageText = FindItem(_ItemNames._MessageTxt);
		mScoreGroup = FindItem(_ItemNames._ScoreGroup);
		mScoreItem = FindItem(_ItemNames._ScoreTxt);
		mAccuracyItem = FindItem(_ItemNames._AccuracyTxt);
		mTimerGroup = FindItem(_ItemNames._TimerGroup);
		mTimerText = FindItem(_ItemNames._TimerTxt);
		mGoalGroup = FindItem(_ItemNames._GoalGroup);
		mGoalText = FindItem(_ItemNames._GoalTxt);
		mGoalCountText = FindItem(_ItemNames._GoalCountTxt);
		mGoalTargetText = FindItem(_ItemNames._GoalTargetTxt);
		mGoalDescGroup = FindItem(_ItemNames._GoalDescGroup);
		mGoalDescText = FindItem(_ItemNames._GoalDescTxt);
		mHealthGroup = FindItem(_ItemNames._HealthGroup);
		mHealthItem = FindItem(_ItemNames._HealthItem);
		if (mHealthItem != null)
		{
			mHealthScale = mHealthItem.GetScale();
		}
		mLivesGroup = FindItem(_ItemNames._LivesGroup);
		mLivesItem = FindItem(_ItemNames._LivesItem);
		mQuestionGroup = FindItem(_ItemNames._QuestionGroup);
		mQuestionText = FindItem(_ItemNames._QuestionTxt);
		mGameOver = FindItem(_ItemNames._GameOverItem);
	}

	public override void OnClick(KAWidget item)
	{
		if (item == mBackBtn)
		{
			SetVisibility(inVisible: false);
			if ((bool)_EventObject)
			{
				_EventObject.SendMessage("OnExit", SendMessageOptions.DontRequireReceiver);
			}
			if (_ExitLevel.Length > 0)
			{
				RsResourceManager.LoadLevel(_ExitLevel);
			}
		}
		else if (item == mHelpBtn && (bool)_EventObject)
		{
			_EventObject.SendMessage("OnHelp", SendMessageOptions.DontRequireReceiver);
		}
		base.OnClick(item);
	}

	public override void OnAnimEnd(KAWidget item, int aidx)
	{
		if (item == mCountDown)
		{
			mCountDown.SetVisibility(inVisible: false);
			if (_EventObject != null)
			{
				_EventObject.SendMessage("OnStartGame", SendMessageOptions.DontRequireReceiver);
			}
			if (mBackBtn != null)
			{
				mBackBtn.SetVisibility(inVisible: true);
			}
			if (mHelpBtn != null)
			{
				mHelpBtn.SetVisibility(inVisible: true);
			}
			if (mFireBtn != null && KAInput.pInstance.IsTouchInput())
			{
				mFireBtn.SetVisibility(inVisible: true);
			}
		}
		base.OnAnimEnd(item, aidx);
	}

	public virtual void StartCountDown()
	{
		SetVisibility(inVisible: true);
		if (mCountDown == null)
		{
			if (_EventObject != null)
			{
				_EventObject.SendMessage("OnStartGame", SendMessageOptions.DontRequireReceiver);
			}
			return;
		}
		mCountDown.SetVisibility(inVisible: true);
		mCountDown.PlayAnim("Play", 0);
		if (_CountDownSFX != null)
		{
			mCountDownTimer = mCountDown.GetCurrentAnimInfo()._Length;
			mPreviousCountDownFrame = -1;
		}
		if (mBackBtn != null)
		{
			mBackBtn.SetVisibility(inVisible: false);
		}
		if (mHelpBtn != null)
		{
			mHelpBtn.SetVisibility(inVisible: false);
		}
		if (mFireBtn != null)
		{
			mFireBtn.SetVisibility(inVisible: false);
		}
		_EventObject.SendMessage("OnStartCountDown", SendMessageOptions.DontRequireReceiver);
	}

	public virtual void SetScore(int score)
	{
		mScore = score;
		if (mScoreItem != null)
		{
			if (mScoreGroup != null)
			{
				mScoreGroup.SetVisibility(inVisible: true);
			}
			mScoreItem.SetText(score.ToString());
		}
	}

	public virtual void SetAccuracy(int accuracy)
	{
		if (mAccuracyItem != null && accuracy >= 0)
		{
			mAccuracyItem.SetText(accuracy.ToString());
		}
	}

	public int GetScore()
	{
		return mScore;
	}

	public virtual void AddScore(int ds)
	{
		mScore += ds;
		SetScore(mScore);
	}

	public void SetTimer(float secs)
	{
		mTimerOn = false;
		mTimer = secs;
		if (mTimerText != null)
		{
			if (mTimerGroup != null)
			{
				mTimerGroup.SetVisibility(inVisible: true);
			}
			mTimerText.SetText(GetTimerString((int)mTimer));
		}
	}

	public float GetTimer()
	{
		return mTimer;
	}

	public void StartTimer()
	{
		mTimerOn = true;
		mCurTimerMarkerIdx = 0;
	}

	public virtual void PauseTimer(bool t)
	{
		mTimerOn = !t;
	}

	public void AddTimeMarker(float sec)
	{
		if (mTimeMarkers == null)
		{
			mTimeMarkers = new List<float>();
		}
		int count = mTimeMarkers.Count;
		for (int i = 0; i < count; i++)
		{
			if (mTimeMarkers[i] < sec)
			{
				mTimeMarkers.Insert(i, sec);
				return;
			}
		}
		mTimeMarkers.Add(sec);
	}

	public void ClearTimeMarkers()
	{
		mTimeMarkers = null;
	}

	public virtual string GetTimerString(int tn)
	{
		int num = tn / 60;
		string text = (tn % 60).ToString();
		if (text.Length == 1)
		{
			text = "0" + text;
		}
		return num + ":" + text;
	}

	public virtual void SetTextMessageColor(Color c)
	{
		_TextMessageColor = c;
	}

	public virtual void SetTextMessagePosition(int x, int y)
	{
		mMessageText.StopMoveTo();
		mMessageText.SetPosition(x, y);
	}

	public virtual void ShowTextMessage(string mtext)
	{
		if (mMessageText == null)
		{
			return;
		}
		mMessageText.StopColorBlendTo();
		mMessageText.SetVisibility(inVisible: true);
		mMessageText.SetText(mtext);
		Color textMessageColor = _TextMessageColor;
		textMessageColor.a = 1f;
		if (_MessageFadeStartTime == 0f)
		{
			Color end = textMessageColor;
			end.a = 0f;
			mMessageText.ColorBlendTo(textMessageColor, end, _MessageFadeTime);
			if (_MessageMoveVector != Vector2.zero)
			{
				Vector2 vector = mMessageText.GetPosition();
				mMessageText.MoveTo(vector + _MessageMoveVector, _MessageFadeTime);
			}
		}
		else
		{
			mMessageText.ColorBlendTo(textMessageColor, textMessageColor, _MessageFadeStartTime);
		}
	}

	public void StartPowerUp(string itemName)
	{
		KAWidget kAWidget = FindItem(itemName);
		if (kAWidget != null)
		{
			kAWidget.SetVisibility(inVisible: true);
			kAWidget.PlayAnim("Still", -1);
		}
	}

	public virtual void FlashPowerUp(string itemName)
	{
		KAWidget kAWidget = FindItem(itemName);
		if (kAWidget != null)
		{
			kAWidget.PlayAnim("Flash", -1);
		}
	}

	public virtual void StopPowerUp(string itemName)
	{
		KAWidget kAWidget = FindItem(itemName);
		if (kAWidget != null)
		{
			kAWidget.SetVisibility(inVisible: false);
			kAWidget.StopAnim();
		}
	}

	public virtual void DisplayHealth(bool damp)
	{
		if (mHealthItem != null)
		{
			if (damp && _HealthUpdatePerSec > 0f)
			{
				mHealthUpdate = true;
				return;
			}
			float num = 1f - (float)mHealthCount / (float)mMaxHealthCount;
			mHealthItem.SetScale(new Vector3(mHealthScale.x * num, mHealthScale.y, 1f));
		}
	}

	public void SetMaxHealth(int maxnum)
	{
		if (mHealthGroup != null)
		{
			mHealthGroup.SetVisibility(inVisible: true);
		}
		mMaxHealthCount = maxnum;
		ResetHealth();
	}

	public void SetHealth(int hc)
	{
		mHealthCount = hc;
		DisplayHealth(damp: false);
	}

	public void ResetHealth()
	{
		mHealthCount = mMaxHealthCount;
		DisplayHealth(damp: false);
	}

	public void UpdateHealth(int deltaVal)
	{
		mHealthCount += deltaVal;
		if (mHealthCount > mMaxHealthCount)
		{
			mHealthCount = mMaxHealthCount;
		}
		else
		{
			if (mHealthCount <= 0)
			{
				mHealthCount = 0;
			}
			if (_EventObject != null)
			{
				_EventObject.SendMessage("OnHealthUpdate", mHealthCount, SendMessageOptions.DontRequireReceiver);
			}
		}
		DisplayHealth(damp: true);
	}

	public void SetGoalDesc(string gtext)
	{
		if (mGoalDescText != null)
		{
			if (mGoalDescGroup != null)
			{
				mGoalDescGroup.SetVisibility(inVisible: true);
			}
			mGoalDescText.SetText(gtext);
		}
	}

	public void SetGoalText(string gtext)
	{
		if (mGoalText != null)
		{
			if (mGoalGroup != null)
			{
				mGoalGroup.SetVisibility(inVisible: true);
			}
			mGoalText.SetText(gtext);
		}
	}

	public void SetGoal(int num)
	{
		mGoalCount = num;
		if (mGoalCountText != null)
		{
			if (mGoalTargetText != null)
			{
				mGoalTargetText.SetText(mGoalCount.ToString());
			}
			if (mGoalGroup != null)
			{
				mGoalGroup.SetVisibility(inVisible: true);
			}
			mGoalCountText.SetText(mGoalCount.ToString());
		}
	}

	public void UpdateGoal(int deltaVal)
	{
		mGoalCount += deltaVal;
		if (mGoalCount <= 0)
		{
			mGoalCount = 0;
		}
		if (mGoalCountText != null)
		{
			mGoalCountText.SetText(mGoalCount.ToString());
		}
		if (_EventObject != null)
		{
			_EventObject.SendMessage("OnGoalUpdate", mGoalCount, SendMessageOptions.DontRequireReceiver);
		}
	}

	public void SetLives(int lives)
	{
		if (mLivesGroup != null)
		{
			mLivesGroup.SetVisibility(inVisible: true);
		}
		mLives = lives;
		if (!(mLivesItem != null))
		{
			return;
		}
		int numChildren = mLivesItem.GetNumChildren();
		for (int i = 0; i < numChildren; i++)
		{
			KAWidget kAWidget = mLivesItem.FindChildItemAt(i);
			if (kAWidget != null)
			{
				kAWidget.SetVisibility(i < mLives);
			}
		}
	}

	public virtual void SetQuestion(string question)
	{
		if (mQuestionGroup != null)
		{
			mQuestionGroup.SetVisibility(inVisible: true);
		}
		if (mQuestionText != null)
		{
			mQuestionText.SetText(question);
		}
	}

	public void ShowGameOver()
	{
		if (mBackBtn != null)
		{
			mBackBtn.SetVisibility(inVisible: false);
		}
		if (mHelpBtn != null)
		{
			mHelpBtn.SetVisibility(inVisible: false);
		}
		if (mFireBtn != null)
		{
			mFireBtn.SetVisibility(inVisible: false);
		}
		mGameOver.SetVisibility(inVisible: true);
		mGameOver.ScaleTo(Vector3.zero, mGameOver.GetScale(), _GameOverScaleTime);
		if (_GameOverSound != null)
		{
			SnChannel.Play(_GameOverSound, inForce: true);
		}
	}

	protected override void Update()
	{
		base.Update();
		if (_EventObject == null && _EventObjectName.Length > 0)
		{
			_EventObject = GameObject.Find(_EventObjectName);
			if (_EventObject == null)
			{
				UtDebug.LogError("Message object " + _EventObjectName + " not found.");
				_EventObjectName = "";
				return;
			}
		}
		if (mCountDownTimer > 0f)
		{
			mCountDownTimer -= Time.deltaTime;
			if (mCountDown.pCurFrame != mPreviousCountDownFrame)
			{
				mPreviousCountDownFrame = mCountDown.pCurFrame;
				SnChannel.Play(_CountDownSFX, "SFX_Pool", 0, inForce: true, null);
			}
		}
		if (mTimerOn)
		{
			int num = (int)mTimer;
			mTimer += _TimerDirection * Time.deltaTime;
			int num2 = (int)mTimer;
			if (mTimer <= 0f)
			{
				if (_EventObject != null)
				{
					_EventObject.SendMessage("OnTimeUp", SendMessageOptions.DontRequireReceiver);
				}
				mTimer = 0f;
				mTimerOn = false;
			}
			if (num != num2 && mTimerText != null)
			{
				mTimerText.SetText(GetTimerString(num2));
			}
			if (_EventObject != null && mTimeMarkers != null)
			{
				while (mCurTimerMarkerIdx < mTimeMarkers.Count && mTimer < mTimeMarkers[mCurTimerMarkerIdx])
				{
					_EventObject.SendMessage("OnTimeMarker", mTimeMarkers[mCurTimerMarkerIdx], SendMessageOptions.DontRequireReceiver);
					mCurTimerMarkerIdx++;
				}
			}
		}
		if (mHealthUpdate)
		{
			float num3 = 1f - (float)mHealthCount / (float)mMaxHealthCount;
			float num4 = mHealthItem.GetScale().x / mHealthScale.x;
			float num5 = _HealthUpdatePerSec / 100f * Time.deltaTime;
			if (num3 > num4)
			{
				num4 += num5;
				if (num4 > num3)
				{
					num4 = num3;
					mHealthUpdate = false;
				}
			}
			else
			{
				num4 -= num5;
				if (num4 < num3)
				{
					num4 = num3;
					mHealthUpdate = false;
				}
			}
			mHealthItem.SetScale(new Vector3(mHealthScale.x * num4, mHealthScale.y, 1f));
		}
		if (mGameOverTimer > 0f)
		{
			mGameOverTimer -= Time.deltaTime;
			if (mGameOverTimer <= 0f)
			{
				mGameOver.SetVisibility(inVisible: false);
				SetVisibility(inVisible: false);
				_EventObject.SendMessage("OnGameOver");
			}
		}
	}

	public void Collect(GameObject collectObj)
	{
		if (_EventObject != null)
		{
			_EventObject.SendMessage("OnCollect", collectObj, SendMessageOptions.DontRequireReceiver);
		}
	}

	public override void EndScaleTo(KAWidget item)
	{
		mGameOverTimer = _GameOverDelayTime;
	}
}
