using UnityEngine;

namespace Xsolla;

public abstract class IBaseAdapter : MonoBehaviour
{
	public abstract int GetElementType(int position);

	public abstract int GetCount();

	public abstract GameObject GetView(int position);

	public abstract GameObject GetPrefab();

	public abstract GameObject GetNext();
}
