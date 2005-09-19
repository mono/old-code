imports System
imports Gtk
imports GtkSharp

 Public Module HelloWorld

	Sub Hello ( ByVal obj as object, ByVal args as EventArgs )

		Console.WriteLine("Hello World")
		Application.Quit()

	End Sub

	Sub OnDeleteEvent ( ByVal obj as object, ByVal args as DeleteEventArgs )

		Console.WriteLine("Delete Event occurred\n")
		Application.Quit()

	End Sub

	Public Sub Main ()

		Application.Init()

			Dim window as Window = new Window("Hello World")
			AddHandler window.DeleteEvent, AddressOf OnDeleteEvent
			window.BorderWidth = 10

			Dim button as Button = new Button("Hello World")
			AddHandler button.Clicked, AddressOf Hello

			window.Add(button)
			window.ShowAll()

		Application.Run()

	End Sub

 End Module

