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
