#!/usr/bin/env python3
"""
Scrape NZ secondary school progression-to-tertiary (institution-type) data from Education Counts.

Source page (per school):
  https://www.educationcounts.govt.nz/find-school/school/progression-to-tertiary/institution-type?school={schoolId}

Extracts from Table 1 ("one year after leaving school"):
  - Each ethnicity's count enrolled at Universities (本科) for a target leaver year (default 2023)
  - Total school leavers in that year (Total / Total row)

Requires:
  pip3 install -r Tools/requirements-scraper.txt
  python3 -m playwright install chromium

Example (macOS: use python3, not python):
  python3 Tools/scrape_tertiary_progression.py \\
    --db-path data/thisiscz-dev.db \\
    --output docs/csv/tertiary-progression-2023.csv \\
    --limit 5
"""

from __future__ import annotations

import argparse
import asyncio
import csv
import re
import sqlite3
import sys
from dataclasses import dataclass, field
from datetime import datetime, timezone
from pathlib import Path
from typing import Any

from bs4 import BeautifulSoup
from playwright.async_api import Browser, Page, async_playwright

BASE_URL = (
    "https://www.educationcounts.govt.nz"
    "/find-school/school/progression-to-tertiary/institution-type"
)

ETHNICITY_GROUPS = [
    "Asian",
    "European/Pākehā",
    "Māori",
    "Pacific",
    "MELAA",
    "Other",
    "International fee paying",
]

UNIVERSITY_INSTITUTION = "Universities"
TOTAL_INSTITUTION = "Total"

OUTPUT_COLUMNS = [
    "school_id",
    "school_name",
    "year_left_school",
    "total_leavers",
    "total_university",
    "asian_university",
    "european_pakeha_university",
    "maori_university",
    "pacific_university",
    "melaa_university",
    "other_university",
    "international_fee_paying_university",
    "asian_total_leavers",
    "european_pakeha_total_leavers",
    "maori_total_leavers",
    "pacific_total_leavers",
    "melaa_total_leavers",
    "other_total_leavers",
    "international_fee_paying_total_leavers",
    "scrape_status",
    "scrape_error",
    "scraped_at",
]

ETHNICITY_TO_COLUMN = {
    "Asian": ("asian_university", "asian_total_leavers"),
    "European/Pākehā": ("european_pakeha_university", "european_pakeha_total_leavers"),
    "Māori": ("maori_university", "maori_total_leavers"),
    "Pacific": ("pacific_university", "pacific_total_leavers"),
    "MELAA": ("melaa_university", "melaa_total_leavers"),
    "Other": ("other_university", "other_total_leavers"),
    "International fee paying": (
        "international_fee_paying_university",
        "international_fee_paying_total_leavers",
    ),
}


@dataclass
class SchoolRef:
    school_id: int
    name: str


@dataclass
class ScrapeResult:
    school_id: int
    school_name: str
    year_left_school: int
    values: dict[str, Any] = field(default_factory=dict)
    status: str = "ok"
    error: str = ""


def load_secondary_schools(db_path: Path) -> list[SchoolRef]:
    conn = sqlite3.connect(db_path)
    try:
        rows = conn.execute(
            """
            SELECT SchoolId, Name
            FROM schools
            WHERE lower(LevelClass) = 'secondary'
            ORDER BY SchoolId
            """
        ).fetchall()
    finally:
        conn.close()

    return [SchoolRef(school_id=row[0], name=row[1]) for row in rows]


def load_completed_school_ids(output_path: Path) -> set[int]:
    if not output_path.exists():
        return set()

    completed: set[int] = set()
    with output_path.open("r", encoding="utf-8", newline="") as f:
        reader = csv.DictReader(f)
        for row in reader:
            if row.get("scrape_status") == "ok" and row.get("school_id"):
                completed.add(int(row["school_id"]))
    return completed


def normalize_cell(value: str) -> str | None:
    text = value.strip()
    if not text or text.lower() == "x":
        return None
    return text


def find_year_column_index(rows: list[Any], target_year: int) -> int:
    """
    Education Counts uses a two-row header:
      Row 0: Group | Institution Type | Year left school (colspan)
      Row 1: 2021 | 2022 | 2023
    Data values for each year start at column index 2.
    """
    for row in rows[:3]:
        year_cells = [
            cell.get_text(" ", strip=True) for cell in row.find_all(["th", "td"])
        ]
        year_positions = [
            (idx, int(text))
            for idx, text in enumerate(year_cells)
            if re.fullmatch(r"\d{4}", text)
        ]
        if not year_positions:
            continue

        for offset, year in year_positions:
            if year == target_year:
                return 2 + offset

    available_years: list[int] = []
    for row in rows[:3]:
        for cell in row.find_all(["th", "td"]):
            text = cell.get_text(" ", strip=True)
            if re.fullmatch(r"\d{4}", text):
                available_years.append(int(text))

    available = ", ".join(str(y) for y in sorted(set(available_years)))
    raise ValueError(f"Target year {target_year} not found. Available years: {available}")


