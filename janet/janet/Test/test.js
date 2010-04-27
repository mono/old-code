// HACK snewman 10/17/01: add tests for the basic Object functionality
// (the Object prototype and constructor properties).


var failureCount = 0; // Number of tests which have failed.
var verbose = false; // If this is true, then we report successes as well as
					 // failures.

TestBasicOps();
if (verbose) writeln();
TestFunctions();
if (verbose) writeln();
TestObjects();
if (verbose) writeln();
TestScopes();
if (verbose) writeln();
TestPrototypes();
if (verbose) writeln();
TestExceptions();
if (verbose) writeln();
TestArrays();
if (verbose) writeln();
TestArrayMethods();
if (verbose) writeln();
TestGlobals();
if (verbose) writeln();
verbose = true;
TestMath();
if (verbose) writeln();
TestStringMethods();

writeln();
if (failureCount > 0)
	writeln("FAILURE: failureCount = " + failureCount);
else
	writeln("SUCCESS: failureCount = " + failureCount);


// Output success/failure for a test.  The test succeeds if computed==expected.
// Label is a human-readable label for this test.
function Test(label, computed, expected)
	{
	if (computed == expected)
		{
		if (verbose)
			writeln(label + ": " + computed);
		}
	else
		{
		writeln( "***MISMATCH for " + label + ": computed " + computed +
		         ", expected " + expected );
		failureCount++;
		}
	
	} // Test


// Like Test, but we allow the computed and expected values to differ by
// up to 10^-8.
function TestApprox(label, computed, expected)
	{
	var epsilon = 0.00000001;
	if ( computed == expected ||
		 (computed+epsilon >= expected && computed-epsilon <= expected) )
		{
		if (verbose)
			writeln(label + ": " + computed);
		}
	else
		{
		writeln( "***MISMATCH for " + label + ": computed " + computed +
		         ", expected " + expected );
		failureCount++;
		}
	
	} // TestApprox



function TestBasicOps()
	{
	writeln("TestBasicOps");
	
	Test("10+3", 10+3, 13);
	Test("10-3", 10-3, 7);
	Test("10*3", 10*3, 30);
	// Test("10/3", 10/3, 3.33333333333);
	Test("10%3", 10%3, 1);
	Test("10<<3", 10<<3, 80);
	Test("10>>3", 10>>3, 1);
	Test("10>>>3", 10>>>3, 1);
	Test("10&3", 10&3, 2);
	Test("10|3", 10|3, 11);
	Test("10^3", 10^3, 9);
	Test("10<3", 10<3, false);
	Test("10>3", 10>3, true);
	Test("10<=3", 10<=3, false);
	Test("10>=3", 10>=3, true);
	Test("10==3", 10==3, false);
	Test("10!=3", 10!=3, true);
	Test("2+3*5", 2+3*5, 17);
	Test("2*3+5", 2*3+5, 11);
	Test("(2+3)*5", (2+3)*5, 25);
	Test("2*(3+5)", 2*(3+5), 16);
	
	Test("typeof(Boolean(1))", typeof(Boolean(1)), "boolean");
	Test("typeof(new Boolean(1))", typeof(new Boolean(1)), "object");
	Test("Boolean(5).toString()", Boolean(5).toString(), "true");
	Test("Boolean(0).toString()", Boolean(0).toString(), "false");
	Test("(new Boolean(5)).toString()", (new Boolean(5)).toString(), "true");
	Test("(new Boolean(0)).toString()", (new Boolean(0)).toString(), "false");
	
	Test("typeof(Number(1))", typeof(Number(1)), "number");
	Test("typeof(new Number(1))", typeof(new Number(1)), "object");
	
	// Test("10&&3", 10&&3, 3);
	// Test("10||3", 10||3, 10);
	} // TestBasicOps


