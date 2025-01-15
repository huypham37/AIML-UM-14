# Import required libraries
import pandas as pd  # For data manipulation and analysis
import numpy as np   # For numerical operations
from scipy.stats import shapiro, mannwhitneyu, ttest_ind  # Statistical testing
import matplotlib.pyplot as plt  # For creating plots
import seaborn as sns  # For enhanced visualizations
from scipy.stats import pearsonr  # For correlation analysis

def create_time_series_plots(lr_groups, metrics):
    """
    Creates time series plots for each metric, comparing different learning rates.
    
    Parameters:
    -----------
    lr_groups : dict
        Dictionary with learning rates as keys and lists of DataFrames as values
    metrics : list
        List of metric names to plot
    
    Returns:
    --------
    matplotlib.figure.Figure
        Figure containing all time series plots
    """
    # Create subplots: one row per metric
    fig, axes = plt.subplots(len(metrics), 1, figsize=(15, 5*len(metrics)))
    if len(metrics) == 1:
        axes = [axes]  # Handle case of single metric
    
    # Define colors for different learning rates
    colors = {'0.001': 'blue', '0.0001': 'red'}
    
    # Create plots for each metric
    for i, metric in enumerate(metrics):
        ax = axes[i]
        
        # Plot data for each learning rate
        for lr, dfs in lr_groups.items():
            # Plot individual runs with transparency
            for j, df in enumerate(dfs):
                ax.plot(df.index, df[metric], 
                       alpha=0.3,  # Make individual runs semi-transparent
                       color=colors[str(lr)], 
                       label=f'LR={lr}' if j == 0 else "")  # Label only first run
                
            # Calculate and plot mean line
            mean_data = pd.concat(dfs)[metric].groupby(level=0).mean()
            ax.plot(mean_data.index, mean_data.values, 
                   color=colors[str(lr)], 
                   linewidth=2,  # Make mean line thicker
                   label=f'LR={lr} (mean)')
        
        # Customize plot appearance
        ax.set_title(f'{metric} Over Time')
        ax.set_xlabel('Time Step')
        ax.set_ylabel(metric)
        ax.legend()
        ax.grid(True, alpha=0.3)
    
    plt.tight_layout()
    return fig

def create_correlation_heatmap(data_001, data_0001, metrics):
    """
    Creates correlation heatmaps for both learning rates to compare metric relationships.
    
    Parameters:
    -----------
    data_001 : pandas.DataFrame
        Data for learning rate 0.001
    data_0001 : pandas.DataFrame
        Data for learning rate 0.0001
    metrics : list
        List of metrics to include in correlation analysis
    
    Returns:
    --------
    matplotlib.figure.Figure
        Figure containing both correlation heatmaps
    """
    # Create side-by-side subplots
    fig, (ax1, ax2) = plt.subplots(1, 2, figsize=(15, 6))
    
    # Calculate correlation matrices for both learning rates
    corr_001 = data_001[metrics].corr()
    corr_0001 = data_0001[metrics].corr()
    
    # Create heatmaps with consistent color scaling
    sns.heatmap(corr_001, annot=True, cmap='coolwarm', vmin=-1, vmax=1, ax=ax1)
    sns.heatmap(corr_0001, annot=True, cmap='coolwarm', vmin=-1, vmax=1, ax=ax2)
    
    # Set titles
    ax1.set_title('Correlations (LR=0.001)')
    ax2.set_title('Correlations (LR=0.0001)')
    
    plt.tight_layout()
    return fig

