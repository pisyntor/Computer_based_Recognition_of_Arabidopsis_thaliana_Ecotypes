import numpy as np
import os
import gc
import torch
import torch.nn as nn
import torchvision
import torchvision.models as models
import torchvision.transforms as T
import torch.distributed as dist

from scipy import stats
import cv2 as cv
import matplotlib
import matplotlib.pyplot as plt
from torchsummary import summary
from sklearn.model_selection import train_test_split
from tqdm.auto import tqdm
from torch.utils.data import DataLoader, DistributedSampler
from torch.nn.parallel import DistributedDataParallel as DDP
from fvcore.nn import FlopCountAnalysis
import pynvml
import csv

from torch.optim.lr_scheduler import _LRScheduler
import torch.nn.functional as F
# for evaluation 
from torchmetrics import ConfusionMatrix
import pandas as pd
import seaborn as sn
import pickle
import re
import time
import json
import shutil

from pytorch_utils.training_utils import *
import pytorch_utils.training_utils as pt_train
default_matplotlib_backend = matplotlib.get_backend()

# Configurable Variables
NUM_CHANNELS=3
USE_2_GPUS = True

VERBOSE = False
DEBUG = True
NUM_EPOCHS = 2
NUM_GPUS = 2
USE_AUGMENTATION = False

# Configure on each run
NUM_CLASSES = 8 # Change to 25 if using Dataset 1
MODEL_NAME = "" # See MODEL_FACTORY
MODELS_DIR=""
CHECKPOINT_WEIGHTS_FILE_NAME = ""
CHECKPOINT_PATH= os.path.join(MODELS_DIR, CHECKPOINT_WEIGHTS_FILE_NAME)

# For first dataset IMAGES_IN_A_DAY is 2, second dataset IMAGES_IN_A_DAY is 4
IMAGES_IN_A_DAY = 4 

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


if USE_2_GPUS:
    # working device
    os.environ["CUDA_DEVICE_ORDER"]="PCI_BUS_ID"  # Arrange GPU devices starting from 0
    os.environ["CUDA_VISIBLE_DEVICES"]= "0,1" 
    device = torch.device("cuda" if torch.cuda.is_available() else "cpu")
    if VERBOSE == True:
        print('Selected device: {}'.format(device))
else:
    # working device
    device = torch.device("cuda:1" if torch.cuda.is_available() else "cpu")
    print('Selected device: {}'.format(device))

# Helper functions for evaluation and accuracy calculation
def _evaluate(model: torch.nn.Module, val_loader: DataLoader, device=device):
    model.eval()
    outputs = []
    with torch.no_grad():
        for batch in val_loader:
            batch = [tensor.to(device).float() for tensor in batch]
            if USE_2_GPUS:
                outputs.append(model.module.validation_step(batch))
            else:
                outputs.append(model.validation_step(batch))
    if USE_2_GPUS:
        return model.module.validation_epoch_end(outputs)
    else:
        return model.validation_epoch_end(outputs)

RUN_MODE = 'LIVE' # Or Dev if needed

# Important, to have the same repartition of data between different machines
np.random.seed(42)

def create_folder(new_path):
    if not os.path.exists(new_path):
        os.makedirs(new_path)


if not os.path.exists(weights_path):
    os.makedirs(weights_path)
if not os.path.exists(stats_path):
    os.makedirs(stats_path)

if not os.path.exists(os.path.dirname(model_save_path)):
    os.makedirs(model_save_path)



# getting the list of the classes
class_list = os.listdir(extracted_data_path)
class_list.sort()
print('Number of classes: {}'.format(len(class_list)))

# working device
device = torch.device("cuda" if torch.cuda.is_available() else "cpu")
print('Selected device: {}'.format(device))
if device.type != 'cpu' and VERBOSE == True:
    print('Device name: {}'.format(torch.cuda.get_device_name(device)))

BATCH_SIZE = 16 if RUN_MODE == 'DEV' else 16
# Change to 8 for actual testing
DAY_TO_USE = 8

SEQ_LEN = DAY_TO_USE * IMAGES_IN_A_DAY
EPOCHS = 60
IMG_SIZE = (350, 350) # base size! if you want to change it you have to do it by T.Resize(*target_size). .npy data already on (350, 350)
RESIZE_SIZE = (128, 128)
NUM_CHANNELS = 3
BACKBONE = 'EfficientNetB2'
OPTIMIZER = 'Adam'

