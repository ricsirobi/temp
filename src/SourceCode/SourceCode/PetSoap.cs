using UnityEngine;

public class PetSoap : MonoBehaviour
{
	public string _PetSoapStartAnim = "SpongeBath";

	public string _PetSoapEndAnim = "ShakeBathFur";

	public bool _IsPetBrushed;

	private bool mIsBathing;

	private float mBathTimer = 5f;

	private KAUIPetPlaySelect mPlayUI;

	private ParticleSystem mBathBubbleParticleSystem;

	public void Initialize(KAUIPetPlaySelect ui)
	{
		mPlayUI = ui;
		if (mPlayUI._BathBubbles != null)
		{
			mBathBubbleParticleSystem = mPlayUI._BathBubbles.GetComponent<ParticleSystem>();
		}
	}

	public void StartBrushing()
	{
		if (mIsBathing)
		{
			return;
		}
		mIsBathing = true;
		_IsPetBrushed = true;
		if (mPlayUI._BathBubbles != null)
		{
			mPlayUI._BathBubbles.transform.position = mPlayUI.pPet.transform.position;
			if (mBathBubbleParticleSystem != null)
			{
				mBathBubbleParticleSystem.Play();
			}
		}
		mPlayUI.pPet._CustomPettingAnim = _PetSoapStartAnim;
	}

	public void StopBrushing(bool remove)
	{
		if (remove && mBathBubbleParticleSystem != null)
		{
			mBathBubbleParticleSystem.Clear();
		}
		if (mIsBathing)
		{
			if (mBathBubbleParticleSystem != null)
			{
				mBathBubbleParticleSystem.Stop();
			}
			mIsBathing = false;
			mPlayUI.pPet.UpdateActionMeters(PetActions.BATH, 1f, doUpdateSkill: true);
			mPlayUI.pPet.PlayAnim(_PetSoapEndAnim);
			mPlayUI.pPet._CustomPettingAnim = null;
		}
	}

	private void Update()
	{
		if (mPlayUI == null)
		{
			return;
		}
		Vector3 vector = new Vector3(1f, 1f, 1f);
		vector = base.transform.position - mPlayUI.pPet.transform.position;
		vector.Normalize();
		vector = vector * 0.5f + mPlayUI.pPet.transform.position;
		if (mPlayUI._BathBubbles != null)
		{
			mPlayUI._BathBubbles.transform.position = vector;
		}
		if (mBathTimer > 0f)
		{
			mBathTimer -= Time.deltaTime;
			if (mBathTimer <= 4f && mBathBubbleParticleSystem != null)
			{
				mBathBubbleParticleSystem.Stop();
			}
			if (mBathTimer <= 0f)
			{
				mPlayUI.pPet.SetBrush(null);
				mPlayUI.DropObject(lookatcam: true);
				mPlayUI.pPet.SetState(Character_State.idle);
			}
		}
		if (Input.GetMouseButton(0))
		{
			mBathTimer = 5f;
		}
	}
}
