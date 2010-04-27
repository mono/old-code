/*
 * dotnetctl.c: .Net controls
 *
 * Authors:
 *   Peter Dennis Bartok (pbartok@novell.com)
 *
 * (C) 2004 Novell, Inc.
 *
 *  $Revision: 1.2 $
 *  $Modtime:   11 Apr 2004 20:53:32  $
 *
 *  $Log: dotnetctl.c,v $
 *  Revision 1.2  2004/04/12 02:21:36  pbartok
 *  - More infrastructure
 *
 *
 */

#include <windows.h>
#include <windowsx.h>

#include <stdio.h>

#include "dotnetctl.h"

#ifndef NDEBUG
void __cdecl
DebugOut(const char *Format, ...)
{
   unsigned char  DebugBuffer[10240];
   va_list  argptr;

   va_start(argptr, Format);
   vsprintf(DebugBuffer, Format, argptr);
   va_end(argptr);

   OutputDebugString(DebugBuffer);
	printf("%s", DebugBuffer);
}
#endif

BOOL __declspec(dllexport)
DotNetCtlInitialize(HINSTANCE Instance)
{
	DebugOut("DotNetCtlInitialize(%x) called\n", Instance);

	DotNetButtonInitialize(Instance);
	return(TRUE);
}

BOOL __declspec(dllexport)
DotNetCtlShutdown(HINSTANCE Instance)
{
	return(TRUE);
}
