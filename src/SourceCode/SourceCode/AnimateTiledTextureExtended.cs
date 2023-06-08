using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimateTiledTextureExtended : KAMonoBase
{
	public delegate void VoidEvent();

	public int _columns = 2;

	public int _rows = 2;

	public Vector2 _scale = new Vector3(1f, 1f);

	public Vector2 _offset = Vector2.zero;

	public Vector2 _buffer = Vector2.zero;

	public float _framesPerSecond = 10f;

	public bool _playOnce;

	public bool _disableUponCompletion;

	public bool _enableEvents;

	public bool _playOnEnable = true;

	public bool _newMaterialInstance;

	private int _index;

	private Vector2 _textureSize = Vector2.zero;

	private Material _materialInstance;

	private bool _hasMaterialInstance;

	private bool _isPlaying;

	private List<VoidEvent> _voidEventCallbackList;

	public void RegisterCallback(VoidEvent cbFunction)
	{
		if (_enableEvents)
		{
			_voidEventCallbackList.Add(cbFunction);
		}
		else
		{
			Debug.LogWarning("AnimateTiledTexture: You are attempting to register a callback but the events of this object are not enabled!");
		}
	}

	public void UnRegisterCallback(VoidEvent cbFunction)
	{
		if (_enableEvents)
		{
			_voidEventCallbackList.Remove(cbFunction);
		}
		else
		{
			Debug.LogWarning("AnimateTiledTexture: You are attempting to un-register a callback but the events of this object are not enabled!");
		}
	}

	public void Play()
	{
		if (_isPlaying)
		{
			StopCoroutine("updateTiling");
			_isPlaying = false;
		}
		base.renderer.enabled = true;
		_index = _columns;
		StartCoroutine(updateTiling());
	}

	public void ChangeMaterial(Material newMaterial, bool newInstance = false)
	{
		if (newInstance)
		{
			if (_hasMaterialInstance)
			{
				Object.Destroy(base.renderer.sharedMaterial);
			}
			_materialInstance = new Material(newMaterial);
			base.renderer.sharedMaterial = _materialInstance;
			_hasMaterialInstance = true;
		}
		else
		{
			base.renderer.sharedMaterial = newMaterial;
		}
		CalcTextureSize();
		base.renderer.sharedMaterial.SetTextureScale("_MainTex", _textureSize);
	}

	private void Awake()
	{
		if (_enableEvents)
		{
			_voidEventCallbackList = new List<VoidEvent>();
		}
		ChangeMaterial(base.renderer.sharedMaterial, _newMaterialInstance);
	}

	private void OnDestroy()
	{
		if (_hasMaterialInstance)
		{
			Object.Destroy(base.renderer.sharedMaterial);
			_hasMaterialInstance = false;
		}
	}

	private void HandleCallbacks(List<VoidEvent> cbList)
	{
		for (int i = 0; i < cbList.Count; i++)
		{
			cbList[i]();
		}
	}

	private void OnEnable()
	{
		CalcTextureSize();
		if (_playOnEnable)
		{
			Play();
		}
	}

	private void CalcTextureSize()
	{
		_textureSize = new Vector2(1f / (float)_columns, 1f / (float)_rows);
		_textureSize.x /= _scale.x;
		_textureSize.y /= _scale.y;
		_textureSize -= _buffer;
	}

	private IEnumerator updateTiling()
	{
		_isPlaying = true;
		int checkAgainst = _rows * _columns;
		while (true)
		{
			if (_index >= checkAgainst)
			{
				_index = 0;
				if (_playOnce)
				{
					if (checkAgainst == _columns)
					{
						break;
					}
					checkAgainst = _columns;
				}
			}
			ApplyOffset();
			_index++;
			yield return new WaitForSeconds(1f / _framesPerSecond);
		}
		if (_enableEvents)
		{
			HandleCallbacks(_voidEventCallbackList);
		}
		if (_disableUponCompletion)
		{
			base.renderer.enabled = false;
		}
		_isPlaying = false;
	}

	private void ApplyOffset()
	{
		Vector2 value = new Vector2((float)_index / (float)_columns - (float)(_index / _columns), 1f - (float)(_index / _columns) / (float)_rows);
		if (value.y == 1f)
		{
			value.y = 0f;
		}
		value.x += (1f / (float)_columns - _textureSize.x) / 2f;
		value.y += (1f / (float)_rows - _textureSize.y) / 2f;
		value.x += _offset.x;
		value.y += _offset.y;
		base.renderer.sharedMaterial.SetTextureOffset("_MainTex", value);
	}
}
