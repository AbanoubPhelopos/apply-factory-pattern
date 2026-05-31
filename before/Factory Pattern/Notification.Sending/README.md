# Notification.Sending — Before Factory Pattern

This folder contains the **before** snapshot of a notification service that demonstrates various code smells before applying the Factory Pattern refactoring.

---

## Table of Contents

- [Overview](#overview)
- [Architecture](#architecture)
- [Code Smells](#code-smells)
- [Supported Channels](#supported-channels)
- [Configuration](#configuration)
- [Class Diagram](#class-diagram)
- [Sequence Diagram: SendAsync](#sequence-diagram-sendasync)
- [How to Run](#how-to-run)

---

## Overview

The `NotificationService` class is responsible for sending notifications across multiple channels (Email, SMS, Slack). However, it suffers from several design issues that make it hard to maintain, test, and extend.

### Key Problems

1. **Construction in the wrong layer** — Third-party clients (`SmtpClient`, `TwilioClient`, `SlackApiClient`) are instantiated directly inside the service
2. **Config key leakage** — Magic strings like `"Smtp:Host"` are scattered throughout the code
3. **Duplicated construction logic** — SMTP client setup is copy-pasted between `SendAsync` and `SendBulkAsync`
4. **Hard to test** — No seam to replace real clients with fakes
5. **Adding a channel = modifying this class** — Every new channel requires changes to `NotificationService`
6. **Violates SRP** — The service knows both *how to send* and *how to construct* every client
7. **Primitive obsession** — Channel is a plain `string`; typos like `"Emal"` compile without complaint
8. **Static side-effects** — `TwilioClient.Init()` is called on every SMS send

---

## Architecture

### Current Structure

```mermaid
graph TB
    subgraph "NotificationService (The Problem)"
        NS[NotificationService]
        NS -->|"new SmtpClient()"| SMTP[SmtpClient<br/>System.Net.Mail]
        NS -->|"TwilioClient.Init()"| TW[TwilioClient<br/>Third-party stub]
        NS -->|"new SlackApiClient()"| SL[SlackApiClient<br/>Third-party stub]
 end

    subgraph "Configuration"
        CFG[appsettings.json]
 CFG -->|"Smtp:*"| NS
        CFG -->|"Twilio:*"| NS
        CFG -->|"Slack:*"| NS
    end

    subgraph "Caller"
        PG[Program.cs]
        PG --> NS
    end
```

### Class Relationships

```mermaid
classDiagram
    class NotificationService {
        +IConfiguration _config
        +SendAsync(channel, recipient, subject, body)
        +SendBulkAsync(requests)
    }

    class NotificationRequestBefore {
        +string Channel
        +string Recipient
        +string? Subject
        +string Body
    }

    class SmtpClient {
        +EnableSsl
        +Credentials
        +SendMailAsync(message)
    }

    class TwilioClient {
        +Init(accountSid, authToken)
    }

    class MessageResource {
        +CreateAsync(to, from, body)
    }

    class SlackApiClient {
        +Chat: SlackChatEndpoint
    }

    class SlackChatEndpoint {
        +PostMessage(message)
    }

    class Message {
        +Channel: string
        +Text: string
    }

    NotificationService --> NotificationRequestBefore
    NotificationService --> SmtpClient
    NotificationService --> TwilioClient
    NotificationService --> MessageResource
    NotificationService --> SlackApiClient
    NotificationService --> Message
```

---

## Code Smells

### 1. Construction in the Wrong Layer

```mermaid
graph LR
    A[NotificationService] -->|creates| B[SmtpClient]
    A -->|creates| C[TwilioClient]
    A -->|creates| D[SlackApiClient]
```

The service is responsible for both **business logic** (sending notifications) AND **object creation** (instantiating third-party clients). This violates the **Dependency Inversion Principle**.

### 2. Duplicated Construction Logic

```mermaid
flowchart LR
    subgraph "SendAsync"
        S1[Create SmtpClient] --> S2[Configure SSL/Credentials] --> S3[Send Mail]
    end
    subgraph "SendBulkAsync"
        B1[Create SmtpClient] --> B2[Configure SSL/Credentials] --> B3[Send Mail]
    end
```

The SMTP client construction code is **copy-pasted** between `SendAsync` and `SendBulkAsync`.

### 3. Primitive Obsession

```mermaid
graph LR
    "Emal" -.->|compiles| X[NotificationService]
    "SMS" -.->|compiles| X
    "Slak" -.->|compiles| X
```

The `channel` parameter is a plain `string`. Typos compile fine and fail at runtime.

### 4. Static Side-Effects

```mermaid
sequenceDiagram
    Caller->>NotificationService: SendAsync("SMS", ...)
    NotificationService->>TwilioClient: Init(accountSid, authToken)
    Note over TwilioClient: Static init called<br/>on EVERY SMS send
    NotificationService->>MessageResource: CreateAsync(...)
```

`TwilioClient.Init()` is called on **every** `SendAsync` call, which is inefficient and creates static global state.

---

## Supported Channels

| Channel | Implementation | Configuration Keys |
|---------|----------------|-------------------|
| **Email** | `SmtpClient` (System.Net.Mail) | `Smtp:Host`, `Smtp:Port`, `Smtp:Username`, `Smtp:Password`, `Smtp:FromAddress` |
| **SMS** | `TwilioClient` + `MessageResource` | `Twilio:AccountSid`, `Twilio:AuthToken`, `Twilio:FromNumber` |
| **Slack** | `SlackApiClient` | `Slack:BotToken` |

---

## Configuration

The `appsettings.json` file contains all required configuration:

```json
{
  "Smtp": {
    "Host": "localhost",
    "Port": 25,
    "Username": "demo",
    "Password": "demo",
    "FromAddress": "noreply@example.com"
  },
  "Twilio": {
    "AccountSid": "ACdemo00000000000000000000000000",
    "AuthToken": "demo-auth-token",
    "FromNumber": "+15550000000"
  },
  "Slack": {
    "BotToken": "xoxb-demo-bot-token"
  },
  "Teams": {
    "WebhookUrl": "https://webhook.example.com/teams/incoming"
  }
}
```

---

## Class Diagram

```mermaid
classDiagram
    namespace Notification.Sending {
        class NotificationService {
            -IConfiguration _config
            +SendAsync(channel: string, recipient: string, subject: string, body: string): Task
            +SendBulkAsync(requests: IEnumerable~NotificationRequestBefore~): Task
        }

        class NotificationRequestBefore {
            +string Channel
            +string Recipient
            +string? Subject
            +string Body
        }

        class ThirdPartyStubs {
            class TwilioClient {
                +Init(accountSid: string, authToken: string)
            }
            class PhoneNumber {
                +ToString(): string
            }
            class MessageResource {
                +CreateAsync(to: PhoneNumber, from: PhoneNumber, body: string): Task
            }
            class SlackApiClient {
                +Chat: SlackChatEndpoint
            }
            class SlackChatEndpoint {
                +PostMessage(message: Message): Task
            }
            class Message {
                +Channel: string
                +Text: string
            }
        }
    }

    NotificationService --> NotificationRequestBefore
    NotificationService --> ThirdPartyStubs.TwilioClient
    NotificationService --> ThirdPartyStubs.MessageResource
    NotificationService --> ThirdPartyStubs.SlackApiClient
    NotificationService --> ThirdPartyStubs.Message
```

---

## Sequence Diagram: SendAsync

### Email Flow

```mermaid
sequenceDiagram
    participant Caller
    participant NotificationService
    participant SmtpClient
    participant MailMessage

    Caller->>NotificationService: SendAsync("Email", "user@example.com", "Subject", "Body")
    NotificationService->>SmtpClient: new SmtpClient(host, port)
    NotificationService->>SmtpClient: EnableSsl = true
    NotificationService->>SmtpClient: Credentials = NetworkCredential(username, password)
    NotificationService->>MailMessage: new MailMessage(from, to, subject, body)
    NotificationService->>MailMessage: IsBodyHtml = true
    NotificationService->>SmtpClient: SendMailAsync(message)
    SmtpClient-->>NotificationService: Complete
    NotificationService-->>Caller: Task
```

### SMS Flow

```mermaid
sequenceDiagram
    participant Caller
    participant NotificationService
    participant TwilioClient
    participant MessageResource

    Caller->>NotificationService: SendAsync("SMS", "+1234567890", "", "Body")
    NotificationService->>TwilioClient: Init(accountSid, authToken)
    Note over TwilioClient: Static side-effect!
    NotificationService->>MessageResource: CreateAsync(to, from, body)
    MessageResource-->>NotificationService: Complete
    NotificationService-->>Caller: Task
```

### Slack Flow

```mermaid
sequenceDiagram
    participant Caller
    participant NotificationService
    participant SlackApiClient
    participant SlackChatEndpoint

    Caller->>NotificationService: SendAsync("Slack", "#channel", "", "Body")
    NotificationService->>SlackApiClient: new SlackApiClient(botToken)
    NotificationService->>SlackChatEndpoint: PostMessage(message)
    SlackChatEndpoint-->>NotificationService: Task
    NotificationService-->>Caller: Task
```

---

## How to Run

1. Navigate to the project directory:
   ```bash
   cd "before/Factory Pattern/Notification.Sending"
   ```

2. Restore dependencies:
   ```bash
   dotnet restore
   ```

3. Run the application:
   ```bash
   dotnet run
   ```

> **Note:** This is a demonstration project using **stub implementations** of third-party libraries. No real network calls are made. The stubs print to the console instead.

---

## Files

| File | Description |
|------|-------------|
| `NotificationService.cs` | Main service with code smells |
| `ThirdPartyStubs.cs` | Stub implementations for Twilio, Slack |
| `Program.cs` | Entry point |
| `appsettings.json` | Configuration file |
| `Notification.Sending.csproj` | Project file |

---

## Next Steps

See the companion folder (`after/`) for the **refactored version** that applies the **Factory Pattern** to solve these problems:

- Factory interfaces for client creation
- Dependency injection for testability
- Strongly-typed channel enums
- Eliminated duplication
- Open/Closed Principle compliance
