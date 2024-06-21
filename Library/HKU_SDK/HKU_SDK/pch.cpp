// pch.cpp: source file corresponding to the pre-compiled header

#include "pch.h"
#include <iostream>
#include <cstring>
#include <sstream>
#include <thread>
#include <chrono>
#define CURL_STATICLIB
#include <curl/curl.h>
#include <nlohmann/json.hpp>
#include "httplib.h"

// Debug output callback
static void (*outputCallback)(const char* message, void* context) = nullptr;
static void* callbackContext = nullptr;

static size_t WriteCallback(void* contents, size_t size, size_t nmemb, std::string* data) {
    size_t realSize = size * nmemb;
    data->append((char*)contents, realSize);
    return realSize;
}

void GetUsers(void (*callback)(char** users, int length, void* context), void* context)
{
    char** users = nullptr;
    int numberOfUsers = 0;
    CURL* curl = curl_easy_init();
    std::string readBuffer;

    if (curl) {
        curl_easy_setopt(curl, CURLOPT_URL, "http://pong.hku.nl:5037/api/users");
        curl_easy_setopt(curl, CURLOPT_WRITEFUNCTION, WriteCallback);
        curl_easy_setopt(curl, CURLOPT_WRITEDATA, &readBuffer);
        CURLcode res = curl_easy_perform(curl);
        if (res != CURLE_OK) {
            SendMessageToCallback("curl_easy_perform() failed: %s", curl_easy_strerror(res));
        } else {
            try {
                auto jsonUsers = nlohmann::json::parse(readBuffer);
                numberOfUsers = jsonUsers.size();
                users = new char* [numberOfUsers];

                int i = 0;
                for (const auto& user : jsonUsers) {
                    // Zorg ervoor dat je alleen de userName eigenschap krijgt.
                    if (!user["userName"].is_string()) {
                        SendMessageToCallback("User name is not a string at index %s", i);
                        users[i] = nullptr; // Gebruik nullptr om aan te geven dat het ophalen niet gelukt is
                        continue;
                    }

                    std::string userName = user["userName"];
                    users[i] = new char[userName.length() + 1];
                    strcpy_s(users[i], userName.length() + 1, userName.c_str());
                    i++;
                }
            }
            catch (nlohmann::json::parse_error& pe) {
                // Dit vangt parse errors
                SendMessageToCallback("JSON parse error: %s", pe.what());
            }
            catch (nlohmann::json::exception& e) {
                // Dit vangt alle andere nlohmann::json gerelateerde exceptions
                SendMessageToCallback("JSON exception: %s", e.what());
            }
        }
        curl_easy_cleanup(curl);
    }

    // Roep de callback aan met de array en het aantal gebruikers.
    callback(const_cast<char**>(users), numberOfUsers, context);

    // Vrijgeven van het geheugen
    for (int i = 0; i < numberOfUsers; i++) {
        delete[] users[i]; // Vrijgeven van elke string
    }
    //delete[] users; // Vrijgeven van de array
}

void ConfigureProject(const char* project_ID, void(*callback)(bool IsSuccess, void* context), void* context)
{
    CURL* curl;
    CURLcode res;
    std::string readBuffer;

    // URL to fetch the project by ID
    std::string checkUrl = "http://pong.hku.nl:5037/api/projects/" + std::string(project_ID);

    curl = curl_easy_init();
    if (curl) {
        curl_easy_setopt(curl, CURLOPT_URL, checkUrl.c_str());
        curl_easy_setopt(curl, CURLOPT_WRITEFUNCTION, WriteCallback);
        curl_easy_setopt(curl, CURLOPT_WRITEDATA, &readBuffer);

        res = curl_easy_perform(curl);
        if (res != CURLE_OK) {
            SendMessageToCallback("curl_easy_perform() failed: %s", curl_easy_strerror(res));
            callback(false, context);
            curl_easy_cleanup(curl);
            return;
        }

        try {
            auto responseJson = nlohmann::json::parse(readBuffer);
            if (responseJson.is_null()) {
                SendMessageToCallback("Project does not exist.");
                callback(false, context);
                curl_easy_cleanup(curl);
                return;
            }

            // Save the project ID locally
			projectID = project_ID;

            SendMessageToCallback("Project ID saved locally: %s", projectID.c_str());
            callback(true, context);

        }
        catch (nlohmann::json::exception& e) {
            SendMessageToCallback("JSON parsing error: %s", e.what());
            callback(false, context);
        }

        curl_easy_cleanup(curl);
    }
    else {
        SendMessageToCallback("Failed to initialize CURL.");
        callback(false, context);
    }
}

