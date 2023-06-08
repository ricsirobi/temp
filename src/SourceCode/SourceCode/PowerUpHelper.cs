using KnowledgeAdventure.Multiplayer.Events;
using UnityEngine;

public class PowerUpHelper
{
	public delegate void ApplyExtraTimeHandler(bool extraTime, float duration);

	public delegate bool CanApplyEffectHandler(MMOMessageReceivedEventArgs args, string powerUpType);

	public delegate GameObject GetParentObjectHandler(MMOMessageReceivedEventArgs args);

	public delegate bool IsMMOUserHandler(MMOMessageReceivedEventArgs args);

	public delegate bool IsMMOGameHandler();

	public delegate void FireProjectileHandler();

	public delegate bool CanActivatePowerUpHandler(string powerUpType);

	public delegate void BombExplodeHandler(Vector3 bombPos, float radius);

	public ApplyExtraTimeHandler _ApplyExtraTime;

	public CanApplyEffectHandler _CanApplyEffect;

	public GetParentObjectHandler _GetParentObject;

	public IsMMOUserHandler _IsMMOUser;

	public IsMMOGameHandler _IsMMO;

	public FireProjectileHandler _FireProjectile;

	public CanActivatePowerUpHandler _CanActivatePowerUp;

	public BombExplodeHandler _OnBombExplode;
}
