#!/bin/bash
# Carga el código de un paso de la clase en el proyecto.
# Uso:  ./LessonSteps/load_step.sh step2_night_shift
# Después, vuelve a Unity: recompila solo.
set -e
cd "$(dirname "$0")/.."
step="LessonSteps/$1"
if [ -z "$1" ] || [ ! -d "$step" ]; then
  echo "Pasos disponibles:"; ls -d LessonSteps/step*/ | xargs -n1 basename; exit 1
fi

dest="Assets/Scripts/Couriers"
# Quitar los repartidores que no existen en este paso (con su .meta).
for f in "$dest"/*.cs; do
  [ -e "$f" ] || continue
  name=$(basename "$f")
  [ -f "$step/$name" ] || rm -f "$f" "$f.meta"
done
# Copiar los del paso. Sobrescribir conserva el .meta, así la escena no pierde referencias.
for f in "$step"/*.cs; do
  name=$(basename "$f")
  if [ "$name" = "GameHUD.cs" ]; then cp "$f" Assets/Scripts/Game/GameHUD.cs; else cp "$f" "$dest/$name"; fi
done
echo "Cargado: $1"
