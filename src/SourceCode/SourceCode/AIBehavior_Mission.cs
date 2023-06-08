using System;
using SWS;
using UnityEngine;
using UnityEngine.AI;

public abstract class AIBehavior_Mission : AIBehavior
{
	[Serializable]
	public enum TaskStatus
	{
		NONE,
		IN_PROGRESS,
		DONE
	}

	[Serializable]
	public enum MoveState
	{
		NONE,
		SPLINE,
		NAV_MESH,
		FLY,
		SWS
	}

	public class MissionData
	{
		public float TotalTime;

		public float GraceTime;

		public float Proximity;

		public string SplineObjectName;
	}

	public float _GroundCheckStartHeight = 2f;

	public float _GroundCheckDist = 20f;

	public float _RotateSpeed = 2f;

	public GameObject _TeleportFx;

	public AIBehavior_DrawProximity _DrawProximity;

	protected float mGraceTime;

	protected Spline mSpline;

	protected Transform mTarget;

	public int _MaxBreadcrumbCanPlace = 30;

	public float _BreadcrumbDistance = 1.5f;

	public float _BreadcrumbHeightOffset;

	private Vector3 mTargetBreadcrumb = Vector3.zero;

	private CustomCircularArray<Vector3> mCustomBreadcrumbArray;

	protected Task mTask;

	protected int mWaterLayer = -1;

	protected TaskStatus mStatus;

	protected MissionData mMissionData;

	protected NavMeshAgent mNavMeshAgent;

	protected bool mAddedNavMesh;

	protected AIActor mActor;

	protected Action mAction;

	protected MoveState mMoveState = MoveState.NAV_MESH;

	protected bool mInProximity = true;

	protected Vector3 mStartPoint = Vector3.zero;

	protected Quaternion mStartRotation = Quaternion.identity;

	protected Vector3 mPreviousPosition = Vector3.zero;

	protected string mCurrentAnim = string.Empty;

	[Header("Arrow")]
	public float _VerticalOffsetScreen = 800f;

	public float _ForwardOffsetWorld = 2f;

	private GameObject mArrow;

	private GameObject mNPC;

	public virtual bool IsFailed()
	{
		if (mTask != null)
		{
			return mTask.Failed;
		}
		return false;
	}

	public virtual bool IsActive()
	{
		if (mStatus == TaskStatus.IN_PROGRESS && !IsTaskCompleted())
		{
			return !IsFailed();
		}
		return false;
	}

	public virtual bool IsTimedOut()
	{
		return mGraceTime > mMissionData.GraceTime;
	}

	public virtual void EndBehavior()
	{
		mGraceTime = 0f;
		mStatus = TaskStatus.DONE;
		SetMoveState(MoveState.NONE);
		mActor.DoAction("MissionComplete");
		SetState(AIBehaviorState.COMPLETED);
	}

	protected virtual void SetMoveState(MoveState newState)
	{
		if (mMoveState == newState)
		{
			return;
		}
		switch (mMoveState)
		{
		case MoveState.NAV_MESH:
			if (mNavMeshAgent != null)
			{
				mNavMeshAgent.enabled = false;
			}
			break;
		case MoveState.SPLINE:
			if (mSpline != null && mActor._Character != null)
			{
				mActor._Character.SetSpline(null);
			}
			break;
		case MoveState.FLY:
			mActor.DoAction("InitLanding");
			break;
		case MoveState.SWS:
		{
			navMove component = mActor.GetComponent<navMove>();
			if (component != null)
			{
				component.Stop();
				break;
			}
			splineMove component2 = mActor.GetComponent<splineMove>();
			if (component2 != null)
			{
				component2.Stop();
			}
			break;
		}
		}
		switch (newState)
		{
		case MoveState.SPLINE:
			if (mNavMeshAgent != null)
			{
				mNavMeshAgent.enabled = false;
			}
			if (mSpline != null && mActor._Character != null)
			{
				mActor._Character.SetSpline(mSpline);
			}
			break;
		case MoveState.FLY:
			if (mNavMeshAgent != null)
			{
				mNavMeshAgent.enabled = false;
			}
			mActor.DoAction("InitFlying");
			break;
		case MoveState.SWS:
			if (mNavMeshAgent != null)
			{
				mNavMeshAgent.enabled = false;
			}
			break;
		case MoveState.NAV_MESH:
			if (mNavMeshAgent != null)
			{
				mNavMeshAgent.enabled = true;
			}
			break;
		case MoveState.NONE:
			PlayIdle();
			break;
		}
		mMoveState = newState;
	}

