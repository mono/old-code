MCS=mcs

ewl-sharp=ewl_app.cs ewl_object.cs ewl_widget.cs ewl_container.cs ewl_overlay.cs ewl_embed.cs ewl_window.cs ewl_entry.cs ewl_text.cs ewl_event.cs ewl_enums.cs

libs=-r ../Evas/evas-sharp.dll

all:

	$(MCS) -unsafe -t:library -o ewl-sharp.dll $(ewl-sharp) $(libs)

	$(MCS) -g -o ewl.exe \
	-r ewl-sharp.dll ewl.cs

