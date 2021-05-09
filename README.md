# Sample application for splitting a bill among friends

## Building 
- Run `docker compose build` to build the container images

## Running
- Run `docker compose up` to bring up the containers.
- The first time you run the app, it will likely crash due to SQL server being unavailable while it completes installation. The application is set to restart automatically and this should resolve quickly.

## API Documentation
With the application running, visit [http://localhost:5000/swagger/index.html](http://localhost:5000/swagger/index.html) to view the swagger documentation and test the API.


## TODO List
- Write more integration tests
- Write more unit tests
- Write tests with docker to test the SQL
- Build kubernetes/helm yaml for the app
- Setup CI/CD (Github actions, buildkite, etc)
- Create react UI
- Write react unit tests
