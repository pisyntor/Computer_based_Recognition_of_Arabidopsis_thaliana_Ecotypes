# PlantEnhancer GUI


A graphical user interface for enhancing and segmenting plant images from RGB datasets of *Arabidopsis thaliana* ecotypes. PlantEnhancer separates plant regions from their background using colour deconvolution and background suppression, producing enhanced images and binary plant masks.

## Features

- **Plant enhancement** -- brightens plant regions while darkening the background via colour deconvolution.
- **Binary mask generation** -- extracts plant masks using Otsu thresholding on the transformed colour space.
- **Interactive parameter tuning** -- adjust desired/undesired RGB colours, smoothing, background suppression level, multiply factor, and shift in real time.
- **Pixel inspector** -- hover over images to read RGB values for both the original and enhanced views.
- **Batch processing** -- process all replicates within a selected ecotype, or process every ecotype in the dataset at once.
- **Save to disk** -- export enhanced images or masks to a user-specified output folder.

## Installation

1. Run `setup.exe`.
2. Accept the Research Use Only licence agreement.
3. Choose an installation folder (default: `C:\Program Files\PlantEnhancer`).
4. Click **Next** to install, then **Finish**.

The installation creates `PlantEnhancer.exe` and its dependencies (`lua5.1.dll`, etc.) in the chosen folder.

## Dataset Structure

PlantEnhancer expects datasets organised as:

```
RootInputFolder/
  Ecotype_1/
    rep_01/
      image1.png
      image2.png
      ...
    rep_02/
      ...
  Ecotype_2/
    ...
```

Each ecotype folder contains one or more replicate folders with RGB plant images.

## Usage

1. **Set input folder** -- browse to or paste the root input folder path.
2. **Set output folder** -- browse to or paste the root output folder path.
3. **Select data** -- choose an ecotype, replicate, and image from the list boxes. The original image appears in the left panel; the enhanced result appears in the right panel.
4. **Tune parameters:**
   - **Desired Colour (R,G,B)** -- a representative colour for the plant leaves.
   - **Undesired Colour (R,G,B)** -- a dominant background colour to suppress.
   - **Smoothing Level** -- Gaussian kernel sigma; removes small pixel "islands" in the mask.
   - **Suppress Background Level by** -- factor by which background pixel values are divided.
   - **Multiply** -- factor applied to the Green channel of the desired colour.
   - **Shift** -- darkest G-channel levels ignored when computing the mask threshold.
5. **Show Mask** -- toggle the checkbox to switch between the enhanced image and the binary mask.
6. **Save** -- click **Save Enhanced Image** to write the current result to disk.
7. **Batch process** -- click **Process Selected Ecotype** or **Process All Ecotypes** to run on multiple images at once.

### Tips

- Use the pixel inspector (status bar) to read RGB values from the original image when choosing desired/undesired colours.
- The colour deconvolution is nonlinear -- sometimes values slightly outside the actual plant or background colour range yield better separation.
- For stressed or purple-tinted plants, increase the red component of the desired colour.
- Increase the smoothing level to remove small artefacts in the binary mask.

## How It Works

1. **Colour deconvolution** -- transforms the RGB colour space so the plant (desired) colour becomes brighter and greener while the background (undesired) colour becomes darker.
2. **Background suppression** -- applies Otsu's method to determine a threshold between plant and background, further darkening the background and brightening the plant. This threshold also produces the binary plant mask.

## Applications

- **Visualization** -- enhanced images highlight plant regions for visual inspection.
- **Geometric trait extraction** -- binary masks enable measurement of plant area, convex hull, roundness, eccentricity, isotropy, and other morphological traits at the replicate or ecotype level.
- **Stress and disease identification** -- colour-segmented images help identify stressed, diseased, or productive plants.
- **ML/DL ground truth** -- automatically generated masks and enhanced images serve as training, validation, and test data for machine learning models (ecotype recognition, growth pattern analysis, canopy density estimation, etc.).

## Licence

Research Use Only. Commercial use, redistribution, and sublicensing are prohibited. Publications using this software must credit the original authors. The software is provided "as is" with no warranty.
