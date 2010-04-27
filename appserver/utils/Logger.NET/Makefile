# first target is default - that is, release
# .PHONY means that the targets will run even if theres a file by that name in the current directory

COMPILER = mcs
SRC      = /recurse:*.cs
LIBS     = System.Drawing.dll
FLAGS    = 

OUT      = Logger.NET.dll
TARGET   = library

set SRC=/recurse:*.cs
set LIBS=System.Drawing.dll
set FLAGS=

set OUT=Logger.NET.dll
set TARGET=library

.PHONY: release debug
release:
	$(COMPILER) /out:$(OUT) /target:$(TARGET) /lib:$(LIBS) $(FLAGS) /optimize+ /define:RELEASE $(SRC)

debug:
	$(COMPILER) /out:$(OUT) /target:$(TARGET) /lib:$(LIBS) $(FLAGS) /optimize- /debug:full /define:DEBUG $(SRC)