using System;
using System.Globalization;
using System.Xml.Serialization;
using UnityEngine;

[Serializable]
[XmlRoot(ElementName = "ImageData", Namespace = "", IsNullable = true)]
public class ImageData
{
	[XmlElement(ElementName = "ImageURL")]
	public string ImageURL;

	[XmlElement(ElementName = "TemplateName")]
	public string TemplateName;

	[XmlElement(ElementName = "SubType", IsNullable = true)]
	public string SubType;

	[XmlElement(ElementName = "PhotoFrame", IsNullable = true)]
	public string PhotoFrame;

	[XmlElement(ElementName = "PhotoFrameMask", IsNullable = true)]
	public string PhotoFrameMask;

	[XmlElement(ElementName = "Border", IsNullable = true)]
	public string Border;

	[XmlElement(ElementName = "Decal")]
	public ImageDataDecal[] Decal;

	public const string IMAGE_TYPE_DRAWING = "Drawing";

	public const string IMAGE_TYPE_DRAWING_COLOR = "DrawingColor";

	public const string IMAGE_TYPE_PHOTO = "Photo";

	public const string IMAGE_TYPE_BILLBOARD = "Billboard";

	public const string IMAGE_TYPE_EGG_COLOR = "EggColor";

	public const string IMAGE_TYPE_SW_TRACK = "SWTrack";

	public const string IMAGE_TYPE_AVATAR = "Avatar";

	public const string IMAGE_TYPE_DRAGON_TEX = "DragonTexture";

	public const string ART_TYPE_COLORING = "Color (Coloring Book)";

	public const string ART_TYPE_PHOTOFRAME = "Photo";

	public const string ART_TYPE_CARD = "Card";

	public static ImageDataInstance[] mPhoto;

	public static ImageDataInstance[] mArtFinal;

	public static ImageDataInstance[] mArtColor;

	public static ImageDataInstance[] mBillboard;

	public static ImageDataInstance[] mEggColor;

	public static ImageDataInstance[] mSWTracks;

	public static ImageDataInstance[] mAvatars;

	public static ImageDataInstance[] GetImageList(string iType)
	{
		switch (iType)
		{
		case "Drawing":
			return mArtFinal;
		case "DrawingColor":
			return mArtColor;
		case "Photo":
			return mPhoto;
		case "Billboard":
			return mBillboard;
		case "EggColor":
			return mEggColor;
		case "SWTrack":
			return mSWTracks;
		case "Avatar":
			return mAvatars;
		default:
			Debug.LogError("Image array " + iType + " not found ");
			return null;
		}
	}

	public void CopyData(ImageDataInstance id)
	{
		ImageURL = id.ImageURL;
		TemplateName = id.TemplateName;
		Decal = id.Decal;
		Border = id.Border;
		PhotoFrameMask = id.PhotoFrameMask;
		PhotoFrame = id.PhotoFrame;
		SubType = id.SubType;
		ImageURL = "";
		if (TemplateName == null || TemplateName.Length == 0)
		{
			TemplateName = "T";
		}
	}

	public static void Init(string iType, int numSlots)
	{
		switch (iType)
		{
		case "Drawing":
			if (mArtFinal == null)
			{
				mArtFinal = new ImageDataInstance[numSlots];
				mArtColor = new ImageDataInstance[numSlots];
			}
			break;
		case "Photo":
			if (mPhoto == null)
			{
				mPhoto = new ImageDataInstance[numSlots];
			}
			break;
		case "Billboard":
			if (mBillboard == null)
			{
				mBillboard = new ImageDataInstance[numSlots];
			}
			break;
		case "EggColor":
			if (mEggColor == null)
			{
				mEggColor = new ImageDataInstance[numSlots];
			}
			break;
		case "SWTrack":
			if (mSWTracks == null)
			{
				mSWTracks = new ImageDataInstance[numSlots];
			}
			break;
		case "Avatar":
			if (mAvatars == null)
			{
				mAvatars = new ImageDataInstance[numSlots];
			}
			break;
		}
	}

