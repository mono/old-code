// created by jay 0.7 (c) 1998 Axel.Schreiner@informatik.uni-osnabrueck.de

#line 2 "cs-parser.jay"
//
// cs-parser.jay: The Parser for the C# compiler
//
// Authors: Miguel de Icaza (miguel@gnu.org)
//          Ravi Pratap     (ravi@ximian.com)
//          Radek Doulik    (rodo@matfyz.cz)
//
// Licensed under the terms of the GNU GPL
//
// (C) 2001 Ximian, Inc (http://www.ximian.com)
// (C) 2003 Radek Doulik

using System.Text;
using System.IO;
using System;

namespace Monotalk.CSharp
{
	using System.Collections;
	using Monotalk.Languages;
	using Monotalk.Indexer;

	/// <summary>
	///    The C# Parser
	/// </summary>
	public class CSharpParser : GenericParser
	{

		Stack Namespaces;
		string Namespace;

		Stack Types;
		string type;

		Stack Rows;

		SourceDB db;
		int fileID;

		bool modStatic;
#line 45 "-"

  /** simplified error message.
      @see <a href="#yyerror(java.lang.String, java.lang.String[])">yyerror</a>
    */
  public void yyerror (string message) {
    yyerror(message, null);
  }

  /** (syntax) error message.
      Can be overwritten to control message format.
      @param message text to be displayed.
      @param expected vector of acceptable tokens, if available.
    */
  public void yyerror (string message, string[] expected) {
    if ((expected != null) && (expected.Length  > 0)) {
      System.Console.Write (message+", expecting");
      for (int n = 0; n < expected.Length; ++ n)
        System.Console.Write (" "+expected[n]);
        System.Console.WriteLine ();
    } else
      System.Console.WriteLine (message);
  }

  /** debugging support, requires the package jay.yydebug.
      Set to null to suppress debugging messages.
    */
  protected yydebug.yyDebug debug;

  protected static  int yyFinal = 4;
  public static  string [] yyRule = {
    "$accept : compilation_unit",
    "compilation_unit : outer_declarations opt_EOF",
    "compilation_unit : outer_declarations attribute_sections opt_EOF",
    "compilation_unit : attribute_sections opt_EOF",
    "compilation_unit : opt_EOF",
    "opt_EOF :",
    "opt_EOF : EOF",
    "outer_declarations : outer_declaration",
    "outer_declarations : outer_declarations outer_declaration",
    "outer_declaration : using_directive",
    "outer_declaration : namespace_member_declaration",
    "using_directives : using_directive",
    "using_directives : using_directives using_directive",
    "using_directive : using_alias_directive",
    "using_directive : using_namespace_directive",
    "using_alias_directive : USING IDENTIFIER ASSIGN namespace_or_type_name SEMICOLON",
    "using_namespace_directive : USING namespace_name SEMICOLON",
    "$$1 :",
    "namespace_declaration : opt_attributes NAMESPACE qualified_identifier $$1 namespace_body opt_semicolon",
    "opt_semicolon :",
    "opt_semicolon : SEMICOLON",
    "opt_comma :",
    "opt_comma : COMMA",
    "qualified_identifier : IDENTIFIER",
    "qualified_identifier : qualified_identifier DOT IDENTIFIER",
    "namespace_name : namespace_or_type_name",
    "namespace_body : OPEN_BRACE opt_using_directives opt_namespace_member_declarations CLOSE_BRACE",
    "opt_using_directives :",
    "opt_using_directives : using_directives",
    "opt_namespace_member_declarations :",
    "opt_namespace_member_declarations : namespace_member_declarations",
    "namespace_member_declarations : namespace_member_declaration",
    "namespace_member_declarations : namespace_member_declarations namespace_member_declaration",
    "namespace_member_declaration : type_declaration",
    "namespace_member_declaration : namespace_declaration",
    "type_declaration : class_declaration",
    "type_declaration : struct_declaration",
    "type_declaration : interface_declaration",
    "type_declaration : enum_declaration",
    "type_declaration : delegate_declaration",
    "opt_attributes :",
    "opt_attributes : attribute_sections",
    "attribute_sections : attribute_section",
    "attribute_sections : attribute_sections attribute_section",
    "attribute_section : OPEN_BRACKET attribute_target_specifier attribute_list opt_comma CLOSE_BRACKET",
    "attribute_section : OPEN_BRACKET attribute_list opt_comma CLOSE_BRACKET",
    "attribute_target_specifier : attribute_target COLON",
    "attribute_target : IDENTIFIER",
    "attribute_target : EVENT",
    "attribute_target : RETURN",
    "attribute_list : attribute",
    "attribute_list : attribute_list COMMA attribute",
    "attribute : attribute_name opt_attribute_arguments",
    "attribute_name : type_name",
    "opt_attribute_arguments :",
    "opt_attribute_arguments : OPEN_PARENS attribute_arguments CLOSE_PARENS",
    "attribute_arguments : opt_positional_argument_list",
    "attribute_arguments : positional_argument_list COMMA named_argument_list",
    "attribute_arguments : named_argument_list",
    "opt_positional_argument_list :",
    "opt_positional_argument_list : positional_argument_list",
    "positional_argument_list : expression",
    "positional_argument_list : positional_argument_list COMMA expression",
    "named_argument_list : named_argument",
    "named_argument_list : named_argument_list COMMA named_argument",
    "named_argument : IDENTIFIER ASSIGN expression",
    "class_body : OPEN_BRACE opt_class_member_declarations CLOSE_BRACE",
    "opt_class_member_declarations :",
    "opt_class_member_declarations : class_member_declarations",
    "class_member_declarations : class_member_declaration",
    "class_member_declarations : class_member_declarations class_member_declaration",
    "class_member_declaration : constant_declaration",
    "class_member_declaration : field_declaration",
    "class_member_declaration : method_declaration",
    "class_member_declaration : property_declaration",
    "class_member_declaration : event_declaration",
    "class_member_declaration : indexer_declaration",
    "class_member_declaration : operator_declaration",
    "class_member_declaration : constructor_declaration",
    "class_member_declaration : destructor_declaration",
    "class_member_declaration : type_declaration",
    "struct_declaration : opt_attributes opt_modifiers STRUCT IDENTIFIER opt_class_base struct_body opt_semicolon",
    "struct_body : OPEN_BRACE opt_struct_member_declarations CLOSE_BRACE",
    "opt_struct_member_declarations :",
    "opt_struct_member_declarations : struct_member_declarations",
    "struct_member_declarations : struct_member_declaration",
    "struct_member_declarations : struct_member_declarations struct_member_declaration",
    "struct_member_declaration : constant_declaration",
    "struct_member_declaration : field_declaration",
    "struct_member_declaration : method_declaration",
    "struct_member_declaration : property_declaration",
    "struct_member_declaration : event_declaration",
    "struct_member_declaration : indexer_declaration",
    "struct_member_declaration : operator_declaration",
    "struct_member_declaration : constructor_declaration",
    "struct_member_declaration : type_declaration",
    "struct_member_declaration : destructor_declaration",
    "constant_declaration : opt_attributes opt_modifiers CONST type constant_declarators SEMICOLON",
    "constant_declarators : constant_declarator",
    "constant_declarators : constant_declarators COMMA constant_declarator",
    "constant_declarator : IDENTIFIER ASSIGN constant_expression",
    "field_declaration : opt_attributes opt_modifiers type variable_declarators SEMICOLON",
    "variable_declarators : variable_declarator",
    "variable_declarators : variable_declarators COMMA variable_declarator",
    "variable_declarator : IDENTIFIER ASSIGN variable_initializer",
    "variable_declarator : IDENTIFIER",
    "variable_initializer : expression",
    "variable_initializer : array_initializer",
    "variable_initializer : STACKALLOC type OPEN_BRACKET expression CLOSE_BRACKET",
    "method_declaration : method_header method_body",
    "opt_error_modifier :",
    "opt_error_modifier : modifiers",
    "method_header : opt_attributes opt_modifiers type member_name OPEN_PARENS opt_formal_parameter_list CLOSE_PARENS",
    "method_header : opt_attributes opt_modifiers VOID member_name OPEN_PARENS opt_formal_parameter_list CLOSE_PARENS",
    "method_body : block",
    "method_body : SEMICOLON",
    "opt_formal_parameter_list :",
    "opt_formal_parameter_list : formal_parameter_list",
    "formal_parameter_list : fixed_parameters",
    "formal_parameter_list : fixed_parameters COMMA parameter_array",
    "formal_parameter_list : parameter_array",
    "fixed_parameters : fixed_parameter",
    "fixed_parameters : fixed_parameters COMMA fixed_parameter",
    "fixed_parameter : opt_attributes opt_parameter_modifier type IDENTIFIER",
    "opt_parameter_modifier :",
    "opt_parameter_modifier : parameter_modifier",
    "parameter_modifier : REF",
    "parameter_modifier : OUT",
    "parameter_array : opt_attributes PARAMS type IDENTIFIER",
    "member_name : qualified_identifier",
    "$$2 :",
    "$$3 :",
    "$$4 :",
    "property_declaration : opt_attributes opt_modifiers type member_name $$2 OPEN_BRACE $$3 accessor_declarations $$4 CLOSE_BRACE",
    "accessor_declarations : get_accessor_declaration opt_set_accessor_declaration",
    "accessor_declarations : set_accessor_declaration opt_get_accessor_declaration",
    "opt_get_accessor_declaration :",
    "opt_get_accessor_declaration : get_accessor_declaration",
    "opt_set_accessor_declaration :",
    "opt_set_accessor_declaration : set_accessor_declaration",
    "$$5 :",
    "get_accessor_declaration : opt_attributes GET $$5 accessor_body",
    "$$6 :",
    "set_accessor_declaration : opt_attributes SET $$6 accessor_body",
    "accessor_body : block",
    "accessor_body : SEMICOLON",
    "interface_declaration : opt_attributes opt_modifiers INTERFACE IDENTIFIER opt_interface_base interface_body opt_semicolon",
    "opt_interface_base :",
    "opt_interface_base : interface_base",
    "interface_base : COLON interface_type_list",
    "interface_type_list : interface_type",
    "interface_type_list : interface_type_list COMMA interface_type",
    "interface_body : OPEN_BRACE opt_interface_member_declarations CLOSE_BRACE",
    "opt_interface_member_declarations :",
    "opt_interface_member_declarations : interface_member_declarations",
    "interface_member_declarations : interface_member_declaration",
    "interface_member_declarations : interface_member_declarations interface_member_declaration",
    "interface_member_declaration : interface_method_declaration",
    "interface_member_declaration : interface_property_declaration",
    "interface_member_declaration : interface_event_declaration",
    "interface_member_declaration : interface_indexer_declaration",
    "opt_new :",
    "opt_new : NEW",
    "interface_method_declaration : opt_attributes opt_new type IDENTIFIER OPEN_PARENS opt_formal_parameter_list CLOSE_PARENS SEMICOLON",
    "interface_method_declaration : opt_attributes opt_new VOID IDENTIFIER OPEN_PARENS opt_formal_parameter_list CLOSE_PARENS SEMICOLON",
    "$$7 :",
    "$$8 :",
    "interface_property_declaration : opt_attributes opt_new type IDENTIFIER OPEN_BRACE $$7 interface_accesors $$8 CLOSE_BRACE",
    "interface_accesors : opt_attributes GET SEMICOLON",
    "interface_accesors : opt_attributes SET SEMICOLON",
    "interface_accesors : opt_attributes GET SEMICOLON opt_attributes SET SEMICOLON",
    "interface_accesors : opt_attributes SET SEMICOLON opt_attributes GET SEMICOLON",
    "interface_event_declaration : opt_attributes opt_new EVENT type IDENTIFIER SEMICOLON",
    "$$9 :",
    "$$10 :",
    "interface_indexer_declaration : opt_attributes opt_new type THIS OPEN_BRACKET formal_parameter_list CLOSE_BRACKET OPEN_BRACE $$9 interface_accesors $$10 CLOSE_BRACE",
    "operator_declaration : opt_attributes opt_modifiers operator_declarator operator_body",
    "operator_body : block",
    "operator_body : SEMICOLON",
    "operator_declarator : type OPERATOR overloadable_operator OPEN_PARENS type IDENTIFIER CLOSE_PARENS",
    "operator_declarator : type OPERATOR overloadable_operator OPEN_PARENS type IDENTIFIER COMMA type IDENTIFIER CLOSE_PARENS",
    "operator_declarator : conversion_operator_declarator",
    "overloadable_operator : BANG",
    "overloadable_operator : TILDE",
    "overloadable_operator : OP_INC",
    "overloadable_operator : OP_DEC",
    "overloadable_operator : TRUE",
    "overloadable_operator : FALSE",
    "overloadable_operator : PLUS",
    "overloadable_operator : MINUS",
    "overloadable_operator : STAR",
    "overloadable_operator : DIV",
    "overloadable_operator : PERCENT",
    "overloadable_operator : BITWISE_AND",
    "overloadable_operator : BITWISE_OR",
    "overloadable_operator : CARRET",
    "overloadable_operator : OP_SHIFT_LEFT",
    "overloadable_operator : OP_SHIFT_RIGHT",
    "overloadable_operator : OP_EQ",
    "overloadable_operator : OP_NE",
    "overloadable_operator : OP_GT",
    "overloadable_operator : OP_LT",
    "overloadable_operator : OP_GE",
    "overloadable_operator : OP_LE",
    "conversion_operator_declarator : IMPLICIT OPERATOR type OPEN_PARENS type IDENTIFIER CLOSE_PARENS",
    "conversion_operator_declarator : EXPLICIT OPERATOR type OPEN_PARENS type IDENTIFIER CLOSE_PARENS",
    "conversion_operator_declarator : IMPLICIT error",
    "conversion_operator_declarator : EXPLICIT error",
    "constructor_declaration : opt_attributes opt_modifiers constructor_declarator constructor_body",
    "constructor_declarator : IDENTIFIER OPEN_PARENS opt_formal_parameter_list CLOSE_PARENS opt_constructor_initializer",
    "constructor_body : block",
    "constructor_body : SEMICOLON",
    "opt_constructor_initializer :",
    "opt_constructor_initializer : constructor_initializer",
    "constructor_initializer : COLON BASE OPEN_PARENS opt_argument_list CLOSE_PARENS",
    "constructor_initializer : COLON THIS OPEN_PARENS opt_argument_list CLOSE_PARENS",
    "destructor_declaration : opt_attributes TILDE IDENTIFIER OPEN_PARENS CLOSE_PARENS block",
    "event_declaration : opt_attributes opt_modifiers EVENT type variable_declarators SEMICOLON",
    "$$11 :",
    "$$12 :",
    "event_declaration : opt_attributes opt_modifiers EVENT type member_name OPEN_BRACE $$11 event_accessor_declarations $$12 CLOSE_BRACE",
    "event_accessor_declarations : add_accessor_declaration remove_accessor_declaration",
    "event_accessor_declarations : remove_accessor_declaration add_accessor_declaration",
    "$$13 :",
    "add_accessor_declaration : opt_attributes ADD $$13 block",
    "$$14 :",
    "remove_accessor_declaration : opt_attributes REMOVE $$14 block",
    "$$15 :",
    "$$16 :",
    "indexer_declaration : opt_attributes opt_modifiers indexer_declarator OPEN_BRACE $$15 accessor_declarations $$16 CLOSE_BRACE",
    "indexer_declarator : type THIS OPEN_BRACKET opt_formal_parameter_list CLOSE_BRACKET",
    "indexer_declarator : type qualified_identifier DOT THIS OPEN_BRACKET opt_formal_parameter_list CLOSE_BRACKET",
    "enum_declaration : opt_attributes opt_modifiers ENUM IDENTIFIER opt_enum_base enum_body opt_semicolon",
    "opt_enum_base :",
    "opt_enum_base : COLON type",
    "enum_body : OPEN_BRACE opt_enum_member_declarations CLOSE_BRACE",
    "opt_enum_member_declarations :",
    "opt_enum_member_declarations : enum_member_declarations opt_comma",
    "enum_member_declarations : enum_member_declaration",
    "enum_member_declarations : enum_member_declarations COMMA enum_member_declaration",
    "enum_member_declaration : opt_attributes IDENTIFIER",
    "enum_member_declaration : opt_attributes IDENTIFIER ASSIGN expression",
    "delegate_declaration : opt_attributes opt_modifiers DELEGATE type IDENTIFIER OPEN_PARENS opt_formal_parameter_list CLOSE_PARENS SEMICOLON",
    "delegate_declaration : opt_attributes opt_modifiers DELEGATE VOID IDENTIFIER OPEN_PARENS opt_formal_parameter_list CLOSE_PARENS SEMICOLON",
    "type_name : namespace_or_type_name",
    "namespace_or_type_name : qualified_identifier",
    "type : type_name",
    "type : builtin_types",
    "type : array_type",
    "type : pointer_type",
    "pointer_type : type STAR",
    "pointer_type : VOID STAR",
    "non_expression_type : builtin_types",
    "non_expression_type : non_expression_type rank_specifier",
    "non_expression_type : non_expression_type STAR",
    "non_expression_type : expression rank_specifiers",
    "non_expression_type : expression STAR",
    "non_expression_type : multiplicative_expression STAR",
    "type_list : type",
    "type_list : type_list COMMA type",
    "builtin_types : OBJECT",
    "builtin_types : STRING",
    "builtin_types : BOOL",
    "builtin_types : DECIMAL",
    "builtin_types : FLOAT",
    "builtin_types : DOUBLE",
    "builtin_types : integral_type",
    "integral_type : SBYTE",
    "integral_type : BYTE",
    "integral_type : SHORT",
    "integral_type : USHORT",
    "integral_type : INT",
    "integral_type : UINT",
    "integral_type : LONG",
    "integral_type : ULONG",
    "integral_type : CHAR",
    "integral_type : VOID",
    "interface_type : type_name",
    "array_type : type rank_specifiers",
    "primary_expression : literal",
    "primary_expression : qualified_identifier",
    "primary_expression : parenthesized_expression",
    "primary_expression : member_access",
    "primary_expression : invocation_expression",
    "primary_expression : element_access",
    "primary_expression : this_access",
    "primary_expression : base_access",
    "primary_expression : post_increment_expression",
    "primary_expression : post_decrement_expression",
    "primary_expression : new_expression",
    "primary_expression : typeof_expression",
    "primary_expression : sizeof_expression",
    "primary_expression : checked_expression",
    "primary_expression : unchecked_expression",
    "primary_expression : pointer_member_access",
    "literal : boolean_literal",
    "literal : integer_literal",
    "literal : real_literal",
    "literal : LITERAL_CHARACTER",
    "literal : LITERAL_STRING",
    "literal : NULL",
    "real_literal : LITERAL_FLOAT",
    "real_literal : LITERAL_DOUBLE",
    "real_literal : LITERAL_DECIMAL",
    "integer_literal : LITERAL_INTEGER",
    "boolean_literal : TRUE",
    "boolean_literal : FALSE",
    "parenthesized_expression : OPEN_PARENS expression CLOSE_PARENS",
    "member_access : primary_expression DOT IDENTIFIER",
    "member_access : predefined_type DOT IDENTIFIER",
    "predefined_type : builtin_types",
    "invocation_expression : primary_expression OPEN_PARENS opt_argument_list CLOSE_PARENS",
    "opt_argument_list :",
    "opt_argument_list : argument_list",
    "argument_list : argument",
    "argument_list : argument_list COMMA argument",
    "argument : expression",
    "argument : REF variable_reference",
    "argument : OUT variable_reference",
    "variable_reference : expression",
    "element_access : primary_expression OPEN_BRACKET expression_list CLOSE_BRACKET",
    "element_access : primary_expression rank_specifiers",
    "expression_list : expression",
    "expression_list : expression_list COMMA expression",
    "this_access : THIS",
    "base_access : BASE DOT IDENTIFIER",
    "base_access : BASE OPEN_BRACKET expression_list CLOSE_BRACKET",
    "post_increment_expression : primary_expression OP_INC",
    "post_decrement_expression : primary_expression OP_DEC",
    "new_expression : object_or_delegate_creation_expression",
    "new_expression : array_creation_expression",
    "object_or_delegate_creation_expression : NEW type OPEN_PARENS opt_argument_list CLOSE_PARENS",
    "array_creation_expression : NEW type OPEN_BRACKET expression_list CLOSE_BRACKET opt_rank_specifier opt_array_initializer",
    "array_creation_expression : NEW type rank_specifiers array_initializer",
    "array_creation_expression : NEW type error",
    "opt_rank_specifier :",
    "opt_rank_specifier : rank_specifiers",
    "rank_specifiers : rank_specifier opt_rank_specifier",
    "rank_specifier : OPEN_BRACKET opt_dim_separators CLOSE_BRACKET",
    "opt_dim_separators :",
    "opt_dim_separators : dim_separators",
    "dim_separators : COMMA",
    "dim_separators : dim_separators COMMA",
    "opt_array_initializer :",
    "opt_array_initializer : array_initializer",
    "array_initializer : OPEN_BRACE CLOSE_BRACE",
    "array_initializer : OPEN_BRACE variable_initializer_list opt_comma CLOSE_BRACE",
    "variable_initializer_list : variable_initializer",
    "variable_initializer_list : variable_initializer_list COMMA variable_initializer",
    "typeof_expression : TYPEOF OPEN_PARENS type CLOSE_PARENS",
    "sizeof_expression : SIZEOF OPEN_PARENS type CLOSE_PARENS",
    "checked_expression : CHECKED OPEN_PARENS expression CLOSE_PARENS",
    "unchecked_expression : UNCHECKED OPEN_PARENS expression CLOSE_PARENS",
    "pointer_member_access : primary_expression OP_PTR IDENTIFIER",
    "unary_expression : primary_expression",
    "unary_expression : BANG prefixed_unary_expression",
    "unary_expression : TILDE prefixed_unary_expression",
    "unary_expression : cast_expression",
    "cast_expression : OPEN_PARENS expression CLOSE_PARENS unary_expression",
    "cast_expression : OPEN_PARENS non_expression_type CLOSE_PARENS prefixed_unary_expression",
    "prefixed_unary_expression : unary_expression",
    "prefixed_unary_expression : PLUS prefixed_unary_expression",
    "prefixed_unary_expression : MINUS prefixed_unary_expression",
    "prefixed_unary_expression : OP_INC prefixed_unary_expression",
    "prefixed_unary_expression : OP_DEC prefixed_unary_expression",
    "prefixed_unary_expression : STAR prefixed_unary_expression",
    "prefixed_unary_expression : BITWISE_AND prefixed_unary_expression",
    "pre_increment_expression : OP_INC prefixed_unary_expression",
    "pre_decrement_expression : OP_DEC prefixed_unary_expression",
    "multiplicative_expression : prefixed_unary_expression",
    "multiplicative_expression : multiplicative_expression STAR prefixed_unary_expression",
    "multiplicative_expression : multiplicative_expression DIV prefixed_unary_expression",
    "multiplicative_expression : multiplicative_expression PERCENT prefixed_unary_expression",
    "additive_expression : multiplicative_expression",
    "additive_expression : additive_expression PLUS multiplicative_expression",
    "additive_expression : additive_expression MINUS multiplicative_expression",
    "shift_expression : additive_expression",
    "shift_expression : shift_expression OP_SHIFT_LEFT additive_expression",
    "shift_expression : shift_expression OP_SHIFT_RIGHT additive_expression",
    "relational_expression : shift_expression",
    "relational_expression : relational_expression OP_LT shift_expression",
    "relational_expression : relational_expression OP_GT shift_expression",
    "relational_expression : relational_expression OP_LE shift_expression",
    "relational_expression : relational_expression OP_GE shift_expression",
    "relational_expression : relational_expression IS type",
    "relational_expression : relational_expression AS type",
    "equality_expression : relational_expression",
    "equality_expression : equality_expression OP_EQ relational_expression",
    "equality_expression : equality_expression OP_NE relational_expression",
    "and_expression : equality_expression",
    "and_expression : and_expression BITWISE_AND equality_expression",
    "exclusive_or_expression : and_expression",
    "exclusive_or_expression : exclusive_or_expression CARRET and_expression",
    "inclusive_or_expression : exclusive_or_expression",
    "inclusive_or_expression : inclusive_or_expression BITWISE_OR exclusive_or_expression",
    "conditional_and_expression : inclusive_or_expression",
    "conditional_and_expression : conditional_and_expression OP_AND inclusive_or_expression",
    "conditional_or_expression : conditional_and_expression",
    "conditional_or_expression : conditional_or_expression OP_OR conditional_and_expression",
    "conditional_expression : conditional_or_expression",
    "conditional_expression : conditional_or_expression INTERR expression COLON expression",
    "assignment_expression : prefixed_unary_expression ASSIGN expression",
    "assignment_expression : prefixed_unary_expression OP_MULT_ASSIGN expression",
    "assignment_expression : prefixed_unary_expression OP_DIV_ASSIGN expression",
    "assignment_expression : prefixed_unary_expression OP_MOD_ASSIGN expression",
    "assignment_expression : prefixed_unary_expression OP_ADD_ASSIGN expression",
    "assignment_expression : prefixed_unary_expression OP_SUB_ASSIGN expression",
    "assignment_expression : prefixed_unary_expression OP_SHIFT_LEFT_ASSIGN expression",
    "assignment_expression : prefixed_unary_expression OP_SHIFT_RIGHT_ASSIGN expression",
    "assignment_expression : prefixed_unary_expression OP_AND_ASSIGN expression",
    "assignment_expression : prefixed_unary_expression OP_OR_ASSIGN expression",
    "assignment_expression : prefixed_unary_expression OP_XOR_ASSIGN expression",
    "expression : conditional_expression",
    "expression : assignment_expression",
    "constant_expression : expression",
    "boolean_expression : expression",
    "$$17 :",
    "class_declaration : opt_attributes opt_modifiers CLASS IDENTIFIER opt_class_base $$17 class_body opt_semicolon",
    "opt_modifiers :",
    "opt_modifiers : modifiers",
    "modifiers : modifier",
    "modifiers : modifiers modifier",
    "modifier : NEW",
    "modifier : PUBLIC",
    "modifier : PROTECTED",
    "modifier : INTERNAL",
    "modifier : PRIVATE",
    "modifier : ABSTRACT",
    "modifier : SEALED",
    "modifier : STATIC",
    "modifier : READONLY",
    "modifier : VIRTUAL",
    "modifier : OVERRIDE",
    "modifier : EXTERN",
    "modifier : VOLATILE",
    "modifier : UNSAFE",
    "opt_class_base :",
    "opt_class_base : class_base",
    "class_base : COLON type_list",
    "block : OPEN_BRACE opt_statement_list CLOSE_BRACE",
    "opt_statement_list :",
    "opt_statement_list : statement_list",
    "statement_list : statement",
    "statement_list : statement_list statement",
    "statement : declaration_statement",
    "statement : embedded_statement",
    "statement : labeled_statement",
    "embedded_statement : block",
    "embedded_statement : empty_statement",
    "embedded_statement : expression_statement",
    "embedded_statement : selection_statement",
    "embedded_statement : iteration_statement",
    "embedded_statement : jump_statement",
    "embedded_statement : try_statement",
    "embedded_statement : checked_statement",
    "embedded_statement : unchecked_statement",
    "embedded_statement : lock_statement",
    "embedded_statement : using_statement",
    "embedded_statement : unsafe_statement",
    "embedded_statement : fixed_statement",
    "empty_statement : SEMICOLON",
    "labeled_statement : IDENTIFIER COLON statement",
    "declaration_statement : local_variable_declaration SEMICOLON",
    "declaration_statement : local_constant_declaration SEMICOLON",
    "local_variable_type : primary_expression opt_rank_specifier",
    "local_variable_type : builtin_types opt_rank_specifier",
    "local_variable_pointer_type : primary_expression STAR",
    "local_variable_pointer_type : builtin_types STAR",
    "local_variable_pointer_type : VOID STAR",
    "local_variable_pointer_type : local_variable_pointer_type STAR",
    "local_variable_declaration : local_variable_type variable_declarators",
    "local_variable_declaration : local_variable_pointer_type opt_rank_specifier variable_declarators",
    "local_constant_declaration : CONST local_variable_type constant_declarator",
    "expression_statement : statement_expression SEMICOLON",
    "statement_expression : invocation_expression",
    "statement_expression : object_creation_expression",
    "statement_expression : assignment_expression",
    "statement_expression : post_increment_expression",
    "statement_expression : post_decrement_expression",
    "statement_expression : pre_increment_expression",
    "statement_expression : pre_decrement_expression",
    "statement_expression : error",
    "object_creation_expression : object_or_delegate_creation_expression",
    "selection_statement : if_statement",
    "selection_statement : switch_statement",
    "if_statement : if_statement_open if_statement_rest",
    "if_statement_open : IF OPEN_PARENS",
    "if_statement_rest : boolean_expression CLOSE_PARENS embedded_statement",
    "if_statement_rest : boolean_expression CLOSE_PARENS embedded_statement ELSE embedded_statement",
    "switch_statement : SWITCH OPEN_PARENS expression CLOSE_PARENS switch_block",
    "switch_block : OPEN_BRACE opt_switch_sections CLOSE_BRACE",
    "opt_switch_sections :",
    "opt_switch_sections : switch_sections",
    "switch_sections : switch_section",
    "switch_sections : switch_sections switch_section",
    "switch_section : switch_labels statement_list",
    "switch_labels : switch_label",
    "switch_labels : switch_labels switch_label",
    "switch_label : CASE constant_expression COLON",
    "switch_label : DEFAULT COLON",
    "switch_label : error",
    "iteration_statement : while_statement",
    "iteration_statement : do_statement",
    "iteration_statement : for_statement",
    "iteration_statement : foreach_statement",
    "while_statement : WHILE OPEN_PARENS boolean_expression CLOSE_PARENS embedded_statement",
    "do_statement : DO embedded_statement WHILE OPEN_PARENS boolean_expression CLOSE_PARENS SEMICOLON",
    "for_statement : FOR OPEN_PARENS opt_for_initializer SEMICOLON opt_for_condition SEMICOLON opt_for_iterator CLOSE_PARENS embedded_statement",
    "opt_for_initializer :",
    "opt_for_initializer : for_initializer",
    "for_initializer : local_variable_declaration",
    "for_initializer : statement_expression_list",
    "opt_for_condition :",
    "opt_for_condition : boolean_expression",
    "opt_for_iterator :",
    "opt_for_iterator : for_iterator",
    "for_iterator : statement_expression_list",
    "statement_expression_list : statement_expression",
    "statement_expression_list : statement_expression_list COMMA statement_expression",
    "foreach_statement : FOREACH OPEN_PARENS type IDENTIFIER IN expression CLOSE_PARENS embedded_statement",
    "jump_statement : break_statement",
    "jump_statement : continue_statement",
    "jump_statement : goto_statement",
    "jump_statement : return_statement",
    "jump_statement : throw_statement",
    "break_statement : BREAK SEMICOLON",
    "continue_statement : CONTINUE SEMICOLON",
    "goto_statement : GOTO IDENTIFIER SEMICOLON",
    "goto_statement : GOTO CASE constant_expression SEMICOLON",
    "goto_statement : GOTO DEFAULT SEMICOLON",
    "return_statement : RETURN opt_expression SEMICOLON",
    "throw_statement : THROW opt_expression SEMICOLON",
    "opt_expression :",
    "opt_expression : expression",
    "try_statement : TRY block catch_clauses",
    "try_statement : TRY block opt_catch_clauses FINALLY block",
    "try_statement : TRY block error",
    "opt_catch_clauses :",
    "opt_catch_clauses : catch_clauses",
    "catch_clauses : catch_clause",
    "catch_clauses : catch_clauses catch_clause",
    "opt_identifier :",
    "opt_identifier : IDENTIFIER",
    "catch_clause : CATCH opt_catch_args block",
    "opt_catch_args :",
    "opt_catch_args : catch_args",
    "catch_args : OPEN_PARENS type opt_identifier CLOSE_PARENS",
    "checked_statement : CHECKED block",
    "unchecked_statement : UNCHECKED block",
    "unsafe_statement : UNSAFE block",
    "fixed_statement : FIXED OPEN_PARENS type fixed_pointer_declarators CLOSE_PARENS embedded_statement",
    "fixed_pointer_declarators : fixed_pointer_declarator",
    "fixed_pointer_declarators : fixed_pointer_declarators COMMA fixed_pointer_declarator",
    "fixed_pointer_declarator : IDENTIFIER ASSIGN expression",
    "lock_statement : LOCK OPEN_PARENS expression CLOSE_PARENS embedded_statement",
    "using_statement : USING OPEN_PARENS resource_acquisition CLOSE_PARENS embedded_statement",
    "resource_acquisition : local_variable_declaration",
    "resource_acquisition : expression",
  };
  protected static  string [] yyName = {    
    "end-of-file",null,null,null,null,null,null,null,null,null,null,null,
    null,null,null,null,null,null,null,null,null,null,null,null,null,null,
    null,null,null,null,null,null,null,"'!'",null,null,null,"'%'","'&'",
    null,null,null,"'*'","'+'","','","'-'","'.'","'/'",null,null,null,
    null,null,null,null,null,null,null,null,null,"'<'","'='","'>'","'?'",
    null,null,null,null,null,null,null,null,null,null,null,null,null,null,
    null,null,null,null,null,null,null,null,null,null,null,null,null,null,
    null,null,"'^'",null,null,null,null,null,null,null,null,null,null,
    null,null,null,null,null,null,null,null,null,null,null,null,null,null,
    null,null,null,null,null,"'|'",null,"'~'",null,null,null,null,null,
    null,null,null,null,null,null,null,null,null,null,null,null,null,null,
    null,null,null,null,null,null,null,null,null,null,null,null,null,null,
    null,null,null,null,null,null,null,null,null,null,null,null,null,null,
    null,null,null,null,null,null,null,null,null,null,null,null,null,null,
    null,null,null,null,null,null,null,null,null,null,null,null,null,null,
    null,null,null,null,null,null,null,null,null,null,null,null,null,null,
    null,null,null,null,null,null,null,null,null,null,null,null,null,null,
    null,null,null,null,null,null,null,null,null,null,null,null,null,null,
    null,null,null,null,null,null,null,null,null,null,null,null,null,
    "EOF","NONE","ERROR","ABSTRACT","AS","ADD","ASSEMBLY","BASE","BOOL",
    "BREAK","BYTE","CASE","CATCH","CHAR","CHECKED","CLASS","CONST",
    "CONTINUE","DECIMAL","DEFAULT","DELEGATE","DO","DOUBLE","ELSE","ENUM",
    "EVENT","EXPLICIT","EXTERN","FALSE","FINALLY","FIXED","FLOAT","FOR",
    "FOREACH","GOTO","IF","IMPLICIT","IN","INT","INTERFACE","INTERNAL",
    "IS","LOCK","LONG","NAMESPACE","NEW","NULL","OBJECT","OPERATOR","OUT",
    "OVERRIDE","PARAMS","PRIVATE","PROTECTED","PUBLIC","READONLY","REF",
    "RETURN","REMOVE","SBYTE","SEALED","SHORT","SIZEOF","STACKALLOC",
    "STATIC","STRING","STRUCT","SWITCH","THIS","THROW","TRUE","TRY",
    "TYPEOF","UINT","ULONG","UNCHECKED","UNSAFE","USHORT","USING",
    "VIRTUAL","VOID","VOLATILE","WHILE","OPEN_BRACE","CLOSE_BRACE",
    "OPEN_PARENS","CLOSE_PARENS","OPEN_BRACKET","CLOSE_BRACKET",
    "SEMICOLON","COLON","GET","\"get\"","SET","\"set\"","DOT","COMMA",
    "TILDE","PLUS","MINUS","BANG","ASSIGN","OP_LT","OP_GT","BITWISE_AND",
    "BITWISE_OR","STAR","PERCENT","DIV","CARRET","INTERR","OP_INC",
    "\"++\"","OP_DEC","\"--\"","OP_SHIFT_LEFT","\"<<\"","OP_SHIFT_RIGHT",
    "\">>\"","OP_LE","\"<=\"","OP_GE","\">=\"","OP_EQ","\"==\"","OP_NE",
    "\"!=\"","OP_AND","\"&&\"","OP_OR","\"||\"","OP_MULT_ASSIGN","\"*=\"",
    "OP_DIV_ASSIGN","\"/=\"","OP_MOD_ASSIGN","\"%=\"","OP_ADD_ASSIGN",
    "\"+=\"","OP_SUB_ASSIGN","\"-=\"","OP_SHIFT_LEFT_ASSIGN","\"<<=\"",
    "OP_SHIFT_RIGHT_ASSIGN","\">>=\"","OP_AND_ASSIGN","\"&=\"",
    "OP_XOR_ASSIGN","\"^=\"","OP_OR_ASSIGN","\"|=\"","OP_PTR","\"->\"",
    "LITERAL_INTEGER","\"int literal\"","LITERAL_FLOAT",
    "\"float literal\"","LITERAL_DOUBLE","\"double literal\"",
    "LITERAL_DECIMAL","\"decimal literal\"","LITERAL_CHARACTER",
    "\"character literal\"","LITERAL_STRING","\"string literal\"",
    "IDENTIFIER","LOWPREC","UMINUS","HIGHPREC",
  };

