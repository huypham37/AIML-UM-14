import pandas as pd
import numpy as np

# List of CSV files and their corresponding learning rates
files = [
    ('LucaRun1.csv', 0.001),
    ('LucaRun2.csv', 0.001),
    ('LucaRun3.csv', 0.001),
    ('LucaRun4.csv', 0.0001),
    ('LucaRun5.csv', 0.0001),
    ('LucaRun6.csv', 0.0001)
]

# Function to perform statistical analysis
def analyze_file(file, learning_rate):
    # Read the CSV file
    df = pd.read_csv(file)
    
    # Add the learning rate column
    df['Learning Rate'] = learning_rate
    
    # Calculate statistics
    statistics = df.describe().loc[['mean', 'std']]
    
    # Save statistics to a CSV file
    output_file = file.replace('.csv', '_statistics.csv')
    statistics.to_csv(output_file)
    
    print(f'Statistics for {file} saved to {output_file}')

# Perform analysis for each file
for file, learning_rate in files:
    analyze_file(file, learning_rate)