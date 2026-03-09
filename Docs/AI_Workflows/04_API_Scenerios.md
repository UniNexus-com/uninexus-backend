# UniNexus Backend: API & Technical Flows

## 1. Authentication Flow
* **Step 1**: User provides email/password.
* **Step 2**: Infrastructure service verifies BCrypt hash.
* **Step 3**: Generate JWT + Refresh Token.
* **Step 4**: Return tokens to client (React Web or Flutter Mobile).

## 2. Real-Time Interactions (SignalR)
* **Global Announcements**: SKS Admins broadcast messages to all users (Latency < 500ms).
* **Leaderboard Sync**: Real-time ranking updates when points are earned through event check-ins.

## 3. Operational Flows
* **Event Publication**: High-priority/Inter-university events must be approved by SKS before appearing in the global feed.
* **Take & Drop**: QR scan initiates a borrowing transaction; system checks Inventory Lock status first.