void OpenLoginPage() {
    const char* url = "http://pong.hku.nl:5037/LoginUser?dll=true"; 
#ifdef _WIN32
    std::string command = "start " + std::string(url);
#elif __APPLE__
    std::string command = "open " + std::string(url);
#elif __linux__
    std::string command = "xdg-open " + std::string(url);
#endif
    system(command.c_str());
}

void PollLoginStatus(void(*callback)(bool IsSuccess, const char* userId, void* context), void* context) {
    pollingActive.store(true);
    httplib::Server svr;

    svr.Get("/callback", [&](const httplib::Request& req, httplib::Response& res) {
        if (req.has_param("status") && req.get_param_value("status") == "success") {
            std::string userId = req.get_param_value("user_id");
            currentUser = userId;
            callback(true, userId.c_str(), context);
            pollingActive.store(false);
            svr.stop();
        }
        res.set_content("Login status received. You can close this window.", "text/plain");
        });

    std::thread server_thread([&]() {
        if (!svr.listen("localhost", 8080)) {
            SendMessageToCallback("Error starting server on port 8080");
            pollingActive.store(false);
            callback(false, nullptr, context);
        }
        });

    // Wait until polling is complete
    while (pollingActive.load()) {
        std::this_thread::sleep_for(std::chrono::seconds(1));
    }

    if (server_thread.joinable()) {
        server_thread.join();
    }
}

void StartPolling(void(*callback)(bool IsSuccess, const char* userId, void* context), void* context)
{
    std::thread(PollLoginStatus, callback, context).detach();
}

void CancelPolling() {
    pollingActive.store(false);
}

void Logout(void(*callback)(bool IsSuccess, void* context), void* context)
{
	CURL* curl;
	CURLcode res;
	struct curl_slist* headers = NULL;
	std::string readBuffer;

	curl = curl_easy_init();
	if (curl) {
		curl_easy_setopt(curl, CURLOPT_URL, "http://pong.hku.nl:5037/api/users/logout");
		curl_easy_setopt(curl, CURLOPT_WRITEFUNCTION, WriteCallback);
		curl_easy_setopt(curl, CURLOPT_WRITEDATA, &readBuffer);

		res = curl_easy_perform(curl);
		if (res == CURLE_OK) {
			try {
				auto responseJson = nlohmann::json::parse(readBuffer);
				bool success = responseJson["Success"];
                SendMessageToCallback("Request performed successfully! Success: %s", success);
				callback(success, context);
			}
			catch (nlohmann::json::exception& e) {
				SendMessageToCallback("JSON parsing error: %s", e.what());
				callback(false, context);
			}
		}
		else {
			SendMessageToCallback("curl_easy_perform() failed: %s", curl_easy_strerror(res));
			callback(false, context);
		}

		curl_easy_cleanup(curl);
	}
	else {
		SendMessageToCallback("Failed to initialize CURL.");
		callback(false, context);
	}
}

void GetUser(char* user_ID, void(*callback)(char* username, int length, void* context), void* context)
{
	CURL* curl;
	CURLcode res;
	struct curl_slist* headers = NULL;
	std::string readBuffer; // Buffer to hold response data

	// URL construction
	std::string url = "http://pong.hku.nl:5037/api/users/by-id/" + std::string(user_ID);

	curl = curl_easy_init();
	if (curl) {
		curl_easy_setopt(curl, CURLOPT_URL, url.c_str());
		headers = curl_slist_append(headers, "Content-Type: application/json");
		curl_easy_setopt(curl, CURLOPT_HTTPHEADER, headers);
		curl_easy_setopt(curl, CURLOPT_WRITEDATA, &readBuffer);
		curl_easy_setopt(curl, CURLOPT_WRITEFUNCTION, WriteCallback);

		res = curl_easy_perform(curl);
		if (res == CURLE_OK) {
			try {
				auto responseJson = nlohmann::json::parse(readBuffer);
				std::string username = responseJson["userName"];
                //SendMessageToCallback("User request performed successfully! Username: %s", username);
				char* usernameCopy = new char[username.length() + 1];
				strcpy_s(usernameCopy, username.length() + 1, username.c_str());
				callback(usernameCopy, 1, context);
			}
			catch (nlohmann::json::exception& e) {
				SendMessageToCallback("JSON parsing error: %s", e.what());
				callback(nullptr, 0, context);
			}
		}
		else {
			SendMessageToCallback("curl_easy_perform() failed: %s", curl_easy_strerror(res));
			callback(nullptr, 0, context);
		}

		curl_easy_cleanup(curl);
		curl_slist_free_all(headers);
	}
	else {
		SendMessageToCallback("Failed to initialize CURL.");
		callback(nullptr, 0, context);
	}
}

