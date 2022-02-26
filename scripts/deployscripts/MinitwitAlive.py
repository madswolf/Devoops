import requests
import time
import sys

alive_url = "http://" + sys.argv[1]

for _ in range(10):
    try:
        res = requests.get(alive_url)
        if res.status_code == 200:
            exit(0)
    except SystemExit:
        exit(0)
    except:
        time.sleep(1)
exit(1)