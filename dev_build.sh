echo Build geometry
cd ../geometry-api-cs
rm -R obj
rm -R bin
git pull origin master
xbuild /p:Configuration=Release geometry-api-cs.csproj

echo Build geometry worker
cd ../geometry-worker
rm -R geometry-worker/bin/
rm -R geometry-worker/obj/
rm lib/geometry-api-cs.dll
mv ../geometry-api-cs/bin/Release/geometry-api-cs.dll lib/
xbuild /p:Configuration=Release ./geometry-worker/geometry-worker.csproj

cd geometry-worker
azure login
docker-machine start geometry-worker
docker-machine env geometry-worker
eval "$(docker-machine env geometry-worker)"
docker-machine stop geometry-worker
docker build -t davidraleigh/geometry-worker .
# if I only knew how to rename old machines...
#docker run -d --name geometry-worker davidraleigh/geometry-worker
docker run -d davidraleigh/geometry-worker
