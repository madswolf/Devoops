import sqlite3
from  urllib import request, parse
import requests
import ssl
import json

API_ENDPOINT = "https://localhost:7077/Users/Create"
DATABASE = "../pyshite/minitwit.db"
conn = sqlite3.connect(DATABASE)
cur = conn.cursor()


def get_all_users():
    cur.execute("SELECT * FROM user")
    return cur.fetchall()


def parse_hash(hash):
    return hash.split("$")[1:]


def post_user(record):
    user_name = record[1]
    email = record[2]
    salt, hash = parse_hash(record[3])
    ctx = ssl.create_default_context()
    ctx.check_hostname = False
    ctx.verify_mode = ssl.CERT_NONE
    data = {"Username": user_name, "Email": email, "Salt": salt, "PasswordHash": hash}
    print(data)
    response = requests.post(API_ENDPOINT, data=data,  verify=False)
    print(response)

def run():
    users = get_all_users()
    for u in users:
        post_user(u)


run()