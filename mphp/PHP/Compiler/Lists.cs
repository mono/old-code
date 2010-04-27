using System;
using System.Collections;


namespace PHP.Compiler {


	public class StatementList : Statement {

		private ArrayList List;

		public StatementList() : base(0, 0) {
			List = new ArrayList();
		}

		public StatementList(Statement s)
			: this() {
			List.Add(s);
		}

		public void Add(Statement s) {
			List.Add(s);
		}

		public void AddRange(StatementList stmtList) {
			foreach (Statement stmt in stmtList)
				List.Add(stmt);
		}

		public void Remove(Statement stmt) {
			List.Remove(stmt);
		}

		public void RemoveRange(StatementList stmtList) {
			foreach (Statement stmt in stmtList)
				List.Remove(stmt);
		}

		public IEnumerator GetEnumerator() {
			return List.GetEnumerator();
		}

		public int Count() {
			return List.Count;
		}

		public Statement Get(int i) {
			object result = List[i];
			if (result == null)
				return null;
			else
				return (Statement)result;
		}

	}



	public class ExpressionList {

		private ArrayList List;

		public ExpressionList() {
			List = new ArrayList();
		}

		public ExpressionList(Expression e)
			: this() {
			List.Add(e);
		}

		public void Add(Expression e) {
			List.Add(e);
		}

		public void AddRange(ExpressionList exprList) {
			foreach (Expression expr in exprList)
				List.Add(expr);
		}

		public void Remove(Expression expr) {
			List.Remove(expr);
		}

		public void RemoveRange(ExpressionList exprList) {
			foreach (Expression expr in exprList)
				List.Remove(expr);
		}

		public IEnumerator GetEnumerator() {
			return List.GetEnumerator();
		}

		public int Count() {
			return List.Count;
		}

		public Expression Get(int i) {
			object result = List[i];
			if (result == null)
				return null;
			else
				return (Expression)result;
		}

	}


}