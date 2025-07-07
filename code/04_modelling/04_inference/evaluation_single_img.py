import torch
import torch.nn as nn
import torch.distributed as dist
import torch.multiprocessing as mp
from torch.nn.parallel import DistributedDataParallel as DDP
from torch.utils.data import DataLoader, DistributedSampler
from torch.cuda.amp import GradScaler, autocast
import torchvision.models as models
import torchvision.transforms as transforms
import torchvision.datasets as datasets
import os
import time
import sys
import pynvml
import torchvision.transforms as T
import numpy as np
import json
import argparse
from tqdm import tqdm
from fvcore.nn import FlopCountAnalysis
# PyTorch utilities
from pytorch_utils.training_utils import *
import pytorch_utils.training_utils as pt_train

import cv2 as cv
# PyTorch utilities
from pytorch_utils.training_utils import *
import pytorch_utils.training_utils as pt_train


from ptflops import get_model_complexity_info

# Add Vision Transformer imports
from torchvision.models import vit_b_16, ViT_B_16_Weights


from ptflops import get_model_complexity_info

# Add Vision Transformer imports
from torchvision.models import vit_b_16, ViT_B_16_Weights

# Configuration before each run
NUM_CHANNELS=3

VERBOSE = False
DEBUG = True
NUM_EPOCHS = 2
train_valid_test_split_json_name = 'NEW25split.json'
MODEL_NAME_FROM_FACTORY = "efficientnet_b1" 
MODEL_WEIGHTS_PATH = "ADD PATH HERE"


# ================== Model Definitions ==================
import csv
from datetime import datetime

class MetricLogger:
    def __init__(self):
        self.metrics_by_model = {}
        self.headers = [
            'model', 
            'training_time',
            'epoch_time',
            'training_fps',
            'training_flops',
            'gpu_resources',
            'testing_time', 
            'testing_fps', 
            'testing_flops', 
            'no_of_parameters',
        ]

    def add_metrics(self, metrics_dict):
        model_name = metrics_dict.get("model", None)
        if model_name is None:
            print("Warning: No 'model' key found in metrics_dict.")
            return

        if model_name not in self.metrics_by_model:
            self.metrics_by_model[model_name] = {'model': model_name}

        self.metrics_by_model[model_name].update(metrics_dict)

    def save_to_csv(self, filename="metrics.csv"):
        existing_data = {}
        if os.path.exists(filename):
            with open(filename, 'r') as f:
                reader = csv.DictReader(f)
                for row in reader:
                    model_name = row['model']
                    existing_data[model_name] = row

        for model_metrics in self.metrics_by_model.values():
            model_name = model_metrics['model']
            if model_name in existing_data:
                existing_row = existing_data[model_name]
                for key, value in model_metrics.items():
                    if value is not None:
                        existing_row[key] = value
            else:
                full_metrics = {h: model_metrics.get(h, None) for h in self.headers}
                existing_data[model_name] = full_metrics

        with open(filename, 'w', newline='') as f:
            writer = csv.DictWriter(f, fieldnames=self.headers)
            writer.writeheader()
            for row in existing_data.values():
                complete_row = {h: row.get(h, None) for h in self.headers}
                writer.writerow(complete_row)

        self.metrics_by_model.clear()
        
    def print_metrics(self):
        for model, metrics in self.metrics_by_model.items():
            print(f"\n Metrics for model: {model}")
            for k, v in metrics.items():
                print(f"  {k:<20}: {v}")


# ================== Model Definitions ==================

class ResNet50(CustomModelBase):
    def __init__(self, num_classes=2):
        super(ResNet50, self).__init__()
        self.model = models.resnet50(weights=models.ResNet50_Weights.DEFAULT)
        self.model.fc = nn.Linear(self.model.fc.in_features, num_classes, bias=True)
        
    def forward(self, x):
        return self.model(x)
        
class ViT_B_16(CustomModelBase):
    def __init__(self, num_classes):
        super().__init__()
        self.model = vit_b_16(weights=ViT_B_16_Weights.DEFAULT)
        self.model.heads = nn.Linear(self.model.heads.head.in_features, num_classes)
        
    def forward(self, x):
        return self.model(x)

class EfficientNetB1(CustomModelBase):
    def __init__(self, num_classes):
        super().__init__()
        self.model = models.efficientnet_b1(weights=models.EfficientNet_B1_Weights.DEFAULT)
        self.model.classifier[1] = nn.Linear(self.model.classifier[1].in_features, num_classes)
        
    def forward(self, x):
        return self.model(x)

