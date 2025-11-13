DROP DATABASE IF EXISTS theraze;
CREATE DATABASE theraze;
USE theraze;

-- =========================================================
-- Create Tables Procedure
-- =========================================================
DELIMITER $$

CREATE PROCEDURE store_procedure_create_all_tables()
BEGIN
  CREATE TABLE IF NOT EXISTS Player (
    PlayerID       INT UNSIGNED NOT NULL AUTO_INCREMENT,
    Username       VARCHAR(40)  NOT NULL UNIQUE,
    Email          VARCHAR(120) NOT NULL UNIQUE,
    PasswordHash   CHAR(64)     NOT NULL,
    IsAdmin        TINYINT(1)   NOT NULL DEFAULT 0,
    IsLocked       TINYINT(1)   NOT NULL DEFAULT 0,
    LoginAttempts  SMALLINT     NOT NULL DEFAULT 0,
    Highscore      INT          NOT NULL DEFAULT 0,
    LastOnline     DATETIME     NULL,
    CreatedAt      TIMESTAMP    NOT NULL DEFAULT CURRENT_TIMESTAMP,
    PRIMARY KEY (PlayerID)
  );

  CREATE TABLE IF NOT EXISTS Game (
    GameID     INT UNSIGNED NOT NULL AUTO_INCREMENT,
    Name       VARCHAR(60)  NOT NULL,
    Status     VARCHAR(20)  NOT NULL DEFAULT 'Waiting',
    StartedAt  DATE         NULL,
    EndedAt    DATE         NULL,
    PRIMARY KEY (GameID)
  );

  CREATE TABLE IF NOT EXISTS Tile (
    TileID   INT UNSIGNED NOT NULL AUTO_INCREMENT,
    GameID   INT UNSIGNED NOT NULL,
    X        SMALLINT     NOT NULL,
    Y        SMALLINT     NOT NULL,
    TileType VARCHAR(50)  NOT NULL DEFAULT 'Floor',
    PRIMARY KEY (TileID),
    CONSTRAINT uq_Tile_Game_Coord UNIQUE (GameID, X, Y),
    CONSTRAINT fk_Tile_Game FOREIGN KEY (GameID) REFERENCES Game(GameID)
      ON DELETE CASCADE ON UPDATE CASCADE
  );

  CREATE TABLE IF NOT EXISTS PlayerGame (
    PlayerGameID INT UNSIGNED NOT NULL AUTO_INCREMENT,
    PlayerID     INT UNSIGNED NOT NULL,
    GameID       INT UNSIGNED NOT NULL,
    Score        INT          NOT NULL DEFAULT 0,
    HP           SMALLINT     NOT NULL DEFAULT 100,
    CurrentTileID INT UNSIGNED NULL,
    IsTurn       TINYINT(1)   NOT NULL DEFAULT 0,
    JoinedAt     DATE         NOT NULL,
    PRIMARY KEY (PlayerGameID),
    CONSTRAINT fk_PG_Player FOREIGN KEY (PlayerID) REFERENCES Player(PlayerID)
      ON DELETE CASCADE ON UPDATE CASCADE,
    CONSTRAINT fk_PG_Game FOREIGN KEY (GameID) REFERENCES Game(GameID)
      ON DELETE CASCADE ON UPDATE CASCADE,
    CONSTRAINT fk_PG_Tile FOREIGN KEY (CurrentTileID) REFERENCES Tile(TileID)
      ON DELETE SET NULL ON UPDATE CASCADE
  );

  CREATE TABLE IF NOT EXISTS Item (
    ItemID   INT UNSIGNED NOT NULL AUTO_INCREMENT,
    Name     VARCHAR(60)  NOT NULL,
    ItemType VARCHAR(60)  NOT NULL,
    Points   SMALLINT     NOT NULL DEFAULT 0,
    Weight   DECIMAL(5,2) NOT NULL DEFAULT 0.0,
    PRIMARY KEY (ItemID)
  );

  CREATE TABLE IF NOT EXISTS Inventory (
    InventoryID  INT UNSIGNED NOT NULL AUTO_INCREMENT,
    PlayerGameID INT UNSIGNED NOT NULL,
    ItemID       INT UNSIGNED NOT NULL,
    Quantity     SMALLINT     NOT NULL DEFAULT 1,
    PRIMARY KEY (InventoryID),
    CONSTRAINT uq_Inv_PG_Item UNIQUE (PlayerGameID, ItemID),
    CONSTRAINT fk_Inv_PG FOREIGN KEY (PlayerGameID) REFERENCES PlayerGame(PlayerGameID)
      ON DELETE CASCADE ON UPDATE CASCADE,
    CONSTRAINT fk_Inv_Item FOREIGN KEY (ItemID) REFERENCES Item(ItemID)
      ON DELETE RESTRICT ON UPDATE CASCADE
  );

  CREATE TABLE IF NOT EXISTS TileItem (
    TileItemID INT UNSIGNED NOT NULL AUTO_INCREMENT,
    TileID     INT UNSIGNED NOT NULL,
    ItemID     INT UNSIGNED NOT NULL,
    Quantity   SMALLINT     NOT NULL DEFAULT 1,
    PRIMARY KEY (TileItemID),
    CONSTRAINT uq_TileItem UNIQUE (TileID, ItemID),
    CONSTRAINT fk_TI_Tile FOREIGN KEY (TileID) REFERENCES Tile(TileID)
      ON DELETE CASCADE ON UPDATE CASCADE,
    CONSTRAINT fk_TI_Item FOREIGN KEY (ItemID) REFERENCES Item(ItemID)
      ON DELETE RESTRICT ON UPDATE CASCADE
  );

  CREATE OR REPLACE VIEW v_player_locations AS
  SELECT pg.PlayerGameID, pg.PlayerID, p.Username, pg.GameID,
         t.TileID, t.X, t.Y, pg.IsTurn
  FROM PlayerGame pg
  LEFT JOIN Player p ON p.PlayerID = pg.PlayerID
  LEFT JOIN Tile t   ON t.TileID   = pg.CurrentTileID;
