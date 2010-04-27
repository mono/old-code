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

using Entagged.Audioformats.Util;

namespace Entagged.Audioformats.Mp3.Util.Id3Frames {
	
	public class ApicId3Frame : TextId3Frame {
	        
	    private string mime;
	    private byte pictureType;
	    private byte[] data;
	    
	    public ApicId3Frame(string description, string mime, byte pictureType, byte[] data)  : base("APIC", description) {
	        this.mime = mime;
	        this.pictureType = pictureType;
	        this.data = data;
	    }
		
	    public ApicId3Frame(byte[] rawContent, byte version) : base("APIC", rawContent, version) {}
	    
	    public string MimeType {
	        get { return mime; }
	    }
	    
	    public byte PictureType {
	        get { return pictureType; }
	    }
	    
	    public string PictureTypeAsString {
	        get {
		        switch(pictureType&0xFF) {
			    	case 0x00:	return "Other";
			    	case 0x01:	return "32x32 pixels file icon";
			    	case 0x02:	return "Other file icon";
			    	case 0x03:	return "Cover (front)";
			    	case 0x04:	return "Cover (back)";
			    	case 0x05:	return "Leaflet page";
			    	case 0x06:	return "Media (e.g. lable side of CD)";
			    	case 0x07:	return "Lead artist/lead performer/soloist";
			    	case 0x08:	return "Artist/performer";
			    	case 0x09:	return "Conductor";
			    	case 0x0A:	return "Band/Orchestra";
			    	case 0x0B:	return "Composer";
			    	case 0x0C:	return "Lyricist/text writer";
			    	case 0x0D:	return "Recording Location";
			    	case 0x0E:	return "During recording";
			    	case 0x0F:	return "During performance";
			    	case 0x10:	return "Movie/video screen capture";
			    	case 0x11:	return "A bright coloured fish";
			    	case 0x12:	return "Illustration";
			    	case 0x13:	return "Band/artist logotype";
			    	case 0x14:	return "Publisher/Studio logotype";
		        }
	        
	        	return "Unknown";
	        }
	    }
	    
	    public byte[] Data {
	        get { return data; }
	    }
	    	    
		public override bool IsBinary {
			get { return true; }
		}
		
		public override bool IsEmpty {
		    get { return base.IsEmpty && data.Length == 0 && mime == ""; }
		}
		
		public override void CopyContent(TagField field) {
		    base.CopyContent(field);
		    
		    if(field is ApicId3Frame) {
		        this.mime = (field as ApicId3Frame).MimeType;
		        this.pictureType = (field as ApicId3Frame).PictureType;
		        this.data = (field as ApicId3Frame).Data;
		    }
		}
		
		protected override void Populate(byte[] raw) {
		    this.encoding = raw[flags.Length];
			if(this.encoding != 0 && this.encoding != 1)
			    this.encoding = 0;
			
			int offset = -1;
			for(int i = flags.Length+1; i<raw.Length; i++)
			    if(raw[i] == 0x00) {
			        offset = i;
			        break;
			    }
			this.mime = GetString(raw, flags.Length+1, offset - flags.Length-1, "ISO-8859-1");
			
			this.pictureType = raw[offset+1];
			
			this.content = GetString(raw, offset+2, raw.Length-offset-2, Encoding);
			
			string[] strings = this.content.Split('\0');
			this.content = strings[0];
			
			int length = GetBytes(this.content+"\u0000", Encoding).Length;
			
			this.data = new byte[raw.Length - offset - 2 - length];
			for(int i = 0; i<data.Length; i++)
			    this.data[i] = raw[offset+2+length+i];
		}
		
		public override string ToString() {
			return "["+mime+" ("+PictureTypeAsString+")] "+base.ToString();
		}
	}
}	
