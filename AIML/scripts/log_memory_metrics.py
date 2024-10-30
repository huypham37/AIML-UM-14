from torch.utils.tensorboard import SummaryWriter
import psutil  # Add this to import the psutil module
import GPUtil
import time

writer = SummaryWriter(log_dir="/Users/mac/Documents/UM/Year_2/PROJECT2_1/AIML/results/Soccer_twos_ppo_1")

def log_system_metrics():
    step = 0
    while True:
        cpu_usage = psutil.cpu_percent(interval=1)
        ram_usage = psutil.virtual_memory().percent
        gpus = GPUtil.getGPUs()
        gpu_usage = gpus[0].load * 100 if gpus else 0

        # Log to TensorBoard
        writer.add_scalar("CPU Usage", cpu_usage, step)
        writer.add_scalar("RAM Usage", ram_usage, step)
        writer.add_scalar("GPU Usage", gpu_usage, step)

        print(f"CPU: {cpu_usage}% | RAM: {ram_usage}% | GPU: {gpu_usage}%")
        time.sleep(5)
        step += 1

    writer.close()

log_system_metrics()