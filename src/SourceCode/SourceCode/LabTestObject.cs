using System.Collections.Generic;
using UnityEngine;

public class LabTestObject : LabObject
{
	public Vector3 _OffsetFromMousePos;

	public Renderer _Renderer;

	protected LabItem mTestItem;

	protected Transform mMarker;

	private bool mHeating;

	private bool mPickable;

	private bool mInitializedMaterial;

	private bool mRespawnOnCrucibleExit;

	private LabItem mParentItem;

	private bool mFreezing;

	private float mMaxStateTemperature = float.NegativeInfinity;

	protected LabState mState;

	private LabState mPrevState;

	protected float mTemperature;

	protected float mPrevTemperature;

	private bool mPausedStateUpdate;

	private float mHeatMultiplier;

	private bool mDestroy;

	private float mDestroyTimer;

	private SnChannel mMixSoundChannelSolid;

	private SnChannel mMixSoundChannelLiquid;

	private SnChannel mCoolingDownChannel;

	private bool mHidden;

	private List<GameObject> mAdditionObjects;

	private List<LabAdditionObjectData> mDefaultAdditionObjects;

	private GameObject mFireObject;

	private bool mInitializedAdditionObjeccts;

	private float mCurrentPropertyLerp = -1f;

	private LabCrucible.TestItemLoader mCrucibleItemLoader;

	private float mFuelTimer;

	private Transform mColliderGO;

	public const float PARTICLE_POS = 1.135f;

	public bool pPickable
	{
		get
		{
			return mPickable;
		}
		set
		{
			mPickable = value;
		}
	}

	public bool pInitializedMaterial => mInitializedMaterial;

	public bool pRespawnOnCrucibleExit
	{
		get
		{
			return mRespawnOnCrucibleExit;
		}
		set
		{
			mRespawnOnCrucibleExit = value;
		}
	}

	public LabItem pParentItem
	{
		get
		{
			return mParentItem;
		}
		set
		{
			mParentItem = value;
		}
	}

	public Material pMaterial
	{
		get
		{
			if (_Renderer != null)
			{
				return _Renderer.material;
			}
			return null;
		}
	}

	public bool pCanWeight { get; set; }

	public bool pCanTestResistance { get; set; }

	public LabCrucible.TestItemLoader pCrucibleItemLoader
	{
		get
		{
			return mCrucibleItemLoader;
		}
		set
		{
			mCrucibleItemLoader = value;
		}
	}

	public float pHeatMultiplier => mHeatMultiplier;

	public LabState pState
	{
		get
		{
			return mState;
		}
		set
		{
			mState = value;
		}
	}

	public float pTemperature
	{
		get
		{
			return mTemperature;
		}
		set
		{
			mTemperature = value;
		}
	}

	public float pFuelTimer => mFuelTimer;

	protected LabCrucible pCrucible
	{
		get
		{
			if (mManager != null)
			{
				return mManager.pCrucible;
			}
			return null;
		}
	}

	public LabItem pTestItem => mTestItem;

	public Transform pMarker
	{
		get
		{
			return mMarker;
		}
		set
		{
			mMarker = value;
		}
	}

	public bool pIsInCrucible
	{
		get
		{
			if (pCrucible != null)
			{
				return pCrucible.HasItemInCrucible(this);
			}
			return false;
		}
	}

	public Transform pColliderGO => mColliderGO;

	public virtual Vector3 pTopPosition => base.transform.position;

	public virtual Vector3 pBottomPosition => base.transform.position;

	public bool HideItemsOnAdd()
	{
		return mTestItem.HideItemsOnAdd;
	}

	public void Show(bool inShow)
	{
		mHidden = !inShow;
		MeshRenderer componentInChildren = GetComponentInChildren<MeshRenderer>();
		if (componentInChildren != null)
		{
			componentInChildren.enabled = inShow;
		}
		if (base.rigidbody != null)
		{
			base.rigidbody.isKinematic = !inShow;
			base.rigidbody.useGravity = inShow;
		}
		MeshCollider component = GetComponent<MeshCollider>();
		if (component != null)
		{
			component.enabled = inShow;
		}
	}

	private void CheckAndRemoveFromCrucible(bool inRemoveFromCrucible)
	{
		if (mRespawnOnCrucibleExit)
		{
			RespawnInCrucible();
			return;
		}
		if (inRemoveFromCrucible)
		{
			mManager.pCrucible.RemoveTestItem(this, inDestroy: false);
		}
		if (!mDestroy)
		{
			mDestroyTimer = mManager._TestItemLifeTimeOnFloor;
			pCanWeight = false;
			pCanTestResistance = false;
			mDestroy = true;
		}
	}

	public void RespawnInCrucible()
	{
		Transform freeMarker = pCrucible.GetFreeMarker();
		if (freeMarker != null)
		{
			base.transform.position = freeMarker.position;
			base.transform.rotation = freeMarker.rotation;
		}
	}

	public void ForceStopDestroy()
	{
		if (mDestroy)
		{
			pCanWeight = true;
			mDestroy = false;
			mDestroyTimer = 0f;
			pCanTestResistance = true;
			RespawnInCrucible();
		}
	}

