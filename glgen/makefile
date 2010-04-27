MCS=mcs
MONO=mono
DIRS=glgen gl glut sample

all:
	-mkdir bin
	for i in $(DIRS); do			\
		$(MAKE) -C $$i || exit 1;	\
	done;

clean:
	-rm -rf bin
	for i in $(DIRS); do			\
		$(MAKE) -C $$i clean || exit 1;	\
	done;
