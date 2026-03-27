# Optimization methods

## Navigation

- `PFSP/` - core .NET 10 library with algorithms, evaluators, and instance loading
- `ExperimentRunner/` - console app for running experiments
- `Analysis/` - Jupyter notebook and helpers for analysis
 - `Instances/Taillard/` - Taillard sample PFSP problems used by tests and experiments (source: [chneau/go-taillard](https://github.com/chneau/go-taillard))
 - `Instances/Custom/` - my custom instances (from om_project_starting_task.pdf)
 - `OM project PDFs` - project PDF files used for the assignment
 - `Experiments/<experiment_name>/` - experiment outputs and configuration by experiment name

## Installation - don't trust my instructions, use common sense!
### Prerequisites
1. Install `uv` (powershell):
   ```powershell
   irm https://astral.sh/uv/install.ps1 | iex
   ```
2. Create a Python virtual environment:

   ```powershell
   uv venv
   ```
3. Activate the virtual environment

- Command Prompt (cmd):
   ```cmd
   .\.venv\Scripts\activate.bat
   ```
- Powershell:
   ```powershell
   & .\.venv\Scripts\activate.bat
   ```
1. Sync Python dependencies:
   ```powershell
   uv sync
   ```
2. Install the .NET 10 SDK if it is not already installed.

### Run Jupyter
Start Jupyter using `uv` (i dont know if it works):
```powershell
uv run jupyter lab
```
There is also a Jupyter extension for **VSCode**.

You can run the experiment runner from a notebook cell by invoking the `dotnet` CLI with a runner config, for example:
```python
cmd = [
    "dotnet",
    "run",
    "--project",
    "ExperimentRunner",
    "--",
    "--config",
    str(config_path),
]

print("Running:", " ".join(cmd))
sa_run_result = subprocess.run(
    cmd,
    cwd=repo_root,
    capture_output=True,
    text=True,
    check=False,
)
```


### Build the Visual Studio solution
Restore and build the solution:
```powershell
dotnet restore
dotnet build
```

You can also start and debug the .NET solution directly from Visual Studio.
# go-taillard
Flowshop scheduling problem (Taillard instances) in Go

## Budget Calculation Formula

The evaluation budget (NFE — number of function evaluations) for one run of the Evolutionary Algorithm is:

```
B = P + G × (P − k)
```

where:
- `P` — population size
- `G` — number of generations
- `k` — elitism count (`k = 0` means no elitism; elites carry over without re-evaluation)

When `k = 0` this simplifies to `B = P × (G + 1)`.

**To compute G from a target budget B:**

```
G = floor((B − P) / (P − k))
```

Requires `P < B` and `0 ≤ k < P`.

**To compute P from a target budget B (no elitism, k = 0):**

```
P = floor(B / (G + 1))
```

**Examples:**

| B      | P   | k | G   |
|--------|-----|---|-----|
| 10 100 | 100 | 0 | 100 |
| 10 000 | 100 | 5 | 104 |
| 10 000 | 200 | 10| 51  |

The Experiment Runner accepts a `Budget` field at the config root (global) or inside each algorithm spec (local). A local value overrides the global one. When provided, it overrides any explicit `Generations` setting and computes `G` automatically using the formula above.

---

## experimentrunner.json Schema

The Experiment Runner is driven by a JSON configuration file. Pass its path with `--config <path>` or use a named experiment under `Experiments/<name>/experimentrunner.json`.

### Top-level fields

| Field | Type | Default | Description |
|---|---|---|---|
| `Instances` | `string[]` | six Taillard instances | Problem instances to solve. Each entry is a base name such as `"tai_20_5_0"`. |
| `OutDir` | `string` | `"experiment_results"` | Directory where result CSV files are written (relative to the working directory). |
| `Budget` | `integer` | _(none)_ | **Global** evaluation budget (NFE). Overridden by a per-algorithm `Budget`. |
| `TimeLimitMs` | `integer` | _(none)_ | **Global** wall-clock time limit in milliseconds. Overridden by a per-algorithm `TimeLimitMs`. |
| `Algorithms` | `object[]` | `[]` | List of algorithm specs to run. |

Every instance is run against every algorithm spec — the full cross-product.

### Algorithm spec fields

| Field | Type | Default | Description |
|---|---|---|---|
| `Type` | `string` | `"Random"` | Algorithm type. Case-insensitive. See types below. |
| `Iterations` | `integer` | `1` | Number of independent stochastic runs per instance. Deterministic algorithms (`Greedy`, `SPT`) always run once regardless of this value. |
| `Budget` | `integer` | _(none)_ | **Local** evaluation budget. Overrides the global `Budget` and any `EvaluationBudget` set inside `Parameters`. |
| `TimeLimitMs` | `integer` | _(none)_ | **Local** time limit in milliseconds. Overrides the global `TimeLimitMs`. |
| `Parameters` | `object` | algorithm defaults | Algorithm-specific parameters (see per-algorithm tables below). Omit entirely to use all defaults. |
| `Parameters.ParameterGrid` | `object` | _(none)_ | Parameter sweep axes. Values are expanded into multiple parameter sets (see below). |
| `Parameters.Product` | `string` | `

### Algorithm types

| `Type` | Description |
|---|---|
| `Random` | Random search — samples random permutations. |
| `Evolutionary` | Evolutionary algorithm with selection, crossover, and mutation. |
| `SimulatedAnnealing` | Simulated annealing with configurable neighborhood, acceptance, and cooling. |
| `Greedy` | Deterministic greedy (NEH heuristic). |
| `SPT` | Deterministic SPT (shortest processing time) heuristic. |

### Budget and time-limit precedence

The effective budget/time-limit for each algorithm run is resolved in priority order (highest first):

1. `AlgorithmSpec.Budget` / `AlgorithmSpec.TimeLimitMs` (per-algorithm config)
2. `EvaluationBudget` inside `Parameters` (algorithm-level parameter)
3. Root-level `Budget` / `TimeLimitMs` (global config)
4. Explicit `Generations` / `Iterations` / `Samples` inside `Parameters`
5. Algorithm defaults

For `Evolutionary`, the resolved budget is converted to `Generations` using `G = floor((B − P) / (P − k))`.
For `SimulatedAnnealing`, the resolved budget sets `Iterations = B` directly.
For `Random`, the resolved budget sets `Samples = B` directly; if a time limit is also active it takes precedence over the budget.

> **Note:** Parameter factories (`FromJson` / `Validate`) are pure — they do not silently rewrite parameters. All budget-driven overrides are applied explicitly by the Experiment Runner factory before validation. If you call `Validate` directly on parameters that have `EvaluationBudget > 0` set but an inconsistent `Generations`/`Iterations`, it will throw.

### Seed and multi-run reproducibility

For stochastic algorithms the `Seed` inside `Parameters` is the *base seed*:

- `Seed = 0` (or omitted): every run in the `Iterations` set uses seed `0`, producing identical results.
- `Seed ≠ 0`: run `i` (0-based) uses `Seed + i × 1_000_003`. This gives reproducible, well-separated sequences.

```json
{ "Seed": 42, ... }   // runs: 42, 1000045, 2000048, ...
```

### ParameterGrid2D

Defines a two-dimensional grid sweep. Provide exactly two named arrays; the runner produces every combination and merges each pair into the base `Parameters`.

```json
"ParameterGrid2D": {
  "PopulationSize": [50, 100],
  "Generations":    [50, 100]
}
```

Generates four runs: (50,50), (50,100), (100,50), (100,100). Grid values override same-named keys in `Parameters`. Cannot be used together with a budget-driven `Generations` override — set `EvaluationBudget` or `Budget` in `Parameters`/config to let the runner derive `Generations` automatically instead.

### Monitoring and metrics

Monitoring is configured inside `Parameters.Monitoring`:

```json
"Monitoring": {
  "Enabled": true,
  "EnabledMetrics": ["BestByGeneration", "ElapsedOnFinished"]
}
```

- `Enabled` (default `true`): set to `false` to disable all metric recording for this algorithm.
- `EnabledMetrics` (default `[]` = **all metrics**): list the metric names you want recorded. An empty list records every available metric.

**Available metrics**

| Metric | Applies to | Description |
|---|---|---|
| `Evaluations` | all | Total NFE used. |
| `ElapsedMs` | all | Wall time in milliseconds. |
| `BestFoundAtEvaluation` | all | NFE index at which the best solution was first found. |
| `BestCostByEvaluation` | all | Best cost at each evaluation (sparse indexed series). |
| `ElapsedOnFinished` | all | `(evaluations, elapsedMs)` pair on completion. |
| `Warnings` | all | String messages for soft constraint violations (e.g. over-budget). |
| `BestCostByIteration` | `SimulatedAnnealing` | Best cost after each SA iteration. |
| `CurrentCostByIteration` | `SimulatedAnnealing` | Current solution cost after each SA iteration. |
| `TemperatureByIteration` | `SimulatedAnnealing` | Temperature after each SA iteration. |
| `BestByGeneration` | `Evolutionary` | Best cost in the population after each generation. |
| `MedianByGeneration` | `Evolutionary` | Median cost in the population after each generation. |
| `DeviationByGeneration` | `Evolutionary` | Standard deviation of cost across the population after each generation. |
| `BestInPopulationByGeneration` | `Evolutionary` | Best individual cost each generation (alias view). |

### Evolutionary parameters

| Parameter | Type | Default | Description |
|---|---|---|---|
| `Seed` | `int` | `0` | Base random seed. |
| `PopulationSize` | `int` | `100` | Number of individuals per generation. |
| `Generations` | `int` | `100` | Number of generations. Overridden when a budget is active. |
| `ElitismK` | `int` | `0` | Number of best individuals carried over unchanged each generation (not re-evaluated). |
| `EvaluationBudget` | `long` | `0` | Parameter-level NFE budget. `0` = disabled. Overridden by config `Budget`. |
| `CrossoverRate` | `double` | `0.7` | Probability of applying crossover per offspring. |
| `MutationRate` | `double` | `0.1` | Probability of applying mutation per offspring. |
| `TournamentSize` | `int` | `5` | Tournament size for tournament selection. |
| `SelectionMethod` | `string` | `"Tournament"` | Selection operator. Options: `Tournament`. |
| `CrossoverMethod` | `string` | `"OX"` | Crossover operator. Options: `OX` (Order Crossover), `CX` (Cycle Crossover). |
| `MutationMethod` | `string` | `"Swap"` | Mutation operator. Options: `Swap`, `Insert`. |

### Simulated Annealing parameters

| Parameter | Type | Default | Description |
|---|---|---|---|
| `Seed` | `int` | `0` | Base random seed. |
| `Iterations` | `int` | `10000` | Number of SA iterations. Overridden when a budget is active. |
| `EvaluationBudget` | `long` | `0` | Parameter-level NFE budget. `0` = disabled. Overridden by config `Budget`. |
| `InitialTemperature` | `double` | `100.0` | Starting temperature. |
| `CoolingRate` | `double` | `0.995` | Per-iteration temperature multiplier (Exponential) or decrement base (Linear). Must be in `(0, 1)`. |
| `MinimumTemperature` | `double` | `0.0001` | Temperature floor; algorithm stops cooling below this. |
| `NeighborhoodOperator` | `string` | `"Swap"` | Neighborhood move. Options: `Swap`, `Insert`, `Reverse`. |
| `AcceptanceFunction` | `string` | `"Probabilistic"` | Acceptance criterion. Options: `Probabilistic` (Metropolis), `Threshold`. |
| `CoolingSchedule` | `string` | `"Exponential"` | Cooling schedule. Options: `Exponential`, `Linear`. |

### Random Search parameters

| Parameter | Type | Default | Description |
|---|---|---|---|
| `Seed` | `int` | `0` | Base random seed. |
| `Samples` | `int` | `100` | Number of random solutions to evaluate. Overridden by `Budget`. |
| `TimeLimitMs` | `int` | _(none)_ | Evaluate for this many milliseconds instead of a fixed sample count. Overridden by config `TimeLimitMs`. |

### Minimal example

```json
{
  "Instances": ["tai_20_5_0"],
  "OutDir": "results",
  "Budget": 10000,
  "Algorithms": [
    {
      "Type": "Evolutionary",
      "Iterations": 5,
      "Parameters": {
        "Seed": 42,
        "PopulationSize": 100,
        "ElitismK": 5
      }
    },
    {
      "Type": "SimulatedAnnealing",
      "Iterations": 5,
      "Parameters": {
        "Seed": 7,
        "CoolingSchedule": "Linear"
      }
    }
  ]
}
```

`Budget: 10000` derives `G = floor((10000 − 100) / (100 − 5)) = 104` generations for the EA and sets `Iterations = 10000` for SA. Five seeds are produced for each stochastic algorithm per instance.
