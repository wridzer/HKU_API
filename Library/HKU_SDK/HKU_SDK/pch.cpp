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
#include <unordered_set>
#include "httplib.h"

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
        curl_easy_setopt(curl, CURLOPT_URL, "https://localhost:5173/api/users");
        curl_easy_setopt(curl, CURLOPT_WRITEFUNCTION, WriteCallback);
        curl_easy_setopt(curl, CURLOPT_WRITEDATA, &readBuffer);
        CURLcode res = curl_easy_perform(curl);
        if (res != CURLE_OK) {
            std::cerr << "curl_easy_perform() failed: " << curl_easy_strerror(res) << std::endl;
        } else {
            try {
                auto jsonUsers = nlohmann::json::parse(readBuffer);
                numberOfUsers = jsonUsers.size();
                users = new char* [numberOfUsers];

                int i = 0;
                for (const auto& user : jsonUsers) {
                    // Zorg ervoor dat je alleen de userName eigenschap krijgt.
                    if (!user["userName"].is_string()) {
                        std::cerr << "User name is not a string at index " << i << std::endl;
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
                std::cerr << "JSON parse error: " << pe.what() << std::endl;
            }
            catch (nlohmann::json::exception& e) {
                // Dit vangt alle andere nlohmann::json gerelateerde exceptions
                std::cerr << "JSON exception: " << e.what() << std::endl;
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

// A set to store unique project IDs
std::unordered_set<std::string> projectIDStore;

extern "C" void ConfigureProject(const char* project_ID, void(*callback)(bool IsSuccess, void* context), void* context)
{
    CURL* curl;
    CURLcode res;
    std::string readBuffer;

    // URL to fetch the project by ID
    std::string checkUrl = "https://localhost:5173/api/projects/" + std::string(project_ID);

    curl = curl_easy_init();
    if (curl) {
        curl_easy_setopt(curl, CURLOPT_URL, checkUrl.c_str());
        curl_easy_setopt(curl, CURLOPT_WRITEFUNCTION, WriteCallback);
        curl_easy_setopt(curl, CURLOPT_WRITEDATA, &readBuffer);

        res = curl_easy_perform(curl);
        if (res != CURLE_OK) {
            std::cerr << "curl_easy_perform() failed: " << curl_easy_strerror(res) << std::endl;
            callback(false, context);
            curl_easy_cleanup(curl);
            return;
        }

        try {
            auto responseJson = nlohmann::json::parse(readBuffer);
            if (responseJson.is_null()) {
                std::cerr << "Project does not exist." << std::endl;
                callback(false, context);
                curl_easy_cleanup(curl);
                return;
            }

            // Save the project ID locally
            projectIDStore.insert(project_ID);

            std::cout << "Project ID saved locally: " << project_ID << std::endl;
            callback(true, context);

        }
        catch (nlohmann::json::exception& e) {
            std::cerr << "JSON parsing error: " << e.what() << std::endl;
            callback(false, context);
        }

        curl_easy_cleanup(curl);
    }
    else {
        std::cerr << "Failed to initialize CURL." << std::endl;
        callback(false, context);
    }
}

void OpenLoginPage() {
    const char* url = "https://localhost:5173/LoginUser?dll=true"; 
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
            std::cerr << "Error starting server on port 8080" << std::endl;
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
		curl_easy_setopt(curl, CURLOPT_URL, "https://localhost:5173/api/users/logout");
		curl_easy_setopt(curl, CURLOPT_WRITEFUNCTION, WriteCallback);
		curl_easy_setopt(curl, CURLOPT_WRITEDATA, &readBuffer);

		res = curl_easy_perform(curl);
		if (res == CURLE_OK) {
			try {
				auto responseJson = nlohmann::json::parse(readBuffer);
				bool success = responseJson["Success"];
				std::cout << "Request performed successfully! Success: " << success << std::endl;
				callback(success, context);
			}
			catch (nlohmann::json::exception& e) {
				std::cerr << "JSON parsing error: " << e.what() << std::endl;
				callback(false, context);
			}
		}
		else {
			std::cerr << "curl_easy_perform() failed: " << curl_easy_strerror(res) << std::endl;
			callback(false, context);
		}

		curl_easy_cleanup(curl);
	}
	else {
		std::cerr << "Failed to initialize CURL." << std::endl;
		callback(false, context);
	}
}

void GetUser(char* user_ID, void(*callback)(char** username, int length, void* context), void* context)
{
	CURL* curl;
	CURLcode res;
	struct curl_slist* headers = NULL;
	std::string readBuffer; // Buffer to hold response data

	// URL construction
	std::string url = "https://localhost:5173/api/users/" + std::string(user_ID);

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
				std::cout << "Request performed successfully! Username: " << username << std::endl;
				char* usernameCopy = new char[username.length() + 1];
				strcpy_s(usernameCopy, username.length() + 1, username.c_str());
				callback(&usernameCopy, 1, context);
			}
			catch (nlohmann::json::exception& e) {
				std::cerr << "JSON parsing error: " << e.what() << std::endl;
				callback(nullptr, 0, context);
			}
		}
		else {
			std::cerr << "curl_easy_perform() failed: " << curl_easy_strerror(res) << std::endl;
			callback(nullptr, 0, context);
		}

		curl_easy_cleanup(curl);
		curl_slist_free_all(headers);
	}
	else {
		std::cerr << "Failed to initialize CURL." << std::endl;
		callback(nullptr, 0, context);
	}
}

