using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CryptexGameManager : MonoBehaviour
{
	public float _TouchSensitivity;

	public float _TapSensitivity = 0.12f;

	public float _FlickSensitivity = 0.2f;

	public float _FlickDistance = 49f;

	private float mTouchSensitivityOnStart;

	private float mTapSensitivityOnStart;

	private float mFlickSensitivityOnStart;

	private float mFlickDistanceOnStart;

	public AudioClip _GameWonSound;

	[Tooltip("The positions that the dial is considered locked, these should line up with the runes on the dial.")]
	public float[] _LockInPositions;

	[Tooltip("The amount that the dial can be over or under the target LockInPosition and be considered locked in.")]
	public float _LockInTolerance = 10f;

	public CryptexDial[] _CryptexDials;

	public CryptexEndPiece _CryptexEndPiece;

	public Material _CryptexMaterial;

	[Tooltip("The start power of the center highlight glow.")]
	public float _CenterHighlightStartPower = 0.1f;

	[Tooltip("The end power of the center highlight glow.")]
	public float _CenterHighlightEndPower = 1f;

	[Tooltip("The speed at which the end piece highlight will fill/unfill.")]
	public float _EndPieceFillTimer = 1f;

	private int mLockedInDials;

	private int mPreviousLockedInDials;

	private float mCurrentFillAmount;

	private IEnumerator mEndPieceFillCoroutine;

	[Tooltip("Enter positions of runes here if you want to have a puzzle with these predefined answers.  There MUST be an equal number of answers to dials or else the puzzle will be random!")]
	public float[] _PredefinedAnswers;

	public float _EndGamePauseBeforeFX = 2f;

	public float _EndGameFXDuration = 3f;

	private List<KAWidget> mRuneWidgets = new List<KAWidget>();

	private List<Texture> mRuneTextures = new List<Texture>();

	private UiCryptex mUiCryptex;

	private Dictionary<float, Texture> mPositionToTextureDictionary = new Dictionary<float, Texture>();

	private ObClickableCryptex mClickableCryptex;

	public ObClickableCryptex pClickableCryptex
	{
		set
		{
			mClickableCryptex = value;
		}
	}

	private void Start()
	{
		mTouchSensitivityOnStart = TouchManager.pInstance._TouchSensitivity;
		mTapSensitivityOnStart = TouchManager.pInstance._TapSensitivity;
		mFlickSensitivityOnStart = TouchManager.pInstance._FlickSensitivity;
		mFlickDistanceOnStart = TouchManager.pInstance._FlickSensitivity;
		TouchManager.pInstance._TouchSensitivity = _TouchSensitivity;
		TouchManager.pInstance._TapSensitivity = _TapSensitivity;
		TouchManager.pInstance._FlickSensitivity = _FlickSensitivity;
		TouchManager.pInstance._FlickDistance = _FlickDistance;
		CryptexDial.OnDialMoveEvent = (CryptexDial.OnDialMove)Delegate.Combine(CryptexDial.OnDialMoveEvent, new CryptexDial.OnDialMove(OnDialMove));
		if (_CryptexMaterial != null)
		{
			_CryptexMaterial.SetFloat("_CenterHighlightPower", _CenterHighlightStartPower);
			_CryptexMaterial.SetFloat("_EndPieceFill", -0.05f);
			_CryptexMaterial.SetFloat("_EndPieceReady", -0.05f);
		}
	}

	public void InitializeRuneInfo(List<KAWidget> runeWidgets, List<Texture> runeTextures, UiCryptex uiCryptex)
	{
		mUiCryptex = uiCryptex;
		mRuneWidgets.AddRange(runeWidgets);
		mRuneTextures.AddRange(runeTextures);
		SetTextureDictionary();
		SetDialRune();
	}

	private void SetTextureDictionary()
	{
		for (int i = 0; i < 8; i++)
		{
			mPositionToTextureDictionary.Add(_LockInPositions[i], mRuneTextures[i]);
		}
	}

	private void SetDialRune()
	{
		for (int i = 0; i < _CryptexDials.Length; i++)
		{
			if (_PredefinedAnswers.Length == _CryptexDials.Length)
			{
				_CryptexDials[i].pLockInPosition = _PredefinedAnswers[i];
			}
			else
			{
				_CryptexDials[i].pLockInPosition = _LockInPositions[UnityEngine.Random.Range(0, _LockInPositions.Length)];
			}
			_CryptexDials[i].pLockInTolerance = _LockInTolerance;
			_CryptexDials[i].pLockInPositions.AddRange(_LockInPositions);
			KAWidget kAWidget = mRuneWidgets[UnityEngine.Random.Range(0, mRuneWidgets.Count)];
			_CryptexDials[i].pKAWidget = kAWidget;
			_CryptexDials[i].pKAWidget.SetTexture(mPositionToTextureDictionary[_CryptexDials[i].pLockInPosition]);
			mRuneWidgets.Remove(kAWidget);
		}
	}

	private void OnDialMove(CryptexDial cryptexDial)
	{
		if (cryptexDial.pKAWidget != null)
		{
			if (cryptexDial.pKAWidget.IsActive() && cryptexDial.pIsLockedIn)
			{
				cryptexDial.pKAWidget.SetDisabled(isDisabled: true);
			}
			else if (!cryptexDial.pKAWidget.IsActive() && !cryptexDial.pIsLockedIn)
			{
				cryptexDial.pKAWidget.SetDisabled(isDisabled: false);
			}
		}
		UpdateLockedInInfo();
	}

	private void UpdateLockedInInfo()
	{
		mLockedInDials = 0;
		for (int i = 0; i < _CryptexDials.Length; i++)
		{
			if (_CryptexDials[i].pIsLockedIn)
			{
				mLockedInDials++;
			}
		}
		if (mLockedInDials != mPreviousLockedInDials)
		{
			mPreviousLockedInDials = mLockedInDials;
			if (mLockedInDials == _CryptexDials.Length)
			{
				WonGame();
			}
			SetShaderHighlight(mLockedInDials);
			_CryptexEndPiece.UpdateLockedDials(mLockedInDials);
		}
	}

	private void SetShaderHighlight(int lockedInCount)
	{
		if (!(_CryptexMaterial == null))
		{
			float num = (float)lockedInCount / 5f;
			if (num == 0f)
			{
				num = -0.05f;
			}
			if (mEndPieceFillCoroutine != null)
			{
				StopCoroutine(mEndPieceFillCoroutine);
			}
			mEndPieceFillCoroutine = RunEndPieceFill(num);
			StartCoroutine(mEndPieceFillCoroutine);
			if (lockedInCount == _CryptexDials.Length)
			{
				_CryptexMaterial.SetFloat("_CenterHighlightPower", _CenterHighlightEndPower);
			}
			else
			{
				_CryptexMaterial.SetFloat("_CenterHighlightPower", _CenterHighlightStartPower);
			}
		}
	}

	private IEnumerator RunEndPieceFill(float fillAmount)
	{
		float lerpPercentage = 0f;
		float currentTimer = 0f;
		float currentFillAmount = mCurrentFillAmount;
		while (lerpPercentage <= 1f)
		{
			lerpPercentage = Mathf.Lerp(currentFillAmount, fillAmount, currentTimer / _EndPieceFillTimer);
			_CryptexMaterial.SetFloat("_EndPieceFill", lerpPercentage);
			currentTimer += Time.deltaTime;
			mCurrentFillAmount = lerpPercentage;
			yield return null;
		}
		mEndPieceFillCoroutine = null;
	}

	public void QuitGame()
	{
		_CryptexEndPiece.OnQuit();
		for (int i = 0; i < _CryptexDials.Length; i++)
		{
			_CryptexDials[i].OnQuit();
		}
		UnityEngine.Object.Destroy(base.gameObject);
	}

	private void WonGame()
	{
		mUiCryptex.SetVisibility(inVisible: false);
		_CryptexEndPiece.UpdateLockedDials(mLockedInDials, puzzleSolved: true);
		SetActive(active: false);
		if (_GameWonSound != null)
		{
			SnChannel.Play(_GameWonSound, "DialPool", inForce: true);
		}
		StartCoroutine(RunEndGameTheatrics());
	}

	private IEnumerator RunEndGameTheatrics()
	{
		yield return new WaitForSeconds(_EndGamePauseBeforeFX);
		if (_CryptexMaterial != null)
		{
			_CryptexMaterial.SetFloat("_EndPieceReady", 1f);
		}
		for (int i = 0; i < _CryptexDials.Length; i++)
		{
			_CryptexDials[i].ActivateFloat(activate: false);
		}
		yield return new WaitForSeconds(_EndGameFXDuration);
		PlayClickableCryptexObject();
		mUiCryptex.WonGame(solvedPuzzle: true);
	}

	public void SetActive(bool active)
	{
		for (int i = 0; i < _CryptexDials.Length; i++)
		{
			_CryptexDials[i].pIsActive = active;
		}
		_CryptexEndPiece.pIsActive = active;
		if (MissionManager.pInstance != null)
		{
			MissionManager.pInstance.SetTimedTaskUpdate(active, active);
		}
	}

	public void PlayClickableCryptexObject()
	{
		if (mClickableCryptex != null && mClickableCryptex._AnimatorToPlay != null && !string.IsNullOrEmpty(mClickableCryptex._MecanimParamater))
		{
			mClickableCryptex._AnimatorToPlay.SetTrigger(mClickableCryptex._MecanimParamater);
		}
	}

	private void OnDestroy()
	{
		TouchManager.pInstance._TouchSensitivity = mTouchSensitivityOnStart;
		TouchManager.pInstance._TapSensitivity = mTapSensitivityOnStart;
		TouchManager.pInstance._FlickSensitivity = mFlickSensitivityOnStart;
		TouchManager.pInstance._FlickDistance = mFlickDistanceOnStart;
	}
}
