# The Raze - DAT602 Milestone 3

## Project Description

A multiplayer turn-based survival game database application demonstrating comprehensive transaction management, exception handling, and CRUD operations through TSQL stored procedures, C# DAO methods, and Windows Forms GUI.

## Technologies Used

- **C# .NET Windows Forms** - GUI layer with comprehensive exception handling
- **MySQL 8.0** - Database with stored procedures and transaction management
- **MySql.Data.MySqlClient** - Database connectivity

## Key Features

### Database Architecture
- **12 Stored Procedures** with full transaction management
- **3 Transaction Isolation Levels** appropriately applied:
  - READ COMMITTED (9 procedures)
  - REPEATABLE READ (2 procedures)
  - SERIALIZABLE (2 procedures)
- **Comprehensive Error Handling** with `DECLARE CONTINUE HANDLER FOR SQLEXCEPTION`
- **UPSERT Operations** for intelligent data merging
- **Cascade Delete Rules** for referential integrity

### Gameplay Features
- **Turn-based multiplayer** system with automatic turn switching
- **5x5 dynamic game board** with visual tile representation
- **Real-time updates** every 2 seconds showing all players
- **NPC item movement** - items move autonomously every 15 seconds
- **Item pickup system** with automatic score calculation
- **Inventory management** with item stacking
- **Account lockout** after 5 failed login attempts
- **Admin panel** for game and player management

### Security & Validation
- **Password hashing** using SHA2-256
- **Session management** with static session class
- **Input validation** at GUI, DAO, and database layers
- **Admin access control** with privilege checking
- **SQL injection prevention** through parameterized queries

## Setup Instructions

### Prerequisites
- MySQL 8.0 or higher
- Visual Studio 2022 or higher
- .NET 8.0 Windows Forms

### Database Setup
1. Open MySQL Workbench
2. Run `DAT602-Assessment-One-M1-SQL-Script.sql` to create database and tables
3. Sample data will be automatically inserted (3 players, 1 game, 4 items)

### Application Setup
1. Update connection string in `Db.cs` if needed:
```csharp
   Server=127.0.0.1;Port=3306;Database=theraze;Uid=root;Pwd=YOUR_PASSWORD;SslMode=None;
```
2. Build and run the solution in Visual Studio
3. Application will start at LoginForm

### Default Test Accounts
- **Admin Account:**
  - Username: `yugenzariah`
  - Password: `password123`
  - Privileges: Full admin access

- **Player Accounts:**
  - Username: `justin`
  - Password: `password123`

## Project Structure
```
TheRaze/
├── Data/                    - DAO classes
│   ├── Db.cs               - Database connection manager
│   ├── AuthDao.cs          - Authentication operations
│   ├── AdminDao.cs         - Admin operations
│   ├── GameDao.cs          - Game operations
│   └── LobbyDao.cs         - Lobby operations
├── Forms/                   - Windows Forms UI
│   ├── LoginForm.cs        - Login with lockout
│   ├── RegisterForm.cs     - User registration
│   ├── LobbyForm.cs        - Game lobby
│   ├── GameForm.cs         - Main game board
│   └── AdminForm.cs        - Admin panel
├── Utils/                   - Helper classes
│   └── HashHelper.cs       - Password hashing
├── Session.cs              - Session management
└── App.config              - Application configuration
```

## Database Schema

### Core Tables
- **Player** - User accounts with login tracking and high scores
- **Game** - Game instances with status tracking
- **Tile** - Board positions (5x5 grid per game)
- **PlayerGame** - Junction table for players in games
- **Item** - Item catalog (Apple, Medkit, Gem, Trap)
- **TileItem** - Items placed on tiles
- **Inventory** - Items collected by players

### Key Relationships
- Game → Tile (1:many, cascade delete)
- Game → PlayerGame (1:many, cascade delete)
- PlayerGame → Inventory (1:many, cascade delete)
- Tile → TileItem (1:many, cascade delete)

## Stored Procedures (All 12)

### Authentication & User Management
1. `store_procedure_login_player` - Login with attempt tracking and lockout
2. `store_procedure_register_player` - User registration with duplicate checking
3. `store_procedure_get_player_info` - Retrieve player data
4. `store_procedure_admin_add_player` - Admin create player
5. `store_procedure_admin_update_player` - Admin update player (can unlock accounts)
6. `store_procedure_admin_delete_player` - Admin delete player

### Game Operations
7. `store_procedure_create_game` - Create game with auto-spawned items
8. `store_procedure_join_game` - Join or resume game
9. `store_procedure_kill_game` - Admin terminate game
10. `store_procedure_reset_board` - Reset game board
11. `store_procedure_init_board` - Initialize board tiles

### Gameplay Operations
12. `store_procedure_move_player` - Move to adjacent tile with turn management
13. `store_procedure_update_score` - Update score with high score tracking
14. `store_procedure_pickup_item` - Pick up item, update inventory and score
15. `store_procedure_place_item_on_tile` - Place item on tile (admin/setup)
16. `store_procedure_move_tile_item` - NPC item movement with UPSERT

### Utility Functions
- `fn_is_adjacent` - Check if tiles are adjacent
- `store_procedure_get_games_list` - Retrieve active games list

## Exception Handling Architecture

### Three-Tier Error Handling

**Database Layer (SQL):**
- `DECLARE CONTINUE HANDLER FOR SQLEXCEPTION`
- Explicit ROLLBACK on errors
- Explicit COMMIT on success
- Meaningful error messages in result sets

**DAO Layer (C#):**
- Try-catch with specific exception types:
  - `MySqlException` - Database errors
  - `InvalidOperationException` - Connection errors
  - `InvalidCastException` - Data conversion errors
  - `Exception` - Catch-all
- Tuple returns with status codes
- Client-side validation before database calls

**GUI Layer (Windows Forms):**
- Try-catch blocks on all button handlers
- Input validation before DAO calls
- User-friendly error messages
- Focus management for error fields

## Testing

### Tested Features
All 12 stored procedures with valid and invalid inputs  
All DAO methods with exception handling  
All GUI forms with validation  
Multiplayer functionality (2+ concurrent users)  
Turn-based system  
Item pickup and score updates  
NPC movement system  
Account lockout mechanism  
Admin access control  
Database connection error handling  
Concurrent transaction handling  

### How to Test Multiplayer
1. Run the application
2. Login as `yugenzariah`
3. Join or create a game
4. Open `bin\Debug\net8.0-windows\TheRaze.exe` (second instance)
5. Login as `justin`
6. Join the same game
7. Both players can see each other and interact in real-time

## Game Rules

1. **Movement:** Players can only move to adjacent tiles (up, down, left, right)
2. **Turns:** Players take turns moving (turn indicator shows whose turn it is)
3. **Items:** Pick up items from your current tile to increase score
4. **Scoring:** 
   - Apple: +5 points
   - Medkit: +15 points
   - Gem: +25 points
   - Trap: -10 points
5. **Winning:** Player with highest score when game ends wins
6. **NPC:** Items move automatically every 15 seconds

## Known Limitations

- Password hashing uses SHA2-256 (production should use bcrypt/Argon2)
- Static session management (production should use token-based auth)
- Fixed 5x5 board size (could be made configurable)
- Polling-based updates (production could use SignalR for real-time)
- Limited to local network multiplayer without VPN/port forwarding
- Test results with screenshots
- Concurrency management essay