  /** index-checked interface to yyName[].
      @param token single character or %token value.
      @return token name or [illegal] or [unknown].
    */
  public static string yyname (int token) {
    if ((token < 0) || (token > yyName.Length)) return "[illegal]";
    string name;
    if ((name = yyName[token]) != null) return name;
    return "[unknown]";
  }

  /** computes list of expected tokens on error by tracing the tables.
      @param state for which to compute the list.
      @return list of token names.
    */
  protected string[] yyExpecting (int state) {
    int token, n, len = 0;
    bool[] ok = new bool[yyName.Length];

    if ((n = yySindex[state]) != 0)
      for (token = n < 0 ? -n : 0;
           (token < yyName.Length) && (n+token < yyTable.Length); ++ token)
        if (yyCheck[n+token] == token && !ok[token] && yyName[token] != null) {
          ++ len;
          ok[token] = true;
        }
    if ((n = yyRindex[state]) != 0)
      for (token = n < 0 ? -n : 0;
           (token < yyName.Length) && (n+token < yyTable.Length); ++ token)
        if (yyCheck[n+token] == token && !ok[token] && yyName[token] != null) {
          ++ len;
          ok[token] = true;
        }

    string [] result = new string[len];
    for (n = token = 0; n < len;  ++ token)
      if (ok[token]) result[n++] = yyName[token];
    return result;
  }

  /** the generated parser, with debugging messages.
      Maintains a state and a value stack, currently with fixed maximum size.
      @param yyLex scanner.
      @param yydebug debug message writer implementing yyDebug, or null.
      @return result of the last reduction, if any.
      @throws yyException on irrecoverable parse error.
    */
  public Object yyparse (yyParser.yyInput yyLex, Object yyd)
				 {
    this.debug = (yydebug.yyDebug)yyd;
    return yyparse(yyLex);
  }

  /** initial size and increment of the state/value stack [default 256].
      This is not final so that it can be overwritten outside of invocations
      of yyparse().
    */
  protected int yyMax;

  /** executed at the beginning of a reduce action.
      Used as $$ = yyDefault($1), prior to the user-specified action, if any.
      Can be overwritten to provide deep copy, etc.
      @param first value for $1, or null.
      @return first.
    */
  protected Object yyDefault (Object first) {
    return first;
  }

