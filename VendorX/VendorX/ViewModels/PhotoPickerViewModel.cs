using System.Collections.Generic;
using SkiaSharp; 
using System.IO;
using System.Linq;
using System.Threading;
using DevExpress.XamarinForms.IO;
using NativeMedia;
using Rg.Plugins.Popup.Services;
using Xamarin.Essentials;
using VendorX.Resources;

namespace Vendor.ViewModels;

public partial class PhotoPickerViewModel : BaseViewModel
{

    [ObservableProperty]
    private string message;

    [ObservableProperty]
    private bool isCameraActive;

    public int Count;

    public List<string> Result { get; set; } = new();

    private static Random random = new Random();
    private const string characters = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";

    public PhotoPickerViewModel()
	{
	}
   
    public static string GenerateRandomString(int length)
    {
       
        return new string(Enumerable.Repeat(characters, length)
            .Select(s => s[random.Next(s.Length)])
            .ToArray());
    }
    // Выбор из галереи
    [RelayCommand]
    private async Task GaleryTapped()
    {
        var cts = new CancellationTokenSource();
        IMediaFile[] files = null;

        try
        {
            var request = new MediaPickRequest(Count, MediaFileType.Image)
            {
                PresentationSourceBounds = System.Drawing.Rectangle.Empty,
                UseCreateChooser = true 
            };

            cts.CancelAfter(TimeSpan.FromMinutes(5));

            var results = await MediaGallery.PickAsync(request, cts.Token);
            files = results?.Files?.ToArray();
        }
        catch (OperationCanceledException)
        {
            // handling a cancellation request
          
        }
        catch (Exception)
        {
            
            // handling other exceptions
        }
        finally
        {
            cts.Dispose();
        }

        if (files == null)
            return;

        foreach (var file in files)
        {
            try
            {
               
                // var contentType = file.ContentType;
                //using var stream = await file.OpenReadAsync();
                using (var stream = await file.OpenReadAsync())
                {
                    var fileName = GenerateRandomString(10); //Can return an null or empty value
                    var extension = file.Extension;
                    SKBitmap bitmap = SKBitmap.Decode(stream);

                    var origin = SKCodec.Create(await file.OpenReadAsync());

                    var _bitmap = AutoOrient(bitmap, origin.EncodedOrigin);


                    int maxSize = 1500; // максимальный размер по большей стороне
                    int width = _bitmap.Width;
                    int height = _bitmap.Height;
                    if (width > height)
                    {
                        height = (int)((float)maxSize / width * height);
                        width = maxSize;
                    }
                    else
                    {
                        width = (int)((float)maxSize / height * width);
                        height = maxSize;
                    }

                    var resizedBitmap = _bitmap.Resize(new SKImageInfo(width, height), SKBitmapResizeMethod.Lanczos3);


                    using (var output = File.OpenWrite($"{FileSystem.CacheDirectory}/{fileName}.{extension}"))
                    {
                        resizedBitmap.Encode(SKEncodedImageFormat.Jpeg, 80).SaveTo(output);
                    }


                    var filePath = Path.Combine(FileSystem.CacheDirectory, $"{fileName}.{extension}");
                    /* using (var stream = new System.IO.FileStream(filePath, System.IO.FileMode.Create, System.IO.FileAccess.Write))
                    {
                        filePath.CopyTo(stream);
                    }*/


                    //using System.IO.FileStream _stream = new (filePath, System.IO.FileMode.Create);
                    //await stream.CopyToAsync(_stream);
                    // await file.s
                    //if (image != null)
                    /*   {
                          using FileStream _stream = new(filePath, FileMode.Create);
                          await image.SaveAsync(_stream, imform, 0.8f);
                       }*/

                    Result.Add(filePath);
                }

               
            }
            catch (Exception ex)
            {
                await DialogService.ShowToast(ex.Message);
            }
            await Task.Delay(100);
            file.Dispose();
        }

        await CloseTapped();
    }

    // Вызов камеры
    [RelayCommand]
    private async Task CameraTapped()
    {


        var status = await Permissions.CheckStatusAsync<Permissions.Camera>();
        if (status != PermissionStatus.Granted)
        {
            status = await Permissions.RequestAsync<Permissions.Camera>();
            if (status != PermissionStatus.Granted)
            {
                await DialogService.ShowToast($"{AppResources.CameraExtError}");
                return;
            }

        }



        IsCameraActive = true;

        if (!MediaGallery.CheckCapturePhotoSupport())
        {
            IsCameraActive = false;
            return;
        }



        //FileResult photo = await MediaPicker.CapturePhotoAsync();

        using var file = await MediaGallery.CapturePhotoAsync();
        if (file == null)
        {
            IsCameraActive = false;
            return;
        }

        using (var stream = await file.OpenReadAsync())
        {
            var fileName = file.NameWithoutExtension; //Can return an null or empty value
            var extension = file.Extension;


            SKBitmap bitmap = SKBitmap.Decode(stream);


            var origin = SKCodec.Create(await file.OpenReadAsync());

            var _bitmap = AutoOrient(bitmap, origin.EncodedOrigin);


            int maxSize = 1500; // максимальный размер по большей стороне
            int width = _bitmap.Width;
            int height = _bitmap.Height;
            if (width > height)
            {
                height = (int)((float)maxSize / width * height);
                width = maxSize;
            }
            else
            {
                width = (int)((float)maxSize / height * width);
                height = maxSize;
            }

            var resizedBitmap = _bitmap.Resize(new SKImageInfo(width, height), SKBitmapResizeMethod.Lanczos3);


            using (var output = File.OpenWrite($"{FileSystem.CacheDirectory}/{fileName}.{extension}"))
            {
                resizedBitmap.Encode(SKEncodedImageFormat.Jpeg, 80).SaveTo(output);
            }

            var filePath = Path.Combine(FileSystem.CacheDirectory, $"{fileName}.{extension}");

            Result.Add(filePath);
            await Task.Delay(100);
            file.Dispose();
            await CloseTapped();
        };
       
    }

    // Закрытие попапа
    [RelayCommand]
    private async Task CloseTapped()
    {
        IsCameraActive = false;
        await PopupNavigation.Instance.PopAsync();
    }


    private static SKBitmap AutoOrient(SKBitmap bitmap, SKEncodedOrigin origin)
    {
        SKBitmap rotated;
        switch (origin)
        {
            case SKEncodedOrigin.BottomRight:
                using (var surface = new SKCanvas(bitmap))
                {
                    surface.RotateDegrees(180, bitmap.Width / 2, bitmap.Height / 2);
                    surface.DrawBitmap(bitmap.Copy(), 0, 0);
                }
                return bitmap;
            case SKEncodedOrigin.RightTop:
                rotated = new SKBitmap(bitmap.Height, bitmap.Width);
                using (var surface = new SKCanvas(rotated))
                {
                    surface.Translate(rotated.Width, 0);
                    surface.RotateDegrees(90);
                    surface.DrawBitmap(bitmap, 0, 0);
                }
                return rotated;
            case SKEncodedOrigin.LeftBottom:
                rotated = new SKBitmap(bitmap.Height, bitmap.Width);
                using (var surface = new SKCanvas(rotated))
                {
                    surface.Translate(0, rotated.Height);
                    surface.RotateDegrees(270);
                    surface.DrawBitmap(bitmap, 0, 0);
                }
                return rotated;
            default:
                return bitmap;
        }
    }
}

