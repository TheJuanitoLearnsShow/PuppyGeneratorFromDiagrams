﻿
```mermaid
sequenceDiagram
    participant api as External Caller:IExternalCaller
    participant o as Other Flow Orchestrator:IOrchestrator
    participant a as Alice the great:IAlice
    participant b as Bob: IB ob
    api->>o: Initiate Flow 2
    o->>b: Hi Bob
    b-->>o: Greeting (greetingResult)
    o->>a: Hi Alice (greetingResult)
    a-->>o: Greeting
    o->>a: Bye
    a-->>o: Good Bye
```