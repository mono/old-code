all: launcher.exe debugserver.exe RemotingContract.dll

debugserver_sources = \
	DebuggerManager.cs	\
	DebuggerServer.cs	\
	ServerProcess.cs	\
	ServerThread.cs		\
	ServerStackFrame.cs	\
	ServerBreakpoint.cs	\
	BreakpointManager.cs	\
	Options.cs

debugserver.exe: $(debugserver_sources) RemotingContract.dll
	gmcs -debug -out:debugserver.exe $(debugserver_sources) -pkg:dotnet -r:./RemotingContract.dll -pkg:mono-debugger

launcher.exe: launcher.cs $(debugserver_sources) RemotingContract.dll
	gmcs -debug -m:Launcher launcher.cs $(debugserver_sources) -pkg:dotnet -r:./RemotingContract.dll -pkg:mono-debugger

RemotingContract.dll: ../RemotingContract/bin/Debug/RemotingContract.dll
	cp ../RemotingContract/bin/Debug/RemotingContract.dll .

run: clean all 
	mono --debug launcher.exe --map="C:\\share=/hlaptop"

martin: clean all
	mono --debug launcher.exe --map="c:\\share=/work/mordor"

clean:
	-rm *exe *dll
