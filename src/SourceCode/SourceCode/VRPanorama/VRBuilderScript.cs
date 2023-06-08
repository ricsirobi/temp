using UnityEngine;

namespace VRPanorama;

public class VRBuilderScript : MonoBehaviour
{
	public GameObject obj;

	public Vector3 spawnPoint;

	public void BuildObject()
	{
		Object.Instantiate(obj, spawnPoint, Quaternion.identity);
	}
}
