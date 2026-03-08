# 2D Morpho Traits

Extraction of 2D morphological traits from plant leaf masks using image processing and machine learning.

## Notebooks

### 01 - Leaf Extraction and Count (`01_leaf_extractor_and_count.ipynb`)
Segments individual leaves from a whole-plant mask and counts them.
- Filters mask noise by keeping the largest connected component
- Extracts the plant contour and computes its center (midpoint of bounding box, not centroid)
- Detects leaf tips (local maxima of center-to-contour distance) and leaf separators (local minima), using a smoothed radial distance signal with moving-average filtering
- Traces gradient-magnitude paths from each separator back to the plant center to delineate leaf boundaries (uses Gaussian gradient magnitude with sigma=3)
- Returns a list of individual leaf masks and the total leaf count

**Inputs:** A binary whole-plant mask (`plant_mask`) and the corresponding RGB image (`rgb_image`, used for gradient computation during leaf boundary tracing).

**Outputs:** A list of individual binary leaf masks, one per detected leaf.

### 02 - Leaf Petiole Length (`02_leaf_petiole_length.ipynb`)
Measures the petiole (stalk) length of a single leaf from its lobe and petiole masks.
- Locates the petiole base as the farthest point from the leaf centroid
- Extracts the leaf contour and computes a midvein as midpoints between left and right contour halves (sampled at 20 evenly-spaced ratio points, then smoothed with a moving average)
- Draws cross-sections orthogonal to the midvein and measures leaf width along its length
- Identifies the petiole-lobe boundary using a maximum-deviation geometric method (farthest point from the line connecting the base to the widest cross-section)
- Petiole length is the midvein length from the base up to this boundary

**Inputs:** Separate lobe and petiole binary mask images for a single leaf (see [Required Directory Structure](#required-directory-structure)).

**Outputs:** The petiole-lobe boundary point index along the midvein, from which petiole length can be read off the cumulative midvein length.

### 03 - Leaf Shape and Margin Classification (`03_leaf_shape_and_margin_classification.ipynb`)
Classifies leaf shape and leaf margin type using Fourier analysis and Random Forest classifiers.
- Resamples the lobe contour to 1001 points per half-contour and computes the radial distance from the centroid to the full contour
- Applies FFT to the radial distance signal
- Splits the spectrum into low-frequency (overall shape) and high-frequency (margin detail) components via a configurable `cutoff_sz` parameter
- Dimensionality reduction with PCA (`n_components=20`) before classification
- Trains separate Random Forest classifiers (`n_estimators=10`) for shape and margin
- **Requires user-provided labels** for training data (see [Configuration](#configuration))

**Inputs:** Lobe and petiole mask images for multiple leaves across multiple plants/time-points.

**Outputs:** Trained shape and margin classifiers; predicted labels for the test set.

## Configuration

### Required Directory Structure
Notebooks 02 and 03 expect leaf data organized as follows:

```
<root_path>/
  leaf seq/
    hidden leaf mask seq/
      <filename1>.png
      <filename2>.png
      ...
  stem seq/
    hidden stem mask seq/
      <filename1>.png
      <filename2>.png
      ...
```

Each file in `leaf seq/hidden leaf mask seq/` is a binary lobe mask, and the corresponding file (same filename) in `stem seq/hidden stem mask seq/` is the petiole (stem) mask. Masks are read with `cv2.imread` and binarized by taking the channel-wise max.

### Variables to Set Before Running

| Notebook | Variable | Description |
|----------|----------|-------------|
| 02 | `root_path` | Path to the leaf data directory (parent of `leaf seq/` and `stem seq/`) |
| 02 | `filename` | Name of the specific leaf mask file to process |
| 03 | `root_path` | Same as above; set once per plant/experiment block |
| 03 | `filenames` | List of mask filenames (time-points) to process for each plant |

### User-Provided Labels (Notebook 03)
The classification cells in notebook 03 are marked as requiring further development. You must supply:

- `list_of_low_freq_labels` — shape class labels (one per leaf), assigned by visual inspection
- `list_of_high_freq_labels` — margin class labels (one per leaf), assigned by visual inspection
- `list_of_low_freq_fft_grad_mag` / `list_of_high_freq_fft_grad_mag` — FFT feature vectors produced by the earlier analysis cells (via `separate_spectrum()`)

Without these, the Random Forest training cells will not run.

## How to Run

1. Install [conda](https://conda.io/projects/conda/en/latest/user-guide/install/index.html)
2. Create the environment:
```
conda env create -f env.yml
```
3. Activate the environment:
```
conda activate plants_p1
```
4. Launch Jupyter and open the notebooks in order.
5. Set `root_path`, `filename`/`filenames`, and label variables as described in [Configuration](#configuration).

> **Note:** The `env.yml` was exported from a Windows environment. On macOS/Linux you may need to recreate the environment from the key dependencies listed below rather than using the pinned file directly.

## Key Dependencies

- Python 3.9
- NumPy 1.24, SciPy 1.9, OpenCV 4.7, scikit-image 0.20, scikit-learn 1.4, matplotlib 3.7
- Jupyter Notebook 6.5