function TestFunctions()
	{
	writeln("TestFunctions");
	
	Test("Fib(1)", Fib(1), 1);
	Test("Fib(2)", Fib(2), 1);
	Test("Fib(3)", Fib(3), 2);
	Test("Fib(4)", Fib(4), 3);
	Test("Fib(5)", Fib(5), 5);
	Test("Fib(6)", Fib(6), 8);
	
	var a=10, b=20, c=30, d=40, e=50, f=60, g=70;
	for (var i=0; i<10; i++)
		{
		a++;
		if (i == 7)
			break;
		
		b++;
		
		if (i > 3)
			continue;
		
		c++;
		
		for (var j=0; j<10; j++)
			{
			d++;
			
			if (j < 4)
				continue;
			
			e++;
			
			if (j == 6)
				break;
			
			f++;
			}
		
		g++;
		}
	
	Test("a", a, 18);
	Test("b", b, 27);
	Test("c", c, 34);
	Test("d", d, 68);
	Test("e", e, 62);
	Test("f", f, 68);
	Test("g", g, 74);
	} // TestFunctions


function Fib(n)
	{
	if (n <= 2)
		return 1;
	
	return Fib(n-1) + Fib(n-2);
	} // Fib


// Test simple object creation and access
function TestObjects()
	{
	writeln("TestObjects");
	
	Test("new Point(10,20).x", new Point(10,20).x, 10);
	Test("new Point(11,21).y", new Point(11,21).y, 21);
	Test("new Point(12,22).Sum()", new Point(12,22).Sum(), 34);
	Test("new Point(13,23).WithBasedSum()", new Point(13,23).Sum(), 36);
	
	var translator = { one: "Uno", two: "Dos", three: "Tres",
					   four: "Quatro", five: "Cinco" };
	Test("translator.one", translator.one, "Uno");
	Test("translator['two']", translator['two'], "Dos");
	Test("translator['five']", translator['five'], "Cinco");
	
	Test("typeof null", typeof null, "object");
	Test("typeof true", typeof true, "boolean");
	Test("typeof -13.5", typeof -13.5, "number");
	Test("typeof translator.four", typeof translator.four, "string");
	Test("typeof translator", typeof translator, "object");
	Test("typeof translator.abc", typeof translator.abc, "undefined");
	Test("typeof (new Point(1,1).Sum)", typeof (new Point(1,1).Sum), "function");
	} // TestObjects


function Point(x, y)
	{
	this.x = x;
	this.y = y;
	this.Sum = SumPoint;
	this.WithBasedSum = WithBasedSumPoint;
	} // Point constructor


function SumPoint()
	{
	return this.x + this.y;
	} // SumPoint


function WithBasedSumPoint()
	{
	with (this)
		return x+y;
	} // WithBasedSumPoint


function TestArrays()
	{
	writeln("TestArrays");
	
	var array = [100, 50, 37, 200, 19, 63];
	
	Test("array[0]", array[0], 100);
	Test("array[1]", array[1], 50);
	Test("array[5]", array[5], 63);
	
	BubbleSort(array, 6);
	
	Test("sorted array[0]", array[0], 19);
	Test("sorted array[1]", array[1], 37);
	Test("sorted array[2]", array[2], 50);
	Test("sorted array[3]", array[3], 63);
	Test("sorted array[4]", array[4], 100);
	Test("sorted array[5]", array[5], 200);
	
	var array0 = new Array(0);
	var array1 = new Array(1);
	var array5 = Array(5);
	var arrayX = Array("x");
	var array3 = new Array(1, 2, "abc");
	
	Test("array0.length", array0.length, 0);
	Test("array1.length", array1.length, 1);
	Test("array5.length", array5.length, 5);
	Test("arrayX.length", arrayX.length, 1);
	Test("array3.length", array3.length, 3);
	
	array3[7] = "entry 7";
	array3["5"] = "entry 5";
	Test("array3.length", array3.length, 8);
	Test("array3[0]", array3[0], 1);
	Test("array3[1]", array3[1], 2);
	Test("array3[2]", array3[2], "abc");
	Test("array3[5]", array3[5], "entry 5");
	Test("array3[7]", array3[7], "entry 7");
	
	Test("0 in array3", 0 in array3, true);
	Test("1 in array3", 1 in array3, true);
	Test("2 in array3", 2 in array3, true);
	Test("3 in array3", 3 in array3, false);
	Test("4 in array3", 4 in array3, false);
	Test("5 in array3", 5 in array3, true);
	Test("6 in array3", 6 in array3, false);
	Test("7 in array3", 7 in array3, true);
	Test("8 in array3", 8 in array3, false);
	
	array3.length = 2;
	Test("array3.length", array3.length, 2);
	Test("0 in array3", 0 in array3, true);
	Test("1 in array3", 1 in array3, true);
	Test("2 in array3", 2 in array3, false);
	Test("3 in array3", 3 in array3, false);
	Test("4 in array3", 4 in array3, false);
	Test("5 in array3", 5 in array3, false);
	Test("6 in array3", 6 in array3, false);
	Test("7 in array3", 7 in array3, false);
	Test("8 in array3", 8 in array3, false);
	
	Test("array.length", array.length, 6);
	array["9"] = null;
	Test("array.length", array.length, 10);
	} // TestArrays


