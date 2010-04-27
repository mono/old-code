namespace Wahid {

	static class Helper {

		/// <summary>
		///   Allows C-like _("string") localization
		/// </summary>
		public static string _ (this object s)
		{
			return s.ToString ();
		}
	}
}
