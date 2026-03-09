UniNexus Backend: API & Technical Flows

This document defines how the backend handles core user scenarios, integrating RESTful actions with SignalR real-time hubs.
1. Authentication & Onboarding (UC-SYS-1)

    Custom Password Login: The user provides an email and password via the Flutter or React client.

    Verification: The Infrastructure service verifies the provided password against the password_hash stored in the database.

    Token Issuance: Upon success, the system generates a JWT (Access Token) and a Refresh Token, allowing multi-platform session management.

    Onboarding: First-time users are directed to select Interest Tags, which are saved to their profile to personalize the event feed.

2. Real-Time Communication (SignalR)

    Global Announcements (UC-SKS-3): SKS Administrators draft urgent messages that are broadcast via the SignalR Real-Time Hub to all authenticated users in under 500ms.

    Live Leaderboard Sync (UC-SYS-3): As students earn points through QR check-ins, the server emits a real-time update to all clients viewing the leaderboard, animating rank changes without a page refresh.

    Budget Alerts: Club Leaders receive immediate push notifications when an SKS Administrator approves or rejects a funding request.

3. Operational Workflows

    Event Discovery & QR Check-in (UC-STU-3):

        The student scans an event QR code.

        The API validates the GPS coordinates (Location-fencing) to ensure they are within a 50m radius of the venue.

        Points are credited to the Score Wallet strictly once to prevent inflation.

    "Take & Drop" Inventory (UC-STU-2):

        Before a borrowing transaction is registered, the system queries for an active Inventory Lock.

        If the student has overdue items from any club, the API rejects the request and returns a list of overdue items.

    Financial Approvals (UC-SKS-1):

        Administrators review pending requests in a prioritized inbox.

        Approval requires capturing a digital signature record; without this, the system is hard-coded to prevent wallet balance updates.

4. Reporting & Analytics (UC-SKS-2, UC-SYS-2)

    Campus Heatmap: The backend uses PostGIS spatial analytics to aggregate student check-in density and return coordinate clusters for the SKS dashboard.

    Transcript Generation: The system queries records across all schemas (Memberships, Attendance, Points) to generate a structured Co-Curricular Transcript PDF for the student.
