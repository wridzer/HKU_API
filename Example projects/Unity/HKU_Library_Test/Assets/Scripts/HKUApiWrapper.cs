using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

public class HKUApiWrapper
{
    [DllImport("HKU_SDK", CallingConvention = CallingConvention.Cdecl)]
    public static extern void GetUsers(IntPtr callback, IntPtr context);
    [DllImport("HKU_SDK", CallingConvention = CallingConvention.Cdecl)]
    public static extern void ConfigureProject(string project_ID, ConfigureProjectCallbackDelegate callback, IntPtr context);
    [DllImport("HKU_SDK", CallingConvention = CallingConvention.Cdecl)]
    public static extern void OpenLoginPage();
    [DllImport("HKU_SDK", CallingConvention = CallingConvention.Cdecl)]
    public static extern void PollLoginStatus(LoginStatusCallbackDelegate callback, IntPtr context);
    [DllImport("HKU_SDK", CallingConvention = CallingConvention.Cdecl)]
    public static extern void CancelPolling();
    [DllImport("HKU_SDK", CallingConvention = CallingConvention.Cdecl)]
    public static extern void Logout(IntPtr callback, IntPtr context);
    [DllImport("HKU_SDK", CallingConvention = CallingConvention.Cdecl)]
    public static extern void GetUser(char[][] user_ID, IntPtr callback, IntPtr context);
    [DllImport("HKU_SDK", CallingConvention = CallingConvention.Cdecl)]
    public static extern void UploadLeaderboardScore(char[][] leaderboard, int score, IntPtr callback, IntPtr context);

    public delegate void UsersCallbackDelegate(IntPtr users, int length, IntPtr context);
    public delegate void ConfigureProjectCallbackDelegate(bool IsSuccess, IntPtr context);
    public delegate void LoginStatusCallbackDelegate(bool IsSucces, IntPtr context);
    public delegate void LogoutCallbackDelegate(bool IsSucces, IntPtr context);
    public delegate void GetUserCallbackDelegate(IntPtr username, int length, IntPtr context);
    public delegate void UploadLeaderboardScoreCallbackDelegate(bool IsSucces, int currentRank, IntPtr context);


}
