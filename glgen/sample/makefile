MCS=mcs
MONO=mono

all: gears.exe

gears.exe:
	$(MCS) /out:gears.exe gears.cs /lib:../bin /r:gl.dll /r:glut.dll
	cp gears.exe ../bin

clean:
	-rm -r gears.exe
