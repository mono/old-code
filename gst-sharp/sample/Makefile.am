SUBDIRS = 

# mixer.exe currently broken
TARGETS = play-mp3.exe spider.exe player.exe metadata.exe

references = /r:../gst-sharp/gst-sharp.dll /pkg:gtk-sharp-2.0

noinst_SCRIPTS = $(TARGETS)
CLEANFILES = $(TARGETS)

player.exe: $(srcdir)/Player.cs
	$(CSC) /out:player.exe $(references) $(srcdir)/Player.cs

play-mp3.exe: $(srcdir)/PlayMp3.cs
	$(CSC) /out:play-mp3.exe $(references) $(srcdir)/PlayMp3.cs

spider.exe: $(srcdir)/Spider.cs
	$(CSC) /out:spider.exe $(references) $(srcdir)/Spider.cs

mixer.exe: $(srcdir)/Mixer.cs
	$(CSC) /out:mixer.exe $(references) $(srcdir)/Mixer.cs

metadata.exe: $(srcdir)/Metadata.cs
	$(CSC) /out:metadata.exe $(references) $(srcdir)/Metadata.cs

EXTRA_DIST = 				\
	PlayMp3.cs \
	Spider.cs \
	Player.cs \
	Metadata.cs \
	Mixer.cs


