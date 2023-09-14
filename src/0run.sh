#!/bin/bash
basepath=$(dirname $(readlink -f "$0"))
basepath_dir=$(dirname "${basepath}")
echo "basepath=$basepath"
echo "basepath_dir=$basepath_dir"
cd $basepath
dotnet mapgis_gm_dump_analyzer.dll