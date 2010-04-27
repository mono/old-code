/*
 * DbProvider.cs
 *
 * Handles all the db interaction for the various Db* bits.
 *
 * Author(s): Vladimir Vukicevic <vladimir@pobox.com>
 *
 * Copyright (C) 2002  Vladimir Vukicevic
 */
 
//#define SQLDEBUG
#define USE_SQLITE
 
using System;
using System.Data;
#if USE_SQLITE
using Mono.Data.SqliteClient;
#else
using System.Data.OleDb;
#endif
using System.Collections;
using System.IO;
using System.Threading;
using System.Text;
 
internal class DbProvider {
	static ArrayList providers;
 
	static int db_uses_strings = -1; /* FIXME!!!! this should be per-provider/connection; instead it is global */
 
	static DbProvider () { 
		providers = new ArrayList ();
	}
 
	private DbProvider () {
        
	}
 
	internal static IDbConnection GetProvider (string s) {
		foreach (IDbConnection dc in providers) {
			if (dc.ConnectionString == s) {
				return dc;
			}
		}

#if USE_SQLITE
		IDbConnection dbConn = new SqliteConnection ();
#else
		OleDbConnection dbConn = new OleDbConnection ();
#endif
		dbConn.ConnectionString = s;
		dbConn.Open ();
 
		if (!VerifyDatabase (dbConn)) {
			throw new InvalidOperationException ();
		}
 
		providers.Add (dbConn);
 
		return dbConn;
	}
 
	internal static void CloseProviders () {
		foreach (IDbConnection p in providers) {
			p.Close ();
		}
	}
 
	internal class SqlHelper {
		private SqlHelper () {
		}
 
		static private string sqlBug (string s) {
#if SQLDEBUG
			Console.WriteLine ("SQL: " + s);
#endif
			return s;
		}
 
		static private string sqlString (string s) {
			return s.Replace ("'", "''");
		}
 
		static internal string sqlDbCountImages () {
			return sqlBug ("SELECT COUNT(*) FROM mp_images");
		}
 
		static internal string sqlDbCountImagesInCollection (int c_id) {
			return sqlBug ("SELECT COUNT(*) FROM mp_images, mp_collection_images " +
				       "WHERE mp_images.i_id = mp_collection_images.i_id " +
				       "      AND mp_collection_images.c_id = " + c_id);
		}
 
		static internal string sqlDbGetImageIds () {
			return sqlBug ("SELECT i_id FROM mp_images");
		}
 
		static internal string sqlDbGetImageIdsInCollection (int c_id) {
			return sqlBug ("SELECT mp_images.i_id FROM mp_images, mp_collection_images " +
				       "WHERE mp_images.i_id = mp_collection_images.i_id " +
				       "      AND mp_collection_images.c_id = " + c_id);
		}
 
		static internal string sqlDbGetImageThumbnailFilename (int i_id) {
			return sqlBug ("SELECT thumbnail_file FROM mp_thumbnails WHERE i_id = " + i_id);
		}
 
		static internal string sqlDbGetImageFilename (int i_id) {
			return sqlBug ("SELECT i_dirname || '/' || i_filename FROM mp_images WHERE i_id = " + i_id);
		}
 
		static internal string sqlDbGetImageInfo (int i_id) {
			return sqlBug ("SELECT * FROM mp_images WHERE i_id = " + i_id);
		}
 
		static internal string sqlDbGetThumbnail (int i_id) {
			return sqlBug ("SELECT thumbnail_file FROM mp_thumbnails WHERE i_id = " + i_id);
		}
 
		static internal string sqlDbDeleteThumbnail (int i_id) {
			return sqlBug ("DELETE FROM mp_thumbnails WHERE i_id = " + i_id);
		}
 
		static internal string sqlDbSetThumbnailFilename (int i_id, string t_file) {
			// FIXME -- t_file needs to be quoted
			return sqlBug ("INSERT INTO mp_thumbnails (i_id, thumbnail_file) VALUES (" + i_id + ", '" + sqlString (t_file) + "')");
		}
 
		static internal string sqlDbGetCollectionIds () {
			return sqlBug ("SELECT c_id FROM mp_collections");
		}
        
		static internal string sqlDbGetCollectionInfo (int c_id) {
			return sqlBug ("SELECT * FROM mp_collections WHERE c_id = " + c_id);
		}
        
		static internal string sqlDbSetCollectionName (int c_id, string name) {
			// FIXME -- quote string
			return sqlBug ("UPDATE mp_collections SET c_name = '" + sqlString (name) + "' WHERE c_id = " + c_id);
		}
 
		static internal string sqlDbAddImageToCollection (int c_id, int i_id) {
			return sqlBug ("INSERT INTO mp_collection_images (c_id, i_id) VALUES (" + c_id + ", " + i_id + ")");
		}
 
		static internal string sqlDbDeleteImageFromCollection (int c_id, int i_id) {
			return sqlBug ("DELETE FROM mp_collection_images WHERE c_id = " + c_id + " AND i_id = " + i_id);
		}
 
		static internal string sqlDbGetImageCollections (int i_id) {
			return sqlBug ("SELECT c_id FROM mp_collection_images WHERE i_id = " + i_id);
		}
 
