import sqlite3
from  urllib import request, parse

API_ENDPOINT = "https://localhost:5000/migrateAcc"
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
    data = parse.urlencode({"username": user_name, "email": email, "salt": salt, "hash": hash}).encode()
    request.Request(API_ENDPOINT, data=data)


def run():
    users = get_all_users()
    for u in users:
        post_user(u)


run()