available_backbones = [
    'EfficientNetB1', 'EfficientNetB2',
    'EfficientNetV2_S', 'ConvNext_T', 'MobileNet_V3_Small', 'MobileNet_V3_Large',
    'ViT_B_16'
]

avaible_optimizers = ['SGD', 'Adam', 'AdamW', 'RMSprop']

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

# some stats about the data
image_files = []
targets = []

# dict helps to go from class_name to class_index
class_dict = dict([(j, i) for i, j in enumerate(class_list)])

# loading all image_paths (IN A SORTED ORDER, this is really important to avoid any weird exceptions)
for class_name in class_dict.keys():
    if class_name.startswith('.'):
        continue
    repetitions_list = os.listdir(extracted_data_path + class_name)
    repetitions_list.sort()
    for repetition in repetitions_list:
        if repetition.startswith('.'):
            continue
        image_list = os.listdir(extracted_data_path + class_name + os.sep + repetition)
        image_list.sort()
        image_files.extend(
            [extracted_data_path + class_name + os.sep + repetition + os.sep + img for img in image_list]
        )
        targets.extend([class_dict[class_name]] * len(image_list))

targets = np.array(targets)


from copy import deepcopy

def _fix_images_paths(images_paths, targets):
    images_paths = deepcopy(images_paths)
    targets = deepcopy(targets)
    while os.path.basename(images_paths[0]).split('_')[1:4] != os.path.basename(image_files[1]).split('_')[1:4]:
        images_paths = images_paths[1:]
        targets = targets[1:]
    while os.path.basename(images_paths[-1]).split('_')[1:4] != os.path.basename(image_files[-2]).split('_')[1:4]:
        images_paths = images_paths[:-1]
        targets = targets[:-1]
    return images_paths, targets

def prepare_dataset(image_files, targets, train_valid_test_split=None, fix_len=10, eval_only=True):
    images_paths, targets_cls = _fix_images_paths(image_files, targets)

    data_as_dict = {}
    for el, _trg in zip(images_paths, targets_cls):
        _cls, _rep_name = el.split(os.sep)[-3:-1]
        k = (_cls, _rep_name)
        if k not in data_as_dict:
            data_as_dict[k] = []
        data_as_dict[k].append((el, _trg))

    # Add more images to make divisible by fix_len
    for k in data_as_dict:
        while len(data_as_dict[k]) % fix_len != 0:
            data_as_dict[k].append(data_as_dict[k][-1])
    
    if eval_only:
        # For evaluation, create sequences from all available data
        datas = {'test': []}
        for k in data_as_dict:
            files_in_rep = len(data_as_dict[k])
            for start_idx in range(0, files_in_rep-fix_len+1, 2):
                seq = data_as_dict[k][start_idx:start_idx+fix_len]
                assert len(seq) == fix_len
                datas['test'].append(seq)
        return datas
    
    # Original training logic
    datas = {}
    for k in train_valid_test_split:
        for rep_name in train_valid_test_split[k]:
            if (k, rep_name) not in data_as_dict:
                continue
            data_type = train_valid_test_split[k][rep_name]
            if data_type not in datas:
                datas[data_type] = []
            files_in_rep = len(data_as_dict[(k, rep_name)])
            for start_idx in range(0,files_in_rep-fix_len+1,2):
                seq = data_as_dict[(k, rep_name)][start_idx:start_idx+fix_len]
                assert len(seq) == fix_len
                datas[data_type].append(seq)
    return datas

print(f" We have {len(image_files)} image files")
datas = prepare_dataset(image_files, targets, fix_len=max(SEQ_LEN, 10))
print(f" We have {len(datas['test'])} sequences")


def rotate_image(image, angle):
    image_center = tuple(np.array(image.shape[1::-1]) / 2)
    rot_mat = cv.getRotationMatrix2D(image_center, angle, 1.0)
    result = cv.warpAffine(image, rot_mat, image.shape[1::-1], flags=cv.INTER_LINEAR)
    return result

