using System;
using System.Collections.Generic;
using KnowledgeAdventure.Multiplayer.Events;
using UnityEngine;

public class PowerUpManager : MonoBehaviour
{
	public const string POWERUP = "POWERUP";

	public const string POWERUP_ACTION = "POWERUP_A";

	public const int SPAWN_DELAY_TIME = 0;

	public const char MESSAGE_SEPARATOR = ':';

	public PowerUp[] _PowerUpList;

	public GameObject _Collectables;

	[NonSerialized]
	public bool _IsGamePaused;

	private List<PowerUp> mPowerUpList = new List<PowerUp>();

	private MonoBehaviour mGameManager;

	private bool mIsMMOInitialized;

	private PowerUpHelper mPowerUpHelper;

	public List<PowerUp> pPowerUpList => mPowerUpList;

	public PowerUpHelper pPowerUpHelper => mPowerUpHelper;

	protected void InitMMO()
	{
		if (!mIsMMOInitialized && MainStreetMMOClient.pInstance != null)
		{
			MainStreetMMOClient._IgnoreSenderMessage = false;
			MainStreetMMOClient.pInstance.AddMessageReceivedEventHandler(MessageResponse);
			mIsMMOInitialized = true;
		}
	}

	protected virtual void Update()
	{
		if (!mIsMMOInitialized)
		{
			InitMMO();
		}
	}

	public void Init(MonoBehaviour gameManager, PowerUpHelper helper)
	{
		mGameManager = gameManager;
		mPowerUpHelper = helper;
	}

	private void MessageResponse(object sender, MMOMessageReceivedEventArgs args)
	{
		if (args == null || args.MMOMessage == null || string.IsNullOrEmpty(args.MMOMessage.MessageText))
		{
			return;
		}
		string[] array = args.MMOMessage.MessageText.Split(':');
		if (array[0] == "POWERUP")
		{
			if (args.MMOMessage.Sender.Username == ProductConfig.pToken || args.MMOMessage.Sender.Username == WsWebService.pUserToken)
			{
				InitPowerUp(array[1], isMMO: true, args);
			}
			else
			{
				InitPowerUp(array[1], isMMO: true, args);
			}
		}
		else
		{
			if (!(array[0] == "POWERUP_A"))
			{
				return;
			}
			foreach (PowerUp mPowerUp in mPowerUpList)
			{
				if (mPowerUp.pHostUserId == array[1])
				{
					mPowerUp.Action(array[2], array[3]);
					break;
				}
			}
		}
	}

	public PowerUp InitPowerUp(string inConsumableName, bool isMMO, MMOMessageReceivedEventArgs args = null)
	{
		PowerUp powerUp = GetPowerUp(inConsumableName);
		if (mGameManager != null && powerUp != null)
		{
			if (!isMMO)
			{
				powerUp.Init(mGameManager, this);
			}
			else
			{
				powerUp.Init(mGameManager, this, args);
			}
		}
		return powerUp;
	}

	private PowerUp GetPowerUp(string inConsumableName)
	{
		foreach (PowerUp mPowerUp in mPowerUpList)
		{
			string type = mPowerUp._Type;
			if (inConsumableName == type && !mPowerUp.IsActive())
			{
				UtDebug.Log("Returning from list " + type);
				return mPowerUp;
			}
			if (inConsumableName == type && mPowerUp.IsActive() && mPowerUp._IsSingleInstance)
			{
				mPowerUp.ResetDuration();
				UtDebug.Log("Same power used again, Reset duration of " + mPowerUp._Type);
				return null;
			}
		}
		PowerUp[] powerUpList = _PowerUpList;
		foreach (PowerUp powerUp in powerUpList)
		{
			string type2 = powerUp._Type;
			if (inConsumableName == type2)
			{
				PowerUp component = UnityEngine.Object.Instantiate(powerUp.gameObject).GetComponent<PowerUp>();
				mPowerUpList.Add(component);
				return component;
			}
		}
		UtDebug.LogWarning("GetPowerUp NULL " + inConsumableName);
		return null;
	}

	private void OnDisable()
	{
		MainStreetMMOClient._IgnoreSenderMessage = true;
		MainStreetMMOClient.pInstance.RemoveMessageReceivedEventHandler(MessageResponse);
	}

	public void GamePause(bool pause)
	{
		_IsGamePaused = pause;
		foreach (PowerUp mPowerUp in mPowerUpList)
		{
			if (mPowerUp.IsActive())
			{
				mPowerUp.GamePause(pause);
			}
		}
	}

	public void DestroyCollectables()
	{
		if (_Collectables != null)
		{
			UnityEngine.Object.Destroy(_Collectables);
		}
	}

	public void DeactivateAllPowerUps()
	{
		foreach (PowerUp mPowerUp in mPowerUpList)
		{
			if (mPowerUp.IsActive())
			{
				mPowerUp.DeActivate();
			}
		}
	}
}
