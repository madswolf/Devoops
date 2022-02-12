import sqlite3
from  urllib import request, parse

API_ENDPOINT = "https://localhost:5000/migrateFollowers"
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
    data = parse.urlencode({"who": cur_user, "whom": followed_user}).encode()
    request.urlopen(API_ENDPOINT, data=data)


def run():
    followers = get_all_followers()
    for f in followers:
        post_follower(f)

run()