END$$

DELIMITER ;

-- =========================================================
-- Insert Sample Data
-- =========================================================
DELIMITER $$

CREATE PROCEDURE store_procedure_insert_sample_data()
BEGIN
  INSERT INTO Player (Username, Email, PasswordHash, IsAdmin)
  VALUES ('yugenzariah', 'yz@example.com', SHA2('password123', 256), 1),
         ('justin', 'justinsux@example.com', SHA2('password123', 256), 0),
         ('isaac', 'isaacsux@example.com', SHA2('password123', 256), 0);

  INSERT INTO Game (Name, Status, StartedAt)
  VALUES ('Raze-Alpha', 'Running', CURDATE());

  INSERT INTO Item (Name, ItemType, Points, Weight) VALUES
  ('Apple',  'Health',  5,  0.2),
  ('Medkit', 'Health', 15,  1.0),
  ('Gem',    'Treasure', 25, 0.5),
  ('Trap',   'Hazard', -10, 0.3);

  CALL store_procedure_init_board(1, 5, 5);

  INSERT INTO TileItem (TileID, ItemID, Quantity)
  SELECT t.TileID, i.ItemID, 1
  FROM Tile t
  JOIN Item i ON ( (i.Name='Gem'  AND t.X=2 AND t.Y=2 AND t.GameID=1)
                OR (i.Name='Trap' AND t.X=1 AND t.Y=1 AND t.GameID=1)
                OR (i.Name='Apple' AND t.X=0 AND t.Y=1 AND t.GameID=1) );

  INSERT INTO PlayerGame (PlayerID, GameID, Score, HP, CurrentTileID, IsTurn, JoinedAt)
  SELECT p.PlayerID, 1, 0, 100,
         CASE 
           WHEN p.Username = 'yugenzariah' THEN (SELECT TileID FROM Tile WHERE GameID=1 AND X=0 AND Y=0)
           WHEN p.Username = 'justin' THEN (SELECT TileID FROM Tile WHERE GameID=1 AND X=1 AND Y=0)
         END,
         IF(p.Username='yugenzariah',1,0), 
         CURDATE()
  FROM Player p
  WHERE p.Username IN ('yugenzariah','justin');
END$$

DELIMITER ;

-- =========================================================
-- Utility Procedures and Functions
-- =========================================================
DELIMITER $$

CREATE PROCEDURE store_procedure_init_board(p_gameId INT UNSIGNED, p_width INT, p_height INT)
BEGIN
  DECLARE x INT DEFAULT 0;
  DECLARE y INT DEFAULT 0;
  WHILE x < p_width DO
    SET y = 0;
    WHILE y < p_height DO
      INSERT INTO Tile(GameID, X, Y, TileType)
      VALUES (p_gameId, x, y, IF(x=0 AND y=0,'Home','Floor'));
      SET y = y + 1;
    END WHILE;
    SET x = x + 1;
  END WHILE;
END$$

CREATE FUNCTION fn_is_adjacent(p_fromTile INT UNSIGNED, p_toTile INT UNSIGNED)
RETURNS TINYINT READS SQL DATA DETERMINISTIC
BEGIN
  DECLARE x1, y1 SMALLINT;
  DECLARE x2, y2 SMALLINT;
  DECLARE g1, g2 INT UNSIGNED;
  
  SELECT X, Y, GameID INTO x1, y1, g1 FROM Tile WHERE TileID = p_fromTile;
  SELECT X, Y, GameID INTO x2, y2, g2 FROM Tile WHERE TileID = p_toTile;
  
  IF g1 IS NULL OR g2 IS NULL OR g1 <> g2 THEN
    RETURN 0;
  END IF;
  
  RETURN IF(ABS(x1-x2) + ABS(y1-y2) = 1, 1, 0);
