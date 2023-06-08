using System;

[Serializable]
public class EnEnemyStateDescription
{
	public EnEnemyStateProperties _Idle;

	public EnEnemyStateProperties _Walk;

	public EnEnemyStateProperties _Alert;

	public EnEnemyStateProperties _Attack;

	public EnEnemyStateProperties _Tired;

	public EnEnemyStateProperties _Stunned;

	public EnEnemyStateProperties _TakeHit;

	public EnEnemyStateProperties _Death;

	public EnEnemyStateProperties _Load;

	public EnEnemyStateProperties _Shoot;

	public EnEnemyStateProperties _ShootIdle;

	public EnEnemyStateProperties _Celebrate;

	public EnEnemyStateProperties _BlockShot;
}
