
using System;
using System.Collections;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System.Reflection;


public abstract class Node : Ice.Object {
  public abstract int Eval();
}

public abstract class TreeNode : Node {
  public Node left;
  public Node right;
}

public class IntNode : Node {
  public int value;
  public IntNode() { }
  public IntNode(int j) { value = j; }
  public override int Eval() { return value; }
}

public class PlusNode : TreeNode {
  public override int Eval() {
    int l = left.Eval();
    int r = right.Eval();
    return l + r;
  }
}

public class MultNode : TreeNode {
  public override int Eval() {
    int l = left.Eval();
    int r = right.Eval();
    return l * r;
  }
}

public class DivNode : TreeNode {
  public override int Eval() {
    int l = left.Eval();
    int r = right.Eval();
    return l / r;
  }
}

public abstract class NodeEvaluator : Ice.Object {
  public abstract int EvalNode (Node n);
}

public class NodeEvaluatorI : NodeEvaluator {
  public override int EvalNode (Node n) {
    return n.Eval();
  }
}

public class Driver {
  public static Node BuildExpr() {
    MultNode tn = new MultNode();

    PlusNode pn = new PlusNode();
    pn.left = new IntNode (5);
    pn.right = new IntNode (4);

    tn.left = pn;
    tn.right = pn;

    return tn;
  }

  public static Hashtable ht;

  public static void Main (string[] args) {
    Node n = BuildExpr();

    if (args.Length != 1) {
      Console.WriteLine ("Specify 'server' or 'client' as first argument");
      return;
    }

    if (args[0] == "client") {
      Ice.IceClientChannel ic = new Ice.IceClientChannel();
      ChannelServices.RegisterChannel (ic);

      NodeEvaluator ne = (NodeEvaluator) Activator.GetObject (typeof (NodeEvaluator),
							      "ice://localhost:10000/ne");

      int res = ne.EvalNode (n);
      Console.WriteLine ("Result: {0}", res);
    } else if (args[0] == "server") {
      Ice.IceChannel ic = new Ice.IceChannel(10000);
      ChannelServices.RegisterChannel (ic);

      RemotingConfiguration.RegisterWellKnownServiceType (typeof (NodeEvaluatorI),
							  "ne",
							  WellKnownObjectMode.Singleton);
      Console.ReadLine();
    }
  }
}
