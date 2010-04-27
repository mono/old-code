#!/bin/sh

TYPES=""
COUNT=0;

for i in `find .`; do 
	for t in `grep "\[SupportedMimeType " $i | sed -e 's/SupportedMimeType//g; s/[^a-z0-9\\/\\-]*//g' | grep -v entagged`; do
		if [ $COUNT -le 0 ]; then
			TYPES=$t;
		else
			TYPES="$TYPES;$t";
		fi;

		COUNT=`expr $COUNT + 1`
	done;
done;

echo $TYPES;


