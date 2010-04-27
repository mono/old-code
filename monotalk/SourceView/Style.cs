namespace Monotalk.SourceView
{
    using GConf;

    public class Style
    {
	public string path;
        public string color;
        public string weight;

        public Style (string Path)
        {
		path = Path;
		Read ();
        }

	public void Read ()
	{
                color    = (string) Config.gconf.Get (path + "/color");
                weight   = (string) Config.gconf.Get (path + "/weight");
	}
    }
}
