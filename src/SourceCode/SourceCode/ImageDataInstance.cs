using System.Collections.Generic;
using UnityEngine;

public class ImageDataInstance : ImageData
{
	public int mSlotIndex = -1;

	public string mType = "";

	public List<GameObject> mMsgObject = new List<GameObject>();

	public string mReadyMsg = "OnImageLoaded";

	public Texture2D mIconTexture;

	public bool mIsLoaded;

	public bool mIsDownLoading;

	public bool mIsDirty;

	public bool mDeleteMe;

	public void OnReady()
	{
		mIsLoaded = true;
		mIsDownLoading = false;
		foreach (GameObject item in mMsgObject)
		{
			if (item != null)
			{
				item.SendMessage(mReadyMsg, this, SendMessageOptions.DontRequireReceiver);
			}
		}
		mMsgObject.Clear();
	}

	public string GetDebugString()
	{
		string text = "ImageData : Slot = " + mSlotIndex + " type = " + mType + "\n";
		text = text + "ImgeURL = " + ImageURL + "\n";
		if (mIconTexture != null)
		{
			text = text + "Texture Name : " + mIconTexture.name + "  Width = " + mIconTexture.width + "\n";
		}
		text = text + "Template = " + TemplateName + "\n";
		text = text + "Border = " + Border + "\n";
		if (Decal != null)
		{
			for (int i = 0; i < Decal.Length; i++)
			{
				text = text + "-- decal[" + i + "] name = " + Decal[i].Name + "(" + Decal[i].Position.X + "," + Decal[i].Position.Y + "," + Decal[i].Width + "," + Decal[i].Height + ")\n";
			}
		}
		return text;
	}
}