	public void OnCollisionEnter(Collision inCollision)
	{
		if (inCollision.gameObject.layer == LayerMask.NameToLayer("MMOAvatar") || inCollision.gameObject.layer == LayerMask.NameToLayer("Avatar") || LabTweenScale.IsScaling(base.gameObject))
		{
			return;
		}
		if (mManager == null || mManager._MainUI == null)
		{
			Object.Destroy(base.gameObject);
			return;
		}
		if (inCollision.gameObject.layer == LayerMask.NameToLayer("Furniture") || inCollision.gameObject.layer == LayerMask.NameToLayer("Floor"))
		{
			CheckAndRemoveFromCrucible(!HideItemsOnAdd());
			return;
		}
		if (mManager._MainUI._Crucible != inCollision.transform && mManager._MainUI._OhmMeter.transform != inCollision.transform)
		{
			if (pCrucible.pTestItems == null || pCrucible.pTestItems.Count == 0)
			{
				return;
			}
			bool flag = false;
			foreach (LabTestObject pTestItem in pCrucible.pTestItems)
			{
				if (pTestItem != null && pTestItem.transform == inCollision.transform)
				{
					flag = true;
					break;
				}
			}
			if (!flag)
			{
				return;
			}
		}
		if (mManager._MainUI.pObjectInHand == null || mManager._MainUI.pObjectInHand.pObject != base.transform)
		{
			if (this.pTestItem != null && mManager._DragonDropSolidSFX != null && this.pTestItem.Category == "SOLID")
			{
				SnChannel.Play(mManager._DragonDropSolidSFX, "SFX_Pool", inForce: true, null);
			}
			pCrucible.AddTestItem(base.gameObject, LabCrucible.ItemPositionOption.DEFAULT, Vector3.zero, Quaternion.identity, base.gameObject.transform.localScale, null, null, null);
		}
	}

	public void OnCollisionExit(Collision inCollision)
	{
		if (HideItemsOnAdd() && (inCollision.transform == mManager._MainUI._Crucible || inCollision.transform == mManager._MainUI._OhmMeter.transform))
		{
			CheckAndRemoveFromCrucible(inRemoveFromCrucible: true);
		}
	}

	public override void OnDisable()
	{
		if (pCrucibleItemLoader != null)
		{
			pCrucibleItemLoader.mRemove = true;
		}
		if (mManager != null && pCrucible != null)
		{
			pCrucible.RemoveTestItem(this);
		}
		if (mAdditionObjects != null && mAdditionObjects.Count > 0)
		{
			SnChannel.StopPool("LabParticlePool");
			foreach (GameObject mAdditionObject in mAdditionObjects)
			{
				if (mAdditionObject != null)
				{
					Object.Destroy(mAdditionObject);
				}
			}
			mAdditionObjects = null;
		}
		RemoveDefaultAdditions();
		if (mCoolingDownChannel != null)
		{
			mCoolingDownChannel.Stop();
		}
		if (mFireObject != null)
		{
			SnChannel.StopPool("LabParticlePool");
			Object.Destroy(mFireObject);
		}
		mMarker = null;
		mFireObject = null;
	}

	private void RemoveDefaultAdditions()
	{
		if (mDefaultAdditionObjects == null || mDefaultAdditionObjects.Count == 0)
		{
			return;
		}
		foreach (LabAdditionObjectData mDefaultAdditionObject in mDefaultAdditionObjects)
		{
			if (mDefaultAdditionObject != null && mDefaultAdditionObject.pAdditionObject != null)
			{
				Object.Destroy(mDefaultAdditionObject.pAdditionObject);
				mDefaultAdditionObject.pAdditionObject = null;
			}
		}
		mDefaultAdditionObjects = null;
	}

	protected virtual void Start()
	{
		InitializeMaterial();
	}

	protected void OnDestroy()
	{
		if (mColliderGO != null)
		{
			mColliderGO.gameObject.SetActive(value: false);
		}
	}

	private void InitializeMaterial()
	{
		if (!(mManager != null))
		{
			return;
		}
		ScientificExperiment.LabSubstanceMap proceduralMaterial = mManager.GetProceduralMaterial(mTestItem.Name);
		if (proceduralMaterial != null)
		{
			if (string.IsNullOrEmpty(proceduralMaterial._MaterialName))
			{
				Material material = UpdateShaderMaterial(_Renderer.material);
				if (material != null)
				{
					_Renderer.material = material;
				}
			}
			else
			{
				Material[] array = new Material[_Renderer.materials.Length];
				for (int i = 0; i < _Renderer.materials.Length; i++)
				{
					if (_Renderer.materials[i].name.StartsWith(proceduralMaterial._MaterialName))
					{
						Material material2 = UpdateShaderMaterial(array[i]);
						if (material2 != null)
						{
							array[i] = material2;
						}
					}
					else
					{
						array[i] = _Renderer.materials[i];
					}
				}
				_Renderer.materials = array;
			}
		}
		mInitializedMaterial = true;
	}

