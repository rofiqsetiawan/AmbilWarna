using Android.App;
using Android.Content;
using Android.OS;
using Android.Util;
using Android.Views;
using Android.Widget;
using Yuku.AmbilWarna;
using R = AmbilWarnaDemo.Resource;

namespace AmbilWarnaDemo
{
	[Activity(Label = "AmbilWarnaDemo", MainLauncher = true, Icon = "@drawable/ic_launcher")]
	public class AmbilWarnaDemoActivity : Activity
	{
		private TextView _text1;
		private int _color = unchecked((int)0xffffff00);

		protected override void OnCreate(Bundle savedInstanceState)
		{
			base.OnCreate(savedInstanceState);
			SetContentView(R.Layout.main);

			View button1 = FindViewById(R.Id.button1);
			View button2 = FindViewById(R.Id.button2);
			View button3 = FindViewById(R.Id.button3);
			_text1 = (TextView)FindViewById(R.Id.text1);
			DisplayColor();

			button1.Click += delegate
			{
				OpenDialog(false);
			};
			button2.Click += delegate
			{
				OpenDialog(true);
			};
			button3.Click += (sender, e) =>
			{
				StartActivity(new Intent(ApplicationContext, typeof(AmbilWarnaDemoPreferenceActivity)));
			};
		}

		private void OpenDialog(bool supportAlpha)
		{
            // Set Listener
			//var listener = new AmbilWarnaDialog.OnAmbilWarnaListener();
			//listener.Ok += (sender, e) =>
			//{
			//	Toast.MakeText(ApplicationContext, "Ok", ToastLength.Short).Show();
			//	_color = e.Color;
			//	DisplayColor();
			//};
			//listener.Cancel += (sender, e) =>
			//{
			//	Toast.MakeText(ApplicationContext, "Cancel", ToastLength.Short).Show();
			//};

			//var dialog = new AmbilWarnaDialog(this, _color, supportAlpha, listener);
			//dialog.Show();



            // EventHandler
            var dialog = new AmbilWarnaDialog(this, _color, supportAlpha);
		    dialog.Ok += (sender, e) =>
		    {
                Toast.MakeText(ApplicationContext, $"Ok : #{e.Color.ToString("x8").ToUpper()}", ToastLength.Short).Show();
                _color = e.Color;
                DisplayColor();
            };
		    dialog.Cancel += (sender, e) =>
		    {
		        Toast.MakeText(ApplicationContext, "Cancel", ToastLength.Short).Show();
            };
            dialog.Show();
		}


		private void DisplayColor()
		{
			_text1.Text = $"Current Color: {_color.ToString("x8").ToUpper()}";
		}
	}
}