  /** the generated parser.
      Maintains a state and a value stack, currently with fixed maximum size.
      @param yyLex scanner.
      @return result of the last reduction, if any.
      @throws yyException on irrecoverable parse error.
    */
  public Object yyparse (yyParser.yyInput yyLex)
				{
    if (yyMax <= 0) yyMax = 256;			// initial size
    int yyState = 0;                                   // state stack ptr
    int [] yyStates = new int[yyMax];	                // state stack 
    Object yyVal = null;                               // value stack ptr
    Object [] yyVals = new Object[yyMax];	        // value stack
    int yyToken = -1;					// current input
    int yyErrorFlag = 0;				// #tks to shift

    int yyTop = 0;
    goto skip;
    yyLoop:
    yyTop++;
    skip:
    for (;; ++ yyTop) {
      if (yyTop >= yyStates.Length) {			// dynamically increase
        int[] i = new int[yyStates.Length+yyMax];
        yyStates.CopyTo (i, 0);
        yyStates = i;
        Object[] o = new Object[yyVals.Length+yyMax];
        yyVals.CopyTo (o, 0);
        yyVals = o;
      }
      yyStates[yyTop] = yyState;
      yyVals[yyTop] = yyVal;
      if (debug != null) debug.push(yyState, yyVal);

      yyDiscarded: for (;;) {	// discarding a token does not change stack
        int yyN;
        if ((yyN = yyDefRed[yyState]) == 0) {	// else [default] reduce (yyN)
          if (yyToken < 0) {
            yyToken = yyLex.advance() ? yyLex.token() : 0;
            if (debug != null)
              debug.lex(yyState, yyToken, yyname(yyToken), yyLex.value());
          }
          if ((yyN = yySindex[yyState]) != 0 && ((yyN += yyToken) >= 0)
              && (yyN < yyTable.Length) && (yyCheck[yyN] == yyToken)) {
            if (debug != null)
              debug.shift(yyState, yyTable[yyN], yyErrorFlag-1);
            yyState = yyTable[yyN];		// shift to yyN
            yyVal = yyLex.value();
            yyToken = -1;
            if (yyErrorFlag > 0) -- yyErrorFlag;
            goto yyLoop;
          }
          if ((yyN = yyRindex[yyState]) != 0 && (yyN += yyToken) >= 0
              && yyN < yyTable.Length && yyCheck[yyN] == yyToken)
            yyN = yyTable[yyN];			// reduce (yyN)
          else
            switch (yyErrorFlag) {
  
            case 0:
              yyerror("syntax error", yyExpecting(yyState));
              if (debug != null) debug.error("syntax error");
              goto case 1;
            case 1: case 2:
              yyErrorFlag = 3;
              do {
                if ((yyN = yySindex[yyStates[yyTop]]) != 0
                    && (yyN += Token.yyErrorCode) >= 0 && yyN < yyTable.Length
                    && yyCheck[yyN] == Token.yyErrorCode) {
                  if (debug != null)
                    debug.shift(yyStates[yyTop], yyTable[yyN], 3);
                  yyState = yyTable[yyN];
                  yyVal = yyLex.value();
                  goto yyLoop;
                }
                if (debug != null) debug.pop(yyStates[yyTop]);
              } while (-- yyTop >= 0);
              if (debug != null) debug.reject();
              throw new yyParser.yyException("irrecoverable syntax error");
  
            case 3:
              if (yyToken == 0) {
                if (debug != null) debug.reject();
                throw new yyParser.yyException("irrecoverable syntax error at end-of-file");
              }
              if (debug != null)
                debug.discard(yyState, yyToken, yyname(yyToken),
  							yyLex.value());
              yyToken = -1;
              goto yyDiscarded;		// leave stack alone
            }
        }
        int yyV = yyTop + 1-yyLen[yyN];
        if (debug != null)
          debug.reduce(yyState, yyStates[yyV-1], yyN, yyRule[yyN], yyLen[yyN]);
        yyVal = yyDefault(yyV > yyTop ? null : yyVals[yyV]);
        switch (yyN) {
case 17:
#line 271 "cs-parser.jay"
  {
		  Namespace = Namespace + (Namespace.Length > 0 ? "." : "") + ((TokenValue)yyVals[0+yyTop]).val.s;
		  Namespaces.Push (((TokenValue)yyVals[0+yyTop]).val.s.Length);
          }
  break;
case 18:
#line 276 "cs-parser.jay"
  {
		  int idx = (int) Namespaces.Pop ();
		  Namespace = Namespace.Remove (Namespace.Length - idx, idx);
	  }
  break;
case 19:
#line 283 "cs-parser.jay"
  { yyVal = new TokenValue (); }
  break;
case 24:
#line 294 "cs-parser.jay"
  {
	        ((TokenValue)yyVal).val = ((TokenValue)yyVals[-2+yyTop]).val.s + "." + ((TokenValue)yyVals[0+yyTop]).val.s; }
  break;
case 26:
#line 308 "cs-parser.jay"
  {
	  }
  break;
case 40:
#line 351 "cs-parser.jay"
  { yyVal = new TokenValue (); }
  break;
case 43:
#line 358 "cs-parser.jay"
  {
		  ((TokenValue)yyVals[-1+yyTop]).Update (((TokenValue)yyVals[0+yyTop]));
	  }
  break;
case 44:
#line 365 "cs-parser.jay"
  {
		  ((TokenValue)yyVals[-4+yyTop]).Update (((TokenValue)yyVals[0+yyTop]));
	  }
  break;
case 45:
#line 369 "cs-parser.jay"
  {
		  ((TokenValue)yyVals[-3+yyTop]).Update (((TokenValue)yyVals[0+yyTop]));
	  }
  break;
case 46:
#line 376 "cs-parser.jay"
  {
		yyVal = yyVals[-1+yyTop];
	  }
  break;
case 47:
#line 383 "cs-parser.jay"
  {
		yyVal = ((TokenValue)yyVals[0+yyTop]);
	  }
  break;
case 48:
#line 386 "cs-parser.jay"
  { yyVal = "event"; }
  break;
case 49:
#line 387 "cs-parser.jay"
  { yyVal = "return"; }
  break;
case 53:
#line 401 "cs-parser.jay"
  { /* reserved attribute name or identifier: 17.4 */ }
  break;
case 54:
#line 405 "cs-parser.jay"
  { yyVal = null; }
  break;
case 59:
#line 418 "cs-parser.jay"
  { yyVal = null; }
  break;
case 66:
#line 439 "cs-parser.jay"
  {
		  ((TokenValue)yyVals[-2+yyTop]).Update (((TokenValue)yyVals[0+yyTop]));
	  }
  break;
case 101:
#line 533 "cs-parser.jay"
  {
		  ((TokenValue)yyVals[-4+yyTop]).Update (((TokenValue)yyVals[-3+yyTop]));
		  ((TokenValue)yyVals[-4+yyTop]).Update (((TokenValue)yyVals[-2+yyTop]));
		  ((TokenValue)yyVals[-4+yyTop]).Update (((TokenValue)yyVals[0+yyTop]));
		  /*Console.WriteLine ("fi {0}", $2);*/
		  foreach (string name in ((TokenValue)yyVals[-1+yyTop]).val.s.Split (','))
		          db.AddMember (Namespace, type, name, new Part (fileID, ((TokenValue)yyVals[-4+yyTop])));
	  }
  break;
case 103:
#line 546 "cs-parser.jay"
  {
		  ((TokenValue)yyVals[-2+yyTop]).Update (((TokenValue)yyVals[0+yyTop]), ((TokenValue)yyVals[-2+yyTop]).val.s + "," + ((TokenValue)yyVals[0+yyTop]).val.s);
	  }
  break;
case 104:
#line 553 "cs-parser.jay"
  {
		  ((TokenValue)yyVals[-2+yyTop]).Update (((TokenValue)yyVals[0+yyTop]));
	  }
  break;
case 109:
#line 568 "cs-parser.jay"
  {
		  ((TokenValue)yyVals[-1+yyTop]).Update (((TokenValue)yyVals[0+yyTop]));
		  db.AddMember (Namespace, type, ((TokenValue)yyVals[-1+yyTop]), new Part (fileID, ((TokenValue)yyVals[-1+yyTop])));
	  }
  break;
case 112:
#line 585 "cs-parser.jay"
  {
		((TokenValue)yyVals[-6+yyTop]).Update (((TokenValue)yyVals[-5+yyTop]));
		((TokenValue)yyVals[-6+yyTop]).Update (((TokenValue)yyVals[-4+yyTop]));
		((TokenValue)yyVals[-6+yyTop]).Update (((TokenValue)yyVals[0+yyTop]), ((TokenValue)yyVals[-3+yyTop]).val.s + " (" + ((TokenValue)yyVals[-1+yyTop]).val.s + "), " + ((TokenValue)yyVals[-4+yyTop]).val.s);
	}
  break;
case 113:
#line 595 "cs-parser.jay"
  {
		((TokenValue)yyVals[-6+yyTop]).Update (((TokenValue)yyVals[-5+yyTop]));
		((TokenValue)yyVals[-6+yyTop]).Update (((TokenValue)yyVals[-4+yyTop]));
		((TokenValue)yyVals[-6+yyTop]).Update (((TokenValue)yyVals[0+yyTop]), ((TokenValue)yyVals[-3+yyTop]).val.s + " (" + ((TokenValue)yyVals[-1+yyTop]).val.s + "), void");
	}
  break;
case 116:
#line 608 "cs-parser.jay"
  { yyVal = new TokenValue (); }
  break;
case 119:
#line 614 "cs-parser.jay"
  { ((TokenValue)yyVal).val = ((TokenValue)yyVals[-2+yyTop]).val.s + ", " + ((TokenValue)yyVals[0+yyTop]).val.s; }
  break;
case 122:
#line 620 "cs-parser.jay"
  { ((TokenValue)yyVal).val = ((TokenValue)yyVals[-2+yyTop]).val.s + ", " + ((TokenValue)yyVals[0+yyTop]).val.s; }
  break;
case 123:
#line 628 "cs-parser.jay"
  {
		((TokenValue)yyVal).val = ((TokenValue)yyVals[-1+yyTop]).val.s + " " + ((TokenValue)yyVals[0+yyTop]).val.s;
	}
  break;
case 124:
#line 634 "cs-parser.jay"
  { yyVal = new TokenValue (); }
  break;
case 130:
#line 656 "cs-parser.jay"
  {
		  Rows.Push (lexer.Location.Row);
	  }
  break;
case 131:
#line 660 "cs-parser.jay"
  {
		lexer.PropertyParsing = true;
	  }
  break;
case 132:
#line 664 "cs-parser.jay"
  {
		lexer.PropertyParsing = false;
	  }
  break;
case 133:
#line 668 "cs-parser.jay"
  {
		  db.AddMember (Namespace, type, ((TokenValue)yyVals[-7+yyTop]).val.s + " " + ((TokenValue)yyVals[-6+yyTop]).val.s, new Part (fileID, (int) Rows.Pop (), lexer.Location.Row));
	  }
  break;
case 136:
#line 679 "cs-parser.jay"
  { yyVal = null; }
  break;
case 138:
#line 684 "cs-parser.jay"
  { yyVal = null; }
  break;
case 140:
#line 690 "cs-parser.jay"
  {
		lexer.PropertyParsing = false;
	  }
  break;
case 141:
#line 694 "cs-parser.jay"
  {
		lexer.PropertyParsing = true;
	  }
  break;
case 142:
#line 701 "cs-parser.jay"
  {
		lexer.PropertyParsing = false;
	  }
  break;
case 143:
#line 705 "cs-parser.jay"
  {
		lexer.PropertyParsing = true;
	  }
  break;
case 145:
#line 712 "cs-parser.jay"
  { yyVal = null; }
  break;
case 147:
#line 724 "cs-parser.jay"
  { yyVal = null; }
  break;
case 149:
#line 729 "cs-parser.jay"
  { yyVal = yyVals[0+yyTop]; }
  break;
case 161:
#line 761 "cs-parser.jay"
  { yyVal = false; }
  break;
case 162:
#line 762 "cs-parser.jay"
  { yyVal = true; }
  break;
case 165:
#line 779 "cs-parser.jay"
  { lexer.PropertyParsing = true; }
  break;
case 166:
#line 781 "cs-parser.jay"
  { lexer.PropertyParsing = false; }
  break;
case 168:
#line 786 "cs-parser.jay"
  { yyVal = 1; }
  break;
case 169:
#line 787 "cs-parser.jay"
  { yyVal = 2; }
  break;
case 170:
#line 789 "cs-parser.jay"
  { yyVal = 3; }
  break;
case 171:
#line 791 "cs-parser.jay"
  { yyVal = 3; }
  break;
case 173:
#line 802 "cs-parser.jay"
  { lexer.PropertyParsing = true; }
  break;
case 174:
#line 804 "cs-parser.jay"
  { lexer.PropertyParsing = false; }
  break;
case 178:
#line 814 "cs-parser.jay"
  { yyVal = null; }
  break;
case 208:
#line 867 "cs-parser.jay"
  {
		  ((TokenValue)yyVals[-3+yyTop]).Update (((TokenValue)yyVals[-2+yyTop]));
		  ((TokenValue)yyVals[-3+yyTop]).Update (((TokenValue)yyVals[-1+yyTop]));
		  ((TokenValue)yyVals[-3+yyTop]).Update (((TokenValue)yyVals[0+yyTop]));
		  db.AddMember (Namespace, type, ((TokenValue)yyVals[-1+yyTop]), new Part (fileID, ((TokenValue)yyVals[-3+yyTop])));
	  }
  break;
case 209:
#line 879 "cs-parser.jay"
  {
		  ((TokenValue)yyVals[-4+yyTop]).Update (((TokenValue)yyVals[-1+yyTop]));
		  ((TokenValue)yyVals[-4+yyTop]).Update (((TokenValue)yyVals[0+yyTop]), (modStatic ? ".cctor" : ".ctor") + " (" + ((TokenValue)yyVals[-2+yyTop]).val.s + ")");
	  }
  break;
case 212:
#line 891 "cs-parser.jay"
  { yyVal = new TokenValue (); }
  break;
case 214:
#line 897 "cs-parser.jay"
  {
		  ((TokenValue)yyVals[-4+yyTop]).Update (((TokenValue)yyVals[0+yyTop]));
	  }
  break;
case 215:
#line 901 "cs-parser.jay"
  {
		  ((TokenValue)yyVals[-4+yyTop]).Update (((TokenValue)yyVals[0+yyTop]));
	  }
  break;
case 218:
#line 918 "cs-parser.jay"
  {
		lexer.EventParsing = true;
	  }
  break;
case 219:
#line 922 "cs-parser.jay"
  {
		lexer.EventParsing = false;  
	  }
  break;
case 223:
#line 935 "cs-parser.jay"
  {
		lexer.EventParsing = false;
	  }
  break;
case 224:
#line 939 "cs-parser.jay"
  {
		lexer.EventParsing = true;
	  }
  break;
case 225:
#line 946 "cs-parser.jay"
  {
		lexer.EventParsing = false;
	  }
  break;
case 226:
#line 950 "cs-parser.jay"
  {
		lexer.EventParsing = true;
	  }
  break;
case 227:
#line 958 "cs-parser.jay"
  {
		lexer.PropertyParsing = true;
	  }
  break;
case 228:
#line 962 "cs-parser.jay"
  {
		  lexer.PropertyParsing = false;
	  }
  break;
case 278:
#line 1102 "cs-parser.jay"
  { ((TokenValue)yyVal).val = ((TokenValue)yyVals[-1+yyTop]).val.s + ((TokenValue)yyVals[0+yyTop]).val.s; }
  break;
case 312:
#line 1170 "cs-parser.jay"
  { yyVal = null; }
  break;
case 319:
#line 1186 "cs-parser.jay"
  { note ("section 5.4"); yyVal = yyVals[0+yyTop]; }
  break;
case 335:
#line 1234 "cs-parser.jay"
  { yyVal = new TokenValue (); }
  break;
case 337:
#line 1240 "cs-parser.jay"
  {
		  ((TokenValue)yyVal).val = ((TokenValue)yyVals[0+yyTop]).val.s + ((TokenValue)yyVals[-1+yyTop]).val.s;
	  }
  break;
case 338:
#line 1247 "cs-parser.jay"
  {
		  ((TokenValue)yyVal).val.s = "[" + ((TokenValue)yyVals[-1+yyTop]).val.s + "]";
	  }
  break;
case 339:
#line 1253 "cs-parser.jay"
  { yyVal = new TokenValue (); }
  break;
case 341:
#line 1258 "cs-parser.jay"
  { ((TokenValue)yyVal).val = ","; }
  break;
case 342:
#line 1260 "cs-parser.jay"
  {
		((TokenValue)yyVal).val = ((TokenValue)yyVals[-1+yyTop]).val.s + ",";
	  }
  break;
case 416:
#line 1433 "cs-parser.jay"
  {
		  Types.Push (type);
		  type += (type.Length > 0 ? "." : "") + ((TokenValue)yyVals[-1+yyTop]).val.s;
	  }
  break;
case 417:
#line 1439 "cs-parser.jay"
  {
		  ((TokenValue)yyVals[-7+yyTop]).Update (((TokenValue)yyVals[-6+yyTop]));
		  ((TokenValue)yyVals[-7+yyTop]).Update (((TokenValue)yyVals[-5+yyTop]));
		  ((TokenValue)yyVals[-7+yyTop]).Update (((TokenValue)yyVals[-1+yyTop]));
		  ((TokenValue)yyVals[-7+yyTop]).Update (((TokenValue)yyVals[0+yyTop]));
		  db.AddType (Namespace, type, new Part (fileID, ((TokenValue)yyVals[-7+yyTop])));
		  type = (string) Types.Pop ();
	  }
  break;
case 418:
#line 1450 "cs-parser.jay"
  { yyVal = new TokenValue (); modStatic = false; }
  break;
case 419:
#line 1452 "cs-parser.jay"
  {
		  modStatic = ((TokenValue)yyVals[0+yyTop]).val.s.IndexOf ("static") >= 0;
	  }
  break;
case 421:
#line 1459 "cs-parser.jay"
  { ((TokenValue)yyVal).val = ((TokenValue)yyVals[-1+yyTop]).val.s + " " + ((TokenValue)yyVals[0+yyTop]).val.s; }
  break;
case 436:
#line 1480 "cs-parser.jay"
  { yyVal = new TokenValue (); }
  break;
case 438:
#line 1485 "cs-parser.jay"
  { yyVal = yyVals[0+yyTop]; }
  break;
case 439:
#line 1504 "cs-parser.jay"
  {
		  ((TokenValue)yyVals[-2+yyTop]).Update (((TokenValue)yyVals[0+yyTop]));
	  }
  break;
case 473:
#line 1584 "cs-parser.jay"
  {
		yyVal = yyVals[-1+yyTop];
	  }
  break;
case 482:
#line 1606 "cs-parser.jay"
  { note ("complain if this is a delegate maybe?"); }
  break;
case 485:
#line 1616 "cs-parser.jay"
  {
		yyVal = yyVals[0+yyTop];
	  }
  break;
case 490:
#line 1642 "cs-parser.jay"
  {
		yyVal = yyVals[-1+yyTop];
	  }
  break;
case 537:
#line 1776 "cs-parser.jay"
  { yyVal = null; }
  break;
case 541:
#line 1786 "cs-parser.jay"
  { yyVal = null; }
  break;
case 544:
#line 1796 "cs-parser.jay"
  { yyVal = null; }
  break;
#line 1319 "-"
        }
        yyTop -= yyLen[yyN];
        yyState = yyStates[yyTop];
        int yyM = yyLhs[yyN];
        if (yyState == 0 && yyM == 0) {
          if (debug != null) debug.shift(0, yyFinal);
          yyState = yyFinal;
          if (yyToken < 0) {
            yyToken = yyLex.advance() ? yyLex.token() : 0;
            if (debug != null)
               debug.lex(yyState, yyToken,yyname(yyToken), yyLex.value());
          }
          if (yyToken == 0) {
            if (debug != null) debug.accept(yyVal);
            return yyVal;
          }
          goto yyLoop;
        }
        if (((yyN = yyGindex[yyM]) != 0) && ((yyN += yyState) >= 0)
            && (yyN < yyTable.Length) && (yyCheck[yyN] == yyState))
          yyState = yyTable[yyN];
        else
          yyState = yyDgoto[yyM];
        if (debug != null) debug.shift(yyStates[yyTop], yyState);
	 goto yyLoop;
      }
    }
  }

