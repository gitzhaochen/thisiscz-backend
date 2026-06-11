-- Add per-ethnicity 2023 total leavers columns to schools (SQLite / PostgreSQL)

ALTER TABLE schools ADD COLUMN "AsianTotalLeavers2023" INTEGER;
ALTER TABLE schools ADD COLUMN "EuropeanPakehaTotalLeavers2023" INTEGER;
ALTER TABLE schools ADD COLUMN "MaoriTotalLeavers2023" INTEGER;
ALTER TABLE schools ADD COLUMN "PacificTotalLeavers2023" INTEGER;
ALTER TABLE schools ADD COLUMN "MelaaTotalLeavers2023" INTEGER;
ALTER TABLE schools ADD COLUMN "OtherTotalLeavers2023" INTEGER;
ALTER TABLE schools ADD COLUMN "InternationalFeePayingTotalLeavers2023" INTEGER;
