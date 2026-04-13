# Solution Architecture

## Project Structure

```
BillSplitting.sln
├── BillSplittingUI/                 (Console App)
├── BillSplitting.Application/       (Class Library)
├── BillSplitting.Domain/            (Class Library)
└── BillSplitting.Data/              (Class Library)
```

## Layered Architecture

**Four-layer design** with dependency flow:

```
UI (Console App)
  |
Application Layer (routing, business logic)
  |
Domain Layer (entities, business rules)
  |
Data Layer (persistence, repositories)
```

## Project References

- **BillSplittingUI** → BillSplitting.Application, BillSplitting.Data
- **BillSplitting.Application** → BillSplitting.Domain, BillSplitting.Data
- **BillSplitting.Data** → BillSplitting.Domain

## Rationale

- **Domain**: Core entities and business logic, no dependencies on other layers
- **Data**: Implements repository pattern to abstract persistence
- **Application**: Orchestrates business operations using Domain and Data
- **UI**: Console interface consuming Application layer services
