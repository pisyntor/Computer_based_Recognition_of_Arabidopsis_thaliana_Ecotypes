import os
import numpy as np
from tqdm import tqdm
import cv2 
import torch


def get_dataset_image_info(dataset_path):
    image_class_info = {}

    for i, class_folder_name in enumerate(os.listdir(dataset_path)):

        # print(f"{i + 1}: {class_folder_name}")
        class_folder_path = os.path.join(dataset_path, class_folder_name)
        if not os.path.isdir(class_folder_path):
            continue

        for j, rep_folder_name in enumerate(os.listdir(class_folder_path)):

            # print(f"\t{j + 1}: {rep_folder_name}")
            rep_folder_path = os.path.join(class_folder_path, rep_folder_name)
            if not os.path.isdir(rep_folder_path):
                continue
            for k, file_name in enumerate(os.listdir(rep_folder_path)):
                image_path = os.path.join(rep_folder_path, file_name)
                image_class_info[image_path] = class_folder_name
                # print(image_class_info[image_path], image_path)

    return image_class_info


def get_images_for_dataloader(extracted_data_path, NUM_CHANNELS):
    if not extracted_data_path.endswith(os.sep):
        extracted_data_path += os.sep
    
    # getting the list of the classes
    class_list = os.listdir(extracted_data_path)
    class_list.sort()
    
    # remove anything that's not a directory
    class_list = [class_name for class_name in class_list if os.path.isdir(extracted_data_path + class_name)]
    
    print('Number of classes: {}'.format(len(class_list)))
    print('Classes: {}'.format(class_list))
    
    print('Loading data ...')
    image_files = []
    targets = []
    
    # dict helps to go from class_name to class_index
    class_dict = dict([(j, i) for i, j in enumerate(class_list)])
    
    # loading all image_paths (IN A SORTED ORDER, this is really important to avoid any weird exceptions)
    for class_name in class_dict.keys():
        repetitions_list = os.listdir(extracted_data_path + class_name)
        repetitions_list.sort()
        for repetition in repetitions_list:
            image_list = os.listdir(extracted_data_path + class_name + os.sep + repetition)
            image_list.sort()
            image_files.extend(
                [extracted_data_path + class_name + os.sep + repetition + os.sep + img for img in image_list]
            )
            targets.extend([class_dict[class_name]] * len(image_list))
    
    # loading everything into memory since we have enough space
    images = np.empty((len(image_files), NUM_CHANNELS, 350, 350), dtype=np.uint8)
    for idx, img_path in enumerate(tqdm(image_files, position=0, leave=True)):
        img = cv2.imread(img_path)
        img = cv2.cvtColor(img, cv2.COLOR_BGR2RGB)
        img = cv2.resize(img, (350, 350)) # saving with 350x350 size, downsample in dataloader transform= property if you want

        img = img.transpose(2, 0, 1)
        images[idx] = img

    print('Data files loaded')
    
    return images, targets


class Predict:
    def __init__(self,
                 model_path,
                 class_list=['Col-0', 'Cvi-0', 'Is-1', 'Kz-9', 'Ler-1', 'TOU-I-17', 'Uk-1', 'Zdr-1'],
                 resize_shape=None,
                 device="cuda"):
        self.model_path = model_path
        self.device = device
        self.class_list = class_list
        self.resize_shape = resize_shape

        self.model = torch.load(self.model_path, map_location=self.device)
        self.model.eval()

    def predict(self, image_path):
        if type(image_path) == str:
            image = cv2.imread(image_path)
            image = cv2.cvtColor(image, cv2.COLOR_BGR2RGB)
        elif type(image_path) == np.ndarray:
            image = image_path
        else:
            raise Exception("Invalid image_path type. It should be either str or np.ndarray")

        image = image.astype(np.float32)
        image = cv2.resize(image, self.resize_shape)
        image = torch.tensor(image).permute(2, 0, 1).unsqueeze(0).float() / 255.0
        image = image.to(self.device)

        with torch.no_grad():
            output = self.model(image)

        return output

    def get_actual_output(self, output, class_list=None, available_classes=None, verbose=False):
        if class_list is None:
            class_list = self.class_list

        output = output.squeeze()
        output = output.cpu().numpy()
        output_list = output.tolist()

        available_classes_opt = []
        for i, val in enumerate(output_list):
            if i in available_classes:
                available_classes_opt.append(val)
            else:
                available_classes_opt.append(0)

        max_val = np.argmax(available_classes_opt)

        if verbose:
            print(available_classes_opt)
            print("output: ", output)
            print("max_val: ", max_val)
            print("class_list[max_val]: ", class_list[max_val])

        return class_list[max_val]


