# QualDivBenchmark

This project is the toy domain from the paper "Covariance Matrix Adaptation for the Rapid Illumination of Behavior Space". The project includes parallelizable implementations of CMA-ES, MAP-Elites, and the new CMA-ME algorithm detailed in the paper.

We test each algorithm against classic test functions Sphere and Rastrigin described in the paper. Despite the name, this project is not a official benchmark and merely tests how well each algorithm can cover a behavior space that is a linear projection from parameter space.

## Installation

To install the project, you need to install the [.NET](https://dotnet.microsoft.com/download) developer toolkit for linux or windows. You may also need the [NuGet](https://docs.microsoft.com/en-us/nuget/install-nuget-client-tools) client tools for updating dependencies for the project.

The first step is running the following commands. These will restore dependency dlls on your system and create a directory to save log files for the experiment. Dependencies are [Math.Net](https://numerics.mathdotnet.com/) which is a linear algebra library needed for Eigen decomposition and [Nett](https://github.com/paiden/Nett) which is used for TOML configuration files.

```
nuget restore
mkdir logs
```

Now we need to build the project.

```
dotnet build -c Release
```

Then running an experiment is as easy as running the associated config file. Let's run the CMA-ES Sphere experiment optimizing 20 parameters.

```
dotnet run config/sphere20/sphere_cma_es.tml
```

The first paramter is the config file of the experiment. Provided are all config files for all experiments run in the paper. Simply change the config file to run a different experiment. CSV log files for individuals and the map of elites are stored in the `logs` folder.

## Configuration

The number of trials and parameters of the experiment are configurable through the TOML file. You can change the optimizing function or algorithm here. To change search specific parameters, modify the separate config file for each search.

```
NumTrials = 30
NumParams = 20
FunctionType = "Sphere"

[Search]
Type = "CMA-ES"
ConfigFilename = "config/cma_es_config.tml"
```

Below is the config file for CMA-ES. Note that parameters like mutation power and population size are configurable in this file.

```
OverflowFactor = 1.0
NumParents = 250
PopulationSize = 500
NumToEvaluate = 50000
MutationPower = 0.5
```
