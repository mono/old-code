imports System
imports Gtk
imports GtkSharp

 Public Module FileSel

	Dim filew as FileSelection

	Sub FileOkSelEvent ( ByVal obj as object, ByVal args as EventArgs )

		Console.WriteLine("{0}\n", filew.Filename)

	End Sub

	Sub OnDeleteEvent ( ByVal obj as object, ByVal args as DeleteEventArgs )

		Application.Quit()

	End Sub

	Sub OnCancelEvent ( ByVal obj as object, ByVal args as EventArgs )

		Application.Quit()

	End Sub

	Public Sub Main ()

		Application.Init()

		filew = new FileSelection("File Selection")

		AddHandler filew.DeleteEvent, AddressOf OnDeleteEvent

		AddHandler filew.OkButton.Clicked, AddressOf FileOkSelEvent

		AddHandler filew.CancelButton.Clicked, AddressOf OnCancelEvent

		filew.Filename = "penguin.png"

		filew.Show()

		Application.Run()

	End Sub

 End Module