	private string GetShader()
	{
		if (_Renderer != null && _Renderer.material != null && _Renderer.material.shader != null)
		{
			return _Renderer.material.shader.name;
		}
		return string.Empty;
	}

	private Material UpdateShaderMaterial(Material inMaterial)
	{
		if (mTestItem == null || mTestItem.DefaultShaderProperties == null || mTestItem.DefaultShaderProperties.Length == 0)
		{
			return null;
		}
		string shader = GetShader();
		if (string.IsNullOrEmpty(shader) || shader == mTestItem.ShaderName)
		{
			return null;
		}
		Material material = new Material(mManager._ShaderMaterial);
		material.shader = Shader.Find(mTestItem.ShaderName);
		for (int i = 0; i < mTestItem.DefaultShaderProperties.Length; i++)
		{
			if (mTestItem.DefaultShaderProperties[i] == null)
			{
				continue;
			}
			switch (mTestItem.DefaultShaderProperties[i].Type)
			{
			case LabShaderDataType.Texture:
			{
				Texture shaderTexture = mTestItem.GetShaderTexture(mTestItem.DefaultShaderProperties[i].Name);
				if (shaderTexture != null)
				{
					material.SetTexture(mTestItem.DefaultShaderProperties[i].Name, shaderTexture);
				}
				break;
			}
			case LabShaderDataType.Float:
				material.SetFloat(mTestItem.DefaultShaderProperties[i].Name, UtStringUtil.Parse(mTestItem.DefaultShaderProperties[i].Value, 0f));
				break;
			case LabShaderDataType.COLOR:
				material.SetColor(mTestItem.DefaultShaderProperties[i].Name, LabStringUtil.Parse(mTestItem.DefaultShaderProperties[i].Value, Color.white));
				break;
			}
		}
		return material;
	}

	private void UpdateShaderMaterial(LabState inState)
	{
		LabShaderProperty[] shaderProperties = inState.ShaderProperties;
		if (shaderProperties == null || shaderProperties.Length == 0)
		{
			return;
		}
		string shader = GetShader();
		if (!string.IsNullOrEmpty(shader) && shader != inState.ShaderName)
		{
			_Renderer.material = mManager._ShaderMaterial;
			_Renderer.material.shader = Shader.Find(inState.ShaderName);
		}
		for (int i = 0; i < shaderProperties.Length; i++)
		{
			LabShaderProperty labShaderProperty = shaderProperties[i];
			if (labShaderProperty == null)
			{
				continue;
			}
			switch (labShaderProperty.Type)
			{
			case LabShaderDataType.Texture:
			{
				Texture shaderTexture = inState.GetShaderTexture(shaderProperties[i].Name);
				if (shaderTexture != null && labShaderProperty != null)
				{
					_Renderer.material.SetTexture(labShaderProperty.Name, shaderTexture);
				}
				break;
			}
			case LabShaderDataType.Float:
				_Renderer.material.SetFloat(labShaderProperty.Name, UtStringUtil.Parse(labShaderProperty.Value, 0f));
				break;
			case LabShaderDataType.COLOR:
				_Renderer.material.SetColor(mTestItem.DefaultShaderProperties[i].Name, LabStringUtil.Parse(mTestItem.DefaultShaderProperties[i].Value, Color.white));
				break;
			}
		}
		UpdateShader(0f, mState.Interpolations);
	}

	public override void Update()
	{
		base.Update();
		if (mDestroy)
		{
			if (mManager._MainUI != null && mManager._MainUI.pObjectInHand != null && mManager._MainUI.pObjectInHand.pType == UiScienceExperiment.InHandObjectType.TEST_ITEM && mManager._MainUI.pObjectInHand.pObject != null && mManager._MainUI.pObjectInHand.pObject.gameObject == base.gameObject)
			{
				mDestroy = false;
			}
			else
			{
				if (mDestroyTimer <= 0f)
				{
					Object.Destroy(base.gameObject);
					return;
				}
				mDestroyTimer -= Time.deltaTime;
			}
		}
		if (pIsInCrucible)
		{
			CheckItemReset();
		}
		if (mManager != null && mManager._MainUI != null && !mManager._MainUI.pUserPromptOn)
		{
			UpdateTemperature();
			UpdateState();
			UpdateFuelTime();
		}
		UpdateRendererProperties();
		HandleAdditionObjects();
		HandleDefaultAdditions();
		HandleFireObjects();
	}

	private void HandleAdditionObjects()
	{
		if (mAdditionObjects == null || mAdditionObjects.Count == 0)
		{
			return;
		}
		foreach (GameObject mAdditionObject in mAdditionObjects)
		{
			if (!(mAdditionObject == null))
			{
				Vector3 position = base.transform.position;
				if (pIsInCrucible)
				{
					position.y = 1.135f;
				}
				else
				{
					position.y = (pTopPosition.y + pBottomPosition.y) / 2f;
				}
				mAdditionObject.transform.position = position;
			}
		}
	}

