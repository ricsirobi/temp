using UnityEngine;

public class GrBitmapUtils
{
	public static GrBitmap CloneTexToNewBitmap(Texture2D cloneMe)
	{
		GrBitmap grBitmap = new GrBitmap(cloneMe);
		grBitmap.SetBlendMode(BlendOp.REPLACE, BlendOp.REPLACE, RepeatMode.REPEAT);
		GrBitmap grBitmap2 = new GrBitmap(new Texture2D(grBitmap._Width, grBitmap._Height, grBitmap._Texture.format, mipChain: false));
		grBitmap.BlitTo(grBitmap2, 0, 0);
		return grBitmap2;
	}

	public static void BlitUsingMask(GrBitmap src, GrBitmap dst, GrBitmap mask)
	{
		Rect rect = new Rect(0f, 0f, src._Width, src._Height);
		Rect srcRect = new Rect(0f, 0f, mask._Width, mask._Height);
		Rect dstRect = new Rect(0f, 0f, dst._Width, dst._Height);
		mask.SetBlendMode(BlendOp.NONE, BlendOp.MULTIPLY, RepeatMode.STAMP);
		mask.BlitTo(src, srcRect, rect);
		src.SetBlendMode(BlendOp.USEALPHA, BlendOp.NONE, RepeatMode.STAMP);
		src.BlitTo(dst, rect, dstRect);
		dst.Apply();
	}
}
