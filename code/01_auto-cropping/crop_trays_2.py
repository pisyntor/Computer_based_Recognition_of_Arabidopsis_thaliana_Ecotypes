import cv2
import os


def crop_images_in_directory(input_dir, output_dir, coords):
    """Crops all images in the specified directory and saves them."""
    for img_file in os.listdir(input_dir):
        input_path = os.path.join(input_dir, img_file)
        if os.path.isfile(input_path):
            # Read the image
            img = cv2.imread(input_path)

            # Crop the image using the given coordinates
            left, right, top, bottom = (
                coords["left"],
                coords["right"],
                coords["top"],
                coords["bottom"],
            )
            cropped_img = img[top:bottom, left:right]

            # Extract folder name and create save directory
            save_folder_name = img_file[-30:-26]
            save_dir = os.path.join(output_dir, save_folder_name)
            os.makedirs(save_dir, exist_ok=True)

            # Define the save file name and path
            save_file_name = img_file.replace("FishEyeCorrected", "cropped_tray")
            save_path = os.path.join(save_dir, save_file_name)

            # Save the cropped image
            cv2.imwrite(save_path, cropped_img)
            print(f"Cropped {img_file}")


if __name__ == "__main__":
    # Define cropping coordinates
    CROP_COORDS = {"left": 927, "right": 3571, "top": 471, "bottom": 2595}

    # Define directories
    INPUT_DIR = ""
    OUTPUT_DIR = ""
    # Run the cropping process
    crop_images_in_directory(INPUT_DIR, OUTPUT_DIR, CROP_COORDS)
