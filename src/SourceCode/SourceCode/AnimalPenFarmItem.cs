using System;
using System.Collections.Generic;
using UnityEngine;

public class AnimalPenFarmItem : FarmItem
{
	public string _AddAnimalText = "Add Animal";

	public int _MaxSheep = 5;

	public DrinkPoints[] _DrinkPoints;

	private float mMaxFeedQuantity;

	public float _FeedQuanitityOnClick = 30f;

	public int[] _AnimalItemIDList;

	public GUISkin _TextSkin;

	public TextMesh _TextMesh;

	public List<AnimalPenSpline> _SplineSets;

	private Dictionary<string, int> mContextDataName_InventoryID_Map = new Dictionary<string, int>();

	public GameObject _OnAddingAnimalFx;

	public AudioClip _OnAddingAnimalSFx;

	public AudioClip[] _AnimalAmbientSFX;

	public GameObject _SplineContainer;

	private UserItemData mCurrentUserItemData;

	public float pMaxFeedQuantity => mMaxFeedQuantity;

	protected override void OnActivate()
	{
		if (CanActivate())
		{
			base.OnActivate();
		}
	}

	protected override void Awake()
	{
		base.Awake();
		InitializeSplinePoints();
	}

	protected override void Update()
	{
		base.Update();
		if (!mInitialized && base.pFarmManager != null && base.pFarmManager.pIsReady)
		{
			Initialize();
		}
	}

	public override void Initialize()
	{
		base.Initialize();
		InitAddAnimalContext();
		mInitialized = true;
		List<AnimalFarmItem> animals = GetAnimals();
		if (animals == null || animals.Count == 0)
		{
			return;
		}
		foreach (AnimalFarmItem item in animals)
		{
			if (item == null || !item.pInitialized)
			{
				break;
			}
		}
	}

	protected virtual void InitAddAnimalContext()
	{
		FarmManager farmManager = MyRoomsIntMain.pInstance as FarmManager;
		if (farmManager != null)
		{
			ContextData contextData = farmManager.GetContextData(_AddAnimalText);
			if (contextData != null)
			{
				_DataList.Add(contextData);
			}
		}
	}

	protected virtual void InitializeSplinePoints()
	{
		if (!(_SplineContainer != null))
		{
			return;
		}
		_SplineSets.Clear();
		for (int i = 0; i < _SplineContainer.transform.childCount; i++)
		{
			GameObject gameObject = _SplineContainer.transform.GetChild(i).gameObject;
			AnimalPenSpline animalPenSpline = new AnimalPenSpline();
			List<Transform> list = new List<Transform>();
			for (int j = 0; j < gameObject.transform.childCount; j++)
			{
				list.Add(gameObject.transform.GetChild(j));
			}
			animalPenSpline._ID = i;
			animalPenSpline._SplinePoints = list.ToArray();
			_SplineSets.Add(animalPenSpline);
		}
	}

	public List<AnimalFarmItem> GetAnimals()
	{
		if (mChildren == null || mChildren.Count == 0)
		{
			return null;
		}
		List<AnimalFarmItem> list = new List<AnimalFarmItem>();
		foreach (FarmItem mChild in mChildren)
		{
			if (mChild is AnimalFarmItem)
			{
				list.Add((AnimalFarmItem)mChild);
			}
		}
		return list;
	}

	protected override void OnMenuActive(ContextSensitiveStateType inMenuType)
	{
		base.OnMenuActive(inMenuType);
		if (!base.pIsBuildMode && base.pUI != null)
		{
			base.pUI.transform.parent = base.transform;
		}
	}

	public override void SetParent(FarmItem inFarmItem)
	{
		Debug.LogError("Cannot add parent for sheep pen");
	}

