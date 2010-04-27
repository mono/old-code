// MetaTableColumn.cs
//
// Author:
//     Daniel Morgan <danielmorgan@verizon.net>
//
// (C)Copyright 2004 by Daniel Morgan
//
// To be included with Mono as a SQL query tool licensed under the LGPL license.
//

using System;
using System.Data;

namespace Mono.Data.SqlSharp.DatabaseBrowser
{
	public class MetaTableColumn
	{
		private string name = "";
		private string owner = "";
		private string table = "";

		private string dataType = "";

		private int length = 0;
		private int precision = 0;
		private int scale = 0;
		
		private bool nullable = false;

		private int column_id = 0;

		public MetaTableColumn()
		{
		}

		public MetaTableColumn(string tableName, string columnName)
		{
			this.table = tableName;
			this.name = columnName;
			
		}

		public MetaTableColumn(string tableOwner, string tableName, string columnName)
		{
			this.owner = tableOwner;
			this.table = tableName;
			this.name = columnName;

		}

		public MetaTableColumn(string tableOwner, string tableName, string columnName,
			int length, bool nullable, int columnid)
		{
			this.owner = tableOwner;
			this.table = tableName;
			this.name = columnName;
			this.length = length;
			this.nullable = nullable;
			this.column_id = columnid;
		}

		public MetaTableColumn(string tableOwner, string tableName, string columnName,
			int precision, int scale, bool nullable, int columnid)
		{
			this.owner = tableOwner;
			this.table = tableName;
			this.name = columnName;
			this.precision = precision;
			this.scale = scale;
			this.nullable = nullable;
			this.column_id = columnid;
		}

		public MetaTableColumn(string tableOwner, string tableName, string columnName,
			string data_type,
			int length, int precision, int scale, bool nullable, int columnid)
		{
			this.owner = tableOwner;
			this.table = tableName;
			this.name = columnName;
			this.dataType = data_type;
			this.length = length;
			this.precision = precision;
			this.scale = scale;
			this.nullable = nullable;
			this.column_id = columnid;
		}

		public string Name 
		{
			get 
			{
				return name;
			}
			
			set 
			{
				name = value;
			}
		}

		public string Owner 
		{
			get 
			{
				return owner;
			}

			set 
			{
				owner = value;
			}
		}

		public string TableName
		{
			get 
			{
				return table;
			}

			set 
			{
				table = value;
			}
		}

		public string DataType 
		{
			get 
			{
				return dataType;
			}

			set 
			{
				dataType = value;
			}
		}

		public int Length 
		{
			get 
			{
				return length;
			}

			set 
			{
				length = value;
			}
		}

		public int Precision 
		{
			get 
			{
				return precision;
			}

			set 
			{
				precision = value;
			}
		}

		public int Scale 
		{
			get 
			{
				return scale;
			}

			set 
			{
				scale = value;
			}
		}

		public bool Nullable 
		{
			get 
			{
				return nullable;
			}

			set 
			{
				nullable = value;
			}
		}

		public int ColumnID 
		{
			get 
			{
				return column_id;
			}

			set 
			{
				column_id = value;
			}
		}

		public override string ToString() 
		{
			if(owner != null)
				if(owner.Equals("") == false)
					return owner + "." + table + "." + name;

			return table + "." + name;
		}
	}
}