void GetLeaderboard(char* leaderboard_Id, char**& outArray, int amount, GetEntryOptions option, void (*callback)(bool IsSuccess, void* context), void* context)
{
    CURL* curl;
    CURLcode res;
    std::string readBuffer;

    std::string url = "http://pong.hku.nl:5037/api/leaderboards/entries/" + std::string(leaderboard_Id) + "?amount=" + std::to_string(amount);
    SendMessageToCallback("Constructed URL: %s", url.c_str());

    switch (option) {
    case GetEntryOptions::Highest:
        url += "&option=Highest";
        break;
    case GetEntryOptions::AroundMe:
        if (currentUser.empty()) {
            SendMessageToCallback("Current user is not set for AroundMe option.");
            callback(false, context);
            return;
        }
        url += "&option=AroundMe&playerId=" + currentUser;
        break;
    case GetEntryOptions::AtRank:
        url += "&option=AtRank";
        break;
    case GetEntryOptions::Friends:
        url += "&option=Friends";
        break;
    default:
        SendMessageToCallback("Invalid option provided.");
        callback(false, context);
        return;
    }

    curl = curl_easy_init();
    if (curl) {
        curl_easy_setopt(curl, CURLOPT_URL, url.c_str());
        curl_easy_setopt(curl, CURLOPT_WRITEFUNCTION, WriteCallback);
        curl_easy_setopt(curl, CURLOPT_WRITEDATA, &readBuffer);

        res = curl_easy_perform(curl);
        if (res != CURLE_OK) {
            SendMessageToCallback("curl_easy_perform() failed: %s", curl_easy_strerror(res));
            callback(false, context);
            curl_easy_cleanup(curl);
            return;
        }

        try {
            auto responseJson = nlohmann::json::parse(readBuffer);
            if (responseJson.is_null()) {
                SendMessageToCallback("No entries found.");
                callback(false, context);
                curl_easy_cleanup(curl);
                return;
            }

            int numberOfEntries = responseJson["entries"].size();
            outArray = new char* [numberOfEntries * 3 + 1]; // Allocate extra space for null terminator

            int i = 0;
            for (const auto& entry : responseJson["entries"]) {
                if (!entry["playerID"].is_string() || !entry["score"].is_number() || !entry["rank"].is_number()) {
                    SendMessageToCallback("Invalid data at index %d", i / 3);
                    outArray[i] = nullptr;
                    outArray[i + 1] = nullptr;
                    outArray[i + 2] = nullptr;
                    continue;
                }

                std::string playerId = entry["playerID"];
                std::string score = std::to_string(entry["score"].get<int>());
                std::string rank = std::to_string(entry["rank"].get<int>());

                outArray[i] = new char[playerId.length() + 1];
                strcpy_s(outArray[i], playerId.length() + 1, playerId.c_str());

                outArray[i + 1] = new char[score.length() + 1];
                strcpy_s(outArray[i + 1], score.length() + 1, score.c_str());

                outArray[i + 2] = new char[rank.length() + 1];
                strcpy_s(outArray[i + 2], rank.length() + 1, rank.c_str());

                i += 3;
            }
            outArray[numberOfEntries * 3] = nullptr; // Null-terminate the array
            callback(true, context);
        }
        catch (nlohmann::json::exception& e) {
            SendMessageToCallback("JSON parsing error: %s", e.what());
            callback(false, context);
        }

        curl_easy_cleanup(curl);
    }
    else {
        SendMessageToCallback("Failed to initialize CURL.");
        callback(false, context);
    }
}

