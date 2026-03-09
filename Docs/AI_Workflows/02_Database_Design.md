UniNexus Backend: Database Design & Entities

This document defines the data structure for UniNexus, organized into five logical schemas as required by the Software Requirements Specification.
1. Identity Schema (Custom Password Support)

    Users: Primary account table containing id, email, password_hash (BCrypt), full_name, student_number, and global role (SKSAdmin, Leader, Student).

    RefreshTokens: Stores id, user_id, token_hash, platform (iOS/Android/Web), and expires_at to manage multi-platform sessions.

    StudentProfiles: Extended data including ScoreWalletPoints and a list of InterestTags for the personalized event feed.

2. Community Schema

    Clubs: The entity for campus organizations, including id, name, description, and leader_id.

        Constraint: A student may be the primary "Leader" of only one club at a time.

    Permissions: Fixed lookup table for granular system actions (e.g., EVENT_CREATE, BUDGET_REQUEST).

    Club_Roles: Dynamic roles created by Club Leaders with specific permission sets.

    Club_Memberships: Links Users to Clubs and assigns a Club_Role. Supports context-switching for users in multiple clubs.

3. Operations Schema (Spatial & Physical)

    Events: Tracks id, club_id, title, and status (Pending/Published).

        Spatial Requirement: Must use PostGIS Geometry(Point, 4326) for the location field to support the campus heatmap.

    Attendance: Records event_id, user_id, checkin_coords (for 50m location-fencing), and points_awarded.

    Inventory_Items: Equipment managed via the "Take & Drop" QR system, including id, club_id, qr_code, and status (Available/OnLoan).

    Loan_Records: Tracks which student has borrowed which item, including borrow_date, due_date, and return_date.

4. Finance Schema

    Wallets: Tracks the current balance for each club.

    Budget_Requests: Formal requests for funds containing amount, justification, and a required digital_signature from an SKS Administrator.

    Transaction_Ledger: An immutable, time-stamped record of all financial movements (Approved, Rejected, or Rolled-back).

5. Social Schema

    Feeds: Stores posts and event announcements visible in the mobile application.

    Interactions: Tracks likes, comments, and poll_votes associated with feed content.
