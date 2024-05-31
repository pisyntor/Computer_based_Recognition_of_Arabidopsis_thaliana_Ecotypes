from torch.optim import Adam as adam_opt
import torch
import torch.nn.functional as F
from tqdm import tqdm
import gc
import traceback


device = "cuda" if torch.cuda.is_available() else "cpu"


def _evaluate(
        model: torch.nn.Module,
        val_loader: torch.utils.data.DataLoader,
        device=device
):
    model.eval()
    outputs = []
    with torch.no_grad():
        for batch in val_loader:
            # Move the input tensors to the GPU if available
            batch = [tensor.to(device) for tensor in batch]
            outputs.append(model.validation_step(batch))
    return model.validation_epoch_end(outputs)


def _accuracy(
        outputs: torch.Tensor,
        labels: torch.Tensor
):
    preds = torch.argmax(outputs, dim=1)
    labels = torch.argmax(labels, dim=1)
    return torch.tensor(torch.sum(preds == labels).item() / len(preds))


def fit(
        epochs: int,
        lr: float,
        weight_decay: float,
        model: torch.nn.Module,
        train_loader: torch.utils.data.DataLoader,
        val_loader: torch.utils.data.DataLoader,
        callbacks_function=None,
        continue_training=False,
        opt_func=adam_opt,
        device=device,
        num_retries_inner=10,
        max_retry=10,
        evaluate=_evaluate
):
    """
    Meant to resemble the fit function in keras.

    Parameters
    ----------
    epochs - Set this to a high number and use callbacks to stop training early
    lr - Initial learning rate in case of a scheduler
    weight_decay - Weight decay to be fed to the optimizer
    model - The model to train. Must inherit from CustomModelBase of this module
    train_loader - The training data loader
    val_loader - The validation data loader
    callbacks_function - A function that takes the model and returns a list of callbacks
    opt_func - The optimizer function to use
    device - The device to use
    num_retries_inner - Number of times to retry training if it fails
    max_retry - Maximum number of times to retry training if anything other than training_step fails, like dataloader
    evaluate - The function to use for evaluation, defaults to _evaluate()

    Returns
    -------
    history - A list of dictionaries containing the loss and accuracy for each epoch

    """

    if weight_decay is not None:
        optimizer = opt_func(model.parameters(), lr, weight_decay=weight_decay)
    else:
        optimizer = opt_func(model.parameters(), lr)

    model.to(device)
    defined_callbacks = None  # must be None so that it can be defined in the function when it is called for the first time
    num_retry = 0
    history = []

    for epoch in range(epochs):
        model.train()  # Make sure the model is in training mode at each epoch, because it is set to eval() in evaluate()
        train_losses = []
        accuracies = []
        print("LR: ", optimizer.param_groups[0]['lr'])
        # Wrap the train_loader with tqdm to create a progress bar

        while num_retry < max_retry:
            try:
                progress_bar = tqdm(train_loader, desc=f"Epoch {epoch + 1}", delay=1)

                for batch in progress_bar:
                    batch = [tensor.to(device) for tensor in batch]

                    # run the training step many times until it works
                    flag = False
                    for i in range(num_retries_inner):
                        try:
                            loss, acc = model.training_step(batch)
                            flag = True
                            break
                        except:
                            if i == num_retries_inner - 1:
                                traceback.print_exc()

                            # try cleaning the cache
                            torch.cuda.empty_cache()
                            gc.collect()

                    if not flag:
                        raise RuntimeError(f"Training step failed {num_retries_inner} times")

                    train_losses.append(loss)
                    loss.backward()
                    optimizer.step()
                    optimizer.zero_grad()

                    accuracies.append(acc)
                    # Update the progress bar with the current loss and accuracy
                    progress_bar.set_postfix(loss=loss.item(), accuracy=acc.item())

                num_retry = 0
                break
            except:
                # try cleaning the cache
                torch.cuda.empty_cache()
                gc.collect()

                num_retry += 1
                if num_retry < max_retry:
                    continue
                else:
                    traceback.print_exc()
                    raise RuntimeError(f"Training failed {max_retry} times")

        result = evaluate(model, val_loader, device)
        result['train_loss'] = torch.stack(train_losses).mean().item()
        result['train_acc'] = torch.stack(accuracies).mean().item()

        if callbacks_function is not None:
            defined_callbacks, stop_flag = callbacks_function(
                optimiser=optimizer,
                result=result,
                model=model,
                defined_callbacks=defined_callbacks,
                continue_training=continue_training,
            )

            model.epoch_end(epoch, result)
            history.append(result)

            if stop_flag:
                print("Early stopping triggered")
                break

    return history