	public virtual void PlayIdle()
	{
		PlayAnim("Idle");
	}

	public void SetSpline(Transform rootTransform)
	{
		if (!(rootTransform != null))
		{
			return;
		}
		SWS.PathManager component = rootTransform.GetComponent<SWS.PathManager>();
		if (component != null)
		{
			navMove component2 = mActor.GetComponent<navMove>();
			if (component2 != null)
			{
				component2.ChangeSpeed(mActor._Character.Speed);
				if (component2.pathContainer != component)
				{
					component2.SetPath(component);
				}
				else
				{
					component2.StartMove();
				}
			}
			splineMove splineMove = mActor.GetComponent<splineMove>();
			if (splineMove == null)
			{
				splineMove = mActor.gameObject.AddComponent<splineMove>();
			}
			if (splineMove != null)
			{
				splineMove.ChangeSpeed(mActor._Character.Speed);
				if (splineMove.pathContainer != component)
				{
					splineMove.SetPath(component);
				}
				else
				{
					splineMove.StartMove();
				}
			}
			SetMoveState(MoveState.SWS);
			return;
		}
		if (mAction == null)
		{
			mAction = (Action)Delegate.Combine(mAction, (Action)delegate
			{
				rootTransform.BroadcastMessage("SetObject", mActor.gameObject, SendMessageOptions.DontRequireReceiver);
			});
		}
		mSpline = new Spline(rootTransform.childCount, looping: false, constSpeed: true, alignTangent: true, hasQ: true);
		for (int i = 0; i < rootTransform.childCount; i++)
		{
			Transform child = rootTransform.GetChild(i);
			mSpline.SetControlPoint(i, child.position, child.rotation, 0f);
		}
		mSpline.RecalculateSpline();
		SetMoveState(MoveState.SPLINE);
	}

	public override void OnStart(AIActor Actor)
	{
		base.OnStart(Actor);
		mActor = Actor;
		mStartPoint = mActor.transform.position;
		mStartRotation = mActor.transform.rotation;
		mPreviousPosition = mActor.transform.position;
		MissionManager.AddMissionEventHandler(OnMissionEvent);
		mWaterLayer = LayerMask.NameToLayer("Water");
		mStatus = TaskStatus.IN_PROGRESS;
		mNavMeshAgent = Actor.GetComponent<NavMeshAgent>();
		if (mNavMeshAgent == null)
		{
			mNavMeshAgent = Actor.gameObject.AddComponent<NavMeshAgent>();
			mAddedNavMesh = true;
		}
		mNavMeshAgent.speed = Actor._Speed;
		mNavMeshAgent.stoppingDistance = Actor._StoppingDistance;
		if (mMissionData != null && !string.IsNullOrEmpty(mMissionData.SplineObjectName) && Actor.pController == null)
		{
			GameObject gameObject = GameObject.Find(mMissionData.SplineObjectName);
			if (gameObject != null)
			{
				SetSpline(gameObject.transform);
			}
			else
			{
				Debug.Log("Can't find the spline object : " + mMissionData.SplineObjectName);
			}
		}
		if (_DrawProximity != null)
		{
			_DrawProximity.UpdateColor(mInProximity);
		}
		InitBreadcrumb();
	}

	public override void OnTerminate(AIActor Actor)
	{
		if (mAddedNavMesh)
		{
			mAddedNavMesh = false;
			UnityEngine.Object.Destroy(mNavMeshAgent);
		}
		ShowProximityArrow(enable: false);
		MissionManager.RemoveMissionEventHandler(OnMissionEvent);
		base.OnTerminate(Actor);
		if (mCustomBreadcrumbArray != null)
		{
			mCustomBreadcrumbArray = null;
		}
	}

