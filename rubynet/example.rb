def puts(s)
	print s
	print "\n"
end

puts "Hello world!"


# Do some fibonacci exercises
puts "Now, for some fibonacci numbers:"
a = 1
b = 1
max = "100".to_i
# Array
series = []
while a < max
	series << a
	temp = a
	a = b
	b = temp + b
end

# Closures and blocks

sum = 0
i = 1

series.each do |x|
	print "The ", i, ". Fibonacci number is: ", x, "\n"
	sum += x
	i += 1
end

# Some string manipulation and conversion
puts "Sum: " + sum.to_s

# Nested scopes and if and []
def odds(arr)
	ret = []
	arr.each_index { |i|
		if i % 2 == 1
			ret << arr[i]
		end
	}
	ret
end

# And so on
puts "The odd elements are: " + odds(series).join(", ")
