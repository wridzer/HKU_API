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
char** currentUser;

extern "C" EXPORT void GetUsers(void (*callback)(char** users, int length, void* context), void* context = nullptr);
extern "C" EXPORT void ConfigureProject(char** project_ID, void (*callback)(bool IsSucces, void* context), void* context = nullptr);
extern "C" EXPORT void Login(char** username, char** password, void(*callback)(bool IsSucces, void* context), void* context = nullptr);
extern "C" EXPORT void Logout(void (*callback)(bool IsSucces, void* context), void* context = nullptr);
extern "C" EXPORT void GetUser(char** user_ID, void (*callback)(char** username, int length, void* context), void* context = nullptr);
extern "C" EXPORT void UploadLeaderboardScore(char** project_ID, char** user_ID, void (*callback)(bool IsSucces, int currentRank, void* context), void* context = nullptr);


#endif //PCH_H