END$$

DELIMITER ;

-- =========================================================
-- MILESTONE 3: All Game Procedures with Transactions
-- =========================================================
DELIMITER $$

-- =========================================================
-- 1) Player Login with Lockout
-- The label allows early exit using LEAVE statement
-- =========================================================
CREATE PROCEDURE store_procedure_login_player(
  IN p_username VARCHAR(40),
  IN p_passwordhash CHAR(64)
)
proc_label: BEGIN
  DECLARE v_id INT UNSIGNED;
  DECLARE v_locked TINYINT;
  DECLARE v_attempts SMALLINT;
  DECLARE v_hash CHAR(64);
  DECLARE error_happened BOOLEAN DEFAULT FALSE;
  
  DECLARE CONTINUE HANDLER FOR SQLEXCEPTION
  BEGIN
    SET error_happened = TRUE;
  END;
  
  SET TRANSACTION ISOLATION LEVEL READ COMMITTED;
  START TRANSACTION;
  
  SELECT PlayerID, IsLocked, LoginAttempts, PasswordHash
    INTO v_id, v_locked, v_attempts, v_hash
  FROM Player WHERE Username = p_username;

  IF v_id IS NULL THEN
    ROLLBACK;
    SELECT 'UNKNOWN' AS Status, 'Username not found' AS Message;
    LEAVE proc_label;
  END IF;

  IF v_locked = 1 THEN
    ROLLBACK;
    SELECT 'LOCKED' AS Status, 'Account is locked due to too many failed attempts' AS Message;
    LEAVE proc_label;
  END IF;

  IF v_hash = p_passwordhash THEN
    UPDATE Player SET LoginAttempts = 0, LastOnline = NOW() WHERE PlayerID = v_id;
    COMMIT;
    SELECT 'OK' AS Status, 'Login successful' AS Message, v_id AS PlayerID;
  ELSE
    SET v_attempts = v_attempts + 1;
    UPDATE Player SET LoginAttempts = v_attempts, IsLocked = IF(v_attempts >= 5, 1, 0)
    WHERE PlayerID = v_id;
    COMMIT;
    
    IF v_attempts >= 5 THEN
      SELECT 'LOCKED' AS Status, 'Account locked after 5 failed attempts' AS Message;
    ELSE
      SELECT 'BADPASS' AS Status, CONCAT('Incorrect password. ', (5 - v_attempts), ' attempts remaining') AS Message;
    END IF;
  END IF;
END$$

-- =========================================================
-- 2) Player Registration
-- Checks if username/email exists BEFORE insert
-- =========================================================
CREATE PROCEDURE store_procedure_register_player(
  IN p_username VARCHAR(40),
  IN p_email VARCHAR(120),
  IN p_passwordhash CHAR(64)
)
BEGIN
  DECLARE v_username_exists INT;
  DECLARE v_email_exists INT;
  DECLARE error_happened BOOLEAN DEFAULT FALSE;
  
  DECLARE CONTINUE HANDLER FOR SQLEXCEPTION
  BEGIN
    SET error_happened = TRUE;
  END;
  
  SET TRANSACTION ISOLATION LEVEL READ COMMITTED;
  START TRANSACTION;
  
  SELECT COUNT(*) INTO v_username_exists 
  FROM Player WHERE Username = p_username;
  
  IF v_username_exists > 0 THEN
    ROLLBACK;
    SELECT 'ERROR' AS Status, 'Username already exists' AS Message;
  ELSE
    SELECT COUNT(*) INTO v_email_exists 
    FROM Player WHERE Email = p_email;
    
    IF v_email_exists > 0 THEN
      ROLLBACK;
      SELECT 'ERROR' AS Status, 'Email already exists' AS Message;
    ELSE
      INSERT INTO Player (Username, Email, PasswordHash, IsAdmin, IsLocked, LoginAttempts, Highscore, LastOnline)
      VALUES (p_username, p_email, p_passwordhash, 0, 0, 0, 0, NOW());
      
      IF error_happened THEN
        ROLLBACK;
        SELECT 'ERROR' AS Status, 'Registration failed due to database error' AS Message;
      ELSE
        COMMIT;
        SELECT 'SUCCESS' AS Status, 'Player registered successfully' AS Message, LAST_INSERT_ID() AS PlayerID;
      END IF;
    END IF;
  END IF;
END$$

