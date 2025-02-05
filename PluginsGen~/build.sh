#!/bin/bash

mkdir -p out/Linux
pushd out/Linux

cmake -DCMAKE_BUILD_TYPE=Release -GNinja ../..

ninja GeneratePlugins

popd
