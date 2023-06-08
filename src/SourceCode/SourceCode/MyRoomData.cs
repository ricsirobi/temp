using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class MyRoomData
{
	public string _RoomID = "";

	public Camera _Camera;

	public GameObject _RoomInterior;

	public bool _ApplyWallpaperAllWalls = true;

	public GameObject[] _RoomWalls;

	public GameObject _MyRoomBuilder;

	public Transform _DecorationMarker;

	public int _DecorationCategory = -1;

	public MyRoomDefaultItems[] _DefaultItems;

	public NPCRoomData[] _NpcRoomData;

	public int _HouseWarmingItem;

	public string _TutHouseWarming;

	public AudioClip _HouseWarmingClip;

	public LocaleString _HouseWarmingText;

	public Vector2 _HouseWarmingTextPos = new Vector2(146f, 100f);

	public Vector2 _HouseWarmingOKBtnPos = new Vector2(520f, 385f);

	public Vector2 _HouseWarmingIconPos = new Vector2(390f, 370f);

	public Vector2 _HouseWarmingIconScale = new Vector2(1f, 1f);

	public TextAsset _TutorialTextAsset;

	public string _LongIntroName;

	public string _ShortIntroName;

	public bool _ObjectDraggable = true;

	private GameObject mSkyBoxObject;

	private GameObject mSkyBoxGroup;

	private GameObject mPetBedObject;

	private GameObject mFloorObject;

	private GameObject mWallPaperObject;

	private GameObject mDecorationObject;

	private GameObject mCushionObject;

	private List<GameObject> mGameObjectReferenceList = new List<GameObject>();

	private string mLevelName = "";

	private List<UserItemPosition> mDefaultItems = new List<UserItemPosition>();

	private int mNumDefaultItemsToLoad;

	private List<Transform> mDefaultWindowList = new List<Transform>();

	private List<GameObject> mCurrentWindowList = new List<GameObject>();

	private Texture mCurrentWallTexture;

	private Texture mTempWallTexture;

	private float mThresholdDistance = 0.005f;

	public GameObject pSkyBoxObject => mSkyBoxObject;

	public GameObject pPetBedObject => mPetBedObject;

	public GameObject pFloorObject => mFloorObject;

	public GameObject pWallPaperObject => mWallPaperObject;

	public GameObject pDecorationObject => mDecorationObject;

	public GameObject pCushionObject => mCushionObject;

	public List<GameObject> pGameObjectReferenceList => mGameObjectReferenceList;

	public int pRoomObjectCount => mGameObjectReferenceList.Count;

	public void Initialize()
	{
		mLevelName = RsResourceManager.pCurrentLevel;
		if (!(_RoomInterior != null))
		{
			return;
		}
		Component[] componentsInChildren = _RoomInterior.GetComponentsInChildren<Transform>();
		componentsInChildren = componentsInChildren;
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			Transform transform = (Transform)componentsInChildren[i];
			if (transform.gameObject.layer == LayerMask.NameToLayer("Window"))
			{
				mDefaultWindowList.Add(transform);
			}
		}
	}

	public void RecreateRoomItems()
	{
		List<string> list = new List<string>();
		UserItemPosition[] list2 = UserItemPositionList.GetList(_RoomID);
		if (list2 != null && list2.Length != 0)
		{
			UserItemPosition[] array = list2;
			foreach (UserItemPosition userItemPosition in array)
			{
				if (userItemPosition.Item != null)
				{
					list.Add(userItemPosition.Item.AssetName);
					if (userItemPosition.Item.Texture != null && userItemPosition.Item.Texture.Length != 0)
					{
						list.Add(userItemPosition.Item.Texture[0].TextureName);
					}
				}
			}
			if (list.Count > 0)
			{
				new RsAssetLoader().Load(list.ToArray(), null, BundleLoadEventHandler);
			}
		}
		else
		{
			ShowDefaultRoomItems();
		}
	}

	private void BundleLoadEventHandler(RsAssetLoader inLoader, RsResourceLoadEvent inEvent, float inProgress, object inUserData)
	{
		if ((uint)(inEvent - 2) > 1u)
		{
			return;
		}
		if (RsResourceManager.pCurrentLevel == mLevelName)
		{
			DeleteAllPrevRoomObjects();
			for (int i = 0; i < UserItemPositionList.GetList(_RoomID).Length; i++)
			{
				UserItemPosition inUserItemPosition = UserItemPositionList.GetList(_RoomID)[i];
				if (inUserItemPosition.Item != null && inUserItemPosition.Item.AssetName.Split('/').Length == 3)
				{
					CreateRoomObject(ref inUserItemPosition);
				}
			}
			LinkParentForRoomObjects();
			MyRoomsIntMain.pInstance.UpdateRoomLoadedCount();
		}
		MyRoomsIntMain.pDisableBuildMode = false;
	}

	public void ShowDefaultRoomItems()
	{
		ShowDefaultRoomItems(_DefaultItems);
	}

	public void ShowDefaultRoomItems(MyRoomDefaultItems[] inDefaultItems)
	{
		List<MyRoomDefaultItem> list = new List<MyRoomDefaultItem>();
		mNumDefaultItemsToLoad = 0;
		mDefaultItems.Clear();
		if (inDefaultItems != null && inDefaultItems.Length != 0)
		{
			foreach (MyRoomDefaultItems myRoomDefaultItems in inDefaultItems)
			{
				if (myRoomDefaultItems._Items.Length == 0)
				{
					continue;
				}
				MyRoomDefaultItem[] items = myRoomDefaultItems._Items;
				foreach (MyRoomDefaultItem myRoomDefaultItem in items)
				{
					if (!myRoomDefaultItem._InventoryOnly)
					{
						list.Add(myRoomDefaultItem);
					}
				}
			}
		}
		if (list.Count > 0)
		{
			mNumDefaultItemsToLoad = list.Count;
			{
				foreach (MyRoomDefaultItem item in list)
				{
					ItemData.Load(item._ItemID, DefaultItemDataEventHandler, item);
				}
				return;
			}
		}
		MyRoomsIntMain.pInstance.UpdateRoomLoadedCount();
	}

	public void ShowDefaultRoomItems(MyRoomDefaultItem[] inDefaultItems)
	{
		bool flag = false;
		if (inDefaultItems != null && inDefaultItems.Length != 0)
		{
			mNumDefaultItemsToLoad = inDefaultItems.Length;
			mDefaultItems.Clear();
			foreach (MyRoomDefaultItem myRoomDefaultItem in inDefaultItems)
			{
				ItemData.Load(myRoomDefaultItem._ItemID, DefaultItemDataEventHandler, myRoomDefaultItem);
				flag = true;
			}
		}
		if (!flag)
		{
			MyRoomsIntMain.pInstance.UpdateRoomLoadedCount();
		}
	}

	private void DefaultItemDataEventHandler(int itemID, ItemData dataItem, object inUserData)
	{
		MyRoomDefaultItem myRoomDefaultItem = (MyRoomDefaultItem)inUserData;
		UserItemPosition item = new UserItemPosition(dataItem, myRoomDefaultItem._Position, new Vector3(0f, myRoomDefaultItem._RotationY, 0f));
		mDefaultItems.Add(item);
		mNumDefaultItemsToLoad--;
		if (mNumDefaultItemsToLoad > 0)
		{
			return;
		}
		List<string> list = new List<string>();
		foreach (UserItemPosition mDefaultItem in mDefaultItems)
		{
			if (mDefaultItem.Item != null)
			{
				list.Add(mDefaultItem.Item.AssetName);
				if (mDefaultItem.Item.Texture != null && mDefaultItem.Item.Texture.Length != 0)
				{
					list.Add(mDefaultItem.Item.Texture[0].TextureName);
				}
			}
		}
		new RsAssetLoader().Load(list.ToArray(), null, DefaultBundleLoadEventHandler);
	}

	private void DefaultBundleLoadEventHandler(RsAssetLoader inLoader, RsResourceLoadEvent inEvent, float inProgress, object inUserData)
	{
		if ((uint)(inEvent - 2) > 1u)
		{
			return;
		}
		if (RsResourceManager.pCurrentLevel == mLevelName)
		{
			DeleteAllPrevRoomObjects();
			for (int i = 0; i < mDefaultItems.Count; i++)
			{
				UserItemPosition inUserItemPosition = mDefaultItems[i];
				if (inUserItemPosition.Item != null && inUserItemPosition.Item.AssetName.Split('/').Length == 3)
				{
					CreateRoomObject(ref inUserItemPosition);
				}
			}
			LinkParentForRoomObjects();
			MyRoomsIntMain.pInstance.UpdateRoomLoadedCount();
		}
		MyRoomsIntMain.pDisableBuildMode = false;
	}

	private void LinkParentForRoomObjects()
	{
		UserItemPosition[] list = UserItemPositionList.GetList(_RoomID);
		if (list == null)
		{
			return;
		}
		UserItemPosition[] array = list;
		foreach (UserItemPosition userItemPosition in array)
		{
			if (userItemPosition.Item == null || userItemPosition.Item.AssetName.Split('/').Length != 3 || !userItemPosition.ParentID.HasValue || !(userItemPosition._GameObject != null))
			{
				continue;
			}
			MyRoomObject component = userItemPosition._GameObject.GetComponent<MyRoomObject>();
			UserItemPosition[] list2 = UserItemPositionList.GetList(_RoomID);
			foreach (UserItemPosition userItemPosition2 in list2)
			{
				if (userItemPosition.ParentID.Value != userItemPosition2.UserItemPositionID)
				{
					continue;
				}
				GameObject gameObject = userItemPosition2._GameObject;
				if (!(gameObject != null))
				{
					break;
				}
				userItemPosition._GameObject.transform.parent = gameObject.transform;
				if (component != null)
				{
					component.pParentObject = gameObject;
					MyRoomObject component2 = gameObject.GetComponent<MyRoomObject>();
					if (component2 != null)
					{
						component2.AddChildReference(userItemPosition._GameObject);
					}
				}
				break;
			}
		}
	}

	private void DeleteAllPrevRoomObjects()
	{
		foreach (GameObject mGameObjectReference in mGameObjectReferenceList)
		{
			if (mGameObjectReference != null)
			{
				if (MyRoomsIntMain.pInstance != null)
				{
					MyRoomsIntMain.pInstance.ObjectDestroyCallback(mGameObjectReference);
				}
				UnityEngine.Object.Destroy(mGameObjectReference);
			}
		}
		mGameObjectReferenceList.Clear();
		if (mWallPaperObject != null)
		{
			UnityEngine.Object.Destroy(mWallPaperObject);
		}
		if (mFloorObject != null)
		{
			UnityEngine.Object.Destroy(mFloorObject);
		}
		if (mCushionObject != null)
		{
			UnityEngine.Object.Destroy(mCushionObject);
		}
		if (mSkyBoxObject != null)
		{
			UnityEngine.Object.Destroy(mSkyBoxObject);
		}
		if (mSkyBoxGroup != null)
		{
			UnityEngine.Object.Destroy(mSkyBoxGroup);
		}
		if (mPetBedObject != null)
		{
			UtUtilities.SetLayerRecursively(mPetBedObject, LayerMask.NameToLayer("Ignore Raycast"));
			UnityEngine.Object.Destroy(mPetBedObject);
		}
		if (mDecorationObject != null)
		{
			UnityEngine.Object.Destroy(mDecorationObject);
		}
		mWallPaperObject = null;
		mFloorObject = null;
		mCushionObject = null;
		mSkyBoxObject = null;
		mSkyBoxGroup = null;
		mPetBedObject = null;
		mDecorationObject = null;
	}

	public void EnableRoomObjectsClickable(bool isEnable, bool isEnableRoomItem = true)
	{
		foreach (GameObject mGameObjectReference in mGameObjectReferenceList)
		{
			if (!(mGameObjectReference != null))
			{
				continue;
			}
			ObClickable component = mGameObjectReference.GetComponent<ObClickable>();
			if (component != null)
			{
				component._Active = isEnable;
				if (component._HighlightMaterial == null)
				{
					component._HighlightMaterial = MyRoomsIntMain.pInstance._HighlightMaterial;
				}
			}
			MyRoomItem component2 = mGameObjectReference.GetComponent<MyRoomItem>();
			if (component2 != null)
			{
				component2.enabled = isEnableRoomItem;
			}
		}
	}

	private void CreateObject(UserItemPosition inUserItemPosition)
	{
		GameObject gameObject = (GameObject)RsResourceManager.LoadAssetFromBundle(inUserItemPosition.Item.AssetName);
		if (!(gameObject != null))
		{
			return;
		}
		GameObject gameObject2 = UnityEngine.Object.Instantiate(gameObject);
		if (inUserItemPosition.Item.Texture != null && inUserItemPosition.Item.Texture.Length != 0)
		{
			Texture t = (Texture)RsResourceManager.LoadAssetFromBundle(inUserItemPosition.Item.Texture[0].TextureName);
			UtUtilities.SetObjectTexture(gameObject2, 0, t);
		}
		inUserItemPosition._GameObject = gameObject2;
		MyRoomObject myRoomObject = gameObject2.GetComponent<MyRoomObject>();
		if (myRoomObject == null)
		{
			myRoomObject = gameObject2.AddComponent<MyRoomObject>();
		}
		MyRoomsIntMain.pInstance.AttachRaycastPoints(gameObject2);
		string name = inUserItemPosition.Item.AssetName.Substring(inUserItemPosition.Item.AssetName.LastIndexOf("/") + 1);
		gameObject2.name = name;
		gameObject2.transform.position = new Vector3((float)inUserItemPosition.PositionX.Value, (float)inUserItemPosition.PositionY.Value, (float)inUserItemPosition.PositionZ.Value);
		gameObject2.transform.rotation = Quaternion.Euler(new Vector3((float)inUserItemPosition.RotationX.Value, (float)inUserItemPosition.RotationY.Value, (float)inUserItemPosition.RotationZ.Value));
		Collider component = gameObject2.GetComponent<Collider>();
		if (component != null)
		{
			component.isTrigger = false;
		}
		myRoomObject.pUserItemData = CreateUserItemData(inUserItemPosition);
		if (!_ObjectDraggable)
		{
			DisableRoomObjectClickable(gameObject2);
		}
		else if (_MyRoomBuilder != null)
		{
			ObClickable obClickable = gameObject2.GetComponent<ObClickable>();
			if (obClickable == null)
			{
				obClickable = gameObject2.AddComponent<ObClickable>();
				obClickable._AvatarWalkTo = false;
			}
			obClickable._MessageObject = _MyRoomBuilder;
			obClickable._Active = false;
		}
		if (!mGameObjectReferenceList.Contains(gameObject2))
		{
			mGameObjectReferenceList.Add(gameObject2);
		}
		UtUtilities.SetLayerRecursively(gameObject2, LayerMask.NameToLayer("Furniture"));
		if (inUserItemPosition.Item.HasCategory(_DecorationCategory))
		{
			if (_DecorationMarker != null && Vector3.Distance(gameObject2.transform.position, _DecorationMarker.position) > mThresholdDistance)
			{
				gameObject2.transform.position = _DecorationMarker.position;
				gameObject2.transform.rotation = _DecorationMarker.rotation;
			}
			mDecorationObject = gameObject2;
		}
		if (MyRoomsIntMain.pInstance != null)
		{
			MyRoomsIntMain.pInstance.ObjectCreatedCallback(gameObject2, myRoomObject.pUserItemData, inSaved: true);
		}
		MyRoomItem component2 = gameObject2.GetComponent<MyRoomItem>();
		if (component2 != null)
		{
			component2.SetState(inUserItemPosition.UserItemState);
		}
	}

	public void CreateRoomObject(ref UserItemPosition inUserItemPosition)
	{
		if (inUserItemPosition == null)
		{
			return;
		}
		if (inUserItemPosition.Item.HasCategory(249))
		{
			if (_ApplyWallpaperAllWalls)
			{
				if (mWallPaperObject == null)
				{
					mWallPaperObject = new GameObject();
					inUserItemPosition._GameObject = mWallPaperObject;
					((MyRoomObject)mWallPaperObject.AddComponent(typeof(MyRoomObject))).pUserItemData = CreateUserItemData(inUserItemPosition);
					Texture wallPaperTexture = (Texture)RsResourceManager.LoadAssetFromBundle(inUserItemPosition.Item.AssetName);
					SetWallPaperTexture(wallPaperTexture);
					if (!MyRoomsIntMain.pInstance.pCategoryIgnoreList.Contains(inUserItemPosition.Item.ItemID))
					{
						MyRoomsIntMain.pInstance.pCategoryIgnoreList.Add(inUserItemPosition.Item.ItemID);
					}
				}
			}
			else
			{
				if (_RoomWalls == null || _RoomWalls.Length == 0)
				{
					return;
				}
				Vector3 vector = new Vector3((float)inUserItemPosition.PositionX.Value, (float)inUserItemPosition.PositionY.Value, (float)inUserItemPosition.PositionZ.Value);
				Texture texture = (Texture)RsResourceManager.LoadAssetFromBundle(inUserItemPosition.Item.AssetName);
				GameObject[] roomWalls = _RoomWalls;
				foreach (GameObject gameObject in roomWalls)
				{
					Vector3 vector2 = default(Vector3);
					vector2.x = (float)Math.Round(gameObject.transform.localPosition.x, 3);
					vector2.y = (float)Math.Round(gameObject.transform.localPosition.y, 3);
					vector2.z = (float)Math.Round(gameObject.transform.localPosition.z, 3);
					if (vector2 == vector && texture != null)
					{
						inUserItemPosition._GameObject = gameObject;
						gameObject.AddComponent<MyRoomObject>().pUserItemData = CreateUserItemData(inUserItemPosition);
						ApplyWallPaper(gameObject, texture, isTemp: false);
						if (!MyRoomsIntMain.pInstance.pCategoryIgnoreList.Contains(inUserItemPosition.Item.ItemID))
						{
							MyRoomsIntMain.pInstance.pCategoryIgnoreList.Add(inUserItemPosition.Item.ItemID);
						}
						break;
					}
				}
			}
		}
		else if (inUserItemPosition.Item.HasCategory(329))
		{
			if (!(mSkyBoxObject == null))
			{
				return;
			}
			mSkyBoxObject = new GameObject();
			mSkyBoxObject.name = "SkyBox_" + _RoomID;
			inUserItemPosition._GameObject = mSkyBoxObject;
			((MyRoomObject)mSkyBoxObject.AddComponent(typeof(MyRoomObject))).pUserItemData = CreateUserItemData(inUserItemPosition);
			Camera camera = _Camera;
			if (camera == null)
			{
				camera = AvAvatar.pAvatarCam.GetComponent<Camera>();
			}
			Skybox skybox = camera.GetComponent<Skybox>();
			if (skybox == null)
			{
				skybox = camera.gameObject.AddComponent<Skybox>();
			}
			skybox.material = (Material)RsResourceManager.LoadAssetFromBundle(inUserItemPosition.Item.AssetName);
			string attribute = inUserItemPosition.Item.GetAttribute<string>("Group", null);
			if (attribute != null)
			{
				string[] array = inUserItemPosition.Item.AssetName.Split('/');
				GameObject gameObject2 = (GameObject)RsResourceManager.LoadAssetFromBundle(array[0] + "/" + array[1] + "/" + attribute);
				if (gameObject2 != null)
				{
					mSkyBoxGroup = UnityEngine.Object.Instantiate(gameObject2);
				}
			}
		}
		else if (inUserItemPosition.Item.HasCategory(248))
		{
			UserItemData userItemData = CreateUserItemData(inUserItemPosition);
			bool flag = false;
			if (userItemData != null)
			{
				flag = userItemData.Item.GetAttribute("2D", defaultValue: false);
			}
			if (flag)
			{
				if (mFloorObject == null)
				{
					mFloorObject = new GameObject();
					inUserItemPosition._GameObject = mFloorObject;
					((MyRoomObject)mFloorObject.AddComponent(typeof(MyRoomObject))).pUserItemData = userItemData;
					Texture floorTexture = (Texture)RsResourceManager.LoadAssetFromBundle(inUserItemPosition.Item.AssetName);
					SetFloorTexture(floorTexture);
					if (!MyRoomsIntMain.pInstance.pCategoryIgnoreList.Contains(inUserItemPosition.Item.ItemID))
					{
						MyRoomsIntMain.pInstance.pCategoryIgnoreList.Add(inUserItemPosition.Item.ItemID);
					}
				}
			}
			else
			{
				CreateObject(inUserItemPosition);
			}
		}
		else if (inUserItemPosition.Item.HasCategory(367))
		{
			UserItemData pUserItemData = CreateUserItemData(inUserItemPosition);
			if (mCushionObject == null)
			{
				mCushionObject = new GameObject();
				inUserItemPosition._GameObject = mCushionObject;
				((MyRoomObject)mCushionObject.AddComponent(typeof(MyRoomObject))).pUserItemData = pUserItemData;
				Texture cushionTexture = (Texture)RsResourceManager.LoadAssetFromBundle(inUserItemPosition.Item.Texture[0].TextureName);
				SetCushionTexture(cushionTexture);
				if (!MyRoomsIntMain.pInstance.pCategoryIgnoreList.Contains(inUserItemPosition.Item.ItemID))
				{
					MyRoomsIntMain.pInstance.pCategoryIgnoreList.Add(inUserItemPosition.Item.ItemID);
				}
			}
		}
		else if (inUserItemPosition.Item.HasCategory(247))
		{
			if (!MyRoomsIntMain.pInstance.pCategoryIgnoreList.Contains(inUserItemPosition.Item.ItemID))
			{
				MyRoomsIntMain.pInstance.pCategoryIgnoreList.Add(inUserItemPosition.Item.ItemID);
			}
			AddWindow(isUpdateServer: false, inUserItemPosition, CreateUserItemData(inUserItemPosition));
		}
		else if (inUserItemPosition.Item.HasCategory(330))
		{
			GameObject gameObject3 = (GameObject)RsResourceManager.LoadAssetFromBundle(inUserItemPosition.Item.AssetName);
			if (gameObject3 != null)
			{
				GameObject gameObject4 = UnityEngine.Object.Instantiate(gameObject3);
				if (inUserItemPosition.Item.Texture != null && inUserItemPosition.Item.Texture.Length != 0)
				{
					Texture t = (Texture)RsResourceManager.LoadAssetFromBundle(inUserItemPosition.Item.Texture[0].TextureName);
					UtUtilities.SetObjectTexture(gameObject4, 0, t);
				}
				inUserItemPosition._GameObject = gameObject4;
				MyRoomObject myRoomObject = gameObject4.GetComponent<MyRoomObject>();
				if (myRoomObject == null)
				{
					myRoomObject = gameObject4.AddComponent<MyRoomObject>();
				}
				gameObject4.name = inUserItemPosition.Item.AssetName;
				gameObject4.transform.position = new Vector3((float)inUserItemPosition.PositionX.Value, (float)inUserItemPosition.PositionY.Value, (float)inUserItemPosition.PositionZ.Value);
				gameObject4.transform.rotation = Quaternion.Euler(new Vector3((float)inUserItemPosition.RotationX.Value, (float)inUserItemPosition.RotationY.Value, (float)inUserItemPosition.RotationZ.Value));
				myRoomObject.pUserItemData = CreateUserItemData(inUserItemPosition);
				ObClickable component = gameObject4.GetComponent<ObClickable>();
				if (component != null)
				{
					UnityEngine.Object.Destroy(component);
				}
				if (MyRoomsIntMain.pInstance._PetBedMarker != null && Vector3.Distance(gameObject4.transform.position, MyRoomsIntMain.pInstance._PetBedMarker.position) > mThresholdDistance)
				{
					gameObject4.transform.position = MyRoomsIntMain.pInstance._PetBedMarker.position;
					gameObject4.transform.rotation = MyRoomsIntMain.pInstance._PetBedMarker.rotation;
				}
				mPetBedObject = gameObject4;
				MyRoomsIntMain.pInstance.PositionPetOnBed();
			}
		}
		else
		{
			CreateObject(inUserItemPosition);
		}
	}

	public void AddRoomObject(GameObject inGameObject, UserItemData inUserItemData, GameObject inParentData, bool isUpdateLocalList)
	{
		if (isUpdateLocalList && !mGameObjectReferenceList.Contains(inGameObject))
		{
			mGameObjectReferenceList.Add(inGameObject);
		}
		if (!_ObjectDraggable)
		{
			DisableRoomObjectClickable(inGameObject);
		}
		UserItemPositionList.CreateObject(_RoomID, inGameObject, inUserItemData, inParentData);
	}

	public void AddRoomObject(GameObject inGameObject, UserItemData inUserItemData, Vector3 inPosition, Vector3 inRotation, GameObject inParentData, bool isUpdateLocalList)
	{
		if (isUpdateLocalList && !mGameObjectReferenceList.Contains(inGameObject))
		{
			mGameObjectReferenceList.Add(inGameObject);
		}
		UserItemPositionList.CreateObject(_RoomID, inGameObject, inUserItemData, inPosition, inRotation, inParentData);
	}

	public void RemoveRoomObject(GameObject inObject, bool isDestroy)
	{
		if (!(inObject != null))
		{
			return;
		}
		MyRoomObject component = inObject.GetComponent<MyRoomObject>();
		if (mGameObjectReferenceList.Contains(inObject))
		{
			mGameObjectReferenceList.Remove(inObject);
		}
		if (component != null)
		{
			MyRoomObject.ChildData[] pChildList = component.pChildList;
			for (int i = 0; i < pChildList.Length; i++)
			{
				GameObject child = pChildList[i]._Child;
				if (child != null)
				{
					UserItemPositionList.RemoveObject(_RoomID, child);
					if (mGameObjectReferenceList.Contains(child))
					{
						mGameObjectReferenceList.Remove(child);
					}
					if (child != null)
					{
						UnityEngine.Object.Destroy(child);
					}
				}
			}
			component.pParentObject = null;
			if (component.pUserItemData != null)
			{
				UserItemPositionList.RemoveObject(_RoomID, inObject);
			}
		}
		if (inObject != null && isDestroy)
		{
			UnityEngine.Object.Destroy(inObject);
		}
	}

	public UserItemData CreateUserItemData(UserItemPosition inUserItemPosition)
	{
		UserItemData userItemData = new UserItemData();
		if (inUserItemPosition != null)
		{
			userItemData.Item = inUserItemPosition.Item;
			if (inUserItemPosition.UserInventoryCommonID.HasValue)
			{
				userItemData.UserInventoryID = inUserItemPosition.UserInventoryCommonID.Value;
			}
			userItemData.Quantity = 1;
			userItemData.Uses = userItemData.Item.Uses;
			userItemData.ItemStats = inUserItemPosition.UserItemStat?.ItemStats;
			userItemData.ItemTier = inUserItemPosition.UserItemStat?.ItemTier;
			userItemData.UserItemAttributes = inUserItemPosition.UserItemAttributes;
		}
		return userItemData;
	}

	public void RemoveSkyBox()
	{
		if (mSkyBoxGroup != null)
		{
			UnityEngine.Object.Destroy(mSkyBoxGroup);
			mSkyBoxGroup = null;
		}
	}

	public void AddSkyBox(UserItemData inItemData, string inAssetName)
	{
		mSkyBoxObject = new GameObject();
		mSkyBoxObject.name = "SkyBox_" + _RoomID;
		((MyRoomObject)mSkyBoxObject.AddComponent(typeof(MyRoomObject))).pUserItemData = inItemData;
		AddRoomObject(mSkyBoxObject, inItemData, null, isUpdateLocalList: false);
		Camera camera = _Camera;
		if (camera == null)
		{
			camera = AvAvatar.pAvatarCam.GetComponent<Camera>();
		}
		Skybox skybox = camera.GetComponent<Skybox>();
		if (skybox == null)
		{
			skybox = camera.gameObject.AddComponent<Skybox>();
		}
		skybox.material = (Material)RsResourceManager.LoadAssetFromBundle(inAssetName);
		string attribute = inItemData.Item.GetAttribute<string>("Group", null);
		if (attribute != null)
		{
			string[] array = inAssetName.Split('/');
			GameObject original = (GameObject)RsResourceManager.LoadAssetFromBundle(array[0] + "/" + array[1] + "/" + attribute);
			mSkyBoxGroup = UnityEngine.Object.Instantiate(original);
		}
	}

	public void ToggleSkyBox(bool active)
	{
		if ((bool)mSkyBoxObject)
		{
			mSkyBoxObject.SetActive(active);
		}
		if ((bool)mSkyBoxGroup)
		{
			mSkyBoxGroup.SetActive(active);
		}
	}

	public void AddPetBedObject(UserItemData inItemData, string inAssetName, ItemDataTexture[] inTexture)
	{
		Vector3 position = Vector3.zero;
		Quaternion rotation = Quaternion.identity;
		if (MyRoomsIntMain.pInstance._PetBedMarker != null)
		{
			position = MyRoomsIntMain.pInstance._PetBedMarker.position;
			rotation = MyRoomsIntMain.pInstance._PetBedMarker.rotation;
		}
		GameObject gameObject = UnityEngine.Object.Instantiate((GameObject)RsResourceManager.LoadAssetFromBundle(inAssetName), position, rotation);
		if (inTexture != null && inTexture.Length != 0)
		{
			Texture t = (Texture)RsResourceManager.LoadAssetFromBundle(inTexture[0].TextureName);
			UtUtilities.SetObjectTexture(gameObject, 0, t);
		}
		MyRoomObject myRoomObject = gameObject.GetComponent<MyRoomObject>();
		if (myRoomObject == null)
		{
			myRoomObject = gameObject.AddComponent<MyRoomObject>();
		}
		myRoomObject.pUserItemData = inItemData;
		AddRoomObject(gameObject, inItemData, null, isUpdateLocalList: false);
		mPetBedObject = gameObject;
		MyRoomsIntMain.pInstance.PositionPetOnBed();
	}

	public void AddDecoration(UserItemData inItemData, string inAssetName, ItemDataTexture[] inTexture, bool inUseTranform, Vector3 inPosition, Quaternion inRotation)
	{
		Vector3 position = inPosition;
		Quaternion rotation = inRotation;
		if (!inUseTranform && _DecorationMarker != null)
		{
			position = _DecorationMarker.position;
			rotation = _DecorationMarker.rotation;
		}
		GameObject gameObject = UnityEngine.Object.Instantiate((GameObject)RsResourceManager.LoadAssetFromBundle(inAssetName), position, rotation);
		if (inTexture != null && inTexture.Length != 0)
		{
			Texture t = (Texture)RsResourceManager.LoadAssetFromBundle(inTexture[0].TextureName);
			UtUtilities.SetObjectTexture(gameObject, 0, t);
		}
		MyRoomObject myRoomObject = gameObject.GetComponent<MyRoomObject>();
		if (myRoomObject == null)
		{
			myRoomObject = gameObject.AddComponent<MyRoomObject>();
		}
		myRoomObject.pUserItemData = inItemData;
		AddRoomObject(gameObject, inItemData, null, isUpdateLocalList: false);
		mDecorationObject = gameObject;
	}

	public void AddFloorObject(UserItemData inItemData, string inAssetName)
	{
		mFloorObject = new GameObject();
		mFloorObject.name = "Floor_" + _RoomID;
		((MyRoomObject)mFloorObject.AddComponent(typeof(MyRoomObject))).pUserItemData = inItemData;
		AddRoomObject(mFloorObject, inItemData, null, isUpdateLocalList: false);
		Texture floorTexture = (Texture)RsResourceManager.LoadAssetFromBundle(inAssetName);
		SetFloorTexture(floorTexture);
	}

	private void SetFloorTexture(Texture inTex)
	{
		if (!(_RoomInterior != null) || !(inTex != null))
		{
			return;
		}
		Component[] componentsInChildren = _RoomInterior.GetComponentsInChildren<Renderer>();
		componentsInChildren = componentsInChildren;
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			Renderer renderer = (Renderer)componentsInChildren[i];
			if (renderer.gameObject.layer != LayerMask.NameToLayer("Floor") || renderer.materials == null)
			{
				continue;
			}
			Material[] materials = renderer.materials;
			foreach (Material material in materials)
			{
				if (material.name.ToLower().Contains("floortex"))
				{
					material.SetTexture("_MainTex", inTex);
				}
			}
		}
	}

	public void AddCushionObject(UserItemData inItemData, string inAssetName)
	{
		mCushionObject = new GameObject();
		mCushionObject.name = "Cushion_" + _RoomID;
		((MyRoomObject)mCushionObject.AddComponent(typeof(MyRoomObject))).pUserItemData = inItemData;
		AddRoomObject(mCushionObject, inItemData, null, isUpdateLocalList: false);
		Texture cushionTexture = (Texture)RsResourceManager.LoadAssetFromBundle(inAssetName);
		SetCushionTexture(cushionTexture);
	}

	private void SetCushionTexture(Texture inTex)
	{
		if (!(_RoomInterior != null) || !(inTex != null))
		{
			return;
		}
		Component[] componentsInChildren = _RoomInterior.GetComponentsInChildren<Renderer>();
		componentsInChildren = componentsInChildren;
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			Renderer renderer = (Renderer)componentsInChildren[i];
			if (!renderer.gameObject.CompareTag("Couch") || renderer.materials == null)
			{
				continue;
			}
			Material[] materials = renderer.materials;
			foreach (Material material in materials)
			{
				if (material.name.ToLower().Contains("cushiontex"))
				{
					material.SetTexture("_MainTex", inTex);
				}
			}
		}
	}

	public void AddWindow(bool isUpdateServer, UserItemPosition inUserItemPosition, UserItemData inItemData)
	{
		string text = null;
		if (inUserItemPosition != null && inUserItemPosition.Item != null)
		{
			text = inUserItemPosition.Item.AssetName;
		}
		else if (inItemData != null && inItemData.Item != null)
		{
			text = inItemData.Item.AssetName;
		}
		if (text == null)
		{
			return;
		}
		GameObject gameObject = (GameObject)RsResourceManager.LoadAssetFromBundle(text);
		if (!(gameObject != null))
		{
			return;
		}
		int num = 0;
		foreach (GameObject mCurrentWindow in mCurrentWindowList)
		{
			if (isUpdateServer && num == 0)
			{
				MyRoomObject component = mCurrentWindow.GetComponent<MyRoomObject>();
				if (component != null)
				{
					CommonInventoryData.pInstance.AddItem(component.pUserItemData, updateServer: false, 1);
					MyRoomsIntMain.pInstance.pCategoryIgnoreList.Remove(component.pUserItemData.Item.ItemID);
					RemoveRoomObject(mCurrentWindow, isDestroy: true);
				}
			}
			else
			{
				UnityEngine.Object.Destroy(mCurrentWindow);
			}
			num++;
		}
		mCurrentWindowList.Clear();
		num = 0;
		foreach (Transform mDefaultWindow in mDefaultWindowList)
		{
			GameObject gameObject2 = UnityEngine.Object.Instantiate(gameObject);
			gameObject2.transform.position = mDefaultWindow.transform.position;
			gameObject2.transform.rotation = mDefaultWindow.transform.rotation;
			gameObject2.transform.parent = _RoomInterior.transform;
			mDefaultWindow.gameObject.SetActive(value: false);
			mCurrentWindowList.Add(gameObject2);
			if (num == 0)
			{
				MyRoomObject myRoomObject = gameObject2.GetComponent(typeof(MyRoomObject)) as MyRoomObject;
				if (myRoomObject == null)
				{
					myRoomObject = (MyRoomObject)gameObject2.AddComponent(typeof(MyRoomObject));
				}
				myRoomObject.pUserItemData = inItemData;
				if (isUpdateServer)
				{
					AddRoomObject(gameObject2, inItemData, null, isUpdateLocalList: false);
				}
			}
			num++;
		}
		if (mCurrentWallTexture != null && _ApplyWallpaperAllWalls)
		{
			SetWallPaperTexture(mCurrentWallTexture);
		}
	}

	private void SetWallPaperTexture(Texture inTex)
	{
		if (!(_RoomInterior != null) || !(inTex != null))
		{
			return;
		}
		mCurrentWallTexture = inTex;
		Component[] componentsInChildren = _RoomInterior.GetComponentsInChildren<Renderer>();
		componentsInChildren = componentsInChildren;
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			Renderer renderer = (Renderer)componentsInChildren[i];
			if (renderer.gameObject.layer != LayerMask.NameToLayer("Wall") || renderer.materials == null)
			{
				continue;
			}
			Material[] materials = renderer.materials;
			foreach (Material material in materials)
			{
				if (material.name.ToLower().Contains("wallpapertex"))
				{
					material.SetTexture("_MainTex", inTex);
				}
			}
		}
	}

	private void AddIndividualWindowWallpapers()
	{
		foreach (GameObject mCurrentWindow in mCurrentWindowList)
		{
			MyRoomObject component = ReturnSelectedWall(mCurrentWindow).GetComponent<MyRoomObject>();
			if (component != null)
			{
				Texture texture = (Texture)RsResourceManager.LoadAssetFromBundle(component.pUserItemData.Item.AssetName);
				if (texture != null)
				{
					ApplyWallPaper(mCurrentWindow, texture, isTemp: false);
				}
			}
		}
	}

	public GameObject ReturnSelectedWall(GameObject inObject)
	{
		GameObject result = null;
		GameObject[] roomWalls = _RoomWalls;
		foreach (GameObject gameObject in roomWalls)
		{
			if (gameObject.transform.localPosition == inObject.transform.localPosition)
			{
				result = gameObject;
				break;
			}
		}
		return result;
	}

	public void ApplyWallPaper(GameObject inObject, Texture inTex, bool isTemp)
	{
		if ((mWallPaperObject == inObject && isTemp) || !(inTex != null) || !(inObject != null))
		{
			return;
		}
		if (isTemp)
		{
			RevertToPrevWallPaper();
			mWallPaperObject = inObject;
		}
		else
		{
			mWallPaperObject = null;
		}
		Component[] components = inObject.GetComponents<Renderer>();
		components = components;
		for (int i = 0; i < components.Length; i++)
		{
			Renderer renderer = (Renderer)components[i];
			if (renderer.gameObject.layer == LayerMask.NameToLayer("Wall") && renderer.material != null && renderer.material.name.ToLower().Contains("wallpapertex"))
			{
				if (isTemp)
				{
					mTempWallTexture = renderer.material.GetTexture("_MainTex");
				}
				renderer.material.SetTexture("_MainTex", inTex);
			}
		}
	}

	public void RevertToPrevWallPaper()
	{
		if (!(mWallPaperObject != null))
		{
			return;
		}
		if (mTempWallTexture != null)
		{
			Component[] components = mWallPaperObject.GetComponents<Renderer>();
			components = components;
			for (int i = 0; i < components.Length; i++)
			{
				Renderer renderer = (Renderer)components[i];
				if (renderer.gameObject.layer == LayerMask.NameToLayer("Wall") && renderer.material != null && renderer.material.name.ToLower().Contains("wallpapertex"))
				{
					renderer.material.SetTexture("_MainTex", mTempWallTexture);
				}
			}
		}
		mWallPaperObject = null;
	}

	public void AddWallPaperObject(UserItemData inItemData, string inAssetName)
	{
		mWallPaperObject = new GameObject();
		mWallPaperObject.name = "WallPaper_" + _RoomID;
		((MyRoomObject)mWallPaperObject.AddComponent(typeof(MyRoomObject))).pUserItemData = inItemData;
		MyRoomsIntMain.pInstance.AddRoomObject(mWallPaperObject, inItemData, null, isUpdateLocalList: false);
		Texture wallPaperTexture = (Texture)RsResourceManager.LoadAssetFromBundle(inAssetName);
		SetWallPaperTexture(wallPaperTexture);
	}

	private void DisableRoomObjectClickable(GameObject inGameObject)
	{
		ObClickable component = inGameObject.GetComponent<ObClickable>();
		if (component != null)
		{
			component.enabled = false;
		}
	}

	public Vector3 CalculatePosition(Vector3 inPos)
	{
		if (MyRoomsIntMain.pInstance == null)
		{
			return inPos;
		}
		return MyRoomsIntMain.pInstance.CalculatePosition(inPos);
	}
}
