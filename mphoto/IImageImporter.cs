/*
 * IImageImporter.cs
 *
 * Author(s): Vladimir Vukicevic <vladimir@pobox.com>
 *
 * Copyright (C) 2002  Vladimir Vukicevic
 */

public interface IImageImporter {
	/// <summary>
	/// Returns true if this image importer can import the given URI.
	/// </summary>
	bool CanImportUri (string uri);

	/// <summary>
	/// Imports the given uri into the given Image Collection or Repo
	/// </summary>
	void ImportUri (string uri, IImageCollection coll);

	void ImportUri (string uri, IImageRepository repo);
}

