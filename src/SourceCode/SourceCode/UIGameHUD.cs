using System.Collections.Generic;
using JSGames.Tween;
using JSGames.UI;
using UnityEngine;
using UnityEngine.EventSystems;

public class UIGameHUD : UI
{
	public UIGameHUDItem _HUDItems;

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

	private Vector2 mHealthScale = Vector2.zero;

	private bool mHealthUpdate;

	protected int mLives;

	private float mGameOverTimer;

	public AudioClip[] _HelpVO;

	public bool _PlayRandomHelpVO;

	public string _BackBtnMessage = "OnExit";

	public string _HelpBtnMessage = "OnHelp";

	public string _SpeakerBtnMessage = "OnSpeakerClick";

	public string _NextBtnMessage = "OnNextClick";

	protected override void OnClick(JSGames.UI.UIWidget item, PointerEventData eventData)
	{
		if (item == _BackButton)
		{
			pVisible = false;
			if ((bool)_EventObject)
			{
				_EventObject.SendMessage(_BackBtnMessage, SendMessageOptions.DontRequireReceiver);
			}
			if (_ExitLevel.Length > 0)
			{
				RsResourceManager.LoadLevel(_ExitLevel);
			}
		}
		else if (item == _HUDItems._HelpBtn)
		{
			if ((bool)_EventObject)
			{
				_EventObject.SendMessage(_HelpBtnMessage, SendMessageOptions.DontRequireReceiver);
			}
		}
		else if (item == _HUDItems._SpeakerBtn)
		{
			if ((bool)_EventObject)
			{
				_EventObject.SendMessage(_SpeakerBtnMessage, SendMessageOptions.DontRequireReceiver);
			}
		}
		else if (item == _HUDItems._NextBtn && (bool)_EventObject)
		{
			_EventObject.SendMessage(_NextBtnMessage, SendMessageOptions.DontRequireReceiver);
		}
		base.OnClick(item, eventData);
	}

	protected override void OnAnimEnd(JSGames.UI.UIWidget item, int aidx)
	{
		if (item == _HUDItems._CountDown)
		{
			_HUDItems._CountDown.pVisible = false;
			if (_EventObject != null)
			{
				_EventObject.SendMessage("OnStartGame", SendMessageOptions.DontRequireReceiver);
			}
			if (_BackButton != null)
			{
				_BackButton.pVisible = true;
			}
			if (_HUDItems._HelpBtn != null)
			{
				_HUDItems._HelpBtn.pVisible = true;
			}
		}
		base.OnAnimEnd(item, aidx);
	}

	public virtual void StartCountDown()
	{
		pVisible = true;
		if (_HUDItems._CountDown == null)
		{
			if (_EventObject != null)
			{
				_EventObject.SendMessage("OnStartGame", SendMessageOptions.DontRequireReceiver);
			}
			return;
		}
		_HUDItems._CountDown.pVisible = true;
		_HUDItems._CountDown.pAnim2D.Play("Play", 0);
		if (_CountDownSFX != null)
		{
			mCountDownTimer = _HUDItems._CountDown.pAnim2D.pCurrentAnimInfo._Length;
			mPreviousCountDownFrame = -1;
		}
		if (_BackButton != null)
		{
			_BackButton.pVisible = false;
		}
		if (_HUDItems._HelpBtn != null)
		{
			_HUDItems._HelpBtn.pVisible = false;
		}
		if (_HUDItems._FireBtn != null)
		{
			_HUDItems._FireBtn.pVisible = false;
		}
		_EventObject.SendMessage("OnStartCountDown", SendMessageOptions.DontRequireReceiver);
	}

	public virtual void SetScore(int score)
	{
		mScore = score;
		if (_HUDItems._ScoreItem != null)
		{
			if (_HUDItems._ScoreGroup != null)
			{
				_HUDItems._ScoreGroup.pVisible = true;
			}
			_HUDItems._ScoreItem.pText = score.ToString();
		}
	}

