using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.SocialPlatforms.Impl;
using static HKU.HKUApiWrapper;
using static UnityEditor.Timeline.TimelinePlaybackControls;

public class HKU_Implementation
{
    private UsersCallbackDelegate myUsersCallbackDelegate;
    private ConfigureProjectCallbackDelegate myConfigureProjectCallbackDelegate;
    private LoginStatusCallbackDelegate myLoginStatusCallbackDelegate;
    private LogoutCallbackDelegate myLogoutCallbackDelegate;
    private GetUserCallbackDelegate myGetUserCallbackDelegate;
    private UploadLeaderboardScoreCallbackDelegate myUploadLeaderboardScoreCallbackDelegate;
    private LeaderboardCallbackDelegate myGetLeaderboardCallbackDelegate;
    private GetLeaderboardsForProjectCallbackDelegate myGetLeaderboardsForProjectCallbackDelegate;
    private DebugOutputDelegate myDebugOutputDelegate;

    private GCHandle gch;
    private IntPtr contextPtr = IntPtr.Zero;

    bool isConfigured = false;
    public bool isLoggedin = false;

    string projectID = "1480e7e3-8c14-4609-8d34-23b82fcdc8ea";
    public string userID = "";

    public void Initialize()
    {
        // Register debug output
        myDebugOutputDelegate = SetOutputDebugCallback;
        SetOutputCallback(myDebugOutputDelegate, IntPtr.Zero);

        // Register project
        myConfigureProjectCallbackDelegate = ConfigureProjectCallback;
        ConfigureProject(projectID, myConfigureProjectCallbackDelegate, IntPtr.Zero);

        gch = GCHandle.Alloc(this);
        contextPtr = GCHandle.ToIntPtr(gch);
    }

    public void Login()
    {
        // Open login page
        OpenLoginPage();

        // Poll login status
        myLoginStatusCallbackDelegate = LoginStatusCallback;
        StartPolling(myLoginStatusCallbackDelegate, contextPtr);
    }

    public void Logoff()
    {
        // Logout
        myLogoutCallbackDelegate = LogoutCallback;
        Logout(myLogoutCallbackDelegate, contextPtr);
    }

    public string GetUsername(string userID)
    {
        string username = null;
        GCHandle handle = GCHandle.Alloc(username);
        IntPtr contextPtr = GCHandle.ToIntPtr(handle);

        myGetUserCallbackDelegate = (usernameOut, length, context) =>
        {
            username = Marshal.PtrToStringAnsi(usernameOut);
            Debug.Log(username);
            handle.Free();
        };

        GetUser(userID, myGetUserCallbackDelegate, contextPtr);

        return username;
    }

    public void UploadScore(string leaderboard, int score)
    {
        // Upload score
        myUploadLeaderboardScoreCallbackDelegate = (isSuccess, rank, context) =>
        {
            if (isSuccess)
            {
                Debug.Log("Score uploaded successfully, you are now rank: " + rank);
            }
            else
            {
                Debug.Log("Failed to upload score");
            }
        };
        UploadLeaderboardScore(leaderboard, score, myUploadLeaderboardScoreCallbackDelegate, contextPtr);
    }

    public void GetLeaderboards(Action<(string name, string id)[]> callback)
    {
        IntPtr outArrayPtr = IntPtr.Zero;
        myGetLeaderboardsForProjectCallbackDelegate = (isSuccess, context) =>
        {
            if (isSuccess)
            {
                // Parse outArrayPtr to string array and invoke the callback
                var leaderboards = MarshalPtrToNameIdArray(outArrayPtr);
                foreach (var leaderboard in leaderboards)
                {
                    Debug.Log($"Leaderboard: Name = {leaderboard.name}, ID = {leaderboard.id}"); // Debugging information
                }
                callback(leaderboards);
                ReleaseMemory(outArrayPtr); // Free the memory
            }
            else
            {
                Debug.LogError("Failed to fetch leaderboards for project.");
                callback(null);
            }
        };
        GetLeaderboardsForProject(ref outArrayPtr, myGetLeaderboardsForProjectCallbackDelegate, contextPtr);
    }

