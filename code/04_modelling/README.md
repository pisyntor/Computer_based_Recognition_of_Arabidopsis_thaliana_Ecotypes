# Ecotype Classification 

Deep learning pipeline for classifying Arabidopsis ecotypes from top-down imaging data using PyTorch. Supports both **single image** and **temporal sequence** classification workflows, with training utilities, hyperparameter tuning, evaluation, and GradCAM visualisations.

---

## Structure

```
04_modelling/
├── 01_data_split_configs/                    # Train/val/test split definitions
│   ├── NEW8split.json                        #   8-ecotypes  dataset
│   └── NEW25split.json                       #   25-ecotypes dataset
│
├── 02_single_image_classification/
│   └── single_image_plant_class_TR1.ipynb    # Single image training notebook
│
├── 03_sequence_of_images_classification/
│   └── seq_images_plant_class_TR1.ipynb      # Sequence training notebook
│
├── 04_inference/
│   ├── evaluation_single_img.py              # Single image inference & metrics
│   ├── evaluation_sequence_imgs.py           # Sequence inference & metrics
│   ├── evaluation_single_img.ipynb           # Notebook version
│   └── evaluation_seqeunce_imgs.ipynb        # Notebook version
│
├── 05_heat_maps/
│   ├── gradcams_single_image/                # GradCAM outputs (single image)
│   ├── gradcams_seqeunce_of_images/          # GradCAM outputs (sequences)
│   └── feature_maps/                         # Intermediate layer activations
│
├── 06_pytorch_utils/                         # Reusable library
│   ├── __init__.py
│   ├── data_utils.py                         # Datasets & augmentation
│   ├── training_utils.py                     # Training loops & scheduling
│   ├── validating_utils.py                   # Prediction & metric calculation
│   ├── callbacks.py                          # Checkpointing, early stopping, LR reduction
│   ├── hyper_tuner.py                        # Hyperopt-based tuning
│   └── requirements.txt                      # Minimal util-level deps
├── requirements.txt                          # Full project dependencies
│
└── README.md
```

---

## Model Architectures

### Best-performing Single Image Classifiers

| Model | Params |
|---|---|
| Vision Transformer B/16/32 (ViT) | ~86 M |
| ResNet50 | ~25.6 M |
| Inception / GoogLeNet | ~23.8 M |
| EfficientNetB1 | ~7.8 M |
| MobileNetV3-Large | ~5.5 M |
| MobileNetV3-Small | ~2.5 M |


### Best-performing Sequence of Images Classifiers 

| Model | Description |
|---|---|
| Eff1_GRU | EfficientNetB1 backbone + GRU head |
| Eff1_LSTM | EfficientNetB1 backbone + LSTM head |
| Eff1_BiGRU | EfficientNetB1 + Bidirectional GRU |
| Eff1_BiLSTM | EfficientNetB1 + Bidirectional LSTM |
| ViViT_Small | Video Vision Transformer |

---

## Data Format

The pipeline expects a directory-based dataset laid out as:

```
dataset/
├── Col-0/
│   ├── rep_01/
│   │   ├── image_0.jpg
│   │   ├── image_1.jpg
│   │   └── ...
│   ├── rep_02/
│   └── ...
├── Cvi-0/
└── ...
```

Split configurations in `01_data_split_configs/` assign each replicate (`rep_XX`) to `train`, `valid`, or `test`. Two configurations are provided:
- **NEW8split.json** — 8 accessions (smaller experiments)
- **NEW25split.json** — 25 accessions (full benchmark)

For sequence models, consecutive images from the same replicate are stacked into tensors of shape `(C, T, H, W)`.

---

## Key Components (`06_pytorch_utils/`)

### `data_utils.py`
- **ClassificationPlantDataset** — single image dataset loader (resizes to 350x350, normalises to [0, 1])
- **ClassificationPlantSequenceDataset** — temporal sequence loader, outputs `(C, T, H, W)` tensors
- **Augmentation pipeline** (albumentations) — rotation, flips, Gaussian noise, blur, optical distortion, CLAHE, brightness/contrast adjustments

### `training_utils.py`
- **fit() / fit_seq()** — training loops with early stopping, LR scheduling, multi-GPU support (DataParallel), AMP, FLOP counting, and GPU memory tracking
- **CustomModelBase** — abstract base providing `training_step`, `validation_step`, and `epoch_end` hooks
- **PolyScheduler** — polynomial LR decay with linear warmup
- **MetricLogger** — CSV-based experiment tracking (time, FPS, FLOPs, memory, params)

### `callbacks.py`
- **model_checkpoint** — saves best model by monitored metric
- **early_stopping** — halts training when a metric plateaus
- **reduce_lr_on_plateau** — adaptive learning rate reduction
- Supports resuming from checkpoints with saved optimiser state

### `validating_utils.py`
- **Predict** — single image inference with optional class filtering
- **calculate_accuracy()** — computes per-class and aggregate metrics: accuracy, precision, recall, F1, specificity, FPR, FNR, FDR, FOR, MCC
- **get_dataset_image_info() / get_images_for_dataloader()** — batch image loading into numpy arrays

### `hyper_tuner.py`
- **HyperTunerUtils** — wraps `hyperopt` (TPE, random, grid search)
- Grid search with randomisation, duplicate-trial skipping, CSV trial logging, and JSON export of best parameters
- Supports resuming interrupted tuning runs

---

## Evaluation Metrics

Classification performance is assessed with:
- Accuracy, Precision, Recall, F1-score
- Specificity, FPR, FNR, FDR, FOR
- Matthews Correlation Coefficient (MCC)
- Misclassification rate

Efficiency metrics captured during training and inference:
- FLOPs (MACs), FPS, training time, GPU memory usage

Confusion matrices are generated with seaborn heatmaps. GradCAM and feature map visualisations are stored in `05_heat_maps/`.

---

## Setup

1. Install [conda](https://conda.io/projects/conda/en/latest/user-guide/install/index.html)

2. Create and activate a new environment:
```bash
conda create -n env3 python=3.10
conda activate env3
```

3. Install dependencies:
```bash
pip install -r requirements.txt
```

Key dependencies: PyTorch, TorchVision, TIMM, albumentations, hyperopt, captum, grad-cam, scikit-learn, OpenCV, matplotlib, seaborn.

---

## Usage

### Training

Open the relevant Jupyter notebook and follow the cells:

- **Single image** — `02_single_image_classification/single_image_plant_class_TR1.ipynb`
- **Sequence** — `03_sequence_of_images_classification/seq_images_plant_class_TR1.ipynb`

Both notebooks use the utilities in `06_pytorch_utils/` and write checkpoints, training stats, and logs to disk.

### Inference & Evaluation

```bash
# Single image evaluation (supports DDP)
python 04_inference/evaluation_single_img.py

# Sequence evaluation
python 04_inference/evaluation_sequence_imgs.py
```

Notebook versions are also available in `04_inference/` for interactive exploration.

### Hyperparameter Tuning

Use `HyperTunerUtils` from `06_pytorch_utils/hyper_tuner.py` to define a search space and run optimisation with TPE, random search, or grid search. Trial results are logged to CSV and the best configuration is saved as JSON.
