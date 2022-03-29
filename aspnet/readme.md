# running locally
```cmd
cd Minitwit
dotnet run
```
This will use a local in-memory database and logging
To use another postgres db set DATABASE_CONNECTION_STRING
To use another logging url set ELASTICSEARCH_CONNECTION_STRING

# provision
Set SSH_KEY_NAME, DIGITAL_OCEAN_TOKEN and DROPLET_NAME to appropriat values
prepare/download deployer environment script named "env.sh" with the fields
DATABASE_CONNECTION_STRING and ELASTICSEARCH_CONNECTION_STRING

Then run
```cmd
vagrant up
```