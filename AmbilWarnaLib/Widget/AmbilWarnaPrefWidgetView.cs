// Created by yukuku
// Ported to Xamarin.Android by Rofiq Setiawan (rofiqsetiawan@gmail.com)

using System;
using Android.Content;
using Android.Graphics;
using Android.Util;
using Android.Views;

namespace Yuku.AmbilWarna.Widget
{
	public class AmbilWarnaPrefWidgetView : View
	{
	    private readonly Paint _paint;
	    private readonly float _rectSize;
	    private readonly float _strokeWidth;

		public AmbilWarnaPrefWidgetView(Context context, IAttributeSet attrs) : base(context, attrs)
		{
			float density = context.Resources.DisplayMetrics.Density;
			_rectSize = (float)Math.Floor(24.0f * density + 0.5f);
			_strokeWidth = (float)Math.Floor(1.0f * density + 0.5f);

		    _paint = new Paint
		    {
		        Color = unchecked((int) 0xffffffff).ToColor(),
		        StrokeWidth = _strokeWidth
		    };
		    _paint.SetStyle(Paint.Style.Stroke);
		}

		protected override void OnDraw(Canvas canvas)
		{
			base.OnDraw(canvas);
			canvas.DrawRect(
                    _strokeWidth,
                    _strokeWidth,
                    _rectSize - _strokeWidth,
                    _rectSize - _strokeWidth,
                    _paint
                );
		}
	}
}