	public void OnDestroy()
	{
		MissionManager.RemoveMissionEventHandler(OnMissionEvent);
	}

	public virtual void OnMissionEvent(MissionEvent inEvent, object inObject)
	{
		if ((inEvent == MissionEvent.TASK_FAIL || inEvent == MissionEvent.TASK_COMPLETE) && mTask == (Task)inObject)
		{
			EndBehavior();
			if (inEvent == MissionEvent.TASK_FAIL && mActor.pHasController)
			{
				TeleportTo(mStartPoint, mStartRotation);
			}
		}
	}

	public virtual void ProcessTime()
	{
		if (!mInProximity && !IsTimedOut() && AvAvatar.pToolbar != null && AvAvatar.pToolbar.activeSelf)
		{
			string titleText = "";
			if (mTask != null && mTask.pData != null && mTask.pData.Reminder != null)
			{
				titleText = MissionManager.pInstance.FormatText(mTask.pData.Reminder.ID, mTask.pData.Reminder.Text, mTask);
			}
			UiMissionStatusDB.Show(titleText, GetTimeFormat(mMissionData.GraceTime - mGraceTime), mMissionData.GraceTime);
			mGraceTime += Time.deltaTime;
			ShowProximityArrow(enable: true);
		}
		else
		{
			ShowProximityArrow(enable: false);
		}
	}

	public string GetTimeFormat(float displayTime)
	{
		int num = (int)(displayTime / 60f);
		int num2 = (int)(displayTime % 60f);
		return $"{num:00}" + ":" + $"{num2:00}";
	}

	protected virtual Transform GetTarget()
	{
		if (!(mActor != null) || !mActor.pHasController)
		{
			return AvAvatar.mTransform;
		}
		return mActor.pController.transform;
	}

	protected virtual Transform GetTargetForProximity()
	{
		return mTarget ?? AvAvatar.mTransform;
	}

	public virtual void ProcessProximity(AIActor Actor)
	{
		if (Vector3.Distance(GetTargetForProximity().position, Actor.transform.position) < mMissionData.Proximity)
		{
			InProximity(Actor);
		}
		else
		{
			OutOfProximity(Actor);
		}
	}

	protected virtual void ProcessLookAt(AIActor Actor)
	{
		if (mMoveState != MoveState.FLY && IsCloseBy(Actor.transform, mTarget.transform, mMissionData.Proximity))
		{
			ComputeLookAt(Actor.transform);
		}
	}

	protected void ComputeLookAt(Transform LookTarget)
	{
		Vector3 normalized = (mTarget.transform.position - LookTarget.position).normalized;
		Quaternion rotation = Quaternion.Lerp(LookTarget.rotation, Quaternion.LookRotation(normalized), Time.deltaTime * _RotateSpeed);
		rotation.x = LookTarget.rotation.x;
		rotation.z = LookTarget.rotation.z;
		LookTarget.rotation = rotation;
	}

	public virtual void ProcessMove(AIActor Actor)
	{
		if (mMoveState != MoveState.SPLINE && mMoveState != MoveState.SWS)
		{
			SetMoveStateSpeed(Actor);
			if (Actor.IsFlying())
			{
				if (mMoveState != MoveState.FLY)
				{
					SetMoveState(MoveState.FLY);
				}
			}
			else
			{
				if (mMoveState != MoveState.NAV_MESH)
				{
					SetMoveState(MoveState.NAV_MESH);
				}
				if (mTarget != null && mNavMeshAgent != null)
				{
					bool flag = CanMove(Actor);
					mNavMeshAgent.speed = (flag ? Actor._Speed : 0f);
				}
			}
		}
		switch (mMoveState)
		{
		case MoveState.NAV_MESH:
			if (mTarget != null && mNavMeshAgent != null && mNavMeshAgent.enabled)
			{
				if (mNavMeshAgent.speed == 0f)
				{
					mNavMeshAgent.SetDestination(Actor.transform.position);
				}
				else
				{
					mNavMeshAgent.SetDestination(mTarget.position);
				}
			}
			break;
		case MoveState.FLY:
			ProcessFly(Actor);
			break;
		}
		if (mMoveState != MoveState.SWS)
		{
			ProcessLookAt(Actor);
		}
		if (CanPlayAnimation(Actor))
		{
			PlayAnimation(Actor);
		}
		mPreviousPosition = mActor.transform.position;
	}

