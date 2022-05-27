# Minitwit

This is an asp.net application created for the course Devops 2022 on ITU.
It uses and ELK stack for logging,
Prometheus and Grafana for monitoring,
and a PSQL database for storage.

Made by:

* Mads Wolf Jespersen (oeje@itu.dk)
* Nicolai Pallund (npal@itu.dk)
* Julian Brandt (jubr@itu.dk)
* Osman Abdinasir Hassan (osha@itu.dk)
* Jakub Sowa (jsow@itu.dk)

## Static analysis

### Better code

[![BCH compliance](https://bettercodehub.com/edge/badge/ChadIImus/Devoops?branch=master)](https://bettercodehub.com/)

### Sonarcloud

[![Bugs](https://sonarcloud.io/api/project_badges/measure?project=devops2022&metric=bugs)](https://sonarcloud.io/summary/new_code?id=devops2022)
[![Code Smells](https://sonarcloud.io/api/project_badges/measure?project=devops2022&metric=code_smells)](https://sonarcloud.io/summary/new_code?id=devops2022)
[![Duplicated Lines (%)](https://sonarcloud.io/api/project_badges/measure?project=devops2022&metric=duplicated_lines_density)](https://sonarcloud.io/summary/new_code?id=devops2022)
[![Lines of Code](https://sonarcloud.io/api/project_badges/measure?project=devops2022&metric=ncloc)](https://sonarcloud.io/summary/new_code?id=devops2022)
[![Maintainability Rating](https://sonarcloud.io/api/project_badges/measure?project=devops2022&metric=sqale_rating)](https://sonarcloud.io/summary/new_code?id=devops2022)
[![Reliability Rating](https://sonarcloud.io/api/project_badges/measure?project=devops2022&metric=reliability_rating)](https://sonarcloud.io/summary/new_code?id=devops2022)
[![Security Rating](https://sonarcloud.io/api/project_badges/measure?project=devops2022&metric=security_rating)](https://sonarcloud.io/summary/new_code?id=devops2022)
[![Technical Debt](https://sonarcloud.io/api/project_badges/measure?project=devops2022&metric=sqale_index)](https://sonarcloud.io/summary/new_code?id=devops2022)
[![Vulnerabilities](https://sonarcloud.io/api/project_badges/measure?project=devops2022&metric=vulnerabilities)](https://sonarcloud.io/summary/new_code?id=devops2022)

### Code Climate

[![Maintainability](https://api.codeclimate.com/v1/badges/cf02af0e82bd566fae60/maintainability)](https://codeclimate.com/github/ChadIImus/Devoops/maintainability)

## Main application

### Prerequisites

1. Docker
2. A PSQL database
3. An ELK stack
4. (deploy) python


### Running (locally)

1. Set environment variables:
    * DATABASE_CONNECTION_STRING
    * ELASTICSEARCH_CONNECTION_STRING (host + basic authentication)

2. set replicas in the docker-compose file to 1
3. Run ```docker compose up```

### Deploying

1. Set environment variables:
    * SSH_KEY_FOOTPRINT
    * DIGITAL_API_TOKEN
    * DATABASE_CONNECTION_STRING
    * ELASTICSEARCH_CONNECTION_STRING (host + basic authentication)
2. Run ```python script/deployscripts/deploy_docker_swarm.py```

## Supporting deployments/applications

See Logging and Monitoring folders' readme's for instructions

### PSQL database

#### Prerequisites

1. dotnet-ef cli tool
2. Fresh PSQL database

#### Initialise

1. Navigate to aspnet/Minitwit
2. set Environment variable "DATABASE_CONNECTION_STRING"
3. Run ```dotnet ef database update```
