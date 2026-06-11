-- Add 2023 tertiary progression columns to schools (SQLite / PostgreSQL)

ALTER TABLE schools ADD COLUMN "TotalLeavers2023" INTEGER;
ALTER TABLE schools ADD COLUMN "TotalUniversity2023" INTEGER;
ALTER TABLE schools ADD COLUMN "AsianUniversity2023" INTEGER;
ALTER TABLE schools ADD COLUMN "EuropeanPakehaUniversity2023" INTEGER;
ALTER TABLE schools ADD COLUMN "MaoriUniversity2023" INTEGER;
ALTER TABLE schools ADD COLUMN "PacificUniversity2023" INTEGER;
ALTER TABLE schools ADD COLUMN "MelaaUniversity2023" INTEGER;
ALTER TABLE schools ADD COLUMN "OtherUniversity2023" INTEGER;
ALTER TABLE schools ADD COLUMN "InternationalFeePayingUniversity2023" INTEGER;
ALTER TABLE schools ADD COLUMN "UeRate" REAL;
