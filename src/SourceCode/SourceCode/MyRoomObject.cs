using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MyRoomObject : KAMonoBase
{
	public class ChildData
	{
		public GameObject _Child;

		public bool _HasToChangeConcave;

		public ChildData(GameObject obj, bool change)
		{
			_Child = obj;
			_HasToChangeConcave = change;
		}
	}

	public MyRoomObjectType _ObjectType = MyRoomObjectType.OnFloor;

	public bool _IsStackableObject;

	public bool _CanStackObject;

	public bool _IsDraggable = true;

	public float _DragYOffset;

	private ArrayList mCollidingObjectsList = new ArrayList();

	private List<ChildData> mChildList = new List<ChildData>();

	private bool mHasToChangeConcave;

	private GameObject mParentObject;

	private UserItemData mUserItemData;

	private GameObject[] mRayCastPoints;

	public ChildData[] pChildList => mChildList.ToArray();

	public GameObject pParentObject
	{
		get
		{
			return mParentObject;
		}
		set
		{
			if (mParentObject != null)
			{
				MyRoomObject component = mParentObject.GetComponent<MyRoomObject>();
				if (component != null)
				{
					component.RemoveChildReference(base.gameObject);
				}
			}
			mParentObject = value;
		}
	}

	public UserItemData pUserItemData
	{
		get
		{
			return mUserItemData;
		}
		set
		{
			mUserItemData = value;
		}
	}

	public GameObject[] pRayCastPoints
	{
		get
		{
			return mRayCastPoints;
		}
		set
		{
			mRayCastPoints = value;
		}
	}

	public void Start()
	{
	}

	public bool IsObjectColliding(GameObject[] ignoreObjects)
	{
		foreach (ChildData mChild in mChildList)
		{
			if (mChild._Child.GetComponent<MyRoomObject>().IsObjectColliding(new GameObject[1] { mParentObject }))
			{
				Debug.Break();
				return true;
			}
		}
		if (mCollidingObjectsList.Count == 0)
		{
			return false;
		}
		int num = 0;
		if (ignoreObjects != null)
		{
			foreach (GameObject gameObject in ignoreObjects)
			{
				if (gameObject != null && mCollidingObjectsList.Contains(gameObject.GetComponent<Collider>()))
				{
					num++;
				}
			}
		}
		if (num == mCollidingObjectsList.Count)
		{
			return false;
		}
		return true;
	}

	private void AddCollisionObject(Collider collider)
	{
		int layer = collider.gameObject.layer;
		int num = LayerMask.NameToLayer("Wall");
		if (_ObjectType == MyRoomObjectType.WallHanging)
		{
			num = LayerMask.NameToLayer("Floor");
		}
		if ((layer == LayerMask.NameToLayer("Furniture") || layer == LayerMask.NameToLayer("Avatar") || layer == LayerMask.NameToLayer("MMOAvatar") || layer == LayerMask.NameToLayer("SafeZoneArea") || layer == num || layer == LayerMask.NameToLayer("Window") || layer == LayerMask.NameToLayer("Default") || layer == LayerMask.NameToLayer("Collectibles")) && !mCollidingObjectsList.Contains(collider))
		{
			mCollidingObjectsList.Add(collider);
		}
	}

	private void RemoveCollisionObject(Collider collider)
	{
		if (mCollidingObjectsList.Contains(collider))
		{
			mCollidingObjectsList.Remove(collider);
		}
	}

	public void ClearCollisionList()
	{
		mCollidingObjectsList.Clear();
	}

	public void EnableTrigger(bool isEnable)
	{
		if (collider != null)
		{
			if (collider is MeshCollider)
			{
				MeshCollider meshCollider = (MeshCollider)collider;
				if (isEnable)
				{
					if (!meshCollider.convex)
					{
						mHasToChangeConcave = true;
						meshCollider.convex = true;
					}
					collider.isTrigger = isEnable;
				}
				else
				{
					collider.isTrigger = isEnable;
					if (mHasToChangeConcave)
					{
						mHasToChangeConcave = false;
						meshCollider.convex = false;
					}
				}
			}
			else
			{
				collider.isTrigger = isEnable;
			}
		}
		foreach (ChildData mChild in mChildList)
		{
			Collider component = mChild._Child.GetComponent<Collider>();
			if (!(component != null))
			{
				continue;
			}
			if (component is MeshCollider)
			{
				MeshCollider meshCollider2 = (MeshCollider)component;
				if (isEnable)
				{
					if (!meshCollider2.convex)
					{
						mChild._HasToChangeConcave = true;
						meshCollider2.convex = true;
					}
					component.isTrigger = isEnable;
				}
				else
				{
					component.isTrigger = isEnable;
					if (mChild._HasToChangeConcave)
					{
						mChild._HasToChangeConcave = false;
						meshCollider2.convex = false;
					}
				}
			}
			else
			{
				component.isTrigger = isEnable;
			}
		}
	}

	public void AddChildReference(GameObject inObject)
	{
		if (inObject != null && mChildList.Find((ChildData c) => c._Child == inObject) == null)
		{
			mChildList.Add(new ChildData(inObject, change: false));
		}
	}

	private void RemoveChildReference(GameObject inObject)
	{
		if (inObject != null)
		{
			ChildData childData = mChildList.Find((ChildData c) => c._Child == inObject);
			if (childData != null)
			{
				mChildList.Remove(childData);
			}
		}
	}
}
