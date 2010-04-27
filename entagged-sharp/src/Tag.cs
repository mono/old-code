/***************************************************************************
 *  Copyright 2005 Raphaël Slinckx <raphael@slinckx.net> 
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
using System.Collections;
using System.Text;
using Entagged.Audioformats.Util;

namespace Entagged.Audioformats
{
    public class TagGenres 
    {
        private static string[] DEFAULT_GENRES = { 
            "Blues", "Classic Rock",
            "Country", "Dance", "Disco", "Funk", "Grunge", "Hip-Hop", "Jazz",
            "Metal", "New Age", "Oldies", "Other", "Pop", "R&B", "Rap",
            "Reggae", "Rock", "Techno", "Industrial", "Alternative", "Ska",
            "Death Metal", "Pranks", "Soundtrack", "Euro-Techno", "Ambient",
            "Trip-Hop", "Vocal", "Jazz+Funk", "Fusion", "Trance", "Classical",
            "Instrumental", "Acid", "House", "Game", "Sound Clip", "Gospel",
            "Noise", "AlternRock", "Bass", "Soul", "Punk", "Space",
            "Meditative", "Instrumental Pop", "Instrumental Rock", "Ethnic",
            "Gothic", "Darkwave", "Techno-Industrial", "Electronic",
            "Pop-Folk", "Eurodance", "Dream", "Southern Rock", "Comedy",
            "Cult", "Gangsta", "Top 40", "Christian Rap", "Pop/Funk", "Jungle",
            "Native American", "Cabaret", "New Wave", "Psychadelic", "Rave",
            "Showtunes", "Trailer", "Lo-Fi", "Tribal", "Acid Punk",
            "Acid Jazz", "Polka", "Retro", "Musical", "Rock & Roll",
            "Hard Rock", "Folk", "Folk-Rock", "National Folk", "Swing",
            "Fast Fusion", "Bebob", "Latin", "Revival", "Celtic", "Bluegrass",
            "Avantgarde", "Gothic Rock", "Progressive Rock",
            "Psychedelic Rock", "Symphonic Rock", "Slow Rock", "Big Band",
            "Chorus", "Easy IListening", "Acoustic", "Humour", "Speech",
            "Chanson", "Opera", "Chamber Music", "Sonata", "Symphony",
            "Booty Bass", "Primus", "Porn Groove", "Satire", "Slow Jam",
            "Club", "Tango", "Samba", "Folklore", "Ballad", "Power Ballad",
            "Rhythmic Soul", "Freestyle", "Duet", "Punk Rock", "Drum Solo",
            "A capella", "Euro-House", "Dance Hall" 
        };

        public static string [] Genres {
            get { 
                return DEFAULT_GENRES; 
            }
        }

        public static string Get(int i) 
        {
            if(i > TagGenres.Genres.Length - 1) {
                return null;
            }
            
            return DEFAULT_GENRES[i];
        }

        public static string Get(byte b)
        {
            return Get((int)b & 0xff);
        }
    }

    public enum CommonField {
        Artist,
        Album,
        Comment,
        Genre,
        Title,
        Track,
        TrackCount,
        Year,
        License
    }

    ////////////////////////////////////////////////////////
    //  
    //  Tag:  Defines basic operations on a Tag
    //
    ///////////////////////////////////////////////////////

    public class Tag : IEnumerable 
    {
        protected Hashtable fields = new Hashtable();
        private Hashtable common_field_lookup = new Hashtable();
        private Hashtable common_field_aliases = new Hashtable();

        protected void AddCommonFieldMapping(CommonField field_id, string internal_name)
        {
            if(internal_name == null) {
                return;
            }
            
            common_field_lookup[internal_name.ToLower()] = field_id;
        }
        
        protected void AddCommonFieldAlias(CommonField field_id, CommonField map_field_id)
        {
            common_field_aliases[field_id] = map_field_id;
        }

        public IList Title {
            get { 
                return Get(CommonField.Title); 
            }
        }
        
        public IList Album {
            get { 
                return Get(CommonField.Album); 
            }
        }
        
        public IList Artist {
            get { 
                return Get(CommonField.Artist); 
            }
        }
        
        public IList Genre {
            get { 
                return Get(CommonField.Genre); 
            }
        }
        
        public IList Track {
            get { 
                return Get(CommonField.Track); 
            }
        }
        
        public IList TrackCount {
            get { 
                return Get(CommonField.TrackCount); 
            }
        }
        
        public IList Year {
            get { 
                return Get(CommonField.Year); 
            }
        }
        
        public IList Comment {
            get { 
                return Get(CommonField.Comment); 
            }
        }
        
        public IList License {
            get {
                return Get(CommonField.License);
            }
        }

        public void SetTitle(string s) 
        {
            Set(CommonField.Title, s);
        }
        
        public void SetAlbum(string s) 
        {
            Set(CommonField.Album, s);
        }
        
        public void SetArtist(string s) 
        {
            Set(CommonField.Artist, s);
        }
        
        public void SetGenre(string s) 
        {
            Set(CommonField.Genre, s);
        }
        
        public void SetTrack(string s) 
        {
            Set(CommonField.Track, s);
        }
        
        public void SetTrackCount(string s) 
        {
            Set(CommonField.TrackCount, s);
        }
        
        public void SetYear(string s) 
        {
            Set(CommonField.Year, s);
        }
        
        public void SetComment(string s) 
        {
            Set(CommonField.Comment, s);
        }
        
        public void SetLicense(string s)
        {
            Set(CommonField.License, s);
        }

        public void AddTitle(string s) 
        {
            Add(CommonField.Title, s);
        }
        
        public void AddAlbum(string s) 
        {
            Add(CommonField.Album, s);
        }
        
        public void AddArtist(string s) 
        {
            Add(CommonField.Artist, s);
        }
        
        public void AddGenre(string s) 
        {
            Add(CommonField.Genre, s);
        }
        
        public void AddTrack(string s) 
        {
            Add(CommonField.Track, s);
        }

        public void AddTrackCount(string s) 
        {
            Add(CommonField.TrackCount, s);
        }

        public void AddYear(string s) 
        {
            Add(CommonField.Year, s);
        }
        
        public void AddComment(string s) 
        {
            Add(CommonField.Comment, s);
        }
        
        public void AddLicense(string s)
        {
            Add(CommonField.License, s);
        }
        
        public bool HasField(string id) 
        {
            return Get(id).Count != 0; 
        }
        
        public bool IsEmpty 
        {
            get { 
                return fields.Count == 0; 
            }
        }
        
        private class FieldsEnumerator : IEnumerator 
        {
            private IEnumerator field;
            private IDictionaryEnumerator it;
            
            public FieldsEnumerator(IDictionaryEnumerator it) 
            {
                this.it = it;
                
                bool fieldMoved = it.MoveNext();
           
                //If no elements at first level , return false
                if(!fieldMoved) {
                    this.field = null;
                } else {
                    //We set field to the first list iterator
                    field = (((DictionaryEntry)it.Current).Value as IList).GetEnumerator();
                }
            }
            
            public object Current {
                get { 
                    return field.Current; 
                }
            }

            public bool MoveNext() 
            {
                if(field == null) {
                    return false;
                }
                
                bool listMoved = field.MoveNext();
                
                while(!listMoved) {
                    bool fieldMoved = it.MoveNext();
                    
                    //If no elements at first level , return false
                    if(!fieldMoved) {
                        return false;
                    }
                    
                    //We set field to the first list iterator
                    field = (( (DictionaryEntry) it.Current).Value as IList).GetEnumerator();
                    listMoved = field.MoveNext();
                }

                return listMoved;
            }
                                
            public void Reset() 
            {
            }
        }
        
        public IEnumerator GetEnumerator() 
        {
            return new FieldsEnumerator(fields.GetEnumerator());
        }
            
        public IList Get(string id) 
        {
            IList list = fields[id.ToLower()] as IList;
            
            if(list == null) {
                return new ArrayList();
            }
            
            return list;
        }

        public IList Get(CommonField id)
        {
            IList list = Get(id.ToString());
            if(list.Count == 0) {
                try {
                    return Get(((CommonField)common_field_aliases[id]).ToString());
                } catch(Exception) {
                    return list;
                }
            }
            
            return list;
        }

        public void Set(string id, string content) 
        {
            if(id == null || content == null) {
                return;
            }
            
            id = id.ToLower();
            
            // If there is already an existing field with same id
            // we update the first element
            
            IList list = fields[id] as IList;
            
            if(list != null && list.Count > 0) {
                list [0] = content;
                return;
            }
            
            // Else we put the new field in the fields.
            list = new ArrayList();
            list.Add(content);
            fields[id] = list;
        }

        public void Set(CommonField id, string content)
        {
            Set(id.ToString(), content);
        }

        private void DoAdd(string id, string content) 
        {
            if(content == null) {
                return;
            }
            
            id = id.ToLower();
        
            // The 'raw' lists
            IList list = fields[id] as IList;
            
            // There was no previous item
            if(list == null) {
                list = new ArrayList();
                fields[id] = list;
            }

            list.Add(content);
        }

        public void Add(string id, string content, bool checkcommon)
        {
            DoAdd(id, content);

            if(checkcommon) {
                object field = common_field_lookup[id.ToLower()];
                if(field != null) {
                    DoAdd(((CommonField)field).ToString().ToLower(), content);
                }
            }
        }

        public void Add(string id, string content)
        {
            Add(id, content, true);
        }

        public void Add(CommonField id, string content)
        {
            Add(id.ToString(), content, false);
        }

        public override string ToString() 
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("Tag content:\n");
            foreach(DictionaryEntry entry in fields) {
                string id = (string) entry.Key;
                IList list = (IList) entry.Value;
                foreach(string content in list) {
                    sb.Append("\t");
                    sb.Append(id);
                    sb.Append(" : ");
                    sb.Append(content);
                    sb.Append("\n");
                }
            }
            
            return sb.ToString().Substring(0,sb.Length-1);
        }
    }
}
