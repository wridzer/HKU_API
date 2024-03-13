#include "HKU_API.h"
#include <curl/curl.h>
#include <iostream>
#include <string>
#include <cstring>

namespace HKU_API {

    static size_t WriteCallback(void* contents, size_t size, size_t nmemb, std::string* data) {
        size_t realSize = size * nmemb;
        data->append((char*)contents, realSize);
        return realSize;
    }

	void HKU::GetUsers(HKU_CALLBACK callback, void* context) {

        char** users = {};

        CURL* curl = curl_easy_init();
        std::string readBuffer;

        if (curl) {
            curl_easy_setopt(curl, CURLOPT_URL, "https://api.example.com/data");
            curl_easy_setopt(curl, CURLOPT_WRITEFUNCTION, WriteCallback);
            curl_easy_setopt(curl, CURLOPT_WRITEDATA, &readBuffer);
            CURLcode res = curl_easy_perform(curl);
            if (res != CURLE_OK) {
                std::cerr << "curl_easy_perform() failed: " << curl_easy_strerror(res) << std::endl;
            }
            curl_easy_cleanup(curl);

            // Allocate memory for the JSON string and copy data into it
            *users = new char[readBuffer.length() + 1];
            strcpy_s(*users, readBuffer.length() + 1, readBuffer.c_str());
        }

		callback(users);
	}
}