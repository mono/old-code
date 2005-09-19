imports System
imports Gtk
imports GtkSharp

 Public Module HelloWorld2

	Sub Callback ( ByVal obj as object, ByVal args as EventArgs )

		Dim mybutton as Button
		mybutton =  CType(obj, Button)
		Console.WriteLine("Hello again - {0} was pressed", mybutton.Label)

	End Sub

	Sub OnDeleteEvent ( ByVal obj as object, ByVal args as DeleteEventArgs )

		Application.Quit()

	End Sub

	Public Sub Main ()

		Application.Init()

		Dim window as Window = new Window("HelloWorld")
		window.Title = "Hello Buttons !"
		AddHandler window.DeleteEvent, AddressOf OnDeleteEvent
		window.BorderWidth = 10

		Dim box1 as HBox = new HBox(false, 0)
		window.Add(box1)

		Dim button1 as ToggleButton = new ToggleButton("Button 1")
		AddHandler button1.Clicked, AddressOf Callback
		box1.PackStart(button1, true, true, 0)
		button1.Show()

		Dim button2 as Button = new Button("Button 2")
		AddHandler button2.Clicked, AddressOf Callback
		box1.PackStart(button2, true, true, 0)

		window.ShowAll()

		Application.Run()

	End Sub

 End Module

