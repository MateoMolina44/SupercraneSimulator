"""Empaqueta un build Web de Unity para itch.io: index.html en la raíz y rutas con '/'."""
import os
import sys
import zipfile

root, out = sys.argv[1], sys.argv[2]
if os.path.exists(out):
    os.remove(out)

# Los .gz ya vienen comprimidos: se guardan sin volver a comprimir.
with zipfile.ZipFile(out, "w", zipfile.ZIP_STORED) as z:
    for folder, _, files in os.walk(root):
        for name in files:
            path = os.path.join(folder, name)
            z.write(path, os.path.relpath(path, root).replace(os.sep, "/"))

with zipfile.ZipFile(out) as z:
    names = z.namelist()
    print("corrupt:", z.testzip())
    print("backslashes:", [n for n in names if chr(92) in n])
    print("index at root:", "index.html" in names)
    print(len(names), "entries,", round(os.path.getsize(out) / 1e6, 1), "MB")
