using System;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace JSGames.Video;

public class VideoSlider : Slider
{
	private bool mIsDragging;

	public float pSliderPosition
	{
		get
		{
			return value;
		}
		set
		{
			if (!mIsDragging)
			{
				this.value = value;
			}
		}
	}

	public Action<float> pOnSliderDrag { get; set; }

	public override void OnDrag(PointerEventData eventData)
	{
		base.OnDrag(eventData);
		if (eventData.dragging)
		{
			mIsDragging = true;
			if (pOnSliderDrag != null)
			{
				pOnSliderDrag(value);
			}
		}
		else
		{
			mIsDragging = false;
		}
	}

	public override void OnPointerUp(PointerEventData eventData)
	{
		base.OnPointerUp(eventData);
		mIsDragging = false;
	}
}
