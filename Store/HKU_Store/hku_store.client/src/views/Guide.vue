<template>
    <div class="container">
        <aside class="sidebar">
            <nav>
                <ul>
                    <li><a href="#setup">Setup</a></li>
                    <li><a href="#configuration">Configuration</a></li>
                    <li><a href="#authentication">User Authentication</a></li>
                    <li><a href="#user-data">User Data</a></li>
                    <li><a href="#leaderboards">Leaderboards</a></li>
                    <li><a href="#debugging">Debugging</a></li>
                </ul>
            </nav>
        </aside>
        <div class="guide-content">
            <!-- Your existing content starts here -->
            <div class="guide">
                <h1 id="setup">Using the HKU API Wrapper in Unity</h1>
                <section>
                    <h2>Setup</h2>
                    <h3>Step-by-Step Guide</h3>
                    <ol>
                        <li>Create Account</li>
                        <li>Create Project</li>
                        <li>Configure Project</li>
                        <li>Download Files</li>
                        <li>Add .dll to Plugins</li>
                        <li>Add Wrapper to project</li>
                    </ol>
                </section>

                <section>
                    <h2>Before We Start</h2>
                    <p>There are a few concepts I'd like to explain before we start. The DLL makes use of callbacks. That means that you will send a function as a parameter. When the DLL is done it will call the function you've passed along.</p>
                    <p>It will also need context, this is the object the function will be called on. You could have multiple enemies in a game for instance so there needs to be a way of telling on which one the callback function should be called.</p>
                </section>

                <section>
                    <h2>Initialization</h2>
                    <p>To start we will make a simple class that will handle most of the function calls and will just print the information to the console. Because we'll just be using this class we can just send the keyword "this" as the context. However, this will first need to be cast to a void pointer because that is what the DLL expects.</p>
                    <pre><code>
using static HKU.HKUApiWrapper;

public class HKUImplementation
{
    private GCHandle gch &#8203;:citation[oaicite:0]{index=0}&#8203;
                            private IntPtr contextPtr = IntPtr.Zero;

    public void Initialize()
    {
        gch = GCHandle.Alloc(this);
        contextPtr = GCHandle.ToIntPtr(gch);
    }
}
                    </code></pre>
                </section>

                <section>
                    <h2>Configure Debug Output</h2>
                    <p>This will make sure that you can receive feedback from the DLL. This will be useful for configuration and debugging. You can see I am using a so-called lambda function that I pass along to the function call. This will now make sure that everything will be printed in the Unity console.</p>
                    <pre><code>
public void Initialize()
{
    gch = GCHandle.Alloc(this);
    contextPtr = GCHandle.ToIntPtr(gch);

    // Set debug output
    DebugOutputDelegate myDebugOutputDelegate = (message, context) =>
    {
        Debug.Log(message);
    };
    SetOutputCallback(myDebugOutputDelegate, IntPtr.Zero);
}
                    </code></pre>
                </section>

                <section>
                    <h2>Configure Project</h2>
                    <p>Next, we will need to configure the project so the DLL knows what project this is.</p>
                    <pre><code>
bool isConfigured = false;
string projectID = "YOUR_PROJECT_ID_HERE";

public void Initialize()
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
                    </code></pre>
                    <p>Try and run this to see if it works. If so, you should see "Project configured successfully" in the console on startup.</p>
                </section>

                <section>
                    <h2>Login</h2>
                    <p>Next we will create the Login. For the login, it is important that you first open the login page and then start polling for a result. This will look something like this:</p>
                    <pre><code>
public void Login()
{
    // Open login page
    OpenLoginPage();

    // Poll login status
    LoginStatusCallbackDelegate myLoginStatusCallbackDelegate = (IsSuccess, Id, context) =>
    {
        if (IsSuccess)
        {
            Debug.Log("Login successful with user: " + Id);
        }
        else
        {
            Debug.Log("Login failed");
        }
    };
    StartPolling(myLoginStatusCallbackDelegate, contextPtr);
}
                    </code></pre>
                    <p>If the user would decide not to log in, you should call CancelPolling(). If the user wants to log out, you can do so like this:</p>
                    <pre><code>
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
                    </code></pre>
                </section>

                <section>
                    <h2>Get Username</h2>
                    <p>You can now test to see if the login works by adding a button or using a context menu if you are familiar with them. If it does, it should print the ID of the user. This is the way users are handled in the backend, but to the user, you might want to show a username. To do so, create the following function and call it when you receive the ID.</p>
                    <pre><code>
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

