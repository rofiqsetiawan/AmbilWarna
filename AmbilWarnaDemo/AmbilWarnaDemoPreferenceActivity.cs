using Android.App;
using Android.OS;
using Android.Preferences;
using R = AmbilWarnaDemo.Resource;

namespace AmbilWarnaDemo
{
	[Activity(Label = "AmbilWarnaDemoPreferenceActivity")]
	public class AmbilWarnaDemoPreferenceActivity : PreferenceActivity
	{
		protected override void OnCreate(Bundle savedInstanceState)
		{
			base.OnCreate(savedInstanceState);

			FragmentManager.BeginTransaction()
						   .Replace(Android.Resource.Id.Content, new MyPreferenceFragment())
						   .Commit();
		}

		public class MyPreferenceFragment : PreferenceFragment
		{
			public override void OnCreate(Bundle savedInstanceState)
			{
				base.OnCreate(savedInstanceState);
				AddPreferencesFromResource(R.Xml.demopreference);
			}
		}
	}
}
