// pch.cpp: source file corresponding to the pre-compiled header

#include "pch.h"

// When you are using pre-compiled headers, this source file is necessary for compilation to succeed.
//#define CURL_STATICLIB
//#include <curl/curl.h>
#include <iostream>
#include <vector>
//#include <nlohmann/json.hpp>

namespace HKU_API {

    static size_t WriteCallback(void* contents, size_t size, size_t nmemb, std::string* data) {
        size_t realSize = size * nmemb;
        data->append((char*)contents, realSize);
        return realSize;
    }

    void GetUsers(void (*callback)(char** users, int length, void* context), void* context) {

        //CURL* curl = curl_easy_init();
        //std::string readBuffer;

        //if (curl) {
        //    curl_easy_setopt(curl, CURLOPT_URL, "https://localhost:5173/api/users");
        //    curl_easy_setopt(curl, CURLOPT_WRITEFUNCTION, WriteCallback);
        //    curl_easy_setopt(curl, CURLOPT_WRITEDATA, &readBuffer);
        //    CURLcode res = curl_easy_perform(curl);
        //    if (res != CURLE_OK) {
        //        std::cerr << "curl_easy_perform() failed: " << curl_easy_strerror(res) << std::endl;
        //    } else {
        //        // Assuming the readBuffer contains a JSON array of strings
        //        auto jsonUsers = nlohmann::json::parse(readBuffer);
        //        for (const auto& user : jsonUsers) {
        //            users.push_back(user.get<std::string>());
        //        }
        //    }
        //    curl_easy_cleanup(curl);
        //}

            // Handmatig een array van gebruikersnamen opstellen.
        const char* users[] = { "peter", "wridzer", "maurice" };
        int numberOfUsers = sizeof(users) / sizeof(char*);

        // Roep de callback aan met de array en het aantal gebruikers.
        callback(const_cast<char**>(users), numberOfUsers, context);
    }
}