	public virtual void SetAccuracy(int accuracy)
	{
		if (_HUDItems._AccuracyItem != null && accuracy > 0)
		{
			_HUDItems._AccuracyItem.pText = accuracy.ToString();
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
		if (_HUDItems._TimerText != null)
		{
			if (_HUDItems._TimerGroup != null)
			{
				_HUDItems._TimerGroup.pVisible = true;
			}
			_HUDItems._TimerText.pText = GetTimerString((int)mTimer);
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
		Tween.Stop(_HUDItems._MessageText.gameObject);
		_HUDItems._MessageText.transform.localPosition = new Vector3(x, y, 1f);
	}

	public virtual void ShowTextMessage(string mtext)
	{
		if (_HUDItems._MessageText == null)
		{
			return;
		}
		Tween.Stop(_HUDItems._MessageText.gameObject);
		_HUDItems._MessageText.pVisible = true;
		_HUDItems._MessageText.pText = mtext;
		Color textMessageColor = _TextMessageColor;
		textMessageColor.a = 1f;
		if (_MessageFadeStartTime == 0f)
		{
			Color to = textMessageColor;
			to.a = 0f;
			TweenParam tweenParam = new TweenParam(_MessageFadeTime, 0f, 1, EaseType.Linear);
			Tween.ColorTo(_HUDItems._MessageText.gameObject, textMessageColor, to, tweenParam);
			if (_MessageMoveVector != Vector2.zero)
			{
				Vector2 vector = new Vector2(_HUDItems._MessageText.transform.localPosition.x, _HUDItems._MessageText.transform.localPosition.y);
				TweenParam tweenParam2 = new TweenParam(_MessageFadeTime, 0f, 1, EaseType.Linear);
				Tween.MoveLocalTo(_HUDItems._MessageText.gameObject, vector, vector + _MessageMoveVector, tweenParam2);
			}
		}
		else
		{
			TweenParam tweenParam3 = new TweenParam(_MessageFadeStartTime, 0f, 1, EaseType.Linear);
			Tween.ColorTo(_HUDItems._MessageText.gameObject, textMessageColor, textMessageColor, tweenParam3);
		}
	}

	public void StartPowerUp()
	{
		if (_HUDItems._PowerUpItem != null)
		{
			_HUDItems._PowerUpItem.pVisible = true;
			if (_HUDItems._PowerUpItem.pAnim2D != null)
			{
				_HUDItems._PowerUpItem.pAnim2D.Play("Still", -1);
			}
		}
	}

	public virtual void FlashPowerUp(string itemName)
	{
		if (_HUDItems._PowerUpItem != null && _HUDItems._PowerUpItem.pAnim2D != null)
		{
			_HUDItems._PowerUpItem.pAnim2D.Play("Flash", -1);
		}
	}

	public virtual void StopPowerUp(string itemName)
	{
		if (_HUDItems._PowerUpItem != null)
		{
			_HUDItems._PowerUpItem.pVisible = false;
			if (_HUDItems._PowerUpItem.pAnim2D != null)
			{
				_HUDItems._PowerUpItem.pAnim2D.Stop();
			}
		}
	}

	public virtual void DisplayHealth(bool damp)
	{
		if (_HUDItems._HealthItem != null)
		{
			if (damp && _HealthUpdatePerSec > 0f)
			{
				mHealthUpdate = true;
				return;
			}
			float num = 1f - (float)mHealthCount / (float)mMaxHealthCount;
			_HUDItems._HealthItem.transform.localScale = new Vector3(mHealthScale.x * num, mHealthScale.y, 1f);
		}
	}

	public void SetMaxHealth(int maxnum)
	{
		if (_HUDItems._HealthGroup != null)
		{
			_HUDItems._HealthGroup.pVisible = true;
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
		if (_HUDItems._GoalDescText != null)
		{
			if (_HUDItems._GoalDescGroup != null)
			{
				_HUDItems._GoalDescGroup.pVisible = true;
			}
			_HUDItems._GoalDescText.pText = gtext;
		}
	}

	public void SetGoalText(string gtext)
	{
		if (_HUDItems._GoalText != null)
		{
			if (_HUDItems._GoalGroup != null)
			{
				_HUDItems._GoalGroup.pVisible = true;
			}
			_HUDItems._GoalText.pText = gtext;
		}
	}

	public void SetGoal(int num)
	{
		mGoalCount = num;
		if (_HUDItems._GoalCountText != null)
		{
			if (_HUDItems._GoalTargetText != null)
			{
				_HUDItems._GoalTargetText.pText = mGoalCount.ToString();
			}
			if (_HUDItems._GoalGroup != null)
			{
				_HUDItems._GoalGroup.pVisible = true;
			}
			_HUDItems._GoalCountText.pText = mGoalCount.ToString();
		}
	}

	public void UpdateGoal(int deltaVal)
	{
		mGoalCount += deltaVal;
		if (mGoalCount <= 0)
		{
			mGoalCount = 0;
		}
		if (_HUDItems._GoalCountText != null)
		{
			_HUDItems._GoalCountText.pText = mGoalCount.ToString();
		}
		if (_EventObject != null)
		{
			_EventObject.SendMessage("OnGoalUpdate", mGoalCount, SendMessageOptions.DontRequireReceiver);
		}
	}

	public virtual void SetLives(int lives)
	{
		if (_HUDItems._LivesGroup != null)
		{
			_HUDItems._LivesGroup.pVisible = true;
		}
		mLives = lives;
		if (!(_HUDItems._LivesItem != null))
		{
			return;
		}
		int count = _HUDItems._LivesItem.pChildWidgets.Count;
		for (int i = 0; i < count; i++)
		{
			JSGames.UI.UIWidget uIWidget = _HUDItems._LivesItem.pChildWidgets[i];
			if (uIWidget != null)
			{
				uIWidget.pVisible = i < mLives;
			}
		}
	}

	public virtual void SetQuestion(Sprite sprite, string text)
	{
		if (_HUDItems._QuestionGroup != null)
		{
			_HUDItems._QuestionGroup.pVisible = true;
		}
		if (_HUDItems._QuestionIcon != null)
		{
			if (sprite != null)
			{
				_HUDItems._QuestionIcon.pVisible = true;
				_HUDItems._QuestionIcon.pSprite = sprite;
			}
			else
			{
				_HUDItems._QuestionIcon.pVisible = false;
			}
		}
		if (_HUDItems._QuestionText != null)
		{
			if (!string.IsNullOrEmpty(text))
			{
				_HUDItems._QuestionText.pVisible = true;
				_HUDItems._QuestionText.pText = text;
			}
			else
			{
				_HUDItems._QuestionText.pVisible = false;
			}
		}
	}

	public void ShowGameOver()
	{
		if (_BackButton != null)
		{
			_BackButton.pVisible = false;
		}
		if (_HUDItems._HelpBtn != null)
		{
			_HUDItems._HelpBtn.pVisible = false;
		}
		if (_HUDItems._FireBtn != null)
		{
			_HUDItems._FireBtn.pVisible = false;
		}
		_HUDItems._GameOver.pVisible = true;
		Vector2 from = new Vector2(_HUDItems._GameOver.transform.localScale.x, _HUDItems._GameOver.transform.localScale.y);
		TweenParam tweenParam = new TweenParam(_GameOverScaleTime, 0f, 1, pingPong: false, EaseType.Linear);
		Tween.ScaleTo(_HUDItems._GameOver.gameObject, from, Vector2.zero, tweenParam);
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
			if (_HUDItems._CountDown.pAnim2D.pSpriteIndex != mPreviousCountDownFrame)
			{
				mPreviousCountDownFrame = _HUDItems._CountDown.pAnim2D.pSpriteIndex;
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
			if (num != num2 && _HUDItems._TimerText != null)
			{
				_HUDItems._TimerText.pText = GetTimerString(num2);
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
			float num4 = _HUDItems._HealthItem.transform.localScale.x / mHealthScale.x;
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
			_HUDItems._HealthItem.transform.localScale = new Vector3(mHealthScale.x * num4, mHealthScale.y, 1f);
		}
		if (mGameOverTimer > 0f)
		{
			mGameOverTimer -= Time.deltaTime;
			if (mGameOverTimer <= 0f)
			{
				_HUDItems._GameOver.pVisible = false;
				pVisible = false;
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

	public void EndScaleTo(JSGames.UI.UIWidget item)
	{
		mGameOverTimer = _GameOverDelayTime;
	}

	public void PlayHelpVO()
	{
		if (_HelpVO.Length != 0)
		{
			if (_PlayRandomHelpVO)
			{
				int num = Random.Range(0, _HelpVO.Length);
				SnChannel.Play(_HelpVO[num], "VO_Pool", 0, inForce: true, base.gameObject);
			}
			else
			{
				SnChannel.Play(_HelpVO, "VO_Pool", 0, inForce: true, base.gameObject);
			}
		}
	}

	public void PlaySpeakerAnimation()
	{
		if (_HUDItems._SpeakerBtn != null)
		{
			if (_HUDItems._SpeakerBtn.pAnim2D != null)
			{
				_HUDItems._SpeakerBtn.pAnim2D.Play(_HUDItems._SpeakerBtn.pAnim2D._Anims[0]._Name);
			}
			_HUDItems._SpeakerBtn.pState = WidgetState.NOT_INTERACTIVE;
		}
	}

	public void StopSpeakerAnimation()
	{
		if (!(_HUDItems._SpeakerBtn != null))
		{
			return;
		}
		if (_HUDItems._SpeakerBtn.pAnim2D != null)
		{
			_HUDItems._SpeakerBtn.pAnim2D.Stop();
			if (_HUDItems._SpeakerBtn.pAnim2D.pCurrentAnimInfo != null)
			{
				_HUDItems._SpeakerBtn.pAnim2D.pCurrentAnimInfo.SetOriginalSprite();
			}
		}
		_HUDItems._SpeakerBtn.pState = WidgetState.INTERACTIVE;
	}

	private void OnSnEvent(SnEvent sndEvent)
	{
		if (sndEvent.mType == SnEventType.END_QUEUE || sndEvent.mType == SnEventType.STOP || sndEvent.mType == SnEventType.END)
		{
			StopSpeakerAnimation();
		}
	}

	protected override void OnHover(JSGames.UI.UIWidget widget, bool isHover, PointerEventData eventData)
	{
		base.OnHover(widget, isHover, eventData);
		if (_EventObject != null)
		{
			if (isHover)
			{
				_EventObject.SendMessage("ProcessHoverEnter", widget, SendMessageOptions.DontRequireReceiver);
			}
			else
			{
				_EventObject.SendMessage("ProcessHoverExit", widget, SendMessageOptions.DontRequireReceiver);
			}
		}
	}
}