	public override bool AddChild(FarmItem inFarmItem)
	{
		if (inFarmItem == null)
		{
			Debug.LogError("Cannot add child.");
			return false;
		}
		if (!inFarmItem.IsDependentOnFarmItem(this))
		{
			Debug.LogError("Only sheep item can be added as child in pen");
			return false;
		}
		if (!base.AddChild(inFarmItem))
		{
			return false;
		}
		AnimalFarmItem animalFarmItem = (AnimalFarmItem)inFarmItem;
		if (animalFarmItem != null)
		{
			SetAnimalStartPosition(animalFarmItem);
			ParticleSystem[] componentsInChildren = animalFarmItem.GetComponentsInChildren<ParticleSystem>(includeInactive: true);
			foreach (ParticleSystem particleSystem in componentsInChildren)
			{
				AddToSkipRenderer(particleSystem.GetComponent<Renderer>());
			}
			animalFarmItem.AddToClickableList(mFarmItemClickables);
			if (animalFarmItem._AmbientSFXChannel != null && _AnimalAmbientSFX != null && _AnimalAmbientSFX.Length != 0)
			{
				int num = UnityEngine.Random.Range(0, _AnimalAmbientSFX.Length);
				animalFarmItem._AmbientSFXChannel.pClip = _AnimalAmbientSFX[num];
				animalFarmItem._AmbientSFXChannel.Play();
			}
		}
		return true;
	}

	private void SetAnimalStartPosition(AnimalFarmItem inAnimal)
	{
		if (inAnimal == null)
		{
			return;
		}
		AnimalPenSpline animalPenSpline = SetUnusedSplineForFarmItem(inAnimal, inNew: false);
		if (animalPenSpline == null)
		{
			inAnimal.transform.position = base.transform.position;
			return;
		}
		if (animalPenSpline._FlipDirOnBackward)
		{
			inAnimal._SplineControl.Speed = -1f;
		}
		Spline spline = animalPenSpline.GetSpline();
		if (spline == null)
		{
			inAnimal.transform.position = base.transform.position;
			return;
		}
		inAnimal._SplineControl.SmoothMovement = false;
		inAnimal.SetSpline(spline);
	}

	public override bool CanPlaceDependentObject(FarmItem inDependentObject)
	{
		if (!base.CanPlaceDependentObject(inDependentObject))
		{
			return false;
		}
		if (inDependentObject is AnimalFarmItem)
		{
			List<AnimalFarmItem> animals = GetAnimals();
			if (animals != null && !((float)animals.Count < _LevelProgressInfo.GetValue("m")))
			{
				return false;
			}
			return true;
		}
		return true;
	}

	public override void ProcessClick()
	{
		base.ProcessClick();
		if (MyRoomsIntMain.pInstance != null && !MyRoomsIntMain.pInstance.pIsBuildMode)
		{
			Upgrade();
		}
	}

	public AnimalPenSpline GetSplineFromFarmItem(FarmItem inFarmItem)
	{
		if (_SplineSets == null || _SplineSets.Count == 0 || inFarmItem == null)
		{
			return null;
		}
		foreach (AnimalPenSpline splineSet in _SplineSets)
		{
			if (splineSet != null && splineSet.pFarmItem == inFarmItem)
			{
				return splineSet;
			}
		}
		return null;
	}

	public AnimalPenSpline GetSpline(int inID)
	{
		if (_SplineSets == null || _SplineSets.Count == 0)
		{
			return null;
		}
		foreach (AnimalPenSpline splineSet in _SplineSets)
		{
			if (splineSet != null && splineSet._ID == inID)
			{
				return splineSet;
			}
		}
		return null;
	}

	public bool IsSplineInUse(int inID)
	{
		return GetSpline(inID)?.IsInUse() ?? true;
	}

	public AnimalPenSpline GetUnusedSpline()
	{
		if (_SplineSets == null || _SplineSets.Count == 0)
		{
			return null;
		}
		foreach (AnimalPenSpline splineSet in _SplineSets)
		{
			if (splineSet != null && !splineSet.IsInUse())
			{
				return splineSet;
			}
		}
		return null;
	}

	public bool SetSplineForFarmItem(AnimalPenSpline inSplineSet, AnimalFarmItem inFarmItem, bool inNewSpline)
	{
		if (inFarmItem == null || inSplineSet == null)
		{
			return false;
		}
		foreach (AnimalPenSpline splineSet in _SplineSets)
		{
			if (splineSet != null && splineSet.pFarmItem == inFarmItem)
			{
				splineSet.pFarmItem = null;
			}
		}
		inSplineSet.pFarmItem = inFarmItem;
		if (inNewSpline)
		{
			float currentPos = inFarmItem._SplineControl.CurrentPos;
			Spline spline = inSplineSet.CreateSpline();
			inFarmItem._SplineControl.SetSpline(spline);
			inFarmItem._SplineControl.SetPosOnSpline(currentPos);
		}
		else
		{
			inFarmItem._SplineControl.SetSpline(inSplineSet.GetSpline());
		}
		inFarmItem._SplineControl.Looping = true;
		inFarmItem.pAvatarMarker = inSplineSet._AvatarMarker;
		return true;
	}

