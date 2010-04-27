CSC=mcs
#PROGFILES=`cygpath \`cygpath -m -s "$(PROGRAMFILES)"\``
#CSC=$(PROGFILES)/Mono-1.1.3/bin/mcs.bat /d:WIN32 
#ROOT=/cygdrive/$(subst \,/,$(subst :\,/,$(SYSTEMROOT)))
#CSC=$(ROOT)/microsoft.net/framework/v1.1.4322/csc.exe /d:WIN32 /optimize+

default:
	$(CSC) /target:library ./src/POP3Connection.cs ./src/POP3Message.cs ./src/AssemblyInfo.cs /out:Mono.Net.POP3.dll
	$(CSC) /target:exe  ./samples/GetLastMessage.cs /r:Mono.Net.POP3.dll /out:./GetLastMessage.exe
	$(CSC) /target:exe  ./samples/ListSubjects.cs /r:Mono.Net.POP3.dll /out:./ListSubjects.exe
	$(CSC) /target:exe  ./samples/ListIDs.cs /r:Mono.Net.POP3.dll /out:./ListIDs.exe
	$(CSC) /target:exe  ./samples/LastMessageHeaders.cs /r:Mono.Net.POP3.dll /out:./LastMessageHeaders.exe

clean:
	rm Mono.Net.POP3.dll GetLastMessage.exe ListIDs.exe ListSubjects.exe LastMessageHeaders.exe
