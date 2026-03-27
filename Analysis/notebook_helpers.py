from __future__ import annotations

import json
from math import exp
import re
import subprocess
from pathlib import Path
from typing import Iterable, Optional, List

import matplotlib.colors as colors
import matplotlib.pyplot as plt
import numpy as np
import pandas as pd

REPO_ROOT = Path(__file__).resolve().parent.parent
EXPERIMENT_CONFIG_NAME = "experimentrunner.json"
SUPPORTED_SUFFIXES = {".csv", ".json", ".jsonl", ".parquet"}
EXPERIMENTS_ROOT = REPO_ROOT / "Experiments"

try:
    from scipy.interpolate import RegularGridInterpolator
    from scipy.ndimage import gaussian_filter
except Exception:  # optional dependency for smoothing/interpolation plots
    RegularGridInterpolator = None
    gaussian_filter = None

__all__ = [
    "json",
    "re",
    "subprocess",
    "Path",
    "np",
    "pd",
    "plt",
    "colors",
    "RegularGridInterpolator",
    "gaussian_filter",
    "REPO_ROOT",
    "EXPERIMENTS_ROOT",
    "EXPERIMENT_CONFIG_NAME",
    "SUPPORTED_SUFFIXES",
    "list_supported_experiment_files",
    "list_experiment_directories",
    "list_experiment_files",
    "find_experiment_results_csv",
    "load_experiment_file",
    "read_csv",
    "read_first_existing_csv",
    "resolve_experiment_directory",
    "resolve_experiment_config_path",
    "get_config_path",
    "load_experiment_config",
    "get_out_dir_from_config",
    "resolve_output_paths",
    "run_experiment",
    "require_file",
    "read_results_csv",
    "read_results_table",
    "read_results_json",
]


def list_supported_experiment_files(
    results_dir: Path,
    supported_suffixes: Iterable[str] | None = None,
) -> list[Path]:
    suffixes = {s.lower() for s in (supported_suffixes or SUPPORTED_SUFFIXES)}
    return sorted(
        p
        for p in results_dir.rglob("*")
        if p.is_file() and p.suffix.lower() in suffixes
    )


def list_experiment_directories() -> List[Path]:
    if not EXPERIMENTS_ROOT.exists():
        return []
    return sorted(p for p in EXPERIMENTS_ROOT.iterdir() if p.is_dir())


def list_experiment_files(
    experiment_name: Optional[str] = None,
    supported_suffixes: Iterable[str] | None = None,
) -> list[Path]:
    base = resolve_experiment_directory(experiment_name) if experiment_name else EXPERIMENTS_ROOT
    return list_supported_experiment_files(base, supported_suffixes=supported_suffixes)


def find_experiment_results_csv(experiment_name: Optional[str] = None) -> Path:
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
    if not path.exists():
        raise FileNotFoundError(f"CSV file not found: {path.resolve()}")
    return pd.read_csv(path)


def read_first_existing_csv(candidate_paths: Iterable[Path]) -> tuple[Path, pd.DataFrame]:
    csv_path = next((p for p in candidate_paths if p.exists()), None)
    if csv_path is None:
        searched = "\n".join(str(p.resolve()) for p in candidate_paths)
        raise FileNotFoundError(f"Could not find CSV file. Searched:\n{searched}")
    return csv_path, pd.read_csv(csv_path)


def resolve_experiment_directory(experiment_name: str) -> Path:
    if not experiment_name:
        raise ValueError("experiment_name must not be empty")
    return EXPERIMENTS_ROOT / experiment_name


def _safe_experiment_name(name: str) -> str:
    safe = re.sub(r'[<>:"/\\|?*\s]+', '_', name.strip())
    return safe or "unnamed_experiment"


def _experiment_name_from_config_path(config_path: Path) -> str:
    stem = config_path.stem
    if stem.lower() == "experimentrunner":
        name = config_path.parent.name
    else:
        name = stem

    if name.lower().startswith("config_"):
        name = name[len("config_"):]

    return _safe_experiment_name(name)


