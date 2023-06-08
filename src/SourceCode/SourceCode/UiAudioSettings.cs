using UnityEngine;

public class UiAudioSettings : KAUI
{
	public UISlider _SoundVolumeSlider;

	public UISlider _MusicVolumeSlider;

	public KAUI _AudioSettingsUI;

	private KAToggleButton mBtnMusicMute;

	private KAToggleButton mBtnSoundMute;

	private KAToggleButton mBtnAudioSettings;

	protected override void Start()
	{
		base.Start();
		mBtnAudioSettings = (KAToggleButton)FindItem("BtnAudioSettings");
		mBtnMusicMute = (KAToggleButton)FindItem("BtnMusicMute");
		mBtnMusicMute?.SetChecked(SnChannel.pTurnOffMusicGroup);
		mBtnSoundMute = (KAToggleButton)FindItem("BtnSoundMute");
		mBtnSoundMute?.SetChecked(SnChannel.pTurnOffSoundGroup);
		_SoundVolumeSlider.onDragFinished = OnSoundSliderChanged;
		_SoundVolumeSlider.value = Mathf.Pow(SnChannel.pSoundGroupVolume, 0.5f);
		_MusicVolumeSlider.onDragFinished = OnMusicSliderChanged;
		_MusicVolumeSlider.value = Mathf.Pow(SnChannel.pMusicGroupVolume, 0.5f);
	}

	private void OnSoundSliderChanged()
	{
		SnChannel.pSoundGroupVolume = Mathf.Pow(_SoundVolumeSlider.value, 2f);
		mBtnSoundMute?.SetChecked(SnChannel.pTurnOffSoundGroup);
	}

	private void OnMusicSliderChanged()
	{
		SnChannel.pMusicGroupVolume = Mathf.Pow(_MusicVolumeSlider.value, 2f);
		mBtnMusicMute?.SetChecked(SnChannel.pTurnOffMusicGroup);
	}

	public override void OnClick(KAWidget inWidget)
	{
		base.OnClick(inWidget);
		if (inWidget == mBtnMusicMute)
		{
			SnChannel.pTurnOffMusicGroup = !SnChannel.pTurnOffMusicGroup;
			mBtnMusicMute?.SetChecked(SnChannel.pTurnOffMusicGroup);
		}
		else if (inWidget == mBtnSoundMute)
		{
			SnChannel.pTurnOffSoundGroup = !SnChannel.pTurnOffSoundGroup;
			mBtnSoundMute?.SetChecked(SnChannel.pTurnOffSoundGroup);
		}
		else if (inWidget == mBtnAudioSettings && (bool)mBtnAudioSettings && (bool)_AudioSettingsUI)
		{
			_AudioSettingsUI.SetVisibility(!_AudioSettingsUI.GetVisibility());
		}
	}
}
