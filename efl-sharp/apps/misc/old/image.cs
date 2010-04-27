using System;
using System.IO;
using Enlightenment.Evas;
using Enlightenment.Ecore;

public class EvasImagePlayground
{
   
  public static void Main(string [] args)
  {
    Enlightenment.Ecore.Canvas.Init();	
	
    Enlightenment.Ecore.Canvas EE  = new Enlightenment.Ecore.Canvas();		
    EE.SoftwareX11New(":0", IntPtr.Zero, 0, 0, 800, 600);
	
       
    Image im = new Image(EE.Get());
    im.Set("/tmp/test.png", null);
	
    int[] pixels = new int[im.Geometry.W * im.Geometry.H];
	
    pixels = im.PixelsGet(1);
	
    // play around here.
	
    im.PixelsSet(pixels);
	
  }      
}
