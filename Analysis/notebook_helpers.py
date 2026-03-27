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
    "SUPPORTED_SUFFIXES",
    "list_experiment_directories",
    "resolve_config_path",
    "load_config",
    "resolve_result_path",

    "load_result",
    "run_experiment",
]


def list_experiment_directories() -> List[Path]:
    if not EXPERIMENTS_ROOT.exists():
        return []
    return sorted(p for p in EXPERIMENTS_ROOT.iterdir() if p.is_dir())

# Replace invalid filename characters with underscores and trim whitespace
def _safe_experiment_name(name: str) -> str:
    safe = re.sub(r'[<>:"/\\|?*\s]+', '_', name.strip())
    return safe or "unnamed_experiment"

# Trim `config_` or `result_` prefix from filename
def _experiment_name_from_file_path(file_path: Path) -> str:
    stem = file_path.stem

    if stem.lower().startswith("config_"):
        name = stem[len("config_"):]
    elif stem.lower().startswith("result_"):
        name = stem[len("result_"):]
    else:
        name = stem

    return _safe_experiment_name(name)

# Return name from config "Name" field if set, otherwise derive from config filename
def _effective_experiment_name(config_path: Path, config: dict) -> str:
    derived_name = _experiment_name_from_file_path(config_path)
    configured_name = _safe_experiment_name(str(config.get("Name") or ""))
    return derived_name if configured_name == "unnamed_experiment" else configured_name


def _resolve_out_dir_path(outdir: str | Path) -> Path:
    out_dir_path = Path(str(outdir))
    normalized = out_dir_path if out_dir_path.is_absolute() else (EXPERIMENTS_ROOT / out_dir_path)
    return normalized.resolve()


def resolve_config_path(out_dir: str, experiment_name: str) -> Path:
    out_dir_path = _resolve_out_dir_path(out_dir)
    ref_path = Path(experiment_name)
    if not ref_path.suffix:
        ref_path = ref_path.with_suffix(".json")
    config_path = (out_dir_path / ref_path.name).resolve()

    if not config_path.exists():
        raise FileNotFoundError(f"Config not found: {config_path}")

    return config_path


def resolve_result_path(outdir: str | Path, experiment_ref: str) -> Path:
    out_dir_path = _resolve_out_dir_path(outdir)
    ref_path = Path(experiment_ref)
    if not ref_path.suffix:
        ref_path = ref_path.with_suffix(".json")

    if not ref_path.stem.lower().startswith("result_"):
        ref_path = ref_path.with_name(f"result_{ref_path.stem}{ref_path.suffix}")

    return (out_dir_path / ref_path.name).resolve()


def load_result(result_or_out_dir: Path | str, experiment_name: Optional[str] = None) -> pd.DataFrame:
    if experiment_name is None:
        result_path = Path(str(result_or_out_dir)).resolve()
    else:
        result_path = resolve_result_path(str(result_or_out_dir), experiment_name)

    if not result_path.exists():
        raise FileNotFoundError(f"Expected file not found: {result_path}")

    suffix = result_path.suffix.lower()
    if suffix == ".csv":
        return pd.read_csv(result_path)
    if suffix == ".parquet":
        return pd.read_parquet(result_path)
    if suffix == ".json":
        data = json.loads(result_path.read_text(encoding="utf-8"))
        if isinstance(data, list):
            return pd.json_normalize(data)
        if isinstance(data, dict):
            return pd.json_normalize([data])
        return pd.DataFrame({"value": [data]})
    if suffix == ".jsonl":
        return pd.read_json(result_path, lines=True)

    raise ValueError(f"Unsupported file type: {result_path}")


def load_config(out_dir: str, config_name: str) -> tuple[Path, dict]:
    config_path = resolve_config_path(out_dir, config_name)
    config = json.loads(config_path.read_text(encoding="utf-8"))
    return config_path, config


def run_experiment(out_dir: str, config_name: str, output: bool = False) -> tuple[str, Path, Path]:
    out_dir_path = _resolve_out_dir_path(out_dir)
    config_path, config = load_config(out_dir, config_name)
    experiment_name = _effective_experiment_name(config_path, config)
    cmd = ["dotnet", "run", "--project", "ExperimentRunner", "--", "--config", str(config_path)]
    print("Running:", " ".join(cmd))
    result = subprocess.run(cmd, cwd=REPO_ROOT, capture_output=True, text=True, check=False)
    if output:
        print(result.stdout)
    if result.stderr.strip():
        print("STDERR:\n" + result.stderr)
    if result.returncode != 0:
        raise RuntimeError(f"ExperimentRunner failed with exit code {result.returncode}")

    result_path = resolve_result_path(out_dir_path, experiment_name)
    generated_config_path = (out_dir_path / f"config_{experiment_name}.json").resolve()
    if not generated_config_path.exists():
        fallback_name = _experiment_name_from_file_path(Path(config_name))
        generated_config_path = (out_dir_path / f"config_{fallback_name}.json").resolve()

    return experiment_name, generated_config_path, result_path


