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

# T-test for Blue Cumulative Reward between groups
if 'Blue Cumulative Reward' in combined_df.columns:
    group_001 = combined_df[combined_df['Learning Rate'] == 0.001]['Blue Cumulative Reward']
    group_0001 = combined_df[combined_df['Learning Rate'] == 0.0001]['Blue Cumulative Reward']
    
    t_stat, p_value = ttest_ind(group_001, group_0001, equal_var=False)
    print(f"T-test results: t-statistic = {t_stat:.4f}, p-value = {p_value:.4e}")
    
    # Save test results
    with open('statistical_test_results.txt', 'w') as f:
        f.write(f"T-test results:\nT-statistic = {t_stat:.4f}\nP-value = {p_value:.4e}\n")
else:
    print("Key metric 'Blue Cumulative Reward' not found in data.")

# Visualization: Boxplot for Blue Cumulative Reward by Learning Rate
sns.set(style="whitegrid")
plt.figure(figsize=(10, 6))
sns.boxplot(data=combined_df, x='Learning Rate', y='Blue Cumulative Reward')
plt.title('Performance Comparison by Learning Rate')
plt.ylabel('Blue Cumulative Reward')
plt.savefig('performance_comparison_boxplot.png')
plt.show()

# Visualization: Distribution of Blue Cumulative Reward by Learning Rate
plt.figure(figsize=(10, 6))
sns.histplot(data=combined_df, x='Blue Cumulative Reward', hue='Learning Rate', kde=True, bins=30)
plt.title('Distribution of Blue Cumulative Reward by Learning Rate')
plt.savefig('performance_comparison_distribution.png')
plt.show()

print("Analysis complete. Results saved as:")
print("1. Aggregated statistics: aggregated_statistics.csv")
print("2. Statistical test results: statistical_test_results.txt")
print("3. Boxplot: performance_comparison_boxplot.png")
print("4. Distribution plot: performance_comparison_distribution.png")
