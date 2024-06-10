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
                        <li>
                            Create an Account
                            <p>Register for an account on the necessary platform.</p>
                        </li>
                        <li>
                            Create a Project
                            <p>Set up a new project on the platform and obtain the `Project ID`.</p>
                        </li>
                        <li>
                            Configure Project
                            <p>Ensure your project settings are configured correctly on the platform.</p>
                        </li>
                        <li>
                            Download Files
                            <p>Download the necessary files, including the DLL and the `HKUApiWrapper.cs`.</p>
                        </li>
                        <li>
                            Add .dll to Plugins
                            <p>Place the DLL file into the `Plugins` folder within your Unity project.</p>
                        </li>
                        <li>
                            Add Wrapper to Project
                            <p>Include the `HKUApiWrapper.cs` script in your Unity project.</p>
                        </li>
                    </ol>
                </section>

                <section>
                    <h2 id="configuration">Configuration</h2>
                    <h3>Configure the Project</h3>
                    <p>Next, configure the project so the DLL knows which project this is. Update `YOU_PROJECT_ID_HERE` with your actual project ID.</p>
                    <pre><code>using static HKU.HKUApiWrapper;
using System;
using System.Runtime.InteropServices;
using UnityEngine;

public class HKU_Implementation : MonoBehaviour
{
    private GCHandle gch;
    private IntPtr contextPtr = IntPtr.Zero;
    private bool isConfigured = false;
    private string projectID = "YOU_PROJECT_ID_HERE";

    public void Initialize()
    {
        // Context for callbacks
        gch = GCHandle.Alloc(this);
        contextPtr = GCHandle.ToIntPtr(gch);

        // Set debug output
#if DEVELOPMENT_BUILD
        DebugOutputDelegate myDebugOutputDelegate = (message, context) =>
        {
            Debug.Log(message);
        };
        SetOutputCallback(myDebugOutputDelegate, IntPtr.Zero);
#endif

        // Register project
        ConfigureProjectCallbackDelegate myConfigureProjectCallbackDelegate = (bool IsSuccess, IntPtr context) =>
        {
            if (IsSuccess)
            {
                Debug.Log("Project configured successfully");
                isConfigured = true;
            }
            else
            {
                Debug.Log("Project configuration failed");
            }
        };
        ConfigureProject(projectID, myConfigureProjectCallbackDelegate, IntPtr.Zero);
    }
}
</code></pre>
                </section>

                <section>
                    <h2 id="authentication">User Authentication</h2>
                    <h3>Login Process</h3>
                    <p>The login process involves opening the login page, starting the polling process, and optionally stopping the polling process.</p>
                    <pre><code>using static HKU.HKUApiWrapper;
using System;
using System.Runtime.InteropServices;
using UnityEngine;

public class HKU_Implementation : MonoBehaviour
{
    private GCHandle gch;
    private IntPtr contextPtr = IntPtr.Zero;
    private bool isConfigured = false;
    private string projectID = "YOU_PROJECT_ID_HERE";

    public void Initialize()
    {
        // Context for callbacks
        gch = GCHandle.Alloc(this);
        contextPtr = GCHandle.ToIntPtr(gch);

        // Set debug output
#if DEVELOPMENT_BUILD
        DebugOutputDelegate myDebugOutputDelegate = (message, context) =>
        {
            Debug.Log(message);
        };
        SetOutputCallback(myDebugOutputDelegate, IntPtr.Zero);
#endif

        // Register project
        ConfigureProjectCallbackDelegate myConfigureProjectCallbackDelegate = (bool IsSuccess, IntPtr context) =>
        {
            if (IsSuccess)
            {
                Debug.Log("Project configured successfully");
                isConfigured = true;
            }
            else
            {
                Debug.Log("Project configuration failed");
            }
        };
        ConfigureProject(projectID, myConfigureProjectCallbackDelegate, IntPtr.Zero);
    }

    public void StartLoginProcess()
    {
        if (!isConfigured)
        {
            Debug.LogError("Project is not configured. Please configure the project before starting the login process.");
            return;
        }

        // Open login page
        OpenLoginPage();

        // Start polling
        StartPollingProcess();
    }

    private void StartPollingProcess()
    {
        LoginStatusCallbackDelegate myLoginStatusCallbackDelegate = (bool IsSuccess, string userId, IntPtr context) =>
        {
            if (IsSuccess)
            {
                Debug.Log("User logged in successfully with ID: " + userId);
            }
            else
            {
                Debug.Log("User login failed");
            }
        };
        StartPolling(myLoginStatusCallbackDelegate, contextPtr);
    }

    public void StopPollingProcess()
    {
        CancelPolling();
        Debug.Log("Polling stopped");
    }

