// Created by yukuku
// Ported to Xamarin.Android by Rofiq Setiawan (rofiqsetiawan@gmail.com)

using System;
using Android.App;
using Android.Content;
using Android.Graphics.Drawables;
using Android.OS;
using Android.Views;
using Android.Widget;
using R = Yuku.AmbilWarna.Resource;

namespace Yuku.AmbilWarna
{
	public class AmbilWarnaDialog : Java.Lang.Object, AmbilWarnaDialog.IOnAmbilWarnaListener
	{
		public interface IOnAmbilWarnaListener
		{
			void OnCancel(AmbilWarnaDialog dialog);

			void OnOk(AmbilWarnaDialog dialog, int color);
		}

		private IOnAmbilWarnaListener _listener;
		private readonly View _viewHue;
		private readonly AmbilWarnaSquare _viewSatVal;
		private readonly ImageView _viewCursor;
		private readonly ImageView _viewAlphaCursor;
		private readonly View _viewAlphaOverlay;
		private readonly ImageView _viewTarget;
		private readonly ImageView _viewAlphaCheckered;
		private readonly ViewGroup _viewContainer;
		private readonly float[] _currentColorHsv = new float[3];
		private int _alpha;

		/// <summary>
		/// Epsilon for precision comparison
		/// </summary>
		private const float Tolerance = 0.01f;

		#region Constructors

		// Tambahan
		public AmbilWarnaDialog(Context context, int color, bool supportsAlpha)
			: this(context, color, supportsAlpha, null)
		{
		}

	    public AmbilWarnaDialog(Context context, int color)
	        : this(context, color, false, null)
	    {
	    }

        /// <summary>
        /// Create an AmbilWarnaDialog.
        /// </summary>
        /// <param name="context"> activity context </param>
        /// <param name="color"> current color </param>
        /// <param name="listener"> an OnAmbilWarnaListener, allowing you to get back error or OK </param>
        public AmbilWarnaDialog(Context context, int color, IOnAmbilWarnaListener listener)
			: this(context, color, false, listener)
		{
		}

