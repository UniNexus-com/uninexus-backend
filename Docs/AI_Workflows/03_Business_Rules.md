# UniNexus Backend: Business Rules & Validation

## 1. Safety & Inventory Locks
* **Inventory Lock**: If a user has any overdue items in ANY club, they are blocked from borrowing across ALL clubs simultaneously.
* **Validation**: Check `LoanRecords` where `ReturnDate` is null and `DueDate < Now`.

## 2. Financial Integrity
* **Digital Signatures**: No wallet balance updates without a verified SKS Admin digital signature.
* **Audit Trail**: Every transaction must generate an immutable, time-stamped ledger record.

## 3. Leadership & Membership
* **Leadership Monopoly**: A student may lead at most ONE club at a time.
* **Context Switching**: The backend must handle users who have different roles across different clubs without permission leaks.

## 4. Gamification & Spatial
* **Score Wallet**: Points awarded strictly once per unique event; duplicate scans are rejected.
* **Location-Fencing**: Attendance is valid only if GPS coordinates are within a 50m radius of the event coordinates.
