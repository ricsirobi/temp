public class NPCIdleManager : CoIdleManager
{
	public override void OnIdlePlay()
	{
		if (_VOs.Length != 0)
		{
			SnChannel.Play(_VOs[mNextPlayIdx], _Pool, _Priority, inForce: false, null);
			ResetIdles();
			mNextPlayIdx++;
			if (mNextPlayIdx == _VOs.Length)
			{
				mNextPlayIdx = 0;
			}
		}
	}
}
