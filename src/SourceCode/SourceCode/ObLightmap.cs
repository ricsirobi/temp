using System;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class ObLightmap : MonoBehaviour
{
	[Serializable]
	public class RendererInfo
	{
		public Renderer _Renderer;

		public int _LightmapIndex;

		public Vector4 _LightmapScaleOffset;
	}

	public List<RendererInfo> _RendererInfo;

	public List<Texture2D> _Lightmaps;

	private void Start()
	{
		if (Application.isPlaying && _RendererInfo != null && _RendererInfo.Count != 0)
		{
			List<LightmapData> list = new List<LightmapData>(LightmapSettings.lightmaps);
			for (int i = 0; i < _Lightmaps.Count; i++)
			{
				LightmapData lightmapData = new LightmapData();
				lightmapData.lightmapColor = _Lightmaps[i];
				list.Add(lightmapData);
			}
			for (int j = 0; j < _RendererInfo.Count; j++)
			{
				_RendererInfo[j]._Renderer.lightmapIndex = LightmapSettings.lightmaps.Length + _RendererInfo[j]._LightmapIndex;
				_RendererInfo[j]._Renderer.lightmapScaleOffset = _RendererInfo[j]._LightmapScaleOffset;
			}
			LightmapSettings.lightmaps = list.ToArray();
		}
	}
}