// Test the methods of the built-in Array class.
function TestArrayMethods()
	{
	writeln("TestArrayMethods");
	
	var array = [100, 50, 37, 200, 19, 63];
	
	Test("array.toString()", array.toString(), "100,50,37,200,19,63");
	// Test("array.toLocaleString()", array.toLocaleString(), "100,50,37,200,19,63");
	
	var array2 = ["one", 2].concat(3, "four", ["five", 6]);
	Test("array2.toString()", array2.toString(), "one,2,3,four,five,6");
	
	Test("array.join()", array.join(), "100,50,37,200,19,63");
	Test("array.join('...')", array.join('...'), "100...50...37...200...19...63");
	
	Test("array.pop()", array.pop(), 63);
	Test("array.toString()", array.toString(), "100,50,37,200,19");
	
	// NOTE: we're being a bit tricky here, inserting a subarray.
	// There is no indication in the toString output that it's a
	// subarray, but it acts like a single entry for subsequent calls
	// such as slice and splice.
	Test("array.push('hello', [1,2])", array.push('hello', [1,2]), 7);
	Test("array.toString()", array.toString(), "100,50,37,200,19,hello,1,2");
	
	array.reverse();
	Test("reversed array.toString()", array.toString(), "1,2,hello,19,200,37,50,100");
	
	Test("array.shift()", array.shift().toString(), "1,2");
	Test("typeof([].shift())", typeof([].shift()), "undefined");
	
	Test("array.slice(1,3)", array.slice(1,3).toString(),"19,200");
	Test("array.slice(-2,-1)", array.slice(-2,-1).toString(),"50");
	
	Test("array.splice(1,2,\"foo\")", array.splice(1,2,"foo").toString(), "19,200");
	Test("array.toString()", array.toString(), "hello,foo,37,50,100");
	Test("array.splice(1,1,\"a\",\"b\",\"c\")", array.splice(1,1,"a","b","c").toString(), "foo");
	Test("array.toString()", array.toString(), "hello,a,b,c,37,50,100");
	
	Test("array.unshift(10,20,30)", array.unshift(10,20,30), 10);
	Test("array.toString()", array.toString(), "10,20,30,hello,a,b,c,37,50,100");
	} // TestArrayMethods


function BubbleSort(array, length)
	{
	for (i=0; i<length; i++)
		for (j=length-2; j >= i; j--)
			{
			if (array[j] > array[j+1])
				{
				temp = array[j];
				array[j] = array[j+1];
				array[j+1] = temp;
				}
			}
	
	} // BubbleSort


function TestScopes()
	{
	writeln("TestScopes");
	
	var i=10;
	var j=20;
	
	var o1 = {j:30, k:40};
	var o2 = {i:50, k:60};
	
	SetGlobalI(70);
	
	Test("i", i, 10);
	Test("j", j, 20);
	Test("global i", GetGlobalI(), 70);
	i = 80;
	Test("updated i", i, 80);
	Test("global i", GetGlobalI(), 70);
	
	with (o1)
		{
		Test("i with o1", i, 80);
		Test("j with o1", j, 30);
		Test("k with o1", k, 40);
		}
	
	with (o2)
		{
		Test("i with o2", i, 50);
		Test("j with o2", j, 20);
		Test("k with o2", k, 60);
		}
	
	with (o1)
		with (o2)
			{
			Test("i with o1,o2", i, 50);
			Test("j with o1,o2", j, 30);
			Test("k with o1,o2", k, 60);
			}
	
	with (o2)
		with (o1)
			{
			Test("i with o2,o1", i, 50);
			Test("j with o2,o1", j, 30);
			Test("k with o2,o1", k, 40);
			}
	
	} // TestScopes


