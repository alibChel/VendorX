using Android.App;
using Android.Content;
using Android.Media;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Vendor.Services;
using Vendor.Droid;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xamarin.Forms;

[assembly: Dependency(typeof(BeepService))]
namespace Vendor.Droid
{
    public class BeepService : IBeep
    {
        public BeepService()
        {

        }

        public void PlayAudioFile(string fileName)
        {
            var player = new MediaPlayer();
            var fd = global::Android.App.Application.Context.Assets.OpenFd(fileName);
            player.Prepared += (s, e) =>
            {
                player.Start();
            };
            player.SetDataSource(fd.FileDescriptor, fd.StartOffset, fd.Length);
            player.Prepare();
        }
    }
}
