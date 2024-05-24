using SQLitePCL;
using System;
using System.Runtime.InteropServices;

public static class CustomSQLitePCLRawProvider
{
    static CustomSQLitePCLRawProvider()
    {
        LoadNativeLibrary();
    }

    public static void Initialize()
    {
        raw.SetProvider(new SQLite3Provider_custom());
        Batteries_V2.Init();
    }

    private static void LoadNativeLibrary()
    {
        var libraryPath = "/usr/lib/x86_64-linux-gnu/libsqlite3.so"; // Adjust the path as needed

        IntPtr handle = dlopen(libraryPath, RTLD_NOW);
        if (handle == IntPtr.Zero)
        {
            IntPtr error = dlerror();
            string errorMessage = Marshal.PtrToStringAnsi(error);
            throw new Exception($"Unable to load library: {libraryPath}, Error: {errorMessage}");
        }
    }

    private const int RTLD_NOW = 2;

    [DllImport("libdl.so")]
    private static extern IntPtr dlopen(string fileName, int flags);

    [DllImport("libdl.so")]
    private static extern IntPtr dlerror();
}

internal class SQLite3Provider_custom : ISQLite3Provider
{
    // Implement methods required by ISQLite3Provider using your custom library loader
}
