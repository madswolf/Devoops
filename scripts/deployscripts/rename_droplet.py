import requests
import sys

"""
sys arg1: current name of droplet
sys arg2: target name of droplet
sys arg3: digitalocean token
"""

digitalocean_getdroplets_url = "https://api.digitalocean.com/v2/droplets"
digitalocean_droplet_name = sys.argv[1]
digitalocean_headers = {"Authorization": "Bearer " + sys.argv[3]}

digitalocean_droplets_response = requests.get(digitalocean_getdroplets_url, headers=digitalocean_headers)
digitalocean_json = digitalocean_droplets_response.json()

def get_id_from_name(name, json):
    for i in range(len(json["droplets"])):
        if json["droplets"][i]["name"] == name:
            return json["droplets"][i]["id"]
    raise Exception("Could not find a droplet with the given name: " + name)

digitalocean_id = get_id_from_name(digitalocean_droplet_name, digitalocean_json)
digitalocean_rename_url = "https://api.digitalocean.com/v2/droplets/" + str(digitalocean_id) + "/actions"
data = {"type": "rename", "name": sys.argv[2]}
digitalocean_rename_response = requests.post(digitalocean_rename_url, data=data, headers=digitalocean_headers)

if 200 <= digitalocean_rename_response.status_code < 300:
    exit(0)
else:
    exit(1)