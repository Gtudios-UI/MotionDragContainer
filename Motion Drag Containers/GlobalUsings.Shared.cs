global using System;
global using System.Collections.Generic;
global using System.IO;
global using System.Linq;
global using System.Diagnostics.CodeAnalysis;
global using System.Numerics;
global using System.Collections;
global using System.Collections.Specialized;
global using System.Diagnostics;
global using System.Runtime.InteropServices;
global using Get.Data.Properties;
global using Get.EasyCSharp;
global using Get.Data.Collections;
global using Get.Data.UIModels;
global using Get.Data.Collections.Update;
global using Get.Data.DataTemplates;
global using Get.Data.Helpers;
global using Get.UI.Data;
global using Gtudios.UI.MotionDrag;
global using Get.Data.Bindings.Linq;
global using static Get.Data.Properties.AutoTyper;
global using static Get.Data.XACL.QuickBindingExtension;
global using static Get.UI.Data.QuickCreate;
global using DataTemplate = Get.Data.DataTemplates.DataTemplate;

namespace Gtudios.UI.MotionDragContainers;
internal static class SelfNote
{
    [DoesNotReturn]
    public static void ThrowNotImplemented() => throw new NotImplementedException();
    [DoesNotReturn]
    public static T ThrowNotImplemented<T>() => throw new NotImplementedException();
    /// <summary>
    /// Notes that the following code has the code that is not allowed in UWP certification.
    /// </summary>
    public static void HasDisallowedPInvoke() { }
    public static void DebugBreakOnShift()
    {
#if DEBUG
        [DllImport("user32.dll")]
        static extern short GetAsyncKeyState(int vKey);
        if (GetAsyncKeyState(16) != 0)
            Debugger.Break();
#endif
    }
}