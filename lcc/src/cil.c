//
// cil.c: Common Intermediate Language backend for lcc
//
// Authors:
//    Miguel de Icaza
//

#include "c.h"

static void address (Symbol q, Symbol p, long n)
{
	q->x.name = stringf ("&%s%s%D", p->x.name, n > 0 ? "+" : "", n);
}

void cil_blockbeg (Env *e)
{
	printf ("begin block");
}

void cil_blockend (Env *e)
{
	printf ("end block");
}

static void defaddress(Symbol p)
{
        print("default address %s\n", p->x.name);
}

static void defconst (int suffix, int size, Value v)
{
	switch (suffix) {
	case I:
		if (size > sizeof (int))
			print("byte %d %D\n", size, v.i);
		else
			print("byte %d %d\n", size, v.i);
		return;
	case U:
		if (size > sizeof (unsigned))
			print("byte %d %U\n", size, v.u);
		else
			print("byte %d %u\n", size, v.u);
		return;
	case P: print("byte %d %U\n", size, (unsigned long)v.p); return;
	case F:
		if (size == 4) {
			float f = v.d;
			print("byte 4 %u\n", *(unsigned *)&f);
		} else {
			unsigned *p = (unsigned *)&v.d;
			print("byte 4 %u\n", p[swap]);
			print("byte 4 %u\n", p[1 - swap]);
		}
		return;
	}
	assert(0);
}

static void defstring (int len, char *str)
{
	char *s;

	for (s = str; s < str + len; s++)
		print("byte 1 %d\n", (*s)&0377);
}

static void defsymbol (Symbol p)
{
	if (p->scope == CONSTANTS)
		switch (optype(ttob(p->type))) {
		case I: p->x.name = stringf("%D", p->u.c.v.i); break;
		case U: p->x.name = stringf("%U", p->u.c.v.u); break;
		case P: p->x.name = stringf("%U", p->u.c.v.p); break;
		default: assert(0);
		}
	else if (p->scope >= LOCAL && p->sclass == STATIC)
		p->x.name = stringf("$%d", genlabel(1));
	else if (p->scope == LABELS || p->generated)
		p->x.name = stringf("$%s", p->name);
	else
		p->x.name = p->name;
}

static int level;

static void prlevel ()
{
	int i;
	
	print ("\t");
	
	for (i = 0; i < level; i++)
		print ("\t");
}

static void cil_dumptree(Node p) {
	level++;
	switch (specific(p->op)) {
	case ASGN+B:
		assert(p->kids[0]);
		assert(p->kids[1]);
		assert(p->syms[0]);
		cil_dumptree(p->kids[0]);
		cil_dumptree(p->kids[1]);
		prlevel ();
		print("%s %d\n", opname(p->op), p->syms[0]->u.c.v.u);
		level--;
		return;
	case RET+V:
		assert(!p->kids[0]);
		assert(!p->kids[1]);
		prlevel ();
		print("%s\n", opname(p->op));
		level--;
		return;
	}
	switch (generic(p->op)) {
	case CNST: case ADDRG: case ADDRF: case ADDRL: case LABEL:
		assert(!p->kids[0]);
		assert(!p->kids[1]);
		assert(p->syms[0] && p->syms[0]->x.name);
		prlevel ();
		print("%s %s\n", opname(p->op), p->syms[0]->x.name);
		level--;
		return;
	case CVF: case CVI: case CVP: case CVU:
		assert(p->kids[0]);
		assert(!p->kids[1]);
		assert(p->syms[0]);
		cil_dumptree(p->kids[0]);
		prlevel ();
		print("%s %d\n", opname(p->op), p->syms[0]->u.c.v.i);
		level--;
		return;
	case ARG: case BCOM: case NEG: case INDIR: case JUMP: case RET:
		assert(p->kids[0]);
		assert(!p->kids[1]);
		cil_dumptree(p->kids[0]);
		prlevel ();
		print("%s\n", opname(p->op));
		level--;
		return;
	case CALL:
		assert(p->kids[0]);
		assert(!p->kids[1]);
		assert(optype(p->op) != B);
		cil_dumptree(p->kids[0]);
		prlevel ();
		print("%s\n", opname(p->op));
		level--;
		return;
	case ASGN: case BOR: case BAND: case BXOR: case RSH: case LSH:
	case ADD: case SUB: case DIV: case MUL: case MOD:
		assert(p->kids[0]);
		assert(p->kids[1]);
		cil_dumptree(p->kids[0]);
		cil_dumptree(p->kids[1]);
		prlevel ();
		print("%s\n", opname(p->op));
		level--;
		return;
	case EQ: case NE: case GT: case GE: case LE: case LT:
		assert(p->kids[0]);
		assert(p->kids[1]);
		assert(p->syms[0]);
		assert(p->syms[0]->x.name);
		cil_dumptree(p->kids[0]);
		cil_dumptree(p->kids[1]);
		prlevel ();
		print("%s %s\n", opname(p->op), p->syms[0]->x.name);
		level--;
		return;
	}
	assert(0);
}