	protected virtual bool CanPlayAnimation(AIActor Actor)
	{
		if (!(Actor._Character != null))
		{
			return true;
		}
		return Actor._Character.GetState() != Character_State.action;
	}

	public virtual void InProximity(AIActor Actor)
	{
		if (!mInProximity)
		{
			if (_DrawProximity != null)
			{
				_DrawProximity.UpdateColor(inRange: true);
			}
			UiMissionStatusDB.RemoveDB();
			mInProximity = true;
			mGraceTime = 0f;
		}
	}

	public virtual void OutOfProximity(AIActor Actor)
	{
		if (mInProximity)
		{
			if (_DrawProximity != null)
			{
				_DrawProximity.UpdateColor(inRange: false);
			}
			mInProximity = false;
		}
	}

	public virtual bool IsMoving(AIActor Actor)
	{
		return (Actor.transform.position - mPreviousPosition).sqrMagnitude > 0.0001f;
	}

	protected virtual void SetSpeed(AIActor Actor, float speed)
	{
		if (mSpline != null && Actor._Character.mSpline != null)
		{
			Actor._Character.Speed = speed;
		}
		else if (mTarget != null && mNavMeshAgent != null && mNavMeshAgent.hasPath)
		{
			Actor._Speed = speed;
			mNavMeshAgent.speed = speed;
		}
	}

	public virtual void DoUpdate(AIActor Actor)
	{
		if (mAction != null)
		{
			mAction();
			mAction = null;
		}
		if (mMissionData != null)
		{
			if (mTarget == null)
			{
				mTarget = GetTarget();
			}
			ProcessMove(Actor);
			if (!Actor.pHasController)
			{
				ProcessProximity(Actor);
				ProcessTime();
			}
		}
	}

	public virtual void PlayAnimation(AIActor Actor)
	{
		Vector3 position = Actor.Position;
		position.y += _GroundCheckStartHeight;
		float groundHeight;
		Vector3 normal;
		Collider groundHeight2 = UtUtilities.GetGroundHeight(position, _GroundCheckDist, out groundHeight, out normal);
		bool flag = IsSwimming(groundHeight2);
		if (IsMoving(Actor))
		{
			PlayAnim(flag ? "Swim" : "Run");
		}
		else
		{
			PlayAnim(flag ? "SwimIdle" : "Idle");
		}
	}

	public bool IsSwimming(Collider c)
	{
		if (c != null)
		{
			return c.gameObject.layer == mWaterLayer;
		}
		return false;
	}

	public bool IsFacing(Transform obj1, Transform obj2)
	{
		return Vector3.Dot(obj1.forward, obj2.forward) < 0f;
	}

	public bool IsCloseBy(Transform obj1, Transform obj2, float distance)
	{
		if (Vector3.Distance(obj1.position, obj2.position) < distance)
		{
			return true;
		}
		return false;
	}

	protected virtual bool CanMove(AIActor Actor)
	{
		return mInProximity;
	}

	protected void PlayAnim(string action)
	{
		string text = mActor.GetAnimationName(action);
		if (string.IsNullOrEmpty(text))
		{
			text = action;
		}
		if (mActor._Animation != null && !mCurrentAnim.Equals(text))
		{
			if (mActor._Animation.GetClip(text) != null)
			{
				mCurrentAnim = text;
				mActor._Animation.CrossFade(text);
			}
			else
			{
				UtDebug.Log("Anim not present  : " + base.gameObject.name + "_" + text);
			}
		}
	}

