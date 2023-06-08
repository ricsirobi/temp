using UnityEngine;

public class ObRespawn : MonoBehaviour
{
	public ObRespawnObject[] _Objects;

	protected GameObject[] mCopies;

	protected virtual void Awake()
	{
		if (_Objects == null)
		{
			return;
		}
		mCopies = new GameObject[_Objects.Length];
		for (int i = 0; i < _Objects.Length; i++)
		{
			if (_Objects[i]._Object != null)
			{
				mCopies[i] = Object.Instantiate(_Objects[i]._Object, _Objects[i]._Object.transform.position, _Objects[i]._Object.transform.rotation);
				mCopies[i].SetActive(value: false);
			}
			else
			{
				Debug.LogError("Object can not be instantiated!!! Null reference.");
			}
		}
	}

	private void Update()
	{
		if (_Objects == null)
		{
			return;
		}
		for (int i = 0; i < _Objects.Length; i++)
		{
			if (_Objects[i]._Object == null && _Objects[i].mRespawnTime <= 0f)
			{
				_Objects[i].mRespawnTime = _Objects[i]._RespawnTime;
			}
			if (_Objects[i].mRespawnTime > 0f)
			{
				_Objects[i].mRespawnTime -= Time.deltaTime;
				if (_Objects[i].mRespawnTime <= 0f)
				{
					Create(i);
				}
			}
		}
	}

	protected virtual void Create(int i)
	{
		_Objects[i]._Object = Object.Instantiate(mCopies[i], mCopies[i].transform.position, mCopies[i].transform.rotation);
		_Objects[i]._Object.SetActive(value: true);
	}
}
