using System;
using System.Runtime.InteropServices;

namespace VeroEngine.Core.Generic;
// Sadly this only works on Win so we cant have a standard debug console on linux
// I'm going to replace this with a fancy new GUI Console soon anyway
public class NativeMethods
{
    [DllImport("kernel32.dll", SetLastError = true)]
    internal static extern int AllocConsole();
    
    [DllImport("kernel32.dll", SetLastError = true)]
    internal static extern int FreeConsole();
    
    [DllImport("User32.dll", CharSet = CharSet.Unicode)]
    public static extern int MessageBox(IntPtr h, string m, string c, int type);
}