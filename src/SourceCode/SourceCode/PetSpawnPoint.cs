using SWS;
using UnityEngine;

public class PetSpawnPoint : KAMonoBase
{
	[Tooltip("The pet will follow one of these splines at random if 'Follow Splines' is checked'")]
	public SWS.PathManager[] _Splines;

	[Tooltip("Check this to make the spawned pet follow the splines. If unchecked, the pet will use one of the specified AIs.")]
	public bool _FollowSplines;

	[Tooltip("One of the AI prefabs specified here will be attached to the pet")]
	public string[] _AIRootPrefabs;

	[Tooltip("The age of the Mythie to be spawned")]
	public RaisedPetStage _SpawnPetStage = RaisedPetStage.BABY;

	[Tooltip("Enter a list of Pet type IDs you want to limit the spawned pet to. Leave blank to allow any pet type.")]
	public int[] _LimitPetTypeIDs;

	[Tooltip("The interval after which the pet must be assigned a spline, when idle.")]
	public float _PetActivityCheckInterval = 2f;

	private SanctuaryPet mPet;

	protected AIEvaluator mAIRoot;

	private splineMove mCurrentMove;

	private PetSpawn mPetSpawn;

	public void SpawnPet(PetSpawn petSpawn)
	{
		mPetSpawn = petSpawn;
		SanctuaryPetTypeInfo sanctuaryPetTypeInfo = null;
		if (_LimitPetTypeIDs.Length != 0)
		{
			sanctuaryPetTypeInfo = SanctuaryData.FindSanctuaryPetTypeInfo(_LimitPetTypeIDs[Random.Range(0, _LimitPetTypeIDs.Length)]);
		}
		else
		{
			int num = Random.Range(0, SanctuaryData.pInstance._PetTypes.Length);
			sanctuaryPetTypeInfo = SanctuaryData.pInstance._PetTypes[num];
		}
		Gender gender = ((Random.Range(0, 2) == 0) ? Gender.Male : Gender.Female);
		string empty = string.Empty;
		SanctuaryManager.CreatePet(RaisedPetData.InitDefault(resName: (sanctuaryPetTypeInfo._AgeData[RaisedPetData.GetAgeIndex(_SpawnPetStage)]._PetResList[0]._Gender != gender) ? sanctuaryPetTypeInfo._AgeData[RaisedPetData.GetAgeIndex(_SpawnPetStage)]._PetResList[1]._Prefab : sanctuaryPetTypeInfo._AgeData[RaisedPetData.GetAgeIndex(_SpawnPetStage)]._PetResList[0]._Prefab, ptype: sanctuaryPetTypeInfo._TypeID, stage: _SpawnPetStage, gender: gender, addToActivePets: false), base.transform.position, Quaternion.identity, base.gameObject, "Full");
	}

	private Color RandomColor()
	{
		return new Color(Random.Range(0f, 1f), Random.Range(0f, 1f), Random.Range(0f, 1f));
	}

	public void OnPetCreated(SanctuaryPet pet)
	{
		if (!_FollowSplines)
		{
			if (_AIRootPrefabs.Length == 0)
			{
				UtDebug.Log("No AIRoot prefab names provided");
			}
			else
			{
				pet.GetComponentInChildren<AIBehavior_PrefabRef>()._PrefabName = _AIRootPrefabs[Random.Range(0, _AIRootPrefabs.Length)];
			}
		}
	}

	public void OnPetReady(SanctuaryPet pet)
	{
		mPet = pet;
		pet.pEnablePetAnim = true;
		pet.SetAvatar(null);
		pet.SetFollowAvatar(follow: false);
		pet.SetColors(RandomColor(), RandomColor(), RandomColor(), bSaveData: false);
		pet.SetClickActivateObject(null);
		pet.transform.parent = base.transform;
		pet.transform.localPosition = Vector3.zero;
		pet.transform.localRotation = Quaternion.identity;
		if (mPetSpawn != null)
		{
			mPetSpawn.OnResidentPetReady();
		}
		if (_FollowSplines)
		{
			InvokeRepeating("CheckPetActivity", _PetActivityCheckInterval, _PetActivityCheckInterval);
		}
		else
		{
			mPet.AIActor.SetState(AISanctuaryPetFSM.NORMAL);
		}
	}

	private void CheckPetActivity()
	{
		if (!(mCurrentMove != null))
		{
			int num = Random.Range(0, _Splines.Length + 1);
			if (num != _Splines.Length)
			{
				MovePetAlongSpline(mPet, _Splines[num]);
			}
		}
	}

	private void MovePetAlongSpline(SanctuaryPet pet, SWS.PathManager spline)
	{
		splineMove splineMove = (mCurrentMove = spline.GetComponentInChildren<splineMove>());
		splineMove.transform.localPosition = Vector3.zero;
		pet.transform.parent = splineMove.transform;
		pet.transform.localPosition = Vector3.zero;
		pet.transform.localRotation = Quaternion.identity;
		splineMove.SetPath(spline);
		splineMove.ResetToStart();
		splineMove.StartMove();
		splineMove.events[splineMove.events.Count - 1].AddListener(OnSplineEndReached);
		CancelInvoke("CheckPetActivity");
	}

	private void OnSplineEndReached()
	{
		mCurrentMove.Stop();
		mCurrentMove.events[mCurrentMove.events.Count - 1].RemoveListener(OnSplineEndReached);
		mCurrentMove = null;
		InvokeRepeating("CheckPetActivity", _PetActivityCheckInterval, _PetActivityCheckInterval);
	}
}
