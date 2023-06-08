using UnityEngine;
using UnityEngine.Networking;

public interface UtIWWWAsync
{
	UnityWebRequest pWebRequest { get; }

	string pURL { get; }

	RsResourceType pResourcetype { get; }

	bool pIsDone { get; }

	float pProgress { get; }

	string pError { get; }

	string pData { get; }

	byte[] pBytes { get; }

	Texture pTexture { get; }

	AudioClip pAudioClip { get; }

	AssetBundle pAssetBundle { get; }

	bool pFromCache { get; }

	void Download(string inURL, RsResourceType inType, UtWWWEventHandler inCallback, bool inSendProgressEvents, bool inDisableCache, bool inDownLoadOnly, bool inIgnoreAssetVersion);

	void DownloadBundle(string url, Hash128 hash, UtWWWEventHandler callback, bool sendProgressEvents, bool disableCache, bool downloadOnly);

	void PostForm(string inURL, WWWForm inForm, UtWWWEventHandler inCallback, bool inSendProgressEvents);

	bool Update();

	void Kill();

	void OnSceneLoaded(string inLevel);
}