def parse_first_progression_table(html: str, target_year: int) -> dict[tuple[str, str], str | None]:
    soup = BeautifulSoup(html, "lxml")
    tables = soup.find_all("table")
    if not tables:
        raise ValueError("No tables found on page")

    table = tables[0]
    rows = table.find_all("tr")
    if len(rows) < 3:
        raise ValueError("Table has insufficient rows")

    year_idx = find_year_column_index(rows, target_year)
    parsed: dict[tuple[str, str], str | None] = {}

    for row in rows[2:]:
        cells = [cell.get_text(" ", strip=True) for cell in row.find_all(["th", "td"])]
        if len(cells) <= year_idx:
            continue

        group = cells[0].strip()
        institution = cells[1].strip() if len(cells) > 1 else ""
        if not group or not institution:
            continue

        parsed[(group, institution)] = normalize_cell(cells[year_idx])

    return parsed


def build_result_row(
    school: SchoolRef,
    target_year: int,
    parsed: dict[tuple[str, str], str | None],
) -> ScrapeResult:
    result = ScrapeResult(
        school_id=school.school_id,
        school_name=school.name,
        year_left_school=target_year,
    )

    total_leavers = parsed.get(("Total", TOTAL_INSTITUTION))
    total_university = parsed.get(("Total", UNIVERSITY_INSTITUTION))

    result.values = {
        "total_leavers": total_leavers,
        "total_university": total_university,
    }

    for ethnicity in ETHNICITY_GROUPS:
        uni_col, total_col = ETHNICITY_TO_COLUMN[ethnicity]
        result.values[uni_col] = parsed.get((ethnicity, UNIVERSITY_INSTITUTION))
        result.values[total_col] = parsed.get((ethnicity, TOTAL_INSTITUTION))

    if total_leavers is None and total_university is None:
        result.status = "no_data"
        result.error = f"No usable {target_year} data in table 1"

    return result


async def dismiss_cookie_banner(page: Page) -> None:
    for label in ("Allow cookies", "Decline"):
        button = page.get_by_role("button", name=label)
        if await button.count() > 0:
            await button.first.click()
            await page.wait_for_timeout(500)
            break


async def fetch_school_page(page: Page, school_id: int, wait_ms: int) -> str:
    url = f"{BASE_URL}?school={school_id}"
    last_error: Exception | None = None

    for attempt in range(1, 4):
        try:
            await page.goto(url, wait_until="domcontentloaded", timeout=120_000)
            await dismiss_cookie_banner(page)
            await page.wait_for_timeout(wait_ms)

            title = await page.title()
            if "couldn't find this school" in title.lower():
                raise ValueError("School not found on Education Counts")

            body_text = await page.inner_text("body")
            if "couldn't find this school" in body_text.lower():
                raise ValueError("School not found on Education Counts")

            if await page.query_selector("table") is None:
                await page.wait_for_selector("table", timeout=60_000, state="attached")

            return await page.content()
        except Exception as exc:  # noqa: BLE001 - retry transient Cloudflare/render delays
            last_error = exc
            await page.wait_for_timeout(3_000 * attempt)

    raise last_error or RuntimeError(f"Failed to load school {school_id}")


async def scrape_school(
    browser: Browser,
    school: SchoolRef,
    target_year: int,
    wait_ms: int,
    delay_ms: int,
) -> ScrapeResult:
    context = await browser.new_context(
        user_agent=(
            "Mozilla/5.0 (Macintosh; Intel Mac OS X 10_15_7) "
            "AppleWebKit/537.36 (KHTML, like Gecko) "
            "Chrome/120.0.0.0 Safari/537.36"
        ),
        locale="en-NZ",
    )
    page = await context.new_page()

    try:
        html = await fetch_school_page(page, school.school_id, wait_ms)
        parsed = parse_first_progression_table(html, target_year)
        return build_result_row(school, target_year, parsed)
    except Exception as exc:  # noqa: BLE001 - collect per-school failures
        return ScrapeResult(
            school_id=school.school_id,
            school_name=school.name,
            year_left_school=target_year,
            status="error",
            error=str(exc),
        )
    finally:
        await context.close()
        if delay_ms > 0:
            await asyncio.sleep(delay_ms / 1000)


