using System.Linq;
using UnityEngine;

namespace SquadTactics;

[ExecuteInEditMode]
public class PetMenuGrid : KAUIMenuGrid
{
	private SortOrder mLastSort;

	private void OnDisable()
	{
		mLastSort = SortOrder.NONE;
	}

	private string SortData(KAWidget widget, SortOrder order)
	{
		SquadData squadData = widget.GetUserData() as SquadData;
		CharacterData character = CharacterDatabase.pInstance.GetCharacter(squadData.unitData._UnitName, squadData.unitData._RaisedPetID);
		return order switch
		{
			SortOrder.NAME => RaisedPetData.GetByID(squadData.unitData._RaisedPetID).Name, 
			SortOrder.SPECIES => character._DisplayNameText.GetLocalizedString(), 
			SortOrder.WEAPON => character._WeaponData._WeaponType, 
			SortOrder.ELEMENT => character._WeaponData._ElementType.ToString(), 
			_ => "", 
		};
	}

	private int SortIntData(KAWidget widget)
	{
		SquadData squadData = widget.GetUserData() as SquadData;
		return CharacterDatabase.pInstance.GetLevel(RaisedPetData.GetByID(squadData.unitData._RaisedPetID));
	}

	public void SortPet(SortOrder order)
	{
		if (mLastSort == order)
		{
			return;
		}
		mLastSort = order;
		if (order == SortOrder.LEVEL)
		{
			mMenu.GetItems().Sort((KAWidget x, KAWidget y) => SortIntData(y).CompareTo(SortIntData(x)));
		}
		else if (order == SortOrder.NAME)
		{
			mMenu.GetItems().Sort((KAWidget x, KAWidget y) => string.Compare(SortData(x, order), SortData(y, order)));
		}
		else
		{
			mMenu.SetItems((from x in mMenu.GetItems()
				orderby SortData(x, order), SortIntData(x) descending
				select x).ToList());
		}
		Reposition();
	}
}
