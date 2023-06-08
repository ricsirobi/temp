using System;
using UnityEngine;

[Serializable]
public class PetAgeData
{
	public string _Name;

	public PetAgeBoneData[] _BoneInfo;

	public float _WalkSpeed = 1f;

	public float _RunSpeed = 1f;

	public float _BathScale;

	public float _PlayScale;

	public float _TPScale;

	public float _LabScale = 1f;

	public float _EelBlastScale = 1f;

	public float _UiScale = 1f;

	public float _ClickRadius;

	public float _ClickHeight = 1f;

	public Vector3 _ClickCenter = Vector3.zero;

	public PetSkillRequirements[] _SkillsRequired;

	public float _MinTotalSkillLevelToNextAge;

	public float _MaxTotalSkillLevelToNextAge;

	public float _MinHours;

	public float _MaxHours;

	public AudioClip _GrowVO;

	public bool _AgeSpecificAnim;

	public SantuayPetResourceInfo[] _PetResList;

	public SanctuaryPetToyOffset[] _PetToyOffset;

	public LocaleString _GrowthText;

	public PetAgeBoneData _PetPictureScaleData;

	public Vector3 _MountAvatarOffsetPos = Vector3.zero;

	public Vector3 _HUDPictureCameraOffset = new Vector3(2f, 5f, 5f);

	public PetMoodParticleData[] _MoodParticlesOnPet;

	public bool _NewlyAdded;
}
