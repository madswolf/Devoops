import requests
import time
import sys

"""
sys arg1: name of droplet to check alive status of
sys arg2: digitalocean token
"""

digitalocean_getdroplets_url = "https://api.digitalocean.com/v2/droplets"
digitalocean_droplet_name = sys.argv[1]

digitalocean_getdroplets_headers = {"Authorization": "Bearer " + sys.argv[2]}

digitalocean_droplets_response = requests.get(digitalocean_getdroplets_url, headers=digitalocean_getdroplets_headers)
json = digitalocean_droplets_response.json()

def get_ip_from_name(name):
    for i in range(len(json["droplets"])):
        if json["droplets"][i]["name"] == name:
            return list(filter(lambda e: e["type"] == "public", json["droplets"][i]["networks"]["v4"]))[0]["ip_address"]
    raise ValueError("Could not find a droplet with the given name: " + digitalocean_droplet_name)

alive_url = "http://" + get_ip_from_name(digitalocean_droplet_name)

print(alive_url)

for _ in range(300):
    try:
        res = requests.get(alive_url)
        if res.status_code == 200:
            exit(0)
    except SystemExit:
        exit(0)
    except:
        time.sleep(1)
exit(1)