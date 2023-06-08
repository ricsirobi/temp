using System;
using System.Collections.Generic;

namespace CI.WSANative.Pickers;

public class WSAStorageFolder
{
	public DateTimeOffset DateCreated { get; set; }

	public string DisplayName { get; set; }

	public string Name { get; set; }

	public string Path { get; set; }

	public string DisplayType { get; set; }

	public void GetFile(string name, Action<WSAStorageFile> result)
	{
	}

	public void GetFiles(Action<IEnumerable<WSAStorageFile>> result)
	{
	}

	public void GetFolders(Action<IEnumerable<WSAStorageFolder>> result)
	{
	}

	public bool DoesFileExist(string name)
	{
		return false;
	}

	public bool DoesFolderExist(string name)
	{
		return false;
	}

	public WSAStorageFile CreateFile(string name)
	{
		return new WSAStorageFile();
	}

	public WSAStorageFolder CreateFolder(string name)
	{
		return new WSAStorageFolder();
	}

	public void DeleteFile(string name)
	{
	}

	public void DeleteFolder(string name)
	{
	}
}