	private void HandleDefaultAdditions()
	{
		if (LabTweenScale.IsScaling(base.gameObject))
		{
			RemoveDefaultAdditions();
		}
		else
		{
			if (!pIsInCrucible)
			{
				return;
			}
			if (!mInitializedAdditionObjeccts)
			{
				InitializeDefaultObjects();
			}
			if (mDefaultAdditionObjects == null || mDefaultAdditionObjects.Count == 0)
			{
				return;
			}
			foreach (LabAdditionObjectData mDefaultAdditionObject in mDefaultAdditionObjects)
			{
				if (mDefaultAdditionObject != null && mDefaultAdditionObject.pAdditionObject != null && mDefaultAdditionObject.UsePosition)
				{
					mDefaultAdditionObject.pAdditionObject.transform.position = mDefaultAdditionObject.pPosition;
				}
			}
		}
	}

	private void HandleFireObjects()
	{
		if (!(mFireObject == null))
		{
			Vector3 position = base.transform.position;
			if (pIsInCrucible)
			{
				position.y = 1.135f;
			}
			else
			{
				position.y = (pTopPosition.y + pBottomPosition.y) / 2f;
			}
			mFireObject.transform.position = position;
		}
	}

	public virtual void OnScaleStopped()
	{
		if (!mHidden && ScientificExperiment.IsSolid(mTestItem.pCategory) && base.rigidbody != null && !LabTweenScale.IsScaling(base.gameObject))
		{
			base.rigidbody.isKinematic = false;
			base.rigidbody.useGravity = true;
		}
	}

	private void CheckItemReset()
	{
		if (mManager == null || mManager._TestItemResetMarker == null || (mManager._MainUI != null && mManager._MainUI.pObjectInHand != null && mManager._MainUI.pObjectInHand.pObject == base.transform))
		{
			return;
		}
		Vector3 normalized = (mManager._TestItemResetMarker.position - (base.transform.position - _OffsetFromMousePos)).normalized;
		Vector3 rhs = mManager._TestItemResetMarker.up * -1f;
		if (!(Vector3.Dot(normalized, rhs) >= 0f))
		{
			Vector3 vector = base.transform.position - mManager._TestItemResetMarker.position;
			if (Mathf.Sqrt(vector.z * vector.z + vector.x * vector.x) <= mManager._CrucibleRadius)
			{
				pCrucible.AddTestItem(base.gameObject, LabCrucible.ItemPositionOption.MARKER, Vector3.zero, Quaternion.identity, base.gameObject.transform.localScale, null, null, null);
			}
		}
	}

	private void UpdateTemperature()
	{
		if (LabCrucible.TestItemLoader.IsLoading() || LabCrucible.pIsMixing)
		{
			return;
		}
		float num = 0f;
		if (mTestItem != null && pTemperature >= mTestItem.MaxTemperature && mManager != null && mManager.pCrucibleItemCount == 1)
		{
			mHeating = false;
		}
		if (pCrucible != null && mHeating)
		{
			num = mTemperature + Time.deltaTime * pCrucible.pCurrentHeatMultiplier;
		}
		else if (pCrucible != null && mFreezing)
		{
			num = mTemperature + Time.deltaTime * pCrucible.pCurrentFreezeMultiplier * -1f;
		}
		else
		{
			num = ((pCrucible == null || !(pCrucible.pTemperatureRefObject != this) || !pCrucible.HasItemInCrucible(this)) ? mTemperature : LabCrucible.GotoTemperature(pCrucible.pTemperature, mTemperature, Time.deltaTime, pCrucible.pCurrentFreezeMultiplier, pCrucible.pCurrentHeatMultiplier));
			if (pCrucible != null && LabData.pInstance != null)
			{
				num = LabCrucible.GotoTemperature(LabData.pInstance.RoomTemperature, num, Time.deltaTime, pCrucible.pCurrentCoolingRate, pCrucible.pCurrentWarmingRate);
			}
		}
		if (mTestItem != null && mManager != null && mManager.pCrucibleItemCount == 1)
		{
			num = Mathf.Min(mTestItem.MaxTemperature, num);
			num = Mathf.Max(mTestItem.MinTemperature, num);
		}
		if (!(mTemperature >= num) || !(mFireObject != null) || mTestItem == null || (mTestItem.pCategory != LabItemCategory.SOLID_COMBUSTIBLE && mTestItem.pCategory != LabItemCategory.LIQUID_COMBUSTIBLE))
		{
			mPrevTemperature = mTemperature;
			mTemperature = num;
			OnTemperatureChanged(mPrevTemperature);
		}
	}

	private void OnTemperatureChanged(float inPrevTemperature)
	{
		if (mTemperature >= mManager._MinTemperartureToStartCooldownSFX && mTemperature < inPrevTemperature)
		{
			if (mTestItem.mCoolingDownClip != null && (mCoolingDownChannel == null || !mCoolingDownChannel.pIsPlaying))
			{
				mCoolingDownChannel = SnChannel.Play(mTestItem.mCoolingDownClip, "CoolingDown_Pool", inForce: true, null);
				mCoolingDownChannel.pLoop = true;
			}
		}
		else
		{
			SnChannel.StopPool("CoolingDown_Pool");
		}
	}

