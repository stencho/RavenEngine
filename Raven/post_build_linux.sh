project_names=("$@")

for name in "${project_names[@]}"; do
  echo "Copying engine content to $name"
  rsync -avh --progress --partial --inplace Content/bin/DesktopGL/Content/ "../$name/bin/Debug/net9.0/Content/"
done
