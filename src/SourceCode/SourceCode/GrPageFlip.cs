using UnityEngine;

public class GrPageFlip : KAMonoBase
{
	public string _MaterialName;

	public int _MaterialIndex;

	public int[] _Sequence;

	public float _TimeInterval = 0.5f;

	public float _Speed = 1f;

	public float _NumColumns = 2f;

	public float _NumRows = 2f;

	public Texture _Texture;

	private float mCurTime;

	private int mCurrentFrame;

	private Material mMaterial;

	private float mMaxTime;

	public void PlayAnim(float s)
	{
		if (!(mMaterial == null) && _Sequence.Length != 0)
		{
			_Speed = s;
			mCurTime = 0f;
			SetFrame(0);
			mMaxTime = (float)_Sequence.Length * _TimeInterval;
			mMaterial.mainTextureScale = new Vector2(1f / _NumColumns, 1f / _NumRows);
		}
	}

	public void StopAnim()
	{
		_Speed = 0f;
	}

	private void Start()
	{
		if (_MaterialName.Length == 0)
		{
			mMaterial = base.renderer.materials[_MaterialIndex];
		}
		else
		{
			string text = _MaterialName + " (Instance)";
			Component[] componentsInChildren = base.gameObject.GetComponentsInChildren(typeof(Renderer));
			for (int i = 0; i < componentsInChildren.Length; i++)
			{
				Renderer renderer = (Renderer)componentsInChildren[i];
				for (int j = 0; j < renderer.materials.Length; j++)
				{
					if (renderer.materials[j].name == text)
					{
						mMaterial = renderer.materials[j];
						if (_Texture != null)
						{
							mMaterial.mainTexture = _Texture;
						}
						break;
					}
				}
			}
		}
		if (_Speed != 0f)
		{
			PlayAnim(_Speed);
		}
		else
		{
			SetFrame(0);
		}
	}

	public void SetFrame(int f)
	{
		if (_NumColumns != 0f && _NumRows != 0f && _Sequence.Length != 0)
		{
			mCurrentFrame = f;
			float x = (float)(f % (int)_NumColumns) / _NumColumns;
			float y = (1f - (float)(f / (int)_NumColumns)) / _NumRows;
			if (_Texture != null)
			{
				mMaterial.mainTexture = _Texture;
			}
			mMaterial.mainTextureOffset = new Vector2(x, y);
		}
	}

	private void Update()
	{
		if (_Speed == 0f || mMaterial == null || _TimeInterval == 0f || _Sequence.Length == 0 || mMaxTime < 0.001f)
		{
			return;
		}
		mCurTime += Time.deltaTime * _Speed;
		int num = (int)(mCurTime / mMaxTime);
		if (num < 0)
		{
			num = -num;
		}
		if (num > 0)
		{
			mCurTime -= mMaxTime * (float)num;
		}
		else
		{
			mCurTime += mMaxTime * (float)(num + 1);
		}
		int num2 = (int)(mCurTime / _TimeInterval);
		if (num2 >= 0 && num2 < _Sequence.Length)
		{
			int num3 = _Sequence[num2];
			if (num3 != mCurrentFrame)
			{
				SetFrame(num3);
			}
		}
	}
}
