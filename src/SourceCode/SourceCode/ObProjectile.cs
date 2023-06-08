using UnityEngine;

public abstract class ObProjectile : KAMonoBase
{
	public Vector3 Velocity = Vector3.zero;

	public float Radius = 0.5f;

	public bool Alive = true;

	protected virtual void Update()
	{
		if (Velocity != Vector3.zero)
		{
			DoMove();
		}
	}

	private void DoMove()
	{
		if (!Alive)
		{
			return;
		}
		Vector3 vector = Velocity * Time.deltaTime;
		RaycastHit[] array = null;
		array = Physics.RaycastAll(base.transform.position, vector.normalized, vector.magnitude);
		if (array != null && array.Length != 0)
		{
			float num = 0f;
			int num2 = -1;
			for (int i = 0; i < array.Length; i++)
			{
				if ((!array[i].collider.isTrigger || array[i].collider.gameObject.GetComponent<ObTargetable>() != null) && (num2 == -1 || array[i].distance < num))
				{
					num = array[i].distance;
					num2 = i;
				}
			}
			if (num2 > -1 && OnCollision(array[num2]))
			{
				Alive = false;
				base.transform.position = array[num2].point;
			}
		}
		if (Alive)
		{
			base.transform.position = base.transform.position + vector;
		}
	}

	protected virtual bool OnCollision(RaycastHit hit)
	{
		return true;
	}
}
