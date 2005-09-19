CSC  =	mcs

DLLS =	-r glib-sharp.dll     \
	-r gtk-sharp.dll      \
	-r gdk-sharp.dll      \
	-r System.Drawing.dll

all:
	$(CSC) /unsafe $(DLLS) menubar.cs
	$(CSC) /unsafe $(DLLS) menubar2.cs
clean: 
	rm -f *.exe
