--
-- ���� ������������ � ������� SQLiteStudio v3.2.1 � �� ��� 4 02:53:43 2020
--
-- �������������� ��������� ������: System
--
PRAGMA foreign_keys = off;
BEGIN TRANSACTION;

-- �������: last_generated_Id
DROP TABLE IF EXISTS last_generated_Id;
CREATE TABLE last_generated_Id (questId INTEGER NOT NULL, objectiveId INTEGER NOT NULL, answerId INTEGER NOT NULL, nodeId INTEGER NOT NULL);
INSERT INTO last_generated_Id (questId, objectiveId, answerId, nodeId) VALUES (1000, 2000, 3000, 4000);

COMMIT TRANSACTION;
PRAGMA foreign_keys = on;
