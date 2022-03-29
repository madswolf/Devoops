So for the bug hunt, we have set up a new project instance along with instance 
of elastic search and kibana. The API instance was connected to our production 
database, so that we could use the data present there. To test different requests, 
we have used a postman with vaiours headers, query strings, request methods and bodies. 
We were checking each endpoint separately with all possible combinations of values, and
after receiving the output we were checking the logs for something unusual. What caught
our attention was the fact, that /fllws endpoint was always returning only 1 folowee, no
matter the '?no' parameter, after inspecting the logs, we realized that the query issued 
from the application was faulty, as the LIMIT parameter was set to 1 always, and that's 
what we had to fix.