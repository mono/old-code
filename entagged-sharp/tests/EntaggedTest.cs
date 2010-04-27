/***************************************************************************
 *  Copyright 2005 Daniel Drake <dsd@gentoo.org>
 ****************************************************************************/

/*  THIS FILE IS LICENSED UNDER THE MIT LICENSE AS OUTLINED IMMEDIATELY BELOW: 
 *
 *  Permission is hereby granted, free of charge, to any person obtaining a
 *  copy of this software and associated documentation files (the "Software"),  
 *  to deal in the Software without restriction, including without limitation  
 *  the rights to use, copy, modify, merge, publish, distribute, sublicense,  
 *  and/or sell copies of the Software, and to permit persons to whom the  
 *  Software is furnished to do so, subject to the following conditions:
 *
 *  The above copyright notice and this permission notice shall be included in 
 *  all copies or substantial portions of the Software.
 *
 *  THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR 
 *  IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, 
 *  FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE 
 *  AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER 
 *  LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING 
 *  FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER 
 *  DEALINGS IN THE SOFTWARE.
 */

using System;
using Entagged;
using NUnit.Framework;

[TestFixture]
public class ApeTest {
	private AudioFile audio_file;

	[TestFixtureSetUp]
	public void Init()
	{
		audio_file = new AudioFile("samples/sample.ape");
	}

	[Test]
	public void ReadTag()
	{
		Assert.AreEqual("APE album", audio_file.Album);
		Assert.AreEqual("APE artist", audio_file.Artist);
		Assert.AreEqual("APE comment", audio_file.Comment);
		Assert.AreEqual("Acid Punk", audio_file.Genre);
		Assert.AreEqual("APE title", audio_file.Title);
		Assert.AreEqual(6, audio_file.TrackNumber);
		Assert.AreEqual(7, audio_file.TrackCount);
		Assert.AreEqual(1234, audio_file.Year);
		Assert.AreEqual(5, audio_file.Duration.Seconds);
	}
}

[TestFixture]
public class FlacTest {
	private AudioFile audio_file;

	[TestFixtureSetUp]
	public void Init()
	{
		audio_file = new AudioFile("samples/sample.flac");
	}

	[Test]
	public void ReadTag()
	{
		Assert.AreEqual("FLAC album", audio_file.Album);
		Assert.AreEqual("FLAC artist", audio_file.Artist);
		Assert.AreEqual("FLAC comment", audio_file.Comment);
		Assert.AreEqual("Acid Punk", audio_file.Genre);
		Assert.AreEqual("FLAC title", audio_file.Title);
		Assert.AreEqual(6, audio_file.TrackNumber);
		Assert.AreEqual(7, audio_file.TrackCount);
		Assert.AreEqual(1234, audio_file.Year);
		Assert.AreEqual(5, audio_file.Duration.Seconds);
	}
}

[TestFixture]
public class Mp3BothTest {
	private AudioFile audio_file;

	[TestFixtureSetUp]
	public void Init()
	{
		audio_file = new AudioFile("samples/sample_both.mp3");
	}

	[Test]
	public void ReadTag()
	{
		Assert.AreEqual("MP3 album v2", audio_file.Album);
		Assert.AreEqual("MP3 artist", audio_file.Artist);
		Assert.AreEqual("MP3 comment v2", audio_file.Comment);
		Assert.AreEqual("Acid Punk", audio_file.Genre);
		Assert.AreEqual("MP3 title v2", audio_file.Title);
		Assert.AreEqual(6, audio_file.TrackNumber);
		Assert.AreEqual(7, audio_file.TrackCount);
		Assert.AreEqual(1234, audio_file.Year);
		Assert.AreEqual(5, audio_file.Duration.Seconds);
	}

