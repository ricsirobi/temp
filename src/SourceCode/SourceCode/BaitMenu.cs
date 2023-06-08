using UnityEngine;

public class BaitMenu : MonoBehaviour
{
	public delegate void OnShowHandler(bool show);

	public UiInventoryCategory _Ui;

	public int _Category = 407;

	private GameObject mBaitMenuGO;

	private int mLastEquippedBaitId;

	public FishingZone _FishingZone;

	public Transform _MenuMarker;

	public AudioClip _SndSelectBait;

	public AudioClip _SndOpenBaitBox;

	public event OnShowHandler OnShow;

	private void Start()
	{
		if (null != _Ui)
		{
			_Ui._Menu.pEvents.OnClick += ItemSelected;
			_Ui._CategoryId = _Category;
			_Ui.SetVisibility(inVisible: false);
		}
		base.transform.position = new Vector3(base.transform.position.x, base.transform.position.y, 0f);
		PositionBaitMenu();
	}

	private void ItemSelected(KAWidget inWidget)
	{
		if (null != _SndSelectBait)
		{
			SnChannel.Play(_SndSelectBait, "SFX_Pool", inForce: true, null);
		}
		InventoryItemDataGeneric inventoryItemDataGeneric = (InventoryItemDataGeneric)inWidget.GetUserData();
		AvatarEquipment.pInstance.EquipItem(EquipmentParts.BAIT, inventoryItemDataGeneric._ItemID);
		if (null != _FishingZone)
		{
			_FishingZone.SendMessage("BaitSelected");
		}
		Object.Destroy(base.gameObject);
	}

	public void Show()
	{
		if (null != _Ui)
		{
			_Ui.SetVisibility(inVisible: true);
			_Ui.ShowMenu();
			if (this.OnShow != null && _Ui._Menu.GetItemCount() > 0)
			{
				this.OnShow(show: true);
			}
		}
	}

	private void OnDestroy()
	{
		if (null != _Ui)
		{
			_Ui._Menu.pEvents.OnClick -= ItemSelected;
		}
	}

	public void OnClick()
	{
		if (null != _SndOpenBaitBox)
		{
			SnChannel.Play(_SndOpenBaitBox, "SFX_Pool", inForce: true, null);
		}
		Show();
	}

	public void SetNoBaitText(LocaleString message)
	{
		if (null != _Ui)
		{
			_Ui.SetNoBaitText(message);
		}
	}

	private void PositionBaitMenu()
	{
		if (null != _Ui && null != _Ui._Menu && _Ui._Menu.GetVisibility() && _MenuMarker != null)
		{
			Vector2 vector = AvAvatar.pAvatarCam.GetComponent<Camera>().WorldToScreenPoint(_MenuMarker.position);
			float z = _Ui._Menu.gameObject.transform.parent.transform.position.z;
			Vector3 position = KAUIManager.pInstance.camera.ScreenToWorldPoint(new Vector2(vector.x, vector.y));
			position.z = z;
			float num = (float)(_Ui._Menu._Template.pBackground.width * _Ui._Menu.GetItemCount()) * 0.5f;
			position.x -= num;
			position.y += (float)_Ui._Menu._Template.pBackground.height * 0.5f;
			_Ui._Menu.gameObject.transform.parent.transform.position = position;
			_Ui.gameObject.transform.position = position;
		}
	}

	private void Update()
	{
		PositionBaitMenu();
	}

	public void OnStoreOpened()
	{
		if (null != _FishingZone)
		{
			_FishingZone.OnStoreOpened();
		}
	}

	public void OnStoreClosed()
	{
		Show();
		if (null != _FishingZone)
		{
			_FishingZone.OnStoreClosed();
		}
	}
}
