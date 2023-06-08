using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace JSGames.Tween;

public class Tween
{
	public static void MoveTo(GameObject tweenObject, Vector3 from, Vector3 to, TweenParam tweenParam)
	{
		Move tweener = new Move(tweenObject, from, to);
		float deltaMove = GetDeltaMove(from, to, tweenParam._DurationOrSpeed, isTimeDepndent: true);
		SetTweenParam(tweener, deltaMove, tweenParam);
	}

	public static void MoveBy(GameObject tweenObject, Vector3 from, Vector3 to, TweenParam tweenParam)
	{
		Move tweener = new Move(tweenObject, from, to);
		float deltaMove = GetDeltaMove(from, to, tweenParam._DurationOrSpeed, isTimeDepndent: false);
		SetTweenParam(tweener, deltaMove, tweenParam);
	}

	public static void MoveLocalTo(GameObject tweenObject, Vector3 from, Vector3 to, TweenParam tweenParam)
	{
		MoveLocal tweener = new MoveLocal(tweenObject, from, to);
		float deltaMove = GetDeltaMove(from, to, tweenParam._DurationOrSpeed, isTimeDepndent: true);
		SetTweenParam(tweener, deltaMove, tweenParam);
	}

	public static void MoveLocalBy(GameObject tweenObject, Vector3 from, Vector3 to, TweenParam tweenParam)
	{
		MoveLocal tweener = new MoveLocal(tweenObject, from, to);
		float deltaMove = GetDeltaMove(from, to, tweenParam._DurationOrSpeed, isTimeDepndent: false);
		SetTweenParam(tweener, deltaMove, tweenParam);
	}

	public static void MoveTo(GameObject tweenObject, Vector2 from, Vector2 to, TweenParam tweenParam)
	{
		Move2D tweener = new Move2D(tweenObject, from, to);
		float deltaMove = GetDeltaMove(from, to, tweenParam._DurationOrSpeed, isTimeDepndent: true);
		SetTweenParam(tweener, deltaMove, tweenParam);
	}

	public static void MoveTo(RectTransform tweenObject, Vector3 from, Vector3 to, TweenParam tweenParam)
	{
		Move tweener = new Move(tweenObject, from, to);
		float deltaMove = GetDeltaMove(from, to, tweenParam._DurationOrSpeed, isTimeDepndent: true);
		SetTweenParam(tweener, deltaMove, tweenParam);
	}

	public static void MoveTo(RectTransform tweenObject, Vector2 from, Vector2 to, TweenParam tweenParam)
	{
		Move2D tweener = new Move2D(tweenObject, from, to);
		float deltaMove = GetDeltaMove(from, to, tweenParam._DurationOrSpeed, isTimeDepndent: true);
		SetTweenParam(tweener, deltaMove, tweenParam);
	}

	public static void MoveBy(GameObject tweenObject, Vector2 from, Vector2 to, TweenParam tweenParam)
	{
		Move2D tweener = new Move2D(tweenObject, from, to);
		float deltaMove = GetDeltaMove(from, to, tweenParam._DurationOrSpeed, isTimeDepndent: false);
		SetTweenParam(tweener, deltaMove, tweenParam);
	}

	public static void MoveBy(RectTransform tweenObject, Vector3 from, Vector3 to, TweenParam tweenParam)
	{
		Move tweener = new Move(tweenObject, from, to);
		float deltaMove = GetDeltaMove(from, to, tweenParam._DurationOrSpeed, isTimeDepndent: false);
		SetTweenParam(tweener, deltaMove, tweenParam);
	}

	public static void MoveBy(RectTransform tweenObject, Vector2 from, Vector2 to, TweenParam tweenParam)
	{
		Move2D tweener = new Move2D(tweenObject, from, to);
		float deltaMove = GetDeltaMove(from, to, tweenParam._DurationOrSpeed, isTimeDepndent: false);
		SetTweenParam(tweener, deltaMove, tweenParam);
	}

	public static void MoveLocalTo(GameObject tweenObject, Vector2 from, Vector2 to, TweenParam tweenParam)
	{
		MoveLocal2D tweener = new MoveLocal2D(tweenObject, from, to);
		float deltaMove = GetDeltaMove(from, to, tweenParam._DurationOrSpeed, isTimeDepndent: true);
		SetTweenParam(tweener, deltaMove, tweenParam);
	}

	public static void MoveLocalBy(GameObject tweenObject, Vector2 from, Vector2 to, TweenParam tweenParam)
	{
		MoveLocal2D tweener = new MoveLocal2D(tweenObject, from, to);
		float deltaMove = GetDeltaMove(from, to, tweenParam._DurationOrSpeed, isTimeDepndent: false);
		SetTweenParam(tweener, deltaMove, tweenParam);
	}

