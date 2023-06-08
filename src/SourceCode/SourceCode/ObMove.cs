using UnityEngine;

public class ObMove : MonoBehaviour
{
	public Vector3 _Direction;

	public float _Speed;

	public void Update()
	{
		base.transform.Translate(_Direction * (_Speed * Time.deltaTime));
	}
}
