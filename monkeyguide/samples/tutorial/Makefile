
SUBDIRS = 	helloworld \
		helloworld2 \
		base \
		arrow \
		aspectframe \
		filesel \
		frame \
		label \
		table \
		buttons \
		custombutton \
		menubar \
		radiobuttons \
		scrolledwin \
		textentry \
		eventbox \
		packing \
		rangewidget \
		spinbutton \
		scribble \
		togglebutton \
		gnome_about \
		messagedialog \
		nunit

all:
	list='$(SUBDIRS)'; \
	for subdir in $$list; do \
	  (cd $$subdir && $(MAKE)); \
	done

clean:
	list='$(SUBDIRS)'; \
	for subdir in $$list; do \
	  (cd $$subdir && $(MAKE) clean); \
	done

