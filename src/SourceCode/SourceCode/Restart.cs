using UnityEngine;
using UnityEngine.SceneManagement;

public class Restart : MonoBehaviour
{
	private void OnMouseDown()
	{
		SceneManager.LoadScene(SceneManager.GetActiveScene().name);
	}

	private void Update()
	{
		if (Input.touchCount > 2)
		{
			SceneManager.LoadScene(SceneManager.GetActiveScene().name);
		}
	}
}
