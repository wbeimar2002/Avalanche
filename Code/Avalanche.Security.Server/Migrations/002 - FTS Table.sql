-- NOTE: tokenize="unicode61 remove_diacritics 2" is to control comparison behavior - using default unicode tokenizer, ignoring diacritics. 
-- the default value of "remove_diacritics 1" has a bug with certain codepoints containing multiple diacritics.


-- NOTE: this is configured to use "Notes" table as external content (see https://sqlite.org/fts5.html section 4.4.2).
CREATE VIRTUAL TABLE UserFts USING fts5(
    Id UNINDEXED,
    FirstName,
    LastName,
    LoginName,
    content=Users,
    content_rowid=Id,
    tokenize="unicode61 remove_diacritics 2"
);

CREATE TRIGGER UserAfterInsert AFTER INSERT on Users BEGIN
    INSERT INTO UserFts(
        rowid,
        FirstName,
        LastName,
        LoginName
    )
    VALUES(
        new.Id,
        new.FirstName,
        new.LastName,
        new.LoginName
    );
END;

CREATE TRIGGER UserAfterUpdate AFTER UPDATE on Users BEGIN
    INSERT INTO UserFts(
        rowid,
        FirstName,
        LastName,
        LoginName
	)
    VALUES(
        'delete',
        old.Id,
        old.FirstName,
        old.LastName,
        old.LoginName
    );
END;

CREATE TRIGGER UserAfterDelete AFTER DELETE on Users BEGIN
    INSERT INTO UserFts(
        rowid,
        FirstName,
        LastName,
        LoginName
	)
    VALUES(
        'delete',
        old.Id,
        old.FirstName,
        old.LastName,
        old.LoginName
    );
END;
