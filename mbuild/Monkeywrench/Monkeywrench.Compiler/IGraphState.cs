using System;
using System.Collections;
using System.Collections.Generic;

using Mono.Build;

namespace Monkeywrench.Compiler {

    public interface IGraphState {

	///////////////////////////////////////////
	// Hooks for providers. Provider ID 0 is the
	// root provider.

	string GetProviderBasis (short id);

	// returns the subdirectory of srcdir in which
	// this provider was actually declared (eg,
	// the directory of the Buildfile for a 
	// Buildfile-based provider; toplevel for
	// bundle-defined providers)

	string GetProviderDeclarationLoc (short id);

	short GetProviderId (string basis);

	short NumProviders { get; }

	// Integer target ID bound:
	// for (int tid = id << 16; tid < GetProviderTargetBound (pid); tid++) { .. }

	int GetProviderTargetBound (short id);

	///////////////////////////////////////////
	// Hooks for targets. Currently just ITarget
	// multiplexed across the target ID. Nothing
	// more obvious to do at the moment ...

	string GetTargetName (int tid);

	int GetTargetId (string target);

	Type GetTargetRuleType (int tid);

	bool ApplyTargetDependencies (int tid, ArgCollector ac, IWarningLogger logger);

	///////////////////////////////////////////
	// Hooks for tags

	int GetTagId (string tag);

	string GetTagName (int tag);

	// Returns a Result, an int, or null
	object GetTargetTag (int tid, int tag);

	IEnumerable<TargetTagInfo> GetTargetsWithTag (int tag);

	///////////////////////////////////////////
	// Hooks for change-checking

	// Name is a topsrcdir-relative file name
	IEnumerable<DependentItemInfo> GetDependentFiles ();

	// Name is a bundle name
	IEnumerable<DependentItemInfo> GetDependentBundles ();

	///////////////////////////////////////////
	// Hooks for project info

	ProjectInfo GetProjectInfo ();
    }
}
