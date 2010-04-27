using System;
using System.Collections;
using System.Collections.Generic;
using System.Xml;

using Mono.Build;
using Mono.Build.Bundling;

namespace Monkeywrench.Compiler {

    public class XmlGraphSerializer {

	private XmlGraphSerializer () {}

	public static void Write (GraphBuilder gb, string file)
	{
	    XmlTextWriter tw = new XmlTextWriter (file, System.Text.Encoding.UTF8);

	    if (DebugOutput)
		tw.Formatting = Formatting.Indented;

	    Write (gb, tw);
	}

	public static void Write (GraphBuilder gb, XmlTextWriter tw)
	{
	    tw.WriteStartElement ("mbuild-graph");

	    WriteDependents (gb, tw);
	    WriteProviders (gb, tw);
	    WriteTargets (gb, tw);
	    WriteTags (gb, tw);
	    WriteProject (gb, tw);

	    tw.WriteEndElement ();
	    tw.Close ();
	}

	public static bool DebugOutput = false;

	// Dependents

	static void WriteDependents (GraphBuilder gb, XmlTextWriter tw)
	{
	    tw.WriteStartElement ("dependents");

	    foreach (DependentItemInfo dii in gb.GetDependentFiles ()) {
		tw.WriteStartElement ("file");
		WriteDependentInterior (dii, tw);
		tw.WriteEndElement ();
	    }

	    foreach (DependentItemInfo dii in gb.GetDependentBundles ()) {
		tw.WriteStartElement ("bundle");
		WriteDependentInterior (dii, tw);
		tw.WriteEndElement ();
	    }

	    tw.WriteEndElement ();
	}

	static void WriteDependentInterior (DependentItemInfo dii, XmlTextWriter tw)
	{
	    tw.WriteAttributeString ("name", dii.Name);
	    dii.Fingerprint.ExportXml (tw, "fingerprint");
	}

	// Providers

	static void WriteProviders (GraphBuilder gb, XmlTextWriter tw)
	{
	    tw.WriteStartElement ("providers");

	    foreach (WrenchProvider wp in gb.Providers) {
		tw.WriteStartElement ("p");

		tw.WriteAttributeString ("id", wp.Id.ToString ());
		tw.WriteAttributeString ("basis", wp.Basis);
		tw.WriteAttributeString ("decl_loc", wp.DeclarationLoc);
		tw.WriteAttributeString ("ntargs", wp.NumTargets.ToString ());

		tw.WriteEndElement ();
	    }

	    tw.WriteEndElement ();
	}

	// Targets

	static void WriteTargets (GraphBuilder gb, XmlTextWriter tw)
	{
	    tw.WriteStartElement ("targets");

	    foreach (WrenchTarget wt in gb.GetTargets ()) {
		tw.WriteStartElement ("t");

		tw.WriteAttributeString ("id", wt.Id.ToString ());
		tw.WriteAttributeString ("name", wt.Name);
		tw.WriteAttributeString ("rule", wt.RuleType.AssemblyQualifiedName);

		tw.WriteStartElement ("deps");
		wt.VisitDependencies (new WriteVisitor (tw));
		tw.WriteEndElement ();

		tw.WriteStartElement ("tags");
		WriteTargetTags (gb, wt, tw);
		tw.WriteEndElement ();

		tw.WriteEndElement ();
	    }

	    tw.WriteEndElement ();
	}

	static void WriteTargetTags (GraphBuilder gb, WrenchTarget wt, XmlTextWriter tw)
	{
	    foreach (KeyValuePair<string,SingleValue<int>> kvp in wt.IdTags) {
		int tagid = gb.GetTagId (kvp.Key);
		
		if (tagid < 0)
		    throw ExHelp.Argument ("tag", "Invalid tag name {0}", kvp.Key);
		
		if (kvp.Value.IsResult) {
		    tw.WriteStartElement ("rt");
		    tw.WriteAttributeString ("id", tagid.ToString ());
		    ((Result) kvp.Value).ExportXml (tw, "r");
		} else {
		    tw.WriteStartElement ("tt");
		    tw.WriteAttributeString ("id", tagid.ToString ());
		    tw.WriteAttributeString ("target", ((int) kvp.Value).ToString ());
		}
		
		tw.WriteEndElement ();
	    }
	}

	class WriteVisitor : IDependencyVisitor {
	    XmlTextWriter tw;

	    public WriteVisitor (XmlTextWriter tw)
	    {
		this.tw = tw;
	    }

	    bool WriteResult (string elt, int aid, Result r)
	    {
		tw.WriteStartElement (elt);
		if (aid >= 0)
		    tw.WriteAttributeString ("arg", aid.ToString ());
		r.ExportXml (tw, "r");
		tw.WriteEndElement ();
		return false;
	    }

	    bool WriteTarget (string elt, int aid, WrenchTarget targ)
	    {
		tw.WriteStartElement (elt);
		if (aid >= 0)
		    tw.WriteAttributeString ("arg", aid.ToString ());
		tw.WriteAttributeString ("id", targ.Id.ToString ());
		tw.WriteEndElement ();
		return false;
	    }

	    bool WriteValue (string elt, int aid, SingleValue<TargetBuilder> sv)
	    {
		if (sv.IsTarget)
		    return WriteTarget (elt + "t", aid, (WrenchTarget) (TargetBuilder) sv);
		else
		    return WriteResult (elt + "r", aid, (Result) sv);
	    }

	    // Deps

	    public bool VisitUnnamed (SingleValue<TargetBuilder> sv)
	    {
		return WriteValue ("u", -1, sv);
	    }

	    public bool VisitNamed (int aid, SingleValue<TargetBuilder> sv)
	    {
		return WriteValue ("n", aid, sv);
	    }

	    public bool VisitDefaultOrdered (SingleValue<TargetBuilder> sv)
	    {
		return WriteValue ("do", -1, sv);
	    }

	    public bool VisitDefaultValue (int aid, SingleValue<TargetBuilder> sv)
	    {
		return WriteValue ("D", aid, sv);
	    }
	}

	// Tags

	static void WriteTags (GraphBuilder gb, XmlTextWriter tw)
	{
	    tw.WriteStartElement ("tags");

	    foreach (string tag in gb.GetTags ()) {
		tw.WriteStartElement ("tag");
		tw.WriteAttributeString ("name", tag);
		tw.WriteAttributeString ("id", gb.GetTagId (tag).ToString ());
		tw.WriteEndElement ();
	    }

	    tw.WriteEndElement ();
	}

	// Project

	static void WriteProject (GraphBuilder gb, XmlTextWriter tw)
	{
	    tw.WriteStartElement ("project-info");

	    ProjectInfo pinfo = gb.PInfo;

	    tw.WriteAttributeString ("name", pinfo.Name);
	    tw.WriteAttributeString ("version", pinfo.Version);
	    tw.WriteAttributeString ("compat-code", pinfo.CompatCode);
	    tw.WriteAttributeString ("buildfile-name", pinfo.BuildfileName);

	    foreach (System.Reflection.AssemblyName aname in pinfo.Refs) {
		tw.WriteStartElement ("ref");
		tw.WriteAttributeString ("name", aname.FullName);
		tw.WriteEndElement ();
	    }

	    tw.WriteEndElement ();
	}
    }
}
