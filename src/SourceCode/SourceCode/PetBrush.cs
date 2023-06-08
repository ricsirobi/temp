using UnityEngine;

public class PetBrush : KAMonoBase
{
	public AudioClip _SFXBrush;

	public bool _IsPetBrushed;

	public float SFXEnergy = 300f;

	private KAUIPetPlaySelect mPlayUI;

	private bool mIsBrushing;

	private float mBrushTimer;

	private float mEnergy;

	private ParticleSystem mSoapParticleSystem;

	private void Start()
	{
		mBrushTimer = 10f;
	}

	public void Initialize(KAUIPetPlaySelect ui)
	{
		mPlayUI = ui;
		if (mPlayUI._SoapBubbles != null)
		{
			mSoapParticleSystem = mPlayUI._SoapBubbles.GetComponent<ParticleSystem>();
		}
	}

	public void PlaySound()
	{
		if (_SFXBrush != null && base.audio != null)
		{
			base.audio.clip = _SFXBrush;
			base.audio.loop = false;
			base.audio.Play();
		}
	}

	public void StartBrushing()
	{
		if (mIsBrushing)
		{
			return;
		}
		PlaySound();
		mIsBrushing = true;
		_IsPetBrushed = true;
		if (mPlayUI._SoapBubbles != null)
		{
			mPlayUI._SoapBubbles.transform.position = mPlayUI.pPet.transform.position;
			if (mSoapParticleSystem != null)
			{
				mSoapParticleSystem.Play();
			}
		}
	}

	public void StopBrushing(bool remove)
	{
		if (remove && mSoapParticleSystem != null)
		{
			mSoapParticleSystem.Clear();
		}
		if (mIsBrushing)
		{
			if (mSoapParticleSystem != null)
			{
				mSoapParticleSystem.Stop();
			}
			mIsBrushing = false;
			mPlayUI.pPet.PlayAnim("ShakeBathFur", 0, 1f, 1);
			mPlayUI.pPet.CheckForTaskCompletion(PetActions.BRUSH);
		}
	}

	private void Update()
	{
		if (mPlayUI == null)
		{
			return;
		}
		Vector3 vector = base.transform.position - mPlayUI.pPet.transform.position;
		vector.Normalize();
		vector = vector * 0.5f + mPlayUI.pPet.transform.position;
		if (mPlayUI._SoapBubbles != null)
		{
			mPlayUI._SoapBubbles.transform.position = vector;
		}
		Vector3 vector2 = new Vector3(1f, 1f, 1f);
		if (mPlayUI.pCurrentHomeIdx == 0)
		{
			if (mPlayUI.pOnGlass)
			{
				vector2.x = (vector2.y = (vector2.z = 0.6f));
			}
			else
			{
				vector2.x = (vector2.y = (vector2.z = 0.7f));
			}
		}
		else
		{
			vector2.x = (vector2.y = (vector2.z = 0.6f));
		}
		if (mBrushTimer > 0f)
		{
			mBrushTimer -= Time.deltaTime;
			if (mBrushTimer <= 4f && mSoapParticleSystem != null)
			{
				mSoapParticleSystem.Stop();
			}
			if (mBrushTimer <= 0f)
			{
				mPlayUI.pPet.SetBrush(null);
				mPlayUI.DropObject(lookatcam: true);
				mPlayUI.pPet.SetState(Character_State.idle);
			}
		}
		if (Input.GetMouseButton(0))
		{
			mBrushTimer = 5f;
		}
		if (mIsBrushing && mEnergy > SFXEnergy)
		{
			mEnergy = 0f;
			PlaySound();
		}
	}
}
