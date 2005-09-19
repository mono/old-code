MCS=mcs
REFERENCES= glib-sharp gdk-sharp gtk-sharp gnome-sharp gda-sharp glade-sharp gnomedb-sharp System.Data System.Drawing

###

REFS= $(addprefix /r:, $(REFERENCES))

all:
	$(MCS) $(REFS) gnomedb-sample.cs

clean:
	rm -f *.exe *.pdb