-- =========================================================
-- 3) Reset Board
-- =========================================================
CREATE PROCEDURE store_procedure_reset_board(
  IN p_gameId INT UNSIGNED,
  IN p_width INT,
  IN p_height INT
)
BEGIN
  DECLARE error_happened BOOLEAN DEFAULT FALSE;
  
  DECLARE CONTINUE HANDLER FOR SQLEXCEPTION
  BEGIN
    SET error_happened = TRUE;
  END;
  
  SET TRANSACTION ISOLATION LEVEL READ COMMITTED;
  START TRANSACTION;
  
  DELETE FROM Tile WHERE GameID = p_gameId;
  CALL store_procedure_init_board(p_gameId, p_width, p_height);
  
  IF error_happened THEN
    ROLLBACK;
    SELECT 'ERROR' AS Status, 'Board reset failed' AS Message;
  ELSE
    COMMIT;
    SELECT 'SUCCESS' AS Status, 'Board reset successfully' AS Message;
  END IF;
END$$

-- =========================================================
-- 4) Place Item on Tile
-- Checks tile exists, item exists, quantity is positive
-- =========================================================
CREATE PROCEDURE store_procedure_place_item_on_tile(
  IN p_tileId INT UNSIGNED,
  IN p_itemId INT UNSIGNED,
  IN p_qty SMALLINT
)
BEGIN
  DECLARE v_tile_exists INT;
  DECLARE v_item_exists INT;
  DECLARE error_happened BOOLEAN DEFAULT FALSE;
  
  DECLARE CONTINUE HANDLER FOR SQLEXCEPTION
  BEGIN
    SET error_happened = TRUE;
  END;
  
  SET TRANSACTION ISOLATION LEVEL READ COMMITTED;
  START TRANSACTION;
  
  SELECT COUNT(*) INTO v_tile_exists FROM Tile WHERE TileID = p_tileId;
  
  IF v_tile_exists = 0 THEN
    ROLLBACK;
    SELECT 'ERROR' AS Status, 'Tile does not exist' AS Message;
  ELSE
    SELECT COUNT(*) INTO v_item_exists FROM Item WHERE ItemID = p_itemId;
    
    IF v_item_exists = 0 THEN
      ROLLBACK;
      SELECT 'ERROR' AS Status, 'Item does not exist' AS Message;
    ELSE
      IF p_qty <= 0 THEN
        ROLLBACK;
        SELECT 'ERROR' AS Status, 'Quantity must be positive' AS Message;
      ELSE
        INSERT INTO TileItem (TileID, ItemID, Quantity)
        VALUES (p_tileId, p_itemId, p_qty)
        ON DUPLICATE KEY UPDATE Quantity = Quantity + VALUES(Quantity);
        
        IF error_happened THEN
          ROLLBACK;
          SELECT 'ERROR' AS Status, 'Failed to place item on tile' AS Message;
        ELSE
          COMMIT;
          SELECT 'SUCCESS' AS Status, CONCAT('Placed ', p_qty, ' item(s) on tile') AS Message;
        END IF;
      END IF;
    END IF;
  END IF;
END$$

-- =========================================================
-- 5) Move Player
-- =========================================================
CREATE PROCEDURE store_procedure_move_player(
  IN p_playerGameId INT UNSIGNED,
  IN p_targetTileId INT UNSIGNED
)
BEGIN
  DECLARE v_from INT UNSIGNED;
  DECLARE v_ok TINYINT;
  DECLARE v_game INT UNSIGNED;
  DECLARE error_happened BOOLEAN DEFAULT FALSE;
  
  DECLARE CONTINUE HANDLER FOR SQLEXCEPTION
  BEGIN
    SET error_happened = TRUE;
  END;
  
  SET TRANSACTION ISOLATION LEVEL REPEATABLE READ;
  START TRANSACTION;
  
  SELECT CurrentTileID, GameID INTO v_from, v_game 
  FROM PlayerGame WHERE PlayerGameID = p_playerGameId;
  
  IF v_from IS NULL THEN
    ROLLBACK;
    SELECT 'ERROR' AS Status, 'Player not placed on board yet' AS Message;
  ELSE
    SET v_ok = fn_is_adjacent(v_from, p_targetTileId);
    
    IF v_ok = 0 THEN
      ROLLBACK;
      SELECT 'ERROR' AS Status, 'Target tile is not adjacent or in different game' AS Message;
    ELSEIF EXISTS(SELECT 1 FROM PlayerGame WHERE CurrentTileID = p_targetTileId) THEN
      ROLLBACK;
      SELECT 'ERROR' AS Status, 'Target tile is occupied by another player' AS Message;
    ELSE
      UPDATE PlayerGame SET CurrentTileID = p_targetTileId, IsTurn = 0
      WHERE PlayerGameID = p_playerGameId;
      
      UPDATE PlayerGame SET IsTurn = 1
      WHERE GameID = v_game AND PlayerGameID <> p_playerGameId
      ORDER BY JoinedAt LIMIT 1;
      
      IF error_happened THEN
        ROLLBACK;
        SELECT 'ERROR' AS Status, 'Move failed due to database error' AS Message;
      ELSE
        COMMIT;
        SELECT 'SUCCESS' AS Status, 'Player moved successfully' AS Message;
      END IF;
    END IF;
  END IF;
