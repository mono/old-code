

	******************************************************
	
		Mono AppServer (c#) - v0.2

		developed by Brian Ritchie 

		http://www12.brinkster.com/brianr/ideas/appserver.aspx
		comments / suggestions: brianlritchie@hotmail.com
	
	******************************************************


1. Introduction
2. Credits
3. License
4. Install


--------------
INTRODUCTION
--------------
This is a .NET AppServer that will hopefully become part of the Mono Platform.

NOTE: Runs on Windows w/ MS .NET Framework...not Mono yet :(
** Work in progress **

See appserver.home.htm for more details.


--------------
CREDITS
--------------
XSP Server:
This product includes software developed by 
 *        Daniel Lopez Ridruejo (daniel@rawbyte.com) and
 *        Ximian Inc. (http://www.ximian.com)

FTP Server:
 *        Pramod Singh (pramodkumarsingh@hotmail.com)

Graphics:
 *        Jakub "Jimmac" Steiner (http://jimmac.musichall.cz/ikony.php3)

Compression Library (SharpZipLib)
 *        Mike Krueger           (http://www.icsharpcode.net/OpenSource/SharpZipLib/)

--------------
LICENSE: X11
--------------

Copyright (c) 2003 Brian Ritchie <brianlritchie@hotmail.com>

Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.

http://www.opensource.org/licenses/mit-license.php


-----------------
INSTALLATION
-----------------
1. Unzip to c:\Mono.AppServer
2. bobbob.usr should be in your c:\ 
	(this is only needed by the security system for the FTP app.  I'll change this soon.)
2. Make sure gacutil is on your path
3. Run "run.bat" in the bin directory
4. Pick one:
  a) Admin Client: 
	Navigate in browser to http://localhost:8080/default.aspx
  b) Sample Web App: 
	Navigate in browser to http://localhost:81/helloworld.aspx
  c) Sample FTP App: 
	ftp localhost
	user: bobbob
	pwd: bob
  d) Sample Remoting Client:
	C:\Mono.AppServer\test\RemotingClient\bin\Debug\RemotingClient.exe