// In the login callback
Debug.Log("Login successful with user: " + GetUsername(Id));
                    </code></pre>
                    <p>Now it should print the username after login instead of the ID.</p>
                </section>

                <section>
                    <h2>Leaderboards</h2>
                    <p>Now that we got the login stuff handled, we move on to the leaderboards. The two main things are of course uploading the score to the leaderboard and fetching it to see the scores. After that, I'll also show how to fetch all the existing leaderboards so you can fetch them dynamically.</p>
                    <pre><code>
public void UploadScore(string leaderboard, int score)
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
    UploadLeaderboardScore(leaderboard, score, myUploadLeaderboardScoreCallbackDelegate, contextPtr);
}
                    </code></pre>
                    <p>You will need two functions to get the leaderboard entries: one to get them and the <code>MarshalPtrToLeaderboardEntryArray</code> to cast them to actual entries.</p>
                    <pre><code>
public void GetLeaderboardEntries(string leaderboard, int amount, GetEntryOptions option, Action&lt;HKU.LeaderboardEntry[]> callback)
{
    IntPtr outArrayPtr = IntPtr.Zero;
    LeaderboardCallbackDelegate myGetLeaderboardCallbackDelegate = (isSuccess, context) =>
    {
        if (isSuccess)
        {
            // Parse outArrayPtr to LeaderboardEntry array and invoke the callback
            HKU.LeaderboardEntry[] entries = MarshalPtrToLeaderboardEntryArray(outArrayPtr);
            callback(entries);
        }
        else
        {
            callback(null);
        }
    };
    GetLeaderboard(leaderboard, ref outArrayPtr, amount, option, myGetLeaderboardCallbackDelegate, contextPtr);
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
                    </code></pre>
                    <p>The fetching of the leaderboards is similar to the fetching of the entries.</p>
                    <pre><code>
public void GetLeaderboards(Action&lt;(string name, string id)[]> callback)
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
                    </code></pre>
                </section>

                <section>
                    <h2>Full Script</h2>
                    <p>Your full script should now look something like this:</p>
                    <pre><code>
using System;
using UnityEngine;
using System.Runtime.InteropServices;
using static HKU.HKUApiWrapper;

public class HKUImplementation
{
    private GCHandle gch;
    private IntPtr contextPtr = IntPtr.Zero;
    bool isConfigured = false;
    string projectID = "YOU_PROJECT_ID_HERE";

    public void Initialize()
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

    public void UploadScore(string leaderboard, int score)
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
        UploadLeaderboardScore(leaderboard, score, myUploadLeaderboardScoreCallbackDelegate, contextPtr);
    }

    public void GetLeaderboardEntries(string leaderboard, int amount, GetEntryOptions option, Action&lt;HKU.LeaderboardEntry[]> callback)
    {
        IntPtr outArrayPtr = IntPtr.Zero;
        LeaderboardCallbackDelegate myGetLeaderboardCallbackDelegate = (isSuccess, context) =>
        {
            if (isSuccess)
            {
                // Parse outArrayPtr to LeaderboardEntry array and invoke the callback
                HKU.LeaderboardEntry[] entries = MarshalPtrToLeaderboardEntryArray(outArrayPtr);
                callback(entries);
            }
            else
            {
                callback(null);
            }
        };
        GetLeaderboard(leaderboard, ref outArrayPtr, amount, option, myGetLeaderboardCallbackDelegate, contextPtr);
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
}
                    </code></pre>
                    <p>Feel free to change stuff and make it fit your project!</p>
                </section>
            </div>
        </div>
    </div>
</template>

<script>
    export default {
        name: 'Guide'
    }
</script>

<style scoped>
    .container {
        display: flow;
    }

    .sidebar {
        width: 200px;
        background-color: #333;
        color: #fff;
        padding: 20px;
        height: 100vh;
        position: fixed;
    }

        .sidebar nav ul {
            list-style: none;
            padding: 0;
        }

            .sidebar nav ul li {
                margin: 10px 0;
            }

                .sidebar nav ul li a {
                    color: #fff;
                    text-decoration: none;
                }

                    .sidebar nav ul li a:hover {
                        text-decoration: underline;
                    }

    .guide-content {
        margin-left: 180px;
        padding: 20px;
        flex-grow: 1;
    }

    .guide {
        font-family: Arial, sans-serif;
        line-height: 1.6;
        color: #f9f9f9;
        background-color: #1e1e1e;
        border-radius: 10px;
        padding: 20px;
        box-shadow: 0 0 10px rgba(0, 0, 0, 0.1);
    }

    h1 {
        color: #fe1f4c;
    }

    h2 {
        color: #007acc;
    }

    h3 {
        color: #3093e5;
    }

    pre {
        background: #333333;
        padding: 10px;
        border-radius: 5px;
        overflow: auto;
    }

    code {
        font-family: 'Courier New', Courier, monospace;
        padding: 2px 4px;
        border-radius: 3px;
    }

    ol {
        padding-left: 20px;
    }
</style>