END$$

-- =========================================================
-- 6) Update Score
-- =========================================================
CREATE PROCEDURE store_procedure_update_score(
  IN p_playerGameId INT UNSIGNED,
  IN p_delta INT
)
BEGIN
  DECLARE error_happened BOOLEAN DEFAULT FALSE;
  DECLARE v_newScore INT DEFAULT 0;
  
  DECLARE CONTINUE HANDLER FOR SQLEXCEPTION
  BEGIN
    SET error_happened = TRUE;
  END;
  
  SET TRANSACTION ISOLATION LEVEL SERIALIZABLE;
  START TRANSACTION;
  
  UPDATE PlayerGame SET Score = Score + p_delta WHERE PlayerGameID = p_playerGameId;
  SELECT Score INTO v_newScore FROM PlayerGame WHERE PlayerGameID = p_playerGameId;
  
  UPDATE Player p
  JOIN PlayerGame pg ON pg.PlayerID = p.PlayerID
  SET p.Highscore = GREATEST(p.Highscore, pg.Score)
  WHERE pg.PlayerGameID = p_playerGameId;
  
  IF error_happened OR v_newScore IS NULL THEN
    ROLLBACK;
    SELECT 'ERROR' AS Status, 'Score update failed' AS Message;
  ELSE
    COMMIT;
    SELECT 'SUCCESS' AS Status, CONCAT('Score updated to ', v_newScore) AS Message, v_newScore AS NewScore;
  END IF;
END$$

-- =========================================================
-- 7) Pickup Item
-- =========================================================
CREATE PROCEDURE store_procedure_pickup_item(
  IN p_playerGameId INT UNSIGNED,
  IN p_itemId INT UNSIGNED,
  IN p_qty SMALLINT
)
BEGIN
  DECLARE v_tile INT UNSIGNED;
  DECLARE v_points SMALLINT;
  DECLARE v_available SMALLINT;
  DECLARE error_happened BOOLEAN DEFAULT FALSE;
  
  DECLARE CONTINUE HANDLER FOR SQLEXCEPTION
  BEGIN
    SET error_happened = TRUE;
  END;
  
  SET TRANSACTION ISOLATION LEVEL SERIALIZABLE;
  START TRANSACTION;
  
  SELECT CurrentTileID INTO v_tile FROM PlayerGame WHERE PlayerGameID = p_playerGameId;
  
  IF v_tile IS NULL THEN
    ROLLBACK;
    SELECT 'ERROR' AS Status, 'Player has no tile' AS Message;
  ELSE
    SELECT Quantity INTO v_available FROM TileItem WHERE TileID = v_tile AND ItemID = p_itemId;
    
    IF v_available IS NULL OR v_available < p_qty THEN
      ROLLBACK;
      SELECT 'ERROR' AS Status, 'Not enough items on tile' AS Message;
    ELSE
      UPDATE TileItem SET Quantity = Quantity - p_qty WHERE TileID = v_tile AND ItemID = p_itemId;
      DELETE FROM TileItem WHERE TileID = v_tile AND ItemID = p_itemId AND Quantity <= 0;
      
      INSERT INTO Inventory (PlayerGameID, ItemID, Quantity)
      VALUES (p_playerGameId, p_itemId, p_qty)
      ON DUPLICATE KEY UPDATE Quantity = Quantity + VALUES(Quantity);
      
      SELECT Points INTO v_points FROM Item WHERE ItemID = p_itemId;
      UPDATE PlayerGame SET Score = Score + (v_points * p_qty) WHERE PlayerGameID = p_playerGameId;
      
      IF error_happened THEN
        ROLLBACK;
        SELECT 'ERROR' AS Status, 'Pickup failed' AS Message;
      ELSE
        COMMIT;
        SELECT 'SUCCESS' AS Status, CONCAT('Picked up ', p_qty, ' item(s)') AS Message;
      END IF;
    END IF;
  END IF;
END$$

