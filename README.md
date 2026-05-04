# Rental App

## Project Overview
Rental App is a .NET MAUI application that allows users to browse items, request rentals, manage incoming and outgoing rentals, leave reviews, and find items near their current location. The project follows an MVVM architecture and uses a shared REST API for most runtime data access.

### Main Features
- User registration and login
- Browse item listings and item details
- Create rental requests
- Approve and reject rental requests
- Manage rental state transitions
- Leave and view reviews
- Find nearby items using location
- Profile and logout functionality

## Project Structure
```text
RentalApp/
├── .devcontainer/
├── setup/
├── StarterApp/                # .NET MAUI application
├── StarterApp.Database/       # shared models and repositories
├── StarterApp.Migrations/     # EF Core migrations
├── StarterApp.Tests/          # xUnit test project
├── docker-compose.yml         # local PostgreSQL container
├── Dockerfile
├── .runsettings               # test coverage configuration
├── README.md
└── StarterApp.sln