class CustomModelBase(torch.nn.Module):
    """
    Base class for custom models. This class is meant to be inherited from and not used directly. Override the training_step, and validation_step if you want to use custom loss functions.
    This class must be inherited in case you want to use the fit() function defined in this module.

    Parameters
    ----------
    class_weights : torch.Tensor
        The class weights to use for the loss function. This should be a 1D tensor with the same number of elements as the number of classes.
        Ideally, they should be normalized so that the sum of the weights is 1.
        Examples:
        - [0.11765947096395296, 0.21896579990935885, 0.2190948310230356, 0.23457661088081475, 0.2097032872228378]
    loss_function
        The loss function to use. Must be touch.nn.functional. This should be a function that takes in the model outputs, the labels, and any other arguments that are needed.
        Defaults to torch.nn.functional.cross_entropy.
    accuracy_function
        The accuracy function to use. This should be a function that takes in the model outputs and the labels and returns the accuracy.
        Defaults to the accuracy function defined in this module.
    """

    def __init__(
            self,
            class_weights=None,
            loss_function=F.cross_entropy,
            accuracy_function=_accuracy
    ):
        super(CustomModelBase, self).__init__()
        self.class_weights = class_weights
        self.loss_function = loss_function
        self.accuracy_function = accuracy_function

    def training_step(self, batch: list):
        """
        The training step. This is meant to be overridden if you want to use a custom loss function.
        Parameters
        ----------
        batch : list of torch.Tensor
            Examples:
            - batch = [tensor.to(device) for tensor in batch]

        Returns
        -------
        loss : torch.Tensor
        acc : torch.Tensor
        """

        images, labels = batch
        out = self(images)  # Generate predictions

        loss = self.loss_function(out, labels, weight=self.class_weights)  # Calculate loss with class weights
        acc = self.accuracy_function(out, labels)  # Calculate accuracy
        return loss, acc

    def validation_step(self, batch: list):
        """
        The validation step. This is meant to be overridden if you want to use a custom loss function.
        Parameters
        ----------
        batch : list of torch.Tensor
            Examples:
            - batch = [tensor.to(device) for tensor in batch]

        Returns
        -------
        loss : torch.Tensor
        acc : torch.Tensor
        """

        images, labels = batch
        out = self(images)  # Generate predictions

        loss = self.loss_function(out, labels, weight=self.class_weights)  # Calculate loss with class weights
        acc = self.accuracy_function(out, labels)  # Calculate accuracy
        return {'val_loss': loss.detach(), 'val_acc': acc}

    def validation_epoch_end(self, outputs):
        """
        Used to combine the results in the validation step and return the average loss and accuracy. Override this if you want to use custom metrics.
        """

        batch_losses = [x['val_loss'] for x in outputs]
        epoch_loss = torch.stack(batch_losses).mean()  # Combine losses
        batch_accs = [x['val_acc'] for x in outputs]
        epoch_acc = torch.stack(batch_accs).mean()  # Combine accuracies
        return {'val_loss': epoch_loss.item(), 'val_acc': epoch_acc.item()}

    def epoch_end(self, epoch, result):
        """
        Used to print the results of the epoch. Called at the end of each epoch in fit()
        """

        print(
            f"train_loss: {result['train_loss']:.4f}, val_loss: {result['val_loss']:.4f}\n"
            f"train_acc: {result['train_acc']:.4f}, val_acc: {result['val_acc']:.4f}"
        )

        print()


import os
import torch
import pickle
import numpy as np
import matplotlib.pyplot as plt
from torch.optim.lr_scheduler import _LRScheduler
import pytorch_utils.callbacks as pt_callbacks

class PolyScheduler(_LRScheduler):
    def __init__(self, optimizer, base_lr, max_steps, warmup_steps, last_epoch=-1):
        self.base_lr = base_lr
        self.warmup_lr_init = 0.0001
        self.max_steps: int = max_steps
        self.warmup_steps: int = warmup_steps
        self.power = 2
        super(PolyScheduler, self).__init__(optimizer, -1, False)
        self.last_epoch = last_epoch

    def get_warmup_lr(self):
        alpha = float(self.last_epoch) / float(self.warmup_steps)
        return [self.base_lr * alpha for _ in self.optimizer.param_groups]

    def get_lr(self):
        if self.last_epoch == -1:
            return [self.warmup_lr_init for _ in self.optimizer.param_groups]
        if self.last_epoch < self.warmup_steps:
            return self.get_warmup_lr()
        else:
            alpha = pow(
                1
                - float(self.last_epoch - self.warmup_steps)
                / float(self.max_steps - self.warmup_steps),
                self.power,
            )
            return [self.base_lr * alpha for _ in self.optimizer.param_groups]

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

def load_model_weights(BACKBONE, OPTIMIZER, weights_path, device):
    # this function loads the whole model with weights
    pth = os.path.join(weights_path, 'backbone_{}_{}.pth'.format(BACKBONE, OPTIMIZER))
    assert os.path.exists(pth), 'Configuration not found'

    model = torch.load(pth).to(device)
    print('Backbone weights loaded: {}'.format(BACKBONE))

    return model

def load_model_stats(BACKBONE, OPTIMIZER, stats_path):
    # this function loads training stats (train/val loss and acc)
    stats_file = os.path.join(stats_path, 'stats_{}_{}.pkl'.format(BACKBONE, OPTIMIZER))
    assert os.path.exists(stats_file), 'Configuration not found'

    with open(stats_file, 'rb') as stats:
        stats = pickle.load(stats)
    print('Train/Validation stats loaded for the model: {}'.format(BACKBONE))

    return stats
