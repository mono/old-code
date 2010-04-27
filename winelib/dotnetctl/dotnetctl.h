/*
 * dotnetctl.h: .Net control definitions
 *
 * Authors:
 *   Peter Dennis Bartok (pbartok@novell.com)
 *
 * (C) 2004 Novell, Inc.
 *
 *  $Revision: 1.1 $
 *  $Modtime:   11 Apr 2004 20:53:32  $
 *
 *  $Log: dotnetctl.h,v $
 *  Revision 1.1  2004/04/12 02:20:23  pbartok
 *  - More infrastructure for Win32 implementations of controls
 *
 *
 */

#include <windows.h>
#include <windowsx.h>

#ifndef NDEBUG
void __cdecl					DebugOut(const char *Format, ...);
#endif

/* From dotnetctl.c */
BOOL __declspec(dllexport)	DotNetCtlInitialize(HINSTANCE Instance);
BOOL __declspec(dllexport)	DotNetCtlShutdown(HINSTANCE Instance);

/* From dotnetbutton.c */
BOOL DotNetButtonInitialize(HINSTANCE Instance);
