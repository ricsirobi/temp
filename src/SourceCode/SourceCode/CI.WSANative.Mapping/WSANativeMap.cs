using System;

namespace CI.WSANative.Mapping;

public static class WSANativeMap
{
	public static Action<WSAMapSettings> Create;

	public static Action<WSAMapSettings> CenterMapToLocation;

	public static Action<WSAMapItem> AddElementToMap;

	public static Action ClearMapElements;

	public static Action Destroy;

	public static void CreateMap(string mapServiceToken, int width, int height, WSAPosition position, WSAGeoPoint center, int zoomLevel, WSAMapInteractionMode interactionMode)
	{
	}

	public static void CenterMap(WSAGeoPoint position, int zoomLevel = -1)
	{
	}

	public static void AddMapElement(string title, WSAGeoPoint location, string imageUri = null)
	{
	}

	public static void ClearMap()
	{
	}

	public static void DestroyMap()
	{
	}

	public static void LaunchMapsApp(string query = "")
	{
	}
}