def resolve_experiment_config_path(experiment_ref: str) -> Path:
    if not experiment_ref:
        raise ValueError("experiment_ref must not be empty")

    ref_path = Path(experiment_ref)

    if ref_path.suffix.lower() == ".json":
        candidates = [
            ref_path,
            REPO_ROOT / ref_path,
            EXPERIMENTS_ROOT / ref_path,
        ]
    else:
        candidates = [
            EXPERIMENTS_ROOT / ref_path / EXPERIMENT_CONFIG_NAME,
            REPO_ROOT / ref_path / EXPERIMENT_CONFIG_NAME,
            REPO_ROOT / ref_path,
        ]

    for candidate in candidates:
        if candidate.exists():
            return candidate.resolve()

    return candidates[0].resolve()


def get_config_path(experiment_ref: str) -> Path:
    return resolve_experiment_config_path(experiment_ref)


def load_experiment_config(experiment_ref: str) -> tuple[Path, dict]:
    config_path = get_config_path(experiment_ref)
    if not config_path.exists():
        raise FileNotFoundError(f"Config not found: {config_path}")
    config = json.loads(config_path.read_text(encoding="utf-8"))
    return config_path, config


def get_out_dir_from_config(config: dict) -> str:
    out_dir = config.get("OutDir")
    if not out_dir:
        raise ValueError("Missing OutDir in experiment config")
    return str(out_dir)


def resolve_output_paths(experiment_ref: str) -> tuple[Path, Path, Path, Path]:
    config_path, config = load_experiment_config(experiment_ref)
    experiment_name = _experiment_name_from_config_path(config_path)
    out_dir_value = Path(get_out_dir_from_config(config))
    out_dir_name = out_dir_value.name

    candidates: list[Path] = []
    if out_dir_value.is_absolute():
        candidates.append(out_dir_value)
    else:
        candidates.append(REPO_ROOT / out_dir_value)
        candidates.append(EXPERIMENTS_ROOT / out_dir_value)
        candidates.append(REPO_ROOT / out_dir_name)
        candidates.append(EXPERIMENTS_ROOT / out_dir_name)

    out_dir = next((p for p in candidates if p.exists()), candidates[0])

    preferred_csv = out_dir / f"result_{experiment_name}.csv"
    preferred_json = out_dir / f"result_{experiment_name}.json"
    legacy_csv = out_dir / "experiment_results.csv"
    legacy_json = out_dir / "experiment_results.json"

    results_csv = preferred_csv if preferred_csv.exists() else legacy_csv
    results_json = preferred_json if preferred_json.exists() else legacy_json

    return (
        config_path,
        out_dir,
        results_csv,
        results_json,
    )


def run_experiment(experiment_ref: str, output = False) -> tuple[subprocess.CompletedProcess[str], Path, Path, Path, Path]:
    config_path, out_dir, results_csv, results_json = resolve_output_paths(experiment_ref)
    cmd = ["dotnet", "run", "--project", "ExperimentRunner", "--", "--config", str(config_path)]
    print("Running:", " ".join(cmd))
    result = subprocess.run(cmd, cwd=REPO_ROOT, capture_output=True, text=True, check=False)
    if output:
        print(result.stdout)
    if result.stderr.strip():
        print("STDERR:\n" + result.stderr)
    if result.returncode != 0:
        raise RuntimeError(f"ExperimentRunner failed with exit code {result.returncode}")
    return result, config_path, out_dir, results_csv, results_json


def require_file(path: Path) -> Path:
    if not path.exists():
        raise FileNotFoundError(f"Expected file not found: {path}")
    return path


def read_results_csv(experiment_name: str) -> pd.DataFrame:
    _, _, results_csv, _ = resolve_output_paths(experiment_name)
    require_file(results_csv)
    return pd.read_csv(results_csv)


def read_results_table(experiment_name: str) -> pd.DataFrame:
    _, _, results_csv, results_json = resolve_output_paths(experiment_name)
    if results_csv.exists():
        return pd.read_csv(results_csv)
    if results_json.exists():
        data = json.loads(results_json.read_text(encoding="utf-8"))
        if isinstance(data, list):
            return pd.json_normalize(data)
        if isinstance(data, dict):
            return pd.json_normalize([data])
    raise FileNotFoundError(
        f"Neither CSV nor JSON results found for experiment: {experiment_name}"
    )


def read_results_json(experiment_name: str) -> list[dict]:
    _, _, _, results_json = resolve_output_paths(experiment_name)
    require_file(results_json)
    return json.loads(results_json.read_text(encoding="utf-8"))