def result_to_csv_row(result: ScrapeResult) -> dict[str, Any]:
    row: dict[str, Any] = {
        "school_id": result.school_id,
        "school_name": result.school_name,
        "year_left_school": result.year_left_school,
        "scrape_status": result.status,
        "scrape_error": result.error,
        "scraped_at": datetime.now(timezone.utc).isoformat(),
    }
    for col in OUTPUT_COLUMNS:
        if col in row:
            continue
        row[col] = result.values.get(col, "")
    return row


def write_header_if_needed(output_path: Path) -> None:
    if output_path.exists() and output_path.stat().st_size > 0:
        return

    output_path.parent.mkdir(parents=True, exist_ok=True)
    with output_path.open("w", encoding="utf-8", newline="") as f:
        writer = csv.DictWriter(f, fieldnames=OUTPUT_COLUMNS)
        writer.writeheader()


def append_result(output_path: Path, result: ScrapeResult) -> None:
    with output_path.open("a", encoding="utf-8", newline="") as f:
        writer = csv.DictWriter(f, fieldnames=OUTPUT_COLUMNS)
        writer.writerow(result_to_csv_row(result))


async def run(args: argparse.Namespace) -> int:
    db_path = Path(args.db_path)
    output_path = Path(args.output)

    if not db_path.exists():
        print(f"Database not found: {db_path}", file=sys.stderr)
        return 1

    schools = load_secondary_schools(db_path)
    if args.school_ids:
        wanted = {int(x) for x in args.school_ids}
        schools = [s for s in schools if s.school_id in wanted]

    if args.limit:
        schools = schools[: args.limit]

    if args.resume:
        done = load_completed_school_ids(output_path)
        schools = [s for s in schools if s.school_id not in done]

    if not schools:
        print("No schools to scrape.")
        return 0

    write_header_if_needed(output_path)

    print(f"Scraping {len(schools)} secondary schools -> {output_path}")

    async with async_playwright() as playwright:
        browser = await playwright.chromium.launch(
            headless=not args.headful,
            args=["--disable-blink-features=AutomationControlled"],
        )

        ok_count = 0
        error_count = 0

        for index, school in enumerate(schools, start=1):
            print(f"[{index}/{len(schools)}] school={school.school_id} {school.name}")
            result = await scrape_school(
                browser=browser,
                school=school,
                target_year=args.year,
                wait_ms=args.wait_ms,
                delay_ms=args.delay_ms,
            )
            append_result(output_path, result)

            if result.status == "ok":
                ok_count += 1
                print(
                    f"  ok: total_leavers={result.values.get('total_leavers')} "
                    f"total_university={result.values.get('total_university')}"
                )
            else:
                error_count += 1
                print(f"  {result.status}: {result.error}")

        await browser.close()

    print(f"Done. ok={ok_count}, failed={error_count}, output={output_path}")
    return 0 if error_count == 0 else 2


def parse_args() -> argparse.Namespace:
    parser = argparse.ArgumentParser(
        description="Scrape Education Counts tertiary progression (Universities) by ethnicity."
    )
    parser.add_argument(
        "--db-path",
        default="data/thisiscz-dev.db",
        help="SQLite database path containing schools table",
    )
    parser.add_argument(
        "--output",
        default="docs/csv/tertiary-progression-2023.csv",
        help="Output CSV path",
    )
    parser.add_argument(
        "--year",
        type=int,
        default=2023,
        help="Year left school column to extract (default: 2023)",
    )
    parser.add_argument(
        "--limit",
        type=int,
        default=0,
        help="Only scrape first N schools (0 = all)",
    )
    parser.add_argument(
        "--school-ids",
        nargs="*",
        type=int,
        help="Optional explicit school IDs to scrape",
    )
    parser.add_argument(
        "--resume",
        action="store_true",
        help="Skip school IDs already marked ok in output CSV",
    )
    parser.add_argument(
        "--delay-ms",
        type=int,
        default=2500,
        help="Delay between school requests",
    )
    parser.add_argument(
        "--wait-ms",
        type=int,
        default=12000,
        help="Wait after page load for table rendering",
    )
    parser.add_argument(
        "--headful",
        action="store_true",
        help="Run browser in headed mode (useful for debugging Cloudflare)",
    )
    return parser.parse_args()


def main() -> None:
    args = parse_args()
    raise SystemExit(asyncio.run(run(args)))


if __name__ == "__main__":
    main()
