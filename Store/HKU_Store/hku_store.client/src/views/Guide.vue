<template>
    <div class="container">
        <div class="sidebar">
            <ul>
                <li><a href="#setup">Setup Unity Project</a></li>
                <li><a href="#import-scripts">Import Scripts</a></li>
                <li><a href="#configure-project">Configure Project</a></li>
                <li><a href="#login-logout">Implement Login and Logout</a></li>
                <li><a href="#leaderboards">Working with Leaderboards</a></li>
                <li><a href="#example-code">Example Code</a></li>
            </ul>
        </div>
        <div class="content">
            <section id="setup">
                <h2>Setup Unity Project</h2>
                <ol>
                    <li>
                        Download and Import the .dll File:
                        <ul>
                            <li>Provide users with the compiled .dll file.</li>
                            <li>Place the .dll file in the <code>Assets/Plugins</code> directory of your Unity project.</li>
                        </ul>
                    </li>
                </ol>
            </section>

            <section id="import-scripts">
                <h2>Import Scripts</h2>
                <ol>
                    <li>
                        Copy the Required Scripts:
                        <ul>
                            <li>
                                Ensure the following scripts are included in your project:
                                <ul>
                                    <li><code>HKUApiWrapper.cs</code></li>
                                    <li><code>HKU_Implementation.cs</code></li>
                                </ul>
                            </li>
                            <li>Place these scripts in the <code>Assets/Scripts</code> directory of your Unity project.</li>
                        </ul>
                    </li>
                </ol>
            </section>

            <section id="configure-project">
                <h2>Configure Project</h2>
                <ol>
                    <li>
                        Configure the Project:
                        <ul>
                            <li>Call the <code>ConfigureProject</code> method with your project ID to set up the project configuration.</li>
                        </ul>
                        <pre><code>HKU_Implementation hku = new HKU_Implementation();
hku.Initialize("YourProjectID");</code></pre>
                    </li>
                </ol>
            </section>

            <section id="login-logout">
                <h2>Implement Login and Logout</h2>
                <h3>Create Login</h3>
                <ol>
                    <li>
                        Call the Login Function:
                        <ul>
                            <li>Use the <code>Login</code> method to initiate the login process. This will open the login page.</li>
                        </ul>
                        <pre><code>hku.Login();</code></pre>
                    </li>
                    <li>
                        Start Polling:
                        <ul>
                            <li>After calling the login function, start polling to check if the user is logged in.</li>
                        </ul>
                        <pre><code>hku.StartPolling((isSuccess, userId) => {
    if (isSuccess) {
        Debug.Log("Logged in as user: " + userId);
    } else {
        Debug.Log("Login failed");
    }
});</code></pre>
                    </li>
                    <li>
                        Cancel Polling:
                        <ul>
                            <li>To cancel polling, call the <code>CancelPolling</code> function.</li>
                        </ul>
                        <pre><code>hku.CancelPolling();</code></pre>
                    </li>
                    <li>
                        Handle Login Callback:
                        <ul>
                            <li>You will receive a callback when the user is logged in, which will contain their user ID.</li>
                        </ul>
                    </li>
                </ol>

                <h3>Create Logout</h3>
                <ol>
                    <li>
                        Call the Logout Function:
                        <ul>
                            <li>Use the <code>Logoff</code> method to log out the current user.</li>
                        </ul>
                        <pre><code>hku.Logoff((isSuccess) => {
    if (isSuccess) {
        Debug.Log("Logged out successfully");
    } else {
        Debug.Log("Logout failed");
    }
});</code></pre>
                    </li>
                </ol>
            </section>

            <section id="leaderboards">
                <h2>Working with Leaderboards</h2>
                <h3>Upload Scores</h3>
                <ol>
                    <li>
                        Upload Score to Leaderboard:
                        <ul>
                            <li>Use the <code>UploadScore</code> method to upload a score to a specific leaderboard.</li>
                        </ul>
                        <pre><code>hku.UploadScore("LeaderboardID", score, (isSuccess, rank) => {
    if (isSuccess) {
        Debug.Log("Score uploaded successfully, current rank: " + rank);
    } else {
        Debug.Log("Failed to upload score");
    }
});</code></pre>
                    </li>
                </ol>

                <h3>Fetch and Display Leaderboards</h3>
                <ol>
                    <li>
                        Fetch Leaderboards:
                        <ul>
                            <li>Use the <code>GetLeaderboards</code> method to fetch the list of leaderboards for the project.</li>
                        </ul>
                        <pre><code>hku.GetLeaderboards((leaderboards) => {
    if (leaderboards != null) {
        foreach (var leaderboard in leaderboards) {
            Debug.Log("Leaderboard: " + leaderboard.name + " - ID: " + leaderboard.id);
        }
    } else {
        Debug.Log("Failed to fetch leaderboards");
    }
});</code></pre>
                    </li>
                    <li>
                        Fetch Leaderboard Entries:
                        <ul>
                            <li>Use the <code>GetLeaderboardEntries</code> method to fetch entries for a specific leaderboard.</li>
                        </ul>
                        <pre><code>hku.GetLeaderboardEntries("LeaderboardID", 10, HKU.HKUApiWrapper.GetEntryOptions.Highest, (entries) => {
    if (entries != null) {
        foreach (var entry in entries) {
            Debug.Log("Rank: " + entry.Rank + ", PlayerID: " + entry.PlayerID + ", Score: " + entry.Score);
        }
    } else {
        Debug.Log("Failed to fetch leaderboard entries");
    }
});</code></pre>
                    </li>
                </ol>
            </section>

            <section id="example-code">
                <h2>Example Code</h2>
                <p>Here is an example of how a Unity MonoBehaviour script might look, integrating your .dll functionalities:</p>
                <pre><code>using UnityEngine;
