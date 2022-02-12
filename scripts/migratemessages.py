import sqlite3
from  urllib import request, parse
import requests

API_ENDPOINT = "https://localhost:7077/Messages/Create"
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
    print(record)
    data = {"AuthorName": author, "Text": text, "Created": pubdate, "Flagged": bool(flagged)}
    response = requests.post(API_ENDPOINT, data=data,  verify=False)
    print(response)


def run():
    messages = get_all_messages()
    for m in messages:
        post_message(m)

run()