import pandas as pd
import numpy as np
from scipy.stats import ttest_ind
import matplotlib.pyplot as plt
import seaborn as sns

# List of CSV files and their corresponding learning rates
files = [
    ('LucaRun1.csv', 0.001),
    ('LucaRun2.csv', 0.001),
    ('LucaRun3.csv', 0.001),
    ('LucaRun4.csv', 0.0001),
    ('LucaRun5.csv', 0.0001),
    ('LucaRun6.csv', 0.0001)
]

# Combine data from all runs
all_data = []
for file, learning_rate in files:
    df = pd.read_csv(file)
    df['Learning Rate'] = learning_rate
    all_data.append(df)

# Combine all runs into a single DataFrame
combined_df = pd.concat(all_data, ignore_index=True)

# Descriptive statistics for all columns grouped by learning rate
aggregated_stats = combined_df.groupby('Learning Rate').agg(['mean', 'median', 'std', 'min', 'max'])
aggregated_stats.to_csv('aggregated_statistics.csv')
print("Aggregated statistics saved to 'aggregated_statistics.csv'.")

# T-test for Main Thread Time (ms) between groups
if 'Main Thread Time (ms)' in combined_df.columns:
    group_001 = combined_df[combined_df['Learning Rate'] == 0.001]['Main Thread Time (ms)']
    group_0001 = combined_df[combined_df['Learning Rate'] == 0.0001]['Main Thread Time (ms)']
    
    t_stat, p_value = ttest_ind(group_001, group_0001, equal_var=False)
    print(f"T-test results for Main Thread Time (ms): t-statistic = {t_stat:.4f}, p-value = {p_value:.4e}")
    
    # Save test results
    with open('statistical_test_results_main_thread_time.txt', 'w') as f:
        f.write(f"T-test results for Main Thread Time (ms):\nT-statistic = {t_stat:.4f}\nP-value = {p_value:.4e}\n")
else:
    print("Key metric 'Main Thread Time (ms)' not found in data.")

# T-test for Wall Time (ms) between groups
if 'Wall Time (ms)' in combined_df.columns:
    group_001 = combined_df[combined_df['Learning Rate'] == 0.001]['Wall Time (ms)']
    group_0001 = combined_df[combined_df['Learning Rate'] == 0.0001]['Wall Time (ms)']
    
    t_stat, p_value = ttest_ind(group_001, group_0001, equal_var=False)
    print(f"T-test results for Wall Time (ms): t-statistic = {t_stat:.4f}, p-value = {p_value:.4e}")
    
    # Save test results
    with open('statistical_test_results_wall_time.txt', 'w') as f:
        f.write(f"T-test results for Wall Time (ms):\nT-statistic = {t_stat:.4f}\nP-value = {p_value:.4e}\n")
else:
    print("Key metric 'Wall Time (ms)' not found in data.")

# Visualization: Boxplot for Main Thread Time (ms) by Learning Rate
sns.set(style="whitegrid")
plt.figure(figsize=(10, 6))
sns.boxplot(data=combined_df, x='Learning Rate', y='Main Thread Time (ms)')
plt.title('Performance Comparison by Learning Rate: Main Thread Time (ms)')
plt.ylabel('Main Thread Time (ms)')
plt.savefig('performance_comparison_main_thread_time_boxplot.png')
plt.show()

# Visualization: Distribution of Main Thread Time (ms) by Learning Rate
plt.figure(figsize=(10, 6))
sns.histplot(data=combined_df, y='Main Thread Time (ms)', hue='Learning Rate', kde=True, bins=30)
plt.title('Distribution of Main Thread Time (ms) by Learning Rate')
plt.xlabel('Count')
plt.ylabel('Main Thread Time (ms)')
plt.savefig('performance_comparison_main_thread_time_distribution.png')
plt.show()

# Visualization: Boxplot for Wall Time (ms) by Learning Rate
plt.figure(figsize=(10, 6))
sns.boxplot(data=combined_df, x='Learning Rate', y='Wall Time (ms)')
plt.title('Performance Comparison by Learning Rate: Wall Time (ms)')
plt.ylabel('Wall Time (ms)')
plt.savefig('performance_comparison_wall_time_boxplot.png')
plt.show()

# Visualization: Distribution of Wall Time (ms) by Learning Rate
plt.figure(figsize=(10, 6))
sns.histplot(data=combined_df, x='Wall Time (ms)', hue='Learning Rate', kde=True, bins=30)
plt.title('Distribution of Wall Time (ms) by Learning Rate')
plt.savefig('performance_comparison_wall_time_distribution.png')
plt.show()

print("Analysis complete. Results saved as:")
print("1. Aggregated statistics: aggregated_statistics.csv")
print("2. Statistical test results for Main Thread Time: statistical_test_results_main_thread_time.txt")
print("3. Statistical test results for Wall Time: statistical_test_results_wall_time.txt")
print("4. Boxplot for Main Thread Time: performance_comparison_main_thread_time_boxplot.png")
print("5. Distribution plot for Main Thread Time: performance_comparison_main_thread_time_distribution.png")
print("6. Boxplot for Wall Time: performance_comparison_wall_time_boxplot.png")
print("7. Distribution plot for Wall Time: performance_comparison_wall_time_distribution.png")