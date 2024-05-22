using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using static HKUApiWrapper;

public class HKU_Implementation : MonoBehaviour
{
    private HKUApiWrapper.UsersCallbackDelegate myUsersCallbackDelegate;
    private ConfigureProjectCallbackDelegate myConfigureProjectCallbackDelegate;
    private LoginStatusCallbackDelegate myLoginStatusCallbackDelegate;
    private GCHandle gch;

    bool isConfigured = false;
    public bool isLoggedin = false;

    string projectID = "1480e7e3-8c14-4609-8d34-23b82fcdc8ea";
    string userID = "";

    private void Start()
    {
        // Register project
        myConfigureProjectCallbackDelegate = ConfigureProjectCallback;
        HKUApiWrapper.ConfigureProject(projectID, myConfigureProjectCallbackDelegate, IntPtr.Zero);
        HKUApiWrapper.OpenLoginPage();

        // Open login page
        myLoginStatusCallbackDelegate = LoginStatusCallback;

        // Poll login status
        gch = GCHandle.Alloc(this);
        IntPtr contextPtr = GCHandle.ToIntPtr(gch);
        PollLoginStatus(myLoginStatusCallbackDelegate, contextPtr);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            myUsersCallbackDelegate = MyUsersCallback;
            IntPtr callbackPtr = Marshal.GetFunctionPointerForDelegate(myUsersCallbackDelegate);
            HKUApiWrapper.GetUsers(callbackPtr, IntPtr.Zero);
        }
        //currentUserID = Marshal.PtrToStringAnsi(Marshal.StringToHGlobalAnsi(HKUApiWrapper.currentUserID));
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

    public static void ConfigureProjectCallback(bool IsSuccess, IntPtr context)
    {
        if (IsSuccess)
        {
            Debug.Log("Project configured successfully");
        }
        else
        {
            Debug.Log("Project configuration failed");
        }
    }

    public static void LoginStatusCallback(bool IsSuccess, IntPtr context)
    {
        if (IsSuccess)
        {
            Debug.Log("Logged in");
            GCHandle gch = GCHandle.FromIntPtr(context);
            HKU_Implementation instance = (HKU_Implementation)gch.Target;
            instance.isLoggedin = true;
        }
        else
        {
            Debug.Log("Not logged in");
        }
    }

    private void OnDestroy()
    {
        if (gch.IsAllocated)
        {
            gch.Free();
        }
    }
}
