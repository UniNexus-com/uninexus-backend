UniNexus Backend: Business Rules & Validation

This document outlines the core business logic and safety constraints that must be enforced within the CleanArchitecture.Application layer (Features/Handlers) and CleanArchitecture.Domain entities.
1. Safety & Inventory Management

    Global Inventory Lock: A student is automatically blocked from borrowing any asset from ANY club if they have a single overdue item.

    Atomic Enforcement: The borrowing block must be applied across all clubs simultaneously with no partial states allowed.

    "Take & Drop" Workflow: Equipment lending is handled via QR codes; the system must verify the "Available" status before changing it to "On Loan".

    Gradual Penalties: Minor items trigger a 3-day block, while major assets can trigger an indefinite block across all clubs.

2. Financial Integrity & Accountability

    Digital Signature Requirement: No club wallet balance can be modified without a verified digital signature from an SKS Administrator on the transaction request.

    Immutable Ledger: All financial movements (Approved, Rejected, or Rolled-back) must be recorded in an immutable, time-stamped transaction ledger.

    Rollback Mechanism: If an approved event is canceled, the system must provide a mechanism to revert the allocated funds to the club wallet.

3. Leadership & Membership Constraints

    Leadership Monopoly Prevention: A student may hold the primary "Leader" role in at most one club at any given time.

    Context-Aware Authorization: The system must handle users who are leaders in one club but regular members in another, preventing permission leaks.

    Server-Side Verification: While the UI may hide actions, the API must independently verify role-based permissions on every request.

4. Gamification & Attendance

    One-Point-Per-Event Rule: Attendance points are awarded strictly once per unique event per student to prevent score inflation.

    Location-Fencing: The system must utilize GPS coordinates to verify that a student is physically within a 50m radius of the event venue during a QR check-in.

    Privacy Compliance (KVKK): Raw individual movement tracks must not be stored; coordinates are obfuscated and aggregated for the campus heatmap.

5. Performance Standards

    API Latency: All REST API endpoints must respond in under 2 seconds.

    Real-Time Latency: SignalR broadcast messages (urgent announcements) must reach authenticated users in under 500ms.