		/// <summary>
		/// Create an AmbilWarnaDialog.
		/// </summary>
		/// <param name="context"> activity context </param>
		/// <param name="color"> current color </param>
		/// <param name="supportsAlpha"> whether alpha/transparency controls are enabled </param>
		/// <param name="listener"> an OnAmbilWarnaListener, allowing you to get back error or OK </param>
		public AmbilWarnaDialog(Context context, int color, bool supportsAlpha, IOnAmbilWarnaListener listener)
		{
			var supportsAlpha1 = supportsAlpha;
			_listener = listener;

			if (!supportsAlpha)
			{
				// remove alpha if not supported
				color = color | unchecked((int)0xff000000);
			}

			Android.Graphics.Color.ColorToHSV(color.ToColor(), _currentColorHsv);
			_alpha = Android.Graphics.Color.GetAlphaComponent(color);

			View view = LayoutInflater.From(context).Inflate(R.Layout.ambilwarna_dialog, null);
			_viewHue = view.FindViewById(R.Id.ambilwarna_viewHue);
			_viewSatVal = (AmbilWarnaSquare)view.FindViewById(R.Id.ambilwarna_viewSatBri);
			_viewCursor = (ImageView)view.FindViewById(R.Id.ambilwarna_cursor);
			var viewOldColor = view.FindViewById(R.Id.ambilwarna_oldColor);
			var viewNewColor = view.FindViewById(R.Id.ambilwarna_newColor);
			_viewTarget = (ImageView)view.FindViewById(R.Id.ambilwarna_target);
			_viewContainer = (ViewGroup)view.FindViewById(R.Id.ambilwarna_viewContainer);
			_viewAlphaOverlay = view.FindViewById(R.Id.ambilwarna_overlay);
			_viewAlphaCursor = (ImageView)view.FindViewById(R.Id.ambilwarna_alphaCursor);
			_viewAlphaCheckered = (ImageView)view.FindViewById(R.Id.ambilwarna_alphaCheckered);

			// Hide/show alpha
			_viewAlphaOverlay.Visibility = supportsAlpha ? ViewStates.Visible : ViewStates.Gone;
			_viewAlphaCursor.Visibility = supportsAlpha ? ViewStates.Visible : ViewStates.Gone;
			_viewAlphaCheckered.Visibility = supportsAlpha ? ViewStates.Visible : ViewStates.Gone;

			_viewSatVal.SetHue(Hue);
			viewOldColor.SetBackgroundColor(color.ToColor());
			viewNewColor.SetBackgroundColor(color.ToColor());


			_viewHue.Touch += (sender, e) =>
			{
				if (e.Event.Action == MotionEventActions.Move
					|| e.Event.Action == MotionEventActions.Down
					|| e.Event.Action == MotionEventActions.Up)
				{
					float y = e.Event.GetY();
					if (y < 0.0f) y = 0.0f;
					if (y > _viewHue.MeasuredHeight)
					{
						// To avoid jumping the cursor from bottom to top.
						y = _viewHue.MeasuredHeight - 0.001f;
					}
					float hue = 360.0f - 360.0f / _viewHue.MeasuredHeight * y;

					// HACK
					// if (hue == 360.0f)
					if (Math.Abs(hue - 360.0f) < Tolerance)
						hue = 0.0f;

					Hue = hue;

					// Update view
					_viewSatVal.SetHue(Hue);
					MoveCursor();
					viewNewColor.SetBackgroundColor(Color.ToColor());
					UpdateAlphaView();

					//return true;
					e.Handled = true;
				}

				//return false;
				e.Handled = false;
			};


			if (supportsAlpha)
			{
				_viewAlphaCheckered.Touch += (sender, e) =>
				{
					if (e.Event.Action == MotionEventActions.Move
						|| e.Event.Action == MotionEventActions.Down
						|| e.Event.Action == MotionEventActions.Up)
					{
						float y = e.Event.GetY();

						if (y < 0.0f)
						{
							y = 0.0f;
						}

						if (y > _viewAlphaCheckered.MeasuredHeight)
						{
							y = _viewAlphaCheckered.MeasuredHeight - 0.001f; // to avoid jumping the cursor from bottom to top.
						}

						var a = (int)Math.Round(255.0f - 255.0f / _viewAlphaCheckered.MeasuredHeight * y);
						Alpha = a;

						// Update view
						MoveAlphaCursor();
						int col = Color;
						int c = a << 24 | col & 0x00ffffff;
						viewNewColor.SetBackgroundColor(c.ToColor());

						//return true;
						e.Handled = true;
					}

					//return false;
					e.Handled = false;
				};
			}


			_viewSatVal.Touch += (sender, e) =>
			{
				if (e.Event.Action == MotionEventActions.Move
                    || e.Event.Action == MotionEventActions.Down
                    || e.Event.Action == MotionEventActions.Up)
				{
					float x = e.Event.GetX(); // Touch event are in dp units.
					float y = e.Event.GetY();

					if (x < 0.0f)
						x = 0.0f;

					if (x > _viewSatVal.MeasuredWidth)
						x = _viewSatVal.MeasuredWidth;

					if (y < 0.0f)
						y = 0.0f;

					if (y > _viewSatVal.MeasuredHeight)
						y = _viewSatVal.MeasuredHeight;

					Sat = 1.0f / _viewSatVal.MeasuredWidth * x;
					Val = 1.0f - 1.0f / _viewSatVal.MeasuredHeight * y;

					// Update view
					MoveTarget();
					viewNewColor.SetBackgroundColor(Color.ToColor());

					//return true;
					e.Handled = true;
				}

				//return false;
				e.Handled = false;
			};


			Dialog = new AlertDialog.Builder(context)
				.SetPositiveButton(
					Android.Resource.String.Ok,
					(sender, e) =>
					{
                        // Set Listener
						listener?.OnOk(this, Color);

                        // EventHandler
                        OnOk(this, Color);
					}
				).SetNegativeButton(
					Android.Resource.String.Cancel,
					(sender, e) =>
					{
					    // Set Listener
                        listener?.OnCancel(this);

					    // EventHandler
                        OnCancel(this);
                    }
                ).SetOnCancelListener(
					// If back button is used, call back our listener.
					new MyDialogInterfaceOnCancelListener(
					(dlg) =>
					{
					    // Set Listener
                        listener?.OnCancel(this);

					    // EventHandler
                        OnCancel(this);
                    })
				).Create();


			// Kill all padding from the dialog window
			Dialog.SetView(view, 0, 0, 0, 0);

			// Move cursor & target on first draw
			ViewTreeObserver vto = view.ViewTreeObserver;
			var vtoListener = new MyVtoOnGlobalLayoutListener();
			vtoListener.GlobalLayoutEvent += delegate
			{
				MoveCursor();

				if (supportsAlpha1)
					MoveAlphaCursor();
				MoveTarget();

				if (supportsAlpha1)
					UpdateAlphaView();

				//view.ViewTreeObserver.RemoveGlobalOnLayoutListener(vtoListener); // obsolete
				RemoveOnGlobalLayoutCompatListener(view, vtoListener);
			};

			vto.AddOnGlobalLayoutListener(vtoListener);
		}

