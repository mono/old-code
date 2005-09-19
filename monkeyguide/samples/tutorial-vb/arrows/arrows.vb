imports System
imports Gtk
imports GtkSharp

 Public Module Arrows

	Sub OnDeleteEvent ( ByVal obj as object, ByVal args as DeleteEventArgs )

		Application.Quit()

	End Sub

	Function CreateArrowButton ( arrowType as ArrowType, shadowType as ShadowType ) as Widget

		Dim button as Button = new Button()
		Dim arrow as Arrow = new Arrow(arrowType, shadowType)

		button.Add(arrow)

		button.Show()
		arrow.Show()

		return button

	End Function

	Public Sub Main ()

		Application.Init()

		Dim window as Window = new Window ("Arrow Buttons")
		AddHandler window.DeleteEvent, AddressOf OnDeleteEvent
		window.BorderWidth = 10

		Dim box as HBox = new HBox (false, 0)
		box.BorderWidth = 2
		window.Add(box)
		box.Show()

		'
		' The constructor functions should work with the ShadowType.In argument
		' but it doesn't ( is In a reserved work in VB ? )
		'

		Dim button1 as Widget = CreateArrowButton(ArrowType.Up, ShadowType.Out )
		box.PackStart(button1, false, false, 3)

		Dim button2 as Widget = CreateArrowButton(ArrowType.Down, ShadowType.Out )
		box.PackStart(button2, false, false, 3)

		Dim button3 as Widget = CreateArrowButton(ArrowType.Left, ShadowType.EtchedIn )
		box.PackStart(button3, false, false, 3)

		Dim button4 as Widget = CreateArrowButton(ArrowType.Right, ShadowType.EtchedOut )
		box.PackStart(button4, false, false, 3)

		window.ShowAll()

		Application.Run()

	End Sub

 End Module

