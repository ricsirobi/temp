using UnityEngine;

public class ObProximityAnimatePromoGate : ObProximityAnimate
{
	public int _ThunderdrumTicketID = 8206;

	public int _WhisperingTicketID = 8205;

	public GameObject _FootPrintParticles;

	public override void Awake()
	{
		base.Awake();
		if (ParentData.pIsReady && _FootPrintParticles != null && (ParentData.pInstance.pInventory.pData.FindItem(_WhisperingTicketID) != null || ParentData.pInstance.pInventory.pData.FindItem(_ThunderdrumTicketID) != null))
		{
			_FootPrintParticles.SetActive(value: true);
		}
	}

	public override void Update()
	{
		if (!(base.animation == null) && _Range != 0f && !(AvAvatar.pObject == null) && !(_Animation == null) && !string.IsNullOrEmpty(_Animation.name))
		{
			base.Update();
		}
	}
}
