using System;
using System.Runtime.InteropServices;

public class HKUApiWrapper
{
    [DllImport("HKU_API_LIB", CallingConvention = CallingConvention.Cdecl)]
    public static extern void GetUsers(IntPtr callback, IntPtr context);

    public delegate void UsersCallbackDelegate(IntPtr users, int length, IntPtr context);

    public static void MyUsersCallback(IntPtr users, int length, IntPtr context)
    {
        // Interpretatie van de ontvangen char* array.
        for (int i = 0; i < length; i++)
        {
            IntPtr userPtr = Marshal.ReadIntPtr(users, i * IntPtr.Size);
            string user = Marshal.PtrToStringAnsi(userPtr);
            Console.WriteLine(user);
        }
    }
}