		#endregion



		protected void MoveCursor()
		{
			float y = _viewHue.MeasuredHeight - Hue * _viewHue.MeasuredHeight / 360.0f;

			// HACK
			// if (y == viewHue.MeasuredHeight)
			if (Math.Abs(y - _viewHue.MeasuredHeight) < Tolerance)
				y = 0.0f;

			var layoutParams = (RelativeLayout.LayoutParams)_viewCursor.LayoutParameters;
			layoutParams.LeftMargin = (int)(_viewHue.Left - Math.Floor(_viewCursor.MeasuredWidth / 2.0) - _viewContainer.PaddingLeft);
			layoutParams.TopMargin = (int)(_viewHue.Top + y - Math.Floor(_viewCursor.MeasuredHeight / 2.0) - _viewContainer.PaddingTop);
			_viewCursor.LayoutParameters = layoutParams;
		}

		protected void MoveTarget()
		{
			float x = Sat * _viewSatVal.MeasuredWidth;
			float y = (1.0f - Val) * _viewSatVal.MeasuredHeight;
			var layoutParams = (RelativeLayout.LayoutParams)_viewTarget.LayoutParameters;
			layoutParams.LeftMargin = (int)(_viewSatVal.Left + x - Math.Floor(_viewTarget.MeasuredWidth / 2.0) - _viewContainer.PaddingLeft);
			layoutParams.TopMargin = (int)(_viewSatVal.Top + y - Math.Floor(_viewTarget.MeasuredHeight / 2.0) - _viewContainer.PaddingTop);
			_viewTarget.LayoutParameters = layoutParams;
		}

		protected void MoveAlphaCursor()
		{
			int measuredHeight = _viewAlphaCheckered.MeasuredHeight;
			float y = measuredHeight - Alpha * measuredHeight / 255.0f;
			var layoutParams = (RelativeLayout.LayoutParams)_viewAlphaCursor.LayoutParameters;
			layoutParams.LeftMargin = (int)(_viewAlphaCheckered.Left - Math.Floor(_viewAlphaCursor.MeasuredWidth / 2.0) - _viewContainer.PaddingLeft);
			layoutParams.TopMargin = (int)(_viewAlphaCheckered.Top + y - Math.Floor(_viewAlphaCursor.MeasuredHeight / 2.0) - _viewContainer.PaddingTop);

			_viewAlphaCursor.LayoutParameters = layoutParams;
		}