	public float GetMaxStateTemperature()
	{
		if (mMaxStateTemperature != float.NegativeInfinity)
		{
			return mMaxStateTemperature;
		}
		if (mTestItem == null || mTestItem.States == null || mTestItem.States.Length == 0)
		{
			mMaxStateTemperature = LabData.pInstance.RoomTemperature;
		}
		else
		{
			LabState[] states = mTestItem.States;
			foreach (LabState labState in states)
			{
				if (labState != null)
				{
					mMaxStateTemperature = Mathf.Max(mMaxStateTemperature, labState.Temperature);
				}
			}
		}
		if (mMaxStateTemperature == float.NegativeInfinity)
		{
			mMaxStateTemperature = LabData.pInstance.RoomTemperature;
		}
		return mMaxStateTemperature;
	}

	public void Heat(bool inHeat = true)
	{
		if (pIsInCrucible && mManager != null && mManager._MainUI != null && !mManager._MainUI.pUserPromptOn)
		{
			UpdateTemperature();
			UpdateState();
		}
		mHeating = inHeat;
	}

	public void Freeze(bool inFreeze)
	{
		mFreezing = inFreeze;
	}

	public void Initialize(LabItem inItem, ScientificExperiment inManager)
	{
		Initialize(inManager);
		mTestItem = inItem;
		mTemperature = mTestItem.pStartTemperature;
		mTestItem.pStartTemperature = LabData.pInstance.RoomTemperature;
		if (LabTutorial.InTutorial)
		{
			mPickable = false;
		}
		else
		{
			mPickable = inItem.Pickable;
		}
		mHeatMultiplier = 0f;
		float num = GetMaxStateTemperature();
		if (num < LabData.pInstance.RoomTemperature)
		{
			num = inItem.MaxTemperature;
		}
		mHeatMultiplier = (num - LabData.pInstance.RoomTemperature) / (float)mTestItem.BlastsToReachMaxTemp / mManager.pHeatTime;
		if (mHeatMultiplier <= 0f)
		{
			mHeatMultiplier = mManager._WarmingConstant;
		}
		mColliderGO = (string.IsNullOrEmpty(mTestItem.ColliderGO) ? null : mManager._ColliderGroup.Find(mTestItem.ColliderGO));
		if (mColliderGO != null)
		{
			mColliderGO.gameObject.SetActive(value: true);
		}
	}

	protected virtual void OnObjectRelease(LabCrucible crucible)
	{
		if (mColliderGO != null)
		{
			mColliderGO.gameObject.SetActive(value: false);
		}
	}

	protected virtual void OnObjectPickup()
	{
		if (mColliderGO != null)
		{
			mColliderGO.gameObject.SetActive(value: true);
		}
	}

	public void UpdateState()
	{
		if (!mPausedStateUpdate && (!(mFireObject != null) || (mTestItem.pCategory != LabItemCategory.SOLID_COMBUSTIBLE && mTestItem.pCategory != LabItemCategory.LIQUID_COMBUSTIBLE)))
		{
			UpdateStateWithTemperature();
		}
	}

	private void UpdateStateWithTemperature()
	{
		if (LabData.pInstance == null || mTestItem == null || (mState != null && Mathf.Abs(mTemperature) <= Mathf.Abs(mState.Temperature) && Mathf.Abs(mTemperature) > Mathf.Abs(mState.ReboundTemperature)))
		{
			return;
		}
		List<LabState> list = null;
		list = ((!(mTemperature < LabData.pInstance.RoomTemperature)) ? mTestItem.pSortedStatesAboveRoomTemp : mTestItem.pSortedStatesBelowRoomTemp);
		LabState labState = null;
		foreach (LabState item in list)
		{
			if (Mathf.Abs(mTemperature - LabData.pInstance.RoomTemperature) >= Mathf.Abs(item.Temperature - LabData.pInstance.RoomTemperature))
			{
				labState = item;
			}
			else if (item.Name == "Transition" && Mathf.Abs(mPrevTemperature - LabData.pInstance.RoomTemperature) >= Mathf.Abs(item.Temperature - LabData.pInstance.RoomTemperature))
			{
				labState = item;
			}
		}
		if (mState != labState)
		{
			mPrevState = mState;
			mState = labState;
			InitChangeState();
		}
	}

	protected virtual void InitChangeState()
	{
		if (pCrucible != null)
		{
			pCrucible.InitStateChange(this, OnStateChanged);
		}
	}

