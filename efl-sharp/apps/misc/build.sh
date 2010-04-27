mcs eblocks_0.2_test.cs -pkg:efl-sharp  -out:eblocks_0.2_test.exe
mcs gtk_test.cs -pkg:gtk-sharp -out:gtk_test.exe

mcs signup.cs -pkg:efl-sharp  -out:signup.exe
mcs signup_gtk.cs -pkg:gtk-sharp -out:signup_gtk.exe

mcs scale.cs -pkg:efl-sharp -out:scale.exe
mcs scale_gtk.cs -pkg:gtk-sharp -out:scale_gtk.exe

mcs -pkg:efl-sharp viewport.cs -out:viewport.exe
mcs -pkg:gtk-sharp viewport_gtk.cs -out:viewport_gtk.exe

mcs -pkg:efl-sharp scrolledwindow.cs -out:scrolledwindow.exe
mcs -pkg:gtk-sharp scrolledwindow_gtk.cs -out:scrolledwindow_gtk.exe

mcs -pkg:efl-sharp simple.cs -out:simple.exe
mcs -pkg:gtk-sharp simple_gtk.cs -out:simple_gtk.exe

mcs -pkg:efl-sharp eblocks_test.cs -out:eblocks_test.exe
