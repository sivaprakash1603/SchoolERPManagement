# Use the .NET 8 SDK image for building
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy project files and restore dependencies
# We copy the solution file (if available) and all csproj files to utilize Docker cache layer for NuGet restore
COPY ["SchoolERPManagementAPI/SchoolERPManagementAPI.csproj", "SchoolERPManagementAPI/"]
COPY ["SchoolERPManagementBLLibrary/SchoolERPManagementBLLibrary.csproj", "SchoolERPManagementBLLibrary/"]
COPY ["SchoolERPManagementDALLibrary/SchoolERPManagementDALLibrary.csproj", "SchoolERPManagementDALLibrary/"]
COPY ["SchoolERPManagementModelLibrary/SchoolERPManagementModelLibrary.csproj", "SchoolERPManagementModelLibrary/"]

# Restore the API project
RUN dotnet restore "SchoolERPManagementAPI/SchoolERPManagementAPI.csproj"

# Copy the remaining source code
COPY . .

# Build and publish the application
WORKDIR "/src/SchoolERPManagementAPI"
RUN dotnet publish "SchoolERPManagementAPI.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Use the ASP.NET Core runtime image for the final stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
EXPOSE 8080

# Copy the published output from the build stage
COPY --from=build /app/publish .

# Set the entry point to run the API
ENTRYPOINT ["dotnet", "SchoolERPManagementAPI.dll"]
