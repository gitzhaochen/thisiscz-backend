-- Create school_tertiary_progression and migrate 2023 data from schools (PostgreSQL / SQLite)

CREATE TABLE IF NOT EXISTS school_tertiary_progression (
    "SchoolId" INTEGER NOT NULL,
    "Year" INTEGER NOT NULL,
    "TotalLeavers" INTEGER,
    "TotalUniversity" INTEGER,
    "AsianUniversity" INTEGER,
    "EuropeanPakehaUniversity" INTEGER,
    "MaoriUniversity" INTEGER,
    "PacificUniversity" INTEGER,
    "MelaaUniversity" INTEGER,
    "OtherUniversity" INTEGER,
    "InternationalFeePayingUniversity" INTEGER,
    "AsianTotalLeavers" INTEGER,
    "EuropeanPakehaTotalLeavers" INTEGER,
    "MaoriTotalLeavers" INTEGER,
    "PacificTotalLeavers" INTEGER,
    "MelaaTotalLeavers" INTEGER,
    "OtherTotalLeavers" INTEGER,
    "InternationalFeePayingTotalLeavers" INTEGER,
    "UeRate" DOUBLE PRECISION,
    "UpdatedAt" TIMESTAMP NOT NULL,
    PRIMARY KEY ("SchoolId", "Year"),
    FOREIGN KEY ("SchoolId") REFERENCES schools ("SchoolId") ON DELETE CASCADE
);

CREATE INDEX IF NOT EXISTS "IX_school_tertiary_progression_Year"
    ON school_tertiary_progression ("Year");

INSERT INTO school_tertiary_progression (
    "SchoolId",
    "Year",
    "TotalLeavers",
    "TotalUniversity",
    "AsianUniversity",
    "EuropeanPakehaUniversity",
    "MaoriUniversity",
    "PacificUniversity",
    "MelaaUniversity",
    "OtherUniversity",
    "InternationalFeePayingUniversity",
    "AsianTotalLeavers",
    "EuropeanPakehaTotalLeavers",
    "MaoriTotalLeavers",
    "PacificTotalLeavers",
    "MelaaTotalLeavers",
    "OtherTotalLeavers",
    "InternationalFeePayingTotalLeavers",
    "UeRate",
    "UpdatedAt"
)
SELECT
    "SchoolId",
    2023,
    "TotalLeavers2023",
    "TotalUniversity2023",
    "AsianUniversity2023",
    "EuropeanPakehaUniversity2023",
    "MaoriUniversity2023",
    "PacificUniversity2023",
    "MelaaUniversity2023",
    "OtherUniversity2023",
    "InternationalFeePayingUniversity2023",
    "AsianTotalLeavers2023",
    "EuropeanPakehaTotalLeavers2023",
    "MaoriTotalLeavers2023",
    "PacificTotalLeavers2023",
    "MelaaTotalLeavers2023",
    "OtherTotalLeavers2023",
    "InternationalFeePayingTotalLeavers2023",
    "UeRate",
    "UpdatedAt"
FROM schools
WHERE "TotalLeavers2023" IS NOT NULL
   OR "TotalUniversity2023" IS NOT NULL
   OR "AsianUniversity2023" IS NOT NULL
   OR "EuropeanPakehaUniversity2023" IS NOT NULL
   OR "MaoriUniversity2023" IS NOT NULL
   OR "PacificUniversity2023" IS NOT NULL
   OR "MelaaUniversity2023" IS NOT NULL
   OR "OtherUniversity2023" IS NOT NULL
   OR "InternationalFeePayingUniversity2023" IS NOT NULL
   OR "AsianTotalLeavers2023" IS NOT NULL
   OR "EuropeanPakehaTotalLeavers2023" IS NOT NULL
   OR "MaoriTotalLeavers2023" IS NOT NULL
   OR "PacificTotalLeavers2023" IS NOT NULL
   OR "MelaaTotalLeavers2023" IS NOT NULL
   OR "OtherTotalLeavers2023" IS NOT NULL
   OR "InternationalFeePayingTotalLeavers2023" IS NOT NULL
   OR "UeRate" IS NOT NULL;

ALTER TABLE schools DROP COLUMN "AsianTotalLeavers2023";
ALTER TABLE schools DROP COLUMN "AsianUniversity2023";
ALTER TABLE schools DROP COLUMN "EuropeanPakehaTotalLeavers2023";
ALTER TABLE schools DROP COLUMN "EuropeanPakehaUniversity2023";
ALTER TABLE schools DROP COLUMN "InternationalFeePayingTotalLeavers2023";
ALTER TABLE schools DROP COLUMN "InternationalFeePayingUniversity2023";
ALTER TABLE schools DROP COLUMN "MaoriTotalLeavers2023";
ALTER TABLE schools DROP COLUMN "MaoriUniversity2023";
ALTER TABLE schools DROP COLUMN "MelaaTotalLeavers2023";
ALTER TABLE schools DROP COLUMN "MelaaUniversity2023";
ALTER TABLE schools DROP COLUMN "OtherTotalLeavers2023";
ALTER TABLE schools DROP COLUMN "OtherUniversity2023";
ALTER TABLE schools DROP COLUMN "PacificTotalLeavers2023";
ALTER TABLE schools DROP COLUMN "PacificUniversity2023";
ALTER TABLE schools DROP COLUMN "TotalLeavers2023";
ALTER TABLE schools DROP COLUMN "TotalUniversity2023";
