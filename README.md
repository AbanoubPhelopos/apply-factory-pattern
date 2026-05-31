# Apply Factory Pattern

A demonstration project showcasing the **Factory Pattern** refactoring applied to a notification service. Compares the **before** (with code smells) and **after** (clean, maintainable) implementations.

---

## Table of Contents

- [Overview](#overview)
- [Project Structure](#project-structure)
- [The Problem: Code Smells](#the-problem-code-smells)
- [The Solution: Factory Pattern](#the-solution-factory-pattern)
- [Before vs After Comparison](#before-vs-after-comparison)
- [Why Factory Pattern is the Best Choice](#why-factory-pattern-is-the-best-choice)
- [Key Design Principles](#key-design-principles)
- [How to Run](#how-to-run)
- [Credits](#credits)

---

## Overview

This project demonstrates how the **Factory Pattern** solves common object creation and configuration problems in a notification service that supports multiple channels (Email, SMS, Slack, Teams).

### Before: NotificationService with Code Smells

- ❌ Construction in the wrong layer
- ❌ Config key leakage (magic strings)
- ❌ Duplicated construction logic
- ❌ Hard to test
- ❌ Violates Single Responsibility Principle
- ❌ Primitive obsession (string-based channels)
- ❌ Static side-effects

### After: Factory-Based Architecture

- ✅ Single Responsibility — each sender handles one channel
- ✅ Open/Closed — add new channels without modifying existing code
- ✅ Dependency Inversion — depends on abstractions
- ✅ Testable — easy to mock senders
- ✅ Type-safe — strongly-typed enum for channels
- ✅ No static side-effects — initialization at construction time

---

## Project Structure

```mermaid
graph TB
    subgraph "apply-factory-pattern"
        subgraph "before/Factory Pattern/Notification.Sending"
            B1[NotificationService.cs]
            B2[ThirdPartyStubs.cs]
            B3[Program.cs]
            B4[appsettings.json]
            B5[README.md]
        end

        subgraph "after/Factory Pattern/Notification.Sending"
            A1[NotificationService.cs]
            A2[NotificationChannel.cs]
            A3[NotificationServiceBefore.cs]
            A4[ThirdPartyStubs.cs]
            A5[Program.cs]
            A6[appsettings.json]
            A7[README.md]

            subgraph "Factory"
                F1[NotificationSenderFactory.cs]
                F2[EmailNotificationSender.cs]
                F3[SmsNotificationSender.cs]
                F4[SlackNotificationSender.cs]
                F5[TeamsNotificationSender.cs]

                subgraph "interfaces"
                    I1[INotificationSender.cs]
                    I2[INotificationSenderFactory.cs]
                end
            end
        end
    end
```

---

## The Problem: Code Smells

### Before Architecture

```mermaid
graph TB
    subgraph "NotificationService (God Class)"
        NS[NotificationService]
        NS -->|"new SmtpClient()"| SMTP[SmtpClient]
        NS -->|"TwilioClient.Init()"| TW[TwilioClient]
        NS -->|"new SlackApiClient()"| SL[SlackApiClient]
    end

    subgraph "Problems"
        P1[Config key leakage]
        P2[Duplicated construction]
        P3[Hard to test]
        P4[Violates SRP]
    end
```

### 8 Code Smells in the Before State

| # | Code Smell | Description |
|---|------------|-------------|
| 1 | **Construction in wrong layer** | `SmtpClient`, `TwilioClient`, `SlackApiClient` created inside service |
| 2 | **Config key leakage** | Magic strings like `"Smtp:Host"` scattered everywhere |
| 3 | **Duplicated construction** | SmtpClient setup copy-pasted in `SendAsync` and `SendBulkAsync` |
| 4 | **Hard to test** | No seam to replace SMTP with a fake |
| 5 | **Adding a channel = modifying service** | Every new channel requires changes to `NotificationService` |
| 6 | **Violates SRP** | Service knows how to send AND how to construct every client |
| 7 | **Primitive obsession** | Channel is `string`; typos like `"Emal"` compile fine |
| 8 | **Static side-effects** | `TwilioClient.Init()` called on every SMS send |

---

## The Solution: Factory Pattern

### After Architecture

```mermaid
graph TB
    subgraph "NotificationService (Clean)"
        NS[NotificationService]
    end

    subgraph "Factory Layer"
        Factory[INotificationSenderFactory]
    end

    subgraph "Sender Implementations"
        Email[EmailNotificationSender]
        SMS[SmsNotificationSender]
        Slack[SlackNotificationSender]
        Teams[TeamsNotificationSender]
    end

    NS -->|uses| Factory
    Factory -->|creates| Email
    Factory -->|creates| SMS
    Factory -->|creates| Slack
    Factory -->|creates| Teams
```

### Key Improvements

| # | Improvement | How |
|---|--------------|-----|
| 1 | **Construction moved to factory** | Service asks factory for sender |
| 2 | **No config key leakage** | Typed `IOptions<T>` settings classes |
| 3 | **No duplication** | Each sender encapsulates its own construction |
| 4 | **Easy to test** | Mock `INotificationSender` interface |
| 5 | **Open/Closed** | Add new sender class, no service changes |
| 6 | **SRP compliant** | Service, senders, and factory have distinct responsibilities |
| 7 | **Type-safe channels** | `NotificationChannel` enum catches typos at compile time |
| 8 | **No static side-effects** | `TwilioClient.Init()` called once at construction |

---

## Before vs After Comparison

### Code Comparison

```mermaid
graph LR
    subgraph "Before"
        B1["if channel == Email"]
        B2["new SmtpClient(...)"]
        B3["TwilioClient.Init() on every call"]
    end

    subgraph "After"
        A1["factory.CreateSender(channel)"]
        A2["sender.SendAsync(message)"]
        A3["Init() called once in constructor"]
    end
```

### Metrics Comparison

| Metric | Before | After |
|--------|--------|-------|
| **Cyclomatic Complexity** | High (branching on string) | Low (single factory call) |
| **Lines of Code (NotificationService)** | ~136 lines | ~25 lines |
| **Testability** | Difficult | Easy (mock interfaces) |
| **Extensibility** | Requires modification | Add new class |
| **Configuration** | Magic strings | Typed settings |

### Class Responsibilities

| Class | Before | After |
|-------|--------|-------|
| `NotificationService` | Sends + Constructs all clients | Sends only (orchestration) |
| `EmailNotificationSender` | N/A | Constructs + Sends Email |
| `SmsNotificationSender` | N/A | Constructs + Sends SMS |
| `SlackNotificationSender` | N/A | Constructs + Sends Slack |
| `TeamsNotificationSender` | N/A | Constructs + Sends Teams |
| `INotificationSenderFactory` | N/A | Creates senders |

---

## Why Factory Pattern is the Best Choice

### 1. Solves the Constructor Pollution Problem

The Factory Pattern **separates object creation from object use**. In the before state, `NotificationService` constructor took `IConfiguration` and had to know how to construct all clients. After refactoring:

```csharp
// Before: constructor polluted with config knowledge
public NotificationServiceBefore(IConfiguration config)
{
    _config = config; // Must pass config to build clients later
}

// After: constructor only needs what it uses directly
public NotificationService(INotificationSenderFactory factory)
{
    _factory = factory; // Asks factory for senders when needed
}
```

### 2. Enables Open/Closed Principle

```mermaid
flowchart TB
    subgraph "Adding Teams Channel"
        A[Create TeamsNotificationSender] --> B[Implement INotificationSender]
        B --> C[Register in DI]
        C --> D[Works immediately]
 end

    subgraph "Before"
        E[Modify NotificationService] --> F[Add if-else branch]
        F --> G[Test entire service]
    end
```

**Before**: Adding a channel required modifying `NotificationService` (violates OCP)
**After**: Adding a channel only requires creating a new sender class (follows OCP)

### 3. Makes Testing Trivial

```mermaid
classDiagram
    class INotificationSender {
        <<interface>>
        +Channel: NotificationChannel
        +SendAsync(message: NotificationMessage)
    }

    class FakeEmailSender {
        +Channel = Email
        +SendAsync(message) --> logs
    }

    class FakeSmsSender {
        +Channel = Sms
        +SendAsync(message) --> logs
    }

    FakeEmailSender ..|> INotificationSender
    FakeSmsSender ..|> INotificationSender
```

Replace any sender with a fake for unit testing — no network calls, no external dependencies.

### 4. Follows Dependency Inversion Principle

```mermaid
graph TB
    subgraph "Before"
        H[High-level: NotificationService]
        L[Low-level: SmtpClient, TwilioClient, SlackApiClient]
        H --> L
    end

    subgraph "After"
        HA[High-level: NotificationService]
        A[Abstraction: INotificationSender]
        L2[Low-level: EmailSender, SmsSender, ...]
        HA --> A
        A ..|> L2
    end
```

**Before**: High-level module depends on low-level modules
**After**: Both depend on abstractions

### 5. Why Not Other Patterns?

| Pattern | Verdict | Reason |
|---------|---------|--------|
| **Abstract Factory** | ❌ Overkill | We only need ONE factory interface, not families of related objects |
| **Builder** | ❌ Irrelevant | Builder helps with step-by-step construction of complex objects; our objects are simple |
| **Strategy** | ❌ Insufficient | Strategy provides interchangeable algorithms but doesn't solve object creation |
| **Service Locator** | ❌ Anti-pattern | Implicit dependency resolution makes testing harder |
| **Mediator** | ❌ Wrong problem | Doesn't address object creation complexity |

---

## Key Design Principles

### SOLID Principles Applied

| Principle | Before Violation | After Compliance |
|-----------|------------------|------------------|
| **S**ingle Responsibility | Service knows how to send AND construct | Each class has one reason to change |
| **O**pen/Closed | Adding channel = modify service | Add sender class, no modifications |
| **L**iskov Substitution | N/A | All senders substitutable via interface |
| **I**nterface Segregation | N/A | Small, focused `INotificationSender` interface |
| **D**ependency Inversion | Depends on concretions | Depends on `INotificationSender` abstraction |

### Dependency Injection Setup

```mermaid
flowchart LR
    subgraph "ServiceCollection"
        A["Configure<SmtpSettings>()"] --> B["AddSingleton<INotificationSender, EmailNotificationSender>()"]
        B --> C["AddSingleton<INotificationSender, SmsNotificationSender>()"]
        C --> D["AddSingleton<INotificationSender, SlackNotificationSender>()"]
        D --> E["AddSingleton<INotificationSender, TeamsNotificationSender>()"]
        E --> F["AddSingleton<INotificationSenderFactory, NotificationSenderFactory>()"]
        F --> G["AddScoped<NotificationService>()"]
    end
```

---

## How to Run

### Before (with code smells)

```bash
cd "before/Factory Pattern/Notification.Sending"
dotnet restore
dotnet run
```

### After (with Factory Pattern)

```bash
cd "after/Factory Pattern/Notification.Sending"
dotnet restore
dotnet run
```

---

## Credits

This project accompanies a video demonstration of Factory Pattern refactoring, highlighting:

- 8 common code smells in object creation
- How Factory Pattern solves each problem
- Comparison with alternative patterns
- Real-world DI integration with .NET

---

## See Also

- [Before README](./before/Factory%20Pattern/README.md) — Detailed before-state documentation
- [After README](./after/Factory%20Pattern/Notification.Sending/README.md) — Detailed after-state documentation
