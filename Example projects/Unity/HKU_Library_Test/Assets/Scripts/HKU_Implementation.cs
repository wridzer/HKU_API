using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using static HKUApiWrapper;

public class HKU_Implementation : MonoBehaviour
{
    private HKUApiWrapper.UsersCallbackDelegate myUsersCallbackDelegate;

    private void Start()
    {
        myUsersCallbackDelegate = HKUApiWrapper.MyUsersCallback;
        IntPtr callbackPtr = Marshal.GetFunctionPointerForDelegate(myUsersCallbackDelegate);
        HKUApiWrapper.GetUsers(callbackPtr, IntPtr.Zero);
    }
}
