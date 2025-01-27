# Unity ML-Agents SoccerTwos Performance Analysis

This repository contains our implementation and analysis of the Unity ML-Agents SoccerTwos environment, focusing on training performance and computational efficiency across different configurations.

## Prerequisites

- Unity (2022.3.0f1 or later)
- Python 3.10.11 or 3.10.12
- ML-Agents Release 20
- Git
- Visual Studio (Windows users)

## Installation

### Basic Installation

1. Clone the repository:
```bash
git clone --branch fix-numpy-release-21-branch https://github.com/DennisSoemers/ml-agents.git
```

2. Create and activate a Python virtual environment:
```bash
python -m venv venv
# On Windows
.\venv\Scripts\activate
# On Unix or MacOS
source venv/bin/activate
```

3. Install dependencies:
```bash
pip install -r requirements.txt
```

4. Install the ML-Agents package in Unity through the Package Manager:
   - Window → Package Manager → Add package from git URL
   - Enter: `com.unity.ml-agents`

### Detailed Installation

For more detailed installation instruction,

[[Official installation guide](https://unity-technologies.github.io/ml-agents/Installation/)]

## Training the Model

### In-Editor Training

1. Open the SoccerTwos scene in Unity Editor
2. Ensure ML-Agents is properly configured in the Unity Editor
3. Run the training using:
```bash
mlagents-learn config/poca/SoccerTwos.yaml --run-id= YOUR-RUN-ID
```

### Build Training (Executable)

1. Build the Unity project:
   - File → Build Settings
   - Add open scenes
   - Build the project
2. Run training on the built executable:
```bash
mlagents-learn config/poca/SoccerTwos.yaml --env=./Build/SoccerTwos --run-id= YOUR-RUN-ID
```

## Loading Trained Models

1. After training, models are saved in the `results` directory
2. In Unity:
   - Select the agent GameObject
   - In the Behavior Parameters component
   - Set Behavior Type to "Inference Only"
   - Drag the .onnx model file to Model field

## Performance Profiling

### Using Unity Profiler

1. Enable the Profiler:
   - Window → Analysis → Profiler
   - Select "Deep Profile" for detailed analysis
   
2. Key metrics to monitor:
   - CPU Usage
   - Physics Processing Time
   - Main Thread Time
   - Memory Usage

3. Start profiling:
   - Click Record button in Profiler window
   - Run your scene
   - Stop recording when done

### Experiment Configuration

Our experiments use the following parameter ranges:

- Learning Rate: {1×10^-4, 1×10^-3}
- Batch Size: {512, 5120}
- Buffer Size: {10000, 409600}

To replicate our experiments:

1. Change the default configuration to certain value, keep the rest of the config file intact.

2. Monitor training progress:
```bash
tensorboard --logdir results
```

## Data Collection

Metrics are sampled at 30-second intervals during training sessions. Each parameter combination is tested in 3 independent runs for statistical significance.

## Results Analysis

Results and analysis scripts can be found in the `stats` directory. Use the provided Jupyter notebooks to reproduce our analysis:

```bash
jupyter notebook Project/stats
```

## Contributing

Please read [CONTRIBUTING.md](CONTRIBUTING.md) for details on our code of conduct and the process for submitting pull requests.

## License

This project is licensed under the MIT License - see the [LICENSE.md](LICENSE.md) file for details.

## Acknowledgments

- Unity ML-Agents team
- Project supervisor: Dennis Soemers.
- Maastricht University, Department of Advanced Computing Sciences.
