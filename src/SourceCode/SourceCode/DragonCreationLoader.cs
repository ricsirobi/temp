using UnityEngine;

public class DragonCreationLoader : MonoBehaviour
{
	private const float LOAD_TIME = 0f;

	public GameObject _ProximityHatch;

	private bool mLoaded;

	private float mTimer;

	private void Start()
	{
		mTimer = 0f;
	}

	private void Update()
	{
		if (mTimer > 0f)
		{
			mTimer -= Time.deltaTime;
		}
		else if (!mLoaded && SanctuaryManager.pCurPetInstance != null)
		{
			mLoaded = true;
			Load();
		}
	}

	public void Load()
	{
		ObProximityHatch component = _ProximityHatch.GetComponent<ObProximityHatch>();
		component._LoadLastLevel = true;
		component.ActivateDragonCreationUIObj();
	}
}
