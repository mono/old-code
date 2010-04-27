/*
 * IIconListAdapter.cs
 *
 * Author(s): Vladimir Vukicevic <vladimir@pobox.com>
 *
 * Copyright (C) 2002  Vladimir Vukicevic
 */

using System;
using Gdk;

public interface IIconListAdapter {
	int Count { get; }
	Pixbuf this[int index] { get; }
	IconList IconList { get; set; }
	void DeleteItem(int index);
	string GetFullFilename (int index);
	string GetImageID (int index);
}

