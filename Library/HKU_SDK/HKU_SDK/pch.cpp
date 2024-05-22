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

void OpenLoginPage()
{
        const char* url = "https://localhost:5173/LoginUser";
#ifdef _WIN32
        std::string command = "start " + std::string(url);
#elif __APPLE__
        std::string command = "open " + std::string(url);
#elif __linux__
        std::string command = "xdg-open " + std::string(url);
#endif
        system(command.c_str());
}

std::atomic<bool> pollingActive(false);

void PollLoginStatus(void(*callback)(bool IsSuccess, void* context), void* context)
{
    pollingActive.store(true);
    CURL* curl;
    CURLcode res;
    std::string readBuffer;

    curl = curl_easy_init();
    if (curl) {
        while (pollingActive.load()) {
            readBuffer.clear();
            curl_easy_setopt(curl, CURLOPT_URL, "https://localhost:5173/api/users/currentuser");
            curl_easy_setopt(curl, CURLOPT_WRITEFUNCTION, WriteCallback);
            curl_easy_setopt(curl, CURLOPT_WRITEDATA, &readBuffer);

            res = curl_easy_perform(curl);
            if (res == CURLE_OK) {
                try {
                    auto responseJson = nlohmann::json::parse(readBuffer);
                    std::cout << "Response JSON: " << responseJson.dump(4) << std::endl;

                    if (!responseJson.is_null() && responseJson.contains("id")) {
                        std::string currentUser = responseJson["id"].get<std::string>();
                        std::cout << "User logged in with ID: " << currentUser << std::endl;
                        callback(true, context);
                        pollingActive.store(false);
                        break;
                    }
                    else {
                        std::cerr << "Response does not contain 'id'." << std::endl;
                    }
                }
                catch (nlohmann::json::exception& e) {
                    std::cerr << "JSON parsing error: " << e.what() << std::endl;
                }
            }
            else {
                std::cerr << "curl_easy_perform() failed: " << curl_easy_strerror(res) << std::endl;
            }

            std::this_thread::sleep_for(std::chrono::seconds(3));
        }

        curl_easy_cleanup(curl);
    }
    else {
        std::cerr << "Failed to initialize CURL." << std::endl;
        callback(false, context);
    }
}


void CancelPolling() {
    pollingActive.store(false);
}

void Logout(void(*callback)(bool IsSucces, void* context), void* context)
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

void GetUser(char** user_ID, void(*callback)(char** username, int length, void* context), void* context)
{
	CURL* curl;
	CURLcode res;
	struct curl_slist* headers = NULL;
	std::string readBuffer; // Buffer to hold response data

	// URL construction
	std::string url = "https://localhost:5173/api/users/" + std::string(*user_ID);

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

void UploadLeaderboardScore(char** leaderboard, int score, void(*callback)(bool IsSucces, int currentRank, void* context), void* context)
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
    std::string url = "https://localhost:5173/api/leaderboards/addentry/" + std::string(*leaderboard);

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

