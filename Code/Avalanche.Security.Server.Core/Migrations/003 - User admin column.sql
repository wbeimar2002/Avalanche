ALTER TABLE "Users"
ADD COLUMN "IsAdmin" INTEGER NOT NULL DEFAULT 0;

-- Set base seed users data
UPDATE "Users"
SET "IsAdmin" = 1
WHERE "UserName" = 'ism_admin' OR "UserName" = 'Administrator'
