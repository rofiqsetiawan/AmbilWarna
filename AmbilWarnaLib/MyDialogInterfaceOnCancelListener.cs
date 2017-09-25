// Created by Rofiq Setiawan (rofiqsetiawan@gmail.com)

using System;
using Android.Content;

namespace Yuku.AmbilWarna
{
    internal class MyDialogInterfaceOnCancelListener : Java.Lang.Object, IDialogInterfaceOnCancelListener
    {
        private readonly Action<IDialogInterface> _callback;

        public MyDialogInterfaceOnCancelListener(Action<IDialogInterface> callback)
        {
            _callback = callback;
        }

        public void OnCancel(IDialogInterface dialog)
        {
            _callback(dialog);
        }
    }
}