	protected virtual void OnStateChanged()
	{
		if (pCrucible == null || mManager == null)
		{
			return;
		}
		pCrucible.OnStateChanged(this);
		if (mState != null && mState.PauseStateUpdate)
		{
			mPausedStateUpdate = true;
		}
		if (mPrevState != null)
		{
			if (mAdditionObjects != null)
			{
				foreach (GameObject mAdditionObject in mAdditionObjects)
				{
					HandleAdditionObjectOnStateChange(mAdditionObject, inPrevStateObjectMatch: false, mPrevState.ScaleOnTransitionTimeSec);
				}
				mAdditionObjects.Clear();
			}
			HandleAdditionObjectOnStateChange(mFireObject, mState != null && mState.FireObjectName == mPrevState.FireObjectName);
		}
		if (mState == null)
		{
			return;
		}
		if (mManager._DragonDisapprovalSFX != null && !string.IsNullOrEmpty(mState.Animation) && mManager.PlayDragonAnim(mState.Animation, inPlayOnce: true))
		{
			SnChannel.Play(mManager._DragonDisapprovalSFX, "SFX_Pool", inForce: true, null);
		}
		if (pIsInCrucible)
		{
			if (mState.Name == "Transition")
			{
				mTemperature = mState.Temperature;
				if (mState.ScaleOnTransition)
				{
					if (mTestItem != null && (mTestItem.pCategory == LabItemCategory.LIQUID || mTestItem.pCategory == LabItemCategory.LIQUID_COMBUSTIBLE))
					{
						((LabLiquidTestObject)this).pCurrentScaleTime = mState.ScaleOnTransitionTime;
					}
					pCrucible.RemoveTestItem(this, inDestroy: false);
				}
				if (mState.ObjectNames != null && mState.ObjectNames.Length != 0)
				{
					string[] objectNames = mState.ObjectNames;
					foreach (string inName in objectNames)
					{
						LabItem item = LabData.pInstance.GetItem(inName);
						if (item == null)
						{
							continue;
						}
						if (mState.ScaleOnTransition)
						{
							LabCrucible.TestItemLoader testItemLoader = pCrucible.AddTestItem(item, mTemperature, OnTransitionItemAdded);
							if (testItemLoader != null)
							{
								if (item.pCategory == LabItemCategory.LIQUID || item.pCategory == LabItemCategory.LIQUID_COMBUSTIBLE)
								{
									testItemLoader.mItemPositionOption = LabCrucible.ItemPositionOption.POSITION;
									testItemLoader.mPosition = mManager._LiquidItemDefaultPos;
								}
								else
								{
									testItemLoader.mItemPositionOption = LabCrucible.ItemPositionOption.MARKER;
									testItemLoader.mPosition = Vector3.zero;
								}
								testItemLoader.mParentItem = mTestItem;
								testItemLoader.Load();
							}
							continue;
						}
						LabCrucible.TestItemLoader testItemLoader2 = pCrucible.AddTestItem(item, mTemperature, OnItemAdded);
						if (testItemLoader2 != null)
						{
							if (item.pCategory != LabItemCategory.LIQUID && item.pCategory != LabItemCategory.LIQUID_COMBUSTIBLE)
							{
								testItemLoader2.mItemPositionOption = LabCrucible.ItemPositionOption.POSITION;
								testItemLoader2.mPosition = base.transform.position;
								testItemLoader2.mRotation = base.transform.rotation;
								testItemLoader2.mScale = base.transform.localScale;
							}
							testItemLoader2.Load();
						}
					}
				}
			}
			if (mAdditionObjects == null)
			{
				mAdditionObjects = new List<GameObject>();
			}
			if (mAdditionObjects.Count == 0 && mState.pAdditionObjects != null && mState.pAdditionObjects.Count > 0)
			{
				foreach (GameObject pAdditionObject in mState.pAdditionObjects)
				{
					if (pAdditionObject != null)
					{
						mAdditionObjects.Add(Object.Instantiate(pAdditionObject));
					}
				}
			}
			if (mAdditionObjects.Count > 0)
			{
				foreach (GameObject mAdditionObject2 in mAdditionObjects)
				{
					InitAdditionObject(mAdditionObject2, mState.pParticleAudioClip);
				}
			}
		}
		if (mFireObject == null && mState.pFireObject != null)
		{
			mFireObject = Object.Instantiate(mState.pFireObject);
			InitAdditionObject(mFireObject, mState.pParticleAudioClip);
			if (mTestItem != null)
			{
				mFuelTimer = mTestItem.FuelTime;
			}
		}
	}

	private void HandleShaderTexOnStateChange()
	{
		if (mState != null)
		{
			UpdateShaderMaterial(mState);
		}
	}

	private bool InitAdditionObject(GameObject inAdditionObject, AudioClip inClicp)
	{
		if (inAdditionObject == null)
		{
			return false;
		}
		ParticleSystem component = inAdditionObject.GetComponent<ParticleSystem>();
		if (component != null)
		{
			return false;
		}
		component = inAdditionObject.GetComponentInChildren<ParticleSystem>();
		if (component == null)
		{
			return false;
		}
		if (inClicp != null)
		{
			SnChannel snChannel = SnChannel.Play(inClicp, "LabParticlePool", inForce: true, base.gameObject);
			if (snChannel != null)
			{
				snChannel.pLoop = false;
			}
		}
		component.Play();
		return true;
	}