	[Test]
	public void TwoTags()
	{
		Assert.AreEqual(2, audio_file.Albums.Length);
		Assert.AreEqual(1, audio_file.Artists.Length);
		Assert.AreEqual(2, audio_file.Comments.Length);
		Assert.AreEqual(2, audio_file.Genres.Length);
		Assert.AreEqual(2, audio_file.TrackNumbers.Length);
		Assert.AreEqual(1, audio_file.TrackCounts.Length);
		Assert.AreEqual(2, audio_file.Years.Length);
	}

	[Test]
	public void FirstTag()
	{
		Assert.AreEqual("MP3 title v2", audio_file.Titles[0] as string);
		Assert.AreEqual("MP3 album v2", audio_file.Albums[0] as string);
		Assert.AreEqual("MP3 comment v2", audio_file.Comments[0] as string);
		Assert.AreEqual(1234, (int) audio_file.Years[0]);
		Assert.AreEqual(6, (int) audio_file.TrackNumbers[0]);
		Assert.AreEqual(7, (int) audio_file.TrackCounts[0]);
	}

	[Test]
	public void SecondTag()
	{
		Assert.AreEqual("MP3 title", audio_file.Titles[1] as string);
		Assert.AreEqual("MP3 album", audio_file.Albums[1] as string);
		Assert.AreEqual("MP3 comment", audio_file.Comments[1] as string);
		Assert.AreEqual("MP3 artist", audio_file.Artists[0] as string);
		Assert.AreEqual(1235, (int) audio_file.Years[1]);
		Assert.AreEqual(6, (int) audio_file.TrackNumbers[1]);
		Assert.AreEqual(7, (int) audio_file.TrackCounts[0]);
	}
}

[TestFixture]
public class Mp3V1Test
{
	private AudioFile audio_file;

	[TestFixtureSetUp]
	public void Init()
	{
		audio_file = new AudioFile("samples/sample_v1_only.mp3");
	}

	[Test]
	public void ReadTag()
	{
		Assert.AreEqual("MP3 album", audio_file.Album);
		Assert.AreEqual("MP3 artist", audio_file.Artist);
		Assert.AreEqual("MP3 comment", audio_file.Comment);
		Assert.AreEqual("Acid Punk", audio_file.Genre);
		Assert.AreEqual("MP3 title", audio_file.Title);
		Assert.AreEqual(6, audio_file.TrackNumber);
		Assert.AreEqual(1234, audio_file.Year);
		Assert.AreEqual(1, audio_file.Duration.Seconds);
	}

}

[TestFixture]
public class Mp3V2Test
{
	private AudioFile audio_file;

	[TestFixtureSetUp]
	public void Init()
	{
		audio_file = new AudioFile("samples/sample_v2_only.mp3");
	}

	[Test]
	public void ReadTag()
	{
		Assert.AreEqual("MP3 album", audio_file.Album);
		Assert.AreEqual("MP3 artist", audio_file.Artist);
		Assert.AreEqual("MP3 comment", audio_file.Comment);
		Assert.AreEqual("Acid Punk", audio_file.Genre);
		Assert.AreEqual("MP3 title", audio_file.Title);
		Assert.AreEqual(6, audio_file.TrackNumber);
		Assert.AreEqual(7, audio_file.TrackCount);
		Assert.AreEqual(1234, audio_file.Year);
		Assert.AreEqual(1, audio_file.Duration.Seconds);
	}

}

[TestFixture]
public class MpcTest {
	private AudioFile audio_file;

	[TestFixtureSetUp]
	public void Init()
	{
		audio_file = new AudioFile("samples/sample.mpc");
	}

	[Test]
	public void ReadTag()
	{
		Assert.AreEqual("MPC album", audio_file.Album);
		Assert.AreEqual("MPC artist", audio_file.Artist);
		Assert.AreEqual("MPC comment", audio_file.Comment);
		Assert.AreEqual("Acid Punk", audio_file.Genre);
		Assert.AreEqual("MPC title", audio_file.Title);
		Assert.AreEqual(6, audio_file.TrackNumber);
		Assert.AreEqual(7, audio_file.TrackCount);
		Assert.AreEqual(1234, audio_file.Year);
		Assert.AreEqual(12, audio_file.Duration.Seconds);
	}
}

