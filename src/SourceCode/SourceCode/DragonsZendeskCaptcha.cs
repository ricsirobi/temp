using System;
using UnityEngine;
using UnityEngine.U2D;
using UnityEngine.UI;

public class DragonsZendeskCaptcha : MonoBehaviour
{
	public Image _CaptchaImage;

	public SpriteAtlas _CaptchaAtlas;

	public InputField _CaptchaAnswerInput;

	public GameObject _CaptchaPanel;

	public GameObject _CaptchaError;

	private string mCaptchaAnswerString;

	private Sprite[] mCaptchas;

	private Sprite mPreviousCaptcha;

	private Action mCaptchaOpened;

	private Action mCaptchaClosed;

	private Action mCaptchaValidated;

	public void Init(Action onCaptchaSuccess, Action onCaptchaOpened, Action onCaptchaClosed)
	{
		if ((bool)_CaptchaAtlas)
		{
			mCaptchas = new Sprite[_CaptchaAtlas.spriteCount];
			_CaptchaAtlas.GetSprites(mCaptchas);
			mCaptchaOpened = onCaptchaOpened;
			mCaptchaClosed = onCaptchaClosed;
			mCaptchaValidated = onCaptchaSuccess;
		}
	}

	public void OpenCaptcha()
	{
		mCaptchaOpened?.Invoke();
		_CaptchaPanel.SetActive(value: true);
		GenerateCaptcha();
	}

	public void GenerateCaptcha()
	{
		_CaptchaError.SetActive(value: false);
		ClearInput();
		int num = UnityEngine.Random.Range(0, _CaptchaAtlas.spriteCount);
		Sprite sprite = mCaptchas[num];
		if (!mPreviousCaptcha)
		{
			mPreviousCaptcha = sprite;
		}
		else if (mPreviousCaptcha == sprite)
		{
			GenerateCaptcha();
			return;
		}
		mPreviousCaptcha = sprite;
		_CaptchaImage.sprite = sprite;
		mCaptchaAnswerString = sprite.name.Split('_')[1].Replace("(Clone)", "").Trim();
	}

	public void SubmitCaptcha()
	{
		if (!ValidateCaptcha())
		{
			_CaptchaError.SetActive(value: true);
			return;
		}
		_CaptchaError.SetActive(value: false);
		mCaptchaValidated?.Invoke();
	}

	private bool ValidateCaptcha()
	{
		return _CaptchaAnswerInput.text.Trim().ToLower() == mCaptchaAnswerString.Trim().ToLower();
	}

	public void CloseCaptcha()
	{
		_CaptchaPanel.SetActive(value: false);
		mCaptchaClosed?.Invoke();
	}

	public void ClearInput()
	{
		_CaptchaAnswerInput.text = string.Empty;
	}
}
