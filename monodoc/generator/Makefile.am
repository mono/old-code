MCS = mcs
SRC = $(srcdir)/updater.cs
PROGRAM = updater.exe

monodocdir = $(libdir)/monodoc
monodoc_DATA = $(PROGRAM)

all: $(PROGRAM)

$(PROGRAM): $(srcdir)/updater.cs
	$(MCS) /debug /out:$(PROGRAM) $(SRC)

CLEANFILES = updater.exe

EXTRA_DIST = $(SRC)

