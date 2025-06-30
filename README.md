# Simple Vote App using ASP.NET Core

This project is meant as a simple demonstration of ASP.NET Core's SignalR, where clients get notified when new votes are created and subscribed votes are updated (consider it as watching a vote in real time).  
For learning purposes, the backend and frontend (web client) projects are separated.

## Features
- CRUD Users (accessible through API endpoints only)
- CRUD Roles (accessible through API endpoints only)
- CRUD Votes (accessible through API endpoints and web client)
- Vote filtering (in the manage vote page)
- Basic User authentication using email, password, and cookies
- Role-based authorization
- Request validations

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

## Installation
1. Clone this repository.
2. Make sure you have .NET 9.0 installed, if you don't, [Download Dotnet](https://dotnet.microsoft.com/en-us/download/dotnet/9.0).
3. Set up the Server Project:
    1. Go to the server project `cd .\Server\`.
    2. Install the dependencies using command `dotnet restore` in your terminal.
    3. Run the database migrations using command `dotnet ef database update` in your terminal.
4. Set up the Console Client Project:
    1. Go to the console client project `cd .\ConsoleClient\`.
    2. Install the dependencies using command `dotnet restore` in your terminal.
5. Set up the Web Client Project:
    1. Go to the Web Client project `cd .\WebClient\`/.
    2. Make sure you have Node and NPM installed, if you don't, [Download Node](https://nodejs.org/en/download).
    3. Install the dependencies using command `npm install` in your terminal.
6. Set up the Flooder Project:
    1. Go to the Flooder project `cd .\Flooder\`.
    2. Install the dependencies using command `dotnet restore` in your terminal.

## Usage
1. Run the Server:
    1. Go to the server project `cd .\Server\`.
    2. Run the server using command `dotnet run` in your terminal.
    3. On the first run, the server will automatically create a Creator user with email `creator@gmail.com`, password `Password123@`, and role admin, which has all authorities. This is the user that can be used for creating new votes.
    4. The server is running at https://localhost:7000/.
2. Run the Web Client:
    1. Go to the web Client project `cd .\WebClient\`.
    2. Run the client using command `npm run dev` in your terminal.
    3. Open the web client URL at https://localhost:3000 in your browser.
    4. Log in using the Creator user credentials.
    5. Open the web client in a new incognito tab
    6. Register a new user in that tab and log in using the registered data.
3. After logging in with the new user, you will be on the main vote page, where you can view a list of votes. If the list is empty, create votes by accessing the manage-votes page in the Creator account.
4. Notice that every time you create a new vote, a new vote item will be added to the vote list automatically in the normal user account. This is where SignalR functions.
5. You can click one of the vote items to view the details of the vote, while also subscribing to the vote, which will make your web client receive the real-time update of that vote item. You can open the web client from another browser and log in using another account, then input a vote while observing the vote item in the other web client. Or you can just update a vote's detail, e.g, its title from the manage-votes page and observe the changing vote item from another account.
6. The console client is basically just like the web client, only it prints the text-only vote notifications on the console.

### Simulate real-time voting
Simulate a real-time voting by sending concurrent vote inputs from multiple users at the same time.

1. Ensure the server is running.
2. Run the Flooder:
    1. Go to the server project `cd .\Flooder\`.
    2. Run the server using command `dotnet run` in your terminal.
    3. Follow the instructions in the console.
    4. Choose the input vote endpoint type: Normal or Queue.
        1. Normal: Handle all concurrent vote inputs at the same time and implement a retry mechanism in case of concurrency error. Using this vote type will very likely result in some of the vote inputs not being processed properly, even after the retry mechanism. For example, if you send 10000 concurrent vote inputs, the successful vote inputs will likely be in around 8000 to 9500+ inputs, with very little chance that all 10000 inputs are processed.
        2. Queue: Queue all vote inputs, and have the vote queue processor process the vote inputs in the background, ensuring no concurrency error. You send 10000 concurrent vote inputs, all those 10000 inputs will be processed as expected
    5. Let the flooder do its job and wait until it finishes.
    6. Repeat again if you want.
