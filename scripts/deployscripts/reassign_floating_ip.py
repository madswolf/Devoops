import requests
import sys

"""
sys arg1: name of droplet to receive floating ip
sys arg2: digital ocean token
"""

digitalocean_floatingips_url = "https://api.digitalocean.com/v2/floating_ips"
digitalocean_droplets_url = "https://api.digitalocean.com/v2/droplets"
digitalocean_droplet_name = sys.argv[1]
headers = {"Authorization": "Bearer " + sys.argv[2]}

digitalocean_floatingip = requests.get(digitalocean_floatingips_url, headers=headers).json()["floating_ips"][0]["ip"]
digitalocean_droplets = requests.get(digitalocean_droplets_url, headers=headers)
def get_id_from_name(name, json):
    for i in range(len(json["droplets"])):
        if json["droplets"][i]["name"] == name:
            return json["droplets"][i]["id"]
    raise Exception("Could not find a droplet with the given name: " + name)

id = get_id_from_name(digitalocean_droplet_name, digitalocean_droplets.json())

digitalocean_floatingip_assign_url = f"https://api.digitalocean.com/v2/floating_ips/{digitalocean_floatingip}/actions"
payload = {"type": "assign", "droplet_id": id}

update_resp = requests.post(digitalocean_floatingip_assign_url, json=payload, headers=headers)
if 200 <= update_resp.status_code < 300:
    exit(0)
else:
    exit(1)
