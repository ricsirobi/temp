using MK.Glow;
using UnityEngine;
using UnityEngine.UI;

public class UiGlowSettings : KAUI
{
	public GameObject _UiGroup;

	public Toggle _GlowPet;

	public Toggle _GlowMMOPet;

	public Toggle _EnableGlowCamera;

	public Dropdown _ColorSelection;

	private static UiGlowSettings mInstance;

	private MKGlow mMKGlow;

	private string mCurrentColor;

	protected override void Awake()
	{
		base.Awake();
		if (mInstance == null)
		{
			mInstance = this;
		}
		else
		{
			Object.Destroy(base.gameObject);
		}
	}

	protected override void Start()
	{
		base.Start();
		KAUI._GlobalExclusiveUI = this;
		KAUICursorManager.SetDefaultCursor("", showHideSystemCursor: false);
		mMKGlow = Camera.main.GetComponent<MKGlow>();
		if (mMKGlow == null)
		{
			mMKGlow = Camera.main.gameObject.AddComponent<MKGlow>();
			mMKGlow.GlowType = GlowType.Selective;
		}
		if (_ColorSelection != null)
		{
			mCurrentColor = _ColorSelection.options[0].text;
		}
		if (_GlowPet.isOn)
		{
			OnToggle(_GlowPet);
		}
		if (_GlowMMOPet.isOn)
		{
			OnToggle(_GlowMMOPet);
		}
		if (_EnableGlowCamera.isOn)
		{
			OnToggle(_EnableGlowCamera);
		}
		UpdateSettings();
	}

	protected override void OnDestroy()
	{
		KAUI._GlobalExclusiveUI = null;
		KAUICursorManager.SetDefaultCursor("Arrow");
		base.OnDestroy();
	}

	public void OnClick(Button button)
	{
		if (button.name == "ShowHide")
		{
			if (_UiGroup.activeInHierarchy)
			{
				_UiGroup.SetActive(value: false);
				KAUI._GlobalExclusiveUI = null;
				KAUICursorManager.SetDefaultCursor("Arrow");
			}
			else
			{
				_UiGroup.SetActive(value: true);
				KAUI._GlobalExclusiveUI = this;
				KAUICursorManager.SetDefaultCursor("", showHideSystemCursor: false);
			}
		}
		else if (button.name == "Close")
		{
			Object.Destroy(base.gameObject);
		}
	}

	public void OnColorChanged()
	{
		mCurrentColor = _ColorSelection.options[_ColorSelection.value].text;
		if (_GlowPet.isOn)
		{
			OnToggle(_GlowPet);
		}
		if (_GlowMMOPet.isOn)
		{
			OnToggle(_GlowMMOPet);
		}
	}

	public void OnToggle(Toggle toggleButton)
	{
		if (mMKGlow == null)
		{
			return;
		}
		if (toggleButton.name == "GlowPet")
		{
			if (toggleButton.isOn)
			{
				if (SanctuaryManager.pCurPetInstance != null && GlowManager.pInstance != null)
				{
					SanctuaryManager.pCurPetInstance.ApplyGlowEffect(mCurrentColor);
				}
			}
			else
			{
				SanctuaryManager.pCurPetInstance.RemoveGlowEffect();
			}
		}
		if (toggleButton.name == "GlowMMOPet")
		{
			SanctuaryPet[] array = Object.FindObjectsOfType<SanctuaryPet>();
			foreach (SanctuaryPet sanctuaryPet in array)
			{
				if (!(SanctuaryManager.pCurPetInstance != null) || !(SanctuaryManager.pCurPetInstance == sanctuaryPet))
				{
					if (toggleButton.isOn)
					{
						sanctuaryPet.ApplyGlowEffect(mCurrentColor);
					}
					else
					{
						sanctuaryPet.RemoveGlowEffect();
					}
				}
			}
		}
		else if (toggleButton.name == "GlowCamera")
		{
			mMKGlow.enabled = toggleButton.isOn;
		}
	}

	public void OnValueChanged(Slider slider)
	{
		if (!(mMKGlow == null))
		{
			if (slider.name == "Samples")
			{
				slider.GetComponentInChildren<Text>().text = slider.value.ToString() ?? "";
				mMKGlow.Samples = (int)slider.value;
			}
			else if (slider.name == "Iterations")
			{
				slider.GetComponentInChildren<Text>().text = slider.value.ToString() ?? "";
				mMKGlow.BlurIterations = (int)slider.value;
			}
			else if (slider.name == "Threshold")
			{
				slider.GetComponentInChildren<Text>().text = slider.value.ToString() ?? "";
				mMKGlow.Threshold = slider.value;
			}
			else if (slider.name == "InnerSpread")
			{
				slider.GetComponentInChildren<Text>().text = slider.value.ToString() ?? "";
				mMKGlow.BlurSpreadInner = slider.value;
			}
			else if (slider.name == "InnerIntensity")
			{
				slider.GetComponentInChildren<Text>().text = slider.value.ToString() ?? "";
				mMKGlow.GlowIntensityInner = slider.value;
			}
			else if (slider.name == "OuterSpread")
			{
				slider.GetComponentInChildren<Text>().text = slider.value.ToString() ?? "";
				mMKGlow.BlurSpreadOuter = slider.value;
			}
			else if (slider.name == "OuterIntensity")
			{
				slider.GetComponentInChildren<Text>().text = slider.value.ToString() ?? "";
				mMKGlow.GlowIntensityOuter = slider.value;
			}
			else if (slider.name == "LensIntensity")
			{
				slider.GetComponentInChildren<Text>().text = slider.value.ToString() ?? "";
				mMKGlow.LensIntensity = slider.value;
			}
		}
	}

	private void UpdateSettings()
	{
		if (mMKGlow == null)
		{
			return;
		}
		Slider[] componentsInChildren = GetComponentsInChildren<Slider>();
		foreach (Slider slider in componentsInChildren)
		{
			if (slider.name == "Samples")
			{
				slider.value = mMKGlow.Samples;
			}
			else if (slider.name == "Iterations")
			{
				slider.value = mMKGlow.BlurIterations;
			}
			else if (slider.name == "Threshold")
			{
				slider.value = mMKGlow.Threshold;
			}
			else if (slider.name == "InnerSpread")
			{
				slider.value = mMKGlow.BlurSpreadInner;
			}
			else if (slider.name == "InnerIntensity")
			{
				slider.value = mMKGlow.GlowIntensityInner;
			}
			else if (slider.name == "OuterSpread")
			{
				slider.value = mMKGlow.BlurSpreadOuter;
			}
			else if (slider.name == "OuterIntensity")
			{
				slider.value = mMKGlow.GlowIntensityOuter;
			}
			else if (slider.name == "LensIntensity")
			{
				slider.value = mMKGlow.LensIntensity;
			}
			OnValueChanged(slider);
		}
	}
}
