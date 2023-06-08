using System.Collections.Generic;
using UnityEngine;

public class ObCouch : MonoBehaviour
{
	public ObCouchAttributes[] _CouchAttributes;

	public GameObject _ChairIcon;

	public Vector3 _ChairIconOffset = new Vector3(0f, 2f, 0f);

	public AvAvatarCamParams _SittingViewCamParams;

	public string[] _AvatarSitAnims = new string[2] { "Sit01", "Sit02" };

	public Vector3 _AvatarLeaveCouchOffset;

	public float _Radius = 2f;

	public bool _ShowAvatarName;

	private static List<ObCouch> mCouchList = null;

	public static int pCurrentAvatarCouch = -1;

	private bool mShowingChairIcon;

	public static List<ObCouch> pCouchList
	{
		get
		{
			return mCouchList;
		}
		set
		{
			mCouchList = value;
		}
	}

	private void OnEnable()
	{
		if (mCouchList == null)
		{
			mCouchList = new List<ObCouch>();
			pCurrentAvatarCouch = -1;
		}
		mCouchList.Add(this);
		ObCouchAttributes[] couchAttributes = _CouchAttributes;
		for (int i = 0; i < couchAttributes.Length; i++)
		{
			couchAttributes[i]._SeatMarker.transform.parent = base.transform;
		}
	}

	private void OnDisable()
	{
		if (pCurrentAvatarCouch != -1)
		{
			ObCouchAttributes[] couchAttributes = _CouchAttributes;
			foreach (ObCouchAttributes obCouchAttributes in couchAttributes)
			{
				if (AvAvatar.IsCurrentPlayer(obCouchAttributes.pOccupiedAvatar))
				{
					LeaveCouch(obCouchAttributes);
					break;
				}
			}
		}
		if (mCouchList != null)
		{
			mCouchList.Remove(this);
		}
	}

	private void ShowChairIcon(ObCouchAttributes inCouch)
	{
		if (inCouch.pChairIcon == null)
		{
			Vector3 position = inCouch._SeatMarker.transform.position + inCouch._SeatMarker.transform.InverseTransformDirection(_ChairIconOffset);
			if (inCouch.pChairIcon == null)
			{
				GameObject gameObject = Object.Instantiate(_ChairIcon, position, Quaternion.identity);
				gameObject.transform.parent = base.transform;
				inCouch.pChairIcon = gameObject.GetComponent<ObCouchIcon>();
				if (inCouch.pChairIcon != null)
				{
					inCouch.pChairIcon._MessageObject = base.gameObject;
					inCouch.pChairIcon._Range = 15f;
					inCouch.pChairIcon._RollOverCursorName = "Activate";
					inCouch.pChairIcon.pCouchAttributes = inCouch;
				}
			}
		}
		inCouch.pChairIcon.SetActive(active: true);
	}

	private void OnClick(GameObject inGameObject)
	{
		ObCouchIcon component = inGameObject.GetComponent<ObCouchIcon>();
		if (component != null)
		{
			OccupyCouch(component.pCouchAttributes, AvAvatar.pObject);
		}
	}

	private void HideChairIcon(ObCouchAttributes couch)
	{
		if (couch.pChairIcon != null)
		{
			couch.pChairIcon.SetActive(active: false);
		}
	}

	private void Update()
	{
		bool flag = IsToUpdateChairIcon();
		ObCouchAttributes[] couchAttributes = _CouchAttributes;
		foreach (ObCouchAttributes obCouchAttributes in couchAttributes)
		{
			if (flag)
			{
				if (mShowingChairIcon && obCouchAttributes.pOccupiedAvatar == null)
				{
					ShowChairIcon(obCouchAttributes);
				}
				else
				{
					HideChairIcon(obCouchAttributes);
				}
			}
			if (obCouchAttributes.pOccupiedAvatar == null)
			{
				continue;
			}
			if (AvAvatar.IsCurrentPlayer(obCouchAttributes.pOccupiedAvatar))
			{
				if (AvAvatar.pToolbar.activeInHierarchy || AvAvatar.pState == AvAvatarState.PAUSED)
				{
					float axis = Input.GetAxis("Vertical");
					if (Input.GetKeyUp(KeyCode.Space) || axis > 0f || AvAvatar.pState != 0 || AvAvatar.IsPlayerOnRide())
					{
						LeaveCouch(obCouchAttributes);
					}
				}
				else if (MainStreetMMOClient.pInstance != null)
				{
					MainStreetMMOClient.pInstance.ResetDisconnectTimer();
				}
			}
			else if (obCouchAttributes.pOccupiedAvatar.GetComponent<AvAvatarController>().pState != 0)
			{
				LeaveCouch(obCouchAttributes);
			}
		}
	}