def zoom_at(img, zoom=1, angle=0, coord=None):
    cy, cx = [ i/2 for i in img.shape[:-1] ] if coord is None else coord[::-1]
    rot_mat = cv.getRotationMatrix2D((cx,cy), angle, zoom)
    result = cv.warpAffine(img, rot_mat, img.shape[1::-1], flags=cv.INTER_LINEAR)
    return result

import albumentations as A

class ClassificationPlantSequenceDataset(torch.utils.data.Dataset):
    def __init__(self, data, use_augmentation=False):
        self.use_augmentation = use_augmentation
        self.data = data

        if self.use_augmentation:
            self.transform = A.Compose([
                A.Resize(RESIZE_SIZE[0], RESIZE_SIZE[1]),
                A.Rotate(limit=180, p=0.7),
                A.Flip(p=0.9),
                A.ShiftScaleRotate(shift_limit=0.2, scale_limit=(0.2, 0.5)),
            ])
        else:
            self.transform = A.Compose([
                A.Resize(RESIZE_SIZE[0], RESIZE_SIZE[1]),
            ])

    def __getitem__(self, index):
        seq, classes = list(zip(*self.data[index]))
        assert len(set(classes)) == 1, 'wrong seq clses'
        seq_cls = classes[0]

        images = []
        for im_path in seq:
            img = cv.imread(im_path)
            img = cv.cvtColor(img, cv.COLOR_BGR2RGB)
            img = cv.resize(img, IMG_SIZE)
            img = self.transform(image=img)['image']
            img = img.astype(np.float32) / 255.
            img = torch.from_numpy(img.transpose((2, 0, 1)))
            images.append(img)

        images = torch.stack(images)
        images = torch.permute(images, (1, 0, 2, 3))
        return images, seq_cls

    def __len__(self):
        return len(self.data)

# loading data
test_dataset = ClassificationPlantSequenceDataset(datas['test'])
print(f"Data set length is {test_dataset}")

# data loaders
test_loader = torch.utils.data.DataLoader(dataset=test_dataset, batch_size=1, shuffle=True, drop_last=False)

if VERBOSE == True:
    print('Data loaders created')

def plot_model_stats(model_name, train_loss, train_acc, test_loss, test_acc):
    fig = plt.figure(figsize=(16, 5))
    ax1 = plt.subplot(1, 2, 1)
    plt.plot(np.arange(len(train_loss)), train_loss, label = 'Train loss')
    plt.plot(np.arange(len(test_loss)), test_loss, label = 'Val loss')
    plt.title('Model: {} - Validation loss: {:.4f}'.format(model_name, test_loss[-1]))
    ax1.set_ylabel("Loss")
    ax1.set_xlabel("Epochs")
    ax1.legend(loc='upper right')
    ax2 = plt.subplot(1, 2, 2)
    plt.plot(np.arange(len(train_acc)), np.array(train_acc) * 100, color='green', label = 'Train accuracy')
    plt.plot(np.arange(len(test_acc)), np.array(test_acc) * 100, color='red', label = 'Val accuracy')
    plt.title('Model: {} - Validation accuracy: {:.2f} %'.format(model_name, test_acc[-1] * 100))
    ax2.set_ylabel("Accuracy")
    ax2.set_xlabel("Epochs")
    ax2.legend(loc='lower right')
    plt.show()

def load_model_weights(BACKBONE=BACKBONE, OPTIMIZER=OPTIMIZER):
    # this function loads the whole model with weights
    pth = os.path.join(weights_path, 'backbone_{}_{}.pth'.format(BACKBONE, OPTIMIZER))
    assert os.path.exists(pth), 'Configuration not found'
    model = torch.load(pth, weights_only=False).to(device)
    print('Backbone weights loaded: {}'.format(BACKBONE))
    return model

def load_model_stats(BACKBONE=BACKBONE, OPTIMIZER=OPTIMIZER):
    # this function loads training stats (train/val loss and acc)
    stats_file = os.path.join(stats_path, 'stats_{}_{}.pkl'.format(BACKBONE, OPTIMIZER))
    assert os.path.exists(stats_file), 'Configuration not found'
    with open(stats_file, 'rb') as stats:
        stats = pickle.load(stats)
    print('Train/Validation stats loaded for the model: {}'.format(BACKBONE))
    return stats

