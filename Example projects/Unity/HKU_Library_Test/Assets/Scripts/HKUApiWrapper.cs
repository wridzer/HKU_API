using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

public class HKUApiWrapper
{
    [DllImport("HKU_SDK", CallingConvention = CallingConvention.Cdecl)]
    public static extern void GetUsers(IntPtr callback, IntPtr context);

    public delegate void UsersCallbackDelegate(IntPtr users, int length, IntPtr context);

}
