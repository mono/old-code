# Tester prerequisites: method calls/defs, print, if, globals, not, ==, !, to_s, inspect
$tests = 0
$test_fails = 0

def suite(name)
    print "*** Testing ", name, "...\n"
end

def print_summary
    print "Tests done: ", $tests.to_s, " failed: ", $test_fails.to_s, "\n"
    if $test_fails == 0
        print "All OK\n"
    else
        print "FAIL\n"
    end
end

def assert(name,bool)
    $tests += 1
    if !bool
        print "Assertion failed: ", name, "\n"
        $test_fails += 1
    end
end

def assert_equal(a,b)
    assert(a.inspect + " == " + b.inspect, a == b)
end

def assert_not_equal(a,b)
    assert(a.inspect + " != " + b.inspect, a != b)
end

suite "equality"
    assert_equal true, true
    assert_not_equal true, false
    assert_not_equal false, true
    assert_equal false, false
    assert_equal nil, nil
    assert_not_equal false, nil
    assert_not_equal false, "a"
    assert_not_equal true, "a"

suite "logic_not"
    assert_equal !false, true
    assert_equal !nil, true
    assert_equal !true, false
    assert_equal !"a", false

suite "logic_and"
    assert_equal(false && false, false)
    assert_equal(false && true, false)
    assert_equal(true && false, false)
    assert_equal(true && true, true)
    assert_equal("a" && "b", "b")
    assert_equal(nil && false, nil)
    assert_equal(false && nil, false)

suite "logic_or"
    assert_equal(false || false, false)
    assert_equal(false || true, true)
    assert_equal(true || false, true)
    assert_equal(true || true, true)
    assert_equal("a" || "b", "a")
    assert_equal("a" || false, "a")
    assert_equal(false || "b", "b")
    assert_equal(false || nil, nil)
    assert_equal(nil || false, false)

suite "int_arithmetic"
    assert_equal(1, 1)
    assert_equal(1+1, 2)
    assert_equal(3-2, 1)
    assert_equal(-3, 4-7)
    assert_equal(2*2, 4)
    assert_equal(9/3, 3)
    assert_equal(9+3*2-7, 2*2+1*4)


print_summary
# vim:et:sts=4:sw=4
