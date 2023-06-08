using UnityEngine;

public class AvPhotoLoader
{
	public AvatarData.InstanceInfo pAvatarInst;

	public AvPhotoCallback _Callback;

	public object _UserData;

	public Texture2D _DstTexture;

	public AvPhotoManager _Manager;

	public string _UserID;

	public int pFrameCounter = 3;

	public bool Update()
	{
		if (pAvatarInst.pIsReady)
		{
			if (pFrameCounter > 0)
			{
				pFrameCounter--;
				return false;
			}
			Texture texture = null;
			texture = ((!_Manager.pPictureCache.ContainsKey(_UserID)) ? _Manager.TakeAShot(pAvatarInst.mAvatar, _DstTexture, _UserID) : _Manager.pPictureCache[_UserID]);
			if (_Callback != null)
			{
				_Callback(texture, _UserData);
			}
			return true;
		}
		return false;
	}
}