   static  short [] yyLhs  = {              -1,
    0,    0,    0,    0,   34,   34,   33,   33,   35,   35,
   38,   38,   36,   36,   39,   40,   45,   43,   24,   24,
   46,   46,   25,   25,   42,   44,   47,   47,   48,   48,
   49,   49,   37,   37,   50,   50,   50,   50,   50,   18,
   18,    2,    2,    1,    1,   56,   58,   58,   58,   57,
   57,   59,   60,   61,   61,   63,   63,   63,   64,   64,
   65,   65,   66,   66,   68,    4,   69,   69,   70,   70,
   71,   71,   71,   71,   71,   71,   71,   71,   71,   71,
   52,   81,   82,   82,   83,   83,   84,   84,   84,   84,
   84,   84,   84,   84,   84,   84,   72,   85,   85,   86,
   73,   31,   31,   30,   30,   32,   32,   32,   74,   89,
   89,   14,   14,   13,   13,   22,   22,   11,   11,   11,
   10,   10,    9,   91,   91,   92,   92,   26,   15,   93,
   95,   96,   75,   94,   94,  100,  100,   98,   98,  102,
   97,  103,   99,  101,  101,   53,  104,  104,  106,  107,
  107,  105,  109,  109,  110,  110,  111,  111,  111,  111,
  116,  116,  112,  112,  118,  119,  113,  117,  117,  117,
  117,  114,  120,  121,  115,   78,  123,  123,  122,  122,
  122,  124,  124,  124,  124,  124,  124,  124,  124,  124,
  124,  124,  124,  124,  124,  124,  124,  124,  124,  124,
  124,  124,  124,  125,  125,  125,  125,    6,    7,    5,
    5,   19,   19,  126,  126,   79,   76,  129,  130,   76,
  128,  128,  133,  131,  134,  132,  136,  137,   77,  135,
  135,   54,  138,  138,  139,  140,  140,  141,  141,  142,
  142,   55,   55,   62,   41,   29,   29,   29,   29,  144,
  144,  145,  145,  145,  145,  145,  145,  147,  147,    3,
    3,    3,    3,    3,    3,    3,   12,   12,   12,   12,
   12,   12,   12,   12,   12,   12,  108,  143,  148,  148,
  148,  148,  148,  148,  148,  148,  148,  148,  148,  148,
  148,  148,  148,  148,  149,  149,  149,  149,  149,  149,
  166,  166,  166,  165,  164,  164,  150,  151,  151,  167,
  152,  127,  127,  168,  168,  169,  169,  169,  170,  153,
  153,  171,  171,  154,  155,  155,  156,  157,  158,  158,
  172,  173,  173,  173,   23,   23,   28,   27,   20,   20,
    8,    8,  174,  174,   88,   88,  175,  175,  159,  160,
  161,  162,  163,  176,  176,  176,  176,  178,  178,  177,
  177,  177,  177,  177,  177,  177,  179,  180,  146,  146,
  146,  146,  181,  181,  181,  182,  182,  182,  183,  183,
  183,  183,  183,  183,  183,  184,  184,  184,  185,  185,
  186,  186,  187,  187,  188,  188,  189,  189,  190,  190,
  191,  191,  191,  191,  191,  191,  191,  191,  191,  191,
  191,   67,   67,   87,  192,  193,   51,   21,   21,   17,
   17,   16,   16,   16,   16,   16,   16,   16,   16,   16,
   16,   16,   16,   16,   16,   80,   80,  194,   90,  195,
  195,  196,  196,  197,  197,  197,  199,  199,  199,  199,
  199,  199,  199,  199,  199,  199,  199,  199,  199,  201,
  200,  198,  198,  215,  215,  216,  216,  216,  216,  213,
  213,  214,  202,  217,  217,  217,  217,  217,  217,  217,
  217,  218,  203,  203,  219,  221,  222,  222,  220,  223,
  224,  224,  225,  225,  226,  227,  227,  228,  228,  228,
  204,  204,  204,  204,  229,  230,  231,  233,  233,  236,
  236,  234,  234,  235,  235,  238,  237,  237,  232,  205,
  205,  205,  205,  205,  239,  240,  241,  241,  241,  242,
  243,  244,  244,  206,  206,  206,  246,  246,  245,  245,
  248,  248,  247,  249,  249,  250,  207,  208,  211,  212,
  251,  251,  252,  209,  210,  253,  253,
  };
   static  short [] yyLen = {           2,
    2,    3,    2,    1,    0,    1,    1,    2,    1,    1,
    1,    2,    1,    1,    5,    3,    0,    6,    0,    1,
    0,    1,    1,    3,    1,    4,    0,    1,    0,    1,
    1,    2,    1,    1,    1,    1,    1,    1,    1,    0,
    1,    1,    2,    5,    4,    2,    1,    1,    1,    1,
    3,    2,    1,    0,    3,    1,    3,    1,    0,    1,
    1,    3,    1,    3,    3,    3,    0,    1,    1,    2,
    1,    1,    1,    1,    1,    1,    1,    1,    1,    1,
    7,    3,    0,    1,    1,    2,    1,    1,    1,    1,
    1,    1,    1,    1,    1,    1,    6,    1,    3,    3,
    5,    1,    3,    3,    1,    1,    1,    5,    2,    0,
    1,    7,    7,    1,    1,    0,    1,    1,    3,    1,
    1,    3,    4,    0,    1,    1,    1,    4,    1,    0,
    0,    0,   10,    2,    2,    0,    1,    0,    1,    0,
    4,    0,    4,    1,    1,    7,    0,    1,    2,    1,
    3,    3,    0,    1,    1,    2,    1,    1,    1,    1,
    0,    1,    8,    8,    0,    0,    9,    3,    3,    6,
    6,    6,    0,    0,   12,    4,    1,    1,    7,   10,
    1,    1,    1,    1,    1,    1,    1,    1,    1,    1,
    1,    1,    1,    1,    1,    1,    1,    1,    1,    1,
    1,    1,    1,    7,    7,    2,    2,    4,    5,    1,
    1,    0,    1,    5,    5,    6,    6,    0,    0,   10,
    2,    2,    0,    4,    0,    4,    0,    0,    8,    5,
    7,    7,    0,    2,    3,    0,    2,    1,    3,    2,
    4,    9,    9,    1,    1,    1,    1,    1,    1,    2,
    2,    1,    2,    2,    2,    2,    2,    1,    3,    1,
    1,    1,    1,    1,    1,    1,    1,    1,    1,    1,
    1,    1,    1,    1,    1,    1,    1,    2,    1,    1,
    1,    1,    1,    1,    1,    1,    1,    1,    1,    1,
    1,    1,    1,    1,    1,    1,    1,    1,    1,    1,
    1,    1,    1,    1,    1,    1,    3,    3,    3,    1,
    4,    0,    1,    1,    3,    1,    2,    2,    1,    4,
    2,    1,    3,    1,    3,    4,    2,    2,    1,    1,
    5,    7,    4,    3,    0,    1,    2,    3,    0,    1,
    1,    2,    0,    1,    2,    4,    1,    3,    4,    4,
    4,    4,    3,    1,    2,    2,    1,    4,    4,    1,
    2,    2,    2,    2,    2,    2,    2,    2,    1,    3,
    3,    3,    1,    3,    3,    1,    3,    3,    1,    3,
    3,    3,    3,    3,    3,    1,    3,    3,    1,    3,
    1,    3,    1,    3,    1,    3,    1,    3,    1,    5,
    3,    3,    3,    3,    3,    3,    3,    3,    3,    3,
    3,    1,    1,    1,    1,    0,    8,    0,    1,    1,
    2,    1,    1,    1,    1,    1,    1,    1,    1,    1,
    1,    1,    1,    1,    1,    0,    1,    2,    3,    0,
    1,    1,    2,    1,    1,    1,    1,    1,    1,    1,
    1,    1,    1,    1,    1,    1,    1,    1,    1,    1,
    3,    2,    2,    2,    2,    2,    2,    2,    2,    2,
    3,    3,    2,    1,    1,    1,    1,    1,    1,    1,
    1,    1,    1,    1,    2,    2,    3,    5,    5,    3,
    0,    1,    1,    2,    2,    1,    2,    3,    2,    1,
    1,    1,    1,    1,    5,    7,    9,    0,    1,    1,
    1,    0,    1,    0,    1,    1,    1,    3,    8,    1,
    1,    1,    1,    1,    2,    2,    3,    4,    3,    3,
    3,    0,    1,    3,    5,    3,    0,    1,    1,    2,
    0,    1,    3,    0,    1,    4,    2,    2,    2,    6,
    1,    3,    3,    5,    5,    1,    1,
  };
   static  short [] yyDefRed = {            0,
    6,    0,    0,    0,   42,    0,    0,    0,    4,    7,
    9,   10,   13,   14,   34,   33,   35,   36,   37,   38,
   39,    0,    0,   25,    0,   48,   49,    0,  244,    0,
    0,    0,   50,    0,   53,   43,    3,  427,  433,  425,
    0,  422,  432,  426,  424,  423,  430,  428,  429,  435,
  431,  434,  420,    0,    0,    0,    1,    8,    0,    0,
   16,   23,    0,    0,    0,   46,    0,   52,    0,  421,
    0,    0,    0,    0,    0,    2,    0,   24,    0,   51,
   45,    0,  262,  268,  275,    0,  263,  265,  306,  264,
  271,  273,    0,  300,  260,  267,  269,    0,  261,  324,
  305,    0,  272,  274,    0,  270,  276,    0,    0,    0,
    0,    0,    0,    0,    0,    0,  304,  301,  302,  303,
  298,  299,    0,  310,  266,    0,    0,   56,    0,    0,
   61,   63,    0,    0,  279,  281,  282,  283,  284,  285,
  286,  287,  288,  289,  290,  291,  292,  293,  294,  295,
  296,  297,    0,  329,  330,  360,    0,  357,    0,    0,
    0,    0,    0,    0,    0,    0,    0,  412,  413,    0,
    0,    0,  247,    0,  246,  248,  249,    0,    0,    0,
   15,   44,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,  356,  361,  362,  355,  366,  365,
  363,  364,    0,   55,    0,    0,    0,    0,    0,    0,
    0,    0,  327,  328,    0,    0,  321,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
  416,  437,  251,    0,    0,  250,    0,  278,    0,    0,
    0,    0,  148,    0,  322,    0,  325,    0,  334,    0,
    0,    0,    0,    0,    0,    0,  256,  255,    0,  254,
  253,    0,   65,    0,   62,    0,   64,  370,  372,  371,
    0,    0,  316,    0,    0,  314,  341,    0,    0,    0,
  308,  353,  337,  336,  309,  401,  402,  403,  404,  405,
  406,  407,  408,  409,  411,  410,    0,  369,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,   11,    0,    0,   20,   18,
    0,    0,    0,    0,    0,    0,    0,    0,  277,    0,
  150,    0,    0,    0,    0,  326,    0,  351,    0,    0,
    0,  333,  350,  349,  352,  358,  359,  319,  318,  317,
  311,    0,  342,  338,  320,    0,   12,    0,   31,    0,
    0,    0,    0,    0,  121,    0,  117,    0,    0,  120,
    0,    0,    0,    0,  238,  232,    0,    0,    0,    0,
  155,  157,  158,  159,  160,  146,   94,    0,    0,   95,
   87,   88,   89,   90,   91,   92,   93,   96,    0,    0,
   85,   81,  323,  331,    0,    0,  345,  347,  106,  107,
    0,  315,  400,   26,   32,    0,   78,   80,    0,    0,
   69,   71,   72,   73,   74,   75,   76,   77,   79,  417,
    0,  127,    0,  126,    0,  125,    0,    0,    0,  235,
    0,  237,  151,  162,    0,  152,  156,    0,  115,  109,
  114,    0,    0,   82,   86,    0,    0,    0,    0,   66,
   70,  122,  119,    0,    0,  243,  242,    0,  239,    0,
    0,    0,  481,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,  460,    0,    0,    0,    0,  447,    0,
    0,    0,    0,    0,    0,  479,  480,  476,    0,    0,
  442,  444,  445,  446,  448,  449,  450,  451,  452,  453,
  454,  455,  456,  457,  458,  459,    0,    0,    0,    0,
    0,  475,  483,  484,    0,  501,  502,  503,  504,  520,
  521,  522,  523,  524,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,  181,    0,  344,  332,    0,  348,
  346,  128,  123,  241,    0,    0,    0,    0,  525,  547,
    0,    0,    0,    0,  526,    0,    0,    0,    0,    0,
    0,    0,  486,    0,  533,    0,    0,    0,    0,  548,
  549,    0,  468,    0,    0,    0,    0,  467,  465,  466,
  464,  321,  439,  443,  462,  463,    0,  102,    0,  469,
    0,  473,  415,    0,  485,    0,    0,    0,  207,    0,
  206,    0,    0,    0,    0,  211,  208,  210,    0,    0,
    0,    0,    0,    0,  178,  177,  176,  227,    0,    0,
    0,    0,  165,    0,    0,    0,  472,    0,    0,  510,
  517,    0,  509,    0,    0,  414,    0,  529,  527,    0,
  530,    0,  531,  536,    0,    0,    0,  539,  557,  556,
    0,    0,  461,    0,    0,    0,    0,    0,    0,   98,
    0,    0,    0,    0,    0,    0,  187,  186,  183,  188,
  189,  182,  201,  200,  193,  194,  190,  192,  191,  195,
  184,  185,  196,  197,  203,  202,  198,  199,    0,    0,
    0,    0,    0,  101,    0,  108,  172,    0,    0,    0,
    0,  307,    0,    0,    0,    0,  551,    0,    0,    0,
  528,    0,    0,    0,    0,  545,  540,    0,    0,    0,
  104,  103,    0,  216,   97,    0,  218,  217,    0,    0,
    0,    0,    0,    0,    0,  131,    0,    0,  228,    0,
    0,    0,    0,    0,  166,    0,  100,    0,    0,    0,
    0,  513,    0,  518,    0,  554,    0,  489,    0,  543,
  535,  555,  505,    0,   99,    0,    0,    0,  113,    0,
  209,  213,    0,  230,  112,    0,    0,  140,  142,    0,
    0,  134,  139,    0,  137,  135,  164,  173,    0,    0,
    0,  163,    0,  553,  550,  552,    0,    0,  500,    0,
    0,    0,    0,  493,    0,  496,  542,    0,  488,    0,
  219,    0,    0,    0,    0,    0,    0,    0,  132,    0,
    0,    0,  229,    0,    0,    0,  167,  506,    0,    0,
  515,    0,    0,  499,  490,  494,    0,    0,  497,  546,
  223,  225,    0,    0,  221,    0,  222,  205,  204,    0,
    0,  179,    0,    0,  231,  145,  144,  141,  143,  174,
    0,    0,    0,  519,  498,    0,    0,  220,    0,    0,
    0,  133,    0,    0,    0,  507,  224,  226,  214,  215,
    0,  175,  170,  171,  180,
  };
  protected static  short [] yyDgoto  = {             4,
    5,  378,  124,  384,  647,  407,  572,  298,  385,  386,
  387,  125,  470,  408,  643,   53,   54,  388,  811,  299,
   55,  389,  619,  340,  126,  390,  216,  258,  174,  628,
  629,  428,    8,    9,   10,   11,   12,  337,   13,   14,
   29,   25,   15,  249,  170,   65,  338,  380,  381,   16,
   17,   18,   19,   20,   21,   30,   31,   32,   33,   34,
   68,  175,  127,  128,  129,  130,  293,  132,  439,  440,
  441,  411,  412,  413,  414,  415,  416,  417,  418,  251,
  355,  419,  420,  421,  699,  667,  677,  430,    0,  519,
  455,  456,  732,  779,  816,  894,  780,  822,  781,  826,
  898,  861,  862,  262,  353,  263,  350,  351,  399,  400,
  401,  402,  403,  404,  405,  465,  785,  740,  831,  864,
  913,  574,  657,  729,  575,  812,  294,  851,  806,  883,
  852,  853,  906,  907,  576,  735,  820,  260,  348,  393,
  394,  395,  176,  177,  193,  133,  342,  134,  135,  136,
  137,  138,  139,  140,  141,  142,  143,  144,  145,  146,
  147,  148,  149,  150,  151,  152,  153,  295,  296,  369,
  266,  154,  155,  578,  431,  156,  157,  158,  526,  527,
  159,  160,  161,  162,  163,  164,  165,  166,  167,  168,
  169,  634,  343,  252,  529,  530,  531,  532,  533,  534,
  535,  536,  537,  538,  539,  540,  541,  542,  543,  544,
  545,  546,  547,  548,  549,  550,  551,  552,  553,  554,
  555,  635,  798,  842,  843,  844,  845,  846,  556,  557,
  558,  559,  672,  793,  869,  673,  674,  871,  560,  561,
  562,  563,  564,  606,  686,  687,  688,  848,  755,  756,
  746,  747,  691,
  };
  protected static  short [] yySindex = {         -199,
    0, -357, -265,    0,    0, -234,  663, -199,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0, -182, -262,    0, -124,    0,    0,    0,    0,  -45,
  -81,    9,    0,   18,    0,    0,    0,    0,    0,    0,
  -45,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0, 6092,   12, -234,    0,    0,  -45,  -11,
    0,    0,  -81,  -45,   48,    0, 3691,    0, -262,    0,
   -8, 4778,   27,   39,  110,    0,   81,    0,   99,    0,
    0, -280,    0,    0,    0,  128,    0,    0,    0,    0,
    0,    0, 4822,    0,    0,    0,    0,  147,    0,    0,
    0,  170,    0,    0,  209,    0,    0, 3827, 3827, 3827,
 3827, 3827, 3827, 3827, 3827, 3827,    0,    0,    0,    0,
    0,    0,  217,    0,    0, -262,  243,    0,  214,  253,
    0,    0,  226,   66,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,  255,    0,    0,    0, 1704,    0, -150, -137,
 -237,  -62,  254,  244,  258,  248, -307,    0,    0,  287,
  290, -275,    0, -285,    0,    0,    0,  295,  297,  290,
    0,    0, 3827,  227, 3827,  283, -229, 4822, 4822, 3827,
    0,  134,  137,  249,    0,    0,    0,    0,    0,    0,
    0,    0, 3827,    0, 3691,  228, 3827, 3827, 3827, 3283,
 3555,  239,    0,    0,  245,  304,    0,  247, 3827, 3827,
 3827, 3827, 3827, 3827, 3827, 3827, 3827, 3827, 3827, 3827,
 3827, 3827, 3827, 4822, 4822, 3827, 3827, 3827, 3827, 3827,
 3827, 3827, 3827, 3827, 3827, 3827, 3827,  327,  319, 4822,
    0,    0,    0,  330,  320,    0,  333,    0, 4822,  336,
  -45,  337,    0,  339,    0,   22,    0,  338,    0, 3283,
 3555,  340,  148,  150,  341, 4235,    0,    0, 3827,    0,
    0, 4099,    0,  253,    0,  217,    0,    0,    0,    0,
 3827, 3827,    0,  342,  335,    0,    0,  343,  344,   72,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,  226,    0,  226, -150,
 -150,  -64,  -64, -137, -137, -137, -137, -237, -237,  -62,
  254,  244,  258,  345,  248,    0,  327,  346,    0,    0,
  -64,  347,  354,  346,  346,  -64,  346,  319,    0,  351,
    0,  346,  319,  346,  319,    0, 3827,    0,  356,   93,
 3147,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0, 3283,    0,    0,    0, 3827,    0,  346,    0,  365,
  346, 4822,  346,  319,    0,  358,    0,  142,  364,    0,
  370,  260,  374,  363,    0,    0,  -45,  381,  376,  346,
    0,    0,    0,    0,    0,    0,    0,  133, 5637,    0,
    0,    0,    0,    0,    0,    0,    0,    0,  377,  346,
    0,    0,    0,    0,  304, 4822,    0,    0,    0,    0,
  366,    0,    0,    0,    0,  -64,    0,    0,  380,  346,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
  346,    0, 4822,    0, 4822,    0,  378,  379,  368,    0,
  346,    0,    0,    0,  499,    0,    0, 1718,    0,    0,
    0,  265, 4408,    0,    0,  340,  -16, 3419,  386,    0,
    0,    0,    0, -245, -238,    0,    0, 3827,    0, 4822,
 -251, -270,    0,  383,   -4, 4340,  388, 1877,  390,  393,
  394, -258,  395,  398, 3827,  399, 3827,  402,  201,  402,
  403,  384,  404,    0, 3827, 3827,  401,    1,    0,  -37,
    0,    0,    0,    0, 1704,    0,    0,    0,  408, 1718,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,  407,  409,  328,    7,
  410,    0,    0,    0, 3827,    0,    0,    0,    0,    0,
    0,    0,    0,    0,  412, 4822, 4822, -106,    4, -195,
  415,  144, -269,  206,    0,  419,    0,    0, 3555,    0,
    0,    0,    0,    0, -198,  420,  421,  208,    0,    0,
 3827,  304,   66,  348,    0,  424, 4822, 2110, 4822, 3827,
  426,  427,    0, 3827,    0,  433, 3827,  434, -193,    0,
    0, 3963,    0, 3827,    0,    0, 1718,    0,    0,    0,
    0,    0,    0,    0,    0,    0,  428,    0,  429,    0,
  328,    0,    0,  446,    0,  447, -188, -175,    0, 4822,
    0, 4822,  449, -262,  346,    0,    0,    0, 1579,  448,
  428,  451,  445,  161,    0,    0,    0,    0,  453,  454,
  346,  346,    0,  346,  458,  455,    0,  463, -121,    0,
    0,  460,    0,  461, -105,    0,  465,    0,    0,  464,
    0,  475,    0,    0,  478,  554,  538,    0,    0,    0,
  484,  488,    0, 3419,  328,  429, 1877,  402,  172,    0,
  494,  180, -292,  -66,  346,  492,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,  495,  346,
  346,  502, -282,    0,  346,    0,    0,  503,  500,  346,
  505,    0, 3827, 3827,  493,   38,    0, 3827, 2246,  556,
    0, 1877,  512, 4822,  402,    0,    0,  402, 1877, 1877,
    0,    0,  573,    0,    0,  348,    0,    0, 4822, 4822,
  515,  513, 4822,  514,  518,    0,  522,  250,    0,  346,
  346,  516,  530,  268,    0,  526,    0,  532, 3827, 1877,
  456,    0,  531,    0, 3827,    0,   34,    0, -100,    0,
    0,    0,    0, 1877,    0,  346,  -99,  -92,    0, -223,
    0,    0,  -82,    0,    0,  346,  346,    0,    0,  535,
  529,    0,    0,  534,    0,    0,    0,    0,  537,  539,
  543,    0,  540,    0,    0,    0, 2246,  544,    0, 3827,
  533,  547,   34,    0, 1400,    0,    0,  549,    0,  -77,
    0,  346,  346,  551,  553,  555,  557,   53,    0,  558,
  237,  237,    0,  346,  346,  346,    0,    0,  559,  461,
    0, 1877,  560,    0,    0,    0,    0, 1718,    0,    0,
    0,    0,  563,  583,    0,  643,    0,    0,    0, 3283,
 3283,    0, 4822,  565,    0,    0,    0,    0,    0,    0,
  561,  562, 1877,    0,    0,  402,  402,    0,  569,  572,
  -69,    0,  575,  571,  574,    0,    0,    0,    0,    0,
  576,    0,    0,    0,    0,
  };
  protected static  short [] yyRindex = {          804,
    0,    0,    0,    0,    0, 1007,  149,  804,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,  224, 2281,    0,    0,    0,    0,  240,    0,    0,
  577,    0,    0,  163,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0, 4494,    0, 1007,    0,    0,    0,    0,
    0,    0,  577,  579,    0,    0,  585,    0,  578,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0, 4938,    0,    0, 2932,    0,    0,  586,  587,
    0,    0,  671, 5009,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0, 5080,    0, 5238, 5383,
 1912, 4255, 2152, 3325,  876,  311,  113,    0,    0,    0,
  593,  582,    0,    0,    0,    0,    0,  595,  596,  593,
    0,    0,    0,    0,    0, 2331,    0,    0,    0,    0,
   89,    0,    0, 1104,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,  594,
  599,    0,    0,    0,    0, 1965,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0, 5683,  954,    0,
    0,    0,    0,    0,  599,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,  594,
  599, -225,    0,    0,    0, 4867,    0,    0,    0,    0,
    0,  152,    0,  597,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,  606,    0,    0,  607,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0, 5131,    0, 5187, 5289,
 5333, 1260, 1419, 5427, 5455, 5499, 5549, 6071, 6099, 6127,
 3189, 3461, 1088,    0, 1247,    0, 5736, 5789,    0,    0,
 -181,  611,    0, 4602, 4602,  613, -319,  871,    0,  615,
    0,  329,  871, 2547,  871,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0, 2454,    0,    0,
 5832,    0, 2627,  871,    0,  276,    0, 4854,    0,    0,
    0,    0,    0,  616,    0,    0,    0, 4746,    0, 4652,
    0,    0,    0,    0,    0,    0,    0,    0, 4562,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0, 2707,
    0,    0,    0,    0, 2790,    0,    0,    0,    0,    0,
  616,    0,    0,    0,    0, -157,    0,    0,    0, 2787,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
 4692,    0,    0,    0,    0,    0,    0,    0,   25,    0,
 -315,    0,    0,    0,    0,    0,    0,  617,    0,    0,
    0,    0,    0,    0,    0, 2861,    0,  618,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
   24,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,  610,    0,  610,    0,    0,    0,
    0, -314,    0,    0,    0,    0, 5966, -308,    0, 3003,
 5794, 5861, 5884, 5951,    0,    0,    0,    0,    0,  620,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,  541,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,   19,
 -252,    0,    0,    0,    0,    0,    0,    0,  599,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0, -308,  541,    0,    0,    0,    0,  621,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,  676,    0,
    0,    0,    0,    0, 1566, 1713,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,  122,    0,  -58,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,  288, 4602,    0,    0,    0,    0,    0,
   67,  626,  288,    0,    0,    0,    0,    0,    0,    0,
 4602, 4692,    0, 4602,    0,    0,    0,    0,    0,    0,
    0,    0,    0,  622,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,  631, 1082,    0,    0,    0,    0,
    0,    0,    0,    0,    0,   26,    0,    0,    0,    0,
    0,    0,    0,    0, 4602,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,  -39,
 4602,    0,    0,    0,  293,    0,    0,    0,    0,  293,
    0,    0,    0,    0,    0,    0,    0,  630,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0, 1241,    0,    0,    0,    0,    0,    0,    0,
    0,  238,    0,    0,    0,    0,    0,    0,    0,    2,
  194,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,  640,    0,  642,    0,
    0,    0,    0,    0,    0,  -42,    0,    0,    0,    0,
    0,    0,    0,    0,    0,  293,  -39,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,  644,    0,    0,    0,
    0,    0,  647,    0,    0,    0,    0,    0,    0,    0,
    0,  677,  729,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,  293,   90,  199,    0,    0,    0,  650,
    0,    0,    0,    0,    0,    0, 1559, -179,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,  594,
  594,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,
  };
  protected static  short [] yyGindex = {            0,
   10,  192,  -68,    0,    0, -189,    0,    0,  546,    0,
  332,    0,    0,    0, -475,  946,    0,    3,    0,    0,
  600, -332, -204,    6,   -2,  552,  809,  -83,  -61,  309,
 -441, -469,    0,   40, 1002, -211, -304,    0,    0,    0,
  158,    0,    0,    0,    0,  -57,    0,    0,    0, -259,
    0,    0,    0,    0,    0,    0,  981,    0,  957,    0,
    0,    5,    0,    0,    0,  817,  334,  819,    0,    0,
  589, -154, -153, -140, -129, -128, -115, -113, -101,  843,
    0,    0,    0,  619,    0, -618, -696, -257,    0, -379,
    0,    0,    0,  219,    0,    0,  259,    0,  261,    0,
  184,    0,    0,    0,    0,    0,    0,  645,    0,    0,
  648,    0,    0,    0,    0,    0,  188,    0,    0,    0,
    0,    0,    0,    0,    0,    0, -268,    0,    0,    0,
  203,  202,    0,    0,    0,    0,    0,    0,    0,    0,
    0,  598,    0,    0,    0,  -88,    0, -463,    0,    0,
    0,   36,    0,    0,    0,  105,  141,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,  686,  768,
 -166,  230,    0,    0,    0,  785,  -29,    0,    0,    0,
  270,  284,  361,  820,  822,  824,  818,  823,    0,    0,
  246, -593,    0,    0,    0,  221, -516,    0, -456,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0, -544,    0,  584,    0, -591,    0,    0,    0,
    0,    0,    0,    0,    0,  229,    0,  234,    0,    0,
    0,    0,    0,    0,    0,    0,  232,    0,    0,    0,
    0,    0,    0,  564,    0,    0,  387,    0,    0,    0,
    0,  296,    0,
  };
  protected static  short [] yyTable = {            23,
   23,  359,    7,  173,  520,   79,  671,   35,  580,  600,
    7,  303,  391,  624,  362,   36,   26,  601,  700,  194,
  692,  236,    1,  234,  173,   22,  269,   23,  471,  276,
  278,  187,  593,  379,   35,  649,  336,  276,   69,  191,
  856,  596,  777,  310,  300,   37,  787,   57,   27,  769,
  217,  255,   23,  670,  587,  650,   23,    1,  255,  246,
  235,   23,  684,  183,   22,   36,  520,  690,   35,   23,
  256,  184,   23,  255,  255,  685,  435,  256,  247,  195,
  196,  197,  198,  199,  200,  201,  202,  253,  495,   60,
   23,   23,  256,  256,  410,   76,  495,  652,  255,   23,
  693,  857,   40,  272,  360,  255,   40,  276,  278,    3,
   23,  253,  270,  335,  271,  590,  278,  256,  278,  173,
  173,  236,  237,  438,  256,  377,  273,  274,  609,  610,
  611,  654,  304,  256,  520,    2,  257,  278,  238,   78,
  239,  317,  319,  873,    3,  255,  254,  805,  520,  639,
  788,  588,  651,  520,  792,  255,   28,  794,  258,   24,
  410,  495,  701,  602,  256,  173,  173,  253,  255,   23,
  586,  258,  322,  323,  256,   59,  582,  288,  289,  290,
  438,  173,  259,  583,  881,   23,   23,  256,  341,  696,
  173,    6,  648,  437,  656,  259,  702,  346,  640,   56,
  318,  318,  318,  318,  230,  231,  318,  318,  318,  318,
  318,  318,  318,  318,  318,  318,   77,  318,  577,   40,
  476,   61,  255,  660,  761,   40,   62,   40,  442,  443,
   40,   23,   23,  666,  232,   40,  233,  882,  255,   40,
  763,  256,  444,  255,  255,  671,  651,   23,   40,  367,
  437,  255,  288,  445,  446,   40,   23,  256,   23,  641,
   40,  255,  256,  256,   40,  349,   40,  447,   40,  448,
  256,   64,   40,   40,  255,  770,   40,  255,   40,  255,
  256,  449,   40,   71,  470,  442,  443,  470,   72,  839,
   40,   40,   73,  256,   40,  796,  256,   40,  256,  444,
  745,  840,  802,  803,  210,  116,  211,   74,  642,  841,
  445,  446,  706,  173,  212,  621,  750,  240,  764,  241,
  436,  847,  854,  276,  447,  620,  448,  579,  738,  855,
  213,  741,  214,  835,   75,  468,  462,  185,  449,  858,
    7,  304,  138,  276,  255,  631,  256,  849,  276,  392,
  255,   40,  921,  396,  398,   66,  409,  173,  406,   67,
  422,  624,  276,  618,  477,  240,  356,  276,  471,  630,
  215,  471,  771,  479,  357,  800,   62,  240,  801,   23,
  790,  520,   40,    7,  173,  409,  173,   36,  621,  450,
  791,  484,   81,  485,   23,  892,  173,  774,  775,  518,
  131,  349,  398,  492,  173,  893,   23,  210,   23,  211,
   78,  573,  105,  171,  520,  904,  375,  212,   23,  105,
  418,  173,  409,   23,  357,  418,  181,  592,  585,  418,
  168,  252,  252,  213,  304,  214,  622,  425,  525,   40,
  310,  192,  409,  182,  418,  357,  916,  452,  178,  453,
   23,  252,   23,  399,  454,  399,  399,  399,  399,  399,
  179,  518,   23,  392,  105,  399,  304,  105,  525,  185,
   23,  418,  468,  215,  105,  399,  276,  255,  469,  279,
  255,  897,  897,  468,  860,  615,  616,   23,  188,  646,
  363,  255,  364,  255,  257,  257,  277,  173,  173,  280,
  525,  320,  321,  521,  637,  638,  734,   54,  304,  622,
  256,  189,  256,  695,  257,   54,  265,  765,  268,  324,
  325,  326,  327,  275,  766,  768,  917,  918,  173,  518,
  173,  180,  695,  521,  136,  669,  283,  675,  285,  169,
  468,   40,  190,  518,  265,  468,   40,  663,  518,  664,
  190,  655,  306,  307,  308,  309,  310,  311,  312,  313,
  314,  315,  316,   23,   23,  521,  205,  644,  525,   23,
  653,  173,  522,  173,  203,   23,  468,  212,  703,  334,
  704,   23,  896,  212,   23,  204,   47,  525,  207,  208,
  209,   23,   23,   40,   23,   40,   23,  818,   40,  819,
  328,  329,  522,   40,  265,  206,  218,   40,  523,  243,
   40,  282,  208,  209,  242,  829,   40,  830,  118,  244,
  118,  909,  910,   40,  368,  368,  248,  129,   40,  129,
   40,  245,   40,  521,  522,  644,  250,   23,  523,   23,
   40,  259,   40,  261,   40,  253,   40,  255,  267,  286,
   40,  397,  521,  397,  397,  397,  397,  397,   40,   40,
  301,    2,   40,  397,  339,   40,  302,  525,  305,  153,
  523,  344,  297,  397,  345,  347,  352,  397,  354,  361,
  358,  459,  464,  365,  371,  173,  565,  372,  374,    3,
  423,  376,  799,  383,  429,  373,  397,  524,  424,  382,
  173,  173,  522,  397,  173,  434,  457,  807,  808,  433,
  451,  813,  458,  528,  460,  461,  466,  474,  478,  525,
  480,  522,  525,  486,  487,  488,  581,  524,  589,  525,
  525,  597,  521,  595,  598,  599,  603,  778,  523,  604,
  607,  468,  784,  528,  612,  614,  613,  617,  623,  627,
   40,   23,  625,  636,  626,  632,  645,  523,  658,  524,
  525,  661,  668,   83,  662,   84,   23,   23,   85,  666,
   23,  678,  679,   87,  525,  528,  518,   88,  681,  683,
  490,  695,  821,  824,  521,  694,   90,  521,  697,  698,
  705,  730,  731,   91,  521,  521,  733,  736,   92,  737,
  742,  522,   95,    5,  744,  748,  752,  525,  850,  518,
  751,  429,  743,  749,   96,  525,   97,  753,  778,  754,
   99,  584,  685,  758,  173,  521,  759,  524,  103,  104,
  760,  911,  106,  767,  772,  491,  773,  523,  605,  521,
  605,  776,  525,  528,  783,  782,  524,  786,  525,  795,
  789,  797,  804,  522,  884,  886,  522,  809,  814,  810,
  815,  827,  528,  522,  522,  817,  784,  901,  902,  828,
   19,  832,  521,  525,  833,  863,  837,  745,  819,  874,
  521,  818,  865,  867,  866,  868,  872,  875,  633,  523,
   23,  880,  523,  888,  522,  889,  890,  882,  891,  523,
  523,  903,  895,  908,  881,  912,  905,  521,  522,  915,
  914,  919,  659,  521,  920,  922,  923,   17,  925,  924,
   62,   21,   38,   22,  665,  276,  524,   59,   60,   58,
  523,  373,  436,  676,  233,  147,  312,  680,  521,   57,
  682,  522,  528,  339,  523,  689,   39,  633,  313,  522,
  438,  340,  234,   19,  149,  532,   21,  440,   22,   40,
  441,  537,  335,   41,   42,  130,  508,  511,  373,   43,
  544,   44,   45,   46,   47,  512,  522,  523,  524,   48,
  491,  524,  522,   49,  541,  523,  514,  492,  524,  524,
   40,   40,  516,  739,  528,   50,  482,  528,   51,   70,
   52,  281,  483,  762,  528,  528,    5,  522,  473,   58,
   63,  373,  523,  373,  373,  373,  373,  373,  523,  524,
   80,  284,  264,  373,  287,  373,  373,  429,  481,  373,
  373,  373,  373,  524,  859,  528,  373,  373,  475,  825,
  823,  463,  373,  523,  373,  899,  373,  467,  373,  528,
  373,  900,  373,  885,  373,  887,  373,  432,  489,  370,
  366,  330,  333,   40,  331,  878,  524,  332,  870,  335,
  608,  876,  757,    0,  524,   40,  676,  633,  879,  594,
   40,  633,  528,    0,   40,    0,  836,   40,    0,    0,
  528,    0,    0,    0,    0,    0,    0,    0,    0,   40,
   40,  524,    0,    0,   40,   40,    0,  524,    0,    0,
   40,    0,   40,   40,   40,   40,    0,  528,    0,    0,
   40,    0,  834,  528,   40,    0,   40,   19,  838,    0,
   19,    0,  524,    0,    0,   19,   40,   19,    0,   40,
   19,   40,   19,   19,    0,   19,    0,   19,  528,   19,
    0,   19,   19,   19,   19,    0,    0,    0,   19,    0,
    0,    0,    0,   19,    0,   19,   19,   19,    0,    0,
   19,   19,   19,  676,   19,    0,    0,   19,    0,   19,
   19,   19,   19,    0,    0,    0,   19,   19,   19,    0,
    0,   19,   19,   19,    0,    0,    0,    0,    0,    0,
   19,   19,    0,   19,   19,   19,   19,   19,   19,    0,
   19,   19,    0,   19,   19,    0,  395,    0,  395,  395,
  395,  395,  395,    0,   19,   19,    0,    0,  395,    0,
   19,    0,    0,    0,   19,    0,    0,   19,  395,    0,
    0,    0,  395,    0,    0,    0,    0,    0,    0,   19,
   19,    0,    0,    0,   19,   19,    0,    0,    0,  395,
   19,  395,   19,   19,   19,   19,   41,    0,    0,    0,
   19,    0,    0,    0,   19,    0,   19,    0,   41,    0,
    0,    0,    0,   41,    0,    0,   19,   41,   19,   19,
   41,   19,   19,    0,   19,    0,    0,   19,    0,    0,
    0,    0,   41,   41,    0,    0,    0,   41,   41,    0,
    0,    0,    0,   41,    0,   41,   41,   41,   41,    0,
    0,    0,    0,   41,    0,    0,    0,   41,    0,   41,
    0,    0,    0,    0,    0,    0,    0,  534,    0,   41,
    0,    0,   41,    0,   41,  534,  534,  534,  534,  534,
    0,  534,  534,    0,  534,  534,  534,  534,    0,  534,
  534,  534,    0,    0,  373,    0,  534,  538,  534,  534,
  534,  534,  534,  534,    0,    0,  534,    0,    0,    0,
  534,  534,    0,  534,  534,  534,    0,    0,    0,    0,
    0,    0,    0,    0,    0,  534,    0,  534,    0,  534,
  534,  373,    0,  534,    0,  534,  534,  534,  534,  534,
  534,  534,  534,  534,  534,  534,  534,    0,  534,    0,
  534,  534,  534,  534,    0,    0,    0,  534,  396,    0,
  396,  396,  396,  396,  396,  534,  534,  534,  534,    0,
  396,    0,  534,    0,  534,    0,  373,  373,    0,  534,
  396,  534,    0,    0,  396,    0,    0,    0,  373,  373,
    0,    0,  373,  373,  373,  373,    0,    0,    0,  373,
  373,  396,    0,  396,    0,  373,    0,  373,    0,  373,
    0,  373,    0,  373,    0,  373,    0,  373,    0,  373,
    0,  534,    0,  534,    0,  534,  487,  534,    0,  534,
    0,  534,    0,  534,  487,  487,  487,  487,  487,    0,
  487,  487,    0,  487,  487,  487,  487,    0,  487,  487,
  385,    0,    0,    0,    0,  487,    0,  487,  487,  487,
  487,  487,  487,    0,    0,  487,    0,    0,    0,  487,
  487,    0,  487,  487,  487,    0,    0,    0,    0,    0,
    0,    0,    0,    0,  487,    0,  487,  385,  487,  487,
    0,    0,  487,    0,  487,  487,  487,  487,  487,  487,
  487,  487,  487,  487,  487,  487,    0,  487,    0,  487,
  487,  487,  487,    0,    0,    0,  487,  398,    0,  398,
  398,  398,  398,  398,  487,  487,  487,  487,    0,  398,
  385,  487,  385,  487,  385,  385,  385,    0,  487,  398,
  487,    0,  385,  398,    0,    0,    0,    0,  385,  385,
  385,  385,    0,    0,    0,  385,  385,    0,    0,    0,
    0,    0,  398,    0,    0,  385,    0,  385,    0,  385,
    0,  385,    0,  385,    0,  385,    0,    0,    0,    0,
  487,    0,  487,    0,  487,  877,  487,    0,  487,    0,
  487,    0,  487,   82,   83,  494,   84,  840,    0,   85,
  495,    0,  496,  497,   87,  841,    0,  498,   88,  384,
    0,    0,    0,    0,   89,    0,  499,   90,  500,  501,
  502,  503,    0,    0,   91,    0,    0,    0,  504,   92,
    0,   93,   94,   95,    0,    0,    0,    0,    0,    0,
    0,    0,    0,  505,    0,   96,  384,   97,   98,    0,
    0,   99,    0,  506,  100,  507,  101,  508,  102,  103,
  104,  509,  510,  106,  511,    0,  512,    0,  513,  468,
    0,  108,    0,    0,    0,  514,    0,    0,    0,    0,
    0,    0,    0,  109,  110,  111,  112,    0,    0,  384,
  113,  384,  114,  384,  384,  384,    0,  515,    0,  516,
    0,  384,    0,    0,    0,    0,    0,  384,  384,  384,
  384,    0,    0,    0,  384,  384,    0,    0,    0,    0,
    0,    0,    0,    0,  384,    0,  384,    0,  384,    0,
  384,    0,  384,    0,  384,    0,    0,    0,    0,  117,
    0,  118,    0,  119,  500,  120,    0,  121,    0,  122,
    0,  517,  500,  500,  500,  500,  500,    0,  500,  500,
    0,  500,  500,  500,  500,    0,  500,  500,    0,    0,
    0,    0,    0,  500,    0,  500,  500,  500,  500,  500,
  500,    0,    0,  500,    0,    0,    0,  500,  500,    0,
  500,  500,  500,  707,    0,    0,    0,    0,    0,    0,
    0,    0,  500,    0,  500,    0,  500,  500,    0,    0,
  500,    0,  500,  500,  500,  500,  500,  500,  500,  500,
  500,  500,  500,  500,    0,  500,    0,  500,  500,    0,
  500,    0,    0,    0,  481,  708,    0,    0,  367,    0,
    0,  367,  500,  500,  500,  500,    0,    0,  367,  500,
    0,  500,    0,  363,    0,    0,  500,    0,  500,    0,
    0,    0,  709,  710,  711,  712,    0,  713,  714,  715,
  716,  717,  718,  719,  720,    0,  721,    0,  722,    0,
  723,    0,  724,  363,  725,  363,  726,  363,  727,  363,
  728,  363,    0,  363,    0,  363,    0,  363,  500,  363,
  500,  363,  500,  493,  500,    0,  500,    0,  500,    0,
  500,   82,   83,  494,   84,    0,    0,   85,  495,    0,
  496,  497,   87,    0,    0,  498,   88,    0,    0,    0,
    0,    0,   89,    0,  499,   90,  500,  501,  502,  503,
    0,    0,   91,    0,    0,    0,  504,   92,    0,   93,
   94,   95,    0,    0,    0,    0,    0,    0,    0,    0,
    0,  505,    0,   96,    0,   97,   98,    0,    0,   99,
    0,  506,  100,  507,  101,  508,  102,  103,  104,  509,
  510,  106,  511,    0,  512,  368,  513,  468,  368,  108,
    0,  219,    0,  514,    0,  368,    0,    0,    0,    0,
  364,  109,  110,  111,  112,    0,    0,    0,  113,    0,
  114,    0,    0,    0,    0,  515,    0,  516,    0,    0,
    0,  220,    0,  221,    0,  222,    0,  223,    0,  224,
  364,  225,  364,  226,  364,  227,  364,  228,  364,  229,
  364,    0,  364,    0,  364,    0,  364,    0,  364,    0,
    0,    0,    0,    0,    0,    0,    0,  117,    0,  118,
    0,  119,  493,  120,    0,  121,    0,  122,    0,  517,
   82,   83,  494,   84,    0,    0,   85,  495,    0,    0,
  497,   87,    0,    0,  498,   88,    0,    0,    0,    0,
    0,   89,    0,  499,   90,  500,  501,  502,  503,    0,
    0,   91,    0,    0,    0,  504,   92,    0,   93,   94,
   95,    0,    0,    0,    0,    0,    0,    0,    0,    0,
  505,    0,   96,    0,   97,   98,    0,    0,   99,    0,
  506,  100,  507,  101,  508,  102,  103,  104,  509,  510,
  106,  511,    0,  107,    0,  513,  468,    0,  108,    0,
  335,    0,  514,    0,    0,  335,    0,    0,    0,    0,
  109,  110,  111,  112,    0,    0,    0,  113,    0,  114,
    0,    0,    0,    0,  515,    0,  516,    0,    0,    0,
    0,    0,  386,    0,  386,  386,  386,  386,  386,    0,
    0,    0,  335,    0,  386,    0,    0,    0,    0,  335,
    0,    0,  386,  386,  386,    0,    0,  386,  386,    0,
    0,    0,    0,    0,    0,    0,  117,    0,  118,  335,
  119,  386,  120,  386,  121,  386,  122,  386,   62,    0,
    0,    0,    0,    0,  335,  335,  335,  335,    0,  335,
  335,  335,    0,    0,    0,    0,  335,  335,    0,  335,
  335,    0,  335,  335,  335,  335,  335,  335,  335,  335,
  335,  335,  335,    0,  335,    0,  335,    0,  335,    0,
  335,    0,  335,    0,  335,    0,  335,    0,  335,    0,
  335,    0,  335,    0,  335,    0,  335,    0,  335,    0,
  335,    0,  335,    0,  335,  493,  335,    0,  335,    0,
  335,    0,  335,   82,   83,    0,   84,    0,    0,   85,
   86,    0,    0,    0,   87,    0,  335,    0,   88,    0,
    0,    0,    0,    0,   89,    0,    0,   90,    0,    0,
    0,    0,    0,    0,   91,    0,    0,    0,    0,   92,
    0,   93,   94,   95,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,   96,    0,   97,   98,    0,
    0,   99,    0,    0,  100,    0,  101,    0,  102,  103,
  104,  105,    0,  106,    0,    0,  512,    0,    0,    0,
    0,  108,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,  109,  110,  111,  112,    0,    0,    0,
  113,    0,  114,    0,    0,    0,    0,  515,    0,  516,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,  391,    0,  391,  391,  391,  391,  391,    0,
    0,  493,    0,    0,  391,    0,    0,    0,    0,   82,
   83,    0,   84,  391,  391,   85,   86,  391,  391,  117,
   87,  118,    0,  119,   88,  120,    0,  121,    0,  122,
   89,   62,    0,   90,    0,  391,  245,  391,    0,    0,
   91,  245,    0,    0,    0,   92,    0,   93,   94,   95,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,   96,    0,   97,   98,    0,    0,   99,    0,    0,
  100,    0,  101,    0,  102,  103,  104,  105,  245,  106,
    0,    0,  107,    0,    0,  245,  276,  108,    0,    0,
    0,  276,    0,    0,    0,    0,    0,    0,    0,  109,
  110,  111,  112,    0,    0,  245,  113,    0,  114,    0,
    0,    0,    0,  515,    0,  516,    0,    0,    0,    0,
  245,  245,  245,  245,  245,  245,  245,  245,  276,    0,
    0,    0,    0,  245,    0,    0,    0,    0,    0,  245,
  245,  245,  245,  245,    0,    0,  245,  245,    0,    0,
    0,    0,    0,    0,    0,  117,  245,  118,  245,  119,
  245,  120,  245,  121,  245,  122,  245,   62,    0,    0,
  276,  276,  276,  276,  276,  276,  276,  276,    0,    0,
    0,    0,    0,  276,    0,    0,    0,    0,    0,  276,
  276,  276,  276,    0,    0,    0,  276,  276,    0,    0,
    0,    0,  245,    0,    0,    0,  276,    0,  276,    0,
  276,    0,  276,   41,  276,   41,  276,    0,   41,    0,
   41,    0,    0,   41,    0,   41,   41,    0,   41,    0,
   41,    0,   41,    0,   41,   41,   41,   41,    0,    0,
    0,   41,    0,    0,    0,    0,   41,    0,   41,   41,
   41,    0,  276,   41,   41,   41,    0,   41,    0,   41,
   41,   41,   41,   41,   41,   41,   41,    0,   41,   41,
   41,   41,    0,    0,   41,   41,   41,    0,    0,    0,
    0,    0,    0,   41,   41,    0,   41,   41,    0,   41,
   41,   41,    0,    0,    0,    0,    0,    0,    0,    0,
    0,   41,    0,   41,    0,    0,   40,   41,    0,    0,
    0,   40,    0,   40,    0,    0,   40,    0,   40,   40,
    0,   40,    0,   40,    0,   40,    0,   40,   40,   40,
   40,    0,    0,    0,   40,    0,    0,    0,    0,   40,
    0,   40,   40,   40,    0,    0,   40,    0,   40,    0,
   40,    0,    0,   40,    0,   40,   40,   40,   40,    0,
    0,    0,   40,   40,   40,    0,    0,   40,   40,   40,
    0,    0,    0,    0,    0,   41,   40,   40,    0,   40,
   40,    0,   40,   40,   40,    0,   40,   83,    0,    0,
    0,   40,    0,   40,    0,    0,   40,    0,   40,   40,
   40,   40,    0,   40,    0,   40,    0,   40,   40,   40,
   40,    0,    0,    0,   40,    0,    0,    0,    0,   40,
    0,   40,   40,   40,    0,    0,   40,    0,   40,    0,
   40,    0,    0,   40,    0,   40,   40,   40,   40,    0,
    0,    0,   40,   40,   40,    0,    0,   40,   40,   40,
    0,    0,    0,    0,    0,    0,   40,   40,    0,   40,
   40,    0,   40,   40,   40,    0,   40,   67,   40,    0,
    0,   40,    0,   40,    0,    0,   40,    0,   40,   40,
   40,   40,    0,   40,    0,   40,    0,   40,   40,   40,
   40,    0,    0,    0,   40,    0,    0,    0,    0,   40,
    0,   40,   40,   40,    0,    0,   40,    0,   40,    0,
   40,    0,    0,   40,    0,   40,   40,   40,   40,    0,
    0,    0,   40,   40,   40,    0,    0,   40,   40,   40,
    0,    0,    0,    0,    0,    0,   40,   40,    0,   40,
   40,    0,   40,   40,   40,    0,   40,   84,   40,    0,
  335,   40,    0,   40,    0,    0,   40,    0,   40,   40,
   40,   40,    0,   40,    0,   40,    0,   40,   40,   40,
   40,    0,    0,    0,   40,    0,    0,    0,    0,   40,
    0,   40,   40,   40,    0,    0,   40,  335,   40,    0,
   40,    0,    0,   40,    0,   40,   40,   40,   40,    0,
    0,    0,   40,   40,   40,    0,    0,   40,   40,   40,
    0,    0,    0,    0,    0,    0,   40,   40,    0,   40,
   40,  343,   40,   40,   40,    0,    0,   68,   40,  335,
  335,  335,  335,    0,  335,  335,  335,    0,    0,    0,
   40,  335,  335,    0,  335,  335,    0,  335,  335,  335,
  335,  335,  335,  335,  335,  335,  335,  335,  343,  335,
    0,  335,    0,  335,    0,  335,    0,  335,    0,  335,
    0,  335,    0,  335,    0,  335,    0,  335,    0,  335,
    0,  335,    0,  335,    0,  335,    0,  335,    0,  335,
    0,  335,  280,  335,    0,  335,    0,  335,    0,    0,
    0,  343,  343,  343,  343,  343,  343,  343,   40,    0,
    0,  335,  343,  343,    0,  343,  343,    0,  343,  343,
  343,  343,  343,  343,  343,  343,  343,  343,  343,  280,
  343,    0,  343,    0,  343,    0,  343,    0,  343,    0,
  343,    0,  343,    0,  343,    0,  343,    0,  343,    0,
  343,    0,  343,    0,  343,    0,  343,    0,  343,    0,
  343,    0,  343,  354,  343,    0,  343,    0,  343,    0,
    0,    0,  280,  280,  280,  280,  280,  280,  280,    0,
    0,    0,  343,    0,  280,    0,  280,  280,    0,  280,
  280,  280,  280,  280,  280,  280,  280,  280,  280,  280,
  354,  280,    0,  280,    0,  280,    0,  280,    0,  280,
    0,  280,    0,  280,    0,  280,    0,  280,    0,  280,
    0,  280,    0,  280,    0,  280,    0,  280,    0,  280,
    0,  280,    0,  280,    0,  280,    0,  280,    0,  280,
    0,    0,    0,    0,    0,  354,    0,    0,    0,    0,
    0,    0,    0,  280,    0,    0,    0,  354,  354,    0,
  354,  354,  354,  354,  354,    0,  354,  354,  354,  354,
    0,    0,    0,    0,  354,    0,  354,    0,  354,    0,
  354,    0,  354,    0,  354,    0,  354,    0,  354,    0,
  354,    0,  354,    0,  354,    0,  354,    0,  354,    0,
  354,    0,  354,    0,  354,    0,  354,    0,  354,    0,
   82,   83,    0,   84,    0,    0,   85,   86,    0,    0,
    0,   87,    0,    0,  335,   88,    0,    0,    0,    0,
    0,   89,    0,    0,   90,    0,    0,    0,    0,    0,
    0,   91,    0,    0,    0,    0,   92,    0,   93,   94,
   95,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,   96,    0,   97,   98,  426,    0,   99,    0,
    0,  100,    0,  101,    0,  102,  103,  104,  105,    0,
  106,    0,    0,  107,    0,    0,  361,  427,  108,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
  109,  110,  111,  112,    0,    0,    0,  113,    0,  114,
    0,    0,    0,    0,  115,    0,  116,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,  392,
    0,  392,  392,  392,  392,  392,    0,    0,    0,    0,
    0,  392,    0,    0,    0,    0,   82,   83,    0,   84,
  392,  392,   85,   86,  392,  392,  117,   87,  118,    0,
  119,   88,  120,    0,  121,    0,  122,   89,   62,    0,
   90,    0,  392,    0,  392,    0,    0,   91,    0,    0,
    0,    0,   92,    0,   93,   94,   95,    0,  291,    0,
    0,    0,    0,    0,    0,  292,    0,    0,   96,    0,
   97,   98,    0,    0,   99,    0,    0,  100,    0,  101,
    0,  102,  103,  104,  105,    0,  106,    0,    0,  107,
    0,    0,    0,    0,  108,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,  109,  110,  111,  112,
    0,    0,    0,  113,    0,  114,    0,    0,    0,    0,
  115,    0,  116,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,  393,    0,  393,  393,  393,
  393,  393,    0,    0,    0,    0,    0,  393,    0,    0,
    0,    0,   82,   83,    0,   84,  393,  393,   85,   86,
    0,  393,  117,   87,  118,    0,  119,   88,  120,    0,
  121,    0,  122,   89,   62,    0,   90,    0,  393,    0,
  393,    0,    0,   91,    0,    0,    0,    0,   92,    0,
   93,   94,   95,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,   96,    0,   97,   98,  426,    0,
   99,    0,    0,  100,    0,  101,    0,  102,  103,  104,
  105,    0,  106,    0,    0,  107,    0,    0,  361,    0,
  108,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,  109,  110,  111,  112,    0,    0,    0,  113,
    0,  114,    0,    0,    0,    0,  115,    0,  116,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,  394,    0,  394,  394,  394,  394,  394,    0,    0,
    0,    0,    0,  394,    0,    0,    0,    0,   82,   83,
    0,   84,  394,  394,   85,   86,    0,  394,  117,   87,
  118,    0,  119,   88,  120,    0,  121,    0,  122,   89,
   62,    0,   90,    0,  394,    0,  394,    0,    0,   91,
    0,    0,    0,    0,   92,    0,   93,   94,   95,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
   96,    0,   97,   98,    0,    0,   99,    0,    0,  100,
    0,  101,    0,  102,  103,  104,  105,    0,  106,    0,
    0,  107,    0,    0,    0,    0,  108,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,  297,  109,  110,
  111,  112,    0,    0,    0,  113,    0,  114,    0,    0,
    0,    0,  115,    0,  116,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,   82,   83,    0,   84,    0,    0,
   85,   86,    0,    0,  117,   87,  118,    0,  119,   88,
  120,    0,  121,    0,  122,   89,   62,    0,   90,    0,
    0,    0,    0,    0,    0,   91,    0,    0,    0,    0,
   92,    0,   93,   94,   95,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,   96,    0,   97,   98,
    0,    0,   99,    0,    0,  100,    0,  101,    0,  102,
  103,  104,  105,    0,  106,    0,    0,  107,    0,    0,
    0,    0,  108,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,  109,  110,  111,  112,    0,    0,
    0,  113,    0,  114,    0,    0,    0,    0,  115,    0,
  116,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
   82,   83,    0,   84,    0,    0,   85,   86,    0,    0,
  117,   87,  118,    0,  119,   88,  120,    0,  121,    0,
  122,   89,  123,    0,   90,    0,    0,    0,    0,    0,
    0,   91,    0,    0,    0,    0,   92,    0,   93,   94,
   95,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,   96,    0,   97,   98,    0,    0,   99,    0,
    0,  100,    0,  101,    0,  102,  103,  104,  105,    0,
  106,    0,    0,  107,    0,    0,    0,    0,  108,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
  109,  110,  111,  112,    0,    0,    0,  113,    0,  114,
    0,    0,    0,    0,  115,    0,  116,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,   82,   83,    0,   84,
    0,    0,   85,   86,    0,    0,  117,   87,  118,    0,
  119,   88,  120,    0,  121,    0,  122,   89,   62,    0,
   90,    0,    0,    0,    0,    0,    0,   91,    0,    0,
    0,    0,   92,    0,   93,   94,   95,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,   96,    0,
   97,   98,    0,    0,   99,    0,    0,  100,    0,  101,
    0,  102,  103,  104,  105,    0,  106,    0,    0,  512,
    0,    0,    0,    0,  108,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,  109,  110,  111,  112,
    0,    0,    0,  113,    0,  114,    0,    0,    0,    0,
  115,    0,  116,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,   82,   83,    0,   84,    0,    0,   85,   86,
    0,    0,  117,   87,  118,    0,  119,   88,  120,    0,
  121,    0,  122,   89,   62,    0,   90,    0,    0,    0,
    0,    0,    0,   91,    0,    0,    0,    0,   92,    0,
   93,   94,   95,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,   96,    0,   97,   98,    0,    0,
   99,    0,    0,  100,    0,  101,    0,  102,  103,  104,
  105,    0,  106,    0,    0,  107,    0,    0,    0,    0,
  108,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,  109,  110,  111,  112,    0,    0,    0,  113,
    0,    0,    0,    0,    0,    0,  115,    0,  116,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,   82,   83,
    0,   84,    0,    0,   85,   86,    0,    0,  117,   87,
  118,    0,  119,   88,  120,    0,  121,    0,  122,   89,
   62,    0,   90,    0,    0,    0,    0,    0,    0,   91,
    0,    0,    0,    0,   92,    0,   93,   94,   95,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
   96,    0,   97,   98,    0,    0,   99,    0,    0,  100,
    0,  101,    0,  102,  103,  104,  105,    0,  106,    0,
    0,  107,    0,    0,    0,    0,  108,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,  109,    0,
    0,  112,    0,    0,    0,  389,    0,  389,  389,  389,
  389,  389,    0,   82,   83,    0,   84,  389,    0,   85,
   86,    0,    0,    0,   87,  389,  389,  389,   88,    0,
  389,  389,    0,    0,   89,    0,    0,   90,    0,    0,
    0,    0,    0,    0,   91,    0,    0,    0,  389,   92,
  389,   93,   94,   95,  117,    0,  118,    0,  119,    0,
  120,    0,  121,    0,  122,   96,   62,   97,   98,    0,
    0,   99,    0,    0,  100,    0,  101,    0,  102,  103,
  104,  105,   83,  106,   84,    0,  107,   85,    0,   71,
  566,  591,   87,    0,   72,    0,   88,    0,   73,  567,
  568,    0,    0,    0,    0,   90,    0,    0,    0,    0,
  569,    0,   91,   74,    0,    0,    0,   92,    0,    0,
    0,   95,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,   96,    0,   97,    0,    0,    0,   99,
   75,    0,    0,    0,    0,    0,    0,  103,  104,    0,
    0,  106,    0,    0,  570,    0,    0,    0,    0,  117,
    0,  118,    0,  119,    0,  120,    0,  121,  419,  122,
  419,   62,    0,  419,    0,  419,  419,    0,  419,    0,
  419,    0,  419,    0,  419,  419,  419,    0,    0,    0,
    0,  419,    0,    0,    0,    0,  419,    0,  419,  419,
    0,    0,    0,  419,    0,    0,    0,  419,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,  419,
    0,  419,    0,    0,    0,  419,  419,    0,    0,    0,
    0,    0,    0,  419,  419,    0,  418,  419,  418,  571,
  419,  418,    0,  418,  418,    0,  418,    0,  418,    0,
  418,    0,  418,  418,  418,    0,    0,    0,    0,  418,
    0,    0,    0,    0,  418,    0,  418,  418,    0,    0,
    0,  418,    0,    0,    0,  418,   40,    0,   40,    0,
    0,   40,    0,    0,    0,    0,   40,  418,    0,  418,
   40,    0,    0,  418,  418,    0,    0,    0,    0,   40,
    0,  418,  418,    0,    0,  418,   40,    0,  418,    0,
    0,   40,    0,    0,    0,   40,    0,   40,    0,   40,
    0,    0,    0,    0,   40,  419,   40,   40,   40,   40,
    0,   40,    0,   40,    0,    0,   40,    0,    0,    0,
   40,   40,   40,   40,    0,   40,    0,    0,   40,   40,
    0,    0,    0,    0,  116,    0,   40,    0,    0,    0,
    0,   40,    0,   40,    0,   40,   40,    0,   40,    0,
    0,   40,    0,    0,    0,    0,   40,   40,    0,   40,
   40,    0,    0,   40,    0,    0,    0,    0,    0,   40,
    0,   40,   40,  418,    0,   40,   40,    0,   40,    0,
    0,   40,  154,    0,    0,   40,    0,   40,    0,   40,
    0,    0,    0,    0,   40,    0,    0,   40,    0,   40,
  161,    0,  161,   40,    0,  161,    0,    0,    0,    0,
  161,   40,   40,   40,  161,   40,    0,  161,   40,    0,
    0,    0,    0,  161,    0,    0,    0,    0,    0,    0,
  161,    0,   83,    0,   84,  161,    0,   85,    0,  161,
    0,    0,   87,    0,    0,    0,   88,    0,    0,    0,
    0,  161,    0,  161,    0,   90,    0,  161,    0,    0,
    0,    0,   91,   40,    0,  161,  161,   92,    0,  161,
    0,   95,  161,    0,    0,    0,   83,    0,   84,    0,
    0,   85,    0,   96,    0,   97,   87,    0,    0,   99,
   88,    0,    0,    0,    0,    0,    0,  103,  104,   90,
    0,  106,    0,   40,  172,    0,   91,    0,  124,    0,
  124,   92,    0,  124,    0,   95,    0,  307,  124,    0,
    0,    0,  124,    0,    0,    0,    0,   96,    0,   97,
    0,  124,    0,   99,    0,    0,    0,    0,  124,    0,
    0,  103,  104,  124,    0,  106,    0,  124,  186,    0,
    0,    0,    0,    0,  307,    0,    0,  161,    0,  124,
    0,  124,    0,    0,    0,  124,    0,    0,    0,    0,
    0,    0,    0,  124,  124,    0,    0,  124,    0,    0,
  124,    0,    0,    0,    0,    0,    0,    0,   23,   62,
    0,    0,    0,    0,    0,    0,    0,  307,    0,  307,
  307,  307,  307,  307,    0,    0,    0,    0,  307,  307,
    0,  307,  307,    0,  307,  307,  307,  307,  307,  307,
  307,  307,  307,  307,  307,   23,  307,    0,  307,    0,
  307,    0,  307,   62,  307,    0,  307,    0,  307,    0,
  307,    0,  307,    0,  307,    0,  307,    0,  307,    0,
  307,    0,  307,    0,  307,    0,  307,    0,  307,  354,
  307,    0,  307,    0,  307,  124,    0,    0,    0,   23,
   23,   23,    0,    0,    0,    0,    0,    0,    0,   23,
   23,    0,   23,   23,    0,    0,   23,   23,   23,   23,
   23,   23,   23,   23,   23,   23,  354,   23,    0,   23,
    0,   23,    0,   23,    0,   23,    0,   23,    0,   23,
    0,   23,    0,   23,    0,   23,    0,   23,    0,   23,
    0,   23,    0,   23,    0,   23,    0,   23,    0,   23,
  369,   23,    0,   23,    0,   23,    0,    0,    0,  354,
    0,  354,    0,  354,  354,  354,    0,    0,    0,    0,
    0,  354,    0,  354,  354,    0,  354,  354,  354,  354,
  354,  354,  354,  354,  354,  354,    0,  369,    0,    0,
  354,    0,  354,    0,  354,    0,  354,    0,  354,    0,
  354,  374,  354,    0,  354,    0,  354,    0,  354,    0,
  354,    0,  354,    0,  354,    0,  354,    0,  354,    0,
  354,    0,  354,    0,  354,    0,    0,    0,    0,    0,
  369,    0,  369,  369,  369,  369,  369,    0,  374,    0,
    0,    0,  369,    0,  369,  369,    0,    0,  369,  369,
  369,  369,  369,  369,  369,  369,  369,  375,    0,    0,
    0,  369,    0,  369,    0,  369,    0,  369,    0,  369,
    0,  369,    0,  369,    0,  369,    0,    0,    0,    0,
    0,  374,    0,  374,  374,  374,  374,  374,    0,    0,
    0,    0,    0,  374,  375,  374,  374,    0,    0,  374,
  374,  374,  374,    0,    0,    0,  374,  374,  376,    0,
    0,    0,  374,    0,  374,    0,  374,    0,  374,    0,
  374,    0,  374,    0,  374,    0,  374,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,  375,    0,  375,
  375,  375,  375,  375,    0,  376,    0,    0,    0,  375,
    0,  375,  375,    0,    0,  375,  375,  375,  375,  377,
    0,    0,  375,  375,    0,    0,    0,    0,  375,    0,
  375,    0,  375,    0,  375,    0,  375,    0,  375,    0,
  375,    0,  375,    0,    0,    0,    0,    0,  376,    0,
  376,  376,  376,  376,  376,    0,  377,    0,    0,    0,
  376,    0,    0,  378,    0,    0,  376,  376,  376,  376,
  376,    0,    0,  376,  376,    0,    0,    0,    0,  376,
    0,  376,    0,  376,    0,  376,    0,  376,    0,  376,
    0,  376,    0,  376,    0,    0,    0,    0,    0,  377,
  378,  377,  377,  377,  377,  377,    0,    0,    0,    0,
    0,  377,    0,  379,    0,    0,    0,  377,  377,  377,
  377,  377,    0,    0,  377,  377,    0,    0,    0,    0,
  377,    0,  377,    0,  377,    0,  377,    0,  377,    0,
  377,    0,  377,  378,  377,  378,  378,  378,  378,  378,
  379,    0,    0,    0,    0,  378,    0,  380,    0,    0,
    0,  378,  378,  378,  378,  378,    0,    0,  378,  378,
    0,    0,    0,    0,  378,    0,  378,    0,  378,    0,
  378,    0,  378,    0,  378,  381,  378,    0,  378,    0,
    0,    0,    0,  379,  380,  379,  379,  379,  379,  379,
    0,    0,    0,    0,    0,  379,    0,    0,    0,    0,
    0,  379,  379,  379,  379,  379,    0,    0,  379,  379,
    0,    0,  381,    0,    0,    0,    0,    0,  379,  382,
  379,    0,  379,    0,  379,    0,  379,  380,  379,  380,
  380,  380,  380,  380,    0,    0,    0,    0,    0,  380,
    0,    0,    0,    0,    0,  380,  380,  380,  380,  380,
    0,    0,  380,  380,    0,  381,  382,  381,  381,  381,
  381,  381,  380,    0,  380,    0,  380,  381,  380,  383,
  380,    0,  380,  381,  381,  381,  381,  381,    0,    0,
  381,  381,    0,    0,    0,    0,    0,    0,    0,    0,
  381,    0,  381,    0,  381,    0,  381,    0,  381,  382,
  381,  382,  382,  382,  382,  382,  383,    0,    0,    0,
    0,  382,    0,    0,    0,    0,    0,  382,  382,  382,
  382,  382,    0,    0,  382,  382,    0,    0,    0,    0,
    0,    0,    0,    0,  382,    0,  382,    0,  382,    0,
  382,    0,  382,    0,  382,    0,    0,    0,    0,  383,
    0,  383,  383,  383,  383,  383,   38,    0,    0,    0,
    0,  383,    0,    0,    0,    0,    0,  383,  383,  383,
  383,  383,    0,    0,  383,  383,    0,    0,    0,    0,
   39,    0,    0,    0,  383,    0,  383,    0,  383,    0,
  383,    0,  383,   40,  383,    0,    0,    0,   42,    0,
    0,    0,   27,   43,    0,   44,   45,   46,   47,    0,
    0,    0,    0,   48,   27,    0,    0,   49,    0,   27,
    0,    0,    0,   27,    0,    0,   27,    0,    0,   50,
    0,    0,   51,    0,   52,    0,    0,    0,   27,   27,
    0,    0,    0,   27,   27,    0,    0,    0,    0,   27,
  472,   27,   27,   27,   27,   28,    0,    0,    0,   27,
    0,    0,    0,   27,    0,   27,    0,   28,    0,    0,
    0,    0,   28,    0,    0,   27,   28,    0,   27,   28,
   27,    0,    0,   27,    0,    0,   27,    0,    0,    0,
    0,   28,   28,    0,    0,    0,   28,   28,    0,    0,
    0,    0,   28,    0,   28,   28,   28,   28,   40,    0,
    0,    0,   28,    0,    0,    0,   28,    0,   28,    0,
   40,    0,    0,    0,    0,   40,    0,    0,   28,   40,
    0,   28,   40,   28,    0,    0,   28,    0,    0,   28,
    0,    0,    0,    0,   40,   40,    0,    0,    0,   40,
   40,   40,    0,    0,    0,   40,    0,   40,   40,   40,
   40,    0,    0,   40,    0,   40,    0,    0,   40,   40,
    0,   40,   40,    0,    0,   40,    0,    0,    0,    0,
    0,   40,    0,    0,   40,    0,   40,   40,   40,   29,
    0,    0,   40,   40,    0,  283,  474,  283,   40,  474,
   40,   40,   40,   40,    0,  283,  474,    0,   40,    0,
    0,  283,   40,    0,   40,    0,  283,    0,    0,    0,
    0,  283,    0,  283,   40,    0,    0,   40,    0,   40,
    0,    0,   30,    0,    0,    0,    0,    0,    0,    0,
    0,  283,    0,  283,    0,  283,    0,  283,    0,  283,
    0,  283,    0,  283,    0,  283,    0,  283,    0,  283,
    0,  283,  287,  477,  287,    0,  477,    0,    0,    0,
    0,    0,  287,  477,    0,  283,    0,    0,  287,    0,
    0,    0,    0,  287,    0,  288,  478,  288,  287,  478,
  287,    0,    0,    0,    0,  288,  478,    0,    0,    0,
    0,  288,    0,    0,    0,    0,  288,    0,  287,    0,
  287,  288,  287,  288,  287,    0,  287,    0,  287,    0,
  287,    0,  287,    0,  287,    0,  287,    0,  287,    0,
    0,  288,    0,  288,    0,  288,    0,  288,    0,  288,
    0,  288,  287,  288,    0,  288,    0,  288,    0,  288,
    0,  288,  329,  482,  329,    0,  482,    0,    0,    0,
    0,    0,  329,  482,    0,  288,    0,   23,  329,   23,
    0,    0,    0,  329,    0,    0,    0,   23,  329,    0,
  329,    0,    0,   23,    0,    0,    0,    0,   23,    0,
    0,    0,    0,   23,    0,   23,    0,    0,  329,    0,
  329,    0,  329,    0,  329,    0,  329,    0,  329,    0,
  329,   38,  329,   23,  329,   23,  329,   23,  329,   23,
    0,   23,    0,   23,    0,   23,    0,   23,    0,   23,
    0,   23,  329,   23,    0,   39,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,   23,   40,    0,
    0,    0,    0,   42,    0,    0,    0,    0,   43,    0,
   44,   45,   46,   47,    0,    0,    0,    0,   48,    0,
    0,  387,   49,  387,  387,  387,  387,  387,    0,    0,
    0,    0,    0,  387,   50,    0,    0,   51,    0,   52,
    0,  387,  387,  387,    0,    0,  387,  387,    0,  388,
    0,  388,  388,  388,  388,  388,    0,    0,    0,    0,
  387,  388,  387,    0,  387,    0,  387,    0,    0,  388,
  388,  388,    0,    0,  388,  388,    0,  390,    0,  390,
  390,  390,  390,  390,    0,    0,    0,    0,  388,  390,
  388,    0,  388,    0,  388,    0,    0,  390,  390,  390,
    0,    0,  390,  390,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
  390,    0,  390,
  };
  protected static  short [] yyCheck = {             2,
    3,  270,    0,   72,  468,   63,  598,    3,  478,  268,
    8,  216,  345,  530,  272,    6,  282,  276,  637,  108,
  614,  341,  257,  261,   93,  341,  256,   30,  408,  344,
  256,   93,  496,  338,   30,  305,  248,  352,   41,  108,
  264,  498,  325,  352,  211,    6,  743,    8,  314,  342,
  134,  344,  305,  598,  325,  325,   59,  257,  344,  367,
  298,   64,  256,  344,  422,   56,  530,  612,   64,   72,
  363,  352,  325,  344,  344,  269,  381,  363,  386,  109,
  110,  111,  112,  113,  114,  115,  116,  363,  268,  352,
   93,  344,  363,  363,  354,   56,  276,  573,  344,  352,
  617,  325,  422,  187,  271,  344,  422,  422,  192,  344,
  363,  363,  342,  422,  344,  495,  342,  363,  344,  188,
  189,  359,  360,  383,  363,  337,  188,  189,  508,  509,
  510,  573,  216,  363,  598,  335,  422,  363,  376,  422,
  378,  230,  231,  840,  344,  344,  422,  766,  612,  256,
  744,  422,  422,  617,  748,  344,  422,  749,  340,    2,
  420,  341,  638,  422,  363,  234,  235,  363,  344,  422,
  422,  353,  234,  235,  363,  358,  422,  207,  208,  209,
  440,  250,  340,  422,  262,  188,  189,  363,  250,  631,
  259,    0,  572,  383,  574,  353,  638,  259,  305,    8,
  230,  231,  232,  233,  355,  356,  236,  237,  238,  239,
  240,  241,  242,  243,  244,  245,   59,  247,  476,  262,
  425,  346,  344,  422,  694,  265,  422,  267,  383,  383,
  270,  234,  235,  422,  372,  275,  374,  315,  344,  279,
  697,  363,  383,  344,  344,  837,  422,  250,  288,  279,
  440,  344,  282,  383,  383,  295,  259,  363,  261,  256,
  300,  344,  363,  363,  304,  261,  306,  383,  308,  383,
  363,  353,  315,  313,  344,  342,  316,  344,  318,  344,
  363,  383,  322,  272,  343,  440,  440,  346,  277,  256,
  330,  331,  281,  363,  334,  752,  363,  337,  363,  440,
  422,  268,  759,  760,  342,  345,  344,  296,  305,  276,
  440,  440,  645,  382,  352,  520,  422,  380,  698,  382,
  382,  422,  422,  305,  440,  363,  440,  344,  661,  422,
  368,  664,  370,  790,  323,  340,  394,  342,  440,  422,
  338,  425,  341,  325,  344,  550,  363,  804,  325,  347,
  344,  350,  422,  348,  352,  347,  354,  426,  353,  342,
  355,  878,  344,  363,  426,  341,  345,  344,  343,  363,
  408,  346,  705,  431,  353,  755,  422,  353,  758,  382,
  343,  845,  422,  381,  453,  383,  455,  378,  593,  384,
  353,  453,  345,  455,  397,  343,  465,  730,  731,  468,
   67,  397,  400,  465,  473,  353,  340,  342,  342,  344,
  422,  473,  346,  422,  878,  872,  345,  352,  352,  353,
  272,  490,  420,  426,  353,  277,  346,  496,  490,  281,
  341,  343,  344,  368,  518,  370,  520,  345,  468,  350,
  352,  108,  440,  345,  296,  353,  903,  306,  422,  308,
  453,  363,  455,  341,  313,  343,  344,  345,  346,  347,
  422,  530,  465,  461,  343,  353,  550,  346,  498,  342,
  473,  323,  340,  408,  353,  363,  343,  344,  346,  343,
  344,  861,  862,  340,  817,  515,  516,  490,  342,  346,
  343,  344,  343,  344,  343,  344,  363,  566,  567,  363,
  530,  232,  233,  468,  566,  567,  346,  345,  592,  593,
  363,  342,  363,  353,  363,  353,  183,  346,  185,  236,
  237,  238,  239,  190,  353,  346,  906,  907,  597,  598,
  599,  422,  353,  498,  341,  597,  203,  599,  205,  341,
  340,  348,  342,  612,  211,  340,  348,  340,  617,  342,
  342,  346,  219,  220,  221,  222,  223,  224,  225,  226,
  227,  228,  229,  566,  567,  530,  353,  570,  598,  346,
  573,  640,  468,  642,  358,  352,  340,  340,  640,  246,
  642,  342,  346,  346,  345,  343,  347,  617,  363,  364,
  365,  352,  353,  265,  597,  267,  599,  348,  270,  350,
  240,  241,  498,  275,  271,  353,  352,  279,  468,  366,
  282,  363,  364,  365,  361,  348,  288,  350,  343,  362,
  345,  890,  891,  295,  291,  292,  340,  340,  300,  342,
  302,  384,  304,  598,  530,  638,  347,  640,  498,  642,
  348,  347,  350,  347,  316,  363,  318,  344,  422,  422,
  322,  341,  617,  343,  344,  345,  346,  347,  330,  331,
  422,  335,  334,  353,  346,  337,  422,  697,  422,  341,
  530,  342,  353,  363,  342,  340,  340,  367,  340,  340,
  343,  422,  302,  343,  343,  754,  422,  353,  345,  344,
  357,  347,  754,  340,  361,  353,  386,  468,  343,  353,
  769,  770,  598,  353,  773,  341,  343,  769,  770,  376,
  353,  773,  343,  468,  341,  353,  341,  341,  353,  749,
  341,  617,  752,  346,  346,  358,  341,  498,  346,  759,
  760,  342,  697,  346,  342,  342,  342,  735,  598,  342,
  342,  340,  740,  498,  342,  342,  363,  347,  341,  422,
  422,  754,  346,  342,  346,  346,  342,  617,  340,  530,
  790,  342,  339,  265,  344,  267,  769,  770,  270,  422,
  773,  346,  346,  275,  804,  530,  845,  279,  346,  346,
  282,  353,  780,  781,  749,  358,  288,  752,  343,  343,
  342,  344,  342,  295,  759,  760,  352,  345,  300,  346,
  343,  697,  304,    0,  342,  346,  343,  837,  806,  878,
  346,  478,  358,  353,  316,  845,  318,  343,  816,  342,
  322,  488,  269,  286,  893,  790,  343,  598,  330,  331,
  343,  893,  334,  340,  343,  337,  342,  697,  505,  804,
  507,  340,  872,  598,  345,  343,  617,  343,  878,  294,
  358,  340,  280,  749,  852,  853,  752,  343,  345,  347,
  343,  346,  617,  759,  760,  344,  864,  865,  866,  340,
    0,  346,  837,  903,  343,  341,  346,  422,  350,  347,
  845,  348,  346,  341,  346,  346,  343,  341,  555,  749,
  893,  343,  752,  343,  790,  343,  342,  315,  342,  759,
  760,  343,  345,  341,  262,  341,  347,  872,  804,  348,
  350,  343,  579,  878,  343,  341,  346,  340,  343,  346,
  422,  345,  260,  345,  591,  344,  697,  343,  343,  343,
  790,  261,  340,  600,  340,  340,  343,  604,  903,  343,
  607,  837,  697,  345,  804,  612,  284,  614,  343,  845,
  340,  345,  340,    0,  340,  346,  341,  341,  341,  297,
  341,  286,  422,  301,  302,  340,  346,  346,  298,  307,
  340,  309,  310,  311,  312,  346,  872,  837,  749,  317,
  341,  752,  878,  321,  343,  845,  343,  341,  759,  760,
  262,  315,  343,  662,  749,  333,  451,  752,  336,   54,
  338,  193,  451,  695,  759,  760,    0,  903,  409,    8,
   30,  341,  872,  343,  344,  345,  346,  347,  878,  790,
   64,  205,  180,  353,  206,  355,  356,  694,  440,  359,
  360,  361,  362,  804,  816,  790,  366,  367,  420,  781,
  780,  397,  372,  903,  374,  862,  376,  400,  378,  804,
  380,  864,  382,  852,  384,  853,  386,  372,  461,  292,
  276,  242,  245,  260,  243,  845,  837,  244,  837,  247,
  507,  843,  686,   -1,  845,  272,  743,  744,  845,  496,
  277,  748,  837,   -1,  281,   -1,  791,  284,   -1,   -1,
  845,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,  296,
  297,  872,   -1,   -1,  301,  302,   -1,  878,   -1,   -1,
  307,   -1,  309,  310,  311,  312,   -1,  872,   -1,   -1,
  317,   -1,  789,  878,  321,   -1,  323,  257,  795,   -1,
  260,   -1,  903,   -1,   -1,  265,  333,  267,   -1,  336,
  270,  338,  272,  273,   -1,  275,   -1,  277,  903,  279,
   -1,  281,  282,  283,  284,   -1,   -1,   -1,  288,   -1,
   -1,   -1,   -1,  293,   -1,  295,  296,  297,   -1,   -1,
  300,  301,  302,  840,  304,   -1,   -1,  307,   -1,  309,
  310,  311,  312,   -1,   -1,   -1,  316,  317,  318,   -1,
   -1,  321,  322,  323,   -1,   -1,   -1,   -1,   -1,   -1,
  330,  331,   -1,  333,  334,  335,  336,  337,  338,   -1,
  257,  341,   -1,  260,  344,   -1,  341,   -1,  343,  344,
  345,  346,  347,   -1,  354,  272,   -1,   -1,  353,   -1,
  277,   -1,   -1,   -1,  281,   -1,   -1,  284,  363,   -1,
   -1,   -1,  367,   -1,   -1,   -1,   -1,   -1,   -1,  296,
  297,   -1,   -1,   -1,  301,  302,   -1,   -1,   -1,  384,
  307,  386,  309,  310,  311,  312,  260,   -1,   -1,   -1,
  317,   -1,   -1,   -1,  321,   -1,  323,   -1,  272,   -1,
   -1,   -1,   -1,  277,   -1,   -1,  333,  281,  335,  336,
  284,  338,  422,   -1,  341,   -1,   -1,  344,   -1,   -1,
   -1,   -1,  296,  297,   -1,   -1,   -1,  301,  302,   -1,
   -1,   -1,   -1,  307,   -1,  309,  310,  311,  312,   -1,
   -1,   -1,   -1,  317,   -1,   -1,   -1,  321,   -1,  323,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,  256,   -1,  333,
   -1,   -1,  336,   -1,  338,  264,  265,  266,  267,  268,
   -1,  270,  271,   -1,  273,  274,  275,  276,   -1,  278,
  279,  280,   -1,   -1,  261,   -1,  285,  286,  287,  288,
  289,  290,  291,  292,   -1,   -1,  295,   -1,   -1,   -1,
  299,  300,   -1,  302,  303,  304,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,  314,   -1,  316,   -1,  318,
  319,  298,   -1,  322,   -1,  324,  325,  326,  327,  328,
  329,  330,  331,  332,  333,  334,  335,   -1,  337,   -1,
  339,  340,  341,  342,   -1,   -1,   -1,  346,  341,   -1,
  343,  344,  345,  346,  347,  354,  355,  356,  357,   -1,
  353,   -1,  361,   -1,  363,   -1,  343,  344,   -1,  368,
  363,  370,   -1,   -1,  367,   -1,   -1,   -1,  355,  356,
   -1,   -1,  359,  360,  361,  362,   -1,   -1,   -1,  366,
  367,  384,   -1,  386,   -1,  372,   -1,  374,   -1,  376,
   -1,  378,   -1,  380,   -1,  382,   -1,  384,   -1,  386,
   -1,  410,   -1,  412,   -1,  414,  256,  416,   -1,  418,
   -1,  420,   -1,  422,  264,  265,  266,  267,  268,   -1,
  270,  271,   -1,  273,  274,  275,  276,   -1,  278,  279,
  261,   -1,   -1,   -1,   -1,  285,   -1,  287,  288,  289,
  290,  291,  292,   -1,   -1,  295,   -1,   -1,   -1,  299,
  300,   -1,  302,  303,  304,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,  314,   -1,  316,  298,  318,  319,
   -1,   -1,  322,   -1,  324,  325,  326,  327,  328,  329,
  330,  331,  332,  333,  334,  335,   -1,  337,   -1,  339,
  340,  341,  342,   -1,   -1,   -1,  346,  341,   -1,  343,
  344,  345,  346,  347,  354,  355,  356,  357,   -1,  353,
  341,  361,  343,  363,  345,  346,  347,   -1,  368,  363,
  370,   -1,  353,  367,   -1,   -1,   -1,   -1,  359,  360,
  361,  362,   -1,   -1,   -1,  366,  367,   -1,   -1,   -1,
   -1,   -1,  386,   -1,   -1,  376,   -1,  378,   -1,  380,
   -1,  382,   -1,  384,   -1,  386,   -1,   -1,   -1,   -1,
  410,   -1,  412,   -1,  414,  256,  416,   -1,  418,   -1,
  420,   -1,  422,  264,  265,  266,  267,  268,   -1,  270,
  271,   -1,  273,  274,  275,  276,   -1,  278,  279,  261,
   -1,   -1,   -1,   -1,  285,   -1,  287,  288,  289,  290,
  291,  292,   -1,   -1,  295,   -1,   -1,   -1,  299,  300,
   -1,  302,  303,  304,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,  314,   -1,  316,  298,  318,  319,   -1,
   -1,  322,   -1,  324,  325,  326,  327,  328,  329,  330,
  331,  332,  333,  334,  335,   -1,  337,   -1,  339,  340,
   -1,  342,   -1,   -1,   -1,  346,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,  354,  355,  356,  357,   -1,   -1,  341,
  361,  343,  363,  345,  346,  347,   -1,  368,   -1,  370,
   -1,  353,   -1,   -1,   -1,   -1,   -1,  359,  360,  361,
  362,   -1,   -1,   -1,  366,  367,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,  376,   -1,  378,   -1,  380,   -1,
  382,   -1,  384,   -1,  386,   -1,   -1,   -1,   -1,  410,
   -1,  412,   -1,  414,  256,  416,   -1,  418,   -1,  420,
   -1,  422,  264,  265,  266,  267,  268,   -1,  270,  271,
   -1,  273,  274,  275,  276,   -1,  278,  279,   -1,   -1,
   -1,   -1,   -1,  285,   -1,  287,  288,  289,  290,  291,
  292,   -1,   -1,  295,   -1,   -1,   -1,  299,  300,   -1,
  302,  303,  304,  285,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,  314,   -1,  316,   -1,  318,  319,   -1,   -1,
  322,   -1,  324,  325,  326,  327,  328,  329,  330,  331,
  332,  333,  334,  335,   -1,  337,   -1,  339,  340,   -1,
  342,   -1,   -1,   -1,  346,  327,   -1,   -1,  343,   -1,
   -1,  346,  354,  355,  356,  357,   -1,   -1,  353,  361,
   -1,  363,   -1,  358,   -1,   -1,  368,   -1,  370,   -1,
   -1,   -1,  354,  355,  356,  357,   -1,  359,  360,  361,
  362,  363,  364,  365,  366,   -1,  368,   -1,  370,   -1,
  372,   -1,  374,  388,  376,  390,  378,  392,  380,  394,
  382,  396,   -1,  398,   -1,  400,   -1,  402,  410,  404,
  412,  406,  414,  256,  416,   -1,  418,   -1,  420,   -1,
  422,  264,  265,  266,  267,   -1,   -1,  270,  271,   -1,
  273,  274,  275,   -1,   -1,  278,  279,   -1,   -1,   -1,
   -1,   -1,  285,   -1,  287,  288,  289,  290,  291,  292,
   -1,   -1,  295,   -1,   -1,   -1,  299,  300,   -1,  302,
  303,  304,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,  314,   -1,  316,   -1,  318,  319,   -1,   -1,  322,
   -1,  324,  325,  326,  327,  328,  329,  330,  331,  332,
  333,  334,  335,   -1,  337,  343,  339,  340,  346,  342,
   -1,  358,   -1,  346,   -1,  353,   -1,   -1,   -1,   -1,
  358,  354,  355,  356,  357,   -1,   -1,   -1,  361,   -1,
  363,   -1,   -1,   -1,   -1,  368,   -1,  370,   -1,   -1,
   -1,  388,   -1,  390,   -1,  392,   -1,  394,   -1,  396,
  388,  398,  390,  400,  392,  402,  394,  404,  396,  406,
  398,   -1,  400,   -1,  402,   -1,  404,   -1,  406,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,  410,   -1,  412,
   -1,  414,  256,  416,   -1,  418,   -1,  420,   -1,  422,
  264,  265,  266,  267,   -1,   -1,  270,  271,   -1,   -1,
  274,  275,   -1,   -1,  278,  279,   -1,   -1,   -1,   -1,
   -1,  285,   -1,  287,  288,  289,  290,  291,  292,   -1,
   -1,  295,   -1,   -1,   -1,  299,  300,   -1,  302,  303,
  304,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
  314,   -1,  316,   -1,  318,  319,   -1,   -1,  322,   -1,
  324,  325,  326,  327,  328,  329,  330,  331,  332,  333,
  334,  335,   -1,  337,   -1,  339,  340,   -1,  342,   -1,
  256,   -1,  346,   -1,   -1,  261,   -1,   -1,   -1,   -1,
  354,  355,  356,  357,   -1,   -1,   -1,  361,   -1,  363,
   -1,   -1,   -1,   -1,  368,   -1,  370,   -1,   -1,   -1,
   -1,   -1,  341,   -1,  343,  344,  345,  346,  347,   -1,
   -1,   -1,  298,   -1,  353,   -1,   -1,   -1,   -1,  305,
   -1,   -1,  361,  362,  363,   -1,   -1,  366,  367,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,  410,   -1,  412,  325,
  414,  380,  416,  382,  418,  384,  420,  386,  422,   -1,
   -1,   -1,   -1,   -1,  340,  341,  342,  343,   -1,  345,
  346,  347,   -1,   -1,   -1,   -1,  352,  353,   -1,  355,
  356,   -1,  358,  359,  360,  361,  362,  363,  364,  365,
  366,  367,  368,   -1,  370,   -1,  372,   -1,  374,   -1,
  376,   -1,  378,   -1,  380,   -1,  382,   -1,  384,   -1,
  386,   -1,  388,   -1,  390,   -1,  392,   -1,  394,   -1,
  396,   -1,  398,   -1,  400,  256,  402,   -1,  404,   -1,
  406,   -1,  408,  264,  265,   -1,  267,   -1,   -1,  270,
  271,   -1,   -1,   -1,  275,   -1,  422,   -1,  279,   -1,
   -1,   -1,   -1,   -1,  285,   -1,   -1,  288,   -1,   -1,
   -1,   -1,   -1,   -1,  295,   -1,   -1,   -1,   -1,  300,
   -1,  302,  303,  304,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,  316,   -1,  318,  319,   -1,
   -1,  322,   -1,   -1,  325,   -1,  327,   -1,  329,  330,
  331,  332,   -1,  334,   -1,   -1,  337,   -1,   -1,   -1,
   -1,  342,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,  354,  355,  356,  357,   -1,   -1,   -1,
  361,   -1,  363,   -1,   -1,   -1,   -1,  368,   -1,  370,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,  341,   -1,  343,  344,  345,  346,  347,   -1,
   -1,  256,   -1,   -1,  353,   -1,   -1,   -1,   -1,  264,
  265,   -1,  267,  362,  363,  270,  271,  366,  367,  410,
  275,  412,   -1,  414,  279,  416,   -1,  418,   -1,  420,
  285,  422,   -1,  288,   -1,  384,  256,  386,   -1,   -1,
  295,  261,   -1,   -1,   -1,  300,   -1,  302,  303,  304,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,  316,   -1,  318,  319,   -1,   -1,  322,   -1,   -1,
  325,   -1,  327,   -1,  329,  330,  331,  332,  298,  334,
   -1,   -1,  337,   -1,   -1,  305,  256,  342,   -1,   -1,
   -1,  261,   -1,   -1,   -1,   -1,   -1,   -1,   -1,  354,
  355,  356,  357,   -1,   -1,  325,  361,   -1,  363,   -1,
   -1,   -1,   -1,  368,   -1,  370,   -1,   -1,   -1,   -1,
  340,  341,  342,  343,  344,  345,  346,  347,  298,   -1,
   -1,   -1,   -1,  353,   -1,   -1,   -1,   -1,   -1,  359,
  360,  361,  362,  363,   -1,   -1,  366,  367,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,  410,  376,  412,  378,  414,
  380,  416,  382,  418,  384,  420,  386,  422,   -1,   -1,
  340,  341,  342,  343,  344,  345,  346,  347,   -1,   -1,
   -1,   -1,   -1,  353,   -1,   -1,   -1,   -1,   -1,  359,
  360,  361,  362,   -1,   -1,   -1,  366,  367,   -1,   -1,
   -1,   -1,  422,   -1,   -1,   -1,  376,   -1,  378,   -1,
  380,   -1,  382,  260,  384,  262,  386,   -1,  265,   -1,
  267,   -1,   -1,  270,   -1,  272,  273,   -1,  275,   -1,
  277,   -1,  279,   -1,  281,  282,  283,  284,   -1,   -1,
   -1,  288,   -1,   -1,   -1,   -1,  293,   -1,  295,  296,
  297,   -1,  422,  300,  301,  302,   -1,  304,   -1,  306,
  307,  308,  309,  310,  311,  312,  313,   -1,  315,  316,
  317,  318,   -1,   -1,  321,  322,  323,   -1,   -1,   -1,
   -1,   -1,   -1,  330,  331,   -1,  333,  334,   -1,  336,
  337,  338,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,  348,   -1,  350,   -1,   -1,  260,  354,   -1,   -1,
   -1,  265,   -1,  267,   -1,   -1,  270,   -1,  272,  273,
   -1,  275,   -1,  277,   -1,  279,   -1,  281,  282,  283,
  284,   -1,   -1,   -1,  288,   -1,   -1,   -1,   -1,  293,
   -1,  295,  296,  297,   -1,   -1,  300,   -1,  302,   -1,
  304,   -1,   -1,  307,   -1,  309,  310,  311,  312,   -1,
   -1,   -1,  316,  317,  318,   -1,   -1,  321,  322,  323,
   -1,   -1,   -1,   -1,   -1,  422,  330,  331,   -1,  333,
  334,   -1,  336,  337,  338,   -1,  260,  341,   -1,   -1,
   -1,  265,   -1,  267,   -1,   -1,  270,   -1,  272,  273,
  354,  275,   -1,  277,   -1,  279,   -1,  281,  282,  283,
  284,   -1,   -1,   -1,  288,   -1,   -1,   -1,   -1,  293,
   -1,  295,  296,  297,   -1,   -1,  300,   -1,  302,   -1,
  304,   -1,   -1,  307,   -1,  309,  310,  311,  312,   -1,
   -1,   -1,  316,  317,  318,   -1,   -1,  321,  322,  323,
   -1,   -1,   -1,   -1,   -1,   -1,  330,  331,   -1,  333,
  334,   -1,  336,  337,  338,   -1,  260,  341,  422,   -1,
   -1,  265,   -1,  267,   -1,   -1,  270,   -1,  272,  273,
  354,  275,   -1,  277,   -1,  279,   -1,  281,  282,  283,
  284,   -1,   -1,   -1,  288,   -1,   -1,   -1,   -1,  293,
   -1,  295,  296,  297,   -1,   -1,  300,   -1,  302,   -1,
  304,   -1,   -1,  307,   -1,  309,  310,  311,  312,   -1,
   -1,   -1,  316,  317,  318,   -1,   -1,  321,  322,  323,
   -1,   -1,   -1,   -1,   -1,   -1,  330,  331,   -1,  333,
  334,   -1,  336,  337,  338,   -1,  260,  341,  422,   -1,
  261,  265,   -1,  267,   -1,   -1,  270,   -1,  272,  273,
  354,  275,   -1,  277,   -1,  279,   -1,  281,  282,  283,
  284,   -1,   -1,   -1,  288,   -1,   -1,   -1,   -1,  293,
   -1,  295,  296,  297,   -1,   -1,  300,  298,  302,   -1,
  304,   -1,   -1,  307,   -1,  309,  310,  311,  312,   -1,
   -1,   -1,  316,  317,  318,   -1,   -1,  321,  322,  323,
   -1,   -1,   -1,   -1,   -1,   -1,  330,  331,   -1,  333,
  334,  261,  336,  337,  338,   -1,   -1,  341,  422,  340,
  341,  342,  343,   -1,  345,  346,  347,   -1,   -1,   -1,
  354,  352,  353,   -1,  355,  356,   -1,  358,  359,  360,
  361,  362,  363,  364,  365,  366,  367,  368,  298,  370,
   -1,  372,   -1,  374,   -1,  376,   -1,  378,   -1,  380,
   -1,  382,   -1,  384,   -1,  386,   -1,  388,   -1,  390,
   -1,  392,   -1,  394,   -1,  396,   -1,  398,   -1,  400,
   -1,  402,  261,  404,   -1,  406,   -1,  408,   -1,   -1,
   -1,  341,  342,  343,  344,  345,  346,  347,  422,   -1,
   -1,  422,  352,  353,   -1,  355,  356,   -1,  358,  359,
  360,  361,  362,  363,  364,  365,  366,  367,  368,  298,
  370,   -1,  372,   -1,  374,   -1,  376,   -1,  378,   -1,
  380,   -1,  382,   -1,  384,   -1,  386,   -1,  388,   -1,
  390,   -1,  392,   -1,  394,   -1,  396,   -1,  398,   -1,
  400,   -1,  402,  261,  404,   -1,  406,   -1,  408,   -1,
   -1,   -1,  341,  342,  343,  344,  345,  346,  347,   -1,
   -1,   -1,  422,   -1,  353,   -1,  355,  356,   -1,  358,
  359,  360,  361,  362,  363,  364,  365,  366,  367,  368,
  298,  370,   -1,  372,   -1,  374,   -1,  376,   -1,  378,
   -1,  380,   -1,  382,   -1,  384,   -1,  386,   -1,  388,
   -1,  390,   -1,  392,   -1,  394,   -1,  396,   -1,  398,
   -1,  400,   -1,  402,   -1,  404,   -1,  406,   -1,  408,
   -1,   -1,   -1,   -1,   -1,  343,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,  422,   -1,   -1,   -1,  355,  356,   -1,
  358,  359,  360,  361,  362,   -1,  364,  365,  366,  367,
   -1,   -1,   -1,   -1,  372,   -1,  374,   -1,  376,   -1,
  378,   -1,  380,   -1,  382,   -1,  384,   -1,  386,   -1,
  388,   -1,  390,   -1,  392,   -1,  394,   -1,  396,   -1,
  398,   -1,  400,   -1,  402,   -1,  404,   -1,  406,   -1,
  264,  265,   -1,  267,   -1,   -1,  270,  271,   -1,   -1,
   -1,  275,   -1,   -1,  422,  279,   -1,   -1,   -1,   -1,
   -1,  285,   -1,   -1,  288,   -1,   -1,   -1,   -1,   -1,
   -1,  295,   -1,   -1,   -1,   -1,  300,   -1,  302,  303,
  304,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,  316,   -1,  318,  319,  320,   -1,  322,   -1,
   -1,  325,   -1,  327,   -1,  329,  330,  331,  332,   -1,
  334,   -1,   -1,  337,   -1,   -1,  340,  341,  342,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
  354,  355,  356,  357,   -1,   -1,   -1,  361,   -1,  363,
   -1,   -1,   -1,   -1,  368,   -1,  370,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,  341,
   -1,  343,  344,  345,  346,  347,   -1,   -1,   -1,   -1,
   -1,  353,   -1,   -1,   -1,   -1,  264,  265,   -1,  267,
  362,  363,  270,  271,  366,  367,  410,  275,  412,   -1,
  414,  279,  416,   -1,  418,   -1,  420,  285,  422,   -1,
  288,   -1,  384,   -1,  386,   -1,   -1,  295,   -1,   -1,
   -1,   -1,  300,   -1,  302,  303,  304,   -1,  306,   -1,
   -1,   -1,   -1,   -1,   -1,  313,   -1,   -1,  316,   -1,
  318,  319,   -1,   -1,  322,   -1,   -1,  325,   -1,  327,
   -1,  329,  330,  331,  332,   -1,  334,   -1,   -1,  337,
   -1,   -1,   -1,   -1,  342,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,  354,  355,  356,  357,
   -1,   -1,   -1,  361,   -1,  363,   -1,   -1,   -1,   -1,
  368,   -1,  370,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,  341,   -1,  343,  344,  345,
  346,  347,   -1,   -1,   -1,   -1,   -1,  353,   -1,   -1,
   -1,   -1,  264,  265,   -1,  267,  362,  363,  270,  271,
   -1,  367,  410,  275,  412,   -1,  414,  279,  416,   -1,
  418,   -1,  420,  285,  422,   -1,  288,   -1,  384,   -1,
  386,   -1,   -1,  295,   -1,   -1,   -1,   -1,  300,   -1,
  302,  303,  304,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,  316,   -1,  318,  319,  320,   -1,
  322,   -1,   -1,  325,   -1,  327,   -1,  329,  330,  331,
  332,   -1,  334,   -1,   -1,  337,   -1,   -1,  340,   -1,
  342,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,  354,  355,  356,  357,   -1,   -1,   -1,  361,
   -1,  363,   -1,   -1,   -1,   -1,  368,   -1,  370,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,  341,   -1,  343,  344,  345,  346,  347,   -1,   -1,
   -1,   -1,   -1,  353,   -1,   -1,   -1,   -1,  264,  265,
   -1,  267,  362,  363,  270,  271,   -1,  367,  410,  275,
  412,   -1,  414,  279,  416,   -1,  418,   -1,  420,  285,
  422,   -1,  288,   -1,  384,   -1,  386,   -1,   -1,  295,
   -1,   -1,   -1,   -1,  300,   -1,  302,  303,  304,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
  316,   -1,  318,  319,   -1,   -1,  322,   -1,   -1,  325,
   -1,  327,   -1,  329,  330,  331,  332,   -1,  334,   -1,
   -1,  337,   -1,   -1,   -1,   -1,  342,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,  353,  354,  355,
  356,  357,   -1,   -1,   -1,  361,   -1,  363,   -1,   -1,
   -1,   -1,  368,   -1,  370,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,  264,  265,   -1,  267,   -1,   -1,
  270,  271,   -1,   -1,  410,  275,  412,   -1,  414,  279,
  416,   -1,  418,   -1,  420,  285,  422,   -1,  288,   -1,
   -1,   -1,   -1,   -1,   -1,  295,   -1,   -1,   -1,   -1,
  300,   -1,  302,  303,  304,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,  316,   -1,  318,  319,
   -1,   -1,  322,   -1,   -1,  325,   -1,  327,   -1,  329,
  330,  331,  332,   -1,  334,   -1,   -1,  337,   -1,   -1,
   -1,   -1,  342,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,  354,  355,  356,  357,   -1,   -1,
   -1,  361,   -1,  363,   -1,   -1,   -1,   -1,  368,   -1,
  370,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
  264,  265,   -1,  267,   -1,   -1,  270,  271,   -1,   -1,
  410,  275,  412,   -1,  414,  279,  416,   -1,  418,   -1,
  420,  285,  422,   -1,  288,   -1,   -1,   -1,   -1,   -1,
   -1,  295,   -1,   -1,   -1,   -1,  300,   -1,  302,  303,
  304,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,  316,   -1,  318,  319,   -1,   -1,  322,   -1,
   -1,  325,   -1,  327,   -1,  329,  330,  331,  332,   -1,
  334,   -1,   -1,  337,   -1,   -1,   -1,   -1,  342,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
  354,  355,  356,  357,   -1,   -1,   -1,  361,   -1,  363,
   -1,   -1,   -1,   -1,  368,   -1,  370,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,  264,  265,   -1,  267,
   -1,   -1,  270,  271,   -1,   -1,  410,  275,  412,   -1,
  414,  279,  416,   -1,  418,   -1,  420,  285,  422,   -1,
  288,   -1,   -1,   -1,   -1,   -1,   -1,  295,   -1,   -1,
   -1,   -1,  300,   -1,  302,  303,  304,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,  316,   -1,
  318,  319,   -1,   -1,  322,   -1,   -1,  325,   -1,  327,
   -1,  329,  330,  331,  332,   -1,  334,   -1,   -1,  337,
   -1,   -1,   -1,   -1,  342,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,  354,  355,  356,  357,
   -1,   -1,   -1,  361,   -1,  363,   -1,   -1,   -1,   -1,
  368,   -1,  370,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,  264,  265,   -1,  267,   -1,   -1,  270,  271,
   -1,   -1,  410,  275,  412,   -1,  414,  279,  416,   -1,
  418,   -1,  420,  285,  422,   -1,  288,   -1,   -1,   -1,
   -1,   -1,   -1,  295,   -1,   -1,   -1,   -1,  300,   -1,
  302,  303,  304,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,  316,   -1,  318,  319,   -1,   -1,
  322,   -1,   -1,  325,   -1,  327,   -1,  329,  330,  331,
  332,   -1,  334,   -1,   -1,  337,   -1,   -1,   -1,   -1,
  342,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,  354,  355,  356,  357,   -1,   -1,   -1,  361,
   -1,   -1,   -1,   -1,   -1,   -1,  368,   -1,  370,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,  264,  265,
   -1,  267,   -1,   -1,  270,  271,   -1,   -1,  410,  275,
  412,   -1,  414,  279,  416,   -1,  418,   -1,  420,  285,
  422,   -1,  288,   -1,   -1,   -1,   -1,   -1,   -1,  295,
   -1,   -1,   -1,   -1,  300,   -1,  302,  303,  304,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
  316,   -1,  318,  319,   -1,   -1,  322,   -1,   -1,  325,
   -1,  327,   -1,  329,  330,  331,  332,   -1,  334,   -1,
   -1,  337,   -1,   -1,   -1,   -1,  342,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,  354,   -1,
   -1,  357,   -1,   -1,   -1,  341,   -1,  343,  344,  345,
  346,  347,   -1,  264,  265,   -1,  267,  353,   -1,  270,
  271,   -1,   -1,   -1,  275,  361,  362,  363,  279,   -1,
  366,  367,   -1,   -1,  285,   -1,   -1,  288,   -1,   -1,
   -1,   -1,   -1,   -1,  295,   -1,   -1,   -1,  384,  300,
  386,  302,  303,  304,  410,   -1,  412,   -1,  414,   -1,
  416,   -1,  418,   -1,  420,  316,  422,  318,  319,   -1,
   -1,  322,   -1,   -1,  325,   -1,  327,   -1,  329,  330,
  331,  332,  265,  334,  267,   -1,  337,  270,   -1,  272,
  273,  342,  275,   -1,  277,   -1,  279,   -1,  281,  282,
  283,   -1,   -1,   -1,   -1,  288,   -1,   -1,   -1,   -1,
  293,   -1,  295,  296,   -1,   -1,   -1,  300,   -1,   -1,
   -1,  304,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,  316,   -1,  318,   -1,   -1,   -1,  322,
  323,   -1,   -1,   -1,   -1,   -1,   -1,  330,  331,   -1,
   -1,  334,   -1,   -1,  337,   -1,   -1,   -1,   -1,  410,
   -1,  412,   -1,  414,   -1,  416,   -1,  418,  265,  420,
  267,  422,   -1,  270,   -1,  272,  273,   -1,  275,   -1,
  277,   -1,  279,   -1,  281,  282,  283,   -1,   -1,   -1,
   -1,  288,   -1,   -1,   -1,   -1,  293,   -1,  295,  296,
   -1,   -1,   -1,  300,   -1,   -1,   -1,  304,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,  316,
   -1,  318,   -1,   -1,   -1,  322,  323,   -1,   -1,   -1,
   -1,   -1,   -1,  330,  331,   -1,  265,  334,  267,  422,
  337,  270,   -1,  272,  273,   -1,  275,   -1,  277,   -1,
  279,   -1,  281,  282,  283,   -1,   -1,   -1,   -1,  288,
   -1,   -1,   -1,   -1,  293,   -1,  295,  296,   -1,   -1,
   -1,  300,   -1,   -1,   -1,  304,  265,   -1,  267,   -1,
   -1,  270,   -1,   -1,   -1,   -1,  275,  316,   -1,  318,
  279,   -1,   -1,  322,  323,   -1,   -1,   -1,   -1,  288,
   -1,  330,  331,   -1,   -1,  334,  295,   -1,  337,   -1,
   -1,  300,   -1,   -1,   -1,  304,   -1,  306,   -1,  308,
   -1,   -1,   -1,   -1,  313,  422,  265,  316,  267,  318,
   -1,  270,   -1,  322,   -1,   -1,  275,   -1,   -1,   -1,
  279,  330,  331,  282,   -1,  334,   -1,   -1,  337,  288,
   -1,   -1,   -1,   -1,  343,   -1,  295,   -1,   -1,   -1,
   -1,  300,   -1,  302,   -1,  304,  265,   -1,  267,   -1,
   -1,  270,   -1,   -1,   -1,   -1,  275,  316,   -1,  318,
  279,   -1,   -1,  322,   -1,   -1,   -1,   -1,   -1,  288,
   -1,  330,  331,  422,   -1,  334,  295,   -1,  337,   -1,
   -1,  300,  341,   -1,   -1,  304,   -1,  306,   -1,  308,
   -1,   -1,   -1,   -1,  313,   -1,   -1,  316,   -1,  318,
  265,   -1,  267,  322,   -1,  270,   -1,   -1,   -1,   -1,
  275,  330,  331,  422,  279,  334,   -1,  282,  337,   -1,
   -1,   -1,   -1,  288,   -1,   -1,   -1,   -1,   -1,   -1,
  295,   -1,  265,   -1,  267,  300,   -1,  270,   -1,  304,
   -1,   -1,  275,   -1,   -1,   -1,  279,   -1,   -1,   -1,
   -1,  316,   -1,  318,   -1,  288,   -1,  322,   -1,   -1,
   -1,   -1,  295,  422,   -1,  330,  331,  300,   -1,  334,
   -1,  304,  337,   -1,   -1,   -1,  265,   -1,  267,   -1,
   -1,  270,   -1,  316,   -1,  318,  275,   -1,   -1,  322,
  279,   -1,   -1,   -1,   -1,   -1,   -1,  330,  331,  288,
   -1,  334,   -1,  422,  337,   -1,  295,   -1,  265,   -1,
  267,  300,   -1,  270,   -1,  304,   -1,  261,  275,   -1,
   -1,   -1,  279,   -1,   -1,   -1,   -1,  316,   -1,  318,
   -1,  288,   -1,  322,   -1,   -1,   -1,   -1,  295,   -1,
   -1,  330,  331,  300,   -1,  334,   -1,  304,  337,   -1,
   -1,   -1,   -1,   -1,  298,   -1,   -1,  422,   -1,  316,
   -1,  318,   -1,   -1,   -1,  322,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,  330,  331,   -1,   -1,  334,   -1,   -1,
  337,   -1,   -1,   -1,   -1,   -1,   -1,   -1,  261,  422,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,  341,   -1,  343,
  344,  345,  346,  347,   -1,   -1,   -1,   -1,  352,  353,
   -1,  355,  356,   -1,  358,  359,  360,  361,  362,  363,
  364,  365,  366,  367,  368,  298,  370,   -1,  372,   -1,
  374,   -1,  376,  422,  378,   -1,  380,   -1,  382,   -1,
  384,   -1,  386,   -1,  388,   -1,  390,   -1,  392,   -1,
  394,   -1,  396,   -1,  398,   -1,  400,   -1,  402,  261,
  404,   -1,  406,   -1,  408,  422,   -1,   -1,   -1,  342,
  343,  344,   -1,   -1,   -1,   -1,   -1,   -1,   -1,  352,
  353,   -1,  355,  356,   -1,   -1,  359,  360,  361,  362,
  363,  364,  365,  366,  367,  368,  298,  370,   -1,  372,
   -1,  374,   -1,  376,   -1,  378,   -1,  380,   -1,  382,
   -1,  384,   -1,  386,   -1,  388,   -1,  390,   -1,  392,
   -1,  394,   -1,  396,   -1,  398,   -1,  400,   -1,  402,
  261,  404,   -1,  406,   -1,  408,   -1,   -1,   -1,  341,
   -1,  343,   -1,  345,  346,  347,   -1,   -1,   -1,   -1,
   -1,  353,   -1,  355,  356,   -1,  358,  359,  360,  361,
  362,  363,  364,  365,  366,  367,   -1,  298,   -1,   -1,
  372,   -1,  374,   -1,  376,   -1,  378,   -1,  380,   -1,
  382,  261,  384,   -1,  386,   -1,  388,   -1,  390,   -1,
  392,   -1,  394,   -1,  396,   -1,  398,   -1,  400,   -1,
  402,   -1,  404,   -1,  406,   -1,   -1,   -1,   -1,   -1,
  341,   -1,  343,  344,  345,  346,  347,   -1,  298,   -1,
   -1,   -1,  353,   -1,  355,  356,   -1,   -1,  359,  360,
  361,  362,  363,  364,  365,  366,  367,  261,   -1,   -1,
   -1,  372,   -1,  374,   -1,  376,   -1,  378,   -1,  380,
   -1,  382,   -1,  384,   -1,  386,   -1,   -1,   -1,   -1,
   -1,  341,   -1,  343,  344,  345,  346,  347,   -1,   -1,
   -1,   -1,   -1,  353,  298,  355,  356,   -1,   -1,  359,
  360,  361,  362,   -1,   -1,   -1,  366,  367,  261,   -1,
   -1,   -1,  372,   -1,  374,   -1,  376,   -1,  378,   -1,
  380,   -1,  382,   -1,  384,   -1,  386,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,  341,   -1,  343,
  344,  345,  346,  347,   -1,  298,   -1,   -1,   -1,  353,
   -1,  355,  356,   -1,   -1,  359,  360,  361,  362,  261,
   -1,   -1,  366,  367,   -1,   -1,   -1,   -1,  372,   -1,
  374,   -1,  376,   -1,  378,   -1,  380,   -1,  382,   -1,
  384,   -1,  386,   -1,   -1,   -1,   -1,   -1,  341,   -1,
  343,  344,  345,  346,  347,   -1,  298,   -1,   -1,   -1,
  353,   -1,   -1,  261,   -1,   -1,  359,  360,  361,  362,
  363,   -1,   -1,  366,  367,   -1,   -1,   -1,   -1,  372,
   -1,  374,   -1,  376,   -1,  378,   -1,  380,   -1,  382,
   -1,  384,   -1,  386,   -1,   -1,   -1,   -1,   -1,  341,
  298,  343,  344,  345,  346,  347,   -1,   -1,   -1,   -1,
   -1,  353,   -1,  261,   -1,   -1,   -1,  359,  360,  361,
  362,  363,   -1,   -1,  366,  367,   -1,   -1,   -1,   -1,
  372,   -1,  374,   -1,  376,   -1,  378,   -1,  380,   -1,
  382,   -1,  384,  341,  386,  343,  344,  345,  346,  347,
  298,   -1,   -1,   -1,   -1,  353,   -1,  261,   -1,   -1,
   -1,  359,  360,  361,  362,  363,   -1,   -1,  366,  367,
   -1,   -1,   -1,   -1,  372,   -1,  374,   -1,  376,   -1,
  378,   -1,  380,   -1,  382,  261,  384,   -1,  386,   -1,
   -1,   -1,   -1,  341,  298,  343,  344,  345,  346,  347,
   -1,   -1,   -1,   -1,   -1,  353,   -1,   -1,   -1,   -1,
   -1,  359,  360,  361,  362,  363,   -1,   -1,  366,  367,
   -1,   -1,  298,   -1,   -1,   -1,   -1,   -1,  376,  261,
  378,   -1,  380,   -1,  382,   -1,  384,  341,  386,  343,
  344,  345,  346,  347,   -1,   -1,   -1,   -1,   -1,  353,
   -1,   -1,   -1,   -1,   -1,  359,  360,  361,  362,  363,
   -1,   -1,  366,  367,   -1,  341,  298,  343,  344,  345,
  346,  347,  376,   -1,  378,   -1,  380,  353,  382,  261,
  384,   -1,  386,  359,  360,  361,  362,  363,   -1,   -1,
  366,  367,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
  376,   -1,  378,   -1,  380,   -1,  382,   -1,  384,  341,
  386,  343,  344,  345,  346,  347,  298,   -1,   -1,   -1,
   -1,  353,   -1,   -1,   -1,   -1,   -1,  359,  360,  361,
  362,  363,   -1,   -1,  366,  367,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,  376,   -1,  378,   -1,  380,   -1,
  382,   -1,  384,   -1,  386,   -1,   -1,   -1,   -1,  341,
   -1,  343,  344,  345,  346,  347,  260,   -1,   -1,   -1,
   -1,  353,   -1,   -1,   -1,   -1,   -1,  359,  360,  361,
  362,  363,   -1,   -1,  366,  367,   -1,   -1,   -1,   -1,
  284,   -1,   -1,   -1,  376,   -1,  378,   -1,  380,   -1,
  382,   -1,  384,  297,  386,   -1,   -1,   -1,  302,   -1,
   -1,   -1,  260,  307,   -1,  309,  310,  311,  312,   -1,
   -1,   -1,   -1,  317,  272,   -1,   -1,  321,   -1,  277,
   -1,   -1,   -1,  281,   -1,   -1,  284,   -1,   -1,  333,
   -1,   -1,  336,   -1,  338,   -1,   -1,   -1,  296,  297,
   -1,   -1,   -1,  301,  302,   -1,   -1,   -1,   -1,  307,
  354,  309,  310,  311,  312,  260,   -1,   -1,   -1,  317,
   -1,   -1,   -1,  321,   -1,  323,   -1,  272,   -1,   -1,
   -1,   -1,  277,   -1,   -1,  333,  281,   -1,  336,  284,
  338,   -1,   -1,  341,   -1,   -1,  344,   -1,   -1,   -1,
   -1,  296,  297,   -1,   -1,   -1,  301,  302,   -1,   -1,
   -1,   -1,  307,   -1,  309,  310,  311,  312,  260,   -1,
   -1,   -1,  317,   -1,   -1,   -1,  321,   -1,  323,   -1,
  272,   -1,   -1,   -1,   -1,  277,   -1,   -1,  333,  281,
   -1,  336,  284,  338,   -1,   -1,  341,   -1,   -1,  344,
   -1,   -1,   -1,   -1,  296,  297,   -1,   -1,   -1,  301,
  302,  260,   -1,   -1,   -1,  307,   -1,  309,  310,  311,
  312,   -1,   -1,  272,   -1,  317,   -1,   -1,  277,  321,
   -1,  323,  281,   -1,   -1,  284,   -1,   -1,   -1,   -1,
   -1,  333,   -1,   -1,  336,   -1,  338,  296,  297,  341,
   -1,   -1,  301,  302,   -1,  342,  343,  344,  307,  346,
  309,  310,  311,  312,   -1,  352,  353,   -1,  317,   -1,
   -1,  358,  321,   -1,  323,   -1,  363,   -1,   -1,   -1,
   -1,  368,   -1,  370,  333,   -1,   -1,  336,   -1,  338,
   -1,   -1,  341,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,  388,   -1,  390,   -1,  392,   -1,  394,   -1,  396,
   -1,  398,   -1,  400,   -1,  402,   -1,  404,   -1,  406,
   -1,  408,  342,  343,  344,   -1,  346,   -1,   -1,   -1,
   -1,   -1,  352,  353,   -1,  422,   -1,   -1,  358,   -1,
   -1,   -1,   -1,  363,   -1,  342,  343,  344,  368,  346,
  370,   -1,   -1,   -1,   -1,  352,  353,   -1,   -1,   -1,
   -1,  358,   -1,   -1,   -1,   -1,  363,   -1,  388,   -1,
  390,  368,  392,  370,  394,   -1,  396,   -1,  398,   -1,
  400,   -1,  402,   -1,  404,   -1,  406,   -1,  408,   -1,
   -1,  388,   -1,  390,   -1,  392,   -1,  394,   -1,  396,
   -1,  398,  422,  400,   -1,  402,   -1,  404,   -1,  406,
   -1,  408,  342,  343,  344,   -1,  346,   -1,   -1,   -1,
   -1,   -1,  352,  353,   -1,  422,   -1,  342,  358,  344,
   -1,   -1,   -1,  363,   -1,   -1,   -1,  352,  368,   -1,
  370,   -1,   -1,  358,   -1,   -1,   -1,   -1,  363,   -1,
   -1,   -1,   -1,  368,   -1,  370,   -1,   -1,  388,   -1,
  390,   -1,  392,   -1,  394,   -1,  396,   -1,  398,   -1,
  400,  260,  402,  388,  404,  390,  406,  392,  408,  394,
   -1,  396,   -1,  398,   -1,  400,   -1,  402,   -1,  404,
   -1,  406,  422,  408,   -1,  284,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,  422,  297,   -1,
   -1,   -1,   -1,  302,   -1,   -1,   -1,   -1,  307,   -1,
  309,  310,  311,  312,   -1,   -1,   -1,   -1,  317,   -1,
   -1,  341,  321,  343,  344,  345,  346,  347,   -1,   -1,
   -1,   -1,   -1,  353,  333,   -1,   -1,  336,   -1,  338,
   -1,  361,  362,  363,   -1,   -1,  366,  367,   -1,  341,
   -1,  343,  344,  345,  346,  347,   -1,   -1,   -1,   -1,
  380,  353,  382,   -1,  384,   -1,  386,   -1,   -1,  361,
  362,  363,   -1,   -1,  366,  367,   -1,  341,   -1,  343,
  344,  345,  346,  347,   -1,   -1,   -1,   -1,  380,  353,
  382,   -1,  384,   -1,  386,   -1,   -1,  361,  362,  363,
   -1,   -1,  366,  367,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
  384,   -1,  386,
  };

#line 1849 "cs-parser.jay"

void output (string s)
{
	Console.WriteLine (s);
}

void note (string s)
{
	// Used to put annotations
}

Tokenizer lexer;

public Tokenizer Lexer {
	get {
		return lexer;
	}
}		   

public CSharpParser (StreamReader reader, string name, ArrayList defines, SourceDB sdb, int FileID)
{
	fileID = FileID;
	lexer = new Tokenizer (reader, name, defines);
	db = sdb;

	Namespaces = new Stack ();
	Namespace = "";

	Types = new Stack ();
	type = "";

	Rows = new Stack ();

	modStatic = false;
}

public override void parse ()
{
	try {
		if (yacc_verbose_flag)
			yyparse (lexer, new yydebug.yyDebugSimple ());
		else
			yyparse (lexer);
		Tokenizer tokenizer = lexer as Tokenizer;
		tokenizer.cleanup ();		
	} catch (Exception e){
		// Please do not remove this, it is used during debugging
		// of the grammar
		//
		//Report.Error (-25, lexer.Location, ": Parsing error ");
		Console.WriteLine (e);
	}
}

/* end end end */
}
#line 3172 "-"
namespace yydebug {
        using System;
	 public interface yyDebug {
		 void push (int state, Object value);
		 void lex (int state, int token, string name, Object value);
		 void shift (int from, int to, int errorFlag);
		 void pop (int state);
		 void discard (int state, int token, string name, Object value);
		 void reduce (int from, int to, int rule, string text, int len);
		 void shift (int from, int to);
		 void accept (Object value);
		 void error (string message);
		 void reject ();
	 }
	 
