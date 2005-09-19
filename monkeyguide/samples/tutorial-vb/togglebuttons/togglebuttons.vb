imports System
imports Gtk
imports GtkSharp

 Public Module ToggleButtons

	Sub OnDeleteEvent ( ByVal obj as object, ByVal args as DeleteEventArgs )

		Application.Quit()

	End Sub

	Sub OnExitButtonEvent ( ByVal obj as object, ByVal args as EventArgs )

		Application.Quit()

	End Sub

	Public Sub Main ()

		Application.Init()

		Dim window as Window = new Window("Toggle Buttons")
		AddHandler window.DeleteEvent, AddressOf OnDeleteEvent
		window.BorderWidth = 0

		Dim box1 as VBox = new VBox (false, 10)
		window.Add(box1)
		box1.Show()

		Dim box2 as VBox = new VBox (false, 10)
		box2.BorderWidth = 10
		box1.PackStart(box2, true, true, 0)
		box2.Show()

		Dim toggleButton as ToggleButton = new ToggleButton ("Button 1")
		box2.PackStart(toggleButton, true, true, 0)
		toggleButton.Show()

		Dim toggleButton2 as ToggleButton = new ToggleButton ("Button 2")
		toggleButton2.Active = true
		box2.PackStart(toggleButton2, true, true, 0)
		toggleButton2.Show()

		Dim separator as HSeparator = new HSeparator ()
		box1.PackStart(separator, false, true, 0)
		separator.Show()

		Dim box3 as VBox = new VBox (false, 10)
		box3.BorderWidth = 10
		box1.PackStart(box3, false, true, 0)

		Dim button as Button = new Button("Close")
		AddHandler button.Clicked, AddressOf OnExitButtonEvent

		box3.PackStart(button, true, true, 0)
		button.CanDefault = true
		button.GrabDefault()
		button.Show()

		window.ShowAll() 

		Application.Run()

	End Sub

 End Module

