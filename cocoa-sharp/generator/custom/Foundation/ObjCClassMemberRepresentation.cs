using System;
using System.Collections;
using System.Reflection;

namespace Apple.Tools {

        public class ObjCClassMemberRepresentation {
                private String[] mNames;
                private String[] mTypes;
                private int[] mSizes;

		public String[] Names {
			get { return mNames; }
			set { mNames = value; }
		}
		
		public String[] Types {
			get { return mTypes; }
			set {  mTypes = value; }
		}

		public int[] Sizes {
			get { return mSizes; }
			set { mSizes = value; }
		}

		public int NumMembers {
			get { return Names.Length; }
		}
        }
}
