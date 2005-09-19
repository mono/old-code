imports System
imports Gtk
imports GtkSharp

 Public Module Base

	Public Sub Main ()

		Application.Init()

		Dim window as Window = new Window("base")
		window.Show()

		Application.Run()

	End Sub

 End Module
