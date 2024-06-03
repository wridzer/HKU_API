#ifndef PCH_H
#define PCH_H

// add headers that you want to pre-compile here
#include "framework.h"
#include <string>
#include <atomic>

#ifdef DLLPROJECT_EXPORTS
#   define EXPORT __declspec(dllexport)
#else
#   define EXPORT __declspec(dllimport)
#endif

std::string projectID = std::string();
std::string currentUser;
std::atomic<bool> pollingActive(false);

enum class GetEntryOptions
{
	Highest,
	AroundMe,
	AtRank,
	Friends
};

extern "C" EXPORT void GetUsers(void (*callback)(char** users, int length, void* context), void* context = nullptr);
extern "C" EXPORT void ConfigureProject(const char* project_ID, void(*callback)(bool IsSuccess, void* context), void* context = nullptr);
extern "C" EXPORT void OpenLoginPage();
extern "C" EXPORT void StartPolling(void(*callback)(bool IsSuccess, const char* userId, void* context), void* context);
extern "C" EXPORT void CancelPolling();
extern "C" EXPORT void Logout(void (*callback)(bool IsSuccess, void* context), void* context = nullptr);
extern "C" EXPORT void GetUser(char* user_ID, void (*callback)(char** username, int length, void* context), void* context = nullptr);
extern "C" EXPORT void GetLeaderboard(char* leaderboard_Id, char**& outArray, int amount, GetEntryOptions option, void (*callback)(bool IsSuccess, void* context), void* context = nullptr); extern "C" EXPORT void GetLeaderboardsForProject(char** &outArray, void (*callback)(bool IsSuccess, void* context), void* context = nullptr);
extern "C" EXPORT void GetLeaderboardsForProject(char**& outArray, void(*callback)(bool IsSuccess, void* context), void* context);
extern "C" EXPORT void UploadLeaderboardScore(char* leaderboard, int score, void (*callback)(bool IsSuccess, int currentRank, void* context), void* context = nullptr);
extern "C" EXPORT void SetOutputCallback(void (*callback)(const char* message, void* context), void* context = nullptr);
void SendMessageToCallback(const char* format, ...);

#endif //PCH_H
