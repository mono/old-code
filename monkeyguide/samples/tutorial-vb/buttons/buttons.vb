imports System
imports Gtk
imports GtkSharp

 Public Module Buttons

	Function XpmLabelBox ( xpmFilename as string, labelText as string ) as Widget

		Dim box as new HBox(false, 0)
		box.BorderWidth = 2

		Dim image as Image = new Image(xpmFilename)

		Dim label as Label = new Label(labelText)

		box.PackStart(image, false, false, 3)
		box.PackStart(label, false, false, 3)

		image.Show()
		label.Show()

		return box

	End Function

	Sub Callback ( ByVal obj as object, ByVal args as EventArgs )

		Console.WriteLine("Helo again - cool button was pressed")

	End Sub

	Sub OnDeleteEvent ( ByVal obj as object, ByVal args as DeleteEventArgs )

		Application.Quit()

	End Sub

	Public Sub Main ()

		Application.Init()

		Dim window as Window = new Window("Pixmap d' Buttons !")
		AddHandler window.DeleteEvent, AddressOf Buttons.OnDeleteEvent
		window.BorderWidth = 10

		Dim button as Button = new Button()
		AddHandler button.Clicked, AddressOf Callback

		Dim box as Widget = XpmLabelBox("info.xpm", "cool button")
		box.Show()

		button.Add(box)

		window.Add(button)

		window.ShowAll()

		Application.Run()

	End Sub

 End Module

