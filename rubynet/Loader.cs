/*
 Copyright(C) 2001-2002 arton

 Permission is granted for use, copying, modification, distribution,
 and distribution of modified versions of this work as long as the
 above copyright notice is included.
*/
using System;
using System.Diagnostics;
using System.Collections;
using System.IO;
using System.Text;
using System.Reflection;
using System.Reflection.Emit;
using System.Security;

namespace NETRuby
{
    internal class Loader
    {
        private Loader(NetRuby rb)
        {
            ruby = rb;
            loadPath = new RArray(rb, true);
            features = new RArray(rb, true);

            string baseDir = AppDomain.CurrentDomain.BaseDirectory;
            loadPath.Add(baseDir);
            loadPath.Add(Path.Combine(baseDir, "lib"));
            loadPath.Add(AppDomain.CurrentDomain.BaseDirectory);
            string lp = Environment.GetEnvironmentVariable("RUBYLIB");
            if (lp != null)
            {
                string[] sp = lp.Split(new char[] { Path.PathSeparator });
                for (int i = 0; i < sp.Length; i++)
                {
                    loadPath.Add(Environment.ExpandEnvironmentVariables(sp[i]));
                }
            }
            /*
            if (rb.SafeLevel == 0)
            {
            */
                loadPath.Add(".");
            /*
            }
            */
        }
        NetRuby ruby;
        RArray loadPath;
        RArray features;
        static string[] exts = { ".rb", ".dll", ".so", ".exe" };

        internal void IncPush(string lp)
        {
            string baseDir = AppDomain.CurrentDomain.BaseDirectory;
            string[] sp = lp.Split(new char[] { Path.PathSeparator });
            for (int i = 0; i < sp.Length; i++)
            {
                if (loadPath.Contains(sp[i]) == false)
                    loadPath.Add(Environment.ExpandEnvironmentVariables(sp[i]));
            }
        }
        internal RArray LoadPath
        {
            get { return loadPath; }
        }
        internal RArray Features
        {
            get { return features; }
        }
        internal bool Require(string s)
        {
            RThread th = ruby.GetCurrentContext();
            ////ruby.CheckSafeString(th, s);
            s = s.Replace('/', Path.DirectorySeparatorChar);
            string fname = null;
            bool script = true;
            string ext = Path.GetExtension(s);
            if (ext != String.Empty)
            {
                if (String.Compare(ext, ".rb", true) == 0)
                {
                    fname = FindFile(s);
                }
                else if (String.Compare(ext, ".dll", true) == 0
                    || String.Compare(ext, ".so", true) == 0)
                {
                    fname = FindFile(s);
                    script = false;
                }
            }
            else
            {
                for (int i = 0; i < exts.Length; i++)
                {
                    fname = FindFile(s + exts[i]);
                    if (fname != null)
                    {
                        if (i != 0)
                        {
                            script = false;
                        }
                        break;
                    }
                }
            }
            if (fname == null)
                throw new eLoadError("No such file to load -- " + s);

            string fileName = Path.GetFileName(fname);
            if (featureCheck(fileName)) return false;
        
            if (script == false)
            {
                try
                {
                    AssemblyName asm = AssemblyName.GetAssemblyName(fname);
                    Assembly a = Assembly.Load(asm);
                    Type[] tps = a.GetTypes();
                    foreach (Type t in tps)
                    {
                        AddType(t, th);
                    }
                    features.Add(fileName);
                }
                catch (FileLoadException)
                {
                    return false;
                }
                catch (BadImageFormatException)
                {
                    throw new eLoadError("Not valid file image to load -- " + fname);
                }
            }
            else
            {
                ////int oldSafeLevel = th.safeLevel;
                ////th.safeLevel = 0;
                ////th.PushTag(Tag.TAG.PROT_NONE);
                try
                {
                    ruby.Load(fname, false);
                    features.Add(fileName);
                }
                catch (Exception e)
                {
#if _DEBUG
                    System.Console.Error.WriteLine(e.Message);
                    System.Console.Error.WriteLine(e.StackTrace);
#endif        
                    throw e;
                }
                finally
                {
                    ////th.PopTag(true);
                    ////th.safeLevel = oldSafeLevel;
                }
            }
            return true;
        }

