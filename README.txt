This was performed on a Windows box.

A. Setup

- Install local SQL Server named instance (localhost\SqlExpress) with SQL authentication and sa user/password. 
- Set sa password in ConnectionStrings section of appsettings.json
- Install .NET Core 3
- Install EntityFrameworkCore 3 via the following command:

    dotnet tool install --global dotnet-ef --version 3.0.0

- From source directory, run the following: (can also run build.bat)

  dotnet restore
  dotnet build
  dotnet run --init true

B. Running/Testing via console

- "dotnet run" to start REST API
- URL is https://localhost:5001/MPI
- "dotnet run test" to run unit test code, only works after 
   resetting db via "dotnet run --init true"

C. Testing via Postman

-  Import test1.postman_collection.json to test the REST API.

