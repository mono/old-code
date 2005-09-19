imports System
imports Gtk
imports GtkSharp

 Public Module CheckButtons

	Sub OnDeleteEvent ( BYVal obj as object, ByVal args as DeleteEventArgs )

		Application.Quit()

	End Sub

	Sub OnClickedEvent ( ByVal obj as object, ByVal args as EventArgs )

		if ( (CType(obj, CheckButton)).Active ) then
			Console.WriteLine("CheckButton clicked, I'm activating")
		else
			Console.WriteLine("CheckButton clicked, I'm desactivating")
		end if

	End Sub

	Public Sub Main ()

		Application.Init()

		Dim hbox as HBox = new HBox (false, 0)
		hbox.BorderWidth = 2

		Dim cb1 as CheckButton = new CheckButton("CheckButton 1")
		AddHandler cb1.Clicked, AddressOf OnClickedEvent

		Dim cb2 as CheckButton = new CheckButton("CheckButton 2")
		AddHandler cb2.Clicked, AddressOf OnClickedEvent

		hbox.PackStart(cb1, false, false, 3)
		hbox.PackStart(cb2, false, false, 3)

		Dim window as Window = new Window("Check Buttons")
		window.BorderWidth = 10
		AddHandler window.DeleteEvent, AddressOf OnDeleteEvent

		window.Add(hbox)
		window.ShowAll()

		Application.Run()

	End Sub

 End Module
