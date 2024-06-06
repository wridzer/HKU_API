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
                    <h3>Step 1: Importing the DLL and Wrapper</h3>
                    <ol>
                        <li>Place the <code>HKUApiWrapper.dll</code> file into the <code>Assets/Plugins</code> folder of your Unity project.</li>
                        <li>Ensure the <code>HKUApiWrapper.cs</code> script is also included in your project.</li>
                    </ol>
                </section>
                <section>
                    <h2 id="configuration">Configuration</h2>
                    <h3>Configure the Project</h3>
                    <p>Before calling any other functions, you need to configure the project with your Project ID.</p>
                    <pre><code>using HKU;
using System;
using UnityEngine;

public class Example : MonoBehaviour
{
    private void Start()
    {
        ConfigureProject("YourProjectID", OnConfigureProject);
    }

    private void ConfigureProject(string projectId, Action&lt;bool&gt; callback)
    {
        HKUApiWrapper.ConfigureProject(projectId, (isSuccess, context) =&gt;
        {
            callback(isSuccess);
        }, IntPtr.Zero);
    }

    private void OnConfigureProject(bool isSuccess)
    {
        if (isSuccess)
        {
            Debug.Log("Project configured successfully.");
        }
        else
        {
            Debug.LogError("Failed to configure project.");
        }
    }
}
          </code></pre>
                </section>

                <section>
                    <h2 id="authentication">User Authentication</h2>
                    <h3>Open Login Page</h3>
                    <p>To authenticate users, you need to open the login page in the browser.</p>
                    <pre><code>public void OpenLogin()
{
    HKUApiWrapper.OpenLoginPage();
}
          </code></pre>

                    <h3>Start Polling for Login Status</h3>
                    <pre><code>public void StartLoginPolling()
{
    HKUApiWrapper.StartPolling(OnLoginStatus, IntPtr.Zero);
}

private void OnLoginStatus(bool isSuccess, string userId, IntPtr context)
{
    if (isSuccess)
    {
        Debug.Log($"User logged in with ID: {userId}");
    }
    else
    {
        Debug.LogError("Login failed or cancelled.");
    }
}
          </code></pre>

                    <h3>Cancel Polling</h3>
                    <pre><code>public void CancelLoginPolling()
{
    HKUApiWrapper.CancelPolling();
}
          </code></pre>

                    <h3>Logout</h3>
                    <pre><code>public void Logout()
{
    HKUApiWrapper.Logout(OnLogout, IntPtr.Zero);
}

private void OnLogout(bool isSuccess, IntPtr context)
{
    if (isSuccess)
    {
        Debug.Log("User logged out successfully.");
    }
    else
    {
        Debug.LogError("Logout failed.");
    }
}
          </code></pre>
                </section>

                <section>
                    <h2 id="user-data">User Data</h2>
                    <h3>Get User Information</h3>
                    <pre><code>public void GetUser(string userId)
{
    HKUApiWrapper.GetUser(userId, OnGetUser, IntPtr.Zero);
}

private void OnGetUser(IntPtr username, int length, IntPtr context)
{
    string userName = Marshal.PtrToStringAnsi(username);
    Debug.Log($"Username: {userName}");
}
          </code></pre>

                    <h3>Get All Users</h3>
                    <pre><code>public void GetUsers()
{
    HKUApiWrapper.GetUsers(OnGetUsers, IntPtr.Zero);
}

private void OnGetUsers(IntPtr usersPtr, int length, IntPtr context)
{
    string[] users = new string[length];
    IntPtr[] userPtrs = new IntPtr[length];
    Marshal.Copy(usersPtr, userPtrs, 0, length);

    for (int i = 0; i &lt; length; i++)
    {
        users[i] = Marshal.PtrToStringAnsi(userPtrs[i]);
    }

    Debug.Log("Users: " + string.Join(", ", users));
}
          </code></pre>
                </section>

                <section>
                    <h2 id="leaderboards">Leaderboards</h2>
                    <h3>Get Leaderboards for Project</h3>
                    <pre><code>public void GetLeaderboardsForProject()
{
    HKUApiWrapper.GetLeaderboardsForProject(OnGetLeaderboards, IntPtr.Zero);
}

private void OnGetLeaderboards(bool isSuccess, IntPtr context)
{
    if (isSuccess)
    {
        Debug.Log("Leaderboards fetched successfully.");
    }
    else
    {
        Debug.LogError("Failed to fetch leaderboards.");
    }
}
          </code></pre>

                    <h3>Get Leaderboard Entries</h3>
                    <pre><code>public void GetLeaderboard(string leaderboardId, int amount, HKUApiWrapper.GetEntryOptions option)
{
    IntPtr outArray = IntPtr.Zero;
    HKUApiWrapper.GetLeaderboard(leaderboardId, ref outArray, amount, option, OnGetLeaderboard, IntPtr.Zero);
}

private void OnGetLeaderboard(bool isSuccess, IntPtr context)
{
    if (isSuccess)
    {
        Debug.Log("Leaderboard entries fetched successfully.");
    }
    else
    {
        Debug.LogError("Failed to fetch leaderboard entries.");
    }
}
          </code></pre>

                    <h3>Upload Leaderboard Score</h3>
                    <pre><code>public void UploadScore(string leaderboardId, int score)
{
    HKUApiWrapper.UploadLeaderboardScore(leaderboardId, score, OnUploadScore, IntPtr.Zero);
}

private void OnUploadScore(bool isSuccess, int currentRank, IntPtr context)
{
    if (isSuccess)
    {
        Debug.Log($"Score uploaded successfully! Current Rank: {currentRank}");
    }
    else
    {
        Debug.LogError("Failed to upload score.");
    }
}
          </code></pre>
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
}
          </code></pre>
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
        margin-left: 220px;
        padding: 20px;
        flex-grow: 1;
    }

    .guide {
        font-family: Arial, sans-serif;
        line-height: 1.6;
        color: #333;
        background-color: #f9f9f9;
        border-radius: 10px;
        padding: 20px;
        box-shadow: 0 0 10px rgba(0, 0, 0, 0.1);
    }

    h1, h2, h3 {
        color: #007acc;
    }

    pre {
        background: #eef;
        padding: 10px;
        border-radius: 5px;
        overflow: auto;
    }

    code {
        font-family: 'Courier New', Courier, monospace;
        background: #f4f4f4;
        padding: 2px 4px;
        border-radius: 3px;
    }

    ol {
        padding-left: 20px;
    }
</style>
