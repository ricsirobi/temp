using UnityEngine;

public class EREelTargetable : ObTargetable
{
	public EREel _Eel;

	public override void OnDamage(int damage, bool isLocal, bool isCritical = false)
	{
		if (!_Active || !isLocal)
		{
			return;
		}
		_Health -= damage;
		if (_Health <= 0)
		{
			_Active = false;
			_Health = 0;
			if (AvAvatar.pToolbar != null)
			{
				AvAvatar.pToolbar.BroadcastMessage("HitObject", base.gameObject, SendMessageOptions.DontRequireReceiver);
			}
			_Eel.OnEelHit();
		}
	}
}
