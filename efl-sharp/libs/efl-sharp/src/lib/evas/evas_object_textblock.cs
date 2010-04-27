namespace Enlightenment.Evas
{  
	using System;
	using System.Collections;
	using System.Runtime.InteropServices;
	using System.Reflection;
	using System.Threading;

	public class TextBlockCursor
	{	
		/* used to reference the C shared object */
		const string Library = "evas";

		/* provides a raw handle to the C object */
		protected HandleRef objRaw;

		/* convenience method to return the handle to the C object */
	 	public virtual IntPtr Raw
		{
			get { return objRaw.Handle; }
		}

		[DllImport(Library)]
		private extern static IntPtr evas_object_textblock_cursor_new(IntPtr textBlock);

		/* create a new cursor for a TextBlock */
		public TextBlockCursor(TextBlock textBlock)
		{
			objRaw = new HandleRef(this, evas_object_textblock_cursor_new(textBlock.Raw));
		}

		[DllImport(Library)]
		private extern static void evas_textblock_cursor_node_first(IntPtr cursor);

		public void NodeFirst()
		{
			evas_textblock_cursor_node_first(Raw);
		}

		[DllImport(Library)]
		private extern static void evas_textblock_cursor_node_last(IntPtr cursor);

		public void NodeLast()
		{
			evas_textblock_cursor_node_last(Raw);
		}

		[DllImport(Library)]
		private extern static bool evas_textblock_cursor_node_next(IntPtr cursor);

		public bool NodeNext()
		{
			return evas_textblock_cursor_node_next(Raw);
		}

		[DllImport(Library)]
		private extern static bool evas_textblock_cursor_node_prev(IntPtr cursor);

		public bool NodePrev()
		{
			return evas_textblock_cursor_node_prev(Raw);
		}

		[DllImport(Library)]
		private extern static bool evas_textblock_cursor_char_next(IntPtr cursor);

		public bool CharNext()
		{
			return evas_textblock_cursor_char_next(Raw);
		}

		[DllImport(Library)]
		private extern static bool evas_textblock_cursor_char_prev(IntPtr cursor);

		public bool CharPrev()
		{
			return evas_textblock_cursor_char_prev(Raw);
		}

		[DllImport(Library)]
		private extern static void evas_textblock_cursor_char_first(IntPtr cursor);

		public void CharFirst()
		{
			evas_textblock_cursor_char_first(Raw);
		}
		
		[DllImport(Library)]
		private extern static void evas_textblock_cursor_char_last(IntPtr cursor);

		public void CharLast()
		{
			evas_textblock_cursor_char_last(Raw);
		}

		[DllImport(Library)]
		private extern static void evas_textblock_cursor_line_first(IntPtr cursor);

		public void LineFirst()
		{
			evas_textblock_cursor_line_first(Raw);
		}

		[DllImport(Library)]
		private extern static void evas_textblock_cursor_line_last(IntPtr cursor);

		public void LineLast()
		{
			evas_textblock_cursor_node_last(Raw);
		}

		[DllImport(Library)]
		private extern static int evas_textblock_cursor_pos_get(IntPtr cursor);

		[DllImport(Library)]
		private extern static void evas_textblock_cursor_pos_set(IntPtr cursor, int position);

		public int Position
		{
			get { return evas_textblock_cursor_pos_get(Raw); }
			set { evas_textblock_cursor_pos_set(Raw, value); }
		}

		[DllImport(Library)]
		private extern static bool evas_textblock_cursor_line_set(IntPtr cursor, int line);

		public bool LineSet(int line)
		{
			return evas_textblock_cursor_line_set(Raw, line);
		}

		[DllImport(Library)]
		private extern static int evas_textblock_cursor_compare(IntPtr cursor1, IntPtr cursor2);

		public int Compare(TextBlockCursor cursor)
		{
			return evas_textblock_cursor_compare(Raw, cursor.Raw);
		}

		[DllImport(Library)]
		private extern static void evas_textblock_cursor_copy(IntPtr cursor1, IntPtr cursor2);

		public void Compeare(TextBlockCursor cursor)
		{
			evas_textblock_cursor_copy(Raw, cursor.Raw);
		}

		/* IMPLEMENT THE FOLLOWING
		 *
		 * void
		 * evas_textblock_cursor_free(Evas_Textblock_Cursor *cur);
		 *
		 *		 
		 * void
		 * evas_textblock_cursor_text_append(Evas_Textblock_Cursor *cur, const
		 * char *text);
		 *
		 *
		 * void
		 * evas_textblock_cursor_text_prepend(Evas_Textblock_Cursor *cur, const
		 * char *text);
		 *
		 *
		 * void
		 * evas_textblock_cursor_format_append(Evas_Textblock_Cursor *cur, const
		 * char *format);
		 *
		 * void
		 * evas_textblock_cursor_format_prepend(Evas_Textblock_Cursor *cur,
		 * const char *format)
		 *
		 * void
		 * evas_textblock_cursor_node_delete(Evas_Textblock_Cursor *cur);
		 *
		 *
		 * void
		 * evas_textblock_cursor_char_delete(Evas_Textblock_Cursor *cur);
		 *
		 *
		 * void
		 * evas_textblock_cursor_range_delete(Evas_Textblock_Cursor *cur1,
		 * Evas_Textblock_Cursor *cur2);
		 *
		 *
		 * const char
		 * *evas_textblock_cursor_node_text_get(const Evas_Textblock_Cursor
		 * *cur);
		 *
		 *
		 * int
		 * evas_textblock_cursor_node_text_length_get(const
		 * Evas_Textblock_Cursor *cur);
		 *
		 *
		 * const char
		 * *evas_textblock_cursor_node_format_get(const Evas_Textblock_Cursor *cur);
		 *
		 * char
		 * *evas_textblock_cursor_range_text_get(const Evas_Textblock_Cursor
		 * *cur1, const Evas_Textblock_Cursor *cur2, Evas_Textblock_Text_Type
		 * format);
		 *
		 *
		 * int
		 * evas_textblock_cursor_char_geometry_get(const Evas_Textblock_Cursor
		 * *cur, Evas_Coord *cx, Evas_Coord *cy, Evas_Coord *cw, Evas_Coord
		 * *ch);
		 *
		 *
		 * int
		 * evas_textblock_cursor_line_geometry_get(const Evas_Textblock_Cursor
		 * *cur, Evas_Coord *cx, Evas_Coord *cy, Evas_Coord *cw, Evas_Coord
		 * *ch);
		 *
		 *
		 * Evas_Bool
		 * evas_textblock_cursor_char_coord_set(Evas_Textblock_Cursor *cur,
		 * Evas_Coord x, Evas_Coord y);
		 *
		 *
		 * int
		 * evas_textblock_cursor_line_coord_set(Evas_Textblock_Cursor *cur,
		 * Evas_Coord y);
		 *
		 *
		 * Evas_List
		 * *evas_textblock_cursor_range_geometry_get(const Evas_Textblock_Cursor
		 * *cur1, const Evas_Textblock_Cursor *cur2);
		 *
		 */
	}

	/**
	 * Structure that holds the text block's coordinates.
	 */
	public struct TextBlockCoords
	{
		/**
	//printf(" moving to %d %d\n", -(multiscale_w / 2 - window_width / 2), -(multiscale_h / 2 - window_height / 2));
		 * X coord
		 */
		public int X;

		/**
		 * Y coord
		 */
		public int Y;

		/**
		 * Width
		 */
		public int W;

		/**
		 * Height
		 */
		public int H;

		/**
		 * Default constructor setting the coordinates.
		 */
		public TextBlockCoords(int x, int y, int w, int h)
		{
			X = x;
			Y = y;
			W = w;
			H = h;
		}
	}
	
	/**
	 * Structure that represents the text block's size.
	 */
	public struct TextBlockSize
	{
		/**
		 * Width
		 */
		public int W;

		/**
		 * Height
		 */
		public int H;
		
		/**
		 * Default constructor.
		 *
		 * @param w the width to set
		 * @param h the height to set
		 */
		public TextBlockSize(int w, int h)
		{
			W = w;
			H = h;
	  }
	}
	
	/**
	 * TextBlock object.
	 *
	 * <br/>Holds and lays out text and allows for effects.
	 */
	public class TextBlock : Item
	{
		const string Library = "evas";
		
		[DllImport(Library)]
		private extern static IntPtr evas_object_textblock_add(IntPtr evas);
		
		/**
		 * Default constructor
		 */
		public TextBlock()
		{}
	
		public TextBlock(Canvas c) : base(c)
	  {
	     objRaw = new HandleRef(this, evas_object_textblock_add(c.Raw));
	  }
	
		/* IMPLEMENT THE FOLLOWING
		 *
		 * void
		 * evas_object_textblock_style_set(Evas_Object *obj,
		 * Evas_Textblock_Style *ts);
		 *
		 *
		 * const Evas_Textblock_Style  *evas_object_textblock_style_get(const
		 * Evas_Object *obj);
		 *
		 *
		 * void
		 * evas_object_textblock_text_markup_set(Evas_Object *obj, const char *text);
		 *
		 *
		 * const char
		 * *evas_object_textblock_text_markup_get(const Evas_Object *obj);
		 *
		 *		
		 * const Evas_Textblock_Cursor *evas_object_textblock_cursor_get(const
		 * Evas_Object *obj);
		 *
		 *
		 * Evas_Bool
		 * evas_object_textblock_line_number_geometry_get(const Evas_Object
		 * *obj, int line, Evas_Coord *cx, Evas_Coord *cy, Evas_Coord *cw,
		 * Evas_Coord *ch);
		 *
		 *
		 * void
		 * evas_object_textblock_clear(Evas_Object *obj);
		 *
		 *
		 * void
		 * evas_object_textblock_size_formatted_get(const Evas_Object *obj,
		 * Evas_Coord *w, Evas_Coord *h);
		 *
		 *
		 * void
		 * evas_object_textblock_size_native_get(const Evas_Object *obj,
		 * Evas_Coord *w, Evas_Coord *h);
		 *
		 *
		 * void
		 * evas_object_textblock_style_insets_get(const Evas_Object *obj,
		 * Evas_Coord *l, Evas_Coord *r, Evas_Coord *t, Evas_Coord *b);
		 *
		 */		
	}
}
