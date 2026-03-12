-- SQL Script to manually fix the QADefects table in Production
-- Error: Unknown column 'q.ResponsibleUserId' in 'field list'

USE GestionProduccionDB;

-- 1. Add the ResponsibleUserId column (Nullable for compatibility)
ALTER TABLE QADefects ADD COLUMN ResponsibleUserId INT NULL;

-- 2. Add the Foreign Key constraint to the Users table
-- Note: Assuming the table name is 'Users', adjust if necessary based on your schema
ALTER TABLE QADefects 
ADD CONSTRAINT FK_QADefects_Users_ResponsibleUserId 
FOREIGN KEY (ResponsibleUserId) REFERENCES Users(Id) 
ON DELETE SET NULL;

-- 3. Verify the change
DESCRIBE QADefects;
