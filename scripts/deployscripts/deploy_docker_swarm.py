import os
import subprocess
import time

import paramiko
import requests

from paramiko import SSHClient

SSH_KEY = os.environ["SSH_KEY_FOOTPRINT"]
DIGITAL_API_TOKEN = os.environ["DIGITAL_API_TOKEN"]
# FILL THESE VALUES ABOVE WITH WHATEVER WORKS FOR YOU

DROPLETS_URL = "https://api.digitalocean.com/v2/droplets"

MANAGER_PAYLOAD = {
    "name": "swarm-manager", "region": "fra1", "size": "s-1vcpu-1gb",
    "image": "docker-20-04",
    "ssh_keys": [SSH_KEY]
}

WORKER1_PAYLOAD = {
    "name": "worker1", "region": "fra1", "size": "s-1vcpu-1gb",
    "image": "docker-20-04",
    "ssh_keys": [SSH_KEY]
}

WORKER2_PAYLOAD = {
    "name": "worker2", "region": "fra1", "size": "s-1vcpu-1gb",
    "image": "docker-20-04",
    "ssh_keys": [SSH_KEY]
}

SLEEPY_TIME = 10

OPEN_PORTS_COMMAND = "ufw allow 22/tcp &&" \
                     " ufw allow 2376/tcp &&" \
                     " ufw allow 2377/tcp &&" \
                     " ufw allow 7946/tcp &&" \
                     " ufw allow 7946/udp &&" \
                     " ufw allow 4789/udp &&" \
                     " ufw reload && ufw --force  enable &&" \
                     " systemctl restart docker"

CREATE_SWARM_TOKEN_COMMAND = "docker swarm join-token worker -q"

GET_SWARM_STATUS_COMMAND = "docker node ls"

DOCKER_DEPLOY_COMMAND = "cd Devoops && " \
                        "docker stack deploy --compose-file docker-compose.yaml minitwit"


def make_authenticated_request(request_type, data=None):
    if request_type == "get":
        return requests.get(DROPLETS_URL, headers={
            'Authorization': f'Bearer {DIGITAL_API_TOKEN}'})
    elif request_type == "post":
        return requests.post(DROPLETS_URL, data, headers={
            'Authorization': f'Bearer {DIGITAL_API_TOKEN}'})


def find_machine_ip(request_json, droplet_name):
    for droplet in request_json["droplets"]:
        if droplet["name"] == droplet_name:
            for network in droplet["networks"]["v4"]:
                if network["type"] == "public":
                    return network["ip_address"]


def perform_ssh_command(client, client_ip, command):
    client.connect(client_ip, username='root')
    stdin, stdout, stderr = client.exec_command(command)
    return_val = stdout.read()
    client.close()
    return return_val


if __name__ == '__main__':
    # Swarm manager
    make_authenticated_request("post", MANAGER_PAYLOAD)
    time.sleep(SLEEPY_TIME)
    swarm_manager_response = make_authenticated_request("get")
    swarm_manager_ip = find_machine_ip(swarm_manager_response.json(), "swarm-manager")

    # worker 1
    make_authenticated_request("post", WORKER1_PAYLOAD)
    time.sleep(SLEEPY_TIME)
    worker1_response = make_authenticated_request("get")
    worker1_ip = find_machine_ip(worker1_response.json(), "worker1")

    # worker 2
    res = make_authenticated_request("post", WORKER2_PAYLOAD)
    time.sleep(SLEEPY_TIME)
    worker2_response = make_authenticated_request("get")
    worker2_ip = find_machine_ip(worker2_response.json(), "worker2")

    client = SSHClient()
    client.set_missing_host_key_policy(paramiko.AutoAddPolicy())
    # TODO: this probably has to be changed to a file-based host keys if we want to CI/CD this
    client.load_system_host_keys()

    # wait for machines to boot
    time.sleep(60)
    perform_ssh_command(client, swarm_manager_ip, OPEN_PORTS_COMMAND)

    start_docker_swarm_command = f"docker swarm init --advertise-addr {swarm_manager_ip}"
    perform_ssh_command(client, swarm_manager_ip, start_docker_swarm_command)

    perform_ssh_command(client, worker1_ip, OPEN_PORTS_COMMAND)
    perform_ssh_command(client, worker2_ip, OPEN_PORTS_COMMAND)

    command_output = perform_ssh_command(client, swarm_manager_ip, CREATE_SWARM_TOKEN_COMMAND)
    swarm_token = command_output.decode().strip()

    remote_join_command = f"docker swarm join --token {swarm_token} {swarm_manager_ip}:2377"
    perform_ssh_command(client, worker1_ip, remote_join_command)
    perform_ssh_command(client, worker2_ip, remote_join_command)

    print(perform_ssh_command(
        client,
        swarm_manager_ip,
        GET_SWARM_STATUS_COMMAND
    ).decode().strip())

    subprocess.run(["scp",
                    "-o", "StrictHostKeyChecking=no",
                    "-r", "../Devoops", f"root@{swarm_manager_ip}:~"])

    perform_ssh_command(client, swarm_manager_ip, DOCKER_DEPLOY_COMMAND)

    print(f"Swarm is available at {swarm_manager_ip}")