	public void SetData(object data)
	{
		mTask = data as Task;
		if (mTask != null)
		{
			mMissionData = new MissionData();
			mMissionData.TotalTime = GetData<float>("Time");
			mMissionData.GraceTime = GetData<float>("GraceTime");
			mMissionData.Proximity = GetData<float>("Proximity");
			mMissionData.SplineObjectName = GetData<string>("Spline");
			base.transform.parent.BroadcastMessage("SetProximity", mMissionData.Proximity, SendMessageOptions.DontRequireReceiver);
			mTarget = null;
		}
	}

	public virtual void ProcessFly(AIActor Actor)
	{
		if (mTarget != null && CanMove(Actor))
		{
			MoveOnBreadcrumb(Actor);
			float num = Actor.collider.bounds.size.y / 2f;
			AddBreadcrumb(new Vector3(mTarget.position.x, mTarget.position.y + num + _BreadcrumbHeightOffset, mTarget.position.z));
		}
		else
		{
			ResetBreadcrumb();
		}
	}

	public virtual TYPE GetData<TYPE>(string key)
	{
		if (mTask != null)
		{
			return mTask.GetObjectiveValue<TYPE>(key);
		}
		return default(TYPE);
	}

	public override AIBehaviorState Think(AIActor Actor)
	{
		if (mStatus == TaskStatus.IN_PROGRESS)
		{
			DoUpdate(Actor);
		}
		int num = ((mStatus == TaskStatus.IN_PROGRESS) ? 1 : 3);
		if (num == 3)
		{
			EndBehavior();
		}
		return (AIBehaviorState)num;
	}

	protected bool IsTaskCompleted()
	{
		if (mTask != null)
		{
			return mTask.pCompleted;
		}
		return false;
	}

	protected void TeleportToStartpoint()
	{
		if (mTask == null)
		{
			return;
		}
		foreach (TaskSetup setup in mTask.GetSetups())
		{
			string[] array = setup.Asset.Split('/');
			if (!string.IsNullOrEmpty(setup.Location) && mActor.gameObject.name == array[^1])
			{
				GameObject gameObject = GameObject.Find(setup.Location);
				if (gameObject != null)
				{
					TeleportTo(gameObject.transform);
				}
				break;
			}
		}
	}

	protected void TeleportTo(Transform location)
	{
		TeleportTo(location.position, location.rotation);
	}

	protected void TeleportTo(Vector3 position, Quaternion rotation)
	{
		UtUtilities.TeleportObjectToPosition(mActor.gameObject, position, rotation, 0f, useEffect: true, _TeleportFx);
	}

	protected void LateUpdate()
	{
		if (mArrow != null && mArrow.activeInHierarchy)
		{
			Vector3 position = Camera.main.ScreenToWorldPoint(new Vector3(Screen.width / 2, 0f + _VerticalOffsetScreen, _ForwardOffsetWorld));
			mArrow.transform.position = position;
			if (mNPC != null)
			{
				mArrow.transform.LookAt(mNPC.transform);
			}
		}
	}

	protected void ShowProximityArrow(bool enable)
	{
		if (enable)
		{
			if (mArrow == null)
			{
				mArrow = UnityEngine.Object.Instantiate((GameObject)RsResourceManager.LoadAssetFromResources("PfProximityArrow"));
			}
			if (mTask != null && mTask.pData != null && (mTask.pData.Type == "Follow" || mTask.pData.Type == "Escort") && mTask.pData.Objectives.Count > 0 && mTask.pData.Objectives[0]._NPC != null)
			{
				mNPC = mTask.pData.Objectives[0]._NPC;
			}
			else
			{
				mNPC = null;
			}
			mArrow.SetActive(value: true);
		}
		else if (mArrow != null)
		{
			mArrow.SetActive(value: false);
		}
		((MissionManagerDO)MissionManager.pInstance).ShowQuestArrow(!enable);
	}

	public virtual void SetMoveStateSpeed(AIActor actor)
	{
	}

