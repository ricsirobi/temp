using UnityEngine;

namespace SquadTactics;

public class AnimationEventHandler : MonoBehaviour
{
	private Character mCharacter;

	private void Start()
	{
		mCharacter = base.gameObject.GetComponentInParent<Character>();
	}

	public void PlayFX(string name)
	{
		if (mCharacter != null)
		{
			mCharacter.PlayFX(name);
		}
	}

	public void ActivateVFX(string name)
	{
		if (mCharacter != null)
		{
			mCharacter.ActivateVFX(name);
		}
	}

	public void AbilityProc()
	{
	}
}
