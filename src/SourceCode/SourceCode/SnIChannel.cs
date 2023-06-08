using UnityEngine;

public interface SnIChannel
{
	string pName { get; }

	string pPool { get; }

	int pPriority { get; set; }

	float pUseTime { get; }

	bool pInUse { get; }

	bool pIsStreaming { get; }

	bool pIsPlaying { get; }

	string pClipName { get; }

	AudioClip pClip { get; set; }

	AudioSource pAudioSource { get; }

	SnIStream pAudioStream { get; }

	GameObject pGameObject { get; }

	Transform pTransform { get; }

	float pVolume { get; set; }

	bool pLoop { get; set; }

	float pRolloffBegin { get; set; }

	float pRolloffEnd { get; set; }

	bool pUseRolloffDistance { get; set; }

	bool pReleaseOnStop { get; set; }

	bool pFollowListener { get; set; }

	GameObject pEventTarget { get; set; }

	SnEventType pLastEvent { get; }

	SnTriggerList pTriggers { get; set; }

	bool pLoopQueue { get; set; }

	AudioClip[] pClipQueue { get; set; }

	SnTriggerList[] pTriggerQueue { get; set; }

	SnSettings pDefaultSettings { get; set; }

	bool Acquire();

	bool Release();

	void ApplySettings(SnSettings inSettings);

	void Play();

	void Play(SnSettings inSettings);

	void Pause();

	void Stop();

	void Mute(bool mute);

	void Kill();

	void Tick();

	void LoadTriggers(string inURL);
}