-- =========================================================
-- 8) Move Tile Item (NPC movement)
-- The INSERT...ON DUPLICATE KEY UPDATE is the UPSERT:
--   - INSERT if item doesn't exist on target tile
--   - UPDATE quantity if item already exists on target tile
-- =========================================================
CREATE PROCEDURE store_procedure_move_tile_item(
  IN p_tileItemId INT UNSIGNED,
  IN p_targetTileId INT UNSIGNED
)
BEGIN
  DECLARE v_srcTile INT UNSIGNED;
  DECLARE v_item INT UNSIGNED;
  DECLARE error_happened BOOLEAN DEFAULT FALSE;
  
  DECLARE CONTINUE HANDLER FOR SQLEXCEPTION
  BEGIN
    SET error_happened = TRUE;
  END;
  
  SET TRANSACTION ISOLATION LEVEL REPEATABLE READ;
  START TRANSACTION;
  
  SELECT TileID, ItemID INTO v_srcTile, v_item FROM TileItem WHERE TileItemID = p_tileItemId;
  
  IF v_srcTile IS NULL THEN
    ROLLBACK;
    SELECT 'ERROR' AS Status, 'TileItem not found' AS Message;
  ELSEIF fn_is_adjacent(v_srcTile, p_targetTileId) = 0 THEN
    ROLLBACK;
    SELECT 'ERROR' AS Status, 'Target tile not adjacent' AS Message;
  ELSE
    UPDATE TileItem SET Quantity = Quantity - 1 WHERE TileItemID = p_tileItemId;
    DELETE FROM TileItem WHERE TileItemID = p_tileItemId AND Quantity <= 0;
    
    -- UPSERT: Insert new or update existing
    INSERT INTO TileItem (TileID, ItemID, Quantity)
    VALUES (p_targetTileId, v_item, 1)
    ON DUPLICATE KEY UPDATE Quantity = Quantity + 1;
    
    IF error_happened THEN
      ROLLBACK;
      SELECT 'ERROR' AS Status, 'Move failed' AS Message;
    ELSE
      COMMIT;
      SELECT 'SUCCESS' AS Status, 'Item moved successfully' AS Message;
    END IF;
  END IF;
END$$

-- =========================================================
-- 9) Kill Game
-- Checks if game exists, then DELETES PlayerGame records
-- =========================================================
CREATE PROCEDURE store_procedure_kill_game(
  IN p_gameId INT UNSIGNED
)
BEGIN
  DECLARE v_game_exists INT;
  DECLARE error_happened BOOLEAN DEFAULT FALSE;
  
  DECLARE CONTINUE HANDLER FOR SQLEXCEPTION
  BEGIN
    SET error_happened = TRUE;
  END;
  
  SET TRANSACTION ISOLATION LEVEL READ COMMITTED;
  START TRANSACTION;
  
  SELECT COUNT(*) INTO v_game_exists FROM Game WHERE GameID = p_gameId;
  
  IF v_game_exists = 0 THEN
    ROLLBACK;
    SELECT 'ERROR' AS Status, 'Game does not exist' AS Message;
  ELSE
    UPDATE Game SET Status = 'Killed', EndedAt = CURDATE() WHERE GameID = p_gameId;
    DELETE FROM PlayerGame WHERE GameID = p_gameId;
    
    IF error_happened THEN
      ROLLBACK;
      SELECT 'ERROR' AS Status, 'Failed to kill game' AS Message;
    ELSE
      COMMIT;
      SELECT 'SUCCESS' AS Status, 'Game killed and player records removed' AS Message;
    END IF;
  END IF;
END$$

-- =========================================================
-- 10) Admin Add Player
-- Checks if username/email already exists
-- =========================================================
CREATE PROCEDURE store_procedure_admin_add_player(
  IN p_username VARCHAR(40),
  IN p_email VARCHAR(120),
  IN p_passwordhash CHAR(64),
  IN p_isAdmin TINYINT
)
BEGIN
  DECLARE v_username_exists INT;
  DECLARE v_email_exists INT;
  DECLARE error_happened BOOLEAN DEFAULT FALSE;
  
  DECLARE CONTINUE HANDLER FOR SQLEXCEPTION
  BEGIN
    SET error_happened = TRUE;
  END;
  
  SET TRANSACTION ISOLATION LEVEL READ COMMITTED;
  START TRANSACTION;
  
  SELECT COUNT(*) INTO v_username_exists FROM Player WHERE Username = p_username;
  
  IF v_username_exists > 0 THEN
    ROLLBACK;
    SELECT 'ERROR' AS Status, 'Username already exists' AS Message;
  ELSE
    SELECT COUNT(*) INTO v_email_exists FROM Player WHERE Email = p_email;
    
    IF v_email_exists > 0 THEN
      ROLLBACK;
      SELECT 'ERROR' AS Status, 'Email already exists' AS Message;
    ELSE
      INSERT INTO Player (Username, Email, PasswordHash, IsAdmin, CreatedAt)
      VALUES (p_username, p_email, p_passwordhash, p_isAdmin, NOW());
      
      IF error_happened THEN
        ROLLBACK;
        SELECT 'ERROR' AS Status, 'Failed to add player' AS Message;
      ELSE
        COMMIT;
        SELECT 'SUCCESS' AS Status, 'Player added successfully' AS Message, LAST_INSERT_ID() AS PlayerID;
      END IF;
    END IF;
  END IF;
END$$