def create_distribution_plots(data_001, data_0001, metrics):
    """
    Creates distribution plots (KDE) with statistical annotations for each metric.
    
    Parameters:
    -----------
    data_001 : pandas.DataFrame
        Data for learning rate 0.001
    data_0001 : pandas.DataFrame
        Data for learning rate 0.0001
    metrics : list
        List of metrics to plot
    
    Returns:
    --------
    matplotlib.figure.Figure
        Figure containing all distribution plots
    """
    # Create subplots: one row per metric
    fig, axes = plt.subplots(len(metrics), 1, figsize=(15, 5*len(metrics)))
    if len(metrics) == 1:
        axes = [axes]
    
    for i, metric in enumerate(metrics):
        # Create kernel density estimation plots
        sns.kdeplot(data=data_001[metric], ax=axes[i], label='LR=0.001')
        sns.kdeplot(data=data_0001[metric], ax=axes[i], label='LR=0.0001')
        
        # Perform statistical tests
        _, p_val = mannwhitneyu(data_001[metric], data_0001[metric], alternative='two-sided')
        
        # Calculate Cohen's d effect size
        d = (np.mean(data_001[metric]) - np.mean(data_0001[metric])) / np.sqrt(
            (np.std(data_001[metric])**2 + np.std(data_0001[metric])**2) / 2
        )
        
        # Customize plot appearance
        axes[i].set_title(f'{metric} Distribution\np={p_val:.2e}, Cohen\'s d={d:.2f}')
        axes[i].legend()
        axes[i].grid(True, alpha=0.3)
    
    plt.tight_layout()
    return fig

def create_summary_stats_plot(results, metrics):
    """
    Creates a bar plot comparing summary statistics between learning rates.
    
    Parameters:
    -----------
    results : dict
        Dictionary containing statistical results for each metric
    metrics : list
        List of metrics to plot
    
    Returns:
    --------
    matplotlib.figure.Figure
        Figure containing the summary statistics plot
    """
    fig, ax = plt.subplots(figsize=(12, 6))
    
    # Set up bar positions
    x = np.arange(len(metrics))
    width = 0.35  # Width of bars
    
    # Extract summary statistics
    means_001 = [results[m]['lr_001_stats']['mean'] for m in metrics]
    means_0001 = [results[m]['lr_0001_stats']['mean'] for m in metrics]
    stds_001 = [results[m]['lr_001_stats']['std'] for m in metrics]
    stds_0001 = [results[m]['lr_0001_stats']['std'] for m in metrics]
    
    # Create grouped bar plot with error bars
    ax.bar(x - width/2, means_001, width, yerr=stds_001, label='LR=0.001', 
           capsize=5, alpha=0.7)
    ax.bar(x + width/2, means_0001, width, yerr=stds_0001, label='LR=0.0001', 
           capsize=5, alpha=0.7)
    
    # Customize plot appearance
    ax.set_ylabel('Value')
    ax.set_title('Summary Statistics Comparison')
    ax.set_xticks(x)
    ax.set_xticklabels(metrics, rotation=45, ha='right')
    ax.legend()
    
    plt.tight_layout()
    return fig

