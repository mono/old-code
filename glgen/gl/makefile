MCS=mcs
MONO=mono

all: gl.dll

gl.dll:
	$(MONO) ../bin/glgen.exe ../api/gl.xml gl.cs
	$(MCS) /target:library /out:gl.dll /unsafe gl.cs
	cp gl.dll ../bin

clean:
	-rm -r gl.dll
	-rm -r gl.cs
