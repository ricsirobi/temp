using UnityEngine;

public class PetPlayAreaLoader : MonoBehaviour
{
	public string _ResourcePath = "";

	public static string _ExitToScene = null;

	public static string _PetPlayAreaResourceName = "";

	public static GameObject _MessageObject = null;

	private const float LOAD_TIME = 0f;

	private bool mLoaded;

	private float mTimer;

	private void Start()
	{
		mTimer = 0f;
		if (string.IsNullOrEmpty(_PetPlayAreaResourceName))
		{
			_PetPlayAreaResourceName = _ResourcePath;
		}
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

	public static void Load()
	{
		if (!string.IsNullOrEmpty(_PetPlayAreaResourceName))
		{
			KAUICursorManager.SetDefaultCursor("Loading");
			AvAvatar.SetUIActive(inActive: false);
			AvAvatar.pState = AvAvatarState.PAUSED;
			string[] array = _PetPlayAreaResourceName.Split('/');
			if (RsResourceManager.pCurrentLevel.Contains(GameConfig.GetKeyData("PetPlayScene")))
			{
				RsResourceManager.LoadAssetFromBundle(array[0] + "/" + array[1], array[2], OnPetPlayAreaLoadEvent, typeof(GameObject));
			}
			else
			{
				UtDebug.LogError("invalid name of prefab");
			}
		}
		else
		{
			UtDebug.LogError("Asset name not set : _PetPlayAreaResourceName is null or empty.");
		}
	}

	private static void OnPetPlayAreaLoadEvent(string inURL, RsResourceLoadEvent inEvent, float inProgress, object inObject, object inUserData)
	{
		switch (inEvent)
		{
		case RsResourceLoadEvent.COMPLETE:
		{
			GameObject obj = Object.Instantiate((GameObject)inObject);
			obj.name = "PetPlayArea";
			ActivateGameObj(obj);
			KAUICursorManager.SetDefaultCursor("Arrow");
			RsResourceManager.DestroyLoadScreen();
			break;
		}
		case RsResourceLoadEvent.ERROR:
			AvAvatar.pState = AvAvatarState.IDLE;
			AvAvatar.SetUIActive(inActive: true);
			KAUICursorManager.SetDefaultCursor("Arrow");
			break;
		}
	}

	private static void ActivateGameObj(GameObject inObject)
	{
		if (inObject != null)
		{
			SnChannel.StopPool("VO_Pool");
			Input.ResetInputAxes();
			inObject.SetActive(value: true);
			inObject.GetComponentInChildren<KAUIPetPlaySelect>().SetPet(SanctuaryPet.pLastClickedPet);
		}
	}
}
