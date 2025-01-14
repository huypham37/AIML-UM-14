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

# Descriptive statistics for Main Thread Time and Wall Time grouped by Learning Rate
metrics = ['Main Thread Time (ms)', 'Wall Time (ms)']
aggregated_stats = combined_df.groupby('Learning Rate')[metrics].agg(['mean', 'median', 'std', 'min', 'max'])
aggregated_stats.to_csv('aggregated_statistics.csv')
print("Aggregated statistics saved to 'aggregated_statistics.csv'.")
print(aggregated_stats)

# Perform t-tests for each metric between learning rate groups
test_results = {}
for metric in metrics:
    if metric in combined_df.columns:
        group_001 = combined_df[combined_df['Learning Rate'] == 0.001][metric]
        group_0001 = combined_df[combined_df['Learning Rate'] == 0.0001][metric]
        
        t_stat, p_value = ttest_ind(group_001, group_0001, equal_var=False)
        test_results[metric] = (t_stat, p_value)
        print(f"\nT-test results for {metric}:")
        print(f"T-statistic = {t_stat:.4f}, P-value = {p_value:.4e}")
    else:
        print(f"\nMetric '{metric}' not found in data.")

# Save test results
with open('statistical_test_results.txt', 'w') as f:
    f.write("T-test results:\n")
    for metric, (t_stat, p_value) in test_results.items():
        f.write(f"\n{metric}:\n")
        f.write(f"T-statistic = {t_stat:.4f}\n")
        f.write(f"P-value = {p_value:.4e}\n")
print("\nStatistical test results saved to 'statistical_test_results.txt'.")

# Visualization: Boxplot for each metric by Learning Rate
sns.set(style="whitegrid")
for metric in metrics:
    if metric in combined_df.columns:
        plt.figure(figsize=(10, 6))
        sns.boxplot(data=combined_df, x='Learning Rate', y=metric)
        plt.title(f'Performance Comparison by Learning Rate ({metric})')
        plt.ylabel(metric)
        filename = f'performance_comparison_boxplot_{metric.replace(" ", "_").lower()}.png'
        plt.savefig(filename)
        plt.show()
        print(f"Boxplot saved as {filename}.")

# Visualization: Distribution of each metric by Learning Rate
for metric in metrics:
    if metric in combined_df.columns:
        plt.figure(figsize=(10, 6))
        sns.histplot(data=combined_df, x=metric, hue='Learning Rate', kde=True, bins=30)
        plt.title(f'Distribution of {metric} by Learning Rate')
        filename = f'performance_comparison_distribution_{metric.replace(" ", "_").lower()}.png'
        plt.savefig(filename)
        plt.show()
        print(f"Distribution plot saved as {filename}.")

print("\nAnalysis complete. Results saved as:")
print("1. Aggregated statistics: aggregated_statistics.csv")
print("2. Statistical test results: statistical_test_results.txt")
print("3. Boxplots and distribution plots for each metric.")