	public void OccupyCouch(ObCouchAttributes inCa, GameObject inAvatarObject)
	{
		if (inCa.pOccupiedAvatar != null)
		{
			UtDebug.LogWarning("Already avatar sitting in " + inCa._ID);
			return;
		}
		if (AvAvatar.IsCurrentPlayer(inAvatarObject))
		{
			pCurrentAvatarCouch = inCa._ID;
			SetAvatarCam(_SittingViewCamParams);
			SendAvatarCouchID();
			if (!_ShowAvatarName)
			{
				AvAvatar.SetDisplayNameVisible(inVisible: false);
			}
			SetTurnArrows(visible: false);
		}
		AvAvatarController component = inAvatarObject.GetComponent<AvAvatarController>();
		component.pState = AvAvatarState.NONE;
		if (component.pPetObject != null)
		{
			component.pPetObject._FollowFront = true;
		}
		inCa.pOccupiedAvatar = inAvatarObject;
		inAvatarObject.transform.position = inCa._SeatMarker.transform.position;
		inAvatarObject.transform.rotation = inCa._SeatMarker.transform.rotation;
		string inAnimName = _AvatarSitAnims[Random.Range(0, _AvatarSitAnims.Length)];
		AvAvatar.PlayAnim(inAvatarObject, inAnimName, WrapMode.Loop);
		HideChairIcon(inCa);
	}

	public void LeaveCouch(ObCouchAttributes inCa)
	{
		if (AvAvatar.IsCurrentPlayer(inCa.pOccupiedAvatar))
		{
			if (!AvAvatar.IsPlayerOnRide())
			{
				AvAvatar.SetPosition(inCa._SeatMarker.transform.position + inCa._SeatMarker.transform.rotation * _AvatarLeaveCouchOffset);
			}
			if (AvAvatar.pState == AvAvatarState.NONE)
			{
				AvAvatar.pState = AvAvatarState.IDLE;
			}
			pCurrentAvatarCouch = -1;
			SetAvatarCam(null);
			SendAvatarCouchID();
			AvAvatar.SetDisplayNameVisible(inVisible: true);
			SetTurnArrows(visible: true);
		}
		AvAvatarController component = inCa.pOccupiedAvatar.GetComponent<AvAvatarController>();
		if (component.pPetObject != null)
		{
			component.pPetObject._FollowFront = false;
		}
		inCa.pOccupiedAvatar = null;
		if (mShowingChairIcon)
		{
			ShowChairIcon(inCa);
		}
	}

	public static void SetAvatarCam(AvAvatarCamParams inAvAvatarCamParams)
	{
		AvAvatarController component = AvAvatar.pObject.GetComponent<AvAvatarController>();
		CaAvatarCam component2 = AvAvatar.pAvatarCam.GetComponent<CaAvatarCam>();
		if (!(component == null) && !(component2 == null))
		{
			if (inAvAvatarCamParams == null)
			{
				component2.SetAvatarCamParams(component.pCurrentStateData._AvatarCameraParams);
			}
			else
			{
				component2.SetAvatarCamParams(inAvAvatarCamParams);
			}
			component2.ForceFreeRotate(inAvAvatarCamParams != null);
		}
	}

	public static void SendAvatarCouchID()
	{
		if (MainStreetMMOClient.pInstance != null)
		{
			MainStreetMMOClient.pInstance.SendCouchSitMessage(pCurrentAvatarCouch.ToString());
		}
	}

	private bool IsToUpdateChairIcon()
	{
		if (pCurrentAvatarCouch == -1 && !AvAvatar.IsPlayerOnRide() && InsideProximityRange())
		{
			if (!mShowingChairIcon)
			{
				mShowingChairIcon = true;
				return true;
			}
		}
		else if (mShowingChairIcon)
		{
			mShowingChairIcon = false;
			return true;
		}
		return false;
	}

	private bool InsideProximityRange()
	{
		if ((AvAvatar.GetPosition() - base.transform.position).magnitude <= _Radius)
		{
			return true;
		}
		return false;
	}

	private void SetTurnArrows(bool visible)
	{
		UiToolbar component = AvAvatar.pToolbar.GetComponent<UiToolbar>();
		if (component != null)
		{
			KAWidget kAWidget = component.FindItem("TurnLt");
			if (kAWidget != null)
			{
				kAWidget.SetVisibility(visible && AvatarData.pControlMode == 1);
			}
			KAWidget kAWidget2 = component.FindItem("TurnRt");
			if (kAWidget2 != null)
			{
				kAWidget2.SetVisibility(visible && AvatarData.pControlMode == 1);
			}
		}
	}
}
