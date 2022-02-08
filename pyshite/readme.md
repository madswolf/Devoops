# setting up environment
```commandline
pip install -r requirements.txt
```
# running
```commandline
python .\minitwit.py
```
# Description of program
## `/`
Shows the page with the logged in user's tweets.

## `/public`
Shows the public feed with all users' tweets.

## `/<username>/`
Shows the timeline of a user. Using the logged in user's username shows a public view of their page (so you cannot post)

## `/<username>/follow`
Adds logged in user as follower of `<username>`

## `/<username>/unfollow`
Removes logged in user from `<username>`'s follower list.

## `/add_message`
Registers a new messag for the user.

## `/login`
Logs the user in.

## `/register`
Registers the user.

## `/logout`
Logs the user out.
