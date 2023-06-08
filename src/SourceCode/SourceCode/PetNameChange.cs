using System;
using UnityEngine;

[Serializable]
public class PetNameChange
{
	public int _TicketStoreID = 93;

	public int _TicketItemID = 13370;

	[HideInInspector]
	public int mTicketCost;

	[HideInInspector]
	public RaisedPetData mPetData;

	[HideInInspector]
	public GameObject mMessageObject;

	public LocaleString _NotEnoughVCashText = new LocaleString("You don't have enough Gems to change name of your dragon. Do you want to buy more Gems?");

	public LocaleString _UseGemsForNameChangeText = new LocaleString("Dragon name change will cost you {cost} Gems. Do you want to continue?");

	public LocaleString _TicketPurchaseFailedText = new LocaleString("Transaction failed. Please try again.");
}