// Return the global variable "i".
function GetGlobalI()
	{
	return i;
	} // GetGlobalI


// Set the global variable "i" to the given value.
function SetGlobalI(x)
	{
	i = x;
	} // SetGlobalI


// Test object prototypes.
function TestPrototypes()
	{
	writeln("TestPrototypes");
	
	ProtoObject.prototype.Sum = SumPoint;
	ProtoObject.prototype.z = 100;
	
	var p1 = new ProtoObject(1, 2);
	var p2 = new ProtoObject(3, 4);
	var p3 = p1;
	
	Test("p1.x", p1.x, 1);
	Test("p1.y", p1.y, 2);
	Test("p1.z", p1.z, 100);
	Test("p1.Sum()", p1.Sum(), 3);
	Test("p2.x", p2.x, 3);
	Test("p2.y", p2.y, 4);
	Test("p2.z", p2.z, 100);
	Test("p2.Sum()", p2.Sum(), 7);
	Test("p3.x", p3.x, 1);
	Test("p3.y", p3.y, 2);
	Test("p3.z", p3.z, 100);
	Test("p3.Sum()", p3.Sum(), 3);
	
	p1.x = 10;
	p2.x = 20;
	p3.z = 30;
	
	Test("p1.x", p1.x, 10);
	Test("p1.y", p1.y, 2);
	Test("p1.z", p1.z, 30);
	Test("p1.Sum()", p1.Sum(), 12);
	Test("p2.x", p2.x, 20);
	Test("p2.y", p2.y, 4);
	Test("p2.z", p2.z, 100);
	Test("p2.Sum()", p2.Sum(), 24);
	Test("p3.x", p3.x, 10);
	Test("p3.y", p3.y, 2);
	Test("p3.z", p3.z, 30);
	Test("p3.Sum()", p3.Sum(), 12);
	} // TestPrototypes


function ProtoObject(x, y)
	{
	this.x = x;
	this.y = y;
	} // ProtoObject


// Test exception throwing and handling.
function TestExceptions()
	{
	writeln("TestExceptions");
	
	var path = "";
	try
		{
		path += "<try>";
		throw 5;
		path += " </try>";
		}
	catch (e)
		{
		path += " <catch> ";
		path += e;
		path += " </catch>";
		}
	finally
		{
		path += " <finally>";
		path += " </finally>";
		}
	
	Test("exception path", path, "<try> <catch> 5 </catch> <finally> </finally>");
	
	Test("Catcher(false, 2)", Catcher(false, 2), 8);
	Test("Catcher(true, 2)", Catcher(true, 2), 4);
	} // TestExceptions


// Invoke Thrower(b, x), and return either the function result or the thrown
// exception.
function Catcher(b, x)
	{
	try
		{
		return Thrower(b, x);
		}
	catch (e)
		{
		return e;
		}
	
	return -1; // should never get here
	} // Catcher


// If b is true, then throw x*x, otherwise return x*x*x.
function Thrower(b, x)
	{
	if (b)
		throw x*x;
	else
		return x*x*x;
	
	} // Thrower