void cil_emit (Node p)
{
	for (; p; p = p->link)
		cil_dumptree (p);
}

static void global (Symbol p)
{
	print ("GLOBAL: %s", p->x.name);
	print ("\talign %d\n", p->type->align > 4 ? 4 : p->type->align);
	print ("\tLABELV %s\n", p->x.name);
}

static void import (Symbol p)
{
	print("import %s\n", p->x.name);
}

static void local (Symbol p)
{
	offset = roundup(offset, p->type->align);
	p->x.name = stringf("%d", offset);
	p->x.offset = offset;
	offset += p->type->size;
}

static void progbeg (int argc, char *argv[])
{
	print ("startup");
}

static void progend (void)
{
	print ("shutdown");
}

static void space (int n)
{
	print("skip %d\n", n);
}

static void export (Symbol p)
{
	print("export %s\n", p->x.name);
}

static void function (Symbol f, Symbol caller[], Symbol callee[], int ncalls)
{
	int i;

	(*IR->segment)(CODE);
	offset = 0;
	for (i = 0; caller[i] && callee[i]; i++) {
		offset = roundup(offset, caller[i]->type->align);
		caller[i]->x.name = callee[i]->x.name = stringf("%d", offset);
		caller[i]->x.offset = callee[i]->x.offset = offset;
		offset += caller[i]->type->size;
	}
	maxargoffset = maxoffset = argoffset = offset = 0;
	gencode(caller, callee);
	print("proc %s %d %d\n", f->x.name, maxoffset, maxargoffset);
	emitcode();
	print("endproc %s %d %d\n", f->x.name, maxoffset, maxargoffset);

}

static void gen02(Node p) {
	assert(p);
	if (generic(p->op) == ARG) {
		assert(p->syms[0]);
		argoffset += (p->syms[0]->u.c.v.i < 4 ? 4 : p->syms[0]->u.c.v.i);
	} else if (generic(p->op) == CALL) {
		maxargoffset = (argoffset > maxargoffset ? argoffset : maxargoffset);
		argoffset = 0;
	}
}

static void gen01(Node p) {
	if (p) {
		gen01(p->kids[0]);
		gen01(p->kids[1]);
		gen02(p);
	}
}

static Node gen (Node p)
{
	Node q;

	assert(p);
	for (q = p; q; q = q->link)
		gen01(q);
	return p;
}

static void segment (int n)
{
	static int cseg;

	if (cseg != n)
		switch (cseg = n) {
		case CODE: print("code\n"); return;
		case DATA: print("data\n"); return;
		case BSS:  print("bss\n");  return;
		case LIT:  print("lit\n");  return;
		default: assert(0);
	}
}


Interface cilIR = {
	1, 1, 0,	/* char */
	2, 2, 0,	/* short */
	4, 4, 0,	/* int */
	4, 4, 0,	/* long */
	4, 4, 0,	/* long long */
	4, 4, 1,	/* float */
	8, 8, 1,	/* double */
	8, 8, 1,	/* long double */
	8, 8, 0,	/* T* */
	0, 4, 0,	/* struct */
	1,		/* little_endian */
	0,		/* mulops_calls */
	0,		/* wants_callb */
	0,		/* wants_argb */
	1,		/* left_to_right */
	0,		/* wants_dag */
	0,		/* unsigned_char */
	address,
	cil_blockbeg,
	cil_blockend,
	defaddress,
	defconst,
	defstring,
	defsymbol,
	cil_emit,
	export,
	function,
	gen,
	global,
	import,
	local,
	progbeg,
	progend,
	segment,
	space,
	0,		/* I(stabblock) */
	0,		/* I(stabend) */
	0,		/* I(stabfend) */
	0,		/* I(stabinit) */
	0,              /* I(stabline) */
	0,		/* I(stabsym) */
	0,		/* I(stabtype) */
};

