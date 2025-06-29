# Simple Vote App using ASP.NET Core

This project is meant as a simple demonstration of ASP.NET Core's SignalR, where clients get notified when new votes are created and subscribed votes are updated (consider it as watching a vote in real time).  
For learning purposes, the backend and frontend (web client) projects are separated.

## Packages used
All .NET projects here are targeting framework .NET 9.0

### Server (Backend)
Microsoft .NET Web Api SDK

- ASP.NET Core
- EF Core version 9.0.5
- EF Core SQLite version 9.05

### Console Client
- ASP.NET Core SignalR Client version 9.0.5

### Web Client
- Vue JS version 3.3.4
- Vue router version 4.5.1
- TailwindCSS version 3.3.2
