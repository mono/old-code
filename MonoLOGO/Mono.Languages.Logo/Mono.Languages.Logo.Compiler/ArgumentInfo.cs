namespace Mono.Languages.Logo.Compiler {
	public struct ArgumentInfo {
		public string name;
		public object val;
		public bool collect;

		public ArgumentInfo (string name, object val, bool collect) {
			this.name = name;
			this.val = val;
			this.collect = collect;
		}
	}
}

