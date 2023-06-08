using UnityEngine;

public class ObSpringBoard : KAMonoBase
{
	public Transform _Target;

	public float _Speed;

	public AudioClip _Sound;

	public bool _PetAllowed = true;

	private void Update()
	{
	}

	private void OnTriggerStay(Collider collision)
	{
		if (AvAvatar.IsCurrentPlayer(collision.gameObject) && _Target != null && AvAvatar.pState != AvAvatarState.SPRINGBOARD && ((AvAvatarController)AvAvatar.pObject.GetComponent("AvAvatarController")).mSpline == null)
		{
			Use(AvAvatar.pObject);
			AvAvatar.pInputEnabled = false;
			AvAvatar.SetUIActive(inActive: false);
			if (base.animation != null)
			{
				base.animation.Play("Go");
			}
			if ((bool)_Sound)
			{
				SnChannel.Play(_Sound, "SFX_Pool", inForce: false);
			}
			if (MainStreetMMOClient.pInstance != null)
			{
				MainStreetMMOClient.pInstance.SendSpringboard(base.gameObject.name);
			}
		}
	}

	public Spline GetNewSpline(Vector3 startingPosition, float gravity)
	{
		int num = 10;
		Vector3[] array = new Vector3[num];
		array[0] = startingPosition;
		array[num - 1] = _Target.transform.position;
		Vector3 vector = _Target.transform.position - startingPosition;
		Vector3 vector2 = vector;
		vector2.y = 0f;
		Vector3 vector3 = vector.normalized + Vector3.up;
		Vector3 vector4 = vector3;
		vector4.y = 0f;
		float magnitude = vector4.magnitude;
		float num2 = vector3.y / magnitude;
		float num3 = Mathf.Sqrt(gravity * vector2.magnitude * vector2.magnitude / (2f * magnitude * magnitude * (vector2.magnitude * num2 - vector.y)));
		float num4 = vector2.magnitude / (num3 * magnitude);
		for (int i = 1; i < num - 1; i++)
		{
			float num5 = num4 * ((float)i / (float)num);
			float num6 = num3 * magnitude * num5;
			float y = num3 * vector3.y * num5 - 0.5f * gravity * num5 * num5;
			array[i] = vector2.normalized * num6;
			array[i].y = y;
			array[i] += array[0];
		}
		return new Spline(array, null, null, looping: false, constSpeed: true, alignTangent: false);
	}

	public void Use(GameObject go)
	{
		go.BroadcastMessage("OnSpringBoardUse", this, SendMessageOptions.DontRequireReceiver);
	}
}