	public static void ScaleTo(GameObject tweenObject, Vector3 from, Vector3 to, TweenParam tweenParam)
	{
		Scale tweener = new Scale(tweenObject, from, to);
		float deltaMove = GetDeltaMove(from, to, tweenParam._DurationOrSpeed, isTimeDepndent: true);
		SetTweenParam(tweener, deltaMove, tweenParam);
	}

	public static void ScaleBy(GameObject tweenObject, Vector3 from, Vector3 to, TweenParam tweenParam)
	{
		Scale tweener = new Scale(tweenObject, from, to);
		float deltaMove = GetDeltaMove(from, to, tweenParam._DurationOrSpeed, isTimeDepndent: false);
		SetTweenParam(tweener, deltaMove, tweenParam);
	}

	public static void ScaleTo(GameObject tweenObject, Vector2 from, Vector2 to, TweenParam tweenParam)
	{
		Scale2D tweener = new Scale2D(tweenObject, from, to);
		float deltaMove = GetDeltaMove(from, to, tweenParam._DurationOrSpeed, isTimeDepndent: true);
		SetTweenParam(tweener, deltaMove, tweenParam);
	}

	public static void ScaleBy(GameObject tweenObject, Vector2 from, Vector2 to, TweenParam tweenParam)
	{
		Scale2D tweener = new Scale2D(tweenObject, from, to);
		float deltaMove = GetDeltaMove(from, to, tweenParam._DurationOrSpeed, isTimeDepndent: false);
		SetTweenParam(tweener, deltaMove, tweenParam);
	}

	public static void RotateTo(GameObject tweenObject, Vector3 from, Vector3 to, TweenParam tweenParam)
	{
		Rotate tweener = new Rotate(tweenObject, from, to);
		float deltaMove = GetDeltaMove(from, to, tweenParam._DurationOrSpeed, isTimeDepndent: true);
		SetTweenParam(tweener, deltaMove, tweenParam);
	}

	public static void RotateBy(GameObject tweenObject, Vector3 from, Vector3 to, TweenParam tweenParam)
	{
		Rotate tweener = new Rotate(tweenObject, from, to);
		float deltaMove = GetDeltaMove(from, to, tweenParam._DurationOrSpeed, isTimeDepndent: false);
		SetTweenParam(tweener, deltaMove, tweenParam);
	}

	public static void RotateLocalTo(GameObject tweenObject, Vector3 from, Vector3 to, TweenParam tweenParam)
	{
		RotateLocal tweener = new RotateLocal(tweenObject, from, to);
		float deltaMove = GetDeltaMove(from, to, tweenParam._DurationOrSpeed, isTimeDepndent: true);
		SetTweenParam(tweener, deltaMove, tweenParam);
	}

	public static void RotateLocalBy(GameObject tweenObject, Vector3 from, Vector3 to, TweenParam tweenParam)
	{
		RotateLocal tweener = new RotateLocal(tweenObject, from, to);
		float deltaMove = GetDeltaMove(from, to, tweenParam._DurationOrSpeed, isTimeDepndent: false);
		SetTweenParam(tweener, deltaMove, tweenParam);
	}

	public static void ColorTo(GameObject tweenObject, Color from, Color to, TweenParam tweenParam)
	{
		Color(tweenObject, from, to, tweenParam, isTimeDependent: true);
	}

	public static void ColorBy(GameObject tweenObject, Color from, Color to, TweenParam tweenParam)
	{
		Color(tweenObject, from, to, tweenParam, isTimeDependent: false);
	}

	private static void Color(GameObject tweenObject, Color from, Color to, TweenParam tweenParam, bool isTimeDependent)
	{
		Tweener tweener = null;
		if (tweenObject.GetComponent<Renderer>() != null)
		{
			tweener = new Colour(tweenObject, tweenObject.GetComponent<Renderer>(), from, to);
		}
		else if (tweenObject.GetComponent<TextMesh>() != null)
		{
			tweener = new TextMeshColor(tweenObject, tweenObject.GetComponent<TextMesh>(), from, to);
		}
		else
		{
			if (!(tweenObject.GetComponent<Graphic>() != null))
			{
				Debug.Log("================  Color Component Not Available ================= " + tweenObject.name);
				return;
			}
			tweener = new GraphicColor(tweenObject, tweenObject.GetComponent<Graphic>(), from, to);
		}
		float deltaMove = GetDeltaMove(from, to, tweenParam._DurationOrSpeed, isTimeDependent);
		SetTweenParam(tweener, deltaMove, tweenParam);
	}

