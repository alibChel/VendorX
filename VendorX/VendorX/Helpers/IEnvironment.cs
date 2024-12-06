using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;

namespace VendorX.Helpers;

public interface IEnvironment
{
    void SetStatusBarColor(Color color, bool darkStatusBarTint);
}
