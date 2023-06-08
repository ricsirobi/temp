using UnityEngine;

public class LevelUpFx : MonoBehaviour
{
	public AudioClip _Sound;

	public float _Duration = 3f;

	public float _BlinkTime = 0.2f;

	private bool mSoundPlayed;

	private bool mShouldPlaySound = true;

	private float mTimer;

	private bool mPlayer = true;

	private bool mShow = true;

	private float mBlinkTimer;

	private KAUI petHUD;

	private void Update()
	{
		if (mShouldPlaySound && !mSoundPlayed)
		{
			mSoundPlayed = true;
			if (_Sound != null)
			{
				SnChannel.Play(_Sound, "TELEPORT_FX_POOL", inForce: false);
			}
		}
		mTimer -= Time.deltaTime;
		if (_BlinkTime > 0f && mTimer > _BlinkTime && mBlinkTimer - mTimer > _BlinkTime)
		{
			mShow = !mShow;
			if (mPlayer && AvAvatar.pToolbar != null)
			{
				AvAvatar.pToolbar.SendMessage("ShowPlayerHUD", mShow, SendMessageOptions.DontRequireReceiver);
			}
			else if (null != petHUD)
			{
				petHUD.SetVisibility(mShow);
			}
			mBlinkTimer = mTimer;
		}
		if (mTimer <= 0f)
		{
			Done();
		}
	}

	public virtual void Done()
	{
		if (mPlayer && AvAvatar.pToolbar != null)
		{
			AvAvatar.pToolbar.SendMessage("ShowPlayerHUD", true, SendMessageOptions.DontRequireReceiver);
		}
		else if (null != petHUD)
		{
			petHUD.SetVisibility(inVisible: true);
		}
		base.gameObject.SetActive(value: false);
		Object.Destroy(base.gameObject);
	}

	public static void PlayPlayerLevelUp(UserRank newRank, bool inPlaySound, string inLevelupFx)
	{
		PlayAt(AvAvatar.position, inPlaySound, inLevelupFx);
	}

	public static void PlayDragonLevelUp(UserRank newRank, Vector3 inPosition, bool inPlaySound, string inLevelupFx)
	{
		PlayAt(inPosition, inPlaySound, inLevelupFx);
	}

	public static void PlayAt(Vector3 inPosition, bool inPlaySound, string inLevelupFx)
	{
		if (string.IsNullOrEmpty(inLevelupFx))
		{
			return;
		}
		GameObject gameObject = (GameObject)RsResourceManager.LoadAssetFromResources(inLevelupFx);
		if (gameObject == null)
		{
			return;
		}
		GameObject gameObject2 = Object.Instantiate(gameObject, inPosition, Quaternion.identity);
		if (!(gameObject2 != null))
		{
			return;
		}
		UtUtilities.ReAssignShader(gameObject2);
		LevelUpFx component = gameObject2.GetComponent<LevelUpFx>();
		if (component != null)
		{
			component.mShouldPlaySound = inPlaySound;
			component.mTimer = component._Duration;
			component.mBlinkTimer = component._Duration;
			if (inLevelupFx.Contains("Dragon"))
			{
				component.mPlayer = false;
			}
		}
	}
}