def _handlezero_division_np(a,b):
    # initialize output tensor with desired value
    c = np.zeros_like(a)
    mask = (b != 0)
    # finally perform division
    c[mask] = a[mask] / b[mask]
    return c

def mathews_correlation_coefficient_np(tp, fp, fn, tn):
    tp = tp.sum().astype(np.float64)
    tn = tn.sum().astype(np.float64)
    fp = fp.sum().astype(np.float64)
    fn = fn.sum().astype(np.float64)
    _numerator = (tp*tn - fp*fn)
    _denomerator = np.sqrt((tp+fp)*(tp+fn)*(tn+fp)*(tn+fn))
    x = _numerator / (_denomerator + 1e-11)
    return x


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





def evaluate_model(model, test_loader, verbose=True, eps=1e-10):
    if verbose:
        print('--------------------------------------------')
        print('Test metrics (on test set)')

    model.eval()

    confusion_matrix = ConfusionMatrix(num_classes=len(class_list))
    eval_preds = list()
    eval_targs = list()

    # computing predictions and confusion matrix
    for i, (images, targets) in enumerate(tqdm(test_loader, position=0, leave=True)):
        images, targets = images.to(device, dtype=torch.float), torch.Tensor(targets).to(device)
        outputs = torch.nn.functional.log_softmax(model(images), dim=1)
        preds = torch.argsort(outputs, dim=1, descending=True)[:, :3]
        eval_preds.extend(preds[:, 0].cpu().numpy())
        eval_targs.extend(targets.cpu().numpy())

    # computing main metrics (acc, precisio, recall and f1 score)
    matrix = confusion_matrix(torch.tensor(eval_preds), torch.tensor(eval_targs))
    accuracy = matrix.trace() / (matrix.sum()+eps)
    # loss = F.cross_entropy(torch.tensor(eval_preds), torch.tensor(eval_targs))
    precision = np.array([matrix[i, i] / (matrix.sum(axis=0)[i]+eps) for i in range(len(class_list))])
    recall = np.array([matrix[i, i] / (matrix.sum(axis=1)[i]+eps) for i in range(len(class_list))])
    f1_score = 2 * precision * recall / (precision + recall+eps)

    # computing false positive rate, false negative rate, false discovery rate, false omission rate
    fp_rate = np.zeros(len(class_list))
    for idx in range(len(class_list)):
        tn = matrix.trace() - matrix[idx, idx]
        fp = np.sum([matrix[j, idx] for j in range(len(class_list)) if j != idx])
        fp_rate[idx] = fp / (fp + tn+eps)

    fn_rate = 1 - recall
    fd_rate = 1 - precision
    specificity = 1 - fp_rate

    fo_rate = np.zeros(len(class_list))
    for idx in range(len(class_list)):
        n = np.sum(np.array(eval_targs) != idx)
        fn = np.sum([matrix.sum(axis=0)[j] - matrix[j, j] for j in range(len(class_list)) if j != idx])
        fo_rate[idx] = fn / (n+eps)

    missclassification_rate = 1 - accuracy
    npv = 1 - fo_rate

    mcc_per_class = []
    for idx in range(len(class_list)):
        tp = matrix[idx, idx].cpu().numpy()
        tn = (matrix.trace() - matrix[idx, idx]).cpu().numpy()
        fp = np.sum([matrix[j, idx] for j in range(len(class_list)) if j != idx])
        fn = np.sum([matrix.sum(axis=0)[j] - matrix[j, j] for j in range(len(class_list)) if j != idx])
        _mcc = mathews_correlation_coefficient_np(tp, fp, fn, tn)
        mcc_per_class.append(_mcc)

    if verbose:
        print('--------------------------------------------')
        print('Accuracy: {:.3f}%'.format(accuracy * 100))
        # print('Loss: {:.3f}'.format(loss))
        print('Average precision: {:.3f}'.format(precision.mean()))
        print('Average recall: {:.3f}'.format(recall.mean()))
        print('Average F1 score: {:.3f}'.format(f1_score.mean()))
        print('Average specificity: {:.3f}'.format(specificity.mean()))
        print('Average false positive rate: {:3f}'.format(fp_rate.mean()))
        print('Average false negative rate: {:3f}'.format(fn_rate.mean()))
        print('Average false discovery rate: {:.3f}'.format(fd_rate.mean()))
        print('Average false omission rate: {:.3f}'.format(fo_rate.mean()))
        print('Missclassification rate: {:.2f}%'.format(missclassification_rate * 100))
        print('Mathews Correlation Coefficient: {:.2f}'.format(np.mean(mcc_per_class)))
        print('--------------------------------------------')
        print('Results by class :')
        print('--------------------------------------------')
        print('{:<15}{:<12}{:<12}{:<12}{:<12}{:<12}{:<12}{:<12}{:<12}{:<12}{:<12}'.format('', 'Precision', 'Recall', 'F1 score', 'Specificity', 'FPR', 'FNR', 'FDR', 'FOR', 'NPV', 'MCC'))
        for idx, class_name in enumerate(class_list):
            print('{:<15}{:<12.2f}{:<12.2f}{:<12.2f}{:<12.3f}{:<12.3f}{:<12.3f}{:<12.3f}{:<12.3f}{:<12.3f}{:<12.3f}'.format(
                class_name, precision[idx], recall[idx], f1_score[idx], specificity[idx], fp_rate[idx], fn_rate[idx], fd_rate[idx], fo_rate[idx], npv[idx], mcc_per_class[idx]
            ))
        print('--------------------------------------------')
        print()

        # ploting confusion matrix
        matrix_df = pd.DataFrame(matrix.numpy(), index=class_list, columns=class_list)
        plt.figure(figsize=(12, 8))
        sn.heatmap(matrix_df, annot=True, fmt='d', cmap='Blues')

    return accuracy, precision, recall, f1_score, matrix_df



