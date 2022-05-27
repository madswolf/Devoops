## Running (locally)

### Prerequisites

1. Docker

### Set Evironment Variables

* GF_SECURITY_ADMIN_USER
* GF_SECURITY_ADMIN_PASSWORD

### Start docker

1. Run ```Docker compose up```

## Provision

### Prerequisites

1. Vagrant

### Install

```cmd
vagrant plugin install vagrant-digitalocean
sudo apt install virtualbox-qt
vagrant plugin install vagrant-docker-compose
```

### Set Evironment Variables

* GRAFANA_SSH_KEY_NAME
* GRAFANA_DIGITAL_OCEAN_TOKEN

### Provision 

```cmd 
vagrant up
```

### Manual Steps

When restarting/migrating the monitoring, remember to always:
1. Fix database data source configuration, by changing the certification type and providing a password
2. Add discord to the list of notifiers, the webhook can be found on actual discord channel.
3. Add alert on `up?` metric, with 0 as triggering condition, waiting period of 60s and the previously mentioned discord as go to channel.