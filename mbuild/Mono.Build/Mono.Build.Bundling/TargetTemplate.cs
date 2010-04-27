namespace Mono.Build.Bundling {

    //public abstract class TargetTemplate : ITargetTemplate {
    [StructureBindingAttribute(typeof (StructureTemplate), false)]
    public abstract class TargetTemplate {

	protected TargetTemplate (StructureTemplate ignored) {}

	protected TargetTemplate () {}

	// We can inherit easily in bundles using this class. 
	// No TargetBuilder Add* methods can raise an error,
	// so we don't have a log argument or a bool return
	// value.

	public virtual void ApplyTemplate (TargetBuilder tb)
	{
	}
    }
}
