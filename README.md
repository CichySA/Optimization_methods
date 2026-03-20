# Optimization methods

## Navigation

- `PFSP/` - core .NET 10 library with algorithms, evaluators, and instance loading
- `ExperimentRunner/` - console app for running experiments
- `Analysis/` - Python notebooks and helpers for analysis
- `Instances/Taillard/` - Taillard sample PFSP problems used by tests and experiments
- `Instances/Custom/` - my custom instances (only the example from the starting task)
 - `Instances/Taillard/` - Taillard sample PFSP problems used by tests and experiments (source: [chneau/go-taillard](https://github.com/chneau/go-taillard))
 - `Instances/Custom/` - my custom instances (from om_project_starting_task.pdf)
 - `OM project PDFs` - project PDF files used for the assignment (see om_project_starting_task.pdf)
 - `Experiments/<experiment_name>/` - experiment outputs and configuration by experiment name

## Installation - don't trust my instructions, use common sense!
### Prerequisites
1. Install `uv`:
   ```powershell
   irm https://astral.sh/uv/install.ps1 | iex
   ```
2. Create a Python virtual environment:
   ```powershell
   uv venv
   ```
3. Sync Python dependencies:
   ```powershell
   uv sync
   ```
4. Install the .NET 10 SDK if it is not already installed.

### Run Jupyter
Start Jupyter using `uv`:
```powershell
uv run jupyter lab
```
There is also a Jupyter extension for VSCode.

You can run the C# project straight from the notebook by invoking the `dotnet` CLI from a notebook cell, for example:
```powershell
dotnet run --project .\PFSP\PFSP.csproj
```


### Build the Visual Studio solution
Restore and build the solution:
```powershell
dotnet restore
dotnet build
```

You can also start and debug the .NET solution directly from Visual Studio.
