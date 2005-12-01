INCLUDES= \
	@MONO_CFLAGS@

EXTRA_DIST= \
	Bundle.m \
	Loader.c
bin_PROGRAMS=CocoaSharpLoader
CocoaSharpLoader_SOURCES = \
	Bundle.m \
	Loader.c

Bundle.o: $(addprefix $(srcdir)/, Bundle.m)
	gcc -x objective-c -c $(addprefix $(srcdir)/, Bundle.m)

Loader.o: $(addprefix $(srcdir)/, Loader.c)
	gcc -c $(addprefix $(srcdir)/, Loader.c) @MONO_CFLAGS@

CocoaSharpLoader$(EXEEXT): Bundle.o Loader.o
	gcc Bundle.o Loader.o -o CocoaSharpLoader -framework Cocoa @MONO_LIBS@
