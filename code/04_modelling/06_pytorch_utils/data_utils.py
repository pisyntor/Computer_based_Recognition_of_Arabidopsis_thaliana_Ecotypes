import torch
import numpy as np
import cv2 as cv
import albumentations as A
from albumentations.pytorch import ToTensorV2

USE_AUGMENTATION = True
IMG_SIZE = (350, 350)
RESIZE_SHAPE = (256, 256)

class ClassificationPlantDataset(torch.utils.data.Dataset):
    def __init__(self, images, targets, transform=None):
        self.images = images
        self.targets = targets
        self.transform = transform

    def __getitem__(self, index):
        image = self.images[index]

        if not isinstance(image, np.ndarray):
            image = np.array(image)

        if image.dtype != np.uint8:
            image = image.astype(np.uint8)

        image = np.transpose(image, (1, 2, 0))
        if RESIZE_SHAPE != IMG_SIZE:
            image = cv.resize(image, RESIZE_SHAPE)

        if self.transform:
            transformed = self.transform(image=image)
            image = transformed["image"]

        image = image.astype(np.float32) / 255.0
        image = torch.tensor(image, dtype=torch.float32)
        image = image.permute(2, 0, 1)
        target = torch.tensor(self.targets[index])

        return image, target

    def __len__(self):
        return len(self.images)

class ClassificationPlantSequenceDataset(torch.utils.data.Dataset):
    def __init__(self, data, use_augmentation=False):
        self.use_augmentation = use_augmentation
        self.data = data

        if self.use_augmentation:
            self.transform = A.Compose([
                A.Resize(RESIZE_SHAPE[0], RESIZE_SHAPE[1]),
                A.Rotate(limit=180, p=0.7),
                A.Flip(p=0.9),
                A.ShiftScaleRotate(shift_limit=0.2, scale_limit=(0.2, 0.5)),
            ])
        else:
            self.transform = A.Compose([
                A.Resize(RESIZE_SHAPE[0], RESIZE_SHAPE[1]),
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

# define the transformations
if USE_AUGMENTATION:
    train_transform = A.Compose([
                A.Rotate(limit=180, p=0.7),
                A.Flip(),
                A.Transpose(),
                A.OneOf([
                    A.IAAAdditiveGaussianNoise(),
                    A.GaussNoise(),
                ], p=0.7),
                A.OneOf([
                    A.MotionBlur(p=1),
                    A.MedianBlur(blur_limit=3, p=1),
                    A.Blur(blur_limit=3, p=1),
                ], p=0.7),
                A.ShiftScaleRotate(shift_limit=0.0625, scale_limit=0.2, rotate_limit=45, p=0.2),
                A.OneOf([
                    A.OpticalDistortion(p=1),
                    A.GridDistortion(p=1),
                    A.IAAPiecewiseAffine(p=1),
                ], p=0.7),
                A.OneOf([
                    A.CLAHE(clip_limit=2),
                    A.IAASharpen(),
                    A.IAAEmboss(),
                    A.RandomBrightnessContrast(),
                ], p=0.7),
                A.HueSaturationValue(p=0.7),
            ])
else:
    train_transform = A.Compose([])