using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using static HKUApiWrapper;

public class HKU_Implementation : MonoBehaviour
{
    private HKUApiWrapper.UsersCallbackDelegate myUsersCallbackDelegate;
    private HKUApiWrapper.ConfigureProjectCallbackDelegate myConfigureProjectCallbackDelegate;

    bool isConfigured = false;
    bool isLoggedin = false;

    string projectID = "1480e7e3-8c14-4609-8d34-23b82fcdc8ea";
    string userID = "";

    private void Start()
    {
        myConfigureProjectCallbackDelegate = ConfigureProjectCallback;
        IntPtr callbackPtr = Marshal.GetFunctionPointerForDelegate(myConfigureProjectCallbackDelegate);
        HKUApiWrapper.ConfigureProject(new char[][] { projectID.ToCharArray() }, callbackPtr, IntPtr.Zero);
        HKUApiWrapper.OpenLoginPage();
    }

    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.Space))
        {
            myUsersCallbackDelegate = MyUsersCallback;
            IntPtr callbackPtr = Marshal.GetFunctionPointerForDelegate(myUsersCallbackDelegate);
            HKUApiWrapper.GetUsers(callbackPtr, IntPtr.Zero);
        }
    }

    // Callbacks
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

    public static void ConfigureProjectCallback(bool IsSucces, IntPtr context)
    {
        if (IsSucces)
        {
            //static_cast<HKU_Implementation>(context).isConfigured = true;
            Debug.Log("Project configured succesfully");
        }
        else
        {
            Debug.Log("Project configuration failed");
        }
    }
}
