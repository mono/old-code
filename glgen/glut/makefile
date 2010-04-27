MCS=mcs
MONO=mono

all: glut.dll

glut.dll:
	$(MCS) /target:library /out:glut.dll /unsafe glut.cs
	cp glut.dll ../bin

clean:
	-rm -r glut.dll
