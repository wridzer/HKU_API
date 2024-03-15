using System;
using System.Runtime.InteropServices;

public class HKUApiWrapper
{
    // Definieer de callback delegate
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void HKUCallback(IntPtr users, int usersLength);

    [DllImport("HKU_API_LIB", CallingConvention = CallingConvention.Cdecl)]
    public static extern void GetUsers(HKUCallback callback, IntPtr context);

    // Een methode om je callback te verwerken
    public static void MyUsersCallback(IntPtr usersPtr, int usersLength)
    {
        IntPtr[] ptrArray = new IntPtr[usersLength];
        Marshal.Copy(usersPtr, ptrArray, 0, usersLength);

        string[] users = new string[usersLength];
        for (int i = 0; i < usersLength; i++)
        {
            users[i] = Marshal.PtrToStringAnsi(ptrArray[i]);
        }

        for (int i = 0; i < users.Length; i++)
        {
            Console.WriteLine(users[i]);
        }
    }
}