void GetLeaderboardsForProject(char**& outArray, void(*callback)(bool IsSuccess, void* context), void* context)
{
    CURL* curl;
    CURLcode res;
    std::string readBuffer;

    if (projectID == "")
	{
		SendMessageToCallback("Project ID is not set.");
		callback(false, context);
		return;
	}

    std::string url = "http://pong.hku.nl:5037/api/leaderboards/by-project/" + std::string(projectID);
    SendMessageToCallback("Constructed URL: %s", url.c_str());

    curl = curl_easy_init();
    if (curl) {
        curl_easy_setopt(curl, CURLOPT_URL, url.c_str());
        curl_easy_setopt(curl, CURLOPT_WRITEFUNCTION, WriteCallback);
        curl_easy_setopt(curl, CURLOPT_WRITEDATA, &readBuffer);

        res = curl_easy_perform(curl);
        if (res != CURLE_OK) {
            SendMessageToCallback("curl_easy_perform() failed: %s", curl_easy_strerror(res));
            callback(false, context);
            curl_easy_cleanup(curl);
            return;
        }

        try {
            auto responseJson = nlohmann::json::parse(readBuffer);
            if (responseJson.is_null()) {
                SendMessageToCallback("No leaderboards found.");
                callback(false, context);
                curl_easy_cleanup(curl);
                return;
            }

            int numberOfLeaderboards = responseJson.size();
            outArray = new char* [numberOfLeaderboards * 2 + 1]; // Allocate extra space for null terminator

            int i = 0;
            for (const auto& leaderboard : responseJson) {
                if (!leaderboard["name"].is_string() || !leaderboard["id"].is_string()) {
                    SendMessageToCallback("Leaderboard ID is not a string at index %d", i);
                    outArray[i] = nullptr; // Use nullptr to indicate that fetching failed
                    continue;
                }

                std::string leaderboardName = leaderboard["name"];
                std::string leaderboardId = leaderboard["id"];

                outArray[i * 2] = new char[leaderboardName.length() + 1];
                strcpy_s(outArray[i * 2], leaderboardName.length() + 1, leaderboardName.c_str());

                outArray[i * 2 + 1] = new char[leaderboardId.length() + 1];
                strcpy_s(outArray[i * 2 + 1], leaderboardId.length() + 1, leaderboardId.c_str());

                SendMessageToCallback("Leaderboard %d: Name = %s, ID = %s", i, outArray[i * 2], outArray[i * 2 + 1]);
                i++;
            }
            outArray[numberOfLeaderboards * 2] = nullptr; // Add null terminator
            callback(true, context);
        }
        catch (nlohmann::json::exception& e) {
			SendMessageToCallback("JSON parsing error: %s", e.what());
            callback(false, context);
        }

        curl_easy_cleanup(curl);
    }
    else {
        SendMessageToCallback("Failed to initialize CURL.");
        callback(false, context);
    }
}

void UploadLeaderboardScore(char* leaderboard, int score, void(*callback)(bool IsSuccess, int currentRank, void* context), void* context)
{
    CURL* curl;
    CURLcode res;
    struct curl_slist* headers = NULL;
    std::string readBuffer; // Buffer to hold response data

    // JSON data construction
    nlohmann::json dataJson = {
        {"PlayerID", currentUser},
        {"Score", score}
    };
    std::string jsonData = dataJson.dump();

    // URL construction
    std::string url = "http://pong.hku.nl:5037/api/leaderboards/addentry/" + std::string(leaderboard);

    curl = curl_easy_init();
    if (curl) {
        curl_easy_setopt(curl, CURLOPT_URL, url.c_str());
        headers = curl_slist_append(headers, "Content-Type: application/json");
        curl_easy_setopt(curl, CURLOPT_HTTPHEADER, headers);
        curl_easy_setopt(curl, CURLOPT_POSTFIELDS, jsonData.c_str());
        curl_easy_setopt(curl, CURLOPT_WRITEDATA, &readBuffer);
        curl_easy_setopt(curl, CURLOPT_WRITEFUNCTION, WriteCallback);

        res = curl_easy_perform(curl);
        if (res == CURLE_OK) {
            try {
                auto responseJson = nlohmann::json::parse(readBuffer);
                int rank = responseJson["rank"];
                SendMessageToCallback("Request performed successfully! Rank: %d", rank);
                callback(true, rank, context);
            }
            catch (nlohmann::json::exception& e) {
                SendMessageToCallback("JSON parsing error: %s", e.what());
				SendMessageToCallback("Response: %s", readBuffer.c_str());
                callback(false, -1, context);
            }
        }
        else {
            SendMessageToCallback("curl_easy_perform() failed: %s", curl_easy_strerror(res));
            callback(false, -1, context);
        }

        curl_easy_cleanup(curl);
        curl_slist_free_all(headers);
    }
    else {
        SendMessageToCallback("Failed to initialize CURL.");
        callback(false, -1, context);
    }
}

void SetOutputCallback(void (*callback)(const char* message, void* context), void* context) {
    outputCallback = callback;
    callbackContext = context;
}

void SendMessageToCallback(const char* format, ...) {
    if (outputCallback) {
        va_list args;
        va_start(args, format);

        // Estimate the size of the formatted message
        size_t size = vsnprintf(nullptr, 0, format, args) + 1; // +1 for null terminator
        va_end(args);

        char* message = new char[size];
        va_start(args, format);
        vsnprintf(message, size, format, args);
        va_end(args);

        outputCallback(message, callbackContext);
        delete[] message;
    }
}

void FreeMemory(void* ptr)
{
    if (ptr) {
        delete[] ptr;
    }
}
