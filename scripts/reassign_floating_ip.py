import os
import sys

import requests


def make_authenticated_request(url, payload=None, method="get"):
    if method == "get":
        response = requests.get(url, headers={"Authorization": f"Bearer {os.environ.get('DIGITAL_OCEAN_TOKEN')}"})
    else:
        response = requests.post(
            url,
            data=payload,
            headers={"Authorization": f"Bearer {os.environ.get('DIGITAL_OCEAN_TOKEN')}"})
    return response.json()


def find_new_droplet_id(droplets, new_droplet_name):
    for droplet in droplets["droplets"]:
        if droplet["name"] == new_droplet_name:
            return droplet["id"]
    return None


'''
first command line argument is the id of the new machine to assign the floating IP to
in order to use the script, the DIGITAL_OCEAN_TOKEN has to be set up within enviroment variables
'''
if __name__ == '__main__':
    new_droplet_name = sys.argv[1]
    droplets = make_authenticated_request("https://api.digitalocean.com/v2/droplets")

    new_droplet_id = find_new_droplet_id(droplets, new_droplet_name)
    if not new_droplet_id:
        sys.exit(1)

    # find floating ip to use
    floating_ip = make_authenticated_request("http://api.digitalocean.com/v2/floating_ips")["floating_ips"][0]["ip"]
    # assign a floating ip to a new droplet
    make_authenticated_request(
        f"http://api.digitalocean.com/v2/floating_ips/{floating_ip}/actions",
        method="post",
        payload={
            "type": "assign",
            "droplet_id": new_droplet_id
        })
