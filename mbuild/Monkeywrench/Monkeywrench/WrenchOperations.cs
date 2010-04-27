// WrenchOperations.cs -- implement some common non-trivial build operations

using System;

using Mono.Build;

namespace Monkeywrench {

	public class WrenchOperations {

		private WrenchOperations () {}

		public static event OperationFunc BeforeBuild;
		public static event OperationFunc BeforeSkip;
		public static event OperationFunc BeforeClean;
		public static event OperationFunc BeforeUncache;
		public static event OperationFunc BeforeInstall;
		public static event OperationFunc BeforeUninstall;
		public static event OperationFunc BeforeDistribute;

		public static BuiltItem BuildValueUnconditional (WrenchProject proj, BuildServices bs) {
			bs.Logger.Log ("operation.build", bs.FullName);
			DoBeforeBuild (proj, bs);
			return bs.BuildValue ();
		}

		public static BuiltItem BuildValue (WrenchProject proj, BuildServices bs) {
			BuiltItem bi;

			if (bs.TryValueRecovery (out bi))
				return BuiltItem.Null;

			if (bi.IsValid) {
				DoBeforeSkip (proj, bs);
				return bi;
			}

			return BuildValueUnconditional (proj, bs);
		}

		public static bool Build (WrenchProject proj, BuildServices bs) {
			return (!BuildValue (proj, bs).IsValid);
		}

		public static bool ForceBuild (WrenchProject proj, BuildServices bs) {
			return (!BuildValueUnconditional (proj, bs).IsValid);
		}

		public static bool Clean (WrenchProject proj, BuildServices bs) {
			BuiltItem bi = bs.GetRawCachedValue ();

			if (!bi.IsValid)
				return false;

			bs.Logger.Log ("operation.clean", bs.FullName);
			if (bi.Result.Clean (bs.Context))
				// FIXME: rename to After
				DoBeforeClean (proj, bs);

			bs.UncacheValue ();
			return false;
		}

		public static bool Uncache (WrenchProject proj, BuildServices bs) {
			bs.Logger.Log ("operation.uncache", bs.FullName);
			DoBeforeUncache (proj, bs);
			bs.UncacheValue ();
			return false;
		}

		public static bool Install (WrenchProject proj, BuildServices bs) {
			return Install (proj, bs, false);
		}

		public static bool Uninstall (WrenchProject proj, BuildServices bs) {
			return Install (proj, bs, true);
		}

		public static bool GetInstaller (WrenchProject proj, BuildServices bs, 
						 out IResultInstaller ret) {
			ret = null;

			// potential recursion!
			Result installer;
			if (bs.GetTag (proj.Graph.GetTagId ("install"), out installer))
				return true;

			if (installer == null)
				return false;

			// use install = false to mean "don't install"

			if (installer is MBBool) {
				if (((MBBool) installer).Value) {
					bs.Logger.Error (2019, "To install this target, set its installer tag to an Installer.", null);
					return true;
				}

				return false;
			}

			ret = installer as IResultInstaller;

			if (ret == null) {
				bs.Logger.Error (2016, "This target's \"install\" tag does not support installation", installer.ToString ());
				return true;
			}

			return false;
		}

		public static bool GetInstallerAndResult (WrenchProject proj,
							  BuildServices bs, out IResultInstaller iri,
							  out Result obj) {
			obj = null;

			if (GetInstaller (proj, bs, out iri))
				return true;

			if (iri == null)
				return false;

			obj = bs.GetValue ().Result;

			if (obj == null)
				// error already reported
				return true;

			bs.Logger.PushLocation (bs.FullName);
			obj = CompositeResult.FindCompatible (obj, iri.OtherType);
			bs.Logger.PopLocation ();

			if (obj == null) {
				bs.Logger.Error (2031, "Trying to install result with incompatible installer", 
					      iri.OtherType.ToString ());
				return true;
			}

			return false;
		}

