import sqlite3
from  urllib import request, parse

API_ENDPOINT = "https://localhost:5000/migrateMsg"
DATABASE = "../pyshite/minitwit.db"
conn = sqlite3.connect(DATABASE)
cur = conn.cursor()


def get_all_messages():
    cur.execute("""
    SELECT u.username, m.text, m.pub_date, m.flagged
    FROM message m
    JOIN user u on u.user_id = m.author_id
    """)
    return cur.fetchall()


def post_message(record):
    author = record[0]
    text = record[1]
    pubdate = record[2]
    flagged = record[3]
    data = parse.urlencode({"author": author, "text": text, "pubdate": pubdate, "flagged": flagged}).encode()
    request.urlopen(API_ENDPOINT, data=data)


def run():
    messages = get_all_messages()
    for m in messages:
        post_message(m)

run()