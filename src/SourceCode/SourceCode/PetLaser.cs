using UnityEngine;

public class PetLaser : PetToy
{
	public GameObject _LaserPointer;

	public float _CutOffValue;

	public float _HeightDiff = 0.1f;

	public float _MinPlayTimer = 5f;

	private float mPlayTimer;

	public void Update()
	{
		if (mPlayUI == null)
		{
			return;
		}
		Ray ray = mPlayUI._Camera.ScreenPointToRay(Input.mousePosition);
		if (Physics.Raycast(ray, out var hitInfo, 50f, UtUtilities.GetGroundRayCheckLayers()))
		{
			base.transform.LookAt(hitInfo.point);
			base.transform.Rotate(0f, 20f, 0f);
			ray = new Ray(base.transform.position, base.transform.forward);
			if (Physics.Raycast(ray, out hitInfo, 50f, UtUtilities.GetGroundRayCheckLayers()))
			{
				_LaserPointer.transform.position = hitInfo.point - ray.direction * 0.2f;
				_LaserPointer.transform.up = hitInfo.normal;
				Vector3 point = hitInfo.point;
				Vector3 position = SanctuaryManager.pCurPetInstance.transform.position;
				point.x = (point.z = (position.x = (position.z = 0f)));
				float num = Vector3.Distance(point, position);
				if (hitInfo.normal.y < _CutOffValue || (num > _HeightDiff && hitInfo.point.y > SanctuaryManager.pCurPetInstance.transform.position.y))
				{
					mPlayUI.pPet.SetState(Character_State.idle);
					mPlayUI.pPet.PlayAnim(mPlayUI.pPet._IdleAnimName, -1, 1f, 0);
					mPlayUI.pPet.SetLookAt(hitInfo.point, tween: true);
				}
				if (mPlayTimer <= 0f)
				{
					mPlayTimer = _MinPlayTimer;
				}
			}
		}
		if (mPlayTimer > 0f)
		{
			mPlayTimer -= Time.deltaTime;
			if (mPlayTimer < 0f)
			{
				mPlayUI.pPetPlayedAtleastOnce = true;
				mPlayUI.pPet.CheckForTaskCompletion(PetActions.FOLLOWLASER);
				mPlayUI.pPet.UpdateActionMeters(PetActions.FOLLOWLASER, 1f, doUpdateSkill: true);
				mPlayUI.ReAttachToy(PetActions.FOLLOWLASER);
			}
		}
	}
}