	public static void Reset()
	{
		mArtFinal = null;
		mArtColor = null;
		mPhoto = null;
		mBillboard = null;
		mEggColor = null;
		mSWTracks = null;
		mAvatars = null;
	}

	public static ImageDataInstance FindImage(ImageDataInstance[] iData, string inURL)
	{
		if (iData != null)
		{
			foreach (ImageDataInstance imageDataInstance in iData)
			{
				if (imageDataInstance != null && imageDataInstance.ImageURL == inURL)
				{
					return imageDataInstance;
				}
			}
		}
		return null;
	}

	public static int GetIndexFromImageName(string iname)
	{
		string[] array = iname.Split('/');
		if (array.Length == 2)
		{
			return int.Parse(array[1], NumberStyles.Number);
		}
		Debug.LogError("Wrong string for image index");
		return 0;
	}

	public static string GetImageName(string iType, int slotIdx)
	{
		return iType + "/" + slotIdx;
	}

	public static void OnResourceReady(string inURL, object inObject)
	{
		UnityEngine.Object.DontDestroyOnLoad((Texture2D)inObject);
		ImageDataInstance imageDataInstance = FindImage(mArtFinal, inURL);
		if (imageDataInstance != null)
		{
			imageDataInstance.mIconTexture = (Texture2D)inObject;
			imageDataInstance.mIconTexture.name = GetImageName("Drawing", imageDataInstance.mSlotIndex);
			imageDataInstance.OnReady();
			return;
		}
		imageDataInstance = FindImage(mArtColor, inURL);
		if (imageDataInstance != null)
		{
			imageDataInstance.mIconTexture = (Texture2D)inObject;
			imageDataInstance.mIconTexture.name = GetImageName("DrawingColor", imageDataInstance.mSlotIndex);
			imageDataInstance.OnReady();
			return;
		}
		imageDataInstance = FindImage(mPhoto, inURL);
		if (imageDataInstance != null)
		{
			imageDataInstance.mIconTexture = (Texture2D)inObject;
			imageDataInstance.mIconTexture.name = GetImageName("Photo", imageDataInstance.mSlotIndex);
			imageDataInstance.OnReady();
			return;
		}
		imageDataInstance = FindImage(mBillboard, inURL);
		if (imageDataInstance != null)
		{
			imageDataInstance.mIconTexture = (Texture2D)inObject;
			imageDataInstance.mIconTexture.name = GetImageName("Billboard", imageDataInstance.mSlotIndex);
			imageDataInstance.OnReady();
			return;
		}
		imageDataInstance = FindImage(mEggColor, inURL);
		if (imageDataInstance != null)
		{
			imageDataInstance.mIconTexture = (Texture2D)inObject;
			imageDataInstance.mIconTexture.name = GetImageName("EggColor", imageDataInstance.mSlotIndex);
			imageDataInstance.OnReady();
			return;
		}
		imageDataInstance = FindImage(mSWTracks, inURL);
		if (imageDataInstance != null)
		{
			imageDataInstance.mIconTexture = (Texture2D)inObject;
			imageDataInstance.mIconTexture.name = GetImageName("SWTrack", imageDataInstance.mSlotIndex);
			imageDataInstance.OnReady();
		}
	}

	public static void OnResLoadingEvent(string inURL, RsResourceLoadEvent inEvent, float inProgress, object inObject, object inUserData)
	{
		switch (inEvent)
		{
		case RsResourceLoadEvent.COMPLETE:
			OnResourceReady(inURL, inObject);
			break;
		case RsResourceLoadEvent.ERROR:
		{
			ImageDataInstance imageDataInstance = FindImage(mEggColor, inURL);
			if (imageDataInstance != null)
			{
				imageDataInstance.mIsDownLoading = false;
			}
			break;
		}
		case RsResourceLoadEvent.PROGRESS:
			break;
		}
	}

	public static void WsSetEventHandler(WsServiceType inType, WsServiceEvent inEvent, float inProgress, object inObject, object inUserData)
	{
	}

