using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using static HKUApiWrapper;

public class HKU_Implementation : MonoBehaviour
{
    private HKUApiWrapper.UsersCallbackDelegate myUsersCallbackDelegate;

    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.Space))
        {
            myUsersCallbackDelegate = MyUsersCallback;
            IntPtr callbackPtr = Marshal.GetFunctionPointerForDelegate(myUsersCallbackDelegate);
            HKUApiWrapper.GetUsers(callbackPtr, IntPtr.Zero);
        }
    }

    public static void MyUsersCallback(IntPtr users, int length, IntPtr context)
    {
        // Interpretatie van de ontvangen char* array.
        for (int i = 0; i < length; i++)
        {
            IntPtr userPtr = Marshal.ReadIntPtr(users, i * IntPtr.Size);
            string user = Marshal.PtrToStringAnsi(userPtr);
            Debug.Log(user);
        }
    }
}
