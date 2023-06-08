public class KAButton : KAWidget
{
	public KASkinInfo _PressInfo = new KASkinInfo();

	public KASkinInfo _ClickInfo = new KASkinInfo();

	protected override void Update()
	{
		base.Update();
		_ClickInfo.Update();
		_PressInfo.Update();
	}

	public override void OnPress(bool inPressed)
	{
		base.OnPress(inPressed);
		if (IsActive())
		{
			if (_HoverInfo != null)
			{
				_HoverInfo.PlayParticle(isPlay: false);
			}
			_PressInfo.DoEffect(inPressed, this);
			if (inPressed)
			{
				PlayAnim("Press");
			}
			else
			{
				PlayAnim("Normal");
			}
		}
	}

	public override void OnHover(bool inIsHover)
	{
		if (GetState() == KAUIState.INTERACTIVE)
		{
			base.OnHover(inIsHover);
		}
	}

	public override void OnClick()
	{
		base.OnClick();
		if (IsActive())
		{
			_ClickInfo.DoEffect(inShowEffect: true, this);
		}
	}

	public override void OnTooltip(bool inShow)
	{
		if (IsActive())
		{
			KATooltip.Show(this, inShow, base.gameObject.layer, -50f);
		}
	}

	protected override void ResetEffects()
	{
		base.ResetEffects();
		if (_PressInfo != null && _PressInfo.pIsEffectOn)
		{
			_PressInfo.DoEffect(inShowEffect: false, this);
		}
		if (_ClickInfo != null && _ClickInfo.pIsEffectOn)
		{
			_ClickInfo.DoEffect(inShowEffect: false, this);
		}
	}

	public virtual void SetSpriteOnHover(string inSpriteName)
	{
		_HoverInfo._SpriteInfo._Sprites[0]._SpriteName = inSpriteName;
	}

	public virtual void SetSpriteOnPress(string inSpriteName)
	{
		_PressInfo._SpriteInfo._Sprites[0]._SpriteName = inSpriteName;
	}

	public virtual void SetSpriteOnDisabled(string inSpriteName)
	{
		_DisabledInfo._SpriteInfo._Sprites[0]._SpriteName = inSpriteName;
	}

	public virtual void SetSpriteOnEnabled(string inSpriteName)
	{
		SetSprite(inSpriteName);
	}

	public override void SetState(KAUIState inState)
	{
		if (inState == KAUIState.DISABLED)
		{
			PlayAnim("Normal");
		}
		base.SetState(inState);
	}

	public override void SetVisibility(bool inVisible)
	{
		base.SetVisibility(inVisible);
		if (!inVisible)
		{
			KATooltip.Show(this, inShow: false, -1, -1f);
		}
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		KATooltip.Show(this, inShow: false, -1, -1f);
	}
}