class MobileNet_V3_Small(CustomModelBase):
    def __init__(self, num_classes):
        super().__init__()
        self.model = models.mobilenet_v3_small(weights=models.MobileNet_V3_Small_Weights.DEFAULT)
        self.model.classifier[-1] = nn.Linear(self.model.classifier[-1].in_features, num_classes)
        
    def forward(self, x):
        return self.model(x)

class MobileNet_V3_Large(CustomModelBase):
    def __init__(self, num_classes):
        super().__init__()
        self.model = models.mobilenet_v3_large(weights=models.MobileNet_V3_Large_Weights.DEFAULT)
        self.model.classifier[-1] = nn.Linear(self.model.classifier[-1].in_features, num_classes)
        
    def forward(self, x):
        return self.model(x)

class InceptionNet(CustomModelBase):
    def __init__(self, num_classes):
        super().__init__()
        self.model = models.googlenet(weights=models.GoogLeNet_Weights.DEFAULT)
        self.model.fc = nn.Linear(self.model.fc.in_features, num_classes)
        
    def forward(self, x):
        return self.model(x)

MODEL_FACTORY = {
    'vit_b_16': ViT_B_16,
    'resnet50': ResNet50,
    'efficientnet_b1': EfficientNetB1,
    'mobilenet_v3_small': MobileNet_V3_Small,
    'mobilenet_v3_large': MobileNet_V3_Large,
    'inception': InceptionNet
}
# Definition end

# Specify which subdataset to use
DATASET_GROUP_IDX = None  # Could be: None, 0, 1, 2

# Define dataset paths based on the chosen JSON file
if train_valid_test_split_json_name == 'NEW25split.json':
    dataset_path = "/data/dataset1"
elif train_valid_test_split_json_name == 'splitNEW8.json':
    dataset_path = "/data/dataset2"
else:
    raise ValueError(f"Unknown JSON file: {train_valid_test_split_json_name}")

# Update extracted_data_path accordingly
extracted_data_path = '' #  TODO: Update this dataset
BATCH_SIZE=64


# Print number of classes
class_list = os.listdir(extracted_data_path)
class_list.sort()
if VERBOSE == True:
    print('Number of classes: {}'.format(len(class_list)))
    
    # Loading data
    print('Loading data ...')
image_files = []
targets = []

# Dictionary to map class_name to class_index
class_dict = {class_name: i for i, class_name in enumerate(class_list)}

# Load all image_paths
for class_name in class_dict.keys():
    repetitions_list = os.listdir(os.path.join(extracted_data_path, class_name))
    repetitions_list.sort()
    for repetition in repetitions_list:
        image_list = os.listdir(os.path.join(extracted_data_path, class_name, repetition))
        image_list.sort()
        image_files.extend(
            [os.path.join(extracted_data_path, class_name, repetition, img) for img in image_list]
        )
        targets.extend([class_dict[class_name]] * len(image_list))

targets = np.array(targets)
root_path=""
npy_data_path = f""
# Save data as .npy files if they do not exist
if not os.path.exists(os.path.join(npy_data_path, 'images.npy')):
    if VERBOSE == True:
        print('Saving data as .npy files to {}'.format(npy_data_path))
    images = np.empty((len(image_files), NUM_CHANNELS, 350, 350), dtype=np.uint8)
    for idx, img_path in enumerate(tqdm(image_files, position=0, leave=True)):
        img = cv.imread(img_path)
        img = cv.cvtColor(img, cv.COLOR_BGR2RGB)
        img = cv.resize(img, (350, 350))
        img = img.transpose(2, 0, 1)
        images[idx] = img

    with open(os.path.join(npy_data_path, 'images.npy'), 'wb') as npy_images_file, \
         open(os.path.join(npy_data_path, 'targets.npy'), 'wb') as npy_targets_file:
        np.save(npy_images_file, images, allow_pickle=True)
        np.save(npy_targets_file, targets, allow_pickle=True)
    with open(os.path.join(npy_data_path, 'image_files.json'), 'w') as f:
        json.dump(image_files, f)
    if VERBOSE == True:
        print('Data files saved')
else:
    images = np.load(os.path.join(npy_data_path, 'images.npy'))
    targets = np.load(os.path.join(npy_data_path, 'targets.npy'))
    if VERBOSE == True:
        print('Data files loaded')


targets = np.array(targets)


