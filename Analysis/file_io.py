from __future__ import annotations

import json
from pathlib import Path
from typing import Iterable
from typing import Iterable, Optional, List

import pandas as pd

REPO_ROOT = Path(__file__).resolve().parent.parent
EXPERIMENTS_ROOT = REPO_ROOT / "Experiments"
EXPERIMENT_CONFIG_NAME = "experimentrunner.json"
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


def list_experiment_directories() -> List[Path]:
    """Return sorted experiment directories under ./Experiments."""
    if not EXPERIMENTS_ROOT.exists():
        return []
    return sorted(p for p in EXPERIMENTS_ROOT.iterdir() if p.is_dir())


def list_experiment_files(
    experiment_name: Optional[str] = None, supported_suffixes: Iterable[str] | None = None
) -> list[Path]:
    """List supported experiment output files.

    If `experiment_name` is provided, search only under `./Experiments/<experiment_name>`.
    Otherwise search recursively under `./Experiments`.
    """
    if experiment_name:
        base = resolve_experiment_directory(experiment_name)
    else:
        base = EXPERIMENTS_ROOT
    return list_supported_experiment_files(base, supported_suffixes=supported_suffixes)


def find_experiment_results_csv(experiment_name: Optional[str] = None) -> Path:
    """
    Locate the canonical `experiment_results.csv` file.

    If `experiment_name` is provided, prefer `./Experiments/<name>/experiment_results.csv`.
    Otherwise return the first matching `experiment_results.csv` found under `./Experiments`.
    """
    filename = "experiment_results.csv"
    if experiment_name:
        candidate = resolve_experiment_directory(experiment_name) / filename
        if candidate.exists():
            return candidate
        raise FileNotFoundError(f"{filename} not found for experiment: {experiment_name}")

    candidates = sorted(EXPERIMENTS_ROOT.rglob(filename)) if EXPERIMENTS_ROOT.exists() else []
    if not candidates:
        raise FileNotFoundError(f"No {filename} found under {EXPERIMENTS_ROOT.resolve()}")
    return candidates[0]


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


def resolve_experiment_directory(experiment_name: str) -> Path:
    """Return the canonical experiment directory under ./Experiments."""
    if not experiment_name:
        raise ValueError("experiment_name must not be empty")
    return EXPERIMENTS_ROOT / experiment_name


def resolve_experiment_config_path(experiment_name: str) -> Path:
    """Return the canonical experiment configuration path under ./Experiments/<name>."""
    return resolve_experiment_directory(experiment_name) / EXPERIMENT_CONFIG_NAME
