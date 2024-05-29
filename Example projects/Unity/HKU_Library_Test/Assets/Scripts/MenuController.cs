using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static HKU.HKUApiWrapper;

public class MenuController : MonoBehaviour
{
    [Header("Menu's")]
    [SerializeField] private TMP_Text UserInfo;
    [SerializeField] private TMP_Text loginButtonText;
    [SerializeField] private GameObject mainMenu;
    [SerializeField] private GameObject LeaderboardMenu;
    [Header("Leaderboard")]
    [SerializeField] private GameObject LeaderboardEntryPrefab;
    [SerializeField] private GameObject LeaderboardButtonPrefab;
    [SerializeField] private int amountOfEntries = 10;

    private HKU_Implementation hku;

    private void Start()
    {
        // Create HKU instance
        hku = new HKU_Implementation();
        hku.Initialize();

        // Check if user is logged in
        if (hku.isLoggedin)
        {
            UserInfo.text = "Logged in as: " + hku.userID;
            loginButtonText.text = "Logout";
        } else
        {
            UserInfo.text = "Not logged in";
            loginButtonText.text = "Login";
        }

        // Set menu's
        mainMenu.SetActive(true);
        LeaderboardMenu.SetActive(false);
    }

    public void Update()
    {
        UpdateLoginStatus();
    }

    // Account login/logout
    public void UpdateLoginStatus()
    {
        if (hku.isLoggedin)
        {
            UserInfo.text = "Logged in as: " + hku.userID;
            loginButtonText.text = "Logout";
        }
        else
        {
            UserInfo.text = "Not logged in";
            loginButtonText.text = "Login";
        }
    }
    
    public void OpenLogin()
    {
        if (hku.isLoggedin)
        {
            hku.Logoff();
        } else
        {
            hku.Login();
        }
    }

    // Leaderboard
    public void OpenLeaderboards()
    {
        mainMenu.SetActive(false);
        LeaderboardMenu.SetActive(true);
        PopulateLeaderboardButtons();
    }

    public void CloseLeaderboards()
    {
        mainMenu.SetActive(true);
        LeaderboardMenu.SetActive(false);
    }

    private void PopulateLeaderboardButtons()
    {
        // Clear leaderboard buttons
        foreach (Transform child in LeaderboardMenu.transform)
        {
            if (child.gameObject.tag == "LeaderboardObject")
            {
                Destroy(child.gameObject);
            }
        }

        // Get leaderboards
        hku.GetLeaderboards((leaderboards) =>
        {
            if (leaderboards != null)
            {
                foreach (string leaderboard in leaderboards)
                {
                    GameObject leaderboardButton = Instantiate(LeaderboardButtonPrefab, LeaderboardMenu.transform);
                    leaderboardButton.GetComponentInChildren<TMP_Text>().text = leaderboard;
                    leaderboardButton.GetComponent<Button>().onClick.AddListener(() => PopulateLeaderboard(leaderboard));
                }
            }
        });
    }

    private void PopulateLeaderboard(string leaderboard)
    {
        // Clear leaderboard entries
        foreach (Transform child in LeaderboardMenu.transform)
        {
            if (child.gameObject.tag == "LeaderboardObject")
            {
                Destroy(child.gameObject);
            }
        }

        // Get leaderboard entries
        hku.GetLeaderboardEntries(leaderboard, amountOfEntries, GetEntryOptions.Highest, (entries) =>
        {
            if (entries != null)
            {
                foreach (HKU.LeaderboardEntry entry in entries)
                {
                    Debug.Log($"Entry: {entry.rank} - {entry.username} - {entry.score}"); // Voeg debuginformatie toe
                    GameObject leaderboardEntry = Instantiate(LeaderboardEntryPrefab, LeaderboardMenu.transform);
                    leaderboardEntry.GetComponentInChildren<TMP_Text>().text = entry.rank + " - " + entry.username + " - " + entry.score;
                }
            }
            else
            {
                Debug.LogError("Failed to fetch leaderboard entries.");
            }
        });
    }


    public void StartGame()
    {

    }

    public void QuitGame()
    {
        Application.Quit();
    }

}