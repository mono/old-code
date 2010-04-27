//
// a simple implementation of a bare build provider
//

using System;
using System.Collections;

namespace Mono.Build {

	public class SimpleProvider : IBuildProvider {

		protected Hashtable targets;

		public SimpleProvider (ITarget[] targets) {
			this.targets = new Hashtable ();

			if (targets == null)
				return;

			foreach (ITarget t in targets)
				AddTarget (t);
		}

		public SimpleProvider () : this (null) {
		}

		protected void AddTarget (ITarget target) {
			this.targets[target.Name] = target;
		}

		protected void AddTarget (string name, Result value) {
			AddTarget (new ConstantTarget (name, value));
		}

		protected void AddTarget (string name, string value) {
			AddTarget (name, new MBString (value));
		}

		public ITarget GetTarget (string name) {
			if (name == null)
				throw new ArgumentNullException ();

			return (ITarget) targets[name];
		}

		public IEnumerable Targets {
			get {
				return targets.Values;
			}
		}

		public IEnumerable TargetNames {
			get {
				return targets.Keys;
			}
		}

		public object Context {
			get {
				return null;
			}
		}
	}
}
