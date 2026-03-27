from __future__ import annotations

from pathlib import Path
from typing import Iterable, Optional

import pandas as pd

import notebook_helpers as nh


def find_experiment_results_csv(experiment_name: Optional[str] = None) -> Path:
    filename = "experiment_results.csv"
    if experiment_name:
        candidate = nh.resolve_experiment_directory(experiment_name) / filename
        if candidate.exists():
            return candidate
        raise FileNotFoundError(f"{filename} not found for experiment: {experiment_name}")

    candidates = sorted(nh.EXPERIMENTS_ROOT.rglob(filename)) if nh.EXPERIMENTS_ROOT.exists() else []
    if not candidates:
        raise FileNotFoundError(f"No {filename} found under {nh.EXPERIMENTS_ROOT.resolve()}")
    return candidates[0]


def read_csv(path: Path) -> pd.DataFrame:
    if not path.exists():
        raise FileNotFoundError(f"CSV file not found: {path.resolve()}")
    return pd.read_csv(path)


def read_first_existing_csv(candidate_paths: Iterable[Path]) -> tuple[Path, pd.DataFrame]:
    csv_path = next((p for p in candidate_paths if p.exists()), None)
    if csv_path is None:
        searched = "\n".join(str(p.resolve()) for p in candidate_paths)
        raise FileNotFoundError(f"Could not find CSV file. Searched:\n{searched}")
    return csv_path, pd.read_csv(csv_path)


def resolve_results_csv_path(experiment_ref: str) -> Path:
    results_json = nh.resolve_result_path(experiment_ref)
    out_dir = results_json.parent
    preferred_csv = results_json.with_suffix(".csv")
    legacy_csv = out_dir / "experiment_results.csv"
    return preferred_csv if preferred_csv.exists() else legacy_csv


def read_results_csv(experiment_ref: str) -> pd.DataFrame:
    results_csv = resolve_results_csv_path(experiment_ref)
    if not results_csv.exists():
        raise FileNotFoundError(f"CSV results not found: {results_csv}")
    return pd.read_csv(results_csv)