	private void HandleAdditionObjectOnStateChange(GameObject inAdditionObject, bool inPrevStateObjectMatch, float destroyFxSecs = 1f)
	{
		if (inAdditionObject == null)
		{
			return;
		}
		ParticleSystem componentInChildren = inAdditionObject.GetComponentInChildren<ParticleSystem>();
		if (componentInChildren != null)
		{
			if (mState == null || !inPrevStateObjectMatch)
			{
				ParticleSystem.MainModule main = componentInChildren.main;
				main.startLifetime = destroyFxSecs;
				Object.Destroy(inAdditionObject.gameObject, destroyFxSecs + 1f);
				SnChannel.StopPool("LabParticlePool");
			}
		}
		else
		{
			Object.Destroy(inAdditionObject.gameObject);
			inAdditionObject = null;
		}
	}

	public void OnSnEvent(SnEvent inSndEvent)
	{
		switch (inSndEvent.mType)
		{
		case SnEventType.PLAY:
			if (inSndEvent.mClip != null && inSndEvent.mClip.name == mManager._WaterSteamSFX.name)
			{
				float maxStateTemperature = GetMaxStateTemperature();
				if (pCrucible.pTemperature > maxStateTemperature + 4f)
				{
					SnChannel.Play(mManager._WaterSteamSuddenSFX, "Default_Pool2", inForce: true, base.gameObject);
				}
			}
			break;
		case SnEventType.END:
			if (inSndEvent.mClip != null && inSndEvent.mClip.name == mManager._FlameStartSFX.name)
			{
				SnChannel snChannel = SnChannel.Play(mManager._FlameSFX, "LabParticlePool", inForce: true, base.gameObject);
				if (snChannel != null)
				{
					snChannel.pLoop = true;
				}
			}
			break;
		}
	}

	private void OnItemAdded(LabTestObject inTestObject)
	{
		pCrucible.RemoveTestItem(this);
	}

	private void OnTransitionItemAdded(LabTestObject inTestObject)
	{
		float num = 0f;
		float num2 = 0f;
		if (mState != null)
		{
			num = mState.ScaleOnTransitionTime;
			num2 = mState.ScaleOnTransitionTimeSec;
		}
		if (mTestItem.pCategory == LabItemCategory.LIQUID || mTestItem.pCategory == LabItemCategory.LIQUID_COMBUSTIBLE)
		{
			LabLiquidTestObject obj = this as LabLiquidTestObject;
			obj.pCurrentScaleTime = num;
			obj.pDestroyOnScaleEnd = true;
		}
		else
		{
			StartScaling(inScaleDown: true, num);
		}
		if (inTestObject != null)
		{
			if (inTestObject.pTestItem.pCategory == LabItemCategory.LIQUID || inTestObject.pTestItem.pCategory == LabItemCategory.LIQUID_COMBUSTIBLE)
			{
				(inTestObject as LabLiquidTestObject).pCurrentScaleTime = num2;
				return;
			}
			inTestObject.transform.localScale = Vector3.zero;
			inTestObject.StartScaling(inScaleDown: false, num2);
		}
	}

	public void UpdateRendererProperties()
	{
		if (mState == null || mState.Properties == null || mState.Properties.Length == 0 || RsResourceManager.pLevelLoading || pMaterial == null)
		{
			return;
		}
		float pMinTemperature = mState.pMinTemperature;
		float pMaxTemperature = mState.pMaxTemperature;
		float num = 0f;
		if (mTemperature < pMinTemperature)
		{
			num = 0f;
		}
		else if (mTemperature >= pMaxTemperature)
		{
			num = 1f;
		}
		else
		{
			float num2 = pMinTemperature;
			if (pMaxTemperature == mState.Temperature)
			{
				num2 = pMaxTemperature;
			}
			float num3 = Mathf.Abs(mTemperature - num2);
			float num4 = Mathf.Abs(pMaxTemperature - pMinTemperature);
			num = num3 / num4;
		}
		float inLerpValue = Mathf.Min(1f, Mathf.Max(0f, num));
		UpdateShader(inLerpValue, mState.Interpolations);
	}

	public void UpdateShader(LabShaderValue[] inValues)
	{
		if (RsResourceManager.pLevelLoading || pMaterial == null || inValues == null || inValues.Length == 0)
		{
			return;
		}
		for (int i = 0; i < inValues.Length; i++)
		{
			if (inValues[i] != null)
			{
				pMaterial.SetFloat(inValues[i].Name, inValues[i].Value);
			}
		}
	}

	public void UpdateShader(float inLerpValue, LabShaderInterpolation[] inInterpolations)
	{
		if (inLerpValue == mCurrentPropertyLerp || RsResourceManager.pLevelLoading || pMaterial == null || inInterpolations == null || inInterpolations.Length == 0)
		{
			return;
		}
		bool flag = false;
		for (int i = 0; i < inInterpolations.Length; i++)
		{
			if (inInterpolations[i] != null)
			{
				float value = Mathf.Lerp(inInterpolations[i].Min, inInterpolations[i].Max, inLerpValue);
				pMaterial.SetFloat(inInterpolations[i].Name, value);
				flag = true;
			}
		}
		if (flag)
		{
			mCurrentPropertyLerp = inLerpValue;
		}
	}

