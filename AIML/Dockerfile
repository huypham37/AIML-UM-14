# Base image: Official ML-Agents setup (CPU-only)
FROM ubuntu:20.04

# Install necessary packages
RUN apt-get update && \
    apt-get install -y --no-install-recommends wget curl vim git build-essential python3-pip unzip \
    xvfb libgl1-mesa-glx libgl1-mesa-dri libglu1-mesa && \
    rm -rf /var/lib/apt/lists/*

# Set up Python and ML-Agents dependencies
RUN python3 -m pip install --upgrade pip
RUN pip install setuptools==41.0.0

# Install ML-Agents dependencies
RUN pip install mlagents tensorflow

# Set environment variables for virtual display
ENV DISPLAY=:99

# Start Xvfb in the background to enable virtual rendering
RUN Xvfb :99 -screen 0 1024x768x16 &

# Optional: Copy your project files into the Docker container
# WORKDIR /workspace
# COPY . /workspace

# Launch ML-Agents Training
CMD ["mlagents-learn", "/workspace/trainer_config.yaml", "--train", "--run-id=trial_run"]
