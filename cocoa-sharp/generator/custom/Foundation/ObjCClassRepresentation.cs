using System;
using System.Collections;
using System.Reflection;

namespace Apple.Tools {

        public class ObjCClassRepresentation {
                private String[] mMethods;
                private String[] mSignatures;

		public String[] Methods {
			get { return mMethods; }
			set { mMethods = value; }
		}
		
		public String[] Signatures {
			get { return mSignatures; }
			set { mSignatures = value; }
		}

		public int NumMethods {
			get { return Methods.Length; }
		}
        }
}
