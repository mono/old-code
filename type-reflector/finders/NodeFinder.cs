//
// NodeFinder.cs: Finds sub-nodes for a given NodeInfo object.
//
// Author: Jonathan Pryor (jonpryor@vt.edu)
//
// (C) 2002 Jonathan Pryor
//

using System;
using System.Diagnostics;
using System.Reflection;

namespace Mono.TypeReflector.Finders
{
	public delegate void BaseTypeEventHandler (object sender, BaseTypeEventArgs e);
	public delegate void TypeEventHandler (object sender, TypeEventArgs e);
	public delegate void InterfacesEventHandler (object sender, InterfacesEventArgs e);
	public delegate void FieldsEventHandler (object sender, FieldsEventArgs e);
	public delegate void PropertiesEventHandler (object sender, PropertiesEventArgs e);
	public delegate void EventsEventHandler (object sender, EventsEventArgs e);
	public delegate void ConstructorsEventHandler (object sender, ConstructorsEventArgs e);
	public delegate void MethodsEventHandler (object sender, MethodsEventArgs e);

	public class NodeFoundEventArgs : EventArgs {
		private NodeInfo _node;

		internal NodeFoundEventArgs (NodeInfo node)
		{
			_node = node;
		}

		public NodeInfo NodeInfo {
			get {return _node;}
		}
	}

	public class BaseTypeEventArgs : NodeFoundEventArgs {

		private Type _base;

		internal BaseTypeEventArgs (NodeInfo node, Type type)
			: base(node)
		{
			_base = type;
		}

		public Type BaseType {
			get {return _base;}
		}
	}

	public class TypeEventArgs : NodeFoundEventArgs {

		private Type _type;

		internal TypeEventArgs (NodeInfo node, Type type)
			: base(node)
		{
			_type = type;
		}

		public Type Type {
			get {return _type;}
		}
	}

	public class InterfacesEventArgs : NodeFoundEventArgs {

		private Type[] _interfaces;

		internal InterfacesEventArgs (NodeInfo node, Type[] interfaces)
			: base(node)
		{
			_interfaces = interfaces ;
		}

		public Type[] Interfaces {
			get {return _interfaces;}
		}
	}

	public class FieldsEventArgs : NodeFoundEventArgs {
		private FieldInfo[] _fields;

		internal FieldsEventArgs (NodeInfo node, FieldInfo[] fields)
			: base(node)
		{
			_fields = fields;
		}

		public FieldInfo[] Fields {
			get {return _fields;}
		}
	}

	public class PropertiesEventArgs : NodeFoundEventArgs {

		private PropertyInfo[] _props;

		internal PropertiesEventArgs (NodeInfo node, PropertyInfo[] properties)
			: base(node)
		{
			_props = properties;
		}

		public PropertyInfo[] Properties {
			get {return _props;}
		}
	}

	public class EventsEventArgs : NodeFoundEventArgs {

		private EventInfo[] _events;

		internal EventsEventArgs (NodeInfo node, EventInfo[] events)
			: base(node)
		{
			_events = events;
		}

		public EventInfo[] Events {
			get {return _events;}
		}
	}

	public class ConstructorsEventArgs : NodeFoundEventArgs {

		private ConstructorInfo[] _ctors;

		internal ConstructorsEventArgs (NodeInfo node, ConstructorInfo[] ctors)
			: base(node)
		{
			_ctors = ctors;
		}

		public ConstructorInfo[] Constructors {
			get {return _ctors;}
		}
	}

	public class MethodsEventArgs : NodeFoundEventArgs {

		private MethodInfo[] _methods;

		internal MethodsEventArgs (NodeInfo node, MethodInfo[] methods)
			: base(node)
		{
			_methods = methods;
		}

		public MethodInfo[] Methods {
			get {return _methods;}
		}
	}

	public abstract class NodeFinder : Policy, INodeFinder {

		private static BooleanSwitch info = 
			new BooleanSwitch ("node-finder", "NodeFinder messages");

		private BindingFlags bindingFlags = 
			BindingFlags.DeclaredOnly |
			BindingFlags.Public |
			BindingFlags.Instance |
			BindingFlags.Static;

		private FindMemberTypes members;

		public BindingFlags BindingFlags {
			get {return bindingFlags;}
			set {bindingFlags = value;}
		}

		public FindMemberTypes FindMembers {
			get {return members;}
			set {members = value;}
		}

		protected bool ShowBase {
			get {return (members & FindMemberTypes.Base) != 0;}
		}

		protected bool ShowConstructors {
			get {return (members & FindMemberTypes.Constructors) != 0;}
		}

		protected bool ShowEvents {
			get {return (members & FindMemberTypes.Events) != 0;}
		}

