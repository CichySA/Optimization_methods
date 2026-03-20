from __future__ import annotations

import json
from pathlib import Path
from typing import Iterable

import pandas as pd

SUPPORTED_SUFFIXES = {".csv", ".json", ".jsonl", ".parquet"}


def list_supported_experiment_files(
    results_dir: Path,
    supported_suffixes: Iterable[str] | None = None,
) -> list[Path]:
    """Return sorted experiment files under results_dir with supported suffixes."""
    suffixes = {s.lower() for s in (supported_suffixes or SUPPORTED_SUFFIXES)}
    return sorted(
        p
        for p in results_dir.rglob("*")
        if p.is_file() and p.suffix.lower() in suffixes
    )


def load_experiment_file(path: Path) -> pd.DataFrame:
    """Load CSV/Parquet/JSON/JSONL experiment output into a DataFrame."""
    suffix = path.suffix.lower()

    if suffix == ".csv":
        return pd.read_csv(path)
    if suffix == ".parquet":
        return pd.read_parquet(path)
    if suffix == ".json":
        data = json.loads(path.read_text(encoding="utf-8"))
        if isinstance(data, (list, dict)):
            return pd.json_normalize(data)
        return pd.DataFrame({"value": [data]})
    if suffix == ".jsonl":
        return pd.read_json(path, lines=True)

    raise ValueError(f"Unsupported file type: {path}")

def read_csv(path: Path) -> pd.DataFrame:
    """Read a CSV file into a DataFrame."""
    if not path.exists():
        raise FileNotFoundError(f"CSV file not found: {path.resolve()}")
    return pd.read_csv(path)

def read_first_existing_csv(candidate_paths: Iterable[Path]) -> tuple[Path, pd.DataFrame]:
    """Read the first existing CSV from candidate paths."""
    csv_path = next((p for p in candidate_paths if p.exists()), None)
    if csv_path is None:
        searched = "\n".join(str(p.resolve()) for p in candidate_paths)
        raise FileNotFoundError(
            f"Could not find CSV file. Searched:\n{searched}"
        )
    return csv_path, pd.read_csv(csv_path)