		private int Color
		{
			get
			{
				int argb = Android.Graphics.Color.HSVToColor(_currentColorHsv);
				return _alpha << 24 | (argb & 0x00ffffff);
			}
		}

		private float Hue
		{
			get => _currentColorHsv[0];
			set => _currentColorHsv[0] = value;
		}


		private float Alpha
		{
			get => _alpha;
			set => _alpha = (int)value;
		}


		/// <summary>
		/// Gets the saturation.
		/// </summary>
		/// <returns>The sat.</returns>
		private float Sat
		{
			get => _currentColorHsv[1];
			set => _currentColorHsv[1] = value;
		}



		private float Val
		{
			get => _currentColorHsv[2];
			set => _currentColorHsv[2] = value;
		}





		public void Show() => Dialog.Show();

		public AlertDialog Dialog { get; }

		private void UpdateAlphaView()
		{
			var gd = new GradientDrawable(
				GradientDrawable.Orientation.TopBottom,
				new int[] { Android.Graphics.Color.HSVToColor(_currentColorHsv), 0x0 }
			);
			//viewAlphaOverlay.SetBackgroundDrawable(gd); // obsolete

			SetBackgroundCompat(_viewAlphaOverlay, gd);
		}

		#region Listener

		public class OnAmbilWarnaListener : AmbilWarnaDialog.IOnAmbilWarnaListener
		{
			public class OnCancelEventArg : EventArgs
			{
				public AmbilWarnaDialog Dialog { get; set; }
			}

			public event EventHandler<OnCancelEventArg> Cancel;

			public void OnCancel(AmbilWarnaDialog dialog)
			{
				Cancel?.Invoke(this, new OnCancelEventArg
				{
					Dialog = dialog
				});
			}

			public class OnOkEventArgs : OnCancelEventArg
			{
				public int Color { get; set; }
			}

			public event EventHandler<OnOkEventArgs> Ok;

			public void OnOk(AmbilWarnaDialog dialog, int color)
			{
				Ok?.Invoke(this, new OnOkEventArgs
				{
					Dialog = dialog,
					Color = color
				});
			}
		}

        #region EventHandler

	    public event EventHandler<OnAmbilWarnaListener.OnCancelEventArg> Cancel;
        public void OnCancel(AmbilWarnaDialog dialog)
	    {
	        Cancel?.Invoke(this, new OnAmbilWarnaListener.OnCancelEventArg
            {
	            Dialog = dialog
	        });
        }

	    public event EventHandler<OnAmbilWarnaListener.OnOkEventArgs> Ok;

        public void OnOk(AmbilWarnaDialog dialog, int color)
	    {
	        Ok?.Invoke(this, new OnAmbilWarnaListener.OnOkEventArgs
            {
	            Dialog = dialog,
	            Color = color
	        });
        }

        #endregion

        #endregion


        // By Rofiq Setiawan rofiqsetiawan@gmail.com
        public static void SetBackgroundCompat(View view, Drawable drawable)
		{
			if (Build.VERSION.SdkInt >= BuildVersionCodes.JellyBean)
				view.Background = drawable;
			else
#pragma warning disable CS0618 // Type or member is obsolete
				view.SetBackgroundDrawable(drawable);
#pragma warning restore CS0618 // Type or member is obsolete
		}

		[Android.Annotation.TargetApi(Value = (int)BuildVersionCodes.JellyBean)]
		public static void RemoveOnGlobalLayoutCompatListener(View view, ViewTreeObserver.IOnGlobalLayoutListener listener)
		{
			if ((int)Build.VERSION.SdkInt < 16)
			{
#pragma warning disable CS0618 // Type or member is obsolete
				view?.ViewTreeObserver.RemoveGlobalOnLayoutListener(listener);
#pragma warning restore CS0618 // Type or member is obsolete
			}
			else
			{
				view?.ViewTreeObserver.RemoveOnGlobalLayoutListener(listener);
			}
		}



	}
}
