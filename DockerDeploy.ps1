﻿# for test
docker rm  learninganalytics_db_1 --force 
docker rm  learninganalytics_app_1 --force 
docker build . -f "LearningAnalytics.API\DockerFile" --force-rm -t krauzerkrip:quizer --no-cache

docker-compose up -d
docker exec learninganalytics_app_1 c:\migration\LearningAnalytics.Migration.exe