    public void GetLeaderboardEntries(string leaderboard, int amount, GetEntryOptions option, Action<HKU.LeaderboardEntry[]> callback)
    {
        IntPtr outArrayPtr = IntPtr.Zero;
        myGetLeaderboardCallbackDelegate = (isSuccess, context) =>
        {
            if (isSuccess)
            {
                // Parse outArrayPtr to LeaderboardEntry array and invoke the callback
                HKU.LeaderboardEntry[] entries = MarshalPtrToLeaderboardEntryArray(outArrayPtr);
                callback(entries);
                //FreeMemory(outArrayPtr); // Free the memory
            }
            else
            {
                callback(null);
            }
        };
        GetLeaderboard(leaderboard, ref outArrayPtr, amount, option, myGetLeaderboardCallbackDelegate, contextPtr);
    }

    private (string name, string id)[] MarshalPtrToNameIdArray(IntPtr ptr)
    {
        int count = 0;
        // First count the number of pairs in the array
        while (Marshal.ReadIntPtr(ptr, count * IntPtr.Size) != IntPtr.Zero)
        {
            count++;
        }

        var result = new (string name, string id)[count / 2];
        for (int i = 0; i < count; i += 2)
        {
            IntPtr namePtr = Marshal.ReadIntPtr(ptr, i * IntPtr.Size);
            IntPtr idPtr = Marshal.ReadIntPtr(ptr, (i + 1) * IntPtr.Size);

            result[i / 2] = (Marshal.PtrToStringAnsi(namePtr), Marshal.PtrToStringAnsi(idPtr));
        }

        return result;
    }

    private HKU.LeaderboardEntry[] MarshalPtrToLeaderboardEntryArray(IntPtr ptr)
    {
        int count = 0;
        // First count the number of entries in the array
        while (Marshal.ReadIntPtr(ptr, count * IntPtr.Size) != IntPtr.Zero)
        {
            count++;
        }

        var result = new HKU.LeaderboardEntry[count / 3];
        for (int i = 0; i < count; i += 3)
        {
            IntPtr playerIdPtr = Marshal.ReadIntPtr(ptr, i * IntPtr.Size);
            IntPtr scorePtr = Marshal.ReadIntPtr(ptr, (i + 1) * IntPtr.Size);
            IntPtr rankPtr = Marshal.ReadIntPtr(ptr, (i + 2) * IntPtr.Size);

            result[i / 3] = new HKU.LeaderboardEntry
            {
                PlayerID = Marshal.PtrToStringAnsi(playerIdPtr),
                Score = int.Parse(Marshal.PtrToStringAnsi(scorePtr)),
                Rank = int.Parse(Marshal.PtrToStringAnsi(rankPtr))
            };
        }

        return result;
    }

    private void ReleaseMemory(IntPtr ptr)
    {
        if (ptr != IntPtr.Zero)
        {
            //FreeMemory(ptr);
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

    public static void LoginStatusCallback(bool IsSuccess, string Id, IntPtr context)
    {
        if (IsSuccess)
        {
            Debug.Log("Logged in as user: " + Id);
            GCHandle gch = GCHandle.FromIntPtr(context);
            HKU_Implementation instance = (HKU_Implementation)gch.Target;
            instance.isLoggedin = true;
            instance.userID = Id;
        }
        else
        {
            Debug.Log("Not logged in");
        }
    }

    public static void LogoutCallback(bool IsSuccess, IntPtr context)
    {
        if (IsSuccess)
        {
            Debug.Log("Logged out");
            GCHandle gch = GCHandle.FromIntPtr(context);
            HKU_Implementation instance = (HKU_Implementation)gch.Target;
            instance.isLoggedin = false;
            instance.userID = "";
        }
        else
        {
            Debug.Log("Logout failed");
        }
    }

    public static void SetOutputDebugCallback(string message, IntPtr context)
    {
        Debug.Log(message);
    }

    // Destroy
    private void OnDestroy()
    {
        if (gch.IsAllocated)
        {
            gch.Free();
        }
    }
}
