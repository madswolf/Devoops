### Prerequisites

1. Docker
2. A PSQL database
3. An ELK stack
4. (deploy) python


### Running (locally)

1. Open shell in git root folder
2. Set environment variables:
    * DATABASE_CONNECTION_STRING
    * ELASTICSEARCH_CONNECTION_STRING (host + basic authentication)
3. set replicas in the docker-compose file to 1
4. Run ```docker compose up```

### Deploying

1. Open shell in git root folder
2. Set environment variables:
    * SSH_KEY_FOOTPRINT
    * DIGITAL_API_TOKEN
    * DATABASE_CONNECTION_STRING
    * ELASTICSEARCH_CONNECTION_STRING (host + basic authentication)
3. Run ```python scripts/deployscripts/deploy_docker_swarm.py```