import pandas as pd
import numpy as np
from scipy.stats import ttest_ind, mannwhitneyu
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

# Exclude 'Blue Cumulative Reward' (assuming column exists)
if 'Blue Cumulative Reward' in combined_df.columns:
    combined_df.drop('Blue Cumulative Reward', axis=1, inplace=True)

# Aggregate statistics by learning rate
aggregated_stats = combined_df.groupby('Learning Rate').agg(['mean', 'std'])

# Save aggregated statistics
aggregated_stats.to_csv('aggregated_statistics.csv')

# Perform a t-test and Mann-Whitney U test on key metrics
key_metrics = ['Physics Time (ms)', 'Main Thread Time (ms)', 'System Memory (MB)', 'Wall Time (ms)']
test_results = []

for metric in key_metrics:
    if metric in combined_df.columns:
        group_001 = combined_df[combined_df['Learning Rate'] == 0.001][metric]
        group_0001 = combined_df[combined_df['Learning Rate'] == 0.0001][metric]
        
        # Perform t-test
        t_stat, t_p_value = ttest_ind(group_001, group_0001, equal_var=False)
        
        # Perform Mann-Whitney U test
        u_stat, u_p_value = mannwhitneyu(group_001, group_0001, alternative='two-sided')
        
        test_results.append({
            'Metric': metric,
            'T-test Statistic': t_stat,
            'T-test P-value': t_p_value,
            'Mann-Whitney U Statistic': u_stat,
            'Mann-Whitney U P-value': u_p_value
        })

# Save test results
test_results_df = pd.DataFrame(test_results)
test_results_df.to_csv('statistical_test_results.csv', index=False)

# Plotting for visualization
sns.set(style="whitegrid")

for metric in key_metrics:
    if metric in combined_df.columns:
        plt.figure(figsize=(12, 8))
        sns.boxplot(data=combined_df, x='Learning Rate', y=metric)
        plt.title(f'Performance Comparison by Learning Rate: {metric}')
        plt.ylabel(metric)
        plt.savefig(f'performance_comparison_{metric.replace(" ", "_")}.png')
        plt.show()

print("Analysis complete. Results saved as:")
print("1. Aggregated statistics: aggregated_statistics.csv")
print("2. Statistical test results: statistical_test_results.csv")
print("3. Visualizations: performance_comparison_<metric>.png")