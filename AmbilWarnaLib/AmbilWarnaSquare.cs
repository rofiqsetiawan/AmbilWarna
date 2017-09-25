// Created by yukuku
// Ported to Xamarin.Android by Rofiq Setiawan (rofiqsetiawan@gmail.com)

using Android.Content;
using Android.Graphics;
using Android.Util;
using Android.Views;

namespace Yuku.AmbilWarna
{
	public class AmbilWarnaSquare : View
	{
	    private Paint _paint;
	    private Shader _luar;
	    private readonly float[] _color = { 1.0f, 1.0f, 1.0f };

		#region Constructors

		public AmbilWarnaSquare(Context context, IAttributeSet attrs)
			: base(context, attrs)
		{
		}

		public AmbilWarnaSquare(Context context, IAttributeSet attrs, int defStyle)
			: base(context, attrs, defStyle)
		{
		}

		#endregion


		protected override void OnDraw(Canvas canvas)
		{
			base.OnDraw(canvas);

			if (_paint == null)
			{
				_paint = new Paint();
				_luar = new LinearGradient(
					0.0f,
					0.0f,
					0.0f,
					MeasuredHeight,
					unchecked((int)0xffffffff).ToColor(),
					unchecked((int)0xff000000).ToColor(),
					Shader.TileMode.Clamp
				);
			}
			int rgb = Color.HSVToColor(_color);
			Shader dalam = new LinearGradient(
				                0.0f,
				                0.0f,
				                MeasuredWidth,
				                0.0f,
				                unchecked((int)0xffffffff).ToColor(),
				                rgb.ToColor(),
				                Shader.TileMode.Clamp
			                );

			var shader = new ComposeShader(
				            _luar,
				            dalam,
				            PorterDuff.Mode.Multiply
			            );
			_paint.SetShader(shader);
			canvas.DrawRect(0.0f, 0.0f, MeasuredWidth, MeasuredHeight, _paint);
		}

	    internal void SetHue(float hue)
	    {
	        _color[0] = hue;
	        Invalidate();
	    }
    }
}
