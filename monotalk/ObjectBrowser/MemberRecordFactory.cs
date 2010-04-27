using System;
using System.Collections;
using System.Reflection;

using Gdk;

namespace Monotalk.Browser
{
	public abstract class MemberRecordFactory
	{
		public MemberRecord [] Info;
		protected BindingFlags flags;
		protected Type type;
		public int staticCount = 0;
		public int instanceCount = 0;

		public MemberRecordFactory (BindingFlags flags)
		{
			this.flags = flags;
		}

		public virtual BindingFlags Flags
		{
			set {
				bool changed = flags != value;
				flags = value;
				if (changed)
					Load ();
			}
			get {
				return flags;
			}
		}

		public virtual Type Type {
			set {
				bool changed = type != value;
				type = value;
				if (changed)
					Load ();
			}
		}

		public virtual bool HideDuplicates {
			get {
				return true;
			}
		}

		public string FullTitle
		{
			get {
				if (Info == null || Info.Length == 0)
					return Title;
				else
					return "<b>" + Title + " (" + Info.Length + ")</b>";
			}
		}

		protected int nRecords {
			get {
				return ((flags & BindingFlags.Static) == BindingFlags.Static ? staticCount : 0)
				+ ((flags & BindingFlags.Instance) == BindingFlags.Instance ? instanceCount : 0);
			}
		}

		public virtual void Load ()
		{
			if (type == null)
				Info = null;
			else {
				MemberInfo[] staticFields = Members (BindingFlags.Static);
				MemberInfo[] instanceFields = Members (BindingFlags.Instance);
				staticCount = staticFields.Length;
				instanceCount = instanceFields.Length;
				Info = new MemberRecord [nRecords];
				int count = 0;

				if ((flags & BindingFlags.Static) == BindingFlags.Static)
					foreach (MemberInfo mi in staticFields) {
					        Info [count] = Record (mi);
						count ++;
				        }

				if ((flags & BindingFlags.Instance) == BindingFlags.Instance)
					foreach (MemberInfo mi in instanceFields) {
					        Info [count] = Record (mi);
						count ++;
				        }
			}
		}

		protected abstract MemberRecord Record (MemberInfo mi);
		protected abstract MemberInfo[] Members (BindingFlags scope);
		public abstract string Title { get; }
		public abstract string ColumnTitle { get; }
	}

	public class MethodRecordFactory : MemberRecordFactory
	{
		public MethodRecordFactory (BindingFlags flags) : base (flags)
		{
		}

		public override string Title {
			get {
				return "methods";
			}
		}

		public override string ColumnTitle {
			get {
				return "Method";
			}
		}

		public override void Load ()
		{
			if (type == null)
				Info = null;
			else {
				MethodInfo[] staticMethods = (MethodInfo[]) Members (BindingFlags.Static);
				MethodInfo[] instanceMethods = (MethodInfo[]) Members (BindingFlags.Instance);
				int count = 0;

				foreach (MethodInfo mi in staticMethods)
				         if (!mi.IsSpecialName
					     && !mi.Name.StartsWith ("add_") && !mi.Name.StartsWith ("remove_") // FIXME: remove once mcs is fixed
					     && !mi.Name.StartsWith ("get_") && !mi.Name.StartsWith ("set_"))
						 count ++;
				staticCount = count;
				foreach (MethodInfo mi in instanceMethods)
				         if (!mi.IsSpecialName
					     && !mi.Name.StartsWith ("add_") && !mi.Name.StartsWith ("remove_") // FIXME: remove once mcs is fixed
					     && !mi.Name.StartsWith ("get_") && !mi.Name.StartsWith ("set_"))
						 count ++;
				instanceCount = count - staticCount;

				Info = new MethodRecord [nRecords];
				count = 0;
				if ((BindingFlags.Static & flags) == BindingFlags.Static)
					foreach (MethodInfo mi in staticMethods)
					        if (!mi.IsSpecialName
						    && !mi.Name.StartsWith ("add_") && !mi.Name.StartsWith ("remove_") // FIXME: remove once mcs is fixed
						    && !mi.Name.StartsWith ("get_") && !mi.Name.StartsWith ("set_")) {
							Info [count] = Record (mi);
							count ++;
						}
				if ((BindingFlags.Instance & flags) == BindingFlags.Instance)
					foreach (MethodInfo mi in instanceMethods)
					        if (!mi.IsSpecialName
						    && !mi.Name.StartsWith ("add_") && !mi.Name.StartsWith ("remove_") // FIXME: remove once mcs is fixed
						    && !mi.Name.StartsWith ("get_") && !mi.Name.StartsWith ("set_")) {
							Info [count] = Record (mi);
							count ++;
						}
			}
		}

