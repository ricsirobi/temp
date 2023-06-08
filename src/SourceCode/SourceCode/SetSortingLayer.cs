public class SetSortingLayer : KAMonoBase
{
	public string layerName = "GameObject";

	public int sortingOrder = 5;

	private void Start()
	{
		base.renderer.sortingLayerName = layerName;
		base.renderer.sortingOrder = sortingOrder;
	}
}
