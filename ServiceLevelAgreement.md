# Service Level Agreement (SLA)

During the Term of agreement under which Group E has agreed to provide the Minitwit service to Customer, Minitwit will provide a Monthly Uptime Percentage to Customer as follows (the "Service Level Objective" or "SLO"):

| Covered Service | Monthly Uptime Percentage |
| --------------- | ------------------------- |
| Minitwit        | >= 98%                    |

If Group E does not meet the SLO, Customer can send an inquiry formulated as a GitHub Issue with relevant information, and an apology will be issued. If Customer asks nicely, which is to be deemed by Group E, Customer may receive an in-person apology and a handshake if both parties agree to use hand sanitizer.

## Definitions

The following definitions apply to the SLA:

**"Back-off requirement"** means, when an error occurs, the Customer is responsible for waiting a period of time before issuing a new request. After the first error, there is a minimum back-off interval of 2 seconds and for each consecutive error, the back-off interval increases exponentially up to 64 seconds.

**"Valid Requests"** are requests that conform to the Documentation, and that would normally result in a non-error response.

**"Downtime"** means more than a 5% Error Rate. Downtime is measured based on server side Error Rate.

**"Downtime Period"** means a period of one or more consecutive minutes of Downtime. Partial minutes or Intermittent Downtime for a period of less than one minute will not be counted towards any Downtime Periods.

**"Error Rate"** means the number of Valid Requests that result in a response with HTTP Status 500 and Code "Internal Error" divided by the total number of Valid Requests during that period. Repeated identical requests do not count towards the Error Rate unless they conform to the Back-off Requirements.

**"Inquiry response time"** denotes the time period that a client can expect to receive a response within, and is shown below:

| Day                     | Time period  |
| ----------------------- | ------------ |
| Monday through Thursday | 48 hours     |
| Friday through Sunday   | 72 hours     |
| National Holiday        | No guarantee |

## SLA Exclusions

The SLA does not apply to any:
* Any feature or service designated Alpha or Beta or otherwise not intended for release.
* Any feature or service excluded from the SLA.
* Errors: 
  * Caused by factors outside of Group E’s reasonable control.
  * That resulted from Customer’s software or hardware, third party software or hardware, or any combination hereof.
  * That resulted from abuses or other behaviors that violates the Agreement.
  * That resulted from quotas applied by the system.
  * Caused by Customer not adhering to the documentation, including but not limited to invalid requests, unauthorized or unrecognized users, or inaccessible data.