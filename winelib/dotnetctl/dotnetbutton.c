/*
 * dotnetbutton.c: .Net Button Class
 *
 * Authors:
 *   Peter Dennis Bartok (pbartok@novell.com)
 *
 * (C) 2004 Novell, Inc.
 *
 *  $Revision: 1.1 $
 *  $Modtime:   11 Apr 2004 20:53:32  $
 *
 *  $Log: dotnetbutton.c,v $
 *  Revision 1.1  2004/04/12 02:20:23  pbartok
 *  - More infrastructure for Win32 implementations of controls
 *
 *
 *
 */

#include <windows.h>
#include <windowsx.h>

#include <stdio.h>

#include "dotnetctl.h"

#define	BUTTON_CLASSNAME		"DotNetButton"

static BOOL			ButtonInitialized	= FALSE;
static WNDPROC		BaseButtonWndProc	= NULL;
static int			ButtonStructIndex	= 0;


#define	DNM_BTN_BASE				WM_USER
#define	DNM_BTN_FLATSTYLE			(DNM_BTN_BASE+1)
#define	DNM_BTN_FORECOLOR			(DNM_BTN_BASE+2)
#define	DNM_BTN_BACKCOLOR			(DNM_BTN_BASE+3)

typedef struct {
	/* Configuration */
	HBRUSH			Background;
	HPEN				Pen;
	HFONT				Font;
	unsigned long	FlatStyle;

	/* Double buffering */
	HDC				BufferDC;
	HBITMAP			BufferBitmap;
	HBITMAP			OrgBitmap;
	RECT				ClientArea;
} ButtonStruct;


static BOOL
DoubleBufferInit(HWND hwnd, ButtonStruct *BS)
{
   HDC   hdc;

   hdc = GetDC(hwnd);

   GetClientRect(hwnd, &BS->ClientArea);
   BS->BufferDC=CreateCompatibleDC(hdc);
   BS->BufferBitmap=CreateCompatibleBitmap(hdc, BS->ClientArea.right, BS->ClientArea.bottom);
   BS->OrgBitmap=SelectObject(BS->BufferDC, BS->BufferBitmap);
   FillRect(BS->BufferDC, &BS->ClientArea, BS->Background);

   ReleaseDC(hwnd, hdc);
   return(TRUE);
}


LRESULT CALLBACK
ButtonSuperclassWndProc(HWND hwnd, UINT msg, WPARAM wParam, LPARAM lParam)
{
	if (msg>=WM_USER) {
		DebugOut("\nButton >=WM_USER received\n");
	}

	switch(msg) {
		case WM_CREATE: {
			ButtonStruct	*BS, *BS2;

			DebugOut("Button WM_CREATE entered\n");

			BS=malloc(sizeof(ButtonStruct));
			if (!BS) {
				DebugOut("Button WM_CREATE failed [line %d]\n", __LINE__);
				return(-1);
			}

			memset(BS, 0, sizeof(ButtonStruct));
			SetWindowLong(hwnd, ButtonStructIndex, (LONG)BS);
			BS2=(ButtonStruct *)GetWindowLong(hwnd, ButtonStructIndex);
			break;
		}

		case DNM_BTN_FLATSTYLE: {
			return(0);
		}

		case DNM_BTN_FORECOLOR: {
			ButtonStruct	*BS=(ButtonStruct *)GetWindowLong(hwnd, ButtonStructIndex);

			BS->Pen=CreatePen(PS_SOLID, 1, (COLORREF)lParam);

			return(0);
		}

		case DNM_BTN_BACKCOLOR: {
			ButtonStruct	*BS=(ButtonStruct *)GetWindowLong(hwnd, ButtonStructIndex);
			LOGBRUSH			lb;

			lb.lbColor=(COLORREF)lParam;
			lb.lbStyle=BS_SOLID;

			BS->Background=CreateBrushIndirect(&lb);
			return(0);
		}

		case WM_PAINT: {
			ButtonStruct	*BS=(ButtonStruct *)GetWindowLong(hwnd, ButtonStructIndex);
			PAINTSTRUCT		ps;
			HDC				hdc;
			HDC				PaintDC;
			HPEN				OrgPen;
			HBRUSH			OrgBrush;
			HFONT				OrgFont;

			DebugOut("DotNetButton WM_PAINT entered\n");

#if 0
			PaintDC=BeginPaint(hwnd, &ps);

			if (ps.fErase) {
				FillRect(PaintDC, &ps.rcPaint, BS->Background);
			}

			hdc=BS->BufferDC;
			OrgPen=SelectObject(hdc, BS->Pen);
			OrgBrush=SelectObject(hdc, BS->Background);
			OrgFont=SelectObject(hdc, BS->Font);

			BitBlt(PaintDC, 0, 0, BS->ClientArea.right, BS->ClientArea.bottom, BS->BufferDC, 0, 0, SRCCOPY);

			SelectObject(hdc, OrgPen);
			SelectObject(hdc, OrgBrush);
			SelectObject(hdc, OrgFont);

			EndPaint(hwnd, &ps);

			return(0);
#endif
		}
	}
	return(CallWindowProc(BaseButtonWndProc, hwnd, msg, wParam, lParam));
}

BOOL
DotNetButtonInitialize(HINSTANCE Instance)
{
	WNDCLASS		wndClass;

	/* Superclass the Button control */
	if (GetClassInfo(Instance, "BUTTON", &wndClass)==FALSE) {
		return(FALSE);
	}

	wndClass.hInstance=Instance;
	wndClass.lpszClassName=BUTTON_CLASSNAME;
	BaseButtonWndProc=wndClass.lpfnWndProc;
	wndClass.lpfnWndProc=ButtonSuperclassWndProc;
	ButtonStructIndex=wndClass.cbWndExtra;
	wndClass.cbWndExtra+=sizeof(ButtonStruct *);

	if (RegisterClass(&wndClass)==0) {
		return(FALSE);
	}
	ButtonInitialized=TRUE;

	return(TRUE);
}

BOOL
DotNetButtonShutdown(HINSTANCE Instance)
{
	UnregisterClass(BUTTON_CLASSNAME, Instance);
	return(TRUE);
}
