using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LabCrucible
{
	public enum ItemPositionOption
	{
		DEFAULT,
		MARKER,
		POSITION
	}

	public class TestItemLoader
	{
		public class ReplaceItemDetails
		{
			public LabTestObject mReplaceItem;

			public float mScaleDownTime;
		}

		public delegate void TestItemLoadedEvent(TestItemLoader inLoader);

		public static List<TestItemLoader> TestItemLoaderList = new List<TestItemLoader>();

		private static int mLoadCount = 0;

		public LabItem mTestItem;

		public ItemPositionOption mItemPositionOption;

		public Vector3 mPosition = Vector3.zero;

		public Quaternion mRotation = Quaternion.identity;

		public Vector3 mScale = Vector3.one;

		private TestItemLoadedEvent mEvent;

		public LabItem mParentItem;

		public object mUserData;

		public List<ReplaceItemDetails> mReplaceItems;

		public bool mRemove;

		private int mProgress;

		public GameObject mLoadedObj;

		public TestItemLoader(LabItem inTestItem, float inStartTemperature, TestItemLoadedEvent inEvent, object inUserData)
		{
			inTestItem.Initialize(OnLabItemInitialized, inStartTemperature);
			mTestItem = inTestItem;
			mEvent = inEvent;
			mUserData = inUserData;
			if (TestItemLoaderList == null)
			{
				TestItemLoaderList = new List<TestItemLoader>();
			}
			TestItemLoaderList.Add(this);
		}

		public void Load()
		{
			string[] array = mTestItem.Prefab.Split('/');
			if (array.Length >= 3)
			{
				KAUICursorManager.SetDefaultCursor("Loading", showHideSystemCursor: false);
				mLoadCount++;
				RsResourceManager.LoadAssetFromBundle(array[0] + "/" + array[1], array[2], TestItemLoaded, typeof(GameObject));
			}
		}

		private void AddProgress(int inCount = 1)
		{
			mProgress += inCount;
			if (mProgress >= 1 && mTestItem.pInitialized && mEvent != null)
			{
				mLoadCount = Mathf.Max(0, mLoadCount - 1);
				mEvent(this);
				mEvent = null;
			}
		}

		private void TestItemLoaded(string inURL, RsResourceLoadEvent inEvent, float inProgress, object inObject, object inUserData)
		{
			switch (inEvent)
			{
			case RsResourceLoadEvent.COMPLETE:
				mLoadedObj = (GameObject)inObject;
				AddProgress();
				break;
			case RsResourceLoadEvent.ERROR:
				Debug.LogError("############ UNEXPECTED ERROR!! The item " + inURL + " cannot be loaded ############");
				break;
			}
		}

		public void OnLabItemInitialized()
		{
			if (mProgress >= 1 && mTestItem != null && mTestItem.pInitialized && mEvent != null && mLoadedObj != null)
			{
				mLoadCount = Mathf.Max(0, mLoadCount - 1);
				mEvent(this);
				mEvent = null;
			}
		}

		public void AddReplaceItem(LabTestObject inObject, float inScaleDownTime)
		{
			if (mReplaceItems == null)
			{
				mReplaceItems = new List<ReplaceItemDetails>();
			}
			ReplaceItemDetails replaceItemDetails = new ReplaceItemDetails();
			replaceItemDetails.mReplaceItem = inObject;
			replaceItemDetails.mScaleDownTime = inScaleDownTime;
			mReplaceItems.Add(replaceItemDetails);
		}

		public static bool IsLoading()
		{
			return mLoadCount != 0;
		}

		public void Unload()
		{
			if (mTestItem != null)
			{
				mTestItem.Unload();
			}
			if (mLoadedObj == null)
			{
				UnityEngine.Object.Destroy(mLoadedObj);
			}
			mLoadedObj = null;
			if (mTestItem != null && !string.IsNullOrEmpty(mTestItem.Prefab))
			{
				RsResourceManager.Unload(mTestItem.Prefab);
			}
		}

		public static void UnloadAll()
		{
			if (TestItemLoaderList == null || TestItemLoaderList.Count == 0)
			{
				return;
			}
			foreach (TestItemLoader testItemLoader in TestItemLoaderList)
			{
				testItemLoader?.Unload();
			}
			TestItemLoaderList.Clear();
			TestItemLoaderList = null;
		}
	}

	public class LabCombinationLoader
	{
		public delegate void Callback(List<LabItemCombination> mCombinationList);

		private Callback mCallback;

		private List<LabItemCombination> mCombinationList;

		private int mLoadedCount;

		public void Add(List<LabItemCombination> inList)
		{
			if (inList != null)
			{
				if (mCombinationList == null)
				{
					mCombinationList = new List<LabItemCombination>();
				}
				mCombinationList.AddRange(inList);
			}
		}

		public void Load(Callback inCallback)
		{
			mCallback = inCallback;
			mLoadedCount = 0;
			if (mCombinationList == null || mCombinationList.Count == 0)
			{
				if (mCallback != null)
				{
					mCallback(mCombinationList);
				}
				return;
			}
			foreach (LabItemCombination mCombination in mCombinationList)
			{
				if (mCombination == null)
				{
					mLoadedCount++;
					if (CheckForAllLoaded())
					{
						break;
					}
				}
				mCombination.Initialize(OnInitalized);
			}
		}

		private void OnInitalized(LabItemCombination inCombination)
		{
			mLoadedCount++;
			CheckForAllLoaded();
		}

		private bool CheckForAllLoaded()
		{
			if (mLoadedCount >= mCombinationList.Count)
			{
				if (mCallback != null)
				{
					mCallback(mCombinationList);
				}
				return true;
			}
			return false;
		}
	}

	public delegate void OnStateChange();

	public delegate void CrucibleItemAddedCallback(LabTestObject inObject);

	public class SortLiquidOnPriority : IComparer<LabTestObject>
	{
		public SortLiquidOnPriority(ScientificExperiment inManager)
		{
		}

		public int Compare(LabTestObject x, LabTestObject y)
		{
			if ((x == null || x.pTestItem == null) && (y == null || y.pTestItem == null))
			{
				return 0;
			}
			if (x != null && x.pTestItem != null && (y == null || y.pTestItem == null))
			{
				return 1;
			}
			if ((x == null || x.pTestItem == null) && y != null && y.pTestItem != null)
			{
				return -1;
			}
			if (x.pTestItem.Name == y.pTestItem.Name)
			{
				return 0;
			}
			if (x.pTestItem.LiquidPriority < y.pTestItem.LiquidPriority)
			{
				return -1;
			}
			return 1;
		}
	}

	protected float mTemperature;

	protected ScientificExperiment mManager;

	protected List<LabTestObject> mTestItems;

	protected bool mHeating;

	protected bool mFreezing;

	private bool mLoadingWater;

	protected bool mMixDone;

	private float mHeatTimer;

	private float mClockTimer;

	protected bool mPaused;

	protected float mCurrentHeatMultiplier;

	protected float mCurrentFreezeMultiplier;

	private float mCurrentCoolingRate;

	private float mCurrentWarmingRate;

	private LabTestObject mTemperatureRefObject;

	public bool pPaused
	{
		get
		{
			return mPaused;
		}
		set
		{
			mPaused = value;
		}
	}

	public static GameObject pMixingEffect { get; set; }

	public float pCurrentHeatMultiplier => mCurrentHeatMultiplier;

	public float pCurrentFreezeMultiplier => mCurrentFreezeMultiplier;

	public float pCurrentCoolingRate => mCurrentCoolingRate;

	public float pCurrentWarmingRate => mCurrentWarmingRate;

	public float pClockTimer => mClockTimer;

	public static bool pIsMixing => pMixingEffect != null;

	public LabTestObject pTemperatureRefObject => mTemperatureRefObject;

	public float pTemperature => mTemperature;

	public List<LabTestObject> pTestItems => mTestItems;

	public bool pHeating => mHeating;

	public bool pFreezing => mFreezing;

	public LabCrucible(ScientificExperiment inManager)
	{
		mManager = inManager;
		mTemperature = LabData.pInstance.RoomTemperature;
		mCurrentHeatMultiplier = mManager._WarmingConstant;
		mCurrentFreezeMultiplier = mManager._FreezeRate;
		mCurrentCoolingRate = mManager._CoolingConstant;
		mCurrentWarmingRate = mManager._WarmingConstant;
	}

	public TestItemLoader AddTestItem(LabItem inItem, float inStartTemperature, CrucibleItemAddedCallback inCallback)
	{
		if (inItem == null || string.IsNullOrEmpty(inItem.Prefab))
		{
			return null;
		}
		inItem.mNoScaling = false;
		if (mManager != null)
		{
			mManager.EnableUI(inEnable: false);
		}
		mManager._MainUI.ActivateCursor(UiScienceExperiment.Cursor.NONE);
		return new TestItemLoader(inItem, inStartTemperature, OnTestItemDownloaded, inCallback);
	}

	public void AddTestItem(GameObject inItem, ItemPositionOption inPositionOption, Vector3 inPosition, Quaternion inRotation, Vector3 inScale, LabItem inParent, List<TestItemLoader.ReplaceItemDetails> inReplaceObjects, CrucibleItemAddedCallback inCallback)
	{
		if (inItem == null || (mTestItems != null && mTestItems.Find((LabTestObject inObj) => inObj != null && inObj.gameObject == inItem) != null))
		{
			return;
		}
		if (mTestItems != null && mTestItems.Count > 0)
		{
			if (mManager._MaxNumItemsAllowedInCrucible > 0 && mTestItems.Count >= mManager._MaxNumItemsAllowedInCrucible)
			{
				UnityEngine.Object.Destroy(inItem);
				return;
			}
			if (mTestItems.Find((LabTestObject x1) => x1 != null && x1.gameObject == inItem) != null)
			{
				return;
			}
		}
		LabTestObject component = inItem.GetComponent<LabTestObject>();
		if (!(component == null) && component.pTestItem != null && !string.IsNullOrEmpty(component.pTestItem.Prefab))
		{
			AddTestItemReal(component, inPositionOption, inPosition, inRotation, inScale, inParent, inReplaceObjects, inCallback);
		}
	}

	protected virtual void AddTestItemReal(LabTestObject inItemToAdd, ItemPositionOption inPositionOption, Vector3 inPosition, Quaternion inRotation, Vector3 inScale, LabItem inParentItem, List<TestItemLoader.ReplaceItemDetails> inReplaceObjects, CrucibleItemAddedCallback inCallback)
	{
		if (inItemToAdd == null || inItemToAdd.transform == null)
		{
			return;
		}
		if (!HandleReplaceItemOnItemAdd(inItemToAdd, inReplaceObjects))
		{
			RemoveTestItem(inItemToAdd);
			return;
		}
		if (inItemToAdd.HideItemsOnAdd() && mTestItems != null)
		{
			foreach (LabTestObject mTestItem in mTestItems)
			{
				mTestItem.Show(inShow: false);
			}
		}
		else if (mTestItems != null && mTestItems.Find((LabTestObject inObj) => inObj != null && inObj.HideItemsOnAdd()) != null)
		{
			inItemToAdd.Show(inShow: false);
		}
		if (mTestItems != null && inItemToAdd.pTestItem.StopsCombustion)
		{
			foreach (LabTestObject mTestItem2 in mTestItems)
			{
				mTestItem2.StopCombustion();
			}
		}
		mMixDone = false;
		mManager._MainUI.pElectrifyDone = false;
		if (mTestItems == null)
		{
			mTestItems = new List<LabTestObject>();
		}
		inItemToAdd.Initialize(mManager);
		inItemToAdd.Freeze(mFreezing);
		mTestItems.Add(inItemToAdd);
		mManager._MainUI.UpdateCrucibleData();
		switch (inPositionOption)
		{
		case ItemPositionOption.MARKER:
		{
			Transform freeMarker = GetFreeMarker(inItemToAdd.pTestItem.pCategory == LabItemCategory.SOLID_POWDER);
			if (freeMarker != null)
			{
				inItemToAdd.pMarker = freeMarker;
				inItemToAdd.transform.position = inItemToAdd.pMarker.position;
				if (inItemToAdd.rigidbody != null)
				{
					inItemToAdd.rigidbody.isKinematic = true;
					inItemToAdd.rigidbody.useGravity = false;
				}
			}
			break;
		}
		case ItemPositionOption.POSITION:
			inItemToAdd.transform.position = inPosition;
			inItemToAdd.transform.rotation = inRotation;
			break;
		}
		if (inItemToAdd.pTestItem.pCategory == LabItemCategory.LIQUID || inItemToAdd.pTestItem.pCategory == LabItemCategory.LIQUID_COMBUSTIBLE)
		{
			inItemToAdd.transform.localScale = Vector3.zero;
		}
		else
		{
			inItemToAdd.transform.localScale = inScale;
		}
		inItemToAdd.pParentItem = inParentItem;
		mManager.CheckForProcedureHalt("Action", "Addition");
		RecalculateTemperatureMultipliers();
		inItemToAdd.pCanWeight = true;
		inItemToAdd.pCanTestResistance = true;
		inCallback?.Invoke(inItemToAdd);
		HandleLiquidItems(inItemToAdd, inRemove: false);
		UpdateLiquidPosition();
		if (inItemToAdd.GetComponent<Rigidbody>() != null && ScientificExperiment.IsSolid(inItemToAdd.pTestItem.pCategory) && inItemToAdd.gameObject.layer == LayerMask.NameToLayer("DraggedObject"))
		{
			List<LabTestObject> crucibleItems = GetCrucibleItems(LabItemCategory.LIQUID);
			if (crucibleItems != null && crucibleItems.Count > 0)
			{
				Vector3 pDroppedPosition = mManager._MainUI.pDroppedPosition;
				float num = Time.timeSinceLevelLoad - mManager._MainUI.pDropStartedTime;
				Vector3 inVelocity = (inItemToAdd.transform.position - pDroppedPosition) / num;
				UpdateLiquidSplash(inVelocity, crucibleItems);
			}
			crucibleItems = GetCrucibleItems(LabItemCategory.LIQUID_COMBUSTIBLE);
			if (crucibleItems != null && crucibleItems.Count > 0)
			{
				Vector3 pDroppedPosition2 = mManager._MainUI.pDroppedPosition;
				float num2 = Time.timeSinceLevelLoad - mManager._MainUI.pDropStartedTime;
				Vector3 inVelocity2 = (inItemToAdd.transform.position - pDroppedPosition2) / num2;
				UpdateLiquidSplash(inVelocity2, crucibleItems);
			}
		}
		CheckForCrucibleItemOnItemAdd(inItemToAdd);
		HandleForceFlameOff();
		ProcessInitOnAddItem(inItemToAdd);
		CheckTaskRules(inFuelTimeout: false, inItemAdded: true);
		HandleCombinationsOnItemAdd(inItemToAdd);
	}

	private void ProcessInitOnAddItem(LabTestObject inItemToAdd)
	{
		if (mManager.pExperimentType == ExperimentType.MAGNETISM_LAB)
		{
			mManager.pMagnetActivated = false;
			if (mManager._MainUI._ExperimentItemMenu != null)
			{
				mManager._MainUI._ExperimentItemMenu.SetInteractive(interactive: false);
			}
			mManager.EnableClickOnPullDown(isEnable: true);
			inItemToAdd.pPickable = false;
		}
	}

	protected void HandleCombinationsOnItemAdd(LabTestObject inItemToAdd)
	{
		if (!(inItemToAdd == null))
		{
			KAUICursorManager.SetDefaultCursor("Loading", showHideSystemCursor: false);
			mManager._MainUI.ActivateCursor(UiScienceExperiment.Cursor.NONE);
			LabCombinationLoader labCombinationLoader = new LabCombinationLoader();
			labCombinationLoader.Add(LabData.pInstance.GetItemCombination(inItemToAdd, mTestItems, "DEFAULT"));
			labCombinationLoader.Add(LabData.pInstance.GetItemCombination(inItemToAdd, mTestItems, "PESTLE"));
			labCombinationLoader.Load(OnCombinationLoaded);
		}
	}

	private void OnCombinationLoaded(List<LabItemCombination> inCombinationList)
	{
		if (!LabItemCombination.pIsLoading && !TestItemLoader.IsLoading() && mManager._MainUI.pCurrentCursor != UiScienceExperiment.Cursor.PESTLE)
		{
			mManager._MainUI.ActivateCursor(UiScienceExperiment.Cursor.DEFAULT);
		}
		if (inCombinationList == null || inCombinationList.Count == 0)
		{
			return;
		}
		foreach (LabItemCombination inCombination in inCombinationList)
		{
			if (inCombination == null || inCombination.Action != "DEFAULT" || !AreCombinationItemsInCrucible(inCombination))
			{
				continue;
			}
			RemoveActionParticle(inCombination);
			if (!AddCombinationResult(LabData.pInstance.RoomTemperature, "DEFAULT") && inCombination.ParticleData != null && inCombination.ParticleData.pAdditionObject != null)
			{
				inCombination.pParticle = UnityEngine.Object.Instantiate(inCombination.ParticleData.pAdditionObject);
				if (inCombination.pParticle != null && inCombination.ParticleData.UsePosition)
				{
					inCombination.pParticle.transform.position = inCombination.ParticleData.pPosition;
				}
			}
		}
	}

	private bool AreCombinationItemsInCrucible(LabItemCombination inCombination)
	{
		if (inCombination == null || inCombination.ItemNames == null || inCombination.ItemNames.Length == 0)
		{
			return false;
		}
		string[] itemNames = inCombination.ItemNames;
		foreach (string text in itemNames)
		{
			if (string.IsNullOrEmpty(text) || !HasItemInCrucible(text))
			{
				return false;
			}
		}
		return true;
	}

	private bool HandleReplaceItemOnItemAdd(LabTestObject inItemToAdd, List<TestItemLoader.ReplaceItemDetails> inReplaceObjects)
	{
		if (inReplaceObjects == null || inReplaceObjects.Count == 0)
		{
			return true;
		}
		foreach (TestItemLoader.ReplaceItemDetails inReplaceObject in inReplaceObjects)
		{
			if (inReplaceObject == null || inReplaceObject.mReplaceItem == null || !HasItemInCrucible(inReplaceObject.mReplaceItem))
			{
				return false;
			}
		}
		foreach (TestItemLoader.ReplaceItemDetails inReplaceObject2 in inReplaceObjects)
		{
			if (inReplaceObject2 != null && !(inReplaceObject2.mReplaceItem == null))
			{
				LabTestObject mReplaceItem = inReplaceObject2.mReplaceItem;
				if (ScientificExperiment.IsSolid(mReplaceItem.pTestItem.pCategory))
				{
					RemoveTestItem(mReplaceItem, inDestroy: false);
					mReplaceItem.pCanWeight = false;
					mReplaceItem.pCanTestResistance = false;
					LabTweenScale.Scale(mReplaceItem.gameObject, inDestroyOnTimeout: true, inReplaceObject2.mScaleDownTime, Vector3.zero);
				}
				else
				{
					RemoveTestItem(mReplaceItem);
				}
			}
		}
		if (inItemToAdd.pTestItem.pCategory == LabItemCategory.LIQUID || inItemToAdd.pTestItem.pCategory == LabItemCategory.LIQUID_COMBUSTIBLE)
		{
			LabLiquidTestObject obj = inItemToAdd as LabLiquidTestObject;
			obj.pCurrentScaleTime = inItemToAdd.pTestItem.ScaleTime;
			obj.pDestroyOnScaleEnd = false;
		}
		return true;
	}

	private void CheckForCrucibleItemOnItemAdd(LabTestObject inItemToAdd)
	{
		if (string.IsNullOrEmpty(inItemToAdd.pTestItem.CrucibleItem))
		{
			return;
		}
		LabItem item = LabData.pInstance.GetItem(inItemToAdd.pTestItem.CrucibleItem);
		if (item == null)
		{
			return;
		}
		TestItemLoader testItemLoader = AddTestItem(item, LabData.pInstance.RoomTemperature, null);
		if (testItemLoader != null)
		{
			item.mNoScaling = false;
			testItemLoader.mScale = item.pScale;
			if (item.pCategory == LabItemCategory.SOLID_POWDER)
			{
				testItemLoader.mItemPositionOption = ItemPositionOption.MARKER;
			}
			else
			{
				testItemLoader.mItemPositionOption = ItemPositionOption.POSITION;
				testItemLoader.mPosition = mManager._LiquidItemDefaultPos;
			}
			testItemLoader.AddReplaceItem(inItemToAdd, 0f);
			testItemLoader.Load();
			inItemToAdd.pCrucibleItemLoader = testItemLoader;
		}
	}

	public void RemoveAllTestItems()
	{
		if (mTestItems == null || mTestItems.Count == 0)
		{
			return;
		}
		List<LabTestObject> list = new List<LabTestObject>();
		foreach (LabTestObject mTestItem in mTestItems)
		{
			list.Add(mTestItem);
		}
		foreach (LabTestObject item in list)
		{
			if (item.pTestItem != null)
			{
				RemoveTestItem(item);
			}
		}
	}

	private void RemoveActionParticle(LabItemCombination inCombination)
	{
		if (inCombination != null && inCombination.pParticle != null)
		{
			UnityEngine.Object.Destroy(inCombination.pParticle);
			inCombination.pParticle = null;
		}
	}

	private void RemoveActionParticle(List<LabItemCombination> inCombinationList)
	{
		if (inCombinationList == null || inCombinationList.Count == 0)
		{
			return;
		}
		foreach (LabItemCombination inCombination in inCombinationList)
		{
			RemoveActionParticle(inCombination);
		}
	}

	public virtual void RemoveTestItem(LabTestObject inTestItem, bool inDestroy = true)
	{
		if (inTestItem == null || LabData.pInstance == null)
		{
			return;
		}
		RemoveActionParticle(LabData.pInstance.GetItemCombination(inTestItem, mTestItems, "PESTLE"));
		RemoveActionParticle(LabData.pInstance.GetItemCombination(inTestItem, mTestItems, "DEFAULT"));
		bool flag = false;
		if (mTestItems != null && mTestItems.Count > 0 && mTestItems.Contains(inTestItem))
		{
			flag = mTestItems.Remove(inTestItem);
			HandleLiquidItems(inTestItem, inRemove: true);
		}
		if (inDestroy)
		{
			if (LabObject.pList != null && LabObject.pList.Count > 0 && LabObject.pList.Contains(inTestItem.gameObject))
			{
				LabObject.pList.Remove(inTestItem.gameObject);
			}
			UnityEngine.Object.Destroy(inTestItem.gameObject);
		}
		else
		{
			inTestItem.Freeze(inFreeze: false);
			inTestItem.Heat(inHeat: false);
		}
		if (!flag)
		{
			return;
		}
		if (inTestItem.HideItemsOnAdd() && mTestItems != null)
		{
			foreach (LabTestObject mTestItem in mTestItems)
			{
				mTestItem.Show(inShow: true);
			}
		}
		mManager.CheckForProcedureHalt("Action", "Remove");
		mManager._MainUI.UpdateCrucibleData();
		RecalculateTemperatureMultipliers();
		if (inTestItem.pCrucibleItemLoader != null)
		{
			inTestItem.pCrucibleItemLoader.mRemove = true;
		}
	}

	public virtual void DoUpdate()
	{
		if (mPaused)
		{
			return;
		}
		CalculateCurrentTemperature();
		if (mHeating)
		{
			mHeatTimer -= Time.deltaTime;
			if (mHeatTimer <= 0f)
			{
				mManager.BreathFlame(inBreath: false);
			}
		}
		CheckTaskRules();
		CheckTaskRequirements();
	}

	public void Heat()
	{
		if (mManager != null)
		{
			if (mManager._MainUI != null)
			{
				mManager._MainUI.HideUserPrompt();
			}
			mManager.CheckForProcedureHalt("Action", "Heat");
		}
		if (mFreezing)
		{
			Freeze(inFreeze: false);
		}
		mHeating = true;
		mHeatTimer = mManager.pHeatTime;
		if (mTestItems == null)
		{
			return;
		}
		foreach (LabTestObject mTestItem in mTestItems)
		{
			if (mTestItem != null)
			{
				mTestItem.Heat();
			}
		}
	}

	public void StopHeat()
	{
		mHeating = false;
		if (mTestItems == null || mTestItems.Count <= 0)
		{
			return;
		}
		LabTestObject[] array = mTestItems.ToArray();
		if (array == null || array.Length == 0)
		{
			return;
		}
		LabTestObject[] array2 = array;
		foreach (LabTestObject labTestObject in array2)
		{
			if (labTestObject != null)
			{
				labTestObject.Heat(inHeat: false);
			}
		}
	}

	public void Freeze(bool inFreeze = true)
	{
		if (inFreeze && mHeating)
		{
			mManager.BreathFlame(inBreath: false);
		}
		if (mManager._IceSet != null)
		{
			mManager._IceSet.SetActive(inFreeze);
		}
		mFreezing = inFreeze;
		if (mFreezing)
		{
			if (!mManager.CheckForProcedureHalt("Action", "Freeze"))
			{
				SnChannel.Play(mManager._IceMeltSFX, "IceMelt_Pool", inForce: true, null);
			}
		}
		else
		{
			SnChannel.StopPool("Default_Pool3");
		}
		if (mTestItems == null || mTestItems.Count <= 0)
		{
			return;
		}
		foreach (LabTestObject mTestItem in mTestItems)
		{
			if (mTestItem != null)
			{
				mTestItem.Freeze(mFreezing);
			}
		}
	}

	public bool AddWater(LabItem inWaterTestItem)
	{
		if (inWaterTestItem == null || mLoadingWater)
		{
			return false;
		}
		if (inWaterTestItem != null && mTestItems != null && mTestItems.Count > 0)
		{
			foreach (LabTestObject mTestItem in mTestItems)
			{
				if (mTestItem != null && mTestItem.pTestItem != null && mTestItem.pTestItem.Name == inWaterTestItem.Name)
				{
					return false;
				}
			}
		}
		inWaterTestItem.mNoScaling = false;
		if (mManager != null)
		{
			mManager.EnableUI(inEnable: false);
		}
		TestItemLoader testItemLoader = new TestItemLoader(inWaterTestItem, LabData.pInstance.RoomTemperature, OnWaterDownloaded, null);
		if (testItemLoader != null)
		{
			testItemLoader.mItemPositionOption = ItemPositionOption.POSITION;
			testItemLoader.mPosition = mManager._LiquidItemDefaultPos;
			testItemLoader.mScale = Vector3.zero;
			testItemLoader.Load();
		}
		if (mHeating)
		{
			mManager.BreathFlame(inBreath: false);
		}
		if (mFreezing)
		{
			Freeze(inFreeze: false);
		}
		return true;
	}

	public int GetUniqueTestItemNameID()
	{
		if (mTestItems == null)
		{
			return 1;
		}
		return mTestItems.Count + 1;
	}

	public void AddWaterReal(LabItem inLabItem, GameObject inGameObj, LabItem inParent)
	{
		GameObject gameObject = UnityEngine.Object.Instantiate(inGameObj, Vector3.up * 5000f, Quaternion.identity);
		LabTestObject component = gameObject.GetComponent<LabTestObject>();
		gameObject.name = "[" + inLabItem.Name + "-" + GetUniqueTestItemNameID() + "]";
		component.Initialize(inLabItem, mManager);
		AddTestItem(gameObject, ItemPositionOption.POSITION, mManager._LiquidItemDefaultPos, Quaternion.identity, Vector3.zero, inParent, null, null);
	}

	public void Mix()
	{
		if (mTestItems != null && mTestItems.Count >= 2)
		{
			mMixDone = true;
			if (mHeating)
			{
				mManager.BreathFlame(inBreath: false);
			}
			if (mFreezing)
			{
				Freeze(inFreeze: false);
			}
			AddCombinationResult(LabData.pInstance.RoomTemperature, "PESTLE");
		}
	}

	protected virtual bool AddCombinationResult(float startTemperature, string action, bool noScaling = true)
	{
		LabItemCombination combination = LabData.pInstance.GetCombination(mTestItems, action);
		if (combination != null)
		{
			RemoveActionParticle(combination);
			LabItem item = LabData.pInstance.GetItem(combination.ResultItemName);
			if (item != null)
			{
				TestItemLoader testItemLoader = AddTestItem(item, startTemperature, OnMixItemAdded);
				if (testItemLoader != null)
				{
					if (noScaling)
					{
						testItemLoader.mScale = item.pScale;
						item.mNoScaling = true;
					}
					testItemLoader.mItemPositionOption = ItemPositionOption.POSITION;
					testItemLoader.mPosition = mManager._LiquidItemDefaultPos;
					foreach (LabTestObject mTestItem in mTestItems)
					{
						testItemLoader.AddReplaceItem(mTestItem, 2f);
					}
					testItemLoader.Load();
					return true;
				}
			}
		}
		return false;
	}

	protected virtual void OnMixItemAdded(LabTestObject inObject)
	{
		StopMixing();
	}

	public void ShowMixingEffect()
	{
		ShowLiquidMixingDefaultEffect();
		LabItemCombination combination = LabData.pInstance.GetCombination(mTestItems, "PESTLE");
		if (combination != null && combination.ParticleData != null && combination.ParticleData.pAdditionObject != null && combination.pParticle == null)
		{
			combination.pParticle = UnityEngine.Object.Instantiate(combination.ParticleData.pAdditionObject);
			if (combination.ParticleData.UsePosition)
			{
				combination.pParticle.transform.position = combination.ParticleData.pPosition;
			}
		}
	}

	public void ShowLiquidMixingDefaultEffect()
	{
		if (mTestItems == null || mTestItems.Count == 0 || (pMixingEffect != null && pMixingEffect.GetComponent<Animation>().isPlaying))
		{
			return;
		}
		LabTestObject labTestObject = null;
		LabTestObject labTestObject2 = null;
		foreach (LabTestObject mTestItem in mTestItems)
		{
			if (!(mTestItem != null) || mTestItem.pTestItem == null || (mTestItem.pTestItem.pCategory != LabItemCategory.LIQUID && mTestItem.pTestItem.pCategory != LabItemCategory.LIQUID_COMBUSTIBLE) || LabTweenScale.IsScaling(mTestItem.gameObject))
			{
				continue;
			}
			if (labTestObject == null)
			{
				labTestObject = mTestItem;
				continue;
			}
			if (labTestObject2 == null)
			{
				labTestObject2 = mTestItem;
				continue;
			}
			break;
		}
		if (labTestObject == null && labTestObject2 != null)
		{
			labTestObject = labTestObject2;
		}
		else if (labTestObject != null && labTestObject2 == null)
		{
			labTestObject2 = labTestObject;
		}
		if (labTestObject != null && labTestObject2 != null)
		{
			if (labTestObject.pTestItem.LiquidPriority < labTestObject2.pTestItem.LiquidPriority)
			{
				LabTestObject labTestObject3 = labTestObject;
				labTestObject = labTestObject2;
				labTestObject2 = labTestObject3;
			}
			ShowLiquidMixingDefaultEffect(labTestObject as LabLiquidTestObject, labTestObject2 as LabLiquidTestObject);
		}
	}

	private void ShowLiquidMixingDefaultEffect(LabLiquidTestObject inTestObject1, LabLiquidTestObject inTestObject2)
	{
		if (inTestObject1 == null || inTestObject2 == null || inTestObject1.pTestItem == null || inTestObject2.pTestItem == null)
		{
			return;
		}
		if (pMixingEffect != null)
		{
			if (pMixingEffect.GetComponent<Animation>().isPlaying)
			{
				return;
			}
			UnityEngine.Object.Destroy(pMixingEffect);
		}
		pMixingEffect = null;
		pMixingEffect = UnityEngine.Object.Instantiate(mManager._MixingEffect);
		Transform transform = pMixingEffect.transform.Find("MixingBowlEffectMesh");
		if (transform != null)
		{
			Material[] materials = new Material[2] { inTestObject1.pMaterial, inTestObject2.pMaterial };
			transform.GetComponent<Renderer>().materials = materials;
		}
		if (inTestObject1.pMaterial != null)
		{
			Transform transform2 = pMixingEffect.transform.Find("PfPrtBlobs");
			if (transform2 != null)
			{
				transform2.GetComponent<Renderer>().material.SetColor("_TintColor", inTestObject1.pTestItem.LiquidSplashParticleColor);
			}
		}
		if (inTestObject2.pMaterial != null)
		{
			Transform transform3 = pMixingEffect.transform.Find("PfPrtSplash");
			if (transform3 != null)
			{
				transform3.GetComponent<Renderer>().material.SetColor("_TintColor", inTestObject2.pTestItem.LiquidSplashParticleColor);
			}
		}
	}

	private void UpdateLiquidSplash(Vector3 inVelocity, List<LabTestObject> inLiquidObjects)
	{
		if (inVelocity.y > -1.5f || inLiquidObjects == null || inLiquidObjects.Count == 0 || (mManager.pSplash != null && mManager.pSplash.isEmitting))
		{
			return;
		}
		LabTestObject labTestObject = null;
		foreach (LabTestObject inLiquidObject in inLiquidObjects)
		{
			if (inLiquidObject != null && !LabTweenScale.IsScaling(inLiquidObject.gameObject))
			{
				labTestObject = inLiquidObject;
				break;
			}
		}
		if (!(labTestObject == null))
		{
			if (mManager.pSplash == null)
			{
				GameObject gameObject = UnityEngine.Object.Instantiate(mManager._Splash.gameObject);
				mManager.pSplash = gameObject.GetComponent<ParticleSystem>();
			}
			mManager.pSplash.Play();
			mManager.pSplash.GetComponent<Renderer>().material.SetColor("_TintColor", labTestObject.pTestItem.LiquidSplashParticleColor);
			mManager.StartCoroutine(DestroySplash(0.7f));
		}
	}

	private IEnumerator DestroySplash(float inSeconds)
	{
		yield return new WaitForSeconds(inSeconds);
		if (mManager.pSplash != null)
		{
			mManager.pSplash.Stop();
		}
	}

	public void StopMixing()
	{
		if (mTestItems != null && mTestItems.Count > 0)
		{
			foreach (LabTestObject mTestItem in mTestItems)
			{
				if (mTestItem != null)
				{
					RemoveActionParticle(LabData.pInstance.GetItemCombination(mTestItem, mTestItems, "PESTLE"));
				}
			}
		}
		if (pMixingEffect != null)
		{
			UnityEngine.Object.Destroy(pMixingEffect);
			pMixingEffect = null;
		}
		mManager._MainUI.pMixing = false;
	}

	public static float GotoTemperature(float inTemperature, float inInitialTemperature, float inTime, float inCoolingConstant, float inWarmingConstant)
	{
		if (inInitialTemperature - inTemperature >= 0f)
		{
			return Mathf.Max(inTemperature, inInitialTemperature - inTime * inCoolingConstant);
		}
		return Mathf.Min(inTemperature, inInitialTemperature + inTime * inWarmingConstant);
	}

	public virtual void Reset()
	{
		if (mManager.pExperimentType == ExperimentType.MAGNETISM_LAB && mManager._MainUI._ExperimentItemMenu != null)
		{
			mManager._MainUI._ExperimentItemMenu.SetInteractive(interactive: true);
		}
		mManager.BreathFlame(inBreath: false);
		Freeze(inFreeze: false);
		RemoveAllTestItems();
		mClockTimer = 0f;
		mTemperature = LabData.pInstance.RoomTemperature;
		if (mManager != null)
		{
			if (mManager._MainUI != null)
			{
				mManager._MainUI.UpdateCrucibleData();
				mManager._MainUI.ShowExperimentDirection();
				mManager._MainUI.HideUserPrompt();
				mManager._MainUI.UpdateCrucibleData();
			}
			if (mManager._WaterStream != null)
			{
				mManager._WaterStream.Stop();
				if (mManager._AddWaterSFX != null)
				{
					SnChannel.StopPool("Default_Pool3");
				}
				if (mManager._WaterSplashSteam != null)
				{
					mManager._WaterSplashSteam.Stop();
				}
			}
			if (mManager._AcidStream != null)
			{
				ParticleSystem.EmissionModule emission = mManager._AcidStream.emission;
				emission.enabled = false;
			}
			if (mManager._BaseStream != null)
			{
				ParticleSystem.EmissionModule emission2 = mManager._BaseStream.emission;
				emission2.enabled = false;
			}
		}
		SnChannel.StopPool("Default_Pool2");
		if (mManager.pSplash != null)
		{
			mManager.pSplash.Stop();
		}
		if (LabData.pInstance != null)
		{
			LabData.pInstance.ResetStartTemperature();
		}
	}

	private void OnWaterDownloaded(TestItemLoader inLoader)
	{
		mLoadingWater = false;
		mManager.OnWaterLoaded(inLoader.mTestItem, inLoader.mLoadedObj, inLoader.mParentItem);
	}

	private void OnTestItemDownloaded(TestItemLoader inLoader)
	{
		if (!LabItemCombination.pIsLoading)
		{
			mManager._MainUI.ActivateCursor(UiScienceExperiment.Cursor.DEFAULT);
		}
		if (mManager != null)
		{
			mManager.EnableUI(inEnable: true);
		}
		if (inLoader.mTestItem != null && !inLoader.mRemove)
		{
			GameObject gameObject = UnityEngine.Object.Instantiate(inLoader.mLoadedObj);
			LabTestObject component = gameObject.GetComponent<LabTestObject>();
			if (component != null)
			{
				component.Initialize(inLoader.mTestItem, mManager);
				gameObject.name = "[" + inLoader.mTestItem.Name + "-" + GetUniqueTestItemNameID() + "]";
				AddTestItem(gameObject, inLoader.mItemPositionOption, inLoader.mPosition, inLoader.mRotation, inLoader.mScale, inLoader.mParentItem, inLoader.mReplaceItems, (CrucibleItemAddedCallback)inLoader.mUserData);
			}
		}
	}

	public Transform GetFreeMarker(bool inForPowderObject = false)
	{
		Transform[] array = null;
		array = ((!inForPowderObject) ? mManager._CrucibleMarkers : mManager._SolidPowderMarkers);
		if (mManager == null || array == null || array.Length == 0)
		{
			return null;
		}
		if (mTestItems == null || mTestItems.Count == 0)
		{
			return array[0];
		}
		Transform[] array2 = array;
		foreach (Transform marker in array2)
		{
			if (mTestItems.Find((LabTestObject x1) => x1.pMarker == marker) == null)
			{
				return marker;
			}
		}
		return array[0];
	}

	public bool HasCategoryItem(LabItemCategory inCategory)
	{
		if (mTestItems == null || mTestItems.Count == 0)
		{
			return false;
		}
		foreach (LabTestObject mTestItem in mTestItems)
		{
			if (mTestItem != null && mTestItem.pTestItem != null && mTestItem.pTestItem.pCategory == inCategory)
			{
				return true;
			}
		}
		return false;
	}

	public bool HasItemInCrucible(Transform inObject)
	{
		if (mTestItems == null || mTestItems.Count == 0)
		{
			return false;
		}
		foreach (LabTestObject mTestItem in mTestItems)
		{
			if (mTestItem != null && mTestItem.transform == inObject)
			{
				return true;
			}
		}
		return false;
	}

	public virtual bool HasItemInCrucible(string inItemName)
	{
		if (mTestItems == null || mTestItems.Count == 0)
		{
			return false;
		}
		foreach (LabTestObject mTestItem in mTestItems)
		{
			if (mTestItem != null && mTestItem.pTestItem != null && mTestItem.pTestItem.Name == inItemName)
			{
				return true;
			}
		}
		return false;
	}

	public List<LabTestObject> GetCrucibleItems(LabItemCategory inCategory1, LabItemCategory inCategory2)
	{
		if (mTestItems == null || mTestItems.Count == 0)
		{
			return null;
		}
		List<LabTestObject> list = null;
		foreach (LabTestObject mTestItem in mTestItems)
		{
			if (mTestItem != null && mTestItem.pTestItem != null && (mTestItem.pTestItem.pCategory == inCategory1 || mTestItem.pTestItem.pCategory == inCategory2))
			{
				if (list == null)
				{
					list = new List<LabTestObject>();
				}
				list.Add(mTestItem);
			}
		}
		return list;
	}

	public List<LabTestObject> GetCrucibleItems(LabItemCategory inCategory)
	{
		if (mTestItems == null || mTestItems.Count == 0)
		{
			return null;
		}
		List<LabTestObject> list = null;
		foreach (LabTestObject mTestItem in mTestItems)
		{
			if (mTestItem != null && mTestItem.pTestItem != null && mTestItem.pTestItem.pCategory == inCategory)
			{
				if (list == null)
				{
					list = new List<LabTestObject>();
				}
				list.Add(mTestItem);
			}
		}
		return list;
	}

	public List<LabTestObject> GetCrucibleItems(string inItemName, bool inConsiderInHandObject = false)
	{
		if (mTestItems == null || mTestItems.Count == 0)
		{
			return null;
		}
		List<LabTestObject> list = null;
		foreach (LabTestObject mTestItem in mTestItems)
		{
			if (mTestItem != null && mTestItem.pTestItem != null && mTestItem.pTestItem.Name == inItemName)
			{
				if (list == null)
				{
					list = new List<LabTestObject>();
				}
				list.Add(mTestItem);
			}
		}
		if (inConsiderInHandObject && mManager._MainUI.pObjectInHand != null && mManager._MainUI.pObjectInHand.pType == UiScienceExperiment.InHandObjectType.TEST_ITEM)
		{
			LabTestObject component = mManager._MainUI.pObjectInHand.pObject.GetComponent<LabTestObject>();
			if (component != null && component.pTestItem != null && component.pTestItem.Name == inItemName)
			{
				if (list == null)
				{
					list = new List<LabTestObject>();
				}
				list.Add(component);
			}
		}
		return list;
	}

	private List<LabTestObject> GetCrucibleItems(string inItemName, string inStateName)
	{
		if (mTestItems == null || mTestItems.Count == 0)
		{
			return null;
		}
		List<LabTestObject> list = null;
		foreach (LabTestObject mTestItem in mTestItems)
		{
			if (mTestItem != null && mTestItem.pTestItem != null && mTestItem.pTestItem.Name == inItemName && mTestItem.pState != null && mTestItem.pState.Name == inStateName)
			{
				if (list == null)
				{
					list = new List<LabTestObject>();
				}
				list.Add(mTestItem);
			}
		}
		return list;
	}

	private void HeatReleaseConfirmationCallback(string inID)
	{
	}

	public bool CanHeat()
	{
		return !mHeating;
	}

	protected virtual void RecalculateTemperatureMultipliers()
	{
		float num = float.NegativeInfinity;
		float num2 = float.NegativeInfinity;
		float num3 = float.NegativeInfinity;
		float num4 = float.NegativeInfinity;
		if (mTestItems != null && mTestItems.Count > 0)
		{
			foreach (LabTestObject mTestItem in mTestItems)
			{
				if (mTestItem != null)
				{
					num = Mathf.Max(num, mTestItem.pHeatMultiplier);
					num2 = Mathf.Max(num2, mTestItem.pTestItem.FreezeRate);
					num3 = Mathf.Max(num3, mTestItem.pTestItem.WarmingRate);
					num4 = Mathf.Max(num4, mTestItem.pTestItem.CoolingRate);
				}
			}
		}
		mCurrentHeatMultiplier = ((num == float.NegativeInfinity) ? mManager._WarmingConstant : num);
		mCurrentFreezeMultiplier = ((num2 == float.NegativeInfinity) ? mManager._FreezeRate : num2);
		mCurrentWarmingRate = ((num3 == float.NegativeInfinity) ? mManager._WarmingConstant : num3);
		mCurrentCoolingRate = ((num4 == float.NegativeInfinity) ? mManager._CoolingConstant : num4);
	}

	public void CalculateCurrentTemperature()
	{
		if (LabData.pInstance == null || TestItemLoader.IsLoading() || pIsMixing)
		{
			return;
		}
		bool flag = false;
		if (mTestItems != null && mTestItems.Count > 0)
		{
			float num = float.NegativeInfinity;
			foreach (LabTestObject mTestItem in mTestItems)
			{
				if (mTestItem != null && num < mTestItem.pTemperature)
				{
					num = mTestItem.pTemperature;
					mTemperatureRefObject = mTestItem;
					flag = true;
				}
			}
			if (flag)
			{
				mTemperature = GotoTemperature(num, mTemperature, Time.deltaTime, mCurrentFreezeMultiplier, mCurrentHeatMultiplier);
				return;
			}
		}
		if (mHeating)
		{
			mTemperature += Time.deltaTime * mCurrentHeatMultiplier;
		}
		else if (mFreezing)
		{
			mTemperature -= Time.deltaTime * pCurrentFreezeMultiplier;
		}
		else
		{
			mTemperature = GotoTemperature(LabData.pInstance.RoomTemperature, mTemperature, Time.deltaTime, mCurrentCoolingRate, mCurrentWarmingRate);
		}
	}

	public bool HasItemInCrucible(LabTestObject inObject)
	{
		if (mTestItems != null)
		{
			return mTestItems.Contains(inObject);
		}
		return false;
	}

	public void CheckTaskRules(bool inFuelTimeout = false, bool inItemAdded = false)
	{
		if (mManager == null || mManager.pExperiment == null || mManager.pExperiment.Tasks == null || mManager.pExperiment.Tasks.Length == 0 || TestItemLoader.IsLoading() || pIsMixing)
		{
			return;
		}
		LabTask[] tasks = mManager.pExperiment.Tasks;
		foreach (LabTask labTask in tasks)
		{
			if (labTask == null || labTask.Rules == null || labTask.Rules.Length == 0)
			{
				continue;
			}
			bool success = true;
			LabTaskRule[] rules = labTask.Rules;
			foreach (LabTaskRule labTaskRule in rules)
			{
				if (labTaskRule != null)
				{
					success = true;
					ProcessRule(labTaskRule, inFuelTimeout, inItemAdded, ref success);
					if (!success)
					{
						break;
					}
				}
			}
			if (success && !labTask.pMetRuleConditions)
			{
				if (mManager._WaterStream.isEmitting)
				{
					break;
				}
				if (CanDragonGetExcited(labTask))
				{
					mManager.PlayDragonAnim("LabExcited", labTask.pDone);
					SnChannel snChannel = SnChannel.Play(mManager._DragonExcitedSFX, "Default_Pool2", inForce: true, null);
					if (snChannel != null)
					{
						snChannel.pLoop = false;
					}
				}
				mManager.DisableTime();
				if (!labTask.pDone)
				{
					mManager._MainUI.ShowUserPromptText(mManager._RecordInJournalText.GetLocalizedString(), inClickable: true, TaskCompleteUserPromptCallback, inShowCloseBtn: false, inSetTimerInteractive: false, labTask);
					labTask.pMetRuleConditions = success;
					break;
				}
			}
			labTask.pMetRuleConditions = success;
		}
	}

	protected virtual void ProcessRule(LabTaskRule rule, bool inFuelTimeout, bool inItemAdded, ref bool success)
	{
		switch (rule.Name)
		{
		case "ItemName":
			if (!HasItemInCrucible(rule.Value))
			{
				success = false;
			}
			break;
		case "ItemAdded":
			if (!inItemAdded || !HasItemInCrucible(rule.Value))
			{
				success = false;
			}
			break;
		case "ItemState":
			success = false;
			if (mTestItems == null || mTestItems.Count == 0)
			{
				break;
			}
			{
				foreach (LabTestObject mTestItem in mTestItems)
				{
					if (mTestItem != null && mTestItem.pState != null && mTestItem.pState.Name == rule.Value)
					{
						success = true;
						break;
					}
				}
				break;
			}
		case "Time":
			if (rule.Value == "SystemEnabled" && (!mManager.pShowClock || !mManager.pTimeEnabled || mManager.pUserEnabledTimer))
			{
				success = false;
			}
			break;
		case "Temperature":
		{
			if (rule.Value == "RoomTemperature")
			{
				if (mTemperature != LabData.pInstance.RoomTemperature)
				{
					success = false;
				}
				break;
			}
			string[] array = rule.Value.Split(new string[1] { ".." }, StringSplitOptions.None);
			if (array.Length < 2)
			{
				success = false;
				break;
			}
			float num = float.Parse(array[0]);
			float num2 = float.Parse(array[1]);
			if (mTemperature < num || mTemperature > num2)
			{
				success = false;
			}
			break;
		}
		case "ItemCount":
		{
			if (mTestItems == null || mTestItems.Count == 0)
			{
				success = false;
				break;
			}
			float num3 = UtStringUtil.Parse(rule.Value, -1f);
			if (num3 == -1f || (float)mTestItems.Count != num3)
			{
				success = false;
			}
			break;
		}
		case "Parent":
		{
			success = false;
			List<LabTestObject> crucibleItems3 = GetCrucibleItems(rule.Value);
			if (crucibleItems3 == null || crucibleItems3.Count <= 0)
			{
				break;
			}
			{
				foreach (LabTestObject item in crucibleItems3)
				{
					if (item != null && item.pParentItem != null && item.pParentItem.Name == rule.AdditionalValue)
					{
						success = true;
						break;
					}
				}
				break;
			}
		}
		case "CompletedAction":
			if (rule.Value == "Pestle" && !mMixDone)
			{
				success = false;
			}
			break;
		case "Timeout":
			if (rule.Value == "Fuel" && !inFuelTimeout)
			{
				success = false;
			}
			break;
		case "Tool":
			switch (rule.Value)
			{
			case "THERMOMETER":
				if (!mManager._MainUI.IsVisible(LabTool.THERMOMETER))
				{
					success = false;
				}
				break;
			case "WEIGHING MACHINE":
				success = false;
				if (mManager._MainUI.IsVisible(LabTool.WEIGHINGMACHINE) && mManager._MainUI.pWeighingMachine != null && Mathf.Abs(mManager._MainUI.pWeighingMachine.pCurrentWeightPoint - LabObject.GetWeight(inCurrentWeight: false)) <= 0.01f)
				{
					success = true;
				}
				break;
			case "OHMMETER":
				success = false;
				if (mManager._MainUI.IsVisible(LabTool.OHMMETER) && mManager._MainUI.pOhmMeter != null && mManager._MainUI.pElectrifyDone && Mathf.Abs(mManager._MainUI.pOhmMeter.pCurrentResistanceReading - mManager._MainUI._OhmMeterInfo._NeedleMinReading) <= 0.01f)
				{
					success = true;
				}
				break;
			}
			break;
		case "ItemInHand":
			if (mManager._MainUI == null || mManager._MainUI.pObjectInHand == null || mManager._MainUI.pObjectInHand.pType != 0)
			{
				success = false;
			}
			break;
		case "InteractionDone":
		{
			success = false;
			List<LabTestObject> crucibleItems = GetCrucibleItems(rule.Value, inConsiderInHandObject: true);
			if (crucibleItems == null || crucibleItems.Count <= 0)
			{
				break;
			}
			{
				foreach (LabTestObject item2 in crucibleItems)
				{
					List<LabTestObject> crucibleItems2 = GetCrucibleItems(rule.AdditionalValue);
					if (crucibleItems2 == null || crucibleItems2.Count == 0)
					{
						continue;
					}
					foreach (LabTestObject item3 in crucibleItems2)
					{
						if (item2 != null && item2.IsInteractionDone(item3))
						{
							success = true;
							break;
						}
					}
					if (success)
					{
						break;
					}
				}
				break;
			}
		}
		case "MagnetActivated":
			if (!mManager.pMagnetActivated || !HasItemInCrucible(rule.Value))
			{
				success = false;
			}
			break;
		default:
			success = false;
			break;
		}
	}

	protected virtual bool CanDragonGetExcited(LabTask task)
	{
		if (task.pDone)
		{
			if (task.pDone)
			{
				return task.PlayExciteAlways;
			}
			return false;
		}
		return true;
	}

	private void CheckTaskRequirements()
	{
		if (mManager == null || mManager.pExperiment == null || mManager.pExperiment.Tasks == null || mManager.pExperiment.Tasks.Length == 0 || mManager.pCrucibleItemCount != 1)
		{
			return;
		}
		LabTask[] tasks = mManager.pExperiment.Tasks;
		foreach (LabTask labTask in tasks)
		{
			if (labTask == null || labTask.pDone || labTask.Requirement == null || !labTask.Requirement.pActive)
			{
				continue;
			}
			LabTaskRequirement requirement = labTask.Requirement;
			if (requirement == null)
			{
				continue;
			}
			List<LabTestObject> crucibleItems = GetCrucibleItems(requirement.ItemName);
			if (crucibleItems == null || crucibleItems.Count == 0)
			{
				continue;
			}
			LabTestObject labTestObject = null;
			foreach (LabTestObject item in crucibleItems)
			{
				if (item.pState != null && item.pState.Name == requirement.State)
				{
					labTestObject = item;
					break;
				}
			}
			if (!(labTestObject == null) && requirement.Tool == "CLOCK" && mManager._MainUI.IsVisible(LabTool.CLOCK))
			{
				mManager._MainUI.pListenClickOnTimer = true;
				labTestObject.pTemperature = labTestObject.pState.Temperature;
				mTemperature = labTestObject.pState.Temperature;
				mManager.pTimerActivatedTask = labTask;
				mManager._MainUI.ShowUserPromptText(mManager._TapTimeToStartText.GetLocalizedString(), inClickable: false, null, inShowCloseBtn: true, inSetTimerInteractive: true, labTask);
				requirement.pActive = false;
			}
		}
	}

	private void TaskCompleteUserPromptCallback(string inID, object inUserData)
	{
		if (!string.IsNullOrEmpty(inID))
		{
			LabTask labTask = inUserData as LabTask;
			if (inID != "Close" && labTask != null)
			{
				labTask.pDone = true;
				mManager.OnExperimentTaskDone(labTask);
			}
		}
	}

	public virtual void InitStateChange(LabTestObject inItem, OnStateChange onStateChange)
	{
		onStateChange?.Invoke();
	}

	public virtual void OnStateChanged(LabTestObject inItem)
	{
		if (inItem == null)
		{
			return;
		}
		if (mManager.pExperiment != null && mManager.pExperiment.Tasks != null && mManager.pExperiment.Tasks.Length != 0 && inItem.pTestItem != null)
		{
			LabTask[] tasks = mManager.pExperiment.Tasks;
			foreach (LabTask labTask in tasks)
			{
				if (labTask != null && labTask.Requirement != null && labTask.Requirement.ItemName == inItem.pTestItem.Name)
				{
					labTask.Requirement.pActive = true;
				}
			}
		}
		CheckTaskRules();
	}

	public void OnExit()
	{
		RemoveAllTestItems();
		TestItemLoader.UnloadAll();
	}

	public virtual void OnLabTaskUpdated(LabTask inTask)
	{
	}

	private void HandleLiquidItems(LabTestObject inAddedLiquidObj, bool inRemove)
	{
		if (mTestItems == null || inAddedLiquidObj == null || inAddedLiquidObj.pTestItem == null || (inAddedLiquidObj.pTestItem.pCategory != LabItemCategory.LIQUID && inAddedLiquidObj.pTestItem.pCategory != LabItemCategory.LIQUID_COMBUSTIBLE))
		{
			return;
		}
		LabLiquidTestObject labLiquidTestObject = inAddedLiquidObj as LabLiquidTestObject;
		float inDuration = ((LabLiquidTestObject)inAddedLiquidObj).pCurrentScaleTime;
		if (inAddedLiquidObj.pTestItem.Name == mManager._WaterItemName)
		{
			inDuration = mManager._WateringTime;
		}
		List<LabTestObject> list = GetCrucibleItems(LabItemCategory.LIQUID, LabItemCategory.LIQUID_COMBUSTIBLE);
		if (list == null || list.Count == 0)
		{
			if (inRemove)
			{
				LabTweenScale.Scale(labLiquidTestObject.gameObject, inDestroyOnTimeout: true, inDuration, Vector3.zero);
			}
			return;
		}
		if (inRemove)
		{
			if (list == null)
			{
				list = new List<LabTestObject>();
			}
			if (!list.Contains(inAddedLiquidObj))
			{
				list.Add(inAddedLiquidObj);
			}
			list.Sort(new SortLiquidOnPriority(mManager));
			int num = list.FindIndex((LabTestObject obj) => obj == inAddedLiquidObj);
			Vector3 pScale = labLiquidTestObject.pTestItem.pScale;
			if (num == list.Count - 1)
			{
				pScale.x = 0f;
				pScale.z = 0f;
			}
			LabTweenScale.Scale(labLiquidTestObject.gameObject, inDestroyOnTimeout: true, inDuration, pScale);
			list.Remove(inAddedLiquidObj);
		}
		list.Sort(new SortLiquidOnPriority(mManager));
		int num2 = 0;
		for (int i = 0; i < list.Count; i++)
		{
			if (i == 0)
			{
				num2++;
			}
			else if (list[i - 1].pTestItem.LiquidPriority != list[i].pTestItem.LiquidPriority)
			{
				num2++;
			}
		}
		if (num2 == 1)
		{
			for (int j = 0; j < list.Count; j++)
			{
				LabLiquidTestObject labLiquidTestObject2 = list[j] as LabLiquidTestObject;
				if (labLiquidTestObject2 != null)
				{
					if (labLiquidTestObject2.pTestItem.mNoScaling)
					{
						labLiquidTestObject2.transform.localScale = labLiquidTestObject2.pTestItem.pScale;
						labLiquidTestObject2.pTestItem.mNoScaling = false;
					}
					LabTweenScale.Scale(labLiquidTestObject2.gameObject, labLiquidTestObject2.pDestroyOnScaleEnd, inDuration, labLiquidTestObject2.pTestItem.pScale);
				}
			}
			return;
		}
		Vector3 inEndScale = Vector3.one;
		int num3 = -1;
		for (int k = 0; k < list.Count; k++)
		{
			LabLiquidTestObject labLiquidTestObject3 = list[k] as LabLiquidTestObject;
			if (k == 0 && list[0] == inAddedLiquidObj)
			{
				labLiquidTestObject3.transform.localScale = labLiquidTestObject3.pTestItem.pScale;
			}
			else
			{
				Vector3 localScale = labLiquidTestObject3.transform.localScale;
				localScale.y = labLiquidTestObject3.pTestItem.pScale.y;
				labLiquidTestObject3.transform.localScale = localScale;
			}
			if (k == 0 || num3 != labLiquidTestObject3.pTestItem.LiquidPriority)
			{
				inEndScale = labLiquidTestObject3.pTestItem.pScale;
				inEndScale.x = 1f - (float)k / (float)num2;
				inEndScale.z = 1f - (float)k / (float)num2;
			}
			num3 = labLiquidTestObject3.pTestItem.LiquidPriority;
			LabTweenScale.Scale(labLiquidTestObject3.gameObject, labLiquidTestObject3.pDestroyOnScaleEnd, inDuration, inEndScale);
		}
	}

	public void UpdateLiquidPosition()
	{
		List<LabTestObject> crucibleItems = GetCrucibleItems(LabItemCategory.LIQUID, LabItemCategory.LIQUID_COMBUSTIBLE);
		if (crucibleItems == null || crucibleItems.Count == 0)
		{
			return;
		}
		crucibleItems.Sort(new SortLiquidOnPriority(mManager));
		Vector3 position = Vector3.zero;
		int num = -1;
		for (int i = 0; i < crucibleItems.Count; i++)
		{
			LabLiquidTestObject labLiquidTestObject = crucibleItems[i] as LabLiquidTestObject;
			if (i != 0 && num == labLiquidTestObject.pTestItem.LiquidPriority)
			{
				labLiquidTestObject.transform.position = position;
			}
			else
			{
				position = mManager._LiquidItemDefaultPos;
				position.y += (float)i * 0.001f;
				labLiquidTestObject.transform.position = position;
			}
			num = labLiquidTestObject.pTestItem.LiquidPriority;
		}
	}

	public void OnJournalActivated()
	{
		if (mTestItems == null || mTestItems.Count <= 0)
		{
			return;
		}
		foreach (LabTestObject mTestItem in mTestItems)
		{
			if (mTestItem != null && mTestItem.rigidbody != null)
			{
				mTestItem.rigidbody.isKinematic = true;
			}
		}
	}

	public void OnJournalExit()
	{
		if (mTestItems == null || mTestItems.Count <= 0)
		{
			return;
		}
		foreach (LabTestObject mTestItem in mTestItems)
		{
			if (mTestItem != null && mTestItem.rigidbody != null)
			{
				mTestItem.rigidbody.isKinematic = false;
			}
		}
	}

	private void HandleForceFlameOff()
	{
		if (mTestItems == null || mTestItems.Count == 0)
		{
			return;
		}
		List<LabTestObject> crucibleItems = GetCrucibleItems(LabItemCategory.SOLID_COMBUSTIBLE, LabItemCategory.LIQUID_COMBUSTIBLE);
		if (crucibleItems == null || crucibleItems.Count == 0)
		{
			return;
		}
		List<LabTestObject> crucibleItems2 = GetCrucibleItems(LabItemCategory.LIQUID);
		if (crucibleItems2 == null || crucibleItems2.Count == 0)
		{
			return;
		}
		LabTestObject[] array = mTestItems.ToArray();
		foreach (LabTestObject labTestObject in array)
		{
			if (labTestObject != null)
			{
				labTestObject.ProcessLiquidInCrucible();
			}
		}
	}

	public void OnTutorialDone()
	{
		if (mTestItems == null || mTestItems.Count <= 0)
		{
			return;
		}
		foreach (LabTestObject mTestItem in mTestItems)
		{
			if (mTestItem != null && mTestItem.pTestItem != null)
			{
				mTestItem.pPickable = mTestItem.pTestItem.Pickable;
			}
		}
	}
}