def accuracy(
        outputs: torch.Tensor,
        labels: torch.Tensor
):
    """
    Custom accuracy function to override the default one in pt_train
    """

    preds = torch.argmax(outputs, dim=1)

    return torch.tensor(torch.sum(preds == labels).item() / len(preds))


class CustomModelBase(pt_train.CustomModelBase):
    """
    ModelBase override for training and validation steps
    """

    def __init__(self, class_weights=None):
        super(CustomModelBase, self).__init__()
        self.class_weights = class_weights
        self.accuracy_function = accuracy

    def training_step(self, batch):
        images, labels = batch
        labels = labels.long() 
        out = self(images)  # Generate predictions
        loss = F.cross_entropy(out, labels, weight=self.class_weights)  # Calculate loss with class weights
        acc = accuracy(out, labels)  # Calculate accuracy
        return loss, acc

    def validation_step(self, batch):
        images, labels = batch
        labels = labels.long()
        out = self(images)  # Generate predictions
        loss = F.cross_entropy(out, labels, weight=self.class_weights)  # Calculate loss with class weights
        acc = accuracy(out, labels)  # Calculate accuracy
        return {'val_loss': loss.detach(), 'val_acc': acc}


DROPOUT = 0.0
HIDDEN_SIZE = 256
NUM_CLASSES = len(class_list)  # default was len(class_list)

from vivit_modules import ViViT

vivit_small = ViViT(
    image_size=128,          # Match your actual image size
    patch_size=16,           # 128/16 = 8 patches per side (64 total patches)
    num_classes=NUM_CLASSES, 
    num_frames=SEQ_LEN,           
    dim=192,                 
    depth=12,
    heads=3,
    pool='cls',
    in_channels=3,
    dim_head=64,
    dropout=0.1,
    emb_dropout=0.1,
    scale_dim=4,
)

#--------------------------------
# EfficientNetB1
#--------------------------------

class EfficientNetB1(CustomModelBase):
    def __init__(self, num_classes=NUM_CLASSES):
        super(EfficientNetB1, self).__init__()
        self.model = models.efficientnet_b1(weights=models.EfficientNet_B1_Weights.DEFAULT)
        # self.model.classifier = nn.Linear(self.model.classifier[-1].in_features, num_classes, bias=True)
        self.model.classifier = nn.Identity()  # Remove last layer. Final layer in not useful when using this model as feature extractor
        self.out_features = models.efficientnet_b1().classifier[-1].in_features



    def forward(self, x):
        return self.model(x)