	public static void WsGetEventHandler(WsServiceType inType, WsServiceEvent inEvent, float inProgress, object inObject, object inUserData)
	{
		if (inEvent == WsServiceEvent.COMPLETE && inType == WsServiceType.GET_IMAGE)
		{
			ImageUserData imageUserData = (ImageUserData)inUserData;
			ImageDataInstance imageDataInstance = GetImageList(imageUserData._Type)[imageUserData._Index];
			if (inObject != null)
			{
				ImageData imageData = (ImageData)inObject;
				imageDataInstance.ImageURL = imageData.ImageURL;
				imageDataInstance.TemplateName = imageData.TemplateName;
				imageDataInstance.Decal = imageData.Decal;
				imageDataInstance.Border = imageData.Border;
				imageDataInstance.PhotoFrameMask = imageData.PhotoFrameMask;
				imageDataInstance.PhotoFrame = imageData.PhotoFrame;
				imageDataInstance.SubType = imageData.SubType;
				RsResourceManager.Load(imageDataInstance.ImageURL, OnResLoadingEvent);
			}
			else
			{
				imageDataInstance.OnReady();
			}
		}
	}

	public static void LoadUserImage(string userID, string iType, int slotIdx, GameObject msgObject)
	{
	}

	public static void Load(string iType, int slotIdx, GameObject msgObject)
	{
		Load(iType, slotIdx, msgObject, "OnImageLoaded");
	}

	public static void Load(string iType, int slotIdx, GameObject msgObject, string readyMsg)
	{
		ImageDataInstance[] imageList = GetImageList(iType);
		ImageDataInstance imageDataInstance = imageList[slotIdx];
		if (imageDataInstance != null && imageDataInstance.mIsLoaded && (imageDataInstance.mIconTexture != null || imageDataInstance.ImageURL != null))
		{
			imageDataInstance.mMsgObject.Add(msgObject);
			imageDataInstance.mReadyMsg = readyMsg;
			if (imageDataInstance.mIconTexture != null)
			{
				imageDataInstance.OnReady();
			}
			else if (imageDataInstance.ImageURL != null)
			{
				imageDataInstance.mIsLoaded = false;
				RsResourceManager.Load(imageDataInstance.ImageURL, OnResLoadingEvent);
			}
			return;
		}
		ImageUserData imageUserData = new ImageUserData();
		imageUserData._Type = iType;
		imageUserData._Index = slotIdx;
		imageUserData._MsgObject = msgObject;
		if (imageDataInstance != null && imageDataInstance.mIsDownLoading)
		{
			imageDataInstance.mMsgObject.Add(msgObject);
			return;
		}
		if (imageDataInstance == null)
		{
			imageDataInstance = new ImageDataInstance();
		}
		imageDataInstance.mIconTexture = null;
		imageDataInstance.mMsgObject.Add(msgObject);
		imageDataInstance.mType = iType;
		imageDataInstance.mSlotIndex = slotIdx;
		imageDataInstance.mIsDirty = false;
		imageDataInstance.mDeleteMe = false;
		imageDataInstance.mIsDownLoading = true;
		imageList[slotIdx] = imageDataInstance;
		WsWebService.GetImageData(iType, slotIdx, WsGetEventHandler, imageUserData);
	}

	public static void Save(string iType, int slotIdx, Texture2D tex)
	{
		ImageDataInstance[] imageList = GetImageList(iType);
		ImageDataInstance imageDataInstance = imageList[slotIdx];
		if (imageDataInstance == null)
		{
			if (tex == null)
			{
				WsWebService.DeleteImageData(iType, slotIdx, WsSetEventHandler, null);
				UtDebug.Log("------- Deleting " + iType + " from index = " + slotIdx);
				imageDataInstance = (imageList[slotIdx] = new ImageDataInstance());
				imageDataInstance.mIsDirty = false;
				imageDataInstance.mIconTexture = null;
				imageDataInstance.mSlotIndex = slotIdx;
				imageDataInstance.mType = iType;
				imageDataInstance.mIsLoaded = true;
				imageDataInstance.mIsDownLoading = false;
				return;
			}
			imageDataInstance = (imageList[slotIdx] = new ImageDataInstance());
			imageDataInstance.mIconTexture = null;
		}
		imageDataInstance.mSlotIndex = slotIdx;
		imageDataInstance.mType = iType;
		imageDataInstance.mIsLoaded = true;
		imageDataInstance.mDeleteMe = false;
		if (tex == null && imageDataInstance.mIconTexture != null)
		{
			imageDataInstance.mDeleteMe = true;
		}
		else if (tex == imageDataInstance.mIconTexture)
		{
			return;
		}
		imageDataInstance.mIconTexture = tex;
		UnityEngine.Object.DontDestroyOnLoad(imageDataInstance.mIconTexture);
		imageDataInstance.mIsDirty = true;
	}

