public class ObAnimSpeed : KAMonoBase
{
	public float _Speed = 1f;

	private void Start()
	{
		base.animation[base.animation.clip.name].speed = _Speed;
	}
}