		public static bool Install (WrenchProject proj, BuildServices bs, bool backwards) {
			IResultInstaller iri;
			Result res;

			if (GetInstallerAndResult (proj, bs, out iri, out res))
				return true;

			if (iri == null)
				return false;

			try {
				bs.Logger.Log ("operation.install", bs.FullName);
				if (backwards)
					DoBeforeUninstall (proj, bs);
				else
					DoBeforeInstall (proj, bs);
				return iri.InstallResult (res, backwards, bs.Context);
			} catch (Exception e) {
				bs.Logger.Error (3012, "Unhandled exception in install routine for " + res.ToString (), e.ToString ());
				return true;
			}
		}

		public static bool Distribute (WrenchProject proj, BuildServices bs) {
			Result res = bs.GetValue ().Result;

			if (res == null)
				return true;

			bs.Logger.Log ("operation.distribute", bs.FullName);
			if (res.DistClone (bs.Context))
				// rename this event
				DoBeforeDistribute (proj, bs);
				
			return false;
		}

		// Manager

		class WrenchManager : IBuildManager {
			WrenchProject proj;

			public WrenchManager (WrenchProject proj) {
				if (proj == null)
					throw new ArgumentNullException ();
				this.proj = proj;
			}

			public BuiltItem[] EvaluateTargets (int[] targets) {
				BuiltItem[] bis = new BuiltItem[targets.Length];

				for (int i = 0; i < targets.Length; i++) {
				    //proj.Log.PushLocation (targets[i]);
					BuildServices bs = proj.GetTargetServices (targets[i]);
					//proj.Log.PopLocation ();

					if (bs == null)
						return null;
					
					switch (proj.GetTargetState (targets[i])) {
					case WrenchProject.TargetState.BuiltOk:
						//Console.WriteLine ("EVAL {0}: from cache", targets[i]);
						bis[i] = bs.GetRawCachedValue ();
						break;
					case WrenchProject.TargetState.BuiltError:
						//Console.WriteLine ("EVAL {0}: errored", targets[i]);
						return null;
					case WrenchProject.TargetState.Building:
						//Console.WriteLine ("EVAL {0}: recursed", targets[i]);
						bs.Logger.Error (2048, "Recursion in build; this target depends on " + targets[i] + 
								 " which is currently being built", null);
						return null;
					default:
						//Console.WriteLine ("EVAL {0}: build it", targets[i]);
						// State unknown. Load from cache if possible, otherwise build.

						proj.Log.PushLocation (bs.FullName);
						bis[i] = WrenchOperations.BuildValue (proj, bs);
						proj.Log.PopLocation ();

						if (!bis[i].IsValid)
							return null;
						break;
					}
				}

				return bis;
			}

			public BuiltItem EvaluateDefaultHack (string targ)
			{
			    int tid = proj.Graph.GetTargetId (targ);

			    BuiltItem[] r = EvaluateTargets (new int[] { tid });

			    if (r == null)
				return new BuiltItem ();

			    return r[0];
			}
		}

		public static IBuildManager MakeManager (WrenchProject proj) {
			return new WrenchManager (proj);
		}

		// Helpahs

		static void DoBeforeBuild (WrenchProject proj, BuildServices bs) {
			if (BeforeBuild == null)
				return;

			BeforeBuild (proj, bs);
		}

		static void DoBeforeSkip (WrenchProject proj, BuildServices bs) {
			if (BeforeSkip == null)
				return;

			BeforeSkip (proj, bs);
		}

		static void DoBeforeClean (WrenchProject proj, BuildServices bs) {
			if (BeforeClean == null)
				return;

			BeforeClean (proj, bs);
		}

		static void DoBeforeUncache (WrenchProject proj, BuildServices bs) {
			if (BeforeUncache == null)
				return;

			BeforeUncache (proj, bs);
		}

		static void DoBeforeInstall (WrenchProject proj, BuildServices bs) {
			if (BeforeInstall == null)
				return;

			BeforeInstall (proj, bs);
		}

		static void DoBeforeUninstall (WrenchProject proj, BuildServices bs) {
			if (BeforeUninstall == null)
				return;

			BeforeUninstall (proj, bs);
		}

		static void DoBeforeDistribute (WrenchProject proj, BuildServices bs) {
			if (BeforeDistribute == null)
				return;

			BeforeDistribute (proj, bs);
		}
	}
}
