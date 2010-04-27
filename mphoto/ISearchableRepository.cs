/*
 * ISearchableRepository.cs
 *
 * Author(s): Vladimir Vukicevic <vladimir@pobox.com>
 *
 * Copyright (C) 2002  Vladimir Vukicevic
 */

using System.Collections;

public interface ISearchableRepository
{
	string[] Keywords { get; }

	bool IsKeyword (string keyword);
	void AddKeyword (string keyword);

	string[] FindImagesByKeyword (string[] keywords);

	// I really want to do this on the ImageItem, but it
	// has no idea of what it is in the database, etc.
	// maybe ImageItem should be a virtual class that is
	// overridden, so you can do keywords and stuff directly
	// to the image?
	void AddImageKeyword (string imageid, string keyword);
	void RemoveImageKeyword (string imageid, string keyword);

	string[] GetImageKeywords (string imageid);
	void SetImageKeywords (string imageid, string[] keywords);
}

