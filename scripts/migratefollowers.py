import sqlite3
from  urllib import request, parse
import requests

API_ENDPOINT = "https://localhost:7077/Users/MigrationFollow"
DATABASE = "../pyshite/minitwit.db"
conn = sqlite3.connect(DATABASE)
cur = conn.cursor()


def get_all_followers():
    cur.execute("""
    SELECT u1.username, u2.username
    FROM follower f
    JOIN user u1 on f.who_id = u1.user_id
    JOIN user u2 on f.whom_id = u2.user_id
    """)
    return cur.fetchall()



def post_follower(record):
    cur_user = record[0]
    followed_user = record[1]
    data = {"Who": cur_user, "Whom": followed_user}
    print(data)
    response = requests.post(API_ENDPOINT, data=data,  verify=False)
    print(response)


def run():
    followers = get_all_followers()
    for f in followers:
        post_follower(f)

run()