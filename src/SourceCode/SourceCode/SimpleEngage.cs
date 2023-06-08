using UnityEngine;

public class SimpleEngage : MonoBehaviour
{
	public AudioClip[] _VOs;

	public int _CurIdx;

	private bool mCameraSwitched;

	public string _CharacterName = "PfFrankie";

	public bool _UnpauseAvatarAfterVO;

	private SimpleNPC mCharacter;

	public void Start()
	{
		base.gameObject.SetActive(value: false);
		GameObject gameObject = GameObject.Find(_CharacterName);
		if (gameObject != null)
		{
			mCharacter = (SimpleNPC)gameObject.GetComponent("SimpleNPC");
			mCharacter._Camera = this;
		}
	}

	public void EndVO()
	{
		mCameraSwitched = false;
		AvAvatar.SetActive(inActive: true);
		if (_UnpauseAvatarAfterVO)
		{
			AvAvatar.pState = AvAvatarState.IDLE;
		}
		base.gameObject.SetActive(value: false);
		mCharacter.SetState(Character_State.idle);
		if (mCharacter._PlayAllCurIndex >= 0)
		{
			mCharacter._PlayAllCurIndex = -1;
			_CurIdx = 0;
		}
	}

	private void Update()
	{
		if (!mCameraSwitched)
		{
			mCameraSwitched = true;
			AvAvatar.SetActive(inActive: false);
			mCharacter.PlayVO(_VOs[_CurIdx]);
			_CurIdx++;
			if (_CurIdx == _VOs.Length)
			{
				_CurIdx = 0;
			}
		}
		if (Input.GetKeyUp(KeyCode.Space) && !mCharacter._WaitForDisplayMessage)
		{
			EndVO();
			SnChannel.StopPool("VO_Pool");
		}
	}
}
