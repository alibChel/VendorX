using AVFoundation;
using Foundation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UIKit;
using Vendor.Services;
using System.IO;
using Forms9Patch.iOS;
using Xamarin.Forms;
using VendorX.iOS;

[assembly: Dependency(typeof(BeepService))]
namespace VendorX.iOS
{
    public class BeepService : IBeep
    {
        public void PlayAudioFile(string fileName)
        {
            string sFilePath = NSBundle.MainBundle.PathForResource(Path.GetFileNameWithoutExtension(fileName), Path.GetExtension(fileName));
            NSUrl url = NSUrl.FromString(sFilePath);
            var _player = AVAudioPlayer.FromUrl(url);
            _player.FinishedPlaying += (object sender, AVStatusEventArgs e) =>
            {
                _player = null;
            };
            _player.Play();
        }
    }
}