		protected bool ShowFields {
			get {return (members & FindMemberTypes.Fields) != 0;}
		}

		protected bool ShowInterfaces {
			get {return (members & FindMemberTypes.Interfaces) != 0;}
		}

		protected bool ShowMethods {
			get {return (members & FindMemberTypes.Methods) != 0;}
		}

		protected bool ShowProperties {
			get {return (members & FindMemberTypes.Properties) != 0;}
		}

		protected bool ShowTypeProperties {
			get {return (members & FindMemberTypes.TypeProperties) != 0;}
		}

		protected bool ShowMonoBroken {
			get {return (members & FindMemberTypes.MonoBroken) != 0;}
		}

		protected bool VerboseOutput {
			get {return (members & FindMemberTypes.VerboseOutput) != 0;}
		}

		public virtual NodeInfoCollection GetChildren (NodeInfo root)
		{
			Trace.WriteLineIf (info.Enabled, "NodeFinder.GetChildren");
			NodeInfoCollection c = new NodeInfoCollection ();

			// always handle NodeTypes.Type
			if (root.NodeType == NodeTypes.Type)
				AddTypeChildren (c, root, (Type) root.ReflectionObject);
			else if (VerboseOutput) {
				switch (root.NodeType) {
					case NodeTypes.BaseType:
						AddBaseTypeChildren (c, root, (Type) root.ReflectionObject);
						break;
					case NodeTypes.Interface:
						AddInterfaceChildren (c, root, (Type) root.ReflectionObject);
						break;
					case NodeTypes.Field:
						AddFieldChildren (c, root, (FieldInfo) root.ReflectionObject);
						break;
					case NodeTypes.Constructor:
						AddConstructorChildren (c, root, (ConstructorInfo) root.ReflectionObject);
						break;
					case NodeTypes.Method:
						AddMethodChildren (c, root, (MethodInfo) root.ReflectionObject);
						break;
					case NodeTypes.Parameter:
						AddParameterChildren (c, root, (ParameterInfo) root.ReflectionObject);
						break;
					case NodeTypes.Property:
						AddPropertyChildren (c, root, (PropertyInfo) root.ReflectionObject);
						break;
					case NodeTypes.Event:
						AddEventChildren (c, root, (EventInfo) root.ReflectionObject);
						break;
					case NodeTypes.ReturnValue:
						AddReturnValueChildren (c, root);
						break;
					case NodeTypes.Other:
					case NodeTypes.Alias:
						AddOtherChildren (c, root);
						break;
					default:
						AddUnhandledChildren (c, root);
						break;
				}
			}
			return c;
		}

		protected virtual void AddTypeChildren (NodeInfoCollection c, NodeInfo root, Type type)
		{
		}

		protected virtual void AddBaseTypeChildren (NodeInfoCollection c, NodeInfo root, Type baseType)
		{
		}

		protected virtual void AddInterfaceChildren (NodeInfoCollection c, NodeInfo root, Type iface)
		{
		}

		protected virtual void AddFieldChildren (NodeInfoCollection c, NodeInfo root, FieldInfo field)
		{
		}

		protected virtual void AddConstructorChildren (NodeInfoCollection c, NodeInfo root, ConstructorInfo ctor)
		{
		}

		protected virtual void AddMethodChildren (NodeInfoCollection c, NodeInfo root, MethodInfo method)
		{
		}

		protected virtual void AddParameterChildren (NodeInfoCollection c, NodeInfo root, ParameterInfo param)
		{
		}

		protected virtual void AddPropertyChildren (NodeInfoCollection c, NodeInfo root, PropertyInfo property)
		{
		}

		protected virtual void AddEventChildren (NodeInfoCollection c, NodeInfo root, EventInfo e)
		{
		}

		protected virtual void AddReturnValueChildren (NodeInfoCollection c, NodeInfo root)
		{
			if (root.ReflectionObject != null)
				AddTypeChildren (c, root, (Type) root.ReflectionObject);
		}

		protected virtual void AddOtherChildren (NodeInfoCollection c, NodeInfo root)
		{
			if (root.Description is NodeGroup) {
				NodeGroup g = (NodeGroup) root.Description;
				g.Invoke (c, root);
			}
		}

		protected virtual void AddUnhandledChildren (NodeInfoCollection c, NodeInfo root)
		{
			c.Add (new NodeInfo (root, "Unhandled child: NodeType=" + root.NodeType));
		}

		public event TypeEventHandler         Types;
		public event BaseTypeEventHandler     BaseType;
		public event InterfacesEventHandler   Interfaces;
		public event FieldsEventHandler       Fields;
		public event PropertiesEventHandler   Properties;
		public event EventsEventHandler       Events;
		public event ConstructorsEventHandler Constructors;
		public event MethodsEventHandler      Methods;
	}
}

