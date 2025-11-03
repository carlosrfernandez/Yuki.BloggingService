# Yuki Blogging Service Tech test for Yuki.

## Requirements:

This is simple blogging service, the request is summaried as follows:

- Expose POST /blogs to create a new blog post.
- Expose GET /blogs/{id} to get a specific blog post by its ID.
- Optionally allow GET /blogs/{id} to return author information if requested.
- Allow different response formats (JSON and XML) based on client preference.
- 90% test coverage
- CQRS and Event Sourcing patterns valued.

## Running the Application

1. Clone the repository:
   ```bash
   git clone https://github.com/carlosrfernandez/Yuki.BloggingService.git
   cd Yuki.BloggingService
   ```
2. Install dependencies:
   ```bash
   dotnet restore
    ```
3. Run the application:
4. ```bash
   dotnet run --project Yuki.BloggingService.Api
   ```
   
5.Use Swagger UI to interact with the API:
   Open your browser and navigate to `http://localhost:5000/swagger` (or the port specified in your configuration).

### Running in docker

There is a compose file to run the app. 

1. Build and run the Docker containers:
   ```bash
   docker-compose up --build
   ```
2. Access the application at `http://localhost:8080/swagger`.

### Architecture Overview

This is a CQRS application. Since this application is for for a technical assessment, I have simplified some parts of the architecture for brevity.
For example, I have used in-memory storage instead of a database for event sourcing and read models.
The main components are:
- ** Domain Layer**: Contains the core business logic, including entities, value objects, aggregates, and domain services.
- ** Application Layer**: Contains commands, queries, and their handlers. It orchestrates the flow of data between the domain and infrastructure layers.
- ** Infrastructure Layer**: Contains implementations for data access, event storage, and other external services.
- ** API Layer**: Exposes RESTful endpoints for clients to interact with the blogging service.

There are two logical sides to this application, visible on the controllers: the read, and write sides.

- **Write Side**: Handles commands to:
  - Create authors and authorize them to "post"
  - Draft new blog posts and "Publish" them
  
- **Read Side**: Subscribes to events from the event store to update read models that are optimized for querying.
  - Retrieve blog posts by ID
  - Optionally include author information based on client request

# Creative freedom

Some *freedom* was given in the implementation details.

I tried to model a real domain workflow, where we have a system that allows authors to register and once their account is
authorized, they can create and publish blog posts... 

Additionally, a blog post isn't simply "published", it is first drafted, and then published. This allows for a more realistic representation
of what a real business workflow might look like. 

This was done to demonstrate an understanding of domain-driven design principles and to showcase the ability to model complex business processes.

There are other concepts around event sourcing and CQRS that could be further expanded upon, such as:
- Implementing snapshots for event sourcing to optimize performance.
- Adding Process Managers or Sagas for handling long-running business processes.

This was left out on purpose, as the time it would take to implement exceed the scope of this technical assessment.

## Testing

To run the tests, navigate to the test project directory and execute the following command:
```bash
dotnet test
```

This exercise has 98% test coverage.

