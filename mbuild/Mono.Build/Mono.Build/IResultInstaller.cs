// IResultInstaller.cs -- something that can 'install' a Result,
// whatever that means. Usually implemented by another Result which
// is set to be the 'installer' tag on a target.

using System;

namespace Mono.Build {

	public interface IResultInstaller {

		Type OtherType { get; }

		// If backwards, do an uninstall (handled as a parameter so that
		// we may symmetrize our code as much as possible.)

		bool InstallResult (Result other, bool backwards, IBuildContext ctxt);

		// FIXME: kind of hack. I18n, l10n?

		string DescribeAction (Result other, IBuildContext ctxt);
	}
}
