using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using UnityEngine;

[Serializable]
[XmlRoot("ConsumableData", Namespace = "")]
public class ConsumableData
{
	[XmlElement(ElementName = "ConsumableGame")]
	public ConsumableGame[] ConsumableGames;

	public static ConsumableData pConsumableData;

	public static bool pIsReady => pConsumableData != null;

	public static void Init()
	{
		if (pConsumableData == null)
		{
			RsResourceManager.Load(GameConfig.GetKeyData("ConsumableDataFile"), OnXMLDownloaded);
		}
	}

	private static void OnXMLDownloaded(string inURL, RsResourceLoadEvent inEvent, float inProgress, object inObject, object inUserData)
	{
		switch (inEvent)
		{
		case RsResourceLoadEvent.COMPLETE:
			pConsumableData = UtUtilities.DeserializeFromXml((string)inObject, typeof(ConsumableData)) as ConsumableData;
			if (pConsumableData != null)
			{
				pConsumableData.AssignConsumableType();
			}
			break;
		case RsResourceLoadEvent.ERROR:
			Debug.LogError("FAILED TO LOAD :: " + inURL);
			break;
		}
	}

	private void AssignConsumableType()
	{
		if (ConsumableGames == null || ConsumableGames.Length == 0)
		{
			return;
		}
		ConsumableGame[] consumableGames = ConsumableGames;
		foreach (ConsumableGame consumableGame in consumableGames)
		{
			if (consumableGame.ConsumableTypes == null || consumableGame.ConsumableTypes.Length == 0)
			{
				break;
			}
			ConsumableType[] consumableTypes = consumableGame.ConsumableTypes;
			foreach (ConsumableType consumableType in consumableTypes)
			{
				if (consumableType.Consumables == null || consumableType.Consumables.Length == 0)
				{
					break;
				}
				Consumable[] consumables = consumableType.Consumables;
				for (int k = 0; k < consumables.Length; k++)
				{
					consumables[k]._Type = consumableType.type;
				}
			}
		}
	}

	public static ConsumableGame GetConsumableGameData(string inGame)
	{
		if (!pIsReady)
		{
			return null;
		}
		ConsumableGame[] consumableGames = pConsumableData.ConsumableGames;
		foreach (ConsumableGame consumableGame in consumableGames)
		{
			if (consumableGame.type == inGame)
			{
				return consumableGame;
			}
		}
		return null;
	}

	public static ConsumableType GetConsumableTypeByGame(string inGame, string inType)
	{
		if (!pIsReady)
		{
			return null;
		}
		ConsumableGame consumableGameData = GetConsumableGameData(inGame);
		if (consumableGameData == null)
		{
			return null;
		}
		ConsumableType[] consumableTypes = consumableGameData.ConsumableTypes;
		foreach (ConsumableType consumableType in consumableTypes)
		{
			if (consumableType.type == inType)
			{
				return consumableType;
			}
		}
		return null;
	}

	public static List<Consumable> GetConsumableByTypeAndMode(string inGame, string inType, int gameMode)
	{
		if (!pIsReady)
		{
			return null;
		}
		ConsumableGame consumableGameData = GetConsumableGameData(inGame);
		if (consumableGameData == null)
		{
			return null;
		}
		List<Consumable> list = new List<Consumable>();
		ConsumableType[] consumableTypes = consumableGameData.ConsumableTypes;
		foreach (ConsumableType consumableType in consumableTypes)
		{
			if (!(consumableType.type == inType))
			{
				continue;
			}
			Consumable[] consumables = consumableType.Consumables;
			foreach (Consumable consumable in consumables)
			{
				if (consumable.Mode == 2 || consumable.Mode == gameMode)
				{
					list.Add(consumable);
				}
			}
		}
		return list;
	}

	public static Consumable GetConsumableByNameAndType(string inName, ConsumableType inType, int gameMode = 0)
	{
		Consumable[] consumables = inType.Consumables;
		foreach (Consumable consumable in consumables)
		{
			if ((consumable.Mode == 2 || consumable.Mode == gameMode) && string.Equals(consumable.name, inName, StringComparison.OrdinalIgnoreCase))
			{
				return consumable;
			}
		}
		return null;
	}

	public static Consumable GetConsumableOnProbability(string inGame, string inType, int position, int gameMode = 0)
	{
		ConsumableType consumableTypeByGame = GetConsumableTypeByGame(inGame, inType);
		if (consumableTypeByGame != null)
		{
			float num = 0f;
			Consumable[] consumables = consumableTypeByGame.Consumables;
			foreach (Consumable consumable in consumables)
			{
				if (consumable.Mode == 2 || consumable.Mode == gameMode)
				{
					num += Convert.ToSingle(consumable.Probabilities[position].Val);
				}
			}
			float num2 = UnityEngine.Random.value * num;
			num = 0f;
			consumables = consumableTypeByGame.Consumables;
			foreach (Consumable consumable2 in consumables)
			{
				if (consumable2.Mode == 2 || consumable2.Mode == gameMode)
				{
					num += Convert.ToSingle(consumable2.Probabilities[position].Val);
					if (num2 <= num)
					{
						UtDebug.Log("Returning consumable " + consumable2.name);
						return consumable2;
					}
				}
			}
		}
		return null;
	}
}