// Test globally defined constants and functions.
function TestGlobals()
	{
	writeln("TestGlobals");
	
	Test("NaN", NaN+"", "NaN");
	Test("Infinity", Infinity+"", "Infinity");
	Test("typeof(undefined)", typeof(undefined), "undefined");
	
	Test("isNaN(0)", isNaN(0), false);
	Test("isNaN(37)", isNaN(37), false);
	Test("isNaN(-0.5)", isNaN(-0.5), false);
	Test("isNaN(NaN)", isNaN(NaN), true);
	Test("isNaN(Infinity)", isNaN(Infinity), false);
	Test("isNaN(1/0)", isNaN(1/0), false);
	Test("isNaN(Math.sqrt(-1))", isNaN(Math.sqrt(-1)), true);
	
	Test("isFinite(0)", isFinite(0), true);
	Test("isFinite(37)", isFinite(37), true);
	Test("isFinite(-0.5)", isFinite(-0.5), true);
	Test("isFinite(NaN)", isFinite(NaN), false);
	Test("isFinite(Infinity)", isFinite(Infinity), false);
	Test("isFinite(1/0)", isFinite(1/0), false);
	Test("isFinite(Math.sqrt(-1))", isFinite(Math.sqrt(-1)), false);
	
	// HACK snewman 10/17/01: test eval, parseInt, parseFloat, decodeURI,
	// decodeURIComponent, encodeURI, encodeURIComponent
	} // TestGlobals


