# Optimization methods

## Navigation

- `PFSP/` - core .NET 10 library with algorithms, evaluators, and instance loading
- `ExperimentRunner/` - console app for running experiments
- `Analysis/` - Jupyter notebook and helpers for analysis
- `Instances/Taillard/` - Taillard sample PFSP problems used by tests and experiments
- `Instances/Custom/` - my custom instances (only the example from the starting task)
 - `Instances/Taillard/` - Taillard sample PFSP problems used by tests and experiments (source: [chneau/go-taillard](https://github.com/chneau/go-taillard))
 - `Instances/Custom/` - my custom instances (from om_project_starting_task.pdf)
 - `OM project PDFs` - project PDF files used for the assignment (see om_project_starting_task.pdf)
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