class Eff1_GRU(CustomModelBase):
    def __init__(self, num_classes, hidden_size=HIDDEN_SIZE, num_layers=2):
        super(Eff1_GRU, self).__init__()
        self.hidden_size = hidden_size
        self.num_layers  = num_layers

        self.model = EfficientNetB1()

        self.gru = nn.GRU(self.model.out_features, self.hidden_size, self.num_layers, batch_first=True, dropout=DROPOUT)
        self.linear1 = nn.Linear(self.hidden_size * SEQ_LEN, num_classes*2, bias=True)
        self.linear2 = nn.Linear(num_classes*2, num_classes, bias=True)

    def forward(self, x):
        embeddings = []
        for idx in range(SEQ_LEN):
            emb = self.model(x[:,:,idx])
            embeddings.append(emb[:,None])
        embeddings = torch.concat(embeddings, 1)

        h = torch.zeros(self.num_layers, x.size(0), self.hidden_size).to(device)
        out, h = self.gru(embeddings, h)
        out = out.reshape(out.shape[0], -1)
        out = self.linear1(out)
        out = self.linear2(out)
        return out


class Eff1_LSTM(CustomModelBase):
    def __init__(self, num_classes, hidden_size=HIDDEN_SIZE, num_layers=2):
        super(Eff1_LSTM, self).__init__()
        self.hidden_size = hidden_size
        self.num_layers  = num_layers

        self.model = EfficientNetB1()

        self.lstm = nn.LSTM(self.model.out_features, self.hidden_size, self.num_layers, batch_first=True, dropout=DROPOUT)
        self.linear1 = nn.Linear(self.hidden_size * SEQ_LEN, num_classes*2, bias=True)
        self.linear2 = nn.Linear(num_classes*2, num_classes, bias=True)

    def forward(self, x):
        embeddings = []
        for idx in range(SEQ_LEN):
            emb = self.model(x[:,:,idx])
            embeddings.append(emb[:,None])
        embeddings = torch.concat(embeddings, 1)

        h0 = torch.zeros(self.num_layers, x.size(0), self.hidden_size).to(device).requires_grad_()
        c0 = torch.zeros(self.num_layers, x.size(0), self.hidden_size).to(device).requires_grad_()

        out, (hn, cn) = self.lstm(embeddings, (h0.detach(), c0.detach()))
        out = out.reshape(out.shape[0], -1)
        out = self.linear1(out)
        out = self.linear2(out)
        return out

class Eff1_BiLSTM(CustomModelBase):
    def __init__(self, num_classes, hidden_size=HIDDEN_SIZE, num_layers=2, bidirectional=True):
        super(Eff1_BiLSTM, self).__init__()
        self.hidden_size = hidden_size
        self.num_layers  = num_layers

        self.model = EfficientNetB1()

        self.lstm = nn.LSTM(self.model.out_features, self.hidden_size, self.num_layers, batch_first=True, bidirectional=bidirectional)
        self.num_directions = 2 if bidirectional else 1
        self.linear1 = nn.Linear(self.hidden_size * SEQ_LEN * self.num_directions, num_classes*2, bias=True)
        self.linear2 = nn.Linear(num_classes*2, num_classes, bias=True)

    def forward(self, x):
        embeddings = []
        for idx in range(SEQ_LEN):
            emb = self.model(x[:,:,idx])
            embeddings.append(emb[:,None])
        embeddings = torch.concat(embeddings, 1)

        h0 = torch.zeros(self.num_layers*self.num_directions, x.size(0), self.hidden_size).to(device).requires_grad_()
        c0 = torch.zeros(self.num_layers*self.num_directions, x.size(0), self.hidden_size).to(device).requires_grad_()

        out, (hn, cn) = self.lstm(embeddings, (h0.detach(), c0.detach()))
        out = out.reshape(out.shape[0], -1)
        out = self.linear1(out)
        out = self.linear2(out)
        return out


