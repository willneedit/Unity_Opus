
wsl /bin/bash -l -c ./build.sh

mkdir out\Windows
cd out\Windows

cmake -DCMAKE_BUILD_TYPE=Release -GNinja ..\..

ninja GeneratePlugins

cd ..\..