	public AnimalPenSpline SetUnusedSplineForFarmItem(AnimalFarmItem inFarmItem, bool inNew)
	{
		if (inFarmItem == null)
		{
			return null;
		}
		AnimalPenSpline animalPenSpline = GetSplineFromFarmItem(inFarmItem);
		if (animalPenSpline == null)
		{
			animalPenSpline = GetUnusedSpline();
		}
		if (!SetSplineForFarmItem(animalPenSpline, inFarmItem, inNew))
		{
			return null;
		}
		return animalPenSpline;
	}

	public override void OnBuildModeChanged(bool inBuildMode)
	{
		base.OnBuildModeChanged(inBuildMode);
		List<AnimalFarmItem> animals = GetAnimals();
		if (animals == null || animals.Count == 0)
		{
			return;
		}
		foreach (AnimalFarmItem item in animals)
		{
			SetUnusedSplineForFarmItem(item, inNew: true);
		}
	}

	public DrinkPoints GetNearestFeedPoint(Vector3 inPoint)
	{
		if (_DrinkPoints == null || _DrinkPoints.Length == 0)
		{
			return null;
		}
		float num = float.PositiveInfinity;
		DrinkPoints result = _DrinkPoints[0];
		for (int i = 0; i < _DrinkPoints.Length; i++)
		{
			if (_DrinkPoints[i] != null && !_DrinkPoints[i]._isOccupied && Vector3.Distance(inPoint, _DrinkPoints[i]._DrinkingPoint.position) < num)
			{
				num = Vector3.Distance(inPoint, _DrinkPoints[i]._DrinkingPoint.position);
				result = _DrinkPoints[i];
			}
		}
		return result;
	}

	public void Refill()
	{
	}

	private void ProcessAddAnimal(int inAnimalItemID)
	{
		mCurrentUserItemData = CommonInventoryData.pInstance.FindItem(inAnimalItemID);
		if (mCurrentUserItemData != null)
		{
			string[] separator = new string[1] { "/" };
			string[] array = mCurrentUserItemData.Item.AssetName.Split(separator, StringSplitOptions.None);
			CommonInventoryData.pInstance.RemoveItem(mCurrentUserItemData, 1);
			FarmItemBase.pIsBundleLoading = true;
			RsResourceManager.LoadAssetFromBundle(array[0] + "/" + array[1], array[2], AnimalEventHandler, typeof(GameObject));
		}
	}

	private void AnimalEventHandler(string inURL, RsResourceLoadEvent inEvent, float inProgress, object inObject, object inUserData)
	{
		switch (inEvent)
		{
		case RsResourceLoadEvent.COMPLETE:
		{
			FarmItemBase.pIsBundleLoading = false;
			GameObject gameObject = UnityEngine.Object.Instantiate((GameObject)inObject);
			MyRoomsIntMain.pInstance.ObjectCreatedCallback(gameObject, mCurrentUserItemData, inSaved: false);
			UtUtilities.SetLayerRecursively(gameObject, LayerMask.NameToLayer("Furniture"));
			gameObject.transform.parent = base.transform;
			gameObject.transform.localRotation = Quaternion.identity;
			CreateRoomObject(gameObject, mCurrentUserItemData);
			base.pFarmManager.AddRoomObject(gameObject, mCurrentUserItemData, base.gameObject, isUpdateLocalList: true);
			MyRoomsIntMain.pInstance.SaveExplicit();
			if (InteractiveTutManager._CurrentActiveTutorialObject != null)
			{
				InteractiveTutManager._CurrentActiveTutorialObject.SendMessage("TutorialManagerAsyncMessage", "PlacedSheep");
			}
			if (_OnAddingAnimalFx != null)
			{
				PlayOneShotFx(_OnAddingAnimalFx, gameObject.transform, addToParent: false);
			}
			if (_OnAddingAnimalSFx != null)
			{
				SnChannel.Play(_OnAddingAnimalSFx, "SFX_Pool", inForce: true);
			}
			break;
		}
		case RsResourceLoadEvent.ERROR:
			UtDebug.LogError("Item could not be downloaded.");
			FarmItemBase.pIsBundleLoading = false;
			break;
		}
	}

