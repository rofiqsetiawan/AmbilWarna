// Created by Rofiq Setiawan (rofiqsetiawan@gmail.com)
using System;
using Android.Views;

namespace Yuku.AmbilWarna
{
	internal class MyVtoOnGlobalLayoutListener : Java.Lang.Object, ViewTreeObserver.IOnGlobalLayoutListener
	{
		public event EventHandler<EventArgs> GlobalLayoutEvent;

		public void OnGlobalLayout()
		{
		    GlobalLayoutEvent?.Invoke(this, EventArgs.Empty);
		}
	}
}