def calculate_accuracy(
        dataset_path,
        model_path,
        class_list,
        available_classes,
        resize_shape=(350, 350),
        device="cuda"
):
    image_class_info = get_dataset_image_info(dataset_path)

    pred_obj = Predict(
        model_path=model_path,
        class_list=class_list,
        device=device,
        resize_shape=resize_shape,
    )

    available_class_names = []
    for i, class_name in enumerate(class_list):
        class_name = class_name.lower()
        if i in available_classes:
            available_class_names.append(class_name)

    print("available_class_names: ", available_class_names)

    # Aggregated values for all available classes
    TP_all, TN_all, FP_all, FN_all = 0, 0, 0, 0

    correct = 0
    total = 0

    print("Calculating accuracy for", dataset_path, "...")
    for i, image_path in tqdm(enumerate(image_class_info), total=len(image_class_info), desc="Inference on images"):

        true_class = image_class_info[image_path]
        true_class = true_class.lower()

        if true_class not in available_class_names:
            continue

        output = pred_obj.predict(image_path)
        actual_output = pred_obj.get_actual_output(output, available_classes=available_classes, verbose=False)
        actual_output = actual_output.lower()

        total += 1

        if actual_output == true_class:
            correct += 1

        for class_name in available_class_names:
            if actual_output == class_name:
                if true_class == class_name:
                    TP_all += 1
                else:
                    FP_all += 1
            else:
                if true_class == class_name:
                    FN_all += 1
                else:
                    TN_all += 1

    # Now, compute the metrics using the aggregated counts
    # To prevent division by zero
    eps = 1e-7

    precision = TP_all / (TP_all + FP_all + eps)
    recall = TP_all / (TP_all + FN_all + eps)
    specificity = TN_all / (TN_all + FP_all + eps)
    fpr = 1 - specificity
    fnr = FN_all / (TP_all + FN_all + eps)
    fdr = FP_all / (TP_all + FP_all + eps)
    for_ = FN_all / (TN_all + FN_all + eps)  # False Omission Rate
    misclassification_rate = (FP_all + FN_all) / (TP_all + TN_all + FP_all + FN_all + eps)
    mcc = (TP_all * TN_all - FP_all * FN_all) / ((TP_all + FP_all) * (TP_all + FN_all) * (TN_all + FP_all) * (TN_all + FN_all) + eps) ** 0.5
    f1 = 2 * precision * recall / (precision + recall + eps)

    results = {
        'Precision': precision,
        'Recall': recall,
        'F1 Score': f1,
        'Specificity': specificity,
        'False Positive Rate': fpr,
        'False Negative Rate': fnr,
        'False Discovery Rate': fdr,
        'False Omission Rate': for_,
        'Misclassification Rate': misclassification_rate,
        'MCC': mcc,
        'Total correct': TP_all + TN_all,
        'Total': total,
    }

    overall_accuracy = correct / total
    print("\n----------------------------------------\n")
    print("Accuracy results:\n")
    print(f"Correct: {correct}")
    print(f"Total: {total}")
    print(f"Overall Accuracy: {round(overall_accuracy * 100, 4)}%")

    # print the results
    print("\nMetrics:")
    for metric_name in results:
        print(f"\t{metric_name}: {results[metric_name]}")

    return results