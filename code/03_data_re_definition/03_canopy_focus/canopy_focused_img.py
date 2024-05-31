import cv2
import argparse
import os
from PIL import Image


def crop_images(image_path, mask_path, out_image_path):
    # Read the input image and mask
    image = cv2.imread(image_path)
    mask = cv2.imread(mask_path, cv2.IMREAD_GRAYSCALE)

    # Resize mask to match the size of the image
    mask = cv2.resize(mask, (image.shape[1], image.shape[0]))

    # Perform connected components analysis on the mask
    num_labels, labels, stats, centroids = cv2.connectedComponentsWithStats(
        mask, connectivity=4, ltype=cv2.CV_32S
    )
    print(f"Number of labels: {num_labels}")

    # If no components found (other than background), crop center part of the image
    if num_labels <= 1:
        cropped_image = image[
            int(0.4 * image.shape[0]) : int(0.6 * image.shape[0]),
            int(0.4 * image.shape[1]) : int(0.6 * image.shape[1]),
        ]
        cv2.imwrite(out_image_path, cropped_image)
        return

    # Initialize bounding box coordinates
    x_min = stats[1, cv2.CC_STAT_LEFT]
    y_min = stats[1, cv2.CC_STAT_TOP]
    x_max = stats[1, cv2.CC_STAT_LEFT] + stats[1, cv2.CC_STAT_WIDTH]
    y_max = stats[1, cv2.CC_STAT_TOP] + stats[1, cv2.CC_STAT_HEIGHT]

    # Iterate through connected components to find the bounding box that includes all components
    for i in range(2, num_labels):
        x, y, w, h = (
            stats[i, cv2.CC_STAT_LEFT],
            stats[i, cv2.CC_STAT_TOP],
            stats[i, cv2.CC_STAT_WIDTH],
            stats[i, cv2.CC_STAT_HEIGHT],
        )
        # Skip components touching the image border
        if (
            x <= 1
            or y <= 1
            or x + w >= image.shape[1] - 2
            or y + h >= image.shape[0] - 2
        ):
            continue
        x_min, y_min = min(x_min, x), min(y_min, y)
        x_max, y_max = max(x_max, x + w), max(y_max, y + h)

    # Calculate the center and radius of the bounding box, then expand it slightly
    center_x, center_y = (x_min + x_max) / 2, (y_min + y_max) / 2
    radius = max(x_max - center_x, y_max - center_y, 0.1 * image.shape[0]) * 1.2
    x_min, y_min = int(center_x - radius), int(center_y - radius)
    x_max, y_max = int(center_x + radius), int(center_y + radius)

    print(f"x_min = {x_min}, y_min = {y_min}, x_max = {x_max}, y_max = {y_max}")

    # Crop the image using PIL and save the result
    with Image.open(image_path) as im:
        im_crop = im.crop((x_min, y_min, x_max, y_max))
        im_crop.save(out_image_path)


if __name__ == "__main__":
    parser = argparse.ArgumentParser(
        description="Program to preprocess plant pictures before CNN training"
    )

    # Add arguments for input, mask, and output folders
    parser.add_argument(
        "-input_folder", "--input_folder", type=str, required=True, help="Input folder"
    )
    parser.add_argument(
        "-mask_folder", "--mask_folder", type=str, required=True, help="Mask folder"
    )
    parser.add_argument(
        "-output_folder",
        "--output_folder",
        type=str,
        required=True,
        help="Output folder",
    )
    args = parser.parse_args()

    # Create the output folder if it doesn't exist
    if not os.path.exists(args.output_folder):
        os.makedirs(args.output_folder)

    # Iterate through the class directories in the input folder
    for class_dir in os.listdir(args.input_folder):
        input_class_dir = os.path.join(args.input_folder, class_dir)
        output_class_dir = os.path.join(args.output_folder, class_dir)
        if not os.path.exists(output_class_dir):
            os.makedirs(output_class_dir)

        # Iterate through the subdirectories in each class directory
        for subdir in os.listdir(input_class_dir):
            input_subdir = os.path.join(input_class_dir, subdir)
            output_subdir = os.path.join(output_class_dir, subdir)
            if not os.path.exists(output_subdir):
                os.makedirs(output_subdir)

            # Process each image in the subdirectory
            for image_filename in os.listdir(input_subdir):
                image_path_full = os.path.join(input_subdir, image_filename)
                out_image_path_full = os.path.join(output_subdir, image_filename)
                mask_path = os.path.join(
                    args.mask_folder,
                    class_dir,
                    subdir,
                    "masks",
                    f"{os.path.splitext(image_filename)[0]}_mask.png",
                )
                print(f"Processing image: {image_path_full}")
                crop_images(image_path_full, mask_path, out_image_path_full)
