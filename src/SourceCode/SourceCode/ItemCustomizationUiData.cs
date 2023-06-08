using System;
using UnityEngine;

[Serializable]
public class ItemCustomizationUiData
{
	public int _ItemCategoryID = -1;

	public string _ResourcePath = "";

	public bool _MultiItemCustomizationUI;

	public ItemCustomizationData[] _CustomizationConfigArray;

	public Vector3 _ThumbnailCameraOffset = Vector3.zero;

	public bool _PaidCustomization;

	public int _CustomizationTicketStoreId;

	public int _CustomizationTicketId;

	public bool _ShowAvatar;
}
