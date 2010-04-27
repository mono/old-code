namespace Monotalk.SourceView
{
    using System;
    using System.IO;
    using System.Collections;
    using System.Text.RegularExpressions;
    using GConf;

    public class Config
    {
	public Pattern[] patterns;
	public Style[] styles;

	public const string GCONF_DIR = "/apps/monotalk/sourceview";
	public const string GCONF_PATH = GCONF_DIR + "/";
        public static GConf.Client gconf      = new GConf.Client ();
/*
	public string [] patterns
	{
	    get
	    {
		string [] result = new string [styles.Length];

		for (int i=0; i<styles.Length; i++)
		    result[i] = styles[i].pattern;

		return result;
	    }
	}


	public string [] spans
	{
	    get
	    {
		string [] result = new string [styles.Length];

		for (int i=0; i<styles.Length; i++)
		    result[i] = styles[i].spanto;

		return result;
	    }
	}
*/

	public Config ()
	{
		Read ();
	}

	public void Read ()
        {
            string keywords, spanto, sdelim, edelim, signore;
            ArrayList stylesList = new ArrayList();
            ArrayList patternsList = new ArrayList();
	    Style style;

            string names = (string) gconf.Get (GCONF_PATH + "styles");

            foreach (string name in names.Split(' ')) {
		    if (name.Length == 0)
			    continue;

		    keywords = (string) gconf.Get (GCONF_PATH + name + "/keywords");

		    style = new Style (GCONF_PATH + name);
		    stylesList.Add (style);

		    sdelim   = (string) gconf.Get (GCONF_PATH + name + "/sdelim");
		    edelim   = (string) gconf.Get (GCONF_PATH + name + "/edelim");

		    try {
			    signore  = (string) gconf.Get (GCONF_PATH + name + "/signore");
		    } catch (NoSuchKeyException e) {
			    signore  = null;
		    }

		    try  {
			    spanto = (string) gconf.Get (GCONF_PATH + name + "/spanto");
		    } catch (NoSuchKeyException e)  {
			    spanto = null;
		    }

		    foreach (string key in keywords.Split(' '))
		            patternsList.Add (new Pattern (style, GCONF_PATH + name, key, spanto, sdelim, edelim, signore));
            }

            patterns = (Pattern[]) patternsList.ToArray (typeof (Pattern));
	    styles = (Style[]) stylesList.ToArray  (typeof (Style));
        }


        void Save (string name, string keywords, string spanto, string color, string weight, string sdelim, string edelim, string signore)
        {
            gconf.Set (GCONF_PATH + name + "/keywords", keywords);
            gconf.Set (GCONF_PATH + name + "/color", color);
            gconf.Set (GCONF_PATH + name + "/weight", weight);
            gconf.Set (GCONF_PATH + name + "/sdelim", sdelim);
            gconf.Set (GCONF_PATH + name + "/edelim", edelim);

	    if ( signore != null )
		gconf.Set (GCONF_PATH + name + "/signore", signore);

            if ( spanto != null )
                gconf.Set (GCONF_PATH + name + "/spanto", spanto);

            string style; 

            try 
            {
                style = (string) gconf.Get (GCONF_PATH + "styles");
            }
            catch (NoSuchKeyException e)
            {
                gconf.Set (GCONF_PATH + "styles", name);
                return;
            }
            
            foreach (string s in style.Split(' '))
                if ( String.Compare(s, name) == 0) return;

            gconf.Set (GCONF_PATH + "styles", style + " " + name);   
        }
   }
}