    private void OnDestroy()
    {
        if (gch.IsAllocated)
        {
            gch.Free();
        }
    }
}</code></pre>
                </section>

                <section>
                    <h2 id="user-data">User Data</h2>
                    <h3>Fetching Users</h3>
                    <pre><code>public void FetchUsers()
{
    UsersCallbackDelegate myUsersCallbackDelegate = (IntPtr users, int length, IntPtr context) =>
    {
        for (int i = 0; i < length; i++)
        {
            IntPtr userPtr = Marshal.ReadIntPtr(users, i * IntPtr.Size);
            string user = Marshal.PtrToStringAnsi &#8203;:citation[oaicite:0]{index=0}&#8203;
                                    (userPtr);
            Debug.Log("User: " + user);
        }
    };
    GetUsers(myUsersCallbackDelegate, contextPtr);
}</code></pre>
                </section>

                <section>
                    <h2 id="leaderboards">Leaderboards</h2>
                    <h3>Uploading Leaderboard Scores</h3>
                    <pre><code>public void UploadScore(string leaderboardId, int score)
{
    UploadLeaderboardScoreCallbackDelegate myUploadScoreCallbackDelegate = (bool IsSuccess, int currentRank, IntPtr context) =>
    {
        if (IsSuccess)
        {
            Debug.Log("Score uploaded successfully. Current Rank: " + currentRank);
        }
        else
        {
            Debug.Log("Failed to upload score");
        }
    };
    UploadLeaderboardScore(leaderboardId, score, myUploadScoreCallbackDelegate, contextPtr);
}</code></pre>

                    <h3>Fetching Leaderboard</h3>
                    <pre><code>public void FetchLeaderboard(string leaderboardId, int amount, GetEntryOptions option)
{
    LeaderboardCallbackDelegate myLeaderboardCallbackDelegate = (bool isSuccess, IntPtr context) =>
    {
        if (isSuccess)
        {
            Debug.Log("Leaderboard fetched successfully");
        }
        else
        {
            Debug.Log("Failed to fetch leaderboard");
        }
    };

    IntPtr outArray = IntPtr.Zero;
    GetLeaderboard(leaderboardId, ref outArray, amount, option, myLeaderboardCallbackDelegate, contextPtr);
    // Don't forget to free the memory
    FreeMemory(outArray);
}</code></pre>
                </section>

                <section>
                    <h2 id="debugging">Debugging</h2>
                    <h3>Set Debug Output Callback</h3>
                    <pre><code>public void SetDebugOutput()
{
    HKUApiWrapper.SetOutputCallback(OnDebugOutput, IntPtr.Zero);
}

private void OnDebugOutput(string message, IntPtr context)
{
    Debug.Log($"Debug: {message}");
}</code></pre>
                </section>

                <section>
                    <h2>Full Example Class</h2>
                    <pre><code>using static HKU.HKUApiWrapper;
using System;
using System.Runtime.InteropServices;
using UnityEngine;

public class HKU_Implementation : MonoBehaviour
{
    private GCHandle gch;
    private IntPtr contextPtr = IntPtr.Zero;
    private bool isConfigured = false;
    private string projectID = "YOU_PROJECT_ID_HERE";

    public void Initialize()
    {
        // Context for callbacks
        gch = GCHandle.Alloc(this);
        contextPtr = GCHandle.ToIntPtr(gch);

        // Set debug output
#if DEVELOPMENT_BUILD
        DebugOutputDelegate myDebugOutputDelegate = (message, context) =>
        {
            Debug.Log(message);
        };
        SetOutputCallback(myDebugOutputDelegate, IntPtr.Zero);
#endif

        // Register project
        ConfigureProjectCallbackDelegate myConfigureProjectCallbackDelegate = (bool IsSuccess, IntPtr context) =>
        {
            if (IsSuccess)
            {
                Debug.Log("Project configured successfully");
                isConfigured = true;
            }
            else
            {
                Debug.Log("Project configuration failed");
            }
        };
        ConfigureProject(projectID, myConfigureProjectCallbackDelegate, IntPtr.Zero);
    }

    public void StartLoginProcess()
    {
        if (!isConfigured)
        {
            Debug.LogError("Project is not configured. Please configure the project before starting the login process.");
            return;
        }

        // Open login page
        OpenLoginPage();

        // Start polling
        StartPollingProcess();
    }

    private void StartPollingProcess()
    {
        LoginStatusCallbackDelegate myLoginStatusCallbackDelegate = (bool IsSuccess, string userId, IntPtr context) =>
        {
            if (IsSuccess)
            {
                Debug.Log("User logged in successfully with ID: " + userId);
            }
            else
            {
                Debug.Log("User login failed");
            }
        };
        StartPolling(myLoginStatusCallbackDelegate, contextPtr);
    }

    public void StopPollingProcess()
    {
        CancelPolling();
        Debug.Log("Polling stopped");
    }

    public void FetchUsers()
    {
        UsersCallbackDelegate myUsersCallbackDelegate = (IntPtr users, int length, IntPtr context) =>
        {
            for (int i = 0; i < length; i++)
            {
                IntPtr userPtr = Marshal.ReadIntPtr(users, i * IntPtr.Size);
                string user = Marshal.PtrToStringAnsi(userPtr);
                Debug.Log("User: " + user);
            }
        };
        GetUsers(myUsersCallbackDelegate, contextPtr);
    }

    public void UploadScore(string leaderboardId, int score)
    {
        UploadLeaderboardScoreCallbackDelegate myUploadScoreCallbackDelegate = (bool IsSuccess, int currentRank, IntPtr context) =>
        {
            if (IsSuccess)
            {
                Debug.Log("Score uploaded successfully. Current Rank: " + currentRank);
            }
            else
            {
                Debug.Log("Failed to upload score");
            }
        };
        UploadLeaderboardScore(leaderboardId, score, myUploadScoreCallbackDelegate, contextPtr);
    }

    public void FetchLeaderboard(string leaderboardId, int amount, GetEntryOptions option)
    {
        LeaderboardCallbackDelegate myLeaderboardCallbackDelegate = (bool isSuccess, IntPtr context) =>
        {
            if (isSuccess)
            {
                Debug.Log("Leaderboard fetched successfully");
            }
            else
            {
                Debug.Log("Failed to fetch leaderboard");
            }
        };

        IntPtr outArray = IntPtr.Zero;
        GetLeaderboard(leaderboardId, ref outArray, amount, option, myLeaderboardCallbackDelegate, contextPtr);
        // Don't forget to free the memory
        FreeMemory(outArray);
    }

    private void OnDestroy()
    {
        if (gch.IsAllocated)
        {
            gch.Free();
        }
    }
}</code></pre>
                </section>
            </div>
            <!-- Your existing content ends here -->
        </div>
    </div>
</template>

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