	public static void AlphaTo(GameObject tweenObject, float from, float to, TweenParam tweenParam)
	{
		Alpha(tweenObject, from, to, tweenParam, isTimeDependent: true);
	}

	public static void AlphaBy(GameObject tweenObject, float from, float to, TweenParam tweenParam)
	{
		Alpha(tweenObject, from, to, tweenParam, isTimeDependent: false);
	}

	private static void Alpha(GameObject tweenObject, float from, float to, TweenParam tweenParam, bool isTimeDependent)
	{
		Renderer component = tweenObject.GetComponent<Renderer>();
		if (component == null)
		{
			Debug.Log("================  Renderer Component Not Available ================= " + tweenObject.name);
			return;
		}
		Alpha tweener = new Alpha(tweenObject, component, from, to);
		float deltaMove = GetDeltaMove(from, to, tweenParam._DurationOrSpeed, isTimeDependent);
		SetTweenParam(tweener, deltaMove, tweenParam);
	}

	public static void TextCounterTo(GameObject tweenObject, float from, float to, TweenParam tweenParam)
	{
		TextCounter(tweenObject, from, to, tweenParam, isTimeDependent: true);
	}

	public static void TextCounterBy(GameObject tweenObject, float from, float to, TweenParam tweenParam)
	{
		TextCounter(tweenObject, from, to, tweenParam, isTimeDependent: false);
	}

	private static void TextCounter(GameObject tweenObject, float from, float to, TweenParam tweenParam, bool isTimeDependent)
	{
		Tweener tweener = null;
		if (tweenObject.GetComponent<Text>() != null)
		{
			tweener = new Count(tweenObject, tweenObject.GetComponent<Text>(), from, to);
		}
		else
		{
			if (!(tweenObject.GetComponent<TextMesh>() != null))
			{
				Debug.Log("================  Text Component Not Available ================= " + tweenObject.name);
				return;
			}
			tweener = new Count(tweenObject, tweenObject.GetComponent<TextMesh>(), from, to);
		}
		float deltaMove = GetDeltaMove(from, to, tweenParam._DurationOrSpeed, isTimeDependent);
		SetTweenParam(tweener, deltaMove, tweenParam);
	}

	public static void Shake(GameObject tweenObject, float duration, float shakeAmount, OnAnimationCompleteCallback onAnimationCompleteCallback = null)
	{
		Tweener tweener = new Shake(tweenObject, shakeAmount);
		float deltaMove = 1f / duration;
		tweener.SetData(deltaMove, 0f, 1, pingPong: false, useAnimationCurve: false, null, null, onAnimationCompleteCallback);
		TweenUpdater.pTweeners.Add(tweener);
	}

	public static void Shake2D(GameObject tweenObject, float duration, float shakeAmount, OnAnimationCompleteCallback onAnimationCompleteCallback = null)
	{
		Tweener tweener = new Shake2D(tweenObject, shakeAmount);
		float deltaMove = 1f / duration;
		tweener.SetData(deltaMove, 0f, 1, pingPong: false, useAnimationCurve: false, null, null, onAnimationCompleteCallback);
		TweenUpdater.pTweeners.Add(tweener);
	}

	public static void PlayAnim(GameObject tweenObject, string name, OnAnimationCompleteCallback onAnimationCompleteCallback = null)
	{
		Animation component = tweenObject.GetComponent<Animation>();
		if (component == null)
		{
			Debug.Log("Aniamtion component has not been attached to this game object: " + tweenObject.name);
			return;
		}
		Tweener item = new AnimatoinComp(tweenObject, component, name);
		TweenUpdater.pTweeners.Add(item);
	}

	public static void Timer(float delay, OnAnimationCompleteCallback onDelayReachesCallback)
	{
		Tweener tweener = new Timer();
		float deltaMove = 1f / delay;
		tweener.SetData(deltaMove, 0f, 1, pingPong: false, useAnimationCurve: false, null, null, onDelayReachesCallback);
		TweenUpdater.pTweeners.Add(tweener);
	}