	 class yyDebugSimple : yyDebug {
		 void println (string s){
			 Console.WriteLine (s);
		 }
		 
		 public void push (int state, Object value) {
			 println ("push\tstate "+state+"\tvalue "+value);
		 }
		 
		 public void lex (int state, int token, string name, Object value) {
			 println("lex\tstate "+state+"\treading "+name+"\tvalue "+value);
		 }
		 
		 public void shift (int from, int to, int errorFlag) {
			 switch (errorFlag) {
			 default:				// normally
				 println("shift\tfrom state "+from+" to "+to);
				 break;
			 case 0: case 1: case 2:		// in error recovery
				 println("shift\tfrom state "+from+" to "+to
					     +"\t"+errorFlag+" left to recover");
				 break;
			 case 3:				// normally
				 println("shift\tfrom state "+from+" to "+to+"\ton error");
				 break;
			 }
		 }
		 
		 public void pop (int state) {
			 println("pop\tstate "+state+"\ton error");
		 }
		 
		 public void discard (int state, int token, string name, Object value) {
			 println("discard\tstate "+state+"\ttoken "+name+"\tvalue "+value);
		 }
		 
		 public void reduce (int from, int to, int rule, string text, int len) {
			 println("reduce\tstate "+from+"\tuncover "+to
				     +"\trule ("+rule+") "+text);
		 }
		 