		static internal string sqlDbDeleteImage (int i_id) {
			return sqlBug ("DELETE FROM mp_images where i_id = " + i_id);
		}
 
		static internal string sqlDbAddImage (string filename, string dirname, int width, int height, string caption, int filesize) {
			return sqlBug ("INSERT INTO mp_images (i_filename, i_dirname, i_width, i_height, i_caption, i_filesize) VALUES (" +
				       "'" + sqlString (filename) + "'," +
				       "'" + sqlString (dirname) + "'," +
				       width + "," +
				       height + "," +
				       (caption == null ? "NULL" : "'" + sqlString (caption) + "'") + "," +
				       filesize +
				       ")");
		}
 
		static internal string sqlDbNewCollection (string name) {
			return sqlBug ("INSERT INTO mp_collections (c_name) VALUES ('" + sqlString (name) + "')");
		}
 
		static internal string sqlDbGetCollectionByName (string name) {
			return sqlBug ("SELECT c_id FROM mp_collections WHERE c_name = '" + sqlString (name) + "'");
		}

		static internal string sqlDbGetKeywords () {
			return sqlBug ("SELECT * FROM mp_keywords");
		}

		static internal string sqlDbAddKeyword (string kw) {
			return sqlBug ("INSERT INTO mp_keywords (k_name) VALUES ('" + sqlString (kw) + "')");
		}

		static internal string sqlDbGetKeyword (string kw) {
			return sqlBug ("SELECT k_id FROM mp_keywords WHERE k_name = '" + sqlString (kw) + "'");
		}

		static internal string sqlDbDeleteKeyword (int k_id) {
			return sqlBug ("DELETE FROM mp_keywords WHERE k_id = " + k_id);
		}

		static internal string sqlDbGetImageKeywords (int i_id) {
			return sqlBug ("SELECT k_id FROM mp_image_keywords WHERE i_id = " + i_id);
		}

		static internal string sqlDbDeleteImageKeywords (int i_id) {
			return sqlBug ("DELETE FROM mp_image_keywords WHERE i_id = " + i_id);
		}

		static internal string sqlDbGetAllImageKeywords () {
			return sqlBug ("SELECT * FROM mp_image_keywords");
		}

		static internal string sqlDbAddImageKeyword (int i_id, int k_id) {
			return sqlBug ("INSERT INTO mp_image_keywords (i_id, k_id) VALUES (" + i_id + "," + k_id + ")");
		}

		static internal string sqlDbDeleteImageKeyword (int i_id, int k_id) {
			return sqlBug ("DELETE FROM mp_image_keywords WHERE i_id = " + i_id + " AND k_id = " + k_id);
		}

		static internal string sqlDbSearchImagesForKeywords (int[] k_ids) {
			StringBuilder k_id_str = new System.Text.StringBuilder (k_ids.Length * 3);
			foreach (int k_id in k_ids)
			k_id_str.Append (k_id + ",");
			// nuke final comma
			k_id_str[k_id_str.Length - 1] = ' ';
			return sqlBug ("SELECT i_id FROM mp_image_keywords WHERE k_id IN (" + k_id_str.ToString () + ")");
		}
	}
 
	// YUCK
	static internal bool DbUsesStrings (IDataReader r, int whichcol) {
		if (db_uses_strings == -1) {
			string dn = r.GetDataTypeName (whichcol);
			if (dn == "string" || dn == "System.String" || dn == "text") {
				Console.WriteLine ("*** Database uses strings for all data types");
				db_uses_strings = 1;
				return true;
			} else {
				Console.WriteLine ("*** Database does NOT use strings for all data types (type is " + dn + ")");
				db_uses_strings = 0;
				return false;
			}
		}
 
		return (db_uses_strings == 1);
	}
 
	// make sure this database has the necessary mphoto tables
	public static bool VerifyDatabase (IDbConnection dbc) {
		IDbCommand cmd = dbc.CreateCommand ();
		string mp_version = null;

		try {
			cmd.CommandText = "SELECT mp_db_version FROM mp_version";
			mp_version = (string) cmd.ExecuteScalar ();
		} catch {
			// ignore
		}
 
		if (mp_version == null) {
			// this database is not set up
			// we do the set up now

			Console.WriteLine ("** Initializing database");
			System.Reflection.Assembly assembly = System.Reflection.Assembly.GetCallingAssembly ();
			System.IO.Stream stream;
              
			StringBuilder cmdstring = new StringBuilder (8192);
			byte[] buf = new byte[8192];
			int n;

			using (stream = assembly.GetManifestResourceStream ("mphoto-sqlite.sql")) {
				StreamReader sreader = new StreamReader (stream);
				cmd.CommandText = sreader.ReadToEnd ();

				cmd.ExecuteNonQuery ();
			}

			Console.WriteLine ("** done");
			// db should be valid now
			return true;
		} else if (mp_version != "1") {
			// this database is too old or too new
			throw new InvalidOperationException ("Database version not correct: got " + mp_version + " expected " + "1");
		}

		Console.WriteLine ("** DB is valid");
		return true;
	}
}

