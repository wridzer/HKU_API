// pch.h: This is a precompiled header file.
// Files listed below are compiled only once, improving build performance for future builds.
// This also affects IntelliSense performance, including code completion and many code browsing features.
// However, files listed here are ALL re-compiled if any one of them is updated between builds.
// Do not add files here that you will be updating frequently as this negates the performance advantage.

#ifndef PCH_H
#define PCH_H

// add headers that you want to pre-compile here
#include "framework.h"
#include <string>

#ifdef DLLPROJECT_EXPORTS
#   define EXPORT __declspec(dllexport)
#else
#   define EXPORT __declspec(dllimport)
#endif

char** project_ID;
std::string currentUser;

extern "C" EXPORT void GetUsers(void (*callback)(char** users, int length, void* context), void* context = nullptr);
extern "C" EXPORT void ConfigureProject(const char* project_ID, void(*callback)(bool IsSuccess, void* context), void* context = nullptr);
extern "C" EXPORT void OpenLoginPage();
extern "C" EXPORT void PollLoginStatus(void(*callback)(bool IsSucces, void* context), void* context);
extern "C" EXPORT void CancelPolling();
extern "C" EXPORT void Logout(void (*callback)(bool IsSucces, void* context), void* context = nullptr);
extern "C" EXPORT void GetUser(char** user_ID, void (*callback)(char** username, int length, void* context), void* context = nullptr);
extern "C" EXPORT void UploadLeaderboardScore(char** leaderboard, int score, void (*callback)(bool IsSucces, int currentRank, void* context), void* context = nullptr);


#endif //PCH_H
