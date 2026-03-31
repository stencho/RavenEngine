#/bin/sh
project_names=("Cassowary")

for name in "${project_names[@]}"; do 
	echo "Copying engine content to $name"
	rsync -avh --progress --partial --inplace Raven/Content/bin/DesktopGL/Content/ $name/bin/Debug/net9.0/Content/
done;
