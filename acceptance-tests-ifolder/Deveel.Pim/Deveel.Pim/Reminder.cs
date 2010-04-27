//
// Reminder.cs:
//
// Author:
//	Antonello Provenzano (antonello@deveel.com)
//
// (C) 2006, Deveel srl, (http://deveel.com)
//

//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//

using System;

namespace Deveel.Pim {
	public sealed class Reminder : Property {
		#region .ctor
		public Reminder() {
			active = false;
			minutes = options = 0;
			soundFile = null;
			interval = 0;
			repeatCount = 0;
		}
		#endregion
		
		#region Fields
		private bool active;
		private int minutes;
		private string soundFile; //the digital sound to be played when the reminder is executed.
		private int options;
		private int interval; //the interval in which the reminder has to be repeated
		private int repeatCount; //the number of times that the reminder has to be repeated
		#endregion

		#region Properties
		public int Minutes {
			get { return minutes; }
			set { minutes = value; }
		}

		public string SoundFile {
			get { return soundFile; }
			set { soundFile = value; }
		}

		public int Options {
			get { return options; }
			set { options = value; }
		}

		public bool IsActive {
			get { return active; }
			set { active = value; }
		}

		public int Interval {
			get { return interval; }
			set { interval = value; }
		}

		public int RepeatCount {
			get { return repeatCount; }
			set { repeatCount = value; }
		}
		#endregion
	}
}