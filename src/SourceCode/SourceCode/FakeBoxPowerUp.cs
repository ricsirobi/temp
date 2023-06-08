using System;
using KnowledgeAdventure.Multiplayer.Events;
using UnityEngine;

public class FakeBoxPowerUp : PowerUp
{
	public GameObject _BoxObject;

	public float _BoxCreateDistance = 2f;

	public const string FakeBoxObjectHitMsg = "OnFakeBoxObjectHit";

	private double mSpawnTime;

	private GameObject mBoxObject;

	private bool mCreate;

	public override void Init(MonoBehaviour gameManager, PowerUpManager manager, MMOMessageReceivedEventArgs args)
	{
		base.Init(gameManager, manager, args);
		if (args != null && !RacingManager.pIsSinglePlayer)
		{
			string[] array = args.MMOMessage.MessageText.Split(':');
			if (_BoxObject != null && array.Length > 2)
			{
				Vector3 inDefaultPos = Vector3.zero;
				UtUtilities.Vector3FromString(array[2], ref inDefaultPos);
				base.transform.position = inDefaultPos;
				mSpawnTime = Convert.ToDouble(array[^1]);
				mCreate = true;
				base.pHostUserId = array[3];
			}
		}
	}

	public override void Update()
	{
		base.Update();
		if (mCreate && MMOTimeManager.pInstance != null && MMOTimeManager.pInstance.GetServerTime() >= mSpawnTime && !RacingManager.pIsSinglePlayer)
		{
			mCreate = false;
			mBoxObject = UnityEngine.Object.Instantiate(_BoxObject, Vector3.zero, Quaternion.identity);
			mBoxObject.transform.parent = base.transform;
			mBoxObject.transform.localPosition = Vector3.zero;
			mBoxObject.transform.localEulerAngles = Vector3.zero;
			base.Activate();
		}
	}

	public override void Activate()
	{
		AvatarRacing component = AvAvatar.pObject.GetComponent<AvatarRacing>();
		if (component != null && !RacingManager.pIsSinglePlayer)
		{
			Vector3 vector = component.transform.position - component.transform.forward * _BoxCreateDistance;
			SendMMOMessage("POWERUP:" + _Type + ":" + vector.ToString() + ":" + UserInfo.pInstance.UserID);
		}
	}

	public override void DeActivate()
	{
		base.DeActivate();
		if (mBoxObject != null)
		{
			UnityEngine.Object.Destroy(mBoxObject);
		}
	}

	public override void Action(string inMessage, string inMsgSourceUserID)
	{
		if (inMessage == "OnFakeBoxObjectHit" && inMsgSourceUserID != UserInfo.pInstance.UserID)
		{
			DeActivate();
		}
	}
}
