using UnityEngine;

public class SanctuaryPetItemDataLogin : SanctuaryPetItemData
{
	public SanctuaryPetItemDataLogin(RaisedPetData pdata, Vector3 pos, Quaternion rot, GameObject msgObj)
		: base(null, pdata, pos, rot, msgObj, "Basic", applyCustomSkin: false)
	{
	}

	public override void OnAllDownloaded()
	{
		if (m3DData != null && !(m3DData._Prefab == null) && !(m3DData._Prefab.gameObject == null))
		{
			GameObject gameObject = Object.Instantiate(m3DData._Prefab.gameObject, mPos, mRot);
			if (UtPlatform.IsEditor())
			{
				UtUtilities.ReAssignShader(gameObject);
			}
			SanctuaryPet component = gameObject.GetComponent<SanctuaryPet>();
			if (!(component == null) && mPetData != null)
			{
				mPetData.pTexture = (Texture2D)mTexture;
				mPetData.pTextureBMP = null;
				component._AnimBundles = mAnimBundles;
				component._MessageObject = mMessageObject;
				component._IdleAnimName = "IdleSit";
				component.Init(mPetData, noHat: false);
			}
		}
	}
}
