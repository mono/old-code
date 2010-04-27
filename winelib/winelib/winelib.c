/*
 * winelib code - Initializes wine as shared library
 *
 * Copyright 2004 Novell, Inc. (http://www.novell.com/)
 *
 * This library is free software; you can redistribute it and/or
 * modify it under the terms of the GNU Lesser General Public
 * License as published by the Free Software Foundation; either
 * version 2.1 of the License, or (at your option) any later version.
 *
 * This library is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
 * Lesser General Public License for more details.
 *
 * You should have received a copy of the GNU Lesser General Public
 * License along with this library; if not, write to the Free Software
 * Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA  02111-1307  USA
 */


#include <stdio.h>
#include <signal.h>
#include <setjmp.h>

#include <windows.h>
#include <winternl.h>

#define	MAX_SIGNALS	32		/* Maximum number of signal actions we save */
static sigjmp_buf		jump;
static HINSTANCE		SharedWineInstance;
static BOOL			UseNewGetUnixPath	= FALSE;
static BOOL			GetUnixPathInitialized	= FALSE;

int WINAPI
WinMain(HINSTANCE hInstance, HINSTANCE hPrevInstance, LPSTR lpszCmdLine, int nCmdShow)
{
	// return to caller.
	SharedWineInstance=hInstance;
	siglongjmp (jump, 1);
}

/*
	WineLoadLibrary is used by System.Windows.Forms to import the Wine dlls
*/
void *
WineGetInstance(void)
{
	return(SharedWineInstance);
}

void *
WineLoadLibrary(unsigned char *dll)
{
	return(LoadLibraryA(dll));
}

void *
WineGetProcAddress(void *handle, unsigned char *function)
{
	return(GetProcAddress(handle, function));
}

/*
	The lovely wine people keep changing existing APIs.
	Yeah, they are internal and the changes are for the
	better, but it really makes it hard using it if
	every release breaks something we use.
*/

typedef char		*(*NewFuncPtrT)(LPCWSTR dosW);
typedef BOOL WINAPI	(*OldFuncPtrT)(LPCWSTR dosW, LPSTR buffer, DWORD len);

unsigned char *
WineGetUnixPath(unsigned char *DosPath)
{
	unsigned char	*RetVal=NULL;
	int		NumOfBytes;
	LPWSTR		DosPathW;
	NewFuncPtrT	NewFuncPtr;
	OldFuncPtrT	OldFuncPtr;


	if (!GetUnixPathInitialized) {
		void	*NTDLLHandle;
		void	*TransFunc;

		NTDLLHandle=WineLoadLibrary("ntdll.dll");
		if (!NTDLLHandle) {
			return(NULL);
		}
		GetUnixPathInitialized=TRUE;

		/* Funky shit, to allow compiling with either Wine version/prototype */
		TransFunc=WineGetProcAddress(NTDLLHandle, "wine_nt_to_unix_file_name");
		if (TransFunc) {
			UseNewGetUnixPath=TRUE;
		} else {
			UseNewGetUnixPath=FALSE;
		}

	}

	if (!DosPath) {
		return(NULL);
	}

	NumOfBytes = MultiByteToWideChar(CP_ACP, 0, (LPCSTR)DosPath, -1, NULL, 0);

	if ((DosPathW = HeapAlloc(GetProcessHeap(), 0, NumOfBytes * sizeof(WCHAR)))) {
		MultiByteToWideChar(CP_ACP, 0, (LPCSTR)DosPath, -1, DosPathW, NumOfBytes);
	} else {
		/* Out of memory */
		return(NULL);
	}

	if (UseNewGetUnixPath) {
		NewFuncPtr=(NewFuncPtrT)wine_get_unix_file_name;
		RetVal=NewFuncPtr(DosPathW);
	} else {
		unsigned char	Buffer[512];

		OldFuncPtr=(OldFuncPtrT)wine_get_unix_file_name;
		if (OldFuncPtr(DosPathW, Buffer, sizeof(Buffer))!=0) {
			RetVal=HeapAlloc(GetProcessHeap(), 0, strlen(Buffer)*sizeof(unsigned char));
			if (RetVal) {
				strcpy(RetVal, Buffer);
			}
		}
	}

	HeapFree(GetProcessHeap(), 0, DosPathW);

	return(RetVal);
}

void
WineReleaseUnixPath(unsigned char *Path)
{
	if (Path) {
		HeapFree(GetProcessHeap(), 0, Path);
	}
}

int
SharedWineInit(void)
{
	TEB			*CurrentTeb;
	unsigned char		Error[1024]="";
	char 			*WineArguments[] = {"sharedapp", LIBPATH "/winelib.exe.so", NULL};
	unsigned int		i;
	struct sigaction	SignalAction[MAX_SIGNALS];
	unsigned int		SignalList[] = { SIGHUP, SIGINT, SIGQUIT, SIGILL, SIGTRAP, 
						SIGABRT, SIGIOT, SIGBUS, SIGFPE, SIGUSR1, 
						SIGSEGV, SIGUSR2, SIGPIPE, SIGALRM, SIGTERM, 
#if defined(SIGSTKFLT)
						SIGSTKFLT, SIGCHLD, SIGCONT, SIGTSTP, SIGSYS, 0};
#else
						SIGCHLD, SIGCONT, SIGTSTP, SIGSYS, 0};
#endif	
	/* Save signal actions */
	for (i=0; SignalList[i]!=0, i<MAX_SIGNALS; i++) {
		sigaction(SignalList[i], NULL, &(SignalAction[i]));
	}

	if (sigsetjmp(jump, 1) == 0){
		wine_init(2, WineArguments, Error, sizeof(Error));
		if (Error[0]!='\0') {
			printf("Wine initialization error:%s\n", Error);
			exit(-1);
		}
	}

	/* Restore signal actions */
	for (i=0; SignalList[i]!=0, i<MAX_SIGNALS; i++) {
		sigaction(SignalList[i], &(SignalAction[i]), NULL);
	}

	CurrentTeb=(TEB *)wine_pthread_get_current_teb();

	CurrentTeb->Tib.ExceptionList = (void *)~0UL;
	VirtualFree( CurrentTeb->DeallocationStack, 0, MEM_RELEASE );
	CurrentTeb->DeallocationStack = NULL;
	CurrentTeb->Tib.StackBase = (void*)~0UL;  /* FIXME: should find actual limits here */
	CurrentTeb->Tib.StackLimit = 0;


	putenv ("_WINE_SHAREDLIB_PATH=" DLLPATH);
	return(0);
}
