using UnityEngine;

public class ObUDTScoreBoard : MonoBehaviour
{
	public TextMesh _TopScoreTextScore;

	public TextMesh _TopScoreText;

	public TextMesh _TopScoreTitle;

	public float _ScoreShowDuration = 300f;

	public UDTTopScoreItem[] _TopScoreInfo;

	public LocaleString _DefaultTitleText;

	public LocaleString _DefaultText;

	private int mCurTopScoreIdx = -1;

	private float mHideScoreTimer;

	public float _UpdateHSInterval = 60f;

	private float mShowHSTimer = 0.01f;

	public LocaleString _ScoreText;

	public LocaleString _NameText;

	private void Start()
	{
		if (_DefaultTitleText._Text == "")
		{
			_DefaultTitleText._Text = _TopScoreTitle.text;
		}
		_DefaultTitleText._Text = StringTable.GetStringData(_DefaultTitleText._ID, _DefaultTitleText._Text);
		_TopScoreTitle.text = _DefaultTitleText._Text;
		if (_DefaultText._Text == "")
		{
			_DefaultText._Text = _TopScoreText.text;
		}
		_DefaultText._Text = StringTable.GetStringData(_DefaultText._ID, _DefaultText._Text);
		_TopScoreText.text = _DefaultText._Text;
	}

	private void ShowTopScore(bool t)
	{
		if (t)
		{
			if (_TopScoreInfo[mCurTopScoreIdx].mScores != null)
			{
				string text = StringTable.GetStringData(_ScoreText._ID, _ScoreText._Text) + "\n";
				string text2 = "";
				text2 = text2 + "\t" + StringTable.GetStringData(_NameText._ID, _NameText._Text) + "\n";
				if (_TopScoreInfo[mCurTopScoreIdx].mScores.Length == 0)
				{
					text2 = "";
					text = "";
				}
				else
				{
					for (int i = 0; i < _TopScoreInfo[mCurTopScoreIdx].mScores.Length; i++)
					{
						text2 = text2 + (i + 1) + "\t" + _TopScoreInfo[mCurTopScoreIdx].mScores[i]._Name + "\n";
						text = text + _TopScoreInfo[mCurTopScoreIdx].mScores[i]._Score + "\n";
					}
				}
				_TopScoreInfo[mCurTopScoreIdx].CheckTitle();
				_TopScoreTitle.text = _TopScoreInfo[mCurTopScoreIdx]._Title;
				_TopScoreText.text = text2;
				_TopScoreTextScore.text = text;
			}
			else
			{
				_TopScoreInfo[mCurTopScoreIdx].CheckTitle();
				_TopScoreTitle.text = _TopScoreInfo[mCurTopScoreIdx]._Title;
				_TopScoreText.text = "";
				_TopScoreTextScore.text = "";
			}
		}
		else
		{
			_TopScoreTitle.text = _DefaultTitleText._Text;
			_TopScoreText.text = _DefaultText._Text;
			_TopScoreTextScore.text = "";
		}
	}

	public void GetTopScoreEventHandler(WsServiceType inType, WsServiceEvent inEvent, float inProgress, object inObject, object inUserData)
	{
		int num = (int)inUserData;
		if (inObject == null)
		{
			_TopScoreInfo[num].mScores = new UDTTopScore[0];
			return;
		}
		UserAchievementInfoResponse userAchievementInfoResponse = (UserAchievementInfoResponse)inObject;
		if (userAchievementInfoResponse != null && userAchievementInfoResponse.AchievementInfo != null)
		{
			_TopScoreInfo[num].mScores = new UDTTopScore[userAchievementInfoResponse.AchievementInfo.Length];
			int num2 = 0;
			UserAchievementInfo[] achievementInfo = userAchievementInfoResponse.AchievementInfo;
			foreach (UserAchievementInfo userAchievementInfo in achievementInfo)
			{
				_TopScoreInfo[num].mScores[num2] = new UDTTopScore();
				_TopScoreInfo[num].mScores[num2]._Name = userAchievementInfo.UserName;
				_TopScoreInfo[num].mScores[num2]._Score = userAchievementInfo.AchievementPointTotal.Value;
				num2++;
			}
		}
		if (num == mCurTopScoreIdx)
		{
			ShowTopScore(t: true);
		}
		else if (mCurTopScoreIdx == num - 1)
		{
			mShowHSTimer = 0.01f;
		}
	}

	private void Update()
	{
		if (UserInfo.pIsReady)
		{
			int num = 0;
			bool flag = true;
			UDTTopScoreItem[] topScoreInfo = _TopScoreInfo;
			foreach (UDTTopScoreItem uDTTopScoreItem in topScoreInfo)
			{
				if (uDTTopScoreItem.mUpdateTimer > 0f)
				{
					uDTTopScoreItem.mUpdateTimer -= Time.deltaTime;
					if (uDTTopScoreItem.mUpdateTimer <= 0f)
					{
						uDTTopScoreItem.mUpdateTimer = 0f;
						UpdateTopScores(uDTTopScoreItem, num);
						if (flag)
						{
							flag = false;
							mCurTopScoreIdx = num - 1;
						}
					}
				}
				num++;
			}
		}
		if (mShowHSTimer > 0f)
		{
			mShowHSTimer -= Time.deltaTime;
			if (mShowHSTimer <= 0f)
			{
				mShowHSTimer = _UpdateHSInterval;
				mCurTopScoreIdx++;
				if (mCurTopScoreIdx == _TopScoreInfo.Length)
				{
					mCurTopScoreIdx = 0;
				}
				ShowTopScore(t: true);
				mHideScoreTimer = _ScoreShowDuration;
				if (_TopScoreInfo[mCurTopScoreIdx].mScores == null || _TopScoreInfo[mCurTopScoreIdx]._Refresh)
				{
					UpdateTopScores(_TopScoreInfo[mCurTopScoreIdx], mCurTopScoreIdx);
				}
			}
		}
		if (mHideScoreTimer > 0f)
		{
			mHideScoreTimer -= Time.deltaTime;
			if (mHideScoreTimer <= 0f)
			{
				mHideScoreTimer = 0f;
				ShowTopScore(t: false);
			}
		}
	}

	private void UpdateTopScores(UDTTopScoreItem ScoreItem, int index)
	{
		WsWebService.GetTopAchievementPointUsers(UserInfo.pInstance.UserID, 1, ScoreItem._Count, ScoreItem._PageType, ScoreItem._ModeType, 12, null, GetTopScoreEventHandler, index);
	}
}
