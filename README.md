# vanderstack.api
VanderStack Api Vision
 
The VanderStack Api will be a dotnet core console application intended to run on azure in a docker container behind a load balancer.
 
The Api will utilize event sourcing. Events will be persisted to the data store prior to being projected into denormalized presentation models. Presentation models contain the exact shape and data payload that is required to fulfill a UI request, thus there should be 1 model for each query. Each model will have an event mapping configuration which defines the events which update the model and how they are applied.
 
The Api will support customization of ALL feature implementations through the use of the SimpleInjector Dependency Injection Package registrations. This will enable us to ship intelligent defaults and use separate packages to override those defaults for custom configurations. Further, all required dependencies will be encapsulated within the Api.
 
The Api will support launching microservices which extend the core api.
These services will initially listen on a port and forward requests to a request handler
This functionality will eventually be extended to enable ASP.NET / WebAPI / MVC / WCF
 
The Api development process will be driven by Function and Integration testing. Once sufficient infrastructure is complete testing will be done by ensuring logs are written with the correct output. Defining log output as test criteria allows for the highest quality debugging experience because well formatted logging results will provide information about where a problem exists.