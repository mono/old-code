#!/bin/bash
echo "namespace ValDoc {"
echo "public class RelaxNgSchema {"
echo "   public static string SCHEMA = \" \" + "
while read line
do
echo "       \""$(echo "$line" | sed s/"\""/"'"/ )"\"+"
done

echo "       \" \"; "
echo "}"
echo "}";