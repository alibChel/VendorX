using Forms9Patch;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace VendorX.Widgets;

[XamlCompilation(XamlCompilationOptions.Compile)]
public partial class CheckView 
{
    CheckViewModel vm;

    public CheckView(string html)
    {
        InitializeComponent();
        vm = (CheckViewModel)BindingContext;
        vm.HttmlPath = html;
      
        ///Forms9Patch.WebViewPrintEffect.ApplyTo(web);
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        vm.OnAppearing();
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
    }
    private async void Share_Tapped(object sender, EventArgs e)
    {
        if (ToPdfService.IsAvailable)
        {
            string name = $"{DateTime.Now.Date.Ticks}";
          
                if ( await webView.ToPdfAsync($"{name}") is ToFileResult pdfResult)
                {
                if (pdfResult.IsError)
                {
                    await DialogService.ShowToast($"{pdfResult.Result}");
                    return;
                }
                else
                {

                    try
                    {
                        var fileStream = File.OpenRead(pdfResult.Result);
                        var cacheFile = Path.Combine(FileSystem.CacheDirectory, $"{name}.pdf");

                        using (var file = new FileStream(cacheFile, FileMode.Create, FileAccess.Write))
                        {
                            fileStream.CopyTo(file);
                            File.Delete(pdfResult.Result);
                        }

                        var reg = new ShareFileRequest();
                        reg.Title = $"{name}.pdf";
                        reg.File = new ShareFile(cacheFile);

                        await Share.RequestAsync(reg);

                        // на андроиде не ожидает завершения, по этому удаляем только на ios
                        if (Device.RuntimePlatform == Device.iOS)
                            File.Delete(cacheFile);
                    }
                    catch (Exception)
                    {

                    }
                }
                }
        
             
        }
       else
            DialogService.ShowToast ( "Данная функция не доступна на устройстве");
    }
    public static string RandomString(int length)
    {
        Random random = new Random();
        const string chars = "123456789";
        return new string(Enumerable.Repeat(chars, length)
            .Select(s => s[random.Next(s.Length)]).ToArray());
    }
}