def get_model_transforms(model_name, mode='train'):
    transforms_list = []
    if model_name =='resnet50':
        base_transforms = [
            #T.Resize((350, 350)) 
            lambda x: x/255.0,  # Built into your dataset
            T.Lambda(lambda x: x.permute(2, 0, 1) if x.shape[-1] == 3 else x)
            
        ]
        if mode == 'train' and USE_AUGMENTATION:
            # Apply augmentations AFTER converting to CHW format
            augmentation_transforms = [
                T.RandomAffine(degrees=(-180, 180)),
                T.RandomHorizontalFlip(),
            ]
            return T.Compose(base_transforms + augmentation_transforms)
        return T.Compose(base_transforms)
    # ViT-specific transforms
    elif model_name == 'vit_b_16': # Example for a specific ViT model
        # ViT typically expects 224x224 input for many pre-trained models
        vit_input_size = 224 
        
       
        VIT_DEFAULT_MEAN = [0.5, 0.5, 0.5]
        VIT_DEFAULT_STD = [0.5, 0.5, 0.5]
    
        # ToTensor and Normalize are always the last steps for ViT
        transforms_list.extend([
            lambda x: x / 255.0,
            T.Lambda(lambda x: x.permute(2, 0, 1) if x.shape[-1] == 3 else x),
            T.Resize((vit_input_size, vit_input_size)),
            T.Normalize(mean=VIT_DEFAULT_MEAN, std=VIT_DEFAULT_STD) 
        ])
        return T.Compose(transforms_list)
    elif model_name == ' inception' :
        inception_input_size = 299
        INCEPTION_DEFAULT_MEAN = [0.485, 0.456, 0.456]
        INCEPTION_DEFAULT_STD = [0.229, 0.224, 0.225]
    
        # ToTensor and Normalize are always the last steps for ViT
        transforms_list.extend([
            lambda x: x / 255.0,
            T.Lambda(lambda x: x.permute(2, 0, 1) if x.shape[-1] == 3 else x),
            T.Resize((inception_input_size, inception_input_size)),
            T.Normalize(mean=INCEPTION_DEFAULT_MEAN, std=INCEPTION_DEFAULT_STD) 
        ])
        return T.compose(transforms_list)
        
    
    return T.Compose([
        T.Resize(256),
        T.CenterCrop(224),
        T.Normalize(mean=[0.5, 0.5, 0.5], std=[0.5, 0.5, 0.5])
    ])


# Define whether to use data augmentation
USE_AUGMENTATION = True

# Define the dataset class for classification
class ClassificationPlantDataset(torch.utils.data.Dataset):
    def __init__(self, images, targets, transform=None):
        self.images = images
        self.targets = targets
        self.transform = transform

    def __getitem__(self, index):
        # Normalize image data
        image = torch.tensor(self.images[index]).type(torch.float32) / 255

        # Apply transformation if provided
        if self.transform:
            image = self.transform(image)

        target = torch.tensor(self.targets[index])
        return image, target

    def __len__(self):
        return len(self.images)

# Create train dataset
if USE_AUGMENTATION:
    train_transforms = T.Compose([
        # Data augmentation effects
        T.RandomAffine(degrees=(-180, 180), translate=(0.2, 0.2), scale=(0.8, 1.5)),
        T.RandomHorizontalFlip(),
        # T.RandomRotation([45, 270])
    ])
else:
    train_transforms = None


# Create validation and test datasets
test_dataset = ClassificationPlantDataset(images, targets, transform=None)

# Combine validation and test datasets

# Data loaders
test_loader = torch.utils.data.DataLoader(dataset=test_dataset, batch_size=1, shuffle=True, drop_last=False)

def setup_distributed(rank, world_size):
    """Initialize distributed training"""
    # No need to set MASTER_ADDR/PORT — torchrun does it
    dist.init_process_group(
        backend="gloo",
        rank=rank,
        world_size=world_size
    )
    torch.cuda.set_device(rank)

def cleanup_distributed():
    dist.destroy_process_group()
 
VERBOSE = True

