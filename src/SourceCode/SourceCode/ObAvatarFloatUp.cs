using System.Collections.Generic;
using UnityEngine;

public class ObAvatarFloatUp : MonoBehaviour
{
	public float _FloatUpSpeed = 3f;

	private List<GameObject> mPlayersUsingTrigger;

	public void Start()
	{
		mPlayersUsingTrigger = new List<GameObject>();
	}

	public void Use(GameObject player)
	{
		AllowPlayerToUseTrigger(player);
		AvAvatarController obj = (AvAvatarController)player.GetComponent("AvAvatarController");
		obj.pGravityMultiplier = 0f;
		obj.pGlideModeSpeedMultiplier = 0f;
		if (AvAvatar.IsCurrentPlayer(player))
		{
			Input.ResetInputAxes();
		}
	}

	public void OnTriggerStay(Collider other)
	{
		GameObject gameObject = other.gameObject;
		if (IsPlayerAllowedToUserTrigger(gameObject))
		{
			AvAvatarController obj = (AvAvatarController)gameObject.GetComponent("AvAvatarController");
			Vector3 position = obj.transform.position;
			position.y += _FloatUpSpeed * Time.deltaTime;
			obj.transform.position = position;
		}
	}

	public void OnTriggerExit(Collider other)
	{
		GameObject gameObject = other.gameObject;
		int num = mPlayersUsingTrigger.IndexOf(gameObject);
		if (num >= 0)
		{
			mPlayersUsingTrigger.RemoveAt(num);
			AvAvatarController obj = (AvAvatarController)gameObject.GetComponent("AvAvatarController");
			obj.pGravityMultiplier = 1f;
			obj.pGlideModeSpeedMultiplier = 1f;
			Vector3 pVelocity = obj.pVelocity;
			pVelocity.y = 0f;
			obj.pVelocity = pVelocity;
		}
	}

	private bool IsPlayerAllowedToUserTrigger(GameObject player)
	{
		return mPlayersUsingTrigger.Contains(player);
	}

	public void AllowPlayerToUseTrigger(GameObject player)
	{
		mPlayersUsingTrigger.Add(player);
	}
}
