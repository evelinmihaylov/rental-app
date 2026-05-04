# Rental App

## Project Overview
Rental App is a .NET MAUI application that allows users to browse items, request rentals, manage incoming and outgoing rentals, leave reviews, and find items near their current location. The project follows an MVVM architecture and uses a shared REST API for most runtime data access.

## Setup Instructions

The project requires the .NET SDK, the .NET MAUI workload, and a supported development environment such as Visual Studio. The application uses the shared REST API by default for runtime data access. 

Requirements
.NET SDK
.NET MAUI workload
Visual Studio / Visual Studio Code
Docker is required when using the provided container-based development environment 

When using the provided Visual Studio / container-based environment with Docker connected:

## How to Run the Application
Open the project in Visual Studio or Visual Studio Code.
Clean and build the project:
dotnet clean
dotnet build -c Debug
Start the Android emulator:
emulator -avd <EMULATOR_NAME>
If needed, remove the previous installed version of the app:
adb uninstall com.companyname.starterapp
Install the newly built APK on the emulator:
adb install -r <PATH_TO_APK_FILE>
Open the application from the emulator.

## How to Run Tests

The automated tests are written using xUnit and are located in the `StarterApp.Tests` project.

In practice, tests were run both from the command line in the development container and through the Visual Studio test runner using the Run with Coverage option.

### Command line
From the development container, run:

```bash
cd /workspace/StarterApp.Tests
dotnet test StarterApp.Tests.csproj -c Debug --settings ../.runsettings --collect:"XPlat Code Coverage"

### API endpoint documentation link
Shared API documentation and interactive endpoint reference:
https://set09102-api.b-davison.workers.dev/

Additional course API reference:
https://moodle.napier.ac.uk/mod/page/view.php?id=2924259



### Architecture Overview
The application follows the MVVM (Model-View-ViewModel) pattern. Views are implemented in XAML and are responsible for the user interface, while ViewModels manage UI state, commands, and interaction logic. Business rules are handled in the service layer, and repositories encapsulate communication with the shared REST API. Shared model classes are stored in `StarterApp.Database` and are used across the application for strongly typed data handling.

Main application flow:
Views -> ViewModels -> Services -> Repositories -> REST API


Main Features
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
