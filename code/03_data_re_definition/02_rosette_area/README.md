# Rosette Area Calculation

## Overview

This project computes **projected rosette (plant) area** from binary segmentation masks. It processes time-series image masks of Arabidopsis rosettes, converts pixel counts to real-world area using a calibration factor, and exports the results to Excel.

## What the Notebook Does (`01_rosette_area_clc.ipynb`)

### Purpose

The notebook batch-processes directories of plant segmentation masks to calculate the projected leaf area (in mm^2) for each plant at each imaging timepoint. It supports multiple datasets, each with its own calibration factor, and can run in parallel for faster processing.

### Pipeline

1. **Directory traversal** — Scans a root mask directory for accession folders (e.g., `Ba4-1`, `Col-0`). Each accession folder contains replicate subfolders (`rep_01`, `rep_02`, ...), each with a `masks/` directory holding the binary mask images.

   Expected input structure:
   ```
   root_mask_path/
   ├── Ba4-1/
   │   ├── rep_01/
   │   │   └── masks/
   │   │       ├── img_2022_05_11_12_09_....png
   │   │       └── ...
   │   └── rep_02/
   │       └── masks/
   └── Col-0/
       └── ...
   ```

2. **Area computation (`compute_plant_area`)** — For each mask image:
   - Reads the image with OpenCV (`cv2.imread`).
   - Counts all non-zero pixels across channels (`np.any(img > 0, axis=2)`) to get the mask area in pixels.
   - Converts pixel area to real-world area (mm^2) by multiplying by `calibration_factor^2` (the factor represents mm-per-pixel).
   - Parses the filename (expected format: `<prefix>_<YYYY>_<MM>_<DD>_<HH>_<MM>_...`) to extract the date and time of capture.

3. **Parallel or sequential processing (`process_dataset`)** — Processes all accession/replicate combinations either:
   - **In parallel** using `joblib.Parallel` with `n_jobs=-2` (all CPUs minus one), or
   - **Sequentially** in a simple loop.

4. **Output** — Concatenates all per-replicate DataFrames into a single table and saves it as an Excel file (`extracted_features_DS<N>.xlsx`) with columns:

   | Column    | Description                                       |
   |-----------|---------------------------------------------------|
   | `Date`    | Capture date (`DD//MM//YYYY`)                     |
   | `Time`    | Capture time (`HH:MM`)                            |
   | `p_area`  | Projected plant area in mm^2 (rounded to 3 decimals) |
   | `rep_num` | Replicate number (integer extracted from folder name) |
   | `class`   | Accession/genotype name                           |

### Configuration (final notebook cell)

Before running, fill in three values:

| Variable               | Description                                                                 |
|------------------------|-----------------------------------------------------------------------------|
| `root_mask_paths`      | List of root directories containing accession folders (one per dataset).    |
| `calibration_factors`  | List of mm-per-pixel calibration values (one per dataset). Defaults: `0.13715` (DS1), `0.14690` (DS2). |
| `out_path`             | Directory where the output Excel files will be written.                     |
| `parallel_processing`  | `True` to use multi-core processing, `False` for sequential.               |

### Dependencies

- `numpy` — array math and pixel summation
- `pandas` — DataFrame construction and Excel export
- `opencv-python` (`cv2`) — mask image reading
- `joblib` — parallel processing
- `openpyxl` — Excel file writing (required by `pandas.to_excel`)

## How to Run

1. Install [conda](https://conda.io/projects/conda/en/latest/user-guide/install/index.html).
2. Create a new environment:
   ```bash
   conda env create -f env.yml
   ```
3. Activate the environment:
   ```bash
   conda activate plants_p1
   ```
4. Open the notebook:
   ```bash
   jupyter notebook 01_rosette_area_clc.ipynb
   ```
5. Set `root_mask_paths`, `calibration_factors`, and `out_path` in the last cell, then **Run All**.
