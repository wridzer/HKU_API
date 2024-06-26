using System;
using UnityEngine;
using System.Runtime.InteropServices;
using static HKU.HKUApiWrapper;

public class HKUImplementation : MonoBehaviour
{
    private GCHandle gch;
    private IntPtr contextPtr = IntPtr.Zero;
    bool isConfigured = false;
    string projectID = "YOUR_PROJECT_ID_HERE";

    public void Awake()
    {
        // this pointer for context
        gch = GCHandle.Alloc(this);
        contextPtr = GCHandle.ToIntPtr(gch);

        // Set debug output
        DebugOutputDelegate myDebugOutputDelegate = (message, context) =>
        {
            Debug.Log(message);
        };
        SetOutputCallback(myDebugOutputDelegate, IntPtr.Zero);

        // Register project
        ConfigureProjectCallbackDelegate myConfigureProjectCallbackDelegate = (IsSuccess, context) =>
        {
            if (IsSuccess)
            {
                Debug.Log("Project configured successfully");
            }
            else
            {
                Debug.Log("Project configuration failed");
            }
        };
        ConfigureProject(projectID, myConfigureProjectCallbackDelegate, IntPtr.Zero);
    }

    [ContextMenu("Login")]
    public void Login()
    {
        // Open login page
        OpenLoginPage();

        // Poll login status
        LoginStatusCallbackDelegate myLoginStatusCallbackDelegate = (IsSuccess, Id, context) =>
        {
            if (IsSuccess)
            {
                Debug.Log("Login successful with user: " + GetUsername(Id));
            }
            else
            {
                Debug.Log("Login failed");
            }
        };
        StartPolling(myLoginStatusCallbackDelegate, contextPtr);
    }

    public void CancelLogin()
    {
        CancelPolling();
    }

    public void Logoff()
    {
        // Logout
        LogoutCallbackDelegate myLogoutCallbackDelegate = (IsSuccess, context) =>
        {
            if (IsSuccess)
            {
                Debug.Log("Logout successful");
            }
            else
            {
                Debug.Log("Logout failed");
            }
        };
        Logout(myLogoutCallbackDelegate, contextPtr);
    }

    public string GetUsername(string userID)
    {
        string username = null;
        GCHandle handle = GCHandle.Alloc(username);
        IntPtr contextPtr = GCHandle.ToIntPtr(handle);

        GetUserCallbackDelegate myGetUserCallbackDelegate = (usernameOut, length, context) =>
        {
            username = Marshal.PtrToStringAnsi(usernameOut);
            Debug.Log(username);
            handle.Free();
        };

        GetUser(userID, myGetUserCallbackDelegate, contextPtr);

        return username;
    }

    // A function to test the upload score functionality
    [ContextMenu("UploadScore")]
    public void UploadScoreTest()
    {
        UploadScore("YOUR_LEADERBOARD_ID_HERE", 100);
    }

    public void UploadScore(string leaderboardId, int score)
    {
        // Upload score
        UploadLeaderboardScoreCallbackDelegate myUploadLeaderboardScoreCallbackDelegate = (isSuccess, rank, context) =>
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
        UploadLeaderboardScore(leaderboardId, score, myUploadLeaderboardScoreCallbackDelegate, contextPtr);
    }

    // A function to test the get leaderboard entries functionality
    [ContextMenu("GetLeaderboardEntries")]
    public void GetLeaderboardEntriesTest()
    {
        GetLeaderboardEntries("YOUR_LEADERBOARD_ID_HERE", 10, GetEntryOptions.Highest, (entries) =>
        {
            if (entries != null)
            {
                foreach (var entry in entries)
                {
                    Debug.Log($"PlayerID: {entry.PlayerID}, Score: {entry.Score}, Rank: {entry.Rank}");
                }
            }
            else
            {
                Debug.Log("Failed to fetch leaderboard entries");
            }
        });
    }

    public void GetLeaderboardEntries(string leaderboardId, int amount, GetEntryOptions option, Action<HKU.LeaderboardEntry[]> callback)
    {
        IntPtr outArrayPtr = IntPtr.Zero;
        LeaderboardCallbackDelegate myGetLeaderboardCallbackDelegate = (isSuccess, context) =>
        {
            if (isSuccess)
            {
                // Parse outArrayPtr to LeaderboardEntry array and invoke the callback
                HKU.LeaderboardEntry[] entries = MarshalPtrToLeaderboardEntryArray(outArrayPtr);
                callback(entries);
                ReleaseMemory(outArrayPtr); // Free the memory
            }
            else
            {
                callback(null);
            }
        };
        GetLeaderboard(leaderboardId, ref outArrayPtr, amount, option, myGetLeaderboardCallbackDelegate, contextPtr);
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

    public void GetLeaderboards(Action<(string name, string id)[]> callback)
    {
        IntPtr outArrayPtr = IntPtr.Zero;
        GetLeaderboardsForProjectCallbackDelegate myGetLeaderboardsForProjectCallbackDelegate = (isSuccess, context) =>
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

    // Cleanup
    private void ReleaseMemory(IntPtr ptr)
    {
        if (ptr != IntPtr.Zero)
        {
            FreeMemory(ptr);
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