def run_inference(model_name, checkpoint_path):
    """Run comprehensive inference metrics for a trained model"""
    # Initialize metrics and device
    rank = int(os.environ['RANK'])
    world = int(os.environ['WORLD_SIZE'])
    setup_distributed(rank, world)
    logger = MetricLogger()
    device = torch.device('cuda' if torch.cuda.is_available() else 'cpu')
    if VERBOSE ==True:
        print(f"\n=== Running inference for {model_name} on {device} ===")

    # 1. Model Loading
    try:
        # Get model class and initialize
        num_classes = len(np.unique(targets))
        model_class = MODEL_FACTORY[model_name]
        model = model_class(8)
        
        # Load trained weights
        model.load_state_dict(torch.load(checkpoint_path, weights_only=True))
        print(" succesful load" )
        # Temporarily try single.
        model = DDP(model).to(device)
        model.eval()
        if VERBOSE == True:
            print(f"Loaded {model_name} from {checkpoint_path}")
    except Exception as e:
        print(f"Error loading model: {str(e)}")
        return

    # 2. Dataset Preparation
    test_transform = get_model_transforms(model_name, mode='val')
    test_dataset = ClassificationPlantDataset(
        images, targets, transform=test_transform
    )
    
    # Add sampler  
    inference_sampler = DistributedSampler(
        test_dataset, 
        num_replicas=world, 
        rank=rank,
        shuffle=True
    )
    
    inference_loader = DataLoader(
        test_dataset,
        batch_size=BATCH_SIZE,  
        sampler=inference_sampler,
        shuffle=False,
        num_workers=2,
        pin_memory=True,
        persistent_workers=True
    )
    print(f"Dataset length is {len(test_dataset)}, Batch size is {BATCH_SIZE} and inference loader has length {len(inference_loader)}")

    # 3. Warmup Runs
    if VERBOSE ==True:
        print("Running warmup cycles...")
    with torch.no_grad(), torch.amp.autocast('cuda'):
        for _ in range(10):
            inputs, _ = next(iter(inference_loader))
            if len(inputs.shape) == 4 and model_name =='resnet50':  # Batch dimension present
                if inputs.shape[1] != 3:  # Not in NCHW format
                    if inputs.shape[-1] == 3:  # NHWC format
                        if VERBOSE == True:
                            print(f"[Rank {rank}] Converting from NHWC to NCHW")
                        inputs = inputs.permute(0, 3, 1, 2)
                    elif inputs.shape[2] == 3:  # NWHC format (unlikely but possible)
                        if VERBOSE == True:
                            print(f"[Rank {rank}] Converting from NWHC to NCHW") 
                        inputs = inputs.permute(0, 2, 1, 3)
            inputs = inputs.to(device, non_blocking=True)
            _ = model(inputs)
    torch.cuda.synchronize()

    # 4. Memory Baseline
    pynvml.nvmlInit()
    handle = pynvml.nvmlDeviceGetHandleByIndex(0)
    mem_before = pynvml.nvmlDeviceGetMemoryInfo(handle).used

    # 5. Timing Runs
    total_images = 0
    start_event = torch.cuda.Event(enable_timing=True)
    end_event = torch.cuda.Event(enable_timing=True)

    if VERBOSE == True:
        print("Measuring inference performance...")
    with torch.no_grad(), torch.amp.autocast('cuda'):
        # Synchronize before timing
        torch.cuda.synchronize()
        
        # Start timing
        start_event.record()
        
        for inputs, _ in inference_loader:
            inputs = inputs.to(device, non_blocking=True)
            if len(inputs.shape) == 4 and model_name =='resnet50':  # Batch dimension present
                if inputs.shape[1] != 3:  # Not in NCHW format
                    if inputs.shape[-1] == 3:  # NHWC format
                        if VERBOSE == True:
                            print(f"[Rank {rank}] Converting from NHWC to NCHW")
                        inputs = inputs.permute(0, 3, 1, 2)
                    elif inputs.shape[2] == 3:  # NWHC format (unlikely but possible)
                        if VERBOSE == True:
                            print(f"[Rank {rank}] Converting from NWHC to NCHW") 
                        inputs = inputs.permute(0, 2, 1, 3)
            outputs = model(inputs)
            total_images += inputs.size(0)
        
        # End timing
        end_event.record()
        torch.cuda.synchronize()

    # 6. Calculate Metrics
    total_time = start_event.elapsed_time(end_event) / 1000  # Convert to seconds
    mem_after = pynvml.nvmlDeviceGetMemoryInfo(handle).used
    pynvml.nvmlShutdown()

    # 7. FLOPs Calculation
    try:
        # Get input size from transforms config
        import contextlib
        with contextlib.redirect_stderr(open(os.devnull, 'w')):
            finput = torch.randn(1, 3, 350, 350).to(device)
            # MACs to GFlops, See: https://discuss.pytorch.org/t/clarification-of-flops-macs-in-model-descriptions/198948
            flops = FlopCountAnalysis(model, finput).total()/(1e9 * 2)
    except Exception as e:
        print(f"FLOPs calculation failed: {str(e)}")
        flops = 0

    # 8. Compile Metrics
    metrics = {
        'model': model_name,
        'testing_time': round(total_time, 4),
        'testing_fps': round(total_images / total_time, 2),
        'testing_flops': round(flops, 2),
        # 'inference_gpu_mem_used': round((mem_after - mem_before) / (1024 ** 2), 2)
    }
    
    logger.add_metrics(metrics)
    if rank == 0:
        logger.print_metrics()
    logger.save_to_csv()
    
    # Print summary
    if VERBOSE == True:
        print("\n=== Inference Metrics ===")
        for k, v in metrics.items():
            print(f"{k:>20}: {v}")
        print("=========================\n")
    print("Inference Completed")

metrics = run_inference(MODEL_NAME_FROM_FACTORY, MODEL_WEIGHTS_PATH)