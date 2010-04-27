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

namespace Entagged.Audioformats.Mp3.Util {
	public class MPEGFrame {
		
		private byte[] mpegBytes;

		/**  The version of this MPEG frame (see the constants) */
		private int MPEGVersion;

		/**  Bitrate of this frame */
		private int bitrate;

		/**  Channel Mode of this Frame (see constants) */
		private int channelMode;

		/**  Emphasis mode string */
		private string emphasis;

		/**  Flag indicating if this frame has padding byte */
		private bool hasPadding;

		/**  Flag indicating if this frame contains copyrighted material */
		private bool isCopyrighted;

		/**  Flag indicating if this frame contains original material */
		private bool isOriginal;

		/**  Flag indicating if this frame is protected */
		private bool isProtected;

		/**  Flag indicating if this is a valid MPEG Frame */
		private bool isValid;

		/**  Contains the mpeg layer of this frame (see constants) */
		private int layer;

		/**  Mode Extension of this frame */
		private string modeExtension;

		/**  Sampling rate of this frame in kbps */
		private int samplingRate;

		/**  Constant holding the Dual Channel Stereo Mode */
		public const int CHANNEL_MODE_DUAL_CHANNEL = 2;

		/**  Constant holding the Joint Stereo Mode */
		public const int CHANNEL_MODE_JOINT_STEREO = 1;

		/**  Constant holding the Mono Mode */
		public const int CHANNEL_MODE_MONO = 3;

		/**  Constant holding the Stereo Mode */
		public const int CHANNEL_MODE_STEREO = 0;

		/**  Constant holding the Layer 1 value Mpeg frame */
		public const int LAYER_I = 3;

		/**  Constant holding the Layer 2 value Mpeg frame */
		public const int LAYER_II = 2;

		/**  Constant holding the Layer 3 value Mpeg frame */
		public const int LAYER_III = 1;

		/**  Constant holding the Reserved Layer value Mpeg frame */
		public const int LAYER_RESERVED = 0;

		/**  Constant holding the mpeg frame version 1 */
		public const int MPEG_VERSION_1 = 3;

		/**  Constant holding the mpeg frame version 2 */
		public const int MPEG_VERSION_2 = 2;

		/**  Constant holding the mpeg frame version 2.5 */
		public const int MPEG_VERSION_2_5 = 0;

		/**  Constant holding the reserved mpeg frame */
		public const int MPEG_VERSION_RESERVED = 1;

		/**  Constant table holding the different Mpeg versions allowed */
		private static int[] MPEGVersionTable = new int[]
				{MPEG_VERSION_2_5, MPEG_VERSION_RESERVED, MPEG_VERSION_2, MPEG_VERSION_1};

		/**  Constant table holding the different Mpeg versions allowed in a string representation  */
		private static string[] MPEGVersionTable_string = new string[]
				{"MPEG Version 2.5", "reserved", "MPEG Version 2 (ISO/IEC 13818-3)", "MPEG Version 1 (ISO/IEC 11172-3)"};

		/**  Constant 3ple table that holds the bitrate in kbps for the given layer, mode and value  */
		private static int[,,] bitrateTable = new int[,,]
				{  //table
				{  //V1
				{0, 32, 64, 96, 128, 160, 192, 224, 256, 288, 320, 352, 384, 416, 448, -1},   //LI
				{0, 32, 48, 56, 64, 80, 96, 112, 128, 160, 192, 224, 256, 320, 384, -1},   //LII
				{0, 32, 40, 48, 56, 64, 80, 96, 112, 128, 160, 192, 224, 256, 320, -1}  //LIII
				},
				{  //V2
				{0, 32, 48, 56, 64, 80, 96, 112, 128, 144, 160, 176, 192, 224, 256, -1},   //LI
				{0, 8, 16, 24, 32, 40, 48, 56, 64, 80, 96, 112, 128, 144, 160, -1},   //LII
				{0, 8, 16, 24, 32, 40, 48, 56, 64, 80, 96, 112, 128, 144, 160, -1}  //LIII
				}
				};

		/**  Constant table holding the channel modes allowed in a string representation */
		private static string[] channelModeTable_string = new string[]
				{"Stereo", "Joint stereo (Stereo)", "Dual channel (2 mono channels)", "Single channel (Mono)"};

		/**  Constant table holding the names of the emphasis modes in a string representation */
		private static string[] emphasisTable = new string[]
				{"none", "50/15 ms", "reserved", "CCIT J.17"};

		/**  Constant table holding the Layer descriptions allowed */
		private static int[] layerDescriptionTable = new int[]
				{LAYER_RESERVED, LAYER_III, LAYER_II, LAYER_I};

		/**  Constant table holding the Layer descriptions allowed in a string representation */
		private static string[] layerDescriptionTable_string = new string[]
				{"reserved", "Layer III", "Layer II", "Layer I"};

		/**  Constant table holding the mode extensions for a given layer in a string representation  */
		private static string[,] modeExtensionTable =new string[,]
				{
				{"4-31", "8-31", "12-31", "16-31"},   //LI , LII
				{"off-off", "on-off", "off-on", "on-on"}  //"intensity Stereo - MS Stereo" //LIII
				};

		/**  Constant table holding the sampling rate in Hz for a given Mpeg version */
		private static int[,] samplingRateTable =new int[,]
				{  //table
				{44100, 48000, 32000, 0},   //V1
				{22050, 24000, 16000, 0},   //V2
				{11025, 12000, 8000, 0}  //V3
				};

		private static int[] SAMPLE_NUMBERS = {-1, 1152, 1152, 384};
		
