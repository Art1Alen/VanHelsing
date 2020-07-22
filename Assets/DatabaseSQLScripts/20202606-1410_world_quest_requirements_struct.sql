--
-- ���� ������������ � ������� SQLiteStudio v3.2.1 � �� ��� 26 14:01:59 2020
--
-- �������������� ��������� ������: System
--
PRAGMA foreign_keys = off;
BEGIN TRANSACTION;

-- �������: quest_requirements
DROP TABLE IF EXISTS quest_requirements;
CREATE TABLE quest_requirements (Id INTEGER PRIMARY KEY AUTOINCREMENT NOT NULL, TargetQuestId INTEGER REFERENCES quest (Id) ON DELETE CASCADE NOT NULL, RequiredQuest INTEGER REFERENCES quest (Id) ON DELETE CASCADE NOT NULL, ForbiddenQuest INTEGER REFERENCES quest (Id) ON DELETE CASCADE NOT NULL);
INSERT INTO quest_requirements (Id, TargetQuestId, RequiredQuest, ForbiddenQuest) VALUES (1, 2, 1, NULL);

COMMIT TRANSACTION;
PRAGMA foreign_keys = on;
