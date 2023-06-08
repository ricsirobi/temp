using KnowledgeAdventure.Multiplayer.Events;
using UnityEngine;

public class MMOClient : MonoBehaviour
{
	public virtual bool IsInvalidState()
	{
		return false;
	}

	public virtual bool IsSinglePlayer()
	{
		return false;
	}

	public virtual bool IsInRacing()
	{
		return false;
	}

	public virtual void AddPlayer(MMOAvatar avatar)
	{
	}

	public virtual void RemovePlayer(MMOAvatar avatar)
	{
	}

	public virtual void Disconnected()
	{
	}

	public virtual void Reset()
	{
	}

	public virtual void Destroy()
	{
	}

	public virtual void RemoveAll()
	{
	}

	public virtual void OnClose()
	{
	}

	public virtual void OnError(MMOErrorEventArgs inError)
	{
	}

	public virtual void OnConnected(MMOConnectedEventArgs inConnectionArgs)
	{
	}

	public virtual void OnLoggedIn(MMOLoggedInEventArgs inLoggedInArgs)
	{
	}

	public virtual void OnLoggedOut(MMOLoggedOutEventArgs inLoggedOutArgs)
	{
	}

	public virtual void OnJoinedRoom(MMOJoinedRoomEventArgs inJoinedRoomArgs)
	{
	}

	public virtual void OnLeftRoom(MMOLeftRoomEventArgs inLeftRoomArgs)
	{
	}

	public virtual void OnRoomListUpdated(MMORoomListUpdatedEventArgs inRoomListUpdatedArgs)
	{
	}
}
