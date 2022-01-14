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
// C# should pass string's length as a parameter, as it already knows its length: no need to calculate it once again
extern "C" __declspec(dllexport) bool dirContainsSubdirectoryW(const wchar_t* dir, int length = 0)
{
    static wchar_t          str[_MAX_PATH];
    static const wchar_t* txt = L"\\*.*";
    static WIN32_FIND_DATAW findFileData;

    size_t len = length > 0 ? static_cast<size_t>(length) : wcslen(dir);

    memcpy((void*)(str), (void*)dir, sizeof(wchar_t) * len);
    memcpy((void*)(str + len), (void*)txt, sizeof(wchar_t) * 4u);

    str[len + 4u] = '\0';

    HANDLE hFind = FindFirstFileW(str, &findFileData);

    if (hFind != INVALID_HANDLE_VALUE)
    {
        do
        {
            wchar_t* fileName = findFileData.cFileName;

            if (wcscmp(fileName, L".") == 0 || wcscmp(fileName, L"..") == 0)
                continue;

            if (findFileData.dwFileAttributes & FILE_ATTRIBUTE_DIRECTORY)
            {
                FindClose(hFind);
                return true;
            }

        } while (FindNextFileW(hFind, &findFileData));

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

