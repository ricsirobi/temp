using UnityEngine;
using UnityEngine.UI;

namespace JSGames.UI;

public class UIProgressBar : UIWidget
{
	public enum FillDirection
	{
		LeftToRight,
		RightToLeft,
		BottomToTop,
		TopToBottom
	}

	public Image _FillImage;

	public FillDirection _FillDirection;

	public float pProgress
	{
		get
		{
			if (_FillImage.type != Image.Type.Filled)
			{
				if (_FillDirection == FillDirection.LeftToRight || _FillDirection == FillDirection.RightToLeft)
				{
					return Mathf.Clamp(_FillImage.rectTransform.localScale.x, 0f, 1f);
				}
				return Mathf.Clamp(_FillImage.rectTransform.localScale.y, 0f, 1f);
			}
			return _FillImage.fillAmount;
		}
		set
		{
			if (_FillImage.type != Image.Type.Filled)
			{
				Vector3 localScale = _FillImage.rectTransform.localScale;
				if (_FillDirection == FillDirection.LeftToRight || _FillDirection == FillDirection.RightToLeft)
				{
					localScale.x = Mathf.Clamp(value, 0f, 1f);
				}
				else
				{
					localScale.y = Mathf.Clamp(value, 0f, 1f);
				}
				_FillImage.rectTransform.localScale = localScale;
			}
			else
			{
				_FillImage.fillAmount = value;
			}
		}
	}

	public override void Initialize(UI parentUI, UIWidget parentWidget)
	{
		base.Initialize(parentUI, parentWidget);
		if (_FillImage != null && _FillImage.type != Image.Type.Filled)
		{
			Vector2 pivot = _FillImage.rectTransform.pivot;
			Vector2 offsetMin = _FillImage.rectTransform.offsetMin;
			Vector2 offsetMax = _FillImage.rectTransform.offsetMax;
			if (_FillDirection == FillDirection.LeftToRight)
			{
				pivot.x = 0f;
			}
			else if (_FillDirection == FillDirection.RightToLeft)
			{
				pivot.x = 1f;
			}
			else if (_FillDirection == FillDirection.BottomToTop)
			{
				pivot.y = 0f;
			}
			else
			{
				pivot.y = 1f;
			}
			_FillImage.rectTransform.pivot = pivot;
			_FillImage.rectTransform.offsetMin = offsetMin;
			_FillImage.rectTransform.offsetMax = offsetMax;
		}
	}
}