// Test the global Math object, as well as the nontrivial properties of the
// Number object.
function TestMath()
	{
	writeln("TestMath");
	
	TestApprox("Number.MAX_VALUE > 1.79769313486231E+308", Number.MAX_VALUE > 1.79769313486231E+308, true);
	TestApprox("isFinite(Number.MAX_VALUE)", isFinite(Number.MAX_VALUE), true);
	// HACK snewman 10/17/01: test Number.MIN_VALUE
	Test("isNaN(Number.NaN)", isNaN(Number.NaN), true);
	Test("isFinite(Number.NEGATIVE_INFINITY)", isFinite(Number.NEGATIVE_INFINITY), false);
	Test("Number.NEGATIVE_INFINITY < 0", Number.NEGATIVE_INFINITY < 0, true);
	Test("isFinite(Number.POSITIVE_INFINITY)", isFinite(Number.POSITIVE_INFINITY), false);
	Test("Number.POSITIVE_INFINITY > 0", Number.POSITIVE_INFINITY > 0, true);
	
	TestApprox("Math.E",       Math.E,       2.7182818284590452354);
	TestApprox("Math.LN10",    Math.LN10,    2.302585092994046);
	TestApprox("Math.LN2",     Math.LN2,     0.6931471805599453);
	TestApprox("Math.LOG2E",   Math.LOG2E,   1.4426950408889634);
	TestApprox("Math.LOG10E",  Math.LOG10E,  0.4342944819032518);
	TestApprox("Math.PI",      Math.PI,      3.1415926535897932);
	TestApprox("Math.SQRT1_2", Math.SQRT1_2, 0.7071067811865476);
	TestApprox("Math.SQRT2",   Math.SQRT2,   1.4142135623730951);
	
	Test("Math.abs(1.5)", Math.abs(1.5), 1.5);
	Test("Math.abs(-1.5)", Math.abs(-1.5), 1.5);
	
	var pi = Math.PI;
	var e  = Math.E;
	
	TestApprox("Math.acos(0.5)", Math.acos(0.5), pi/3);
	TestApprox("Math.asin(0.5)", Math.asin(0.5), pi/6);
	TestApprox("Math.atan(1)",   Math.atan(1),   pi/4);
	
	TestApprox("Math.cos(pi/3)", Math.cos(pi/3), 0.5);
	TestApprox("Math.sin(pi/6)", Math.sin(pi/6), 0.5);
	TestApprox("Math.tan(pi/4)", Math.tan(pi/4), 1);
	
	TestApprox("Math.exp(2)",   Math.exp(2),   e*e);
	TestApprox("Math.log(e*e)", Math.log(e*e), 2);
	Test("Math.sqrt(36)", Math.sqrt(36), 6);
	Test("Math.pow(2,8)", Math.pow(2,8), 256);
	
	Test("Math.ceil(-1.1)", Math.ceil(-1.1), -1);
	Test("Math.ceil(-1.0)", Math.ceil(-1.0), -1);
	Test("Math.ceil(-0.9)", Math.ceil(-0.9), 0);
	Test("Math.ceil(-0.5)", Math.ceil(-0.5), 0);
	Test("Math.ceil(-0.1)", Math.ceil(-0.1), 0);
	Test("Math.ceil(0)", Math.ceil(0), 0);
	Test("Math.ceil(0.1)", Math.ceil(0.1), 1);
	Test("Math.ceil(0.5)", Math.ceil(0.5), 1);
	Test("Math.ceil(0.9)", Math.ceil(0.9), 1);
	Test("Math.ceil(1)", Math.ceil(1), 1);
	Test("Math.ceil(1.1)", Math.ceil(1.1), 2);
	
	Test("Math.floor(-1.1)", Math.floor(-1.1), -2);
	Test("Math.floor(-1.0)", Math.floor(-1.0), -1);
	Test("Math.floor(-0.9)", Math.floor(-0.9), -1);
	Test("Math.floor(-0.5)", Math.floor(-0.5), -1);
	Test("Math.floor(-0.1)", Math.floor(-0.1), -1);
	Test("Math.floor(0)", Math.floor(0), 0);
	Test("Math.floor(0.1)", Math.floor(0.1), 0);
	Test("Math.floor(0.5)", Math.floor(0.5), 0);
	Test("Math.floor(0.9)", Math.floor(0.9), 0);
	Test("Math.floor(1)", Math.floor(1), 1);
	Test("Math.floor(1.1)", Math.floor(1.1), 1);
	
	Test("Math.round(-1.1)", Math.round(-1.1), -1);
	Test("Math.round(-1.0)", Math.round(-1.0), -1);
	Test("Math.round(-0.9)", Math.round(-0.9), -1);
	Test("Math.round(-0.5)", Math.round(-0.5), 0);
	Test("Math.round(-0.1)", Math.round(-0.1), 0);
	Test("Math.round(0)", Math.round(0), 0);
	Test("Math.round(0.1)", Math.round(0.1), 0);
	Test("Math.round(0.5)", Math.round(0.5), 1);
	Test("Math.round(0.9)", Math.round(0.9), 1);
	Test("Math.round(1)", Math.round(1), 1);
	Test("Math.round(1.1)", Math.round(1.1), 1);
	
	Test("Math.max(-7)", Math.max(-7), -7);
	Test("Math.max(1,5)", Math.max(1,5), 5);
	Test("Math.max(-1,-5)", Math.max(-1,-5), -1);
	Test("Math.max(-1,-2,3,-4)", Math.max(-1,-2,3,-4), 3);
	Test("Math.max(-1,Infinity,3,-4)", Math.max(-1,Infinity,3,-4), Infinity);
	Test("isNaN(Math.max(-1,-2,3,NaN))", isNaN(Math.max(-1,-2,3,NaN)), true);
	
	Test("Math.min(-7)", Math.min(-7), -7);
	Test("Math.min(1,5)", Math.min(1,5), 1);
	Test("Math.min(-1,-5)", Math.min(-1,-5), -5);
	Test("Math.min(-1,-2,3,-4)", Math.min(-1,-2,3,-4), -4);
	Test("Math.min(-1,-Infinity,3,-4)", Math.min(-1,-Infinity,3,-4), -Infinity);
	Test("isNaN(Math.min(-1,-2,3,NaN))", isNaN(Math.min(-1,-2,3,NaN)), true);
	
	// HACK snewman 10/17/01: test atan2, random.  Also, there are
	// numerous edge cases I haven't tested, e.g. max or min with zero
	// arguments.
	} // TestMath


