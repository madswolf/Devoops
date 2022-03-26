#Running ELK containers locally
```cmd
sudo chmod +x setup_elk.sh
source setup_elk.sh
printf "USERNAME:$(openssl passwd -crypt PASSWORD)\n" > .htpasswd 
docker-compose up
```
go to localhost:5601 and enter the username and password entered above.


#Provisioning ELK VM with digitalocean
Set environment variables ELASTIC_PASS and DIGITAL_OCEAN_TOKEN to appropriate values.
Place ssh-keys, registered on the digitalocean team corresponding to the token,
in this directory.
```cmd
vagrant plugin install vagrant-digitalocean
vagrant plugin install vagrant-docker-compose
vagrant up
```
