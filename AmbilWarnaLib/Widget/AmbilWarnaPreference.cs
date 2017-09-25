// Created by yukuku
// Ported to Xamarin.Android by Rofiq Setiawan (rofiqsetiawan@gmail.com)

using Android.Content;
using Android.Content.Res;
using Android.OS;
using Android.Preferences;
using Android.Util;
using Android.Views;
using Object = Java.Lang.Object;
using R = Yuku.AmbilWarna.Resource;

namespace Yuku.AmbilWarna.Widget
{
	public class AmbilWarnaPreference : Preference
	{
		private readonly bool _supportsAlpha;
	    internal int Value;

        #region Constructor

	    public AmbilWarnaPreference(System.IntPtr javaReference, Android.Runtime.JniHandleOwnership transfer)
            : base (javaReference, transfer)
	    {
	    }

        public AmbilWarnaPreference(Context context, IAttributeSet attrs)
            : base(context, attrs)
	    {
	        TypedArray ta = context.ObtainStyledAttributes(attrs, R.Styleable.AmbilWarnaPreference);
	        _supportsAlpha = ta.GetBoolean(R.Styleable.AmbilWarnaPreference_supportsAlpha, false);
	    }

        #endregion


        /// <summary>
        /// to avoid virtual member call on constructor
        /// </summary>
        private void Init()
	    {
	        WidgetLayoutResource = R.Layout.ambilwarna_pref_widget;
        }

		protected override void OnBindView(View view)
		{
			base.OnBindView(view);

			// Set our custom views inside the layout
			View box = view.FindViewById(R.Id.ambilwarna_pref_widget_box);

		    box?.SetBackgroundColor(Value.ToColor());
		}

		protected override void OnClick()
		{
			var listener = new AmbilWarnaDialog.OnAmbilWarnaListener();
			listener.Ok += (sender, e) =>
			{
				// They don't want the value to be set
				if (!CallChangeListener(e.Color))
					return;

				Value = e.Color;
				PersistInt(Value);
				NotifyChanged();
			};
			listener.Cancel += (sender, e) =>
			{
				// nothing to do
			};

			new AmbilWarnaDialog(Context, Value, _supportsAlpha, listener).Show();
		}

		public void ForceSetValue(int value)
		{
			Value = value;
			PersistInt(value);
			NotifyChanged();
		}

		// This preference type's value type is Integer,
		// so we read the default value from the attributes as an Integer.
		protected override Object OnGetDefaultValue(TypedArray a, int index) => a.GetInteger(index, 0);

		protected override void OnSetInitialValue(bool restorePersistedValue, Object defaultValue)
		{
			if (restorePersistedValue)
			{
				// Restore state
				Value = GetPersistedInt(Value);
			}
			else
			{
				// Set state
				var value = (int)defaultValue;
				Value = value;
				PersistInt(value);
			}
		}

		// Suppose a client uses this preference type without persisting. We
		// must save the instance state so it is able to, for example, survive
		// orientation changes.
		protected override IParcelable OnSaveInstanceState()
		{
			var superState = base.OnSaveInstanceState();

		    // No need to save instance state since it's persistent
            if (Persistent)
				return superState;

		    return new SavedState(superState)
		    {
		        Value = Value
		    };
		}

		protected override void OnRestoreInstanceState(IParcelable state)
		{
            // if `state` not `SavedState`
			//if (!state.GetType().Equals(typeof(SavedState)))
			if (!(state.GetType() == typeof(SavedState)))
             {
				// Didn't save state for us in OnSaveInstanceState
				base.OnRestoreInstanceState(state);
				return;
			}

			// Restore the instance state
			var myState = (SavedState)state;
			base.OnRestoreInstanceState(myState.SuperState);
			Value = myState.Value;
			NotifyChanged();
		}

		/// <summary>
		/// SavedState, a subclass of <seealso cref="Android.Preferences.Preference.BaseSavedState"/>, will store the state
		/// of MyPreference, a subclass of Preference.
		/// It is important to always call through to super methods.
		/// </summary>
		private class SavedState : BaseSavedState
		{
			public int Value;

			public SavedState(Parcel source) : base(source)
			{
				Value = source.ReadInt();
			}

			public SavedState(IParcelable superState) : base(superState)
			{
			}

			public override void WriteToParcel(Parcel dest, ParcelableWriteFlags flags)
			{
				base.WriteToParcel(dest, flags);
				dest.WriteInt(Value);
			}

			[Java.Interop.ExportField("CREATOR")]
			public static MySavedStateCreator InitializeCreator()
			{
				return new MySavedStateCreator();
			}

			public class MySavedStateCreator : Object, IParcelableCreator
			{
				public Object CreateFromParcel(Parcel source)
				{
					return new SavedState(source);
				}

				public Object[] NewArray(int size)
				{
					return new SavedState[size];
				}
			}

		}


	}
}
