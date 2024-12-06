using System;
using Remotion.Linq.Parsing.Structure.IntermediateModel;
using Rg.Plugins.Popup.Services;

namespace Vendor.ViewModels;

public partial class ChangePriceViewModel : BaseViewModel
{

    [ObservableProperty]
    private double startprice, numGreedW, numGreedH, numButtonRadius;
    [ObservableProperty, NotifyPropertyChangedFor(nameof(OperIsVisoble)), NotifyPropertyChangedFor(nameof(SnumIsVisoble))]
    private string oper, fnum, snum;

    private bool ischangestart;
    private bool isdoublef, isdoubles;
    public bool OperIsVisoble { get => !string.IsNullOrWhiteSpace(Oper); }
    public bool SnumIsVisoble { get => !string.IsNullOrWhiteSpace(Snum); }

    public ChangePriceViewModel()
    {
    }

    [RelayCommand]
    private async Task OkTapped()
    {
        if (!string.IsNullOrWhiteSpace(Snum) && !string.IsNullOrWhiteSpace(Oper))
        {
            if (Oper == "+")
                Fnum = (double.Parse(Fnum) + double.Parse(Snum)).ToString();
            if (Oper == "-")
                Fnum = (double.Parse(Fnum) - double.Parse(Snum)).ToString();
            if (Oper == "*")
                Fnum = (double.Parse(Fnum) * double.Parse(Snum)).ToString();
            if (Oper == "/")
            {
                if (double.Parse(Snum) == 0)
                {
                    // Деление на ноль - обработка ошибки
                    return;
                }
                Fnum = (double.Parse(Fnum) / double.Parse(Snum)).ToString();
            }


            Snum = string.Empty;
            Oper = string.Empty;
        }
        await PopupNavigation.Instance.PopAsync();
    }

    private bool IsParseDoubl()
    {
        if (!double.TryParse(Fnum, out double fnum) || !double.TryParse(Snum, out double snum))
        {
            
            return false;
            //throw new Exception("Неверный ввод. Пожалуйста, введите действительный номер.");
        }
        return true;
    }