void GetLeaderboard(char* leaderboard_Id, char**& outArray, int amount, GetEntryOptions option, void (*callback)(bool IsSuccess, void* context), void* context)
{
    CURL* curl;
    CURLcode res;
    std::string readBuffer;

    std::string url = "https://localhost:5173/api/leaderboards/entries/" + std::string(leaderboard_Id) + "?amount=" + std::to_string(amount);

    switch (option) {
    case GetEntryOptions::Highest:
        url += "&option=Highest";
        break;
    case GetEntryOptions::AroundMe:
        if (currentUser.empty()) {
            std::cerr << "Current user is not set for AroundMe option." << std::endl;
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
        std::cerr << "Invalid option provided." << std::endl;
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
            std::cerr << "curl_easy_perform() failed: " << curl_easy_strerror(res) << std::endl;
            callback(false, context);
            curl_easy_cleanup(curl);
            return;
        }

        try {
            auto responseJson = nlohmann::json::parse(readBuffer);
            if (responseJson.is_null()) {
                std::cerr << "No entries found." << std::endl;
                callback(false, context);
                curl_easy_cleanup(curl);
                return;
            }

            int numberOfEntries = responseJson.size();
            outArray = new char* [numberOfEntries * 3];

            int i = 0;
            for (const auto& entry : responseJson) {
                if (!entry["PlayerID"].is_string() || !entry["Score"].is_number() || !entry["Rank"].is_number()) {
                    std::cerr << "Invalid data at index " << i / 3 << std::endl;
                    outArray[i] = nullptr;
                    outArray[i + 1] = nullptr;
                    outArray[i + 2] = nullptr;
                    continue;
                }

                std::string playerId = entry["PlayerID"];
                std::string score = std::to_string(entry["Score"].get<int>());
                std::string rank = std::to_string(entry["Rank"].get<int>());

                outArray[i] = new char[playerId.length() + 1];
                strcpy_s(outArray[i], playerId.length() + 1, playerId.c_str());

                outArray[i + 1] = new char[score.length() + 1];
                strcpy_s(outArray[i + 1], score.length() + 1, score.c_str());

                outArray[i + 2] = new char[rank.length() + 1];
                strcpy_s(outArray[i + 2], rank.length() + 1, rank.c_str());

                i += 3;
            }

            callback(true, context);
        }
        catch (nlohmann::json::exception& e) {
            std::cerr << "JSON parsing error: " << e.what() << std::endl;
            callback(false, context);
        }

        curl_easy_cleanup(curl);
    }
    else {
        std::cerr << "Failed to initialize CURL." << std::endl;
        callback(false, context);
    }
}



void GetLeaderboardsForProject(char**& outArray, void(*callback)(bool IsSuccess, void* context), void* context)
{
    CURL* curl;
    CURLcode res;
    std::string readBuffer;

    curl = curl_easy_init();
    if (curl) {
        curl_easy_setopt(curl, CURLOPT_URL, "https://localhost:5173/api/leaderboards/by-project/" + std::string(*project_ID));
        curl_easy_setopt(curl, CURLOPT_WRITEFUNCTION, WriteCallback);
        curl_easy_setopt(curl, CURLOPT_WRITEDATA, &readBuffer);

        res = curl_easy_perform(curl);
        if (res != CURLE_OK) {
            std::cerr << "curl_easy_perform() failed: " << curl_easy_strerror(res) << std::endl;
            callback(false, context);
            curl_easy_cleanup(curl);
            return;
        }

        try {
            auto responseJson = nlohmann::json::parse(readBuffer);
            if (responseJson.is_null()) {
                std::cerr << "No leaderboards found." << std::endl;
                callback(false, context);
                curl_easy_cleanup(curl);
                return;
            }

            int numberOfLeaderboards = responseJson.size();
            outArray = new char* [numberOfLeaderboards];

            int i = 0;
            for (const auto& leaderboard : responseJson) {
                if (!leaderboard["leaderboardId"].is_string()) {
                    std::cerr << "Leaderboard ID is not a string at index " << i << std::endl;
                    outArray[i] = nullptr; // Use nullptr to indicate that fetching failed
                    continue;
                }

                std::string leaderboardId = leaderboard["leaderboardId"];
                outArray[i] = new char[leaderboardId.length() + 1];
                strcpy_s(outArray[i], leaderboardId.length() + 1, leaderboardId.c_str());
                std::cout << "Leaderboard ID " << i << ": " << outArray[i] << std::endl; // Debugging informatie
                i++;
            }

            callback(true, context);
        }
        catch (nlohmann::json::exception& e) {
            std::cerr << "JSON parsing error: " << e.what() << std::endl;
            callback(false, context);
        }

        curl_easy_cleanup(curl);
    }
    else {
        std::cerr << "Failed to initialize CURL." << std::endl;
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
    std::string url = "https://localhost:5173/api/leaderboards/addentry/" + std::string(leaderboard);

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
                int rank = responseJson["Rank"];
                std::cout << "Request performed successfully! Rank: " << rank << std::endl;
                callback(true, rank, context);
            }
            catch (nlohmann::json::exception& e) {
                std::cerr << "JSON parsing error: " << e.what() << std::endl;
                callback(false, -1, context);
            }
        }
        else {
            std::cerr << "curl_easy_perform() failed: " << curl_easy_strerror(res) << std::endl;
            callback(false, -1, context);
        }

        curl_easy_cleanup(curl);
        curl_slist_free_all(headers);
    }
    else {
        std::cerr << "Failed to initialize CURL." << std::endl;
        callback(false, -1, context);
    }
}

extern "C" void FreeMemory(void* ptr)
{
    if (ptr) {
        delete[] ptr;
    }
}
