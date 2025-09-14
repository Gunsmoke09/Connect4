## Requirements
.NET 8 SDK

## To run

```bash
dotnet clean
dotnet run
```

## Test Mode
```bash 
dotnet run -- --test o1,o2,o1,o3,o1,o3
```

- Provide a comma-separated sequence of moves after the test command to run test
- invalid moves are skipped in test mode

## Features
- A connectN game where you need to connect [rows X cols X 0.1] discs to win
- It has 3 discs
  - Ordinary disc (used like O1, O2, 03)
  - Boring disc (used like B1, B2, b3)
  - Magnetic disc (used like M1, M2, m3)
- It has two modes 1. human vs human and 2. computer vs human
- It can save your game and load your last saved game

---
Author- Anant Srivastava \
Student ID- 12138177
---