		 public void shift (int from, int to) {
			 println("goto\tfrom state "+from+" to "+to);
		 }
		 
		 public void accept (Object value) {
			 println("accept\tvalue "+value);
		 }
		 
		 public void error (string message) {
			 println("error\t"+message);
		 }
		 
		 public void reject () {
			 println("reject");
		 }
		 
	 }
}
// %token constants
 class Token {
  public const int EOF = 257;
  public const int NONE = 258;
  public const int ERROR = 259;
  public const int ABSTRACT = 260;
  public const int AS = 261;
  public const int ADD = 262;
  public const int ASSEMBLY = 263;
  public const int BASE = 264;
  public const int BOOL = 265;
  public const int BREAK = 266;
  public const int BYTE = 267;
  public const int CASE = 268;
  public const int CATCH = 269;
  public const int CHAR = 270;
  public const int CHECKED = 271;
  public const int CLASS = 272;
  public const int CONST = 273;
  public const int CONTINUE = 274;
  public const int DECIMAL = 275;
  public const int DEFAULT = 276;
  public const int DELEGATE = 277;
  public const int DO = 278;
  public const int DOUBLE = 279;
  public const int ELSE = 280;
  public const int ENUM = 281;
  public const int EVENT = 282;
  public const int EXPLICIT = 283;
  public const int EXTERN = 284;
  public const int FALSE = 285;
  public const int FINALLY = 286;
  public const int FIXED = 287;
  public const int FLOAT = 288;
  public const int FOR = 289;
  public const int FOREACH = 290;
  public const int GOTO = 291;
  public const int IF = 292;
  public const int IMPLICIT = 293;
  public const int IN = 294;
  public const int INT = 295;
  public const int INTERFACE = 296;
  public const int INTERNAL = 297;
  public const int IS = 298;
  public const int LOCK = 299;
  public const int LONG = 300;
  public const int NAMESPACE = 301;
  public const int NEW = 302;
  public const int NULL = 303;
  public const int OBJECT = 304;
  public const int OPERATOR = 305;
  public const int OUT = 306;
  public const int OVERRIDE = 307;
  public const int PARAMS = 308;
  public const int PRIVATE = 309;
  public const int PROTECTED = 310;
  public const int PUBLIC = 311;
  public const int READONLY = 312;
  public const int REF = 313;
  public const int RETURN = 314;
  public const int REMOVE = 315;
  public const int SBYTE = 316;
  public const int SEALED = 317;
  public const int SHORT = 318;
  public const int SIZEOF = 319;
  public const int STACKALLOC = 320;
  public const int STATIC = 321;
  public const int STRING = 322;
  public const int STRUCT = 323;
  public const int SWITCH = 324;
  public const int THIS = 325;
  public const int THROW = 326;
  public const int TRUE = 327;
  public const int TRY = 328;
  public const int TYPEOF = 329;
  public const int UINT = 330;
  public const int ULONG = 331;
  public const int UNCHECKED = 332;
  public const int UNSAFE = 333;
  public const int USHORT = 334;
  public const int USING = 335;
  public const int VIRTUAL = 336;
  public const int VOID = 337;
  public const int VOLATILE = 338;
  public const int WHILE = 339;
  public const int OPEN_BRACE = 340;
  public const int CLOSE_BRACE = 341;
  public const int OPEN_PARENS = 342;
  public const int CLOSE_PARENS = 343;
  public const int OPEN_BRACKET = 344;
  public const int CLOSE_BRACKET = 345;
  public const int SEMICOLON = 346;
  public const int COLON = 347;
  public const int GET = 348;
  public const int get = 349;
  public const int SET = 350;
  public const int set = 351;
  public const int DOT = 352;
  public const int COMMA = 353;
  public const int TILDE = 354;
  public const int PLUS = 355;
  public const int MINUS = 356;
  public const int BANG = 357;
  public const int ASSIGN = 358;
  public const int OP_LT = 359;
  public const int OP_GT = 360;
  public const int BITWISE_AND = 361;
  public const int BITWISE_OR = 362;
  public const int STAR = 363;
  public const int PERCENT = 364;
  public const int DIV = 365;
  public const int CARRET = 366;
  public const int INTERR = 367;
  public const int OP_INC = 368;
  public const int OP_DEC = 370;
  public const int OP_SHIFT_LEFT = 372;
  public const int OP_SHIFT_RIGHT = 374;
  public const int OP_LE = 376;
  public const int OP_GE = 378;
  public const int OP_EQ = 380;
  public const int OP_NE = 382;
  public const int OP_AND = 384;
  public const int OP_OR = 386;
  public const int OP_MULT_ASSIGN = 388;
  public const int OP_DIV_ASSIGN = 390;
  public const int OP_MOD_ASSIGN = 392;
  public const int OP_ADD_ASSIGN = 394;
  public const int OP_SUB_ASSIGN = 396;
  public const int OP_SHIFT_LEFT_ASSIGN = 398;
  public const int OP_SHIFT_RIGHT_ASSIGN = 400;
  public const int OP_AND_ASSIGN = 402;
  public const int OP_XOR_ASSIGN = 404;
  public const int OP_OR_ASSIGN = 406;
  public const int OP_PTR = 408;
  public const int LITERAL_INTEGER = 410;
  public const int LITERAL_FLOAT = 412;
  public const int LITERAL_DOUBLE = 414;
  public const int LITERAL_DECIMAL = 416;
  public const int LITERAL_CHARACTER = 418;
  public const int LITERAL_STRING = 420;
  public const int IDENTIFIER = 422;
  public const int LOWPREC = 423;
  public const int UMINUS = 424;
  public const int HIGHPREC = 425;
  public const int yyErrorCode = 256;
 }
 namespace yyParser {
  using System;
  /** thrown for irrecoverable syntax errors and stack overflow.
    */
  public class yyException : System.Exception {
    public yyException (string message) : base (message) {
    }
  }

  /** must be implemented by a scanner object to supply input to the parser.
    */
  public interface yyInput {
    /** move on to next token.
        @return false if positioned beyond tokens.
        @throws IOException on input error.
      */
    bool advance (); // throws java.io.IOException;
    /** classifies current token.
        Should not be called if advance() returned false.
        @return current %token or single character.
      */
    int token ();
    /** associated with current token.
        Should not be called if advance() returned false.
        @return value for token().
      */
    Object value ();
  }
 }
} // close outermost namespace, that MUST HAVE BEEN opened in the prolog