-- =========================================================
-- 11) Admin Update Player
-- =========================================================
CREATE PROCEDURE store_procedure_admin_update_player(
  IN p_playerId INT UNSIGNED,
  IN p_username VARCHAR(40),
  IN p_email VARCHAR(120),
  IN p_passwordhash CHAR(64),
  IN p_isAdmin TINYINT,
  IN p_isLocked TINYINT
)
BEGIN
  DECLARE v_player_exists INT;
  DECLARE error_happened BOOLEAN DEFAULT FALSE;
  
  DECLARE CONTINUE HANDLER FOR SQLEXCEPTION
  BEGIN
    SET error_happened = TRUE;
  END;
  
  SET TRANSACTION ISOLATION LEVEL READ COMMITTED;
  START TRANSACTION;
  
  SELECT COUNT(*) INTO v_player_exists FROM Player WHERE PlayerID = p_playerId;
  
  IF v_player_exists = 0 THEN
    ROLLBACK;
    SELECT 'ERROR' AS Status, 'Player does not exist' AS Message;
  ELSE
    UPDATE Player
    SET Username = p_username, Email = p_email, PasswordHash = p_passwordhash,
        IsAdmin = p_isAdmin, IsLocked = p_isLocked
    WHERE PlayerID = p_playerId;
    
    IF error_happened THEN
      ROLLBACK;
      SELECT 'ERROR' AS Status, 'Update failed' AS Message;
    ELSE
      COMMIT;
      SELECT 'SUCCESS' AS Status, 'Player updated successfully' AS Message;
    END IF;
  END IF;
END$$

-- =========================================================
-- 12) Admin Delete Player
-- =========================================================
CREATE PROCEDURE store_procedure_admin_delete_player(
  IN p_playerId INT UNSIGNED
)
BEGIN
  DECLARE v_player_exists INT;
  DECLARE error_happened BOOLEAN DEFAULT FALSE;
  
  DECLARE CONTINUE HANDLER FOR SQLEXCEPTION
  BEGIN
    SET error_happened = TRUE;
  END;
  
  SET TRANSACTION ISOLATION LEVEL READ COMMITTED;
  START TRANSACTION;
  
  SELECT COUNT(*) INTO v_player_exists FROM Player WHERE PlayerID = p_playerId;
  
  IF v_player_exists = 0 THEN
    ROLLBACK;
    SELECT 'ERROR' AS Status, 'Player does not exist' AS Message;
  ELSE
    DELETE FROM Player WHERE PlayerID = p_playerId;
    
    IF error_happened THEN
      ROLLBACK;
      SELECT 'ERROR' AS Status, 'Delete failed' AS Message;
    ELSE
      COMMIT;
      SELECT 'SUCCESS' AS Status, 'Player deleted successfully' AS Message;
    END IF;
  END IF;
END$$

-- =========================================================
-- 13) Get Games List (READ ONLY - no transaction needed)
-- =========================================================
CREATE PROCEDURE store_procedure_get_games_list()
BEGIN
  SELECT g.GameID, g.Name, g.Status, g.StartedAt,
         COUNT(pg.PlayerGameID) as PlayerCount
  FROM Game g
  LEFT JOIN PlayerGame pg ON pg.GameID = g.GameID
  WHERE g.Status IN ('Waiting', 'Running')
  GROUP BY g.GameID, g.Name, g.Status, g.StartedAt
  ORDER BY g.GameID DESC;
END$$

-- =========================================================
-- 14) Create Game
-- =========================================================
CREATE PROCEDURE store_procedure_create_game(
  IN p_gameName VARCHAR(60)
)
BEGIN
  DECLARE v_gameId INT UNSIGNED;
  DECLARE error_happened BOOLEAN DEFAULT FALSE;
  
  DECLARE CONTINUE HANDLER FOR SQLEXCEPTION
  BEGIN
    SET error_happened = TRUE;
  END;
  
  SET TRANSACTION ISOLATION LEVEL READ COMMITTED;
  START TRANSACTION;
  
  -- Create the game
  INSERT INTO Game (Name, Status, StartedAt) VALUES (p_gameName, 'Waiting', CURDATE());
  SET v_gameId = LAST_INSERT_ID();
  
  -- Create 5x5 board
  CALL store_procedure_init_board(v_gameId, 5, 5);
  
  -- NEW: Place random items on the board
  -- Place 2 Apples in random locations
  INSERT INTO TileItem (TileID, ItemID, Quantity)
  SELECT t.TileID, 1, 1  -- ItemID 1 = Apple
  FROM Tile t
  WHERE t.GameID = v_gameId 
    AND NOT (t.X = 0 AND t.Y = 0)  -- Not on home tile
  ORDER BY RAND()
  LIMIT 2;
  
  -- Place 1 Medkit in random location
  INSERT INTO TileItem (TileID, ItemID, Quantity)
  SELECT t.TileID, 2, 1  -- ItemID 2 = Medkit
  FROM Tile t
  WHERE t.GameID = v_gameId 
    AND NOT (t.X = 0 AND t.Y = 0)
  ORDER BY RAND()
  LIMIT 1;
  
  -- Place 3 Gems in random locations
  INSERT INTO TileItem (TileID, ItemID, Quantity)
  SELECT t.TileID, 3, 1  -- ItemID 3 = Gem
  FROM Tile t
  WHERE t.GameID = v_gameId 
    AND NOT (t.X = 0 AND t.Y = 0)
  ORDER BY RAND()
  LIMIT 3;
  
  -- Place 2 Traps in random locations
  INSERT INTO TileItem (TileID, ItemID, Quantity)
  SELECT t.TileID, 4, 1  -- ItemID 4 = Trap
  FROM Tile t
  WHERE t.GameID = v_gameId 
    AND NOT (t.X = 0 AND t.Y = 0)
  ORDER BY RAND()
  LIMIT 2;
  
  IF error_happened THEN
    ROLLBACK;
    SELECT 'ERROR' AS Status, 'Failed to create game' AS Message;
  ELSE
    COMMIT;
    SELECT 'SUCCESS' AS Status, 'Game created successfully' AS Message, v_gameId AS GameID;
  END IF;
