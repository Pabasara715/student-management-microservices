# This is the fixed Dockerfile for any .NET Service

# Declare build arguments at the top
ARG SERVICE_FOLDER
ARG CSPROJ_NAME  
ARG DLL_NAME

# Stage 1: Build the application
FROM studentsystem/dotnet-sdk-base:1.0 AS build

# Re-declare ARGs after FROM to make them available in this stage
ARG SERVICE_FOLDER
ARG CSPROJ_NAME
ARG DLL_NAME

WORKDIR /src

# 1. Copy the solution file
COPY *.sln .

# 2. Copy all project files
COPY AuditlogService/AuditlogService.csproj ./AuditlogService/
COPY CourseManagementAPI/CourseManagementAPI.csproj ./CourseManagementAPI/
COPY EnrollmentEventHandler/EnrollmentEventHandler.csproj ./EnrollmentEventHandler/
COPY EnrollmentManagementAPI/EnrollmentManagementAPI.csproj ./EnrollmentManagementAPI/
COPY NotificationService/NotificationService.csproj ./NotificationService/
COPY StudentManagementAPI/StudentManagementAPI.csproj ./StudentManagementAPI/
COPY TimeService/TimeService.csproj ./TimeService/
COPY TuitionService/TuitionService.csproj ./TuitionService/
COPY WebApp/WebApp.csproj ./WebApp/
COPY Infrastructure.Messaging/Infrastructure.Messaging.csproj ./Infrastructure.Messaging/

# 3. Restore dependencies for the ENTIRE solution
RUN dotnet restore "StudentSystem.sln"

# 4. Copy the rest of the source code
COPY . .

# 5. Publish the specific project - FIXED: Use shell form to expand variables
RUN dotnet publish "$SERVICE_FOLDER/$CSPROJ_NAME" -c Release -o /app/out

# Stage 2: Create the final runtime image
FROM studentsystem/dotnet-runtime-base:1.0

# Re-declare ARG for the runtime stage
ARG DLL_NAME

WORKDIR /app
COPY --from=build /app/out .

# Use shell form for ENTRYPOINT to expand the variable
ENTRYPOINT sh -c "dotnet $DLL_NAME"