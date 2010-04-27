# C#/.NET Ice bindings
#
# Copyright (C) 2003
# Sparkle Studios, LLC
#
# Author(s):
#    Vladimir Vukicevic <vladimir@sparklestudios.com>
#
# slice2cs is distributed under the GNU Public License, contained within
# the "LICENSE.GPL" file in the slice2cs directory.
#


ICE_ROOT=/opt/Ice-1.1.0

SLICE2CS = slice2cs
SLICEFLAGS = -I$(ICE_ROOT)/slice
CXXFLAGS = $(OPT) -I. -I$(ICE_ROOT)/include 
LDFLAGS = $(OPT) -L$(ICE_ROOT)/lib \
	-lSlice -lIceUtil -lm

all: slice2cs

slice2cs: Main.o Gen.o
	gcc -o $@ $^ $(LDFLAGS)

Main.o: Gen.h

Gen.o: Gen.cpp Gen.h

clean:
	rm -f slice2cs Gen.o Main.o
