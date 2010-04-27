ifeq ("$(WINDIR)","")
	CSC := mcs /debug+
	S:=/
else
	CSC := csc /debug+
	S:=\\
endif

.SUFFIXES: .cs .exe

%.exe: %.cs
	$(CSC) /out:$@ /r:..$(S)ice.dll $<

all: hello.exe hello-server.exe expreval.exe

clean:
	rm -f *.exe
