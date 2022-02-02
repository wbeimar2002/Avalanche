-- NOTE: tokenize="unicode61 remove_diacritics 2" is to control comparison behavior - using default unicode tokenizer, ignoring diacritics.
-- the default value of "remove_diacritics 1" has a bug with certain codepoints containing multiple diacritics.

-- NOTE: this is configured to use "Users" table as external content (see https://sqlite.org/fts5.html section 4.4.2).
CREATE VIRTUAL TABLE UsersFts USING fts5 (
    Id UNINDEXED,
    FirstName,
    LastName,
    UserName,
    content=Users,
    content_rowid=Id,
    tokenize="unicode61 remove_diacritics 2"
);

CREATE TRIGGER UsersAfterInsert AFTER INSERT on Users BEGIN
    INSERT INTO UsersFts(
        rowid,
        FirstName,
        LastName,
        UserName
    )
    VALUES(
        new.Id,
        new.FirstName,
        new.LastName,
        new.UserName
    );
END;

CREATE TRIGGER UsersAfterUpdate AFTER UPDATE on Users BEGIN
    INSERT INTO UsersFts(
        UsersFts,
        rowid,
        FirstName,
        LastName,
        UserName
	)
    VALUES(
        'delete',
        old.Id,
        old.FirstName,
        old.LastName,
        old.UserName
    );
END;

CREATE TRIGGER UsersAfterDelete AFTER DELETE on Users BEGIN
    INSERT INTO UsersFts(
        UsersFts,
        rowid,
        FirstName,
        LastName,
        UserName
	)
    VALUES(
        'delete',
        old.Id,
        old.FirstName,
        old.LastName,
        old.UserName
	);
END;
