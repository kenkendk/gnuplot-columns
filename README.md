gnuplot-columns
===============

Column ouput from text files, compatible with Gnuplot and LibreOffice calc.

This project solves a recurring problem with tabulating data from test runs.

When gathering data from scientific experiments (performance timings in particular), I often generate a large blob of data that I need to filter and combine.

When I run the test, there is a repetition and a parameter variation part.

The typical files look like:
  	
	nbody 10000x100000: 300 sec
	garbage text, debugging info, etc
	garbage text, debugging info, etc
	shallowwater 100000x36: 120 sec 
	garbage text, debugging info, etc
	nbody 10000x100000: 300 sec
	garbage text, debugging info, etc
	garbage text, debugging info, etc
	shallowwater 100000x36: 120 sec 
	garbage text, debugging info, etc
	... more data, next parameter, etc ...


To get some meaning from the data I need to tabulate this into a row parameter so each row represents all tests with a particular parameter setup.

To solve this, you could write a script similar to:
  
	cat data.txt | grep nbody | awk '{print $3}'

This will give you a list of all the numbers from the nbody runs, which you then need to split into columns. I have not found any tool that could do that and output a simple tab separated file.

This tool can do it all in a single go:
  
	columns --inputfile=data.txt --lineregexp=nbody --columns=10 --fieldindex=-2

Documentation output:

	columns - A tool for formatting data in columns, compatible with gnuplot
	
	Usage: 
	columns [--option=value]
	
	If no options are given, the program will read from stdin and write to stdout
	
	Options:
		--columns: The number of columns to output
			Default: 3
		--columseperator: The character(s) to output as columns seperators
			Default: \t
		--fieldseperator: The character(s) used to seperate fields
			Default: \t<space>
		--fieldindex: The index of the field to output, 0 means entire line, -1 means first number field, -2 means last number field
			Default: 0
		--lineregexp: The regular expression to find in the line
			Default: .*
		--inputfile: The file to read input data from, will use stdin if not present
		--outputfile: The file to write output data to, will use stdout if not present
		--verbose: True to generate debug output, false otherwise
			Default: false