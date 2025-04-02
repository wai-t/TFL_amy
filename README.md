# TfL Journey Planner

A small web app using **ASP.NET Core** and **React** that works with the **Transport for London (TfL) API**.  
It shows live tube statuses and helps plan journeys between tube stations.

---

## Features

- View current status of all TfL tube lines (e.g. delays, suspensions)
- Plan a journey by selecting a departure and arrival tube station
- Autocomplete for station names while typing
- Local Redis caching
- Ongoing code improvements and refactoring
- Unit tests will be added soon

---

## Tech Stack

- **Backend**: ASP.NET Core (.NET 8), C#, Redis, TfL API
- **Frontend**: React
- **Secrets**: Stored securely using .NET User Secrets during development

---

## Getting Started

### Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/)
- [Node.js and npm](https://nodejs.org/)
- [Redis](https://redis.io/) running locally 
- TfL API `appId` and `appKey` 

---

## Backend Setup

1. Go to the backend folder:

```bash
cd backend
```
2. Set up your development secrets:

```
dotnet user-secrets init
dotnet user-secrets set "ConnectionStrings:Redis" "localhost:6379,abortConnect=false"
dotnet user-secrets set "AppSettings:baseUrl" "https://api.tfl.gov.uk/"
dotnet user-secrets set "AppSettings:appKey" "cb52c92815b94cabb22449624d95e007"
dotnet user-secrets set "AppSettings:appId" "123"
```

## Frontend Setup
1. Navigate to the frontend folder:
```
cd frontend
```

2. Install dependencies
```
npm install

```

3. Start the React development server:
```
npm start
```

## Notes
- Redis is used for basic caching (can be customized or removed)

- Secrets are stored using .NET User Secrets for development

- Backend uses an AppSettings class to bind configuration

- Journey and stop point logic is being refactored into separate services

## To Do
- Extract stop point logic into its own service
  
- Refactor frontend components and state handling
  
- Add unit tests






