using System;
using System.Collections.Generic;

namespace CI.WSANative.FileStorage;

public class WSANativeStorageLibrary
{
	public static void GetFile(WSAStorageLibrary library, string relativePath, Action<WSAStorageFile> result)
	{
	}

	public static void GetFiles(WSAStorageLibrary library, string relativePath, Action<IEnumerable<WSAStorageFile>> result)
	{
	}

	public static void GetFolders(WSAStorageLibrary library, string relativePath, Action<IEnumerable<string>> result)
	{
	}

	public static bool DoesFileExist(WSAStorageLibrary library, string relativePath)
	{
		return false;
	}

	public static bool DoesFolderExist(WSAStorageLibrary library, string relativePath)
	{
		return false;
	}

	public static WSAStorageFile CreateFile(WSAStorageLibrary library, string relativePath)
	{
		return new WSAStorageFile();
	}

	public static void CreateFolder(WSAStorageLibrary library, string relativePath)
	{
	}

	public static void DeleteFile(WSAStorageLibrary library, string relativePath)
	{
	}

	public static void DeleteFolder(WSAStorageLibrary library, string relativePath)
	{
	}
}
