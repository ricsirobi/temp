using System;
using UnityEngine;

namespace SquadTactics;

[Serializable]
public class CinematicLayerData
{
	public GameObject _RendererObject;

	public int _OriginalLayer;

	public CinematicLayerData(GameObject rendererObject, int originalLayer)
	{
		_RendererObject = rendererObject;
		_OriginalLayer = originalLayer;
	}
}
