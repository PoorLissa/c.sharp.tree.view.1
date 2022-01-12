// dllmain.cpp : Defines the entry point for the DLL application.
#include "pch.h"
#include <string>

// -----------------------------------------------------------------------------------------------------------------------

// Checks if the directory contains any subdirectories
// Prerequisites:
//  - Path parameter must be ending with "*.*"
extern "C" __declspec(dllexport) bool dirContainsSubdirectoryA(const char* path)
{
    WIN32_FIND_DATAA FindFileData;

    HANDLE hFind = FindFirstFileA(path, &FindFileData);

    if (hFind != INVALID_HANDLE_VALUE)
    {
        do
        {
            char* fileName = FindFileData.cFileName;

            if (strcmp(fileName, ".") == 0 || strcmp(fileName, "..") == 0)
                continue;

            if (FindFileData.dwFileAttributes & FILE_ATTRIBUTE_DIRECTORY)
            {
                FindClose(hFind);
                return true;
            }

        } while (FindNextFileA(hFind, &FindFileData));

        FindClose(hFind);
    }

    return false;
}

// -----------------------------------------------------------------------------------------------------------------------

// Checks if the directory contains any subdirectories
extern "C" __declspec(dllexport) bool dirContainsSubdirectoryW(const wchar_t* dir)
{
    static wchar_t str[_MAX_PATH];
    static const wchar_t* txt = L"\\*.*";

    size_t len = wcslen(dir);

    memcpy((void*)(str), (void*)dir, sizeof(wchar_t) * len);
    memcpy((void*)(str + len), (void*)txt, sizeof(wchar_t) * 4u);

    str[len + 4u] = '\0';

    WIN32_FIND_DATAW FindFileData;

    HANDLE hFind = FindFirstFileW(str, &FindFileData);

    if (hFind != INVALID_HANDLE_VALUE)
    {
        do
        {
            wchar_t* fileName = FindFileData.cFileName;

            if (wcscmp(fileName, L".") == 0 || wcscmp(fileName, L"..") == 0)
                continue;

            if (FindFileData.dwFileAttributes & FILE_ATTRIBUTE_DIRECTORY)
            {
                FindClose(hFind);
                return true;
            }

        } while (FindNextFileW(hFind, &FindFileData));

        FindClose(hFind);
    }

    return false;
}

// -----------------------------------------------------------------------------------------------------------------------

BOOL APIENTRY DllMain(HMODULE hModule, DWORD  ul_reason_for_call, LPVOID lpReserved)
{
    switch (ul_reason_for_call)
    {
        case DLL_PROCESS_ATTACH:
        case DLL_THREAD_ATTACH:
        case DLL_THREAD_DETACH:
        case DLL_PROCESS_DETACH:
            break;
    }

    return TRUE;
}