	public static ImageDataInstance GetImage(string iType, int slotIdx)
	{
		return GetImageList(iType)[slotIdx];
	}

	public static void SetImage(string iType, int slotIdx, ImageDataInstance newImage, bool deleteOld)
	{
		ImageDataInstance[] imageList = GetImageList(iType);
		if (deleteOld && imageList[slotIdx] != null)
		{
			_ = imageList[slotIdx].mIconTexture != null;
		}
		if (newImage != null)
		{
			if (newImage.mIconTexture != null)
			{
				newImage.mIconTexture.name = GetImageName(iType, slotIdx);
				UnityEngine.Object.DontDestroyOnLoad(newImage.mIconTexture);
			}
			else if (imageList[slotIdx] != null)
			{
				newImage.mDeleteMe = true;
			}
			newImage.mDeleteMe = false;
			newImage.mIsDirty = true;
			newImage.mIsLoaded = true;
			newImage.mType = iType;
			newImage.mSlotIndex = slotIdx;
			imageList[slotIdx] = newImage;
		}
		else
		{
			imageList[slotIdx] = new ImageDataInstance();
			imageList[slotIdx].mSlotIndex = slotIdx;
			imageList[slotIdx].mType = iType;
			imageList[slotIdx].mIconTexture = null;
			imageList[slotIdx].mIsDirty = true;
			imageList[slotIdx].mDeleteMe = true;
		}
	}

	public static void UpdateImages(ImageDataInstance[] iData)
	{
		ImageData imageData = new ImageData();
		foreach (ImageDataInstance imageDataInstance in iData)
		{
			if (imageDataInstance != null && imageDataInstance.mIsDirty)
			{
				imageDataInstance.mIsLoaded = true;
				imageDataInstance.mIsDirty = false;
				ImageUserData imageUserData = new ImageUserData();
				imageUserData._Type = imageDataInstance.mType;
				imageUserData._Index = imageDataInstance.mSlotIndex;
				imageUserData._MsgObject = null;
				if (imageDataInstance.mDeleteMe)
				{
					imageDataInstance.mDeleteMe = false;
					WsWebService.DeleteImageData(imageDataInstance.mType, imageDataInstance.mSlotIndex, WsSetEventHandler, imageUserData);
					UtDebug.Log("------- Deleting " + imageDataInstance.mType + " from index = " + imageDataInstance.mSlotIndex);
				}
				else
				{
					imageData.CopyData(imageDataInstance);
					UtDebug.Log("------- Saving " + imageDataInstance.GetDebugString());
					WsWebService.SetImageData(imageDataInstance.mType, imageDataInstance.mSlotIndex, imageDataInstance.mIconTexture, imageData, WsSetEventHandler, imageUserData);
				}
			}
		}
	}

	public static void UpdateImages(string iType)
	{
		UpdateImages(GetImageList(iType));
	}

	public static string[] GetObjectNamesInPhoto(ImageDataInstance pd)
	{
		int num = pd.Decal.Length;
		string[] array = new string[num];
		for (int i = 0; i < num; i++)
		{
			array[i] = pd.Decal[i].Name;
		}
		return array;
	}

	public static int GetSubTaskType(string artType)
	{
		return artType switch
		{
			"Color (Coloring Book)" => 2, 
			"Photo" => 3, 
			"Card" => 4, 
			_ => 1, 
		};
	}
}
