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

## Endpoints
All endpoints return an HTTP status code that reflects the result of the operation, e.g., `400` means the resource you are accessing a resource with invalid request body or parameter, and a ResponseObject which has the following structure:
```
{
  "message": "",
  "result": {
    **ObjectResult**
  }
}
```
- The message field will only have a non-empty value if the operation is not successful, and serves as a meaningful error message. For example, when receiving a 404 Not Found result, you might receive a message that indicates what resource you were accessing was not found.
- The result field will be populated with an object as a result of the operation. For example, if you are accessing the `POST /votes` endpoint successfully, you will receive the detailed created vote in this result field. Another one, if you are accessing an endpoint that results in a list of objects, e.g, `GET /votes` successfully, you will receive the list of detailed vote objects.
- In some endpoints, most likely in endpoints with the DELETE method, the result field will be populated with a null value regardless successful request
- Request that results in a 400 Bad Request status code and has a validation error message, the result will be populated with an [ValidationErrors](#validation-errors)

Base URL `https://localhost:7000`.  

With the above in mind, here are the endpoints provided by the Server.
|        **Path**       | **Method** |     **Status Codes**    |               **Authorization**               |                           **Request Data**                          |                 **Object Result**                 |
|:----------------------|:----------:|:-----------------------:|:---------------------------------------------:|:-------------------------------------------------------------------:|:-------------------------------------------------:|
| /register             | POST       | 200, 400, 500           | -                                             |         JSON body [CreateUserRequest](#create-user-request)         |           [UserResponse](#user-response)          |
| /login                | POST       | 200, 400, 404           | -                                             |          JSON body [LoginUserRequest](#login-user-request)          |           [UserResponse](#user-response)          |
| /logout               | GET        | 200, 401                | Cookies                                       |                                  -                                  |                        null                          |
|          ===          |     ===    |           ===           |                      ===                      |                                 ===                                 |                        ===                        |
| /users                |            |                         |                                               |                                                                     |                                                   |
| /{id}                 | GET        | 200, 400, 401, 404      | Cookies                                       |                                  -                                  |           [UserResponse](#user-response)          |
| /roles                | GET        | 200, 400, 401, 404      | Cookies                                       |                                  -                                  |  List of [RoleResponse](#role-response)  |
| /vote-inputs          | GET        | 200, 401, 404,          | Cookies                                       |                                  -                                  | List of [VoteInputResponse](#vote-input-response) |
|          ===          |     ===    |           ===           |                      ===                      |                                 ===                                 |                        ===                        |
| /roles                |            |                         |                                               |                                                                     |                                                   |
| /                     | POST       | 200, 400, 409, 500      | Cookies, admin Role                           |         JSON body [CreateRoleRequest](#create-role-request)         |           [RoleResponse](#role-response)          |
| /                     | GET        | 200                     | Cookies, admin Role                           |                                  -                                  |       List of [RoleResponse](#role-response)      |
| /{role}               | GET        | 200, 400, 404           | Cookies, admin Role                           |                                  -                                  |           [RoleResponse](#role-response)          |
| /{role}               | PUT        | 200, 400, 404, 409, 500 | Cookies, admin Role                           |         JSON body [UpdateRoleRequest](#update-role-request)         |           [RoleResponse](#role-response)          |
| /{role}               | DELETE     | 200, 400, 404, 500      | Cookies, admin Role                           |                                  -                                  |                        null                       |
| /{role}/assign/{user} | POST       | 200, 400, 404, 409, 500 | Cookies, admin Role                           |                                  -                                  |      [UserRoleResponse](#user-role-response)      |
| /{role}/remove/{user} | POST       | 200, 400, 404, 409, 500 | Cookies, admin Role                           |                                  -                                  |                        null                       |
|          ===          |     ===    |           ===           |                      ===                      |                                 ===                                 |                        ===                        |
| /votes                |            |                         |                                               |                                                                     |                                                   |
| /                     | GET        | 200, 400, 401           | Cookies                                       |                                  -                                  |       List of [VoteResponse](#vote-response)      |
| /{id}                 | GET        | 200, 400, 404           | Cookies                                       |                                  -                                  |           [VoteResponse](#vote-response)          |
| /filter-options       | GET        | 200                     | Cookies                                       |                                  -                                  |    [VoteFilterResponse](#vote-filter-response)    |
| /inputs/users/{user}  | GET        | 200, 400, 404           | Cookies, admin Role or voteinspector Role     |                                  -                                  | List of [VoteInputResponse](#vote-input-response) |
| /can-manage           | GET        | 200                     | Cookies, admin Role or voteadministrator Role |                                  -                                  |                        null                          |
| /                     | POST       | 201, 400, 500           | Cookies, admin Role or voteadministrator Role |         JSON body [CreateVoteRequest](#create-vote-request)         |           [VoteResponse](#vote-response)          |
| /inputs               | POST       | 200, 400, 404, 409, 500 | Cookies                                       |         Query param [InputVoteRequest](#input-vote-request)         |     [VoteInputResponse](#vote-input-response)     |
| /inputs/queue         | POST       | 200, 400, 404, 409, 500 | Cookies                                       |         Query param [InputVoteRequest](#input-vote-request)         |     [VoteQueueResponse](#vote-queue-response)     |
| /{id}                 | PUT        | 200, 400, 404, 500      | Cookies, admin Role or voteadministrator Role | Query param id, JSON body [UpdateVoteRequest](#update-vote-request) |           [VoteResponse](#vote-response)          |
| /{id}                 | DELETE     | 200, 400, 404, 500      | Cookies, admin Role or voteadministrator Role |                            Query param id                           |                        null                       |

For complete lists, you can access the `/openapi/spec-v1.json` endpoint

### Validation Errors
```
"validationErrors": {
    "title": [
        "Title length cannot be less than 3 characters"
    ],
    "subjects": [
        "Subject count cannot be less than 2"
    ],
    "maximumCount": [
        "Maximum count cannot be less than subject count (1)"
    ]
}
```

### Create User Request
```
{
    "email": string,
    "password": string,
    "firstName": string | null,
    "lastName": string | null,
}
```

### Login User Request
```
{
    "email": string,
    "password": string,
    "firstName": string | null,
    "lastName": string | null,
}
```

### Create Role Request
```
{
    "name": string,
    "description": string | null
}
```

### Update Role Request
```
{
    "name": string,
    "description": string | null
}
```

### Create Vote Request
```
{
    "title": string,
    "subjects": [
        string
    ],
    "duration": int | null,
    "maximumCount": int | null,
}
```

### Input Vote Request
Query param
1. voteId: string, required
2. subjectId: int, required

### Update Vote Request
Query param
1. id: string, required

```
{
    "title": string,
    "subjects": [
        string
    ],
    "duration": int | null,
    "maximumCount": int | null,
}
```

### User Response
```
{
    "id": "3701f7ac-bcc6-4e74-b127-2107a68aa34a",
    "email": "creator@gmail.com",
    "firstName": "The",
    "lastName": "Creator"
}
```

### Role Response
```
{
    "id": "01973a1b-f35d-7c18-99a2-b77ea19275da",
    "name": "voteadmin",
    "normalizedName": "VOTEADMIN",
    "description": "Handles all vote creations, updates, and deletions",
    "createdTime": "2025-06-04T08:43:19.7730678Z"
}
```

### User Role Response
```
{
    "id": "0197bfe1-366d-7b07-9b0b-d7612de417ff",
    "userId": "fdfb6fc6-c6c0-4049-a1d2-2af1413c486f",
    "userEmail": "creator2@gmail.com",
    "roleId": "01973a1c-6138-7182-beb0-a4e5a947e617",
    "roleName": "voteinspector"
}
```

### Vote Response
```
{
    "id": "3713139a-505f-4250-83ee-030d4d96b1f8",
    "title": "Vote Title3232das",
    "subjects": [
    {
      "id": 1144,
      "name": "eqwe",
      "voteCount": 1
    },
    {
      "id": 1145,
      "name": "1e21e",
      "voteCount": 0
    }
    ],
    "currentTotalCount": 1,
    "createdTime": "2025-06-19T09:18:25.784447Z",
    "expiredTime": null,
    "maximumCount": null,
    "creatorId": "3701f7ac-bcc6-4e74-b127-2107a68aa34a"
}
```

### Vote Filter Response
```
{
    "sortBy": {
        "ttl": {
            "normalizedName": "Title"
        },
        "cdt": {
            "normalizedName": "Created Time"
        },
        "mxc": {
            "normalizedName": "Maximum Count"
        },
        "exp": {
            "normalizedName": "Expired Time"
        }
    },
    "sortOrder": {
        "asc": {
            "normalizedName": "Ascending"
        },
        "desc": {
            "normalizedName": "Descending"
        }
    },
    "search": {
        "ttl": {
            "normalizedName": "Title"
        },
        "ctr": {
            "normalizedName": "Creator"
        }
    }
}
```

### Vote Input Response
```
{
    "id": 185397,
    "subjectId": 1165,
    "subjectName": "dasdas",
    "voteId": "93054c31-eb17-414e-9f37-752f83e2d411",
    "voteTitle": "Vodddddddddd",
    "inputTime": "2025-06-30T07:41:17.4033202Z"
  }
```

### Vote Queue Response
```
{
    "voteId": "36532f8c-d4b3-4cd9-95be-e6b253043adc",
    "subjectId": "1136",
    "voterId": "3701f7ac-bcc6-4e74-b127-2107a68aa34a",
    "inputTime": "2025-06-30T07:42:13.3093378Z",
    "processedTime": null,
    "status": "Processing",
    "statusDetail": "Vote input is being processed"
}
```

The "status" field currently will always have a value of "Processing" since the queue is not fully implemented yet (only an in-memory queue, not stored in the DB yet).