[TestFixture]
public class M4aTest {
	private AudioFile audio_file;

	[TestFixtureSetUp]
	public void Init()
	{
		audio_file = new AudioFile("samples/sample.m4a");
	}

	[Test]
	public void ReadTag()
	{
		Assert.AreEqual("M4A album", audio_file.Album);
		Assert.AreEqual("M4A artist", audio_file.Artist);
		Assert.AreEqual("M4A comment", audio_file.Comment);
		Assert.AreEqual("Acid Punk", audio_file.Genre);
		Assert.AreEqual("M4A title", audio_file.Title);
		Assert.AreEqual(6, audio_file.TrackNumber);
		Assert.AreEqual(1234, audio_file.Year);
		Assert.AreEqual(5, audio_file.Duration.Seconds);
	}
}

[TestFixture]
public class OggTest {
	private AudioFile audio_file;

	[TestFixtureSetUp]
	public void Init()
	{
		audio_file = new AudioFile("samples/sample.ogg");
	}

	[Test]
	public void ReadTag()
	{
		Assert.AreEqual("OGG album", audio_file.Album);
		Assert.AreEqual("OGG artist", audio_file.Artist);
		Assert.AreEqual("OGG comment", audio_file.Comment);
		Assert.AreEqual("Acid Punk", audio_file.Genre);
		Assert.AreEqual("OGG title", audio_file.Title);
		Assert.AreEqual(6, audio_file.TrackNumber);
		Assert.AreEqual(7, audio_file.TrackCount);
		Assert.AreEqual(1234, audio_file.Year);
		Assert.AreEqual(5, audio_file.Duration.Seconds);
	}
}

[TestFixture]
public class TrackerS3mTest {
	private AudioFile audio_file;

	[TestFixtureSetUp]
	public void Init()
	{
		audio_file = new AudioFile("samples/sample.s3m");
	}

	[Test]
	public void ReadTag()
	{
		Assert.AreEqual("64-Mania", audio_file.Title);
	}
}

[TestFixture]
public class TrackerModTest {
	private AudioFile audio_file;

	[TestFixtureSetUp]
	public void Init()
	{
		audio_file = new AudioFile("samples/sample.mod");
	}

	[Test]
	public void ReadTag()
	{
		Assert.AreEqual("Crazy Cow Level-1", audio_file.Title);
	}
}

[TestFixture]
public class TrackerXmTest {
	private AudioFile audio_file;

	[TestFixtureSetUp]
	public void Init()
	{
		audio_file = new AudioFile("samples/sample.xm");
	}

	[Test]
	public void ReadTag()
	{
		Assert.AreEqual(".midnight.", audio_file.Title);
	}
}

[TestFixture]
public class TrackerItTest {
	private AudioFile audio_file;

	[TestFixtureSetUp]
	public void Init()
	{
		audio_file = new AudioFile("samples/sample.it");
	}

	[Test]
	public void ReadTag()
	{
		Assert.AreEqual("Brass Funk", audio_file.Title);
	}
}

[TestFixture]
public class WmaTest {
	private AudioFile audio_file;

	[TestFixtureSetUp]
	public void Init()
	{
		audio_file = new AudioFile("samples/sample.wma");
	}

	[Test]
	public void ReadTag()
	{
		Assert.AreEqual("WMA title", audio_file.Title);
		Assert.AreEqual("WMA artist", audio_file.Artist);
		Assert.AreEqual("WMA album", audio_file.Album);
		Assert.AreEqual("WMA comment", audio_file.Comment);
		Assert.AreEqual("Brit Pop", audio_file.Genre);
		Assert.AreEqual(5, audio_file.TrackNumber);
		Assert.AreEqual(2005, audio_file.Year);
	}
}

