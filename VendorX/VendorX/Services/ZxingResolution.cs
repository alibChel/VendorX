
using Xamarin.Essentials;
using ZXing.Mobile;

namespace VendorX.Services
{
    public class ZxingResolution
    {
        public static CameraResolution SelectLowestResolutionMatchingDisplayAspectRatio(List<CameraResolution> availableResolutions)
        {
            CameraResolution result = null;  
            double aspectTolerance = 0.1;
            var displayOrientationHeight = DeviceDisplay.MainDisplayInfo.Orientation == DisplayOrientation.Portrait ? DeviceDisplay.MainDisplayInfo.Height : DeviceDisplay.MainDisplayInfo.Width;
            var displayOrientationWidth = DeviceDisplay.MainDisplayInfo.Orientation == DisplayOrientation.Portrait ? DeviceDisplay.MainDisplayInfo.Width : DeviceDisplay.MainDisplayInfo.Height;
          
            var targetRatio = displayOrientationHeight / displayOrientationWidth;
            var targetHeight = displayOrientationHeight;
            var minDiff = double.MaxValue;
            
            foreach (var r in availableResolutions.Where(r => Math.Abs(((double)r.Width / r.Height) - targetRatio) < aspectTolerance))
            {
                if (Math.Abs(r.Height - targetHeight) < minDiff)
                    minDiff = Math.Abs(r.Height - targetHeight);
                result = r;
            }
            return result;
        }


    }
}