		public MPEGFrame( byte[] b ) {
			this.mpegBytes = b;
			
			if ( isMPEGFrame() ) {
				MPEGVersion = _MPEGVersion();
				layer = layerDescription();
				isProtected = _isProtected();
				bitrate = _bitrate();
				samplingRate = _samplingRate();
				hasPadding = _hasPadding();
				channelMode = _channelMode();
				modeExtension = _modeExtension();
				isCopyrighted = _isCopyrighted();
				isOriginal = _isOriginal();
				emphasis = _emphasis();
				isValid = true;

			}
			else
				//Ce n'est pas un frame MPEG
				isValid = false;
			this.mpegBytes = null;

		}

		public int Bitrate {
			get { return bitrate; }
		}

		public int ChannelNumber {
			get {
				switch(channelMode) {
					case CHANNEL_MODE_DUAL_CHANNEL: return 2;
					case CHANNEL_MODE_JOINT_STEREO: return 2;
					case CHANNEL_MODE_MONO: return 1;
					case CHANNEL_MODE_STEREO: return 2;
				}
				return 0;
			}
		}
		
		public int ChannelMode {
			get { return channelMode; }
		}

		public int LayerVersion {
			get { return layer; } 
		}

		public int MpegVersion {
			get { return MPEGVersion; }
		}

		public int PaddingLength {
			get { 
				if ( hasPadding && layer != LAYER_I)
					return 1;
				if ( hasPadding && layer == LAYER_I)
				    return 4;
				
				return 0;
			}
		}

		public int SamplingRate {
			get { return samplingRate; }
		}


		public bool Valid {
			get { return isValid; }
		}


		public int FrameLength {
		    get {
			    if (layer == LAYER_I) {
			        return (12 * (Bitrate * 1000) / SamplingRate + PaddingLength) * 4;
			    }
			    
			    return 144 * (Bitrate * 1000) / SamplingRate + PaddingLength;
			}
		}
		
		public int SampleNumber {
		    get {
			    int sn = SAMPLE_NUMBERS[layer];
			    
			    //if ( ( MPEGVersion == MPEGFrame.MPEG_VERSION_2 ) || ( MPEGVersion == MPEGFrame.MPEG_VERSION_2_5 ) && (layer == LAYER_III))
					//sn = sn/2;
			    
			    return sn;
			}
		}
	
		public string MpegVersionToString( int i ) {
			return MPEGVersionTable_string[i];
		}


		public string ChannelModeToString( int i ) {
			return channelModeTable_string[i];
		}


		public string LayerToString( int i ) {
			return layerDescriptionTable_string[i];
		}

		public override string ToString() {
			string output = "\n----MPEGFrame--------------------\n";

			output += "MPEG Version: " + MpegVersionToString( MPEGVersion ) + "\tLayer: " + LayerToString( layer ) + "\n";
			output += "Bitrate: " + bitrate + "\tSamp.Freq.: " + samplingRate + "\tChan.Mode: " + ChannelModeToString( channelMode ) + "\n";
			output += "Mode Extension: " + modeExtension + "\tEmphasis: " + emphasis + "\n";
			output += "Padding? " + hasPadding + "\tProtected? " + isProtected + "\tCopyright? " + isCopyrighted + "\tOriginal? " + isOriginal + "\n";
			output += "--------------------------------";
			return output;
		}

		private bool _isCopyrighted() {
			return ( (mpegBytes[3]&0x08) == 0x08);
		}

		private bool isMPEGFrame() {
			return ( (mpegBytes[0]&0xFF) == 0xFF ) && ( (mpegBytes[1]&0xE0) == 0xE0 );
		}

		private bool _isOriginal() {
			return (mpegBytes[3]&0x04) == 0x04;
		}

		private bool _isProtected() {
			return (mpegBytes[1]&0x01) == 0x00;
		}

		private int _MPEGVersion() {
			//System.err.println("V.:"+mpegBytes[1]+"|"+(mpegBytes[1]&0x18)+"|"+((mpegBytes[1]&0x18) >> 3));
			int index = ((mpegBytes[1]&0x18) >> 3);
			//System.err.println(MPEGVersionTable[index]);

			return MPEGVersionTable[index];
		}

		private int _bitrate() {
			int index3 = ((mpegBytes[2]&0xF0) >> 4);
			int index1 = ( MPEGVersion == MPEG_VERSION_1 ) ? 0 : 1;
			int index2;

			if ( layer == LAYER_I )
				index2 = 0;
			else if ( layer == LAYER_II )
				index2 = 1;
			else
				index2 = 2;
			return bitrateTable[index1,index2,index3];
		}

		private int _channelMode() {
			int index = ((mpegBytes[3]&0xC0) >> 6);

			return index;
		}

		private string _emphasis() {
			int index = (mpegBytes[3]&0x03);

			return emphasisTable[index];
		}

		private bool _hasPadding() {
			return (mpegBytes[2]&0x02) == 0x02;
		}

		private int layerDescription() {
			int index = ((mpegBytes[1]&0x06) >> 1);

			return layerDescriptionTable[index];
		}

		private string _modeExtension() {
			int index2 = ((mpegBytes[3]&0x30) >> 4);
			int index1 = ( layer == LAYER_III ) ? 1 : 0;

			return modeExtensionTable[index1,index2];
		}

		private int _samplingRate() {
			int index2 = ((mpegBytes[2]&0x0c) >> 2);
			int index1;

			if ( MPEGVersion == MPEG_VERSION_1 )
				index1 = 0;
			else if ( MPEGVersion == MPEG_VERSION_2 )
				index1 = 1;
			else
				index1 = 2;
			return samplingRateTable[index1,index2];
		}

	}
}