class Eff1_BiGRU(CustomModelBase):
    def __init__(self, num_classes, hidden_size=HIDDEN_SIZE, num_layers=2, bidirectional=True):
        super(Eff1_BiGRU, self).__init__()
        self.hidden_size = hidden_size
        self.num_layers  = num_layers

        self.model = EfficientNetB1()

        self.gru = nn.GRU(self.model.out_features, self.hidden_size, self.num_layers, batch_first=True, bidirectional=bidirectional)
        self.num_directions = 2 if bidirectional else 1
        self.linear1 = nn.Linear(self.hidden_size * SEQ_LEN * self.num_directions, num_classes*2, bias=True)
        self.linear2 = nn.Linear(num_classes*2, num_classes, bias=True)

    def forward(self, x):
        embeddings = []
        for idx in range(SEQ_LEN):
            emb = self.model(x[:,:,idx])
            embeddings.append(emb[:,None])
        embeddings = torch.concat(embeddings, 1)

        h = torch.zeros(self.num_layers*self.num_directions, x.size(0), self.hidden_size).to(device)
        out, h = self.gru(embeddings, h)
        out = out.reshape(out.shape[0], -1)
        out = self.linear1(out)
        out = self.linear2(out)
        return out
        
MODEL_FACTORY = {
        "Eff1_GRU": Eff1_GRU,
        "Eff1_LSTM": Eff1_LSTM,
        "Eff1_BiGRU": Eff1_BiGRU,
        "Eff1_BiLSTM": Eff1_BiLSTM,
        "ViViT_Small": vivit_small,
}
def get_model_by_name(name):    
    return MODEL_FACTORY[name]



def run_inference(model_name):
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
        model_class = MODEL_FACTORY[model_name]
        if model_name == "ViViT_Small":
            model = ViViT(
                    image_size=128,          # Match your actual image size
                    patch_size=16,           # 128/16 = 8 patches per side (64 total patches)
                    num_classes=NUM_CLASSES, 
                    num_frames=SEQ_LEN,           
                    dim=192,                 
                    depth=12,
                    heads=3,
                    pool='cls',
                    in_channels=3,
                    dim_head=64,
                    dropout=0.1,
                    emb_dropout=0.1,
                    scale_dim=4,
                ).to(device)
        else:
            model = model_class(NUM_CLASSES)
        
        # Load trained weights
        model.load_state_dict(torch.load(CHECKPOINT_PATH, weights_only=True))
        model = DDP(model).to(device)
        model.eval()
        if VERBOSE == True:
            print(f"Loaded {model_name} from {CHECKPOINT_PATH}")
    except Exception as e:
        print(f"Error loading model: {str(e)}")
        return

    # 2. Dataset Preparation
    
    inference_loader = DataLoader(
        test_dataset,
        batch_size=BATCH_SIZE,  
        shuffle=False,
        num_workers=2,
        pin_memory=True,
        persistent_workers=True
    )

    # 3. Warmup Runs
    if VERBOSE ==True:
        print("Running warmup cycles...")
    with torch.no_grad(), torch.amp.autocast('cuda'):
        for _ in range(10):
            inputs, _ = next(iter(inference_loader))
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
        
        for inputs, _ in tqdm(inference_loader):
            inputs = inputs.to(device, non_blocking=True)

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
            image_size = 350 # Default without resize
            if model_name == "ViViT_Small":
                image_size = 128
            finput = torch.randn(1, 3, SEQ_LEN, image_size, image_size).to(device)
            # MACs to GFlops, See: https://discuss.pytorch.org/t/clarification-of-flops-macs-in-model-descriptions/198948
            flops = FlopCountAnalysis(model, finput).total()/(1e9 * 2)
    except Exception as e:
        print(f"FLOPs calculation failed: {str(e)}")
        flops = 0

    # 8. Compile Metrics
    metrics = {
        'model': model_name,
        'testing_time': round(total_time, 4),
        'testing_fps': round((total_images * SEQ_LEN) / total_time, 2),
        'testing_flops': round(flops, 2),
        # 'inference_gpu_mem_used': round((mem_after - mem_before) / (1024 ** 2), 2)
    }
    
    logger.add_metrics(metrics)
    if rank == 0:
        logger.print_metrics()
    logger.save_to_csv()
    cleanup_distributed()
    
    # Print summary
    if rank == 0:
        print("\n=== Inference Metrics ===")
        for k, v in metrics.items():
            print(f"{k:>20}: {v}")
        print("=========================\n")
        print("Inference Completed")

if __name__ == "__main__" :
    run_inference(MODEL_NAME)