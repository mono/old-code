CSC = mcs 

BASE_SOURCES = \
	console.cs 

GTK_SOURCES = \
	$(BASE_SOURCES) \
	console-gtk.cs  \
	textwriter-gtk.cs 

all: console-gtk.exe 

top_srcdir = ..
console-gtk.exe: $(GTK_SOURCES) $(top_srcdir)/logolib.DLL 
	$(CSC) -o console-gtk.exe $(GTK_SOURCES) -r $(top_srcdir)/logolib.DLL -r /gnome/head/INSTALL/lib/gtk-sharp.dll -r /usr/lib/System.Drawing.dll 
	

clean:
	rm -f console-gtk.exe