	public void StartScaling(bool inScaleDown, float inDuration)
	{
		if (!(this == null))
		{
			Vector3 inEndScale = Vector3.zero;
			if (!inScaleDown)
			{
				inEndScale = mTestItem.pScale;
			}
			LabTweenScale.Scale(base.gameObject, inScaleDown, inDuration, inEndScale);
		}
	}

	public float GetCurrentWeight()
	{
		float num = pTestItem.pScale.x * 3f;
		float num2 = pTestItem.Weight / num;
		return (base.transform.localScale.x + base.transform.localScale.y + base.transform.localScale.z) * num2;
	}

	public void OnPestleHit()
	{
		AudioClip[] array = mManager._SolidMoveSFX;
		SnChannel snChannel = mMixSoundChannelSolid;
		string inPool = "PestleSolid_Pool";
		if (mTestItem.pCategory == LabItemCategory.LIQUID || mTestItem.pCategory == LabItemCategory.LIQUID_COMBUSTIBLE)
		{
			array = mManager._LiquidMoveSFX;
			snChannel = mMixSoundChannelLiquid;
			inPool = "PestleLiquid_Pool";
		}
		if ((snChannel == null || !snChannel.pIsPlaying) && array != null && array.Length != 0)
		{
			snChannel = SnChannel.Play(array[Random.Range(0, array.Length)], inPool, inForce: true, null);
		}
		if (mTestItem.pCategory == LabItemCategory.LIQUID || mTestItem.pCategory == LabItemCategory.LIQUID_COMBUSTIBLE)
		{
			mMixSoundChannelLiquid = snChannel;
		}
		else
		{
			mMixSoundChannelSolid = snChannel;
		}
	}

	public void StopCombustion()
	{
		mFuelTimer = 0f;
	}

	public void UpdateFuelTime()
	{
		if (mFireObject != null && (mTestItem.pCategory == LabItemCategory.SOLID_COMBUSTIBLE || mTestItem.pCategory == LabItemCategory.LIQUID_COMBUSTIBLE))
		{
			mFuelTimer -= Time.deltaTime;
			if (mFuelTimer <= 0f)
			{
				RemoveFireParticle();
				pCrucible.CheckTaskRules(inFuelTimeout: true);
			}
		}
	}

	private void RemoveFireParticle()
	{
		if (!(mFireObject == null))
		{
			ParticleSystem particleSystem = mFireObject.GetComponent<ParticleSystem>();
			if (particleSystem == null)
			{
				particleSystem = mFireObject.GetComponentInChildren<ParticleSystem>();
			}
			if (particleSystem != null)
			{
				particleSystem.Stop();
				Object.Destroy(mFireObject.gameObject, 1f);
			}
			else
			{
				Object.Destroy(mFireObject.gameObject);
				mFireObject = null;
			}
			SnChannel.StopPool("LabParticlePool");
		}
	}

	public void ProcessLiquidInCrucible()
	{
		if (mTestItem != null && mTestItem.pCategory == LabItemCategory.SOLID_COMBUSTIBLE && mFireObject != null)
		{
			RemoveFireParticle();
		}
	}

	public virtual bool IsInteractionDone(LabTestObject inTestObject)
	{
		return false;
	}

	public void InitializeDefaultObjects()
	{
		if (mTestItem == null || mInitializedAdditionObjeccts || mTestItem.DefaultAdditions == null || mTestItem.DefaultAdditions.Length == 0)
		{
			return;
		}
		mInitializedAdditionObjeccts = true;
		if (mDefaultAdditionObjects == null)
		{
			mDefaultAdditionObjects = new List<LabAdditionObjectData>();
		}
		if (mDefaultAdditionObjects.Count > 0)
		{
			foreach (LabAdditionObjectData mDefaultAdditionObject in mDefaultAdditionObjects)
			{
				if (mDefaultAdditionObject.pAdditionObject != null)
				{
					Object.Destroy(mDefaultAdditionObject.pAdditionObject);
					mDefaultAdditionObject.pAdditionObject = null;
				}
			}
			mDefaultAdditionObjects.Clear();
		}
		LabAdditionObjectData[] defaultAdditions = mTestItem.DefaultAdditions;
		foreach (LabAdditionObjectData labAdditionObjectData in defaultAdditions)
		{
			if (labAdditionObjectData != null && !(labAdditionObjectData.pAdditionObject == null))
			{
				LabAdditionObjectData labAdditionObjectData2 = new LabAdditionObjectData();
				labAdditionObjectData2.pAdditionObject = Object.Instantiate(labAdditionObjectData.pAdditionObject);
				labAdditionObjectData2.UsePosition = labAdditionObjectData.UsePosition;
				if (labAdditionObjectData2.UsePosition)
				{
					labAdditionObjectData2.pPosition = labAdditionObjectData.pPosition;
				}
				mDefaultAdditionObjects.Add(labAdditionObjectData2);
			}
		}
	}
}