        internal string FindFile(string fname)
        {
            fname = Environment.ExpandEnvironmentVariables(fname);
            if (Path.IsPathRooted(fname))
            {
/*            
                if (ruby.SafeLevel >= 2 && PathCheck(fname) == false)
                    throw new SecurityException("loading from unsafe file " + fname);
*/
                return (File.Exists(fname)) ? fname : null;
            }
            ArrayList ar = loadPath.ArrayList;
            string result = null;
            lock (ar.SyncRoot)
            {
                foreach (object dir in ar)
                {
                    string path = Path.Combine(dir.ToString(), fname);
                    if (File.Exists(path))
                    {
                        result = path;
                        break;
                    }
                }
            }
            return result;
        }

        bool featureCheck(string fpath)
        {
            ArrayList ar = features.ArrayList;
            bool result = false;
            lock (ar.SyncRoot)
            {
                foreach (string s in ar)
                {
                    if (String.Compare(s, fpath, true) == 0)
                    {
                        result = true;
                        break;
                    }
                }
            }
            return result;
        }
        bool PathCheck(string fname)
        {
            string path = Path.GetDirectoryName(fname);
            ArrayList ar = loadPath.ArrayList;
            bool result = false;
            lock (ar.SyncRoot)
            {
                foreach (string s in ar)
                {
                    if (String.Compare(s, path, true) == 0)
                    {
                        result = true;
                        break;
                    }
                }
            }
            return result;
        }
        bool AddType(Type tp, RThread th)
        {
            BindingFlags bf = BindingFlags.Public | BindingFlags.NonPublic
                | BindingFlags.Static | BindingFlags.InvokeMethod;
            MethodInfo mi = tp.GetMethod("Init", bf, null,
                                         new Type[] {typeof(NetRuby)}, null);
#if REQUIRE_DEBUG
            System.Console.WriteLine("type:{0} has {1}", tp.ToString(), (mi == null) ? "null" : mi.ToString());
#endif
/*
            // NETRuby's extended libray            
            if (mi != null && mi.IsStatic)
            {
                Scope.ScopeMode vm = th.PushScope();
                th.PushTag(Tag.TAG.PROT_NONE);
                Tag.TAG state = Tag.TAG.EMPTY;
                try
                {
                    mi.Invoke(null, new object[] {ruby});
                }
                catch (eTagJump ej)
                {
                    state = ej.state;
                }
                catch (Exception e)
                {
                    th.errInfo = new RException(ruby, e);
                    state = Tag.TAG.RAISE;
                }
                th.PopTag(true);
                th.PopScope(vm);
                if (state != Tag.TAG.EMPTY)
                {
                    th.TagJump(state);
                }
            }
            else
            {
                ruby.cDotNet.AddFrameworkClass(tp);
            }
*/            
            return false;
        }

        object require(RBasic r, params object[] args)
        {
            string s = RString.AsString(ruby, args[0]);
            return Require(s);
        }
        object f_load(RBasic r, params object[] args)
        {
            object[] argv = new object[2];
            ruby.ScanArgs(args, "11", argv);
            ruby.Load(argv[0], RBasic.RTest(argv[1]));
            return true;
        }
        object lpGetter(uint id, GlobalEntry gb, NetRuby rb)
        {
            return loadPath;
        }
        object ftGetter(uint id, GlobalEntry gb, NetRuby rb)
        {
            return features;
        }
        internal static Loader Init(NetRuby rb)
        {
            Loader ld = new Loader(rb);
            rb.DefineReadonlyVariable("$-I", null, new GlobalEntry.Getter(ld.lpGetter));
            rb.DefineReadonlyVariable("$:", null, new GlobalEntry.Getter(ld.lpGetter));
            rb.DefineReadonlyVariable("$LOAD_PATH", null, new GlobalEntry.Getter(ld.lpGetter));
            rb.DefineReadonlyVariable("$\"", null, new GlobalEntry.Getter(ld.ftGetter));
            rb.DefineGlobalFunction("load", new RBasic.RMethod(ld.f_load), -1);
            rb.DefineGlobalFunction("require", new RBasic.RMethod(ld.require), 1);
            return ld;
        }
    }
}
