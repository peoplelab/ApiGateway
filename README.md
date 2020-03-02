# ApiGateway
Api gateway for microservices.

A Gateway is a "bridge" between web applications (front end) and API server(s) and Authentication server(s) (back end).

(written in C#, .Net Core 3.0)


# 1 - Routes.json (Routing configuration file)
Dictionary of all routes handled by the gateway.

A Route is a complex object containing infos about "destination" API.

2 types of "main" entries in this file:
1) Routes: a list of generic routes;
2) Authentication Services: a particular entry defining a list of authentication services.

## 1.1 - Route
Routes is a list of generic routes.

A generic route is made of:
1) `Endpoint` : entry url of service API.
2) `Destination`: a "Destination" route object.

### 1.1.1 Destination
Destination is an object defining features of a service API. It's made of:
1) `Uri` : service API url,
2) `Method`: service API method (get, post, ...)
3) `Requires Authentication`: a boolean indicating if request must be authenticated before accessing service API.
4) `Format`: service API payload format (xml, json, ...)
5) `Login Service`: only for login service API, it defines the identifier of a login service (for the Authentication Service API)

## 1.2 - Authentication Service
Authentication Services is a list of authentication services.

An authentication service is made of:
1) `Uri` : authentication API service url (not the login service, but the validation one!)
2) `Login Service`: Login type/service identifier (linked on a "login" route in Routes).