	public static CustomCurve GetCustomCurve(EaseType easeType)
	{
		return easeType switch
		{
			EaseType.Linear => EaseCurve.Linear, 
			EaseType.Spring => EaseCurve.Spring, 
			EaseType.EaseInQuad => EaseCurve.EaseInQuad, 
			EaseType.EaseOutQuad => EaseCurve.EaseOutQuad, 
			EaseType.EaseInOutQuad => EaseCurve.EaseInOutQuad, 
			EaseType.EaseInCubic => EaseCurve.EaseInCubic, 
			EaseType.EaseOutCubic => EaseCurve.EaseOutCubic, 
			EaseType.EaseInOutCubic => EaseCurve.EaseInOutCubic, 
			EaseType.EaseInQuart => EaseCurve.EaseInQuart, 
			EaseType.EaseOutQuart => EaseCurve.EaseOutQuart, 
			EaseType.EaseInOutQuart => EaseCurve.EaseInOutQuart, 
			EaseType.EaseInQuint => EaseCurve.EaseInQuint, 
			EaseType.EaseOutQuint => EaseCurve.EaseOutQuint, 
			EaseType.EaseInOutQuint => EaseCurve.EaseInOutQuint, 
			EaseType.EaseInSine => EaseCurve.EaseInSine, 
			EaseType.EaseOutSine => EaseCurve.EaseOutSine, 
			EaseType.EaseInOutSine => EaseCurve.EaseInOutSine, 
			EaseType.EaseInExpo => EaseCurve.EaseInExpo, 
			EaseType.EaseOutExpo => EaseCurve.EaseOutExpo, 
			EaseType.EaseInOutExpo => EaseCurve.EaseInOutExpo, 
			EaseType.EaseInCirc => EaseCurve.EaseInCirc, 
			EaseType.EaseOutCirc => EaseCurve.EaseOutCirc, 
			EaseType.EaseInOutCirc => EaseCurve.EaseInOutCirc, 
			EaseType.EaseInBounce => EaseCurve.EaseInBounce, 
			EaseType.EaseOutBounce => EaseCurve.EaseOutBounce, 
			EaseType.EaseInOutBounce => EaseCurve.EaseInOutBounce, 
			EaseType.EaseInBack => EaseCurve.EaseInBack, 
			EaseType.EaseOutBack => EaseCurve.EaseOutBack, 
			EaseType.EaseInOutBack => EaseCurve.EaseInOutBack, 
			EaseType.EaseInElastic => EaseCurve.EaseInElastic, 
			EaseType.EaseOutElastic => EaseCurve.EaseOutElastic, 
			EaseType.EaseInOutElastic => EaseCurve.EaseInOutElastic, 
			_ => EaseCurve.Linear, 
		};
	}

	private static float GetDeltaMove(Vector2 from, Vector2 to, float factor, bool isTimeDepndent)
	{
		return factor = (isTimeDepndent ? (1f / factor) : (factor / Vector2.Distance(from, to)));
	}

	private static float GetDeltaMove(Vector3 from, Vector3 to, float factor, bool isTimeDepndent)
	{
		return factor = (isTimeDepndent ? (1f / factor) : (factor / Vector3.Distance(from, to)));
	}

	private static float GetDeltaMove(Vector4 from, Vector4 to, float factor, bool isTimeDepndent)
	{
		return factor = (isTimeDepndent ? (1f / factor) : (factor / Vector4.Distance(from, to)));
	}

	private static float GetDeltaMove(float from, float to, float factor, bool isTimeDepndent)
	{
		return factor = (isTimeDepndent ? (1f / factor) : (factor / Mathf.Abs(from - to)));
	}

	private static void SetTweenParam(Tweener tweener, float deltaMove, TweenParam tweenParam)
	{
		tweener.SetData(deltaMove, tweenParam._Delay, tweenParam._LoopCount, tweenParam._PingPong, tweenParam._UseAnimationCurve, tweenParam._AnimationCurve, tweenParam.pCustomAnimationCurve, tweenParam.pOnTweenCompleteCallback);
		TweenUpdater.pTweeners.Add(tweener);
	}

	public static void Pause(GameObject tweenObject, bool state)
	{
		List<Tweener> list = TweenUpdater.pTweeners.FindAll((Tweener elememt) => elememt.pTweenObject.GetInstanceID() == tweenObject.GetInstanceID());
		for (int i = 0; i < list.Count; i++)
		{
			list[i].pState = ((!state) ? TweenState.RUNNING : TweenState.PAUSE);
		}
	}

	public static bool IsRunning(GameObject tweenObject)
	{
		List<Tweener> list = TweenUpdater.pTweeners.FindAll((Tweener elememt) => elememt.pTweenObject.GetInstanceID() == tweenObject.GetInstanceID());
		for (int i = 0; i < list.Count; i++)
		{
			if (list[i].pState == TweenState.RUNNING)
			{
				return true;
			}
		}
		return false;
	}

	public static void Stop(GameObject tweenObject)
	{
		List<Tweener> list = TweenUpdater.pTweeners.FindAll((Tweener elememt) => elememt.pTweenObject.GetInstanceID() == tweenObject.GetInstanceID());
		for (int i = 0; i < list.Count; i++)
		{
			TweenUpdater.pTweeners.Remove(list[i]);
		}
		list.Clear();
	}

	public static void PauseAll(bool state)
	{
		Singleton<TweenUpdater>.pInstance.enabled = !state;
	}
}
