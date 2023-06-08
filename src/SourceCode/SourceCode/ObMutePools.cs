using UnityEngine;

public class ObMutePools : MonoBehaviour
{
	public string[] _PoolNames;

	private void OnEnable()
	{
		if (_PoolNames != null)
		{
			for (int i = 0; i < _PoolNames.Length; i++)
			{
				SnChannel.MutePool(_PoolNames[i], mute: true);
			}
		}
	}

	private void OnDisable()
	{
		SnChannel.TurnOffPools(SnChannel.pTurnOffMusicGroup, PoolGroup.MUSIC);
		SnChannel.TurnOffPools(SnChannel.pTurnOffSoundGroup, PoolGroup.SOUND);
	}
}
