using System.Collections.Generic;
using UnityEngine;

public class NPCPetManager : MonoBehaviour
{
	public delegate void PetReady(SanctuaryPet pet);

	public List<NPCPetData> _PetData;

	public List<PetMarker> _PetMarkers;

	public bool _IsFollowAvatar;

	public float _StartOffsetRear = 100f;

	public float _FollowRearDistance = 3f;

	private bool mInitialized;

	public event PetReady OnPetReadyEvent;

	private void Update()
	{
		if (mInitialized || !(SanctuaryData.pInstance != null))
		{
			return;
		}
		mInitialized = true;
		if (_PetData == null)
		{
			return;
		}
		foreach (NPCPetData petDatum in _PetData)
		{
			RaisedPetData raisedPetData = RaisedPetData.CreateCustomizedPetData(petDatum._TypeID, petDatum._Age, petDatum._DataPath, petDatum._Gender, null, noColorMap: true);
			raisedPetData.Name = petDatum._Name;
			SanctuaryManager.CreatePet(raisedPetData, Vector3.zero, Quaternion.identity, base.gameObject, "Basic");
		}
	}

	public void OnPetReady(SanctuaryPet pet)
	{
		pet.SetAvatar(base.transform);
		pet.pMeterPaused = true;
		pet.SetFollowAvatar(_IsFollowAvatar);
		if (!string.IsNullOrEmpty(pet.pData.Name))
		{
			pet.gameObject.name = pet.pData.Name;
		}
		ObClickable component = pet.GetComponent<ObClickable>();
		if (component != null)
		{
			component._Active = false;
		}
		pet._StartOffsetRear = _StartOffsetRear;
		pet._FollowRearDistance = _FollowRearDistance;
		if (_PetMarkers.Count > 0)
		{
			PetMarker petMarker = _PetMarkers[0];
			Transform transform = (petMarker._PetMarker ? petMarker._PetMarker.transform : GameObject.Find(petMarker._MarkerName).transform);
			pet.transform.position = transform.position;
			pet.transform.rotation = transform.rotation;
			_PetMarkers.RemoveAt(0);
		}
		else
		{
			pet.MoveToAvatar(postponed: true);
		}
		if (this.OnPetReadyEvent != null)
		{
			this.OnPetReadyEvent(pet);
		}
	}
}
