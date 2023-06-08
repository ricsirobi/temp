using UnityEngine;

public class ChatBubble : MonoBehaviour
{
	public TextMesh _TextMesh;

	public float _DisplayTime = 6f;

	public Vector2 _Position;

	public Vector3 _LocalOffset;

	public int _LineLength = 20;

	private float mTimer;

	private string mChat;

	public void Start()
	{
		base.transform.localPosition = _LocalOffset;
	}

	public void WriteChat(string chat)
	{
		WriteChat(chat, "", ChatOptions.CHAT_ROOM, "", null, addToHistory: false);
	}

	public void WriteChat(string chat, string groupID, ChatOptions channel, string from, GameObject fromGO, bool addToHistory)
	{
		if (addToHistory)
		{
			UiChatHistory.WriteLine(chat, from, channel);
		}
		mChat = UtStringUtil.Wordwrap(chat, _LineLength);
		if (_TextMesh != null)
		{
			_TextMesh.text = mChat;
		}
		mTimer = _DisplayTime;
	}

	private void OnDisable()
	{
		Object.Destroy(base.gameObject);
	}

	private void Update()
	{
		mTimer -= Time.deltaTime;
		if (mTimer <= 0f)
		{
			base.gameObject.transform.parent = null;
			Object.Destroy(base.gameObject);
		}
	}
}