		protected override MemberRecord Record (MemberInfo mi)
		{
			return new MethodRecord ((MethodInfo) mi);
		}

		protected override MemberInfo[] Members (BindingFlags scope) {
			return type.GetMethods ((flags & ~(BindingFlags.Static | BindingFlags.Instance)) | scope);
		}
	}

	public class ConstructorRecordFactory : MemberRecordFactory
	{
		public ConstructorRecordFactory (BindingFlags flags) : base (flags) {}

		public override string Title {
			get {
				return "constructors";
			}
		}

		public override string ColumnTitle {
			get {
				return "Constructor";
			}
		}

		public override bool HideDuplicates {
			get {
				return false;
			}
		}

		protected override MemberRecord Record (MemberInfo mi)
		{
			return new ConstructorRecord ((ConstructorInfo) mi);
		}

		protected override MemberInfo[] Members (BindingFlags scope) {
			return type.GetConstructors ((flags & ~(BindingFlags.Static | BindingFlags.Instance)) | scope);
		}
	}

	public class EventRecordFactory : MemberRecordFactory
	{
		public EventRecordFactory (BindingFlags flags) : base (flags) {}

		public override string Title {
			get {
				return "events";
			}
		}

		public override string ColumnTitle {
			get {
				return "Event";
			}
		}

		protected override MemberRecord Record (MemberInfo mi)
		{
			return new EventRecord ((EventInfo) mi);
		}

		protected override MemberInfo[] Members (BindingFlags scope) {
			return type.GetEvents ((flags & ~(BindingFlags.Static | BindingFlags.Instance)) | scope);
		}
	}

	public class FieldRecordFactory : MemberRecordFactory
	{
		public FieldRecordFactory (BindingFlags flags) : base (flags) {}

		public override string Title {
			get {
				return "fields";
			}
		}

		public override string ColumnTitle {
			get {
				return "Field";
			}
		}

		protected override MemberRecord Record (MemberInfo mi)
		{
			return new FieldRecord ((FieldInfo) mi);
		}

		protected override MemberInfo[] Members (BindingFlags scope) {
			return type.GetFields ((flags & ~(BindingFlags.Static | BindingFlags.Instance)) | scope);
		}
	}

	public class PropertyRecordFactory : MemberRecordFactory
	{
		public PropertyRecordFactory (BindingFlags flags) : base (flags) {}

		public override string Title {
			get {
				return "properties";
			}
		}

		public override string ColumnTitle {
			get {
				return "Property";
			}
		}

		protected override MemberRecord Record (MemberInfo mi)
		{
			return new PropertyRecord ((PropertyInfo) mi);
		}

		protected override MemberInfo[] Members (BindingFlags scope) {
			return type.GetProperties ((flags & ~(BindingFlags.Static | BindingFlags.Instance)) | scope);
		}
	}

	public class AllRecordFactory : MemberRecordFactory
	{
		private ArrayList list;

		public AllRecordFactory (BindingFlags flags) : base (flags)
		{
			list = new ArrayList ();
		}

		public void Add (MemberRecordFactory view)
		{
			list.Add (view);
		}

		public override Type Type {
			set {
				bool changed = type != value;
				type = value;
				foreach (MemberRecordFactory view in list)
				        view.Type = value;
				if (changed)
					Load ();
			}
		}

		public override BindingFlags Flags {
			set {
				bool changed = flags != value;
				flags = value;
				foreach (MemberRecordFactory view in list)
				        view.Flags = value;
				if (changed)
					Load ();
			}
			get {
				return base.Flags;
			}
		}

		public override string Title {
			get {
				return "all";
			}
		}

		public override string ColumnTitle {
			get {
				return "Member";
			}
		}

		public override void Load ()
		{
			if (type == null)
				Info = null;
			else {
				ArrayList infoList = new ArrayList ();
				int count = 0;

				instanceCount = staticCount = 0;
				foreach (MemberRecordFactory view in list) {
					count += view.Info.Length;
					instanceCount += view.instanceCount;
					staticCount += view.staticCount;
					infoList.AddRange (view.Info);
				}

				Info = (MemberRecord[]) infoList.ToArray (typeof (MemberRecord));
			}
		}

		protected override MemberRecord Record (MemberInfo mi)
		{
			return new MethodRecord ((MethodInfo) mi);
		}

		protected override MemberInfo[] Members (BindingFlags scope) {
			return type.GetMethods ((flags & ~(BindingFlags.Static | BindingFlags.Instance)) | scope);
		}
	}
}
