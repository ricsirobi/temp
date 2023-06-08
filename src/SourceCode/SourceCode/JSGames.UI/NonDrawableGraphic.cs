using UnityEngine;
using UnityEngine.UI;

namespace JSGames.UI;

[RequireComponent(typeof(CanvasRenderer))]
public class NonDrawableGraphic : Graphic
{
	public override void SetMaterialDirty()
	{
	}

	public override void SetVerticesDirty()
	{
	}

	protected override void OnPopulateMesh(VertexHelper vh)
	{
		vh.Clear();
	}
}
