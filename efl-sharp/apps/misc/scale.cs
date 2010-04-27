using Enlightenment.Eblocks;

public class MainClass 
{	
	static Label scale_label;
	
	public static void hscale_value_changed_cb (object o, object ev)
	{
		HScale hscale = (HScale)o;
		
		scale_label.Text = hscale.Value.ToString();
	}
	
	public static void Main (string [] args)
	{
		Application.Init ();
		SetUpGui ();
		Application.Run ();
	}
	
	static void SetUpGui ()
	{
		Window w = new Window ("Scale Test");
		
		VBox vbox = new VBox ();
		
		HScale hscale = new HScale (1, 100, 10);
		hscale.ValueChanged += hscale_value_changed_cb;
		hscale.Value = 50;
		
		scale_label = new Label (hscale.Value.ToString());
				
		vbox.PackStart (scale_label, true, false, 0);
		vbox.PackStart (hscale, true, false, 0);
		
		w.Add (vbox);
		w.SetDefaultSize (160, 120);		
		w.ShowAll ();
	}
}