END$$

-- =========================================================
-- 15) Join Game
-- =========================================================
CREATE PROCEDURE store_procedure_join_game(
  IN p_playerId INT UNSIGNED,
  IN p_gameId INT UNSIGNED
)
BEGIN
  DECLARE v_homeTileId INT UNSIGNED;
  DECLARE v_gameStatus VARCHAR(20);
  DECLARE v_playerGameId INT UNSIGNED;
  DECLARE error_happened BOOLEAN DEFAULT FALSE;
  
  DECLARE CONTINUE HANDLER FOR SQLEXCEPTION
  BEGIN
    SET error_happened = TRUE;
  END;
  
  SET TRANSACTION ISOLATION LEVEL READ COMMITTED;
  START TRANSACTION;
  
  SELECT Status INTO v_gameStatus FROM Game WHERE GameID = p_gameId;
  
  IF error_happened THEN
    ROLLBACK;
    SELECT 'ERROR' AS Status, 'Database error' AS Message;
  ELSEIF v_gameStatus IS NULL THEN
  ROLLBACK;
    SELECT 'ERROR' AS Status, 'Game not found' AS Message;
  ELSEIF v_gameStatus IN ('Finished', 'Killed') THEN
    ROLLBACK;
    SELECT 'ERROR' AS Status, 'Game already ended' AS Message;
  ELSEIF EXISTS(SELECT 1 FROM PlayerGame WHERE PlayerID = p_playerId AND GameID = p_gameId) THEN
    SELECT PlayerGameID INTO v_playerGameId 
    FROM PlayerGame WHERE PlayerID = p_playerId AND GameID = p_gameId;
    COMMIT;
    SELECT 'ALREADY_JOINED' AS Status, 'Already in this game' AS Message, v_playerGameId AS PlayerGameID;
  ELSE
    SELECT TileID INTO v_homeTileId 
    FROM Tile WHERE GameID = p_gameId AND X = 0 AND Y = 0 AND TileType = 'Home' LIMIT 1;
    
    IF v_homeTileId IS NULL THEN
      ROLLBACK;
      SELECT 'ERROR' AS Status, 'Game board not initialized' AS Message;
    ELSE
      INSERT INTO PlayerGame (PlayerID, GameID, Score, HP, CurrentTileID, IsTurn, JoinedAt)
      VALUES (p_playerId, p_gameId, 0, 100, v_homeTileId, 0, CURDATE());
      
      SET v_playerGameId = LAST_INSERT_ID();
      UPDATE Game SET Status = 'Running' WHERE GameID = p_gameId AND Status = 'Waiting';
      
      IF error_happened THEN
        ROLLBACK;
        SELECT 'ERROR' AS Status, 'Failed to join' AS Message;
      ELSE
        COMMIT;
        SELECT 'SUCCESS' AS Status, 'Joined game successfully' AS Message, v_playerGameId AS PlayerGameID;
      END IF;
    END IF;
  END IF;
END$$

-- =========================================================
-- 16) Get Player Info (READ ONLY - no transaction needed)
-- =========================================================
CREATE PROCEDURE store_procedure_get_player_info(
  IN p_username VARCHAR(40)
)
BEGIN
  SELECT PlayerID, Username, IsAdmin, Highscore
  FROM Player WHERE Username = p_username;
END$$

DELIMITER ;

-- =========================================================
-- Initialize Database
-- =========================================================
CALL store_procedure_create_all_tables();
CALL store_procedure_insert_sample_data();