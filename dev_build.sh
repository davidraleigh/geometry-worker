# to run this script you have to have opened a docker quickstart terminal
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
docker images
docker ps -a
docker-machine ls
docker stop geometry-worker-container
docker images
docker ps -a
docker-machine ls
docker rm geometry-worker-container
docker images
docker ps -a
docker-machine ls
docker-machine stop geometry-worker
docker images
docker ps -a
docker-machine ls
docker-machine start geometry-worker
docker images
docker ps -a
docker-machine ls
docker-machine env geometry-worker
eval "$(docker-machine env geometry-worker)"
docker images
docker ps -a
docker-machine ls
docker build -t davidraleigh/geometry-worker .
docker images
docker ps -a
docker-machine ls
docker run -d --name geometry-worker-container davidraleigh/geometry-worker
docker images
docker ps -a
docker-machine ls
