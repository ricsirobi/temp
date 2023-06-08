using UnityEngine;

public class CharacterEngage : MonoBehaviour
{
	private bool mCameraSwitched;

	public string _CharacterName = "PfDyan";

	private ChCharacter mCharacter;

	private void Start()
	{
		mCharacter = (ChCharacter)GameObject.Find(_CharacterName).GetComponent("ChCharacter");
	}

	public void EndVO()
	{
		if (mCameraSwitched)
		{
			mCameraSwitched = false;
			AvAvatar.SetActive(inActive: true);
			base.gameObject.SetActive(value: false);
			mCharacter.SetState(Character_State.idle);
		}
	}

	private void Update()
	{
		if (!mCameraSwitched)
		{
			mCameraSwitched = true;
			AvAvatar.SetActive(inActive: false);
			mCharacter.SendMessage("DoEngage", this);
		}
		if (Input.GetKeyUp(KeyCode.Space))
		{
			SnChannel.StopPool("VO_Pool");
			EndVO();
		}
	}
}
