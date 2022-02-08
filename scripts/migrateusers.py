import sqlite3
from  urllib import request

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
    qs = "?username={}&email={}&salt={}&hash={}".format(user_name, email, salt, hash)
    print(API_ENDPOINT + qs)
    #request.Request(API_ENDPOINT + qs)


def run():
    users = get_all_users()
    for u in users:
        post_user(u)


run()