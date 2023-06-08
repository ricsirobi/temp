public class UiJAEquipmentBeltMenu : KAUIMenu
{
	public override void OnClick(KAWidget inWidget)
	{
		base.OnClick(inWidget);
		EquipmentBeltData equipmentBeltData = (EquipmentBeltData)inWidget.GetUserData();
		if (equipmentBeltData != null && equipmentBeltData._ItemData != null)
		{
			((UiJAEquipment)_ParentUi).SelectTab(equipmentBeltData._CategoryID);
		}
	}
}
