// pch.cpp: source file corresponding to the pre-compiled header

#include "pch.h"
#include <iostream>
#include <cstring>
#define CURL_STATICLIB
#include <curl/curl.h>
#include <nlohmann/json.hpp>

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