def load_and_analyze_data(files):
    """
    Main function to load data, perform statistical analysis, and create visualizations.
    
    Parameters:
    -----------
    files : list of tuples
        List of (filename, learning_rate) pairs
    """
    # Initialize dictionary to store DataFrames grouped by learning rate
    lr_groups = {0.001: [], 0.0001: []}
    
    # Load data files and group by learning rate
    for file, lr in files:
        df = pd.read_csv(file)
        lr_groups[lr].append(df)
    
    # Combine DataFrames for each learning rate
    data_001 = pd.concat(lr_groups[0.001], ignore_index=True)
    data_0001 = pd.concat(lr_groups[0.0001], ignore_index=True)
    
    # Define metrics to analyze
    metrics = ['Physics Time (ms)', 'Main Thread Time (ms)', 
              'System Memory (MB)', 'Wall Time (ms)']
    
    # Create and save all visualization plots
    time_series_fig = create_time_series_plots(lr_groups, metrics)
    time_series_fig.savefig('time_series_plots.png', dpi=300, bbox_inches='tight')
    
    corr_fig = create_correlation_heatmap(data_001, data_0001, metrics)
    corr_fig.savefig('correlation_heatmap.png', dpi=300, bbox_inches='tight')
    
    dist_fig = create_distribution_plots(data_001, data_0001, metrics)
    dist_fig.savefig('distribution_plots.png', dpi=300, bbox_inches='tight')
    
    # Create violin plots for detailed distribution comparison
    fig, axes = plt.subplots(2, 2, figsize=(15, 12))
    axes = axes.ravel()
    
    # Dictionary to store statistical results
    results = {}
    
    # Analyze each metric
    for i, metric in enumerate(metrics):
        # Perform normality tests (Shapiro-Wilk)
        _, p_val_001 = shapiro(data_001[metric])
        _, p_val_0001 = shapiro(data_0001[metric])
        
        # Choose appropriate statistical test based on normality
        if p_val_001 > 0.05 and p_val_0001 > 0.05:
            # Use t-test if both distributions are normal
            stat_name = "T-test"
            statistic, p_value = ttest_ind(data_001[metric], data_0001[metric])
        else:
            # Use non-parametric test if distributions are not normal
            stat_name = "Mann-Whitney U"
            statistic, p_value = mannwhitneyu(
                data_001[metric], 
                data_0001[metric],
                alternative='two-sided'
            )
        
        # Calculate effect size (Cohen's d)
        d = (np.mean(data_001[metric]) - np.mean(data_0001[metric])) / np.sqrt(
            (np.std(data_001[metric])**2 + np.std(data_0001[metric])**2) / 2
        )
        
        # Store results
        results[metric] = {
            'normality_test': 'Shapiro-Wilk',
            'normality_p_001': p_val_001,
            'normality_p_0001': p_val_0001,
            'statistical_test': stat_name,
            'test_p_value': p_value,
            'effect_size': d,
            'lr_001_stats': data_001[metric].describe(),
            'lr_0001_stats': data_0001[metric].describe()
        }
        
        # Create violin plot
        sns.violinplot(data=[data_001[metric], data_0001[metric]], 
                      ax=axes[i], inner='box')
        axes[i].set_title(f'{metric}\n{stat_name} p={p_value:.2e}\nCohen\'s d={d:.2f}')
        axes[i].set_xticks([0, 1])
        axes[i].set_xticklabels(['LR=0.001', 'LR=0.0001'])
        
        # Special handling for Wall Time to convert to seconds
        if metric == 'Wall Time (ms)':
            axes[i].set_ylabel('Wall Time (seconds)')
            axes[i].set_yticklabels([f'{x/1000:.0f}' for x in axes[i].get_yticks()])
    
    plt.tight_layout()
    plt.savefig('violin_plots.png', dpi=300, bbox_inches='tight')
    
    # Create and save summary statistics plot
    summary_fig = create_summary_stats_plot(results, metrics)
    summary_fig.savefig('summary_stats.png', dpi=300, bbox_inches='tight')
    
    # Close all figures to free memory
    plt.close('all')
    
    # Print detailed statistical results
    print("\n=== Statistical Analysis Results ===")
    for metric, stats in results.items():
        print(f"\n{'-'*50}")
        print(f"\n{metric}:")
        
        # Print results for LR=0.001
        print(f"\nLR=0.001:")
        print(f"Mean: {stats['lr_001_stats']['mean']:.3f}")
        print(f"Std:  {stats['lr_001_stats']['std']:.3f}")
        print(f"Min:  {stats['lr_001_stats']['min']:.3f}")
        print(f"Max:  {stats['lr_001_stats']['max']:.3f}")
        print(f"Median: {stats['lr_001_stats']['50%']:.3f}")
        
        # Print results for LR=0.0001
        print(f"\nLR=0.0001:")
        print(f"Mean: {stats['lr_0001_stats']['mean']:.3f}")
        print(f"Std:  {stats['lr_0001_stats']['std']:.3f}")
        print(f"Min:  {stats['lr_0001_stats']['min']:.3f}")
        print(f"Max:  {stats['lr_0001_stats']['max']:.3f}")
        print(f"Median: {stats['lr_0001_stats']['50%']:.3f}")
        
        # Print statistical test results
        print(f"\nNormality Test ({stats['normality_test']}):")
        print(f"LR=0.001 p-value: {stats['normality_p_001']:.2e}")
        print(f"LR=0.0001 p-value: {stats['normality_p_0001']:.2e}")
        print(f"\n{stats['statistical_test']} p-value: {stats['test_p_value']:.2e}")
        print(f"Effect size (Cohen's d): {stats['effect_size']:.3f}")

# Data files to analyze
files = [
    ('LucaRun1.csv', 0.001),  # Training runs with learning rate 0.001
    ('LucaRun2.csv', 0.001),
    ('LucaRun3.csv', 0.001),
    ('LucaRun4.csv', 0.0001), # Training runs with learning rate 0.0001
    ('LucaRun5.csv', 0.0001),
    ('LucaRun6.csv', 0.0001)
]

# Execute the analysis
load_and_analyze_data(files)