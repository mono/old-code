//
// Mono.AppServer.ApplicationLog
//
// Authors:
//   Brian Ritchie (brianlritchie@hotmail.com)
//
// Copyright (C) Brian Ritchie, 2003
//
using System;
using System.IO;
using System.Data;

namespace Mono.AppServer
{
  [Serializable]
  public class ApplicationLog
  {
    private string file;

    public ApplicationLog(string File)
    {
      Console.WriteLine("- Log file: {0}",File);
      this.file=File;
    }

    private void WriteFile(string type, string s)
    {
      StreamWriter writer=File.AppendText(file);
      writer.WriteLine("{0}|{1}|{2}",DateTime.Now,type,s);
      writer.Close();
    }

    public void WriteLine(string type,string s)
    {
      WriteFile(type,s);
    }

    public void WriteLine(string s)
    {
      WriteLine("Application",s);
    }

    public DataSet GetRecentEntriesDataSet()
    {
      const int BufSize=10000;
      FileStream fs=new FileStream(file, FileMode.Open, FileAccess.Read, FileShare.Read);
      if (fs.Length>BufSize)
        fs.Seek(fs.Length-BufSize,SeekOrigin.Begin);
      StreamReader reader=new StreamReader(fs);
      reader.ReadLine();
      DataSet ds=new DataSet();
      DataTable dt=new DataTable();
      ds.Tables.Add(dt);
      dt.Columns.Add("Date",typeof(DateTime));
      dt.Columns.Add("Type",typeof(string));
      dt.Columns.Add("Description",typeof(string));
      while (reader.Peek()>0)
      {
        string[] s=reader.ReadLine().Split(new char[1] {'|'} );
        DataRow r=dt.NewRow();
        dt.Rows.Add(r);
        r["Date"]=DateTime.Parse(s[0]);
        r["Type"]=s[1];
        r["Description"]=s[2];
      }
      fs.Close();
      return ds;
    }		
  }
}