	public void InitBreadcrumb()
	{
		if (mCustomBreadcrumbArray == null)
		{
			mCustomBreadcrumbArray = new CustomCircularArray<Vector3>(_MaxBreadcrumbCanPlace, Vector3.zero);
		}
		ResetBreadcrumb();
	}

	public void ResetBreadcrumb()
	{
		mCustomBreadcrumbArray.Clear();
		mTargetBreadcrumb = mCustomBreadcrumbArray.pInvalideValue;
	}

	public void AddBreadcrumb(Vector3 breadCrumbPos)
	{
		if (mCustomBreadcrumbArray.IsEmpty())
		{
			mCustomBreadcrumbArray.Add(breadCrumbPos);
		}
		else if ((mCustomBreadcrumbArray.GetLastItem() - breadCrumbPos).sqrMagnitude >= _BreadcrumbDistance * _BreadcrumbDistance)
		{
			mCustomBreadcrumbArray.Add(breadCrumbPos);
		}
	}

	public void MoveOnBreadcrumb(AIActor Actor)
	{
		Transform transformOnFlying = Actor.GetTransformOnFlying();
		Vector3 position = mTargetBreadcrumb;
		if (mTargetBreadcrumb == mCustomBreadcrumbArray.pInvalideValue && mCustomBreadcrumbArray.pCount <= 1)
		{
			position = mTarget.position;
		}
		Vector3 vector = position - transformOnFlying.transform.position;
		float sqrMagnitude = vector.sqrMagnitude;
		Vector3 normalized = vector.normalized;
		float num = Actor._StoppingDistance * Actor._StoppingDistance;
		if (Vector3.Distance(transformOnFlying.position, mTarget.position) > Actor._StoppingDistance)
		{
			float num2 = (Vector3.Distance(transformOnFlying.position, mTarget.position) - Actor._StoppingDistance) / Actor._DecelerateRange;
			if (Vector3.Distance(transformOnFlying.position, mTarget.position) > Actor._DecelerateRange || Actor.CanAccelerate)
			{
				if (Actor.Velocity >= Actor._Speed)
				{
					Actor.CanAccelerate = false;
				}
				else
				{
					Actor.Velocity += Time.deltaTime * Actor._Speed;
				}
			}
			else if (Actor.Velocity <= 0f)
			{
				Actor.CanAccelerate = true;
			}
			else
			{
				Actor.Velocity -= Time.deltaTime * Actor._Speed;
			}
			Actor.Velocity = Mathf.Clamp(Actor.Velocity, 0f, Actor._Speed);
			float num3 = Actor.Velocity * Time.deltaTime + 0.5f * Actor._Acceleration * (Time.deltaTime * Time.deltaTime);
			transformOnFlying.position += normalized * num3 * num2;
			Quaternion rotation = Quaternion.Lerp(transformOnFlying.rotation, Quaternion.LookRotation(normalized), Time.deltaTime * Actor._Speed);
			rotation.x = transformOnFlying.rotation.x;
			rotation.z = transformOnFlying.rotation.z;
			transformOnFlying.rotation = rotation;
		}
		else
		{
			Actor.CanAccelerate = false;
			Actor.Velocity = 0f;
		}
		if (mTargetBreadcrumb != mCustomBreadcrumbArray.pInvalideValue)
		{
			if (sqrMagnitude <= num && mCustomBreadcrumbArray.pCount >= 2)
			{
				mCustomBreadcrumbArray.TryGetNextItem(out mTargetBreadcrumb);
			}
		}
		else
		{
			mCustomBreadcrumbArray.TryGetNextItem(out mTargetBreadcrumb);
		}
	}

	public NavMeshPathStatus GetPathStatus(AIActor actor)
	{
		if (mTarget != null && mNavMeshAgent != null)
		{
			NavMeshPath navMeshPath = new NavMeshPath();
			if (NavMesh.CalculatePath(actor.transform.position, mTarget.position, -1, navMeshPath))
			{
				return navMeshPath.status;
			}
		}
		return NavMeshPathStatus.PathInvalid;
	}
}
