--
-- ���� ������������ � ������� SQLiteStudio v3.2.1 � �� ��� 22 00:07:55 2020
--
-- �������������� ��������� ������: System
--
PRAGMA foreign_keys = off;
BEGIN TRANSACTION;

-- �������: completed_quests_objectives
DROP TABLE IF EXISTS completed_quests_objectives;
CREATE TABLE completed_quests_objectives (Id INTEGER PRIMARY KEY NOT NULL, ObjectiveId INTEGER NOT NULL, Value INTEGER NOT NULL);

COMMIT TRANSACTION;
PRAGMA foreign_keys = on;