    [RelayCommand]
    private void KeyInput(string data)
    {
        string commands = @"-+*/=%";
        if (data == "back")
        {
            if (string.IsNullOrWhiteSpace(Oper))
            {
                if (Fnum.Length > 1)
                    if (Fnum.Last() == ',')
                    {
                        isdoublef = false;
                        Fnum = Fnum.Substring(0, Fnum.Length - 1);
                    }
                    else
                        Fnum = Fnum.Substring(0, Fnum.Length - 1);
                else
                    Fnum = "0";
            }
            else
            {
                if (string.IsNullOrWhiteSpace(Snum) && Oper.Length != 0)
                {
                    Oper = string.Empty;
                }
                else if (!string.IsNullOrEmpty(Oper) && Snum.Length == 0)
                {
                    Oper = string.Empty;
                }
                else
                {
                    if (Snum.Length > 1)
                        if (Snum.Last() == ',')
                        {
                            isdoubles = false;
                            Snum = Snum.Substring(0, Snum.Length - 1);
                        }
                        else
                            Snum = Snum.Substring(0, Snum.Length - 1);
                    else if (Snum.Length == 1)
                    {
                        Snum = string.Empty;
                    }
                }
            }
        }
        if (data == "clear")
        {
            Fnum = "0";
            Snum = string.Empty;
            Oper = string.Empty;
            isdoublef = false;
            isdoubles = false;
        }
        else if (commands.Contains(data))
        {
            ischangestart = true;
            if (Snum == null)
                Snum = string.Empty;
            if (data != "=")
            {
                if (data == "%" && string.IsNullOrEmpty(Oper))
                    Oper = data;
                else
                    if (data != "%") { Oper = data; }
            }
            if (data == "=" && Snum != string.Empty || data == "%" && Snum != string.Empty)
            {
                Fnum = Fnum.Replace(',', '.');
                Snum = Snum.Replace(',', '.');

                if (!IsParseDoubl())
                {
                    Fnum = Fnum.Replace('.', ',');
                    Snum = Snum.Replace('.', ',');
                }

                if (!double.TryParse(Fnum, out double fnum) || !double.TryParse(Snum, out double snum))
                {
                    // Проверка На возможность запарсить в дабл
                    return;
                    // Выводит сообщение об ошибке, если либо Num, либо Num не удается разобрать как double

                }

                if (data == "%")
                {
                    if (Oper == "+")
                    {
                        Fnum = (double.Parse(Fnum) + (double.Parse(Fnum) * double.Parse(Snum) / 100)).ToString();
                    }
                    if (Oper == "-")
                    {
                        Fnum = (double.Parse(Fnum) - (double.Parse(Fnum) * double.Parse(Snum) / 100)).ToString();
                    }
                    if (Oper == "*")
                    {
                        Fnum = (double.Parse(Fnum) * double.Parse(Snum) / 100).ToString();
                    }
                    if (Oper == "%")
                        Fnum = (double.Parse(Fnum) * double.Parse(Snum) / 100).ToString();

                    if (Oper == "/")
                    {
                        if (snum == 0)
                        {
                            // Деление на ноль - обработка ошибки
                            return;
                        }
                        Fnum = (double.Parse(Fnum) / (double.Parse(Snum) / 100)).ToString();
                    }
                }
                if (data == "=")
                {
                    if (Oper == "+")
                        Fnum = (double.Parse(Fnum) + double.Parse(Snum)).ToString();
                    if (Oper == "-")
                        Fnum = (double.Parse(Fnum) - double.Parse(Snum)).ToString();
                    if (Oper == "*")
                        Fnum = (double.Parse(Fnum) * double.Parse(Snum)).ToString();
                    if (Oper == "%")
                        Fnum = (double.Parse(Fnum) * double.Parse(Snum) / 100).ToString();
                    if (Oper == "/")
                    {
                        if (snum == 0)
                        {
                            // Деление на ноль - обработка ошибки
                            return;
                        }
                        Fnum = (double.Parse(Fnum) / double.Parse(Snum)).ToString();
                    }
                }

                Snum = string.Empty;
                if (!Fnum.Contains(',') && !Fnum.Contains('.'))
                    isdoublef = false;
                else
                    isdoublef = true;
                isdoubles = false;
                if (data != "=" && data != "%")
                {
                    Oper = data;
                }
                else
                    Oper = string.Empty;
            }
            else if (Oper != data && data != "=")
                Oper = data;
        }
        else
        {
            if (!char.IsDigit(data[0]) && data != ",")
            {
                // Отобразит сообщение об ошибке, если вводимые данные не являются цифрой или десятичной точкой
                return;
            }

            if (ischangestart)
            {
                if (data == ",")
                {
                    if (!isdoublef && string.IsNullOrEmpty(Oper) && Fnum.Length < 21)
                    {
                        Fnum += data;
                        isdoublef = true;
                    }
                    else if (!isdoubles && !string.IsNullOrEmpty(Oper) && Snum.Length < 21)
                    {
                        {
                            if (string.IsNullOrEmpty(Snum))
                                Snum = "0,";
                            else
                                Snum += data;
                            isdoubles = true;
                        }
                    }
                    else
                    {
                        return;
                    }
                }
                else if (string.IsNullOrWhiteSpace(Oper) && Fnum.Length < 21)
                {
                    if (Fnum != "0")
                        Fnum += data;
                    else
                        Fnum = data;
                }
                else if (!string.IsNullOrWhiteSpace(Oper) && Snum.Length < 21)
                {
                    Snum += data;
                }
                else
                {
                    return;
                }
            }
            else
            {
                if (data == ",")
                {
                    Fnum = "0,";
                    isdoublef = true;
                }
                else
                {
                    Fnum = "";
                    Fnum += data;
                }
                if (!Fnum.Contains(','))
                    isdoublef = false;
                isdoubles = false;
                ischangestart = true;
                Snum = "";
            }
        }
    }

    [RelayCommand]
    private async Task CencelTapped()
    {
        Fnum = "-1";
        //Fnum = startprice.ToString();
        await PopupNavigation.Instance.PopAsync();
    }
}

