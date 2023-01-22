datenbank ersetllen

rem -- CREATE TABLE users (id UUID PRIMARY KEY DEFAULT gen_random_uuid(), username VARCHAR(255) NOT NULL, password VARCHAR(255) NOT NULL, coins INT DEFAULT 20, bio TEXT DEFAULT NULL, image VARCHAR(255) DEFAULT NULL);

rem -- select * from users

rem -- CREATE TABLE cards (id UUID PRIMARY KEY DEFAULT gen_random_uuid(), owner UUID NOT NULL, name VARCHAR(255) NOT NULL, damage INT NOT NULL, depot VARCHAR(255) DEFAULT 'stack');
rem -- select * from cards

rem -- CREATE TABLE stats (id UUID PRIMARY KEY DEFAULT gen_random_uuid(), userid UUID NOT NULL, elo INT DEFAULT 100, wins INT DEFAULT 0, loses INT DEFAULT 0, draws INT DEFAULT 0);


rem -- select * from stats

rem -- CREATE TABLE store (id UUID PRIMARY KEY DEFAULT gen_random_uuid(), card1 JSON NOT NULL, card2 JSON NOT NULL, card3 JSON NOT NULL, card4 JSON NOT NULL, card5 JSON NOT NULL);

rem -- select * from store

rem -- ALTER TABLE users RENAME COLUMN username TO name;


rem -- select * from store

rem select * from users


rem CREATE TABLE trades (
rem id UUID DEFAULT gen_random_uuid() PRIMARY KEY,
rem userid UUID NOT NULL REFERENCES users(id),
rem cardid UUID NOT NULL REFERENCES cards(id),
rem tradinginfo TEXT NOT NULL
rem );