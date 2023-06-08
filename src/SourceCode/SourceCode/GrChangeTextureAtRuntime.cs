public class GrChangeTextureAtRuntime : KAMonoBase
{
	public ChangeTextureAtRuntimeData[] _Data;

	private void Start()
	{
		for (int i = 0; i < _Data.Length; i++)
		{
			base.renderer.materials[_Data[i]._MaterialIndex].mainTexture = _Data[i]._Texture;
		}
	}
}