	public override void CreateRoomObject(GameObject inObject, UserItemData inUserItemData)
	{
		base.CreateRoomObject(inObject, inUserItemData);
		MyRoomObject component = inObject.GetComponent<MyRoomObject>();
		if (component != null)
		{
			component.pParentObject = base.gameObject;
			MyRoomObject component2 = base.gameObject.GetComponent<MyRoomObject>();
			if (component2 != null)
			{
				component2.AddChildReference(inObject);
			}
		}
	}

	protected override bool CanProcessUpdateData()
	{
		return CanActivate();
	}

	protected override void ProcessSensitiveData(ref List<string> menuItemNames)
	{
		if (!base.pIsBuildMode && GetAvailableAnimalInInventory() > 0)
		{
			if (menuItemNames.Contains("Store"))
			{
				menuItemNames.Remove("Store");
			}
			UpdateAddAnimalMenu(ref menuItemNames);
		}
		if (GetSheepCount() >= _MaxSheep && menuItemNames.Contains("Store"))
		{
			menuItemNames.Remove("Store");
		}
	}

	protected override void OnContextAction(string inActionName)
	{
		base.OnContextAction(inActionName);
		if (!(inActionName == "Refill"))
		{
			if (inActionName == "Store" && AvAvatar.pToolbar != null && base.pFarmManager._AnimalsStoreInfo != null)
			{
				StoreLoader.Load(setDefaultMenuItem: true, base.pFarmManager._AnimalsStoreInfo._Category, base.pFarmManager._AnimalsStoreInfo._Store, AvAvatar.pToolbar);
			}
		}
		else
		{
			KAInput.ResetInputAxes();
			Refill();
		}
		int[] animalItemIDList = _AnimalItemIDList;
		foreach (int itemID in animalItemIDList)
		{
			UserItemData userItemData = CommonInventoryData.pInstance.FindItem(itemID);
			if (userItemData != null && userItemData.Item.ItemName == inActionName)
			{
				ProcessAddAnimal(userItemData.Item.ItemID);
			}
		}
	}

	private void UpdateAddAnimalMenu(ref List<string> inNames)
	{
		ContextData contextData = GetContextData(_AddAnimalText);
		if (contextData == null)
		{
			return;
		}
		int[] animalItemIDList = _AnimalItemIDList;
		foreach (int itemID in animalItemIDList)
		{
			UserItemData userItemData = CommonInventoryData.pInstance.FindItem(itemID);
			if (userItemData != null && CanAnimalShow())
			{
				inNames.Add(userItemData.Item.ItemName);
				AddChildContextDataToParent(contextData, userItemData.Item, inShowInventoryCount: true, userItemData.Item.IconName);
				mContextDataName_InventoryID_Map[userItemData.Item.ItemName] = userItemData.Item.ItemID;
			}
		}
	}

	private bool CanAnimalShow()
	{
		int availableAnimalInInventory = GetAvailableAnimalInInventory();
		int sheepCount = GetSheepCount();
		if (availableAnimalInInventory <= 0 || sheepCount >= _MaxSheep)
		{
			return false;
		}
		return true;
	}

	private int GetSheepCount()
	{
		int result = 0;
		List<AnimalFarmItem> animals = GetAnimals();
		if (animals != null)
		{
			result = animals.Count;
		}
		return result;
	}

	private int GetAvailableAnimalInInventory()
	{
		int num = 0;
		if (CommonInventoryData.pInstance != null)
		{
			int[] animalItemIDList = _AnimalItemIDList;
			foreach (int itemID in animalItemIDList)
			{
				num += CommonInventoryData.pInstance.GetQuantity(itemID);
			}
		}
		return num;
	}

	public void RefreshCSM()
	{
		Refresh();
	}
}
