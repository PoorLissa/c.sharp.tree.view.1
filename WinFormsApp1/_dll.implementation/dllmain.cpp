// dllmain.cpp : Defines the entry point for the DLL application.
#include "pch.h"
#include <string>

// -----------------------------------------------------------------------------------------------------------------------

// Checks if the directory contains at least one subdirectory
// Prerequisites:
//  - Path parameter must be ending with "*.*"
extern "C" __declspec(dllexport) bool dirHasSubdirs(const char *path)
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

BOOL APIENTRY DllMain( HMODULE hModule, DWORD  ul_reason_for_call, LPVOID lpReserved)
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

