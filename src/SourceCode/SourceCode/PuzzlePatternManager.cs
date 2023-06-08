using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PuzzlePatternManager : ObSwitchBase
{
	[Header("Pressure Plates, Colors, and Displays")]
	[Tooltip("GameObjects with ObPressurePlatePattern - what the player steps on")]
	public List<ObPressurePlatePattern> _PatternObjects;

	public List<PuzzlePattern> _PatternColors = new List<PuzzlePattern>();

	[Tooltip("Assign any colors you want to show up in the pattern")]
	public List<Color> _StartingColors;

	[Tooltip("Assign any materials/shapes you want to show up in the pattern")]
	public List<Material> _StartingShapes;

	[Tooltip("References for the objects displaying the color to the player in the scene")]
	public List<Renderer> _PatternDisplays;

	[Tooltip("References for the objects displaying the pattern to the player in the scene")]
	public List<Renderer> _ShapeDisplays;

	[Header("Settings")]
	[Tooltip("Delay in seconds (float) before the displays will show the next color in the pattern")]
	public float _DisplayTime;

	[Tooltip("Transform to teleport the player to if they mess up")]
	public Transform _AvatarResetTransform;

	[Tooltip("If true, entire puzzle with regenerate with a new pattern on fail")]
	public bool _ResetOnFailure;

	[Header("Hooks")]
	[Tooltip("Will trigger when all plates are correct")]
	public SnSound _OnSucceedSound;

	[Tooltip("Will trigger if the player fails and the puzzle resets")]
	public SnSound _OnFailSound;

	[Tooltip("Will trigger if the player hits the correct plate in the pattern")]
	public SnSound _OnCorrectPlateSound;

	[Tooltip("Will sound when displays show a color")]
	public SnSound _OnDisplaySound;

	public Color _DefaultColor;

	public Material _DefaultShape;

	private int mCurrentIndex;

	private List<PuzzlePattern> mPatterns = new List<PuzzlePattern>();

	private IEnumerator mCoroutine;

	private void Awake()
	{
		SetupPuzzlePattern();
		GeneratePuzzlePattern();
		mCoroutine = Display(_DisplayTime);
		StartCoroutine(mCoroutine);
	}

	private void SetupPuzzlePattern()
	{
		for (int i = 0; i < _StartingColors.Count; i++)
		{
			_PatternColors.Add(new PuzzlePattern(_StartingColors[i], _StartingShapes[i]));
		}
	}

	private void GeneratePuzzlePattern()
	{
		System.Random rnd = new System.Random();
		_PatternObjects = _PatternObjects.OrderBy((ObPressurePlatePattern platePattern) => rnd.Next()).ToList();
		_PatternColors = ShuffleColors(_PatternColors);
		int num = 0;
		while (mPatterns.Count < _PatternObjects.Count)
		{
			if (num == _PatternColors.Count)
			{
				_PatternColors = ShuffleColors(_PatternColors);
				num = 0;
			}
			mPatterns.Add(_PatternColors[num]);
			num++;
		}
		mPatterns = ShuffleColors(mPatterns);
		for (int i = 0; i < _PatternObjects.Count; i++)
		{
			Color color = mPatterns[i].GetColor();
			Material shape = mPatterns[i].GetShape();
			_PatternObjects[i].Init(color, shape);
			_PatternObjects[i].name = ColorUtility.ToHtmlStringRGB(color);
		}
	}

	private List<PuzzlePattern> ShuffleColors(List<PuzzlePattern> patterns)
	{
		System.Random rnd = new System.Random();
		return new List<PuzzlePattern>(patterns.OrderBy((PuzzlePattern pattern) => rnd.Next()));
	}

	private IEnumerator Display(float displayTime)
	{
		foreach (PuzzlePattern mPattern in mPatterns)
		{
			foreach (Renderer patternDisplay in _PatternDisplays)
			{
				if ((bool)patternDisplay)
				{
					patternDisplay.material.color = mPattern.GetColor();
				}
			}
			foreach (Renderer shapeDisplay in _ShapeDisplays)
			{
				if ((bool)shapeDisplay)
				{
					shapeDisplay.material = mPattern.GetShape();
				}
			}
			_OnDisplaySound?.Play();
			yield return new WaitForSeconds(displayTime);
		}
	}

	private void DisplayDefault()
	{
		foreach (Renderer patternDisplay in _PatternDisplays)
		{
			if ((bool)patternDisplay)
			{
				patternDisplay.material.color = _DefaultColor;
			}
		}
		foreach (Renderer shapeDisplay in _ShapeDisplays)
		{
			if ((bool)shapeDisplay)
			{
				shapeDisplay.material = _DefaultShape;
			}
		}
		_OnDisplaySound?.Play();
	}

	private void OnSwitchOn(string objectName)
	{
		if (objectName == ColorUtility.ToHtmlStringRGB(mPatterns[mCurrentIndex].GetColor()))
		{
			mCurrentIndex++;
			_OnCorrectPlateSound?.Play();
		}
		else
		{
			AvAvatar.TeleportTo(_AvatarResetTransform.position, _AvatarResetTransform.forward);
			_OnFailSound?.Play();
			foreach (ObPressurePlatePattern patternObject in _PatternObjects)
			{
				patternObject.Reset();
			}
			mCurrentIndex = 0;
			if (_ResetOnFailure)
			{
				GeneratePuzzlePattern();
			}
			StopCoroutine(mCoroutine);
			mCoroutine = Display(_DisplayTime);
			StartCoroutine(mCoroutine);
		}
		if (mCurrentIndex == _PatternObjects.Count)
		{
			DisplayDefault();
			base.SwitchOn();
			_OnSucceedSound?.Play();
		}
	}
}