using HKU;

public class ExampleIntegration : MonoBehaviour
{
    private HKU_Implementation hku;

    void Start()
    {
        // Initialize HKU implementation with your project ID
        hku = new HKU_Implementation();
        hku.Initialize("YourProjectID");
    }

    public void OnLoginButtonClicked()
    {
        // Start the login process
        hku.Login();
        hku.StartPolling((isSuccess, userId) => {
            if (isSuccess)
            {
                Debug.Log("Logged in as user: " + userId);
                // Update your UI or game state accordingly
            }
            else
            {
                Debug.Log("Login failed");
            }
        });
    }

    public void OnCancelPollingButtonClicked()
    {
        // Cancel the polling process
        hku.CancelPolling();
    }

    public void OnLogoutButtonClicked()
    {
        // Log out the current user
        hku.Logoff((isSuccess) => {
            if (isSuccess)
            {
                Debug.Log("Logged out successfully");
                // Update your UI or game state accordingly
            }
            else
            {
                Debug.Log("Logout failed");
            }
        });
    }

    public void OnUploadScoreButtonClicked(int score)
    {
        // Upload a score to a specific leaderboard
        hku.UploadScore("LeaderboardID", score, (isSuccess, rank) => {
            if (isSuccess)
            {
                Debug.Log("Score uploaded successfully, current rank: " + rank);
                // Update your UI or game state accordingly
            }
            else
            {
                Debug.Log("Failed to upload score");
            }
        });
    }

    public void OnFetchLeaderboardsButtonClicked()
    {
        // Fetch the list of leaderboards for the project
        hku.GetLeaderboards((leaderboards) => {
            if (leaderboards != null)
            {
                foreach (var leaderboard in leaderboards)
                {
                    Debug.Log("Leaderboard: " + leaderboard.name + " - ID: " + leaderboard.id);
                }
            }
            else
            {
                Debug.Log("Failed to fetch leaderboards");
            }
        });
    }

    public void OnFetchLeaderboardEntriesButtonClicked()
    {
        // Fetch entries for a specific leaderboard
        hku.GetLeaderboardEntries("LeaderboardID", 10, HKUApiWrapper.GetEntryOptions.Highest, (entries) => {
            if (entries != null)
            {
                foreach (var entry in entries)
                {
                    Debug.Log("Rank: " + entry.Rank + ", PlayerID: " + entry.PlayerID + ", Score: " + entry.Score);
                }
            }
            else
            {
                Debug.Log("Failed to fetch leaderboard entries");
            }
        });
    }
}</code></pre>
            </section>
        </div>
    </div>
</template>

<script>
    export default {
        name: 'IntegrationGuide'
    }
</script>

<style scoped>
    .container {
        display: flex;
        height: 100vh;
        width: 100%;
    }

    .sidebar {
        width: 250px;
        padding: 20px;
        background-color: #333;
        color: #fff;
        border-right: 1px solid #ddd;
        position: fixed;
        height: 100%;
        overflow-y: auto;
    }

        .sidebar ul {
            list-style: none;
            padding: 0;
        }

            .sidebar ul li {
                margin-bottom: 10px;
            }

                .sidebar ul li a {
                    text-decoration: none;
                    color: #fff;
                }

                    .sidebar ul li a:hover {
                        text-decoration: underline;
                    }

    .content {
        flex: 1;
        margin-left: 250px;
        padding: 20px;
        overflow-y: auto;
        background-color: #f5f5f5;
        height: 100vh;
        color: #333;
    }

        .content ul li{
            color: #333;
        }

    section {
        margin-bottom: 40px;
    }

    h2, h3 {
        color: #333;
    }

    code {
        background-color: #e8e8e8;
        padding: 5px;
        border-radius: 3px;
        color: #000;
    }

    pre {
        background-color: #e8e8e8;
        padding: 10px;
        border-radius: 3px;
        overflow-x: auto;
        white-space: pre-wrap;
        word-wrap: break-word;
    }
</style>
