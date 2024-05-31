from pathlib import Path
import datetime
import shutil
import pandas as pd
import matplotlib.pyplot as plt
import seaborn as sns
from tqdm.auto import tqdm

# INPUTS
INPUT_FILE = ""
OUTPUT_FOLDER = ""
FEATURES = [
    "Color distribution modes",
    "Rank #unique colors",
    "Spectral distribution modes",
    "SIFT", "Pixel entropy",
    "HoG", "LoG",
    "VGG16", "ResNet50"
]

STARTING_DATES = {
    'accessions_dataset1': datetime.datetime(year=2022, month=5, day=11, hour=0, minute=0, second=0),
    'accessions_dataset2': datetime.datetime(year=2022, month=8, day=2, hour=0, minute=0, second=0),
}

def plot_timeseries(data, feature, level="replicate"):
    """Plot a timeseries of a feature."""
    fig, ax = plt.subplots(figsize=(20, 10), dpi=300)
    hue_order = sorted(data[level].unique())
    sns.lineplot(
        data=data, 
        x='datetimes_corrected_str', 
        y=feature, 
        hue=level, 
        hue_order=hue_order, 
        ax=ax, 
        errorbar=None
    )
    xticklabels = [x.split(' ')[0] for x in data['datetimes_corrected_str'].unique()]
    ax.set_xticks(range(len(xticklabels)))
    ax.set_xticklabels(xticklabels, rotation=45)
    
    ax.set_xlabel('Date', fontsize=14)
    ax.set_ylabel(feature, fontsize=14)
    ax.set_title(f"{feature} over time", fontsize=18, fontweight='bold')
    ax.grid()
    plt.tight_layout()
    return fig

def save_figure(fig, path, dpi=300):
    """Save a figure to a file."""
    path.parent.mkdir(parents=True, exist_ok=True)
    fig.savefig(path, dpi=dpi)

def process_data(input_file, output_folder, features, starting_dates):
    """Process data to generate and save timeseries plots for each feature."""
    data_path = Path(input_file)
    output_folder = Path(output_folder)

    # Clear the output directory if it exists
    if output_folder.exists():
        shutil.rmtree(output_folder)

    # Read and preprocess the data
    data = pd.read_csv(data_path)
    filenames = [name.split('-')[0].split('_')[1:-1] for name in data['filename'].values]
    datetimes = [datetime.datetime(int(name[0]), int(name[1]), int(name[2]), 0, 0, 0) for name in filenames]
    
    data['datetimes'] = datetimes
    data = data.sort_values(by='datetimes').reset_index(drop=True)
    data['datetimes_str'] = data['datetimes'].apply(lambda x: str(x))

    # Correct datetimes based on starting dates
    for dataset_name, dataset_data in data.groupby('dataset'):
        for class_name, class_data in dataset_data.groupby('class'):
            for replicate_name, replicate_data in class_data.groupby('replicate'):
                starting_date = replicate_data['datetimes'].min()
                correct_start_date = starting_dates.get(dataset_name)
                
                if correct_start_date is None:
                    raise ValueError(f'Unknown dataset: {dataset_name}')
                
                if starting_date == correct_start_date:
                    data.loc[replicate_data.index, 'datetimes_corrected'] = replicate_data['datetimes']
                elif dataset_name == 'accessions_dataset2' and starting_date == correct_start_date + datetime.timedelta(days=7):
                    data.loc[replicate_data.index, 'datetimes_corrected'] = replicate_data['datetimes'] - datetime.timedelta(days=7)
                else:
                    raise ValueError('Wrong starting date')
                    
    data = data.sort_values(by='datetimes_corrected').reset_index(drop=True)
    data['datetimes_corrected_str'] = data['datetimes_corrected'].apply(lambda x: str(x))

    # Generate and save plots
    for dataset in tqdm(data['dataset'].unique(), desc="Processing datasets..."):
        dataset_data = data[data['dataset'] == dataset]
        
        for feature in tqdm(features, desc="Processing class features...", leave=False):
            fig = plot_timeseries(dataset_data, feature, level="class")
            feature_filename = '_'.join(feature.lower().split())
            save_path = output_folder / dataset / f"{feature_filename}.png"
            save_figure(fig, save_path)
            plt.close()
            
        for class_ in tqdm(dataset_data['class'].unique(), desc="Processing classes...", leave=False):
            class_data = dataset_data[dataset_data['class'] == class_]
            for feature in tqdm(features, desc="Processing features...", leave=False):
                fig = plot_timeseries(class_data, feature, level="replicate")
                feature_filename = '_'.join(feature.lower().split())
                save_path = output_folder / dataset / class_ / f"{feature_filename}.png"
                save_figure(fig, save_path)
                plt.close()

if __name__ == "__main__":
    process_data(INPUT_FILE, OUTPUT_FOLDER, FEATURES, STARTING_DATES)