// Test the methods of the built-in String class.  Also test the global
// function String.fromCharCode.  We don't test any of the methods that
// involve regular expressions; those are left for a separate suite.
function TestStringMethods()
	{
	writeln("TestStringMethods");
	
	Test("String.fromCharCode()", String.fromCharCode(), "");
	Test("String.fromCharCode(65)", String.fromCharCode(65), "A");
	Test("String.fromCharCode(104, 101, 108, 108, 111)", String.fromCharCode(104, 101, 108, 108, 111), "hello");
	
	var empty = new String("");
	var abc = new String("abc");
	var def = "def";
	
	Test("abc.toString()", abc.toString(), "abc");
	Test("def.toString()", def.toString(), "def");
	Test("abc.valueOf()", abc.valueOf(), "abc");
	Test("def.valueOf()", def.valueOf(), "def");
	
	Test("abc.charAt(0)", abc.charAt(0), "a");
	Test("def.charAt(1)", def.charAt(1), "e");
	Test("def.charCodeAt(2)", def.charCodeAt(2), "102");
	
	Test("empty.concat()", empty.concat(), "");
	Test("empty.concat(abc)", empty.concat(abc), "abc");
	Test("empty.concat(abc,empty,def,'x')", empty.concat(abc,empty,def,'x'), "abcdefx");
	
	Test("empty.length", empty.length, 0);
	Test("abc.length", abc.length, 3);
	Test("def.length", def.length, 3);
	
	var abcabc = "abcabc";
	
	Test("abcabc.indexOf('x')", abcabc.indexOf('x'), -1);
	Test("abcabc.indexOf('a')", abcabc.indexOf('a'), 0);
	Test("abcabc.indexOf('c')", abcabc.indexOf('c'), 2);
	Test("abcabc.indexOf('bc')", abcabc.indexOf('bc'), 1);
	Test("abcabc.indexOf('ac')", abcabc.indexOf('ac'), -1);
	Test("abcabc.indexOf('a',2)", abcabc.indexOf('a',2), 3);
	
	Test("abcabc.lastIndexOf('x')", abcabc.lastIndexOf('x'), -1);
	Test("abcabc.lastIndexOf('a')", abcabc.lastIndexOf('a'), 3);
	Test("abcabc.lastIndexOf('c')", abcabc.lastIndexOf('c'), 5);
	Test("abcabc.lastIndexOf('bc')", abcabc.lastIndexOf('bc'), 4);
	Test("abcabc.lastIndexOf('ac')", abcabc.lastIndexOf('ac'), -1);
	
	Test("abc.localeCompare(def) < 0", abc.localeCompare(def) < 0, true);
	Test("abc.localeCompare(abc)", abc.localeCompare(abc), 0);
	
	Test("abcabc.replace('b', 'xbx')", abcabc.replace('b', 'xbx'), "axbxcaxbxc");
	Test("abcabc.replace('d', 'xbx')", abcabc.replace('d', 'xbx'), "abcabc");
	Test("abcabc.replace('b', '')", abcabc.replace('b', ''), "acac");
	
	Test("'aBc'.toLowerCase()", 'aBc'.toLowerCase(), "abc");
	Test("'aBc'.toUpperCase()", 'aBc'.toUpperCase(), "ABC");
	Test("'aBc'.toLocaleLowerCase()", 'aBc'.toLocaleLowerCase(), "abc");
	Test("'aBc'.toLocaleUpperCase()", 'aBc'.toLocaleUpperCase(), "ABC");
	
	Test("abcabc.slice(0,4)", abcabc.slice(0,4), "abca");
	Test("abcabc.slice(2,4)", abcabc.slice(2,4), "ca");
	Test("abcabc.slice(3,-1)", abcabc.slice(3,-1), "ab");
	Test("abcabc.slice(-2,-1)", abcabc.slice(-2,-1), "b");
	Test("abcabc.slice(-4)", abcabc.slice(-4), "cabc");
	
	Test("abcabc.substring(0,4)", abcabc.substring(0,4), "abca");
	Test("abcabc.substring(2,4)", abcabc.substring(2,4), "ca");
	Test("abcabc.substring(3,-1)", abcabc.substring(3,-1), "abc");
	Test("abcabc.substring(-2,-1)", abcabc.substring(-2,-1), "");
	Test("abcabc.substring(-4)", abcabc.substring(-4), "abcabc");
	Test("abcabc.substring(8,4)", abcabc.substring(8,4), "bc");
	
	Test("abcabc.split('b').join(',')", abcabc.split('b').join(','), "a,ca,c");
	Test("abcabc.split('').join(',')", abcabc.split('').join(','), "a,b,c,a,b,c");
	Test("abcabc.split('',4).join(',')", abcabc.split('',4).join(','), "a,b,c,a");
	} // TestStringMethods
