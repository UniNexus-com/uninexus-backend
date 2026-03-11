# UniNexus Backend: Database Design & Entities

## 1. Identity Schema (Custom Password Support)
* **Users**: `id`, `email`, `password_hash` (BCrypt), `full_name`, `student_number`, `role`.
* **RefreshTokens**: `id`, `user_id`, `token_hash`, `platform` (Web/Mobile), `expires_at`.

## 2. Community Schema
* **Clubs**: `id`, `leader_id`, `name`, `description`, `is_active`.
* **Permissions**: Global codes (e.g., `EVENT_CREATE`, `INVENTORY_MANAGE`).
* **Club_Roles**: Dynamic roles created by Leaders with granular permission matrices.
* **Club_Memberships**: Links users to clubs with a specific context-aware role.

## 3. Operations Schema
* **Events**: Must include `location` (PostGIS `Geometry`) and `status` (Pending/Published).
* **Attendance**: `event_id`, `user_id`, `checkin_coords` (PostGIS), `points_awarded`.
* **Inventory_Items**: `id`, `club_id`, `qr_code`, `status` (Available/OnLoan).
* **Loan_Records**: `item_id`, `user_id`, `borrow_date`, `due_date`, `return_date`.

## 4. Finance Schema
* **Wallets**: `club_id`, `balance`.
* **Budget_Requests**: `id`, `club_id`, `amount`, `justification`, `digital_signature`, `status`.
* **Transaction_Ledger**: Immutable record of all financial movements.
