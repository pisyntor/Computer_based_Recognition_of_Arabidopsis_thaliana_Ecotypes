import os
import random
import torch
import shutil
import numpy as np
import matplotlib
from tqdm import tqdm
from pytorch_grad_cam import GradCAM
from pytorch_grad_cam.utils.image import show_cam_on_image
import matplotlib.pyplot as plt
matplotlib.use('Agg')

def get_all_conv_layers(model, modules_list=None, conv_layers=[], depth=0, grad_cam=False, feature_map=False):
    """
    Get all the convolutional layers of a given model
    """
    if modules_list is None:
        modules_list = list(model.modules())

    # get all the conv layers so that the last layer is used for grad cam visualisation
    if grad_cam and (not feature_map):
        for layer in modules_list:
            if isinstance(layer, torch.nn.Conv2d):
                conv_layers.append(layer)
            elif isinstance(layer, torch.nn.Sequential):
                get_all_conv_layers(model, layer, conv_layers, depth=depth + 1)

    # get all inner conv layers for feature map visualisation
    elif feature_map and (not grad_cam):
        for layer in modules_list:
            if isinstance(layer, torch.nn.Conv2d) and depth > 0:
                conv_layers.append(layer)
            elif isinstance(layer, torch.nn.Sequential) and depth > 0:
                get_all_conv_layers(model, layer, conv_layers, depth=depth + 1)

    return conv_layers


def visualise_feature_maps(feature_map, feature_map_name):
    """
    Visualise the feature maps of a given layer
    """
    feature_map = feature_map.cpu().numpy()

    # Get the number of feature maps
    num_feature_maps = feature_map.shape[1]

    # Calculate the number of rows and columns for the plot
    num_cols = 8
    num_rows = num_feature_maps // num_cols + int(num_feature_maps % num_cols > 0)

    fig, axes = plt.subplots(num_rows, num_cols, figsize=(num_cols * 2, num_rows * 2))
    plots = []
    for i in range(num_feature_maps):
        ax = axes[i // num_cols, i % num_cols]
        ax.imshow(feature_map[0, i], cmap="viridis")
        ax.axis("off")
        plots.append(feature_map[0, i])

    # Hide empty subplots
    for i in range(num_feature_maps, num_rows * num_cols):
        axes[i // num_cols, i % num_cols].axis("off")

    plt.savefig(feature_map_name)
    plt.close('all')

    # return the figure
    return plots


def normalize_feature_map(feature_map):
    min_val, max_val = np.min(feature_map), np.max(feature_map)
    return (feature_map - min_val) / (max_val - min_val)


def get_gradcam_feature_maps(model, test_loader, class_list, device, show_gradcam=False, max_gradcam_images=5, show_feature_map=False, max_feature_map_images=99999999, num_single_chart_layers=None, num_single_chart_conv_imgs=None, feature_map_max_classes=3, feature_map_max_images_per_class=2):
    """
    Compute GradCAM and feature maps for given number of images for a given model and a given test_loader
    """

    model.eval()

    # Get all conv layers from the given model and get the last layer to visualize in GradCAM
    layers = get_all_conv_layers(model, grad_cam=show_gradcam, feature_map=show_feature_map)
    target_layers = layers.copy()
    layer = layers[-1]

    if num_single_chart_layers is None:
        num_single_chart_layers = len(target_layers)

    cam = GradCAM(model=model, target_layers=[layer], use_cuda=True)

    # create folders if they don't exist
    if show_gradcam:
        if not os.path.exists("gradcams"):
            os.makedirs("gradcams")
        else:
            shutil.rmtree("gradcams")
            os.makedirs("gradcams")

    if show_feature_map:
        if not os.path.exists("feature_maps"):
            os.makedirs("feature_maps")
        else:
            shutil.rmtree("feature_maps")
            os.makedirs("feature_maps")

    # computing predictions and confusion matrix
    class_num_imgs = {}
    for i, (images, targets) in enumerate(tqdm(test_loader, position=0, leave=True)):
        # convert torch target to numpy and get the class name
        targets = targets.numpy()
        class_name = class_list[targets[0]]
        print(f"class_name: {class_name}")
        # print(f"images shape", images.shape)

        if show_feature_map:
            # check if there are enough images in all classes
            num_reached_classes = 0
            for class_name_index in class_num_imgs:
                if class_num_imgs[class_name_index] >= feature_map_max_images_per_class:
                    num_reached_classes += 1
            if num_reached_classes >= feature_map_max_classes:
                print("Reached max number of images per class, for all classes")
                break

            # check if we have enough classes
            if (len(class_num_imgs) >= feature_map_max_classes) and (class_name not in class_num_imgs):
                print("Reached max classes, yet to reach max images per class in all classes")
                continue

            # skip if we already have enough images for this class
            if class_name in class_num_imgs:
                if class_num_imgs[class_name] >= feature_map_max_images_per_class:
                    print("Reached max images per class, for:", class_name)
                    continue

            if class_name not in class_num_imgs:
                class_num_imgs[class_name] = 1
            else:
                class_num_imgs[class_name] += 1

        # get the image and the target
        images, targets = images.to(device, dtype=torch.float), torch.Tensor(targets).to(device)
        outputs = torch.nn.functional.log_softmax(model(images), dim=1)
        preds = torch.argsort(outputs, dim=1, descending=True)[:, :3]

        # get numpy array from images
        images_numpy = images.cpu().numpy()
        images_numpy = np.transpose(images_numpy, (0, 2, 3, 1))
        images_numpy = np.squeeze(images_numpy)

        # show GradCAMs for the max given images
        if show_gradcam and (i < max_gradcam_images):
            print(f"Extracting grad cam for image {i + 1}/{max_gradcam_images}")

            plt.figure(figsize=(30, 10))
            plt.subplot(1, 3, 1)
            plt.imshow(images_numpy)
            plt.gca().set_title(class_name, fontsize=40, pad=20, y=-0.2)
            plt.axis('off')

            grayscale_cam = cam(input_tensor=images, targets=None)
            grayscale_cam = grayscale_cam[0, :]
            plt.subplot(1, 3, 2)
            plt.imshow(grayscale_cam)
            plt.gca().set_title(class_name, fontsize=40, pad=20, y=-0.2)
            plt.axis('off')

            visualization = show_cam_on_image(images_numpy, grayscale_cam, use_rgb=True, image_weight=0.8)
            plt.subplot(1, 3, 3)
            plt.imshow(visualization)
            plt.gca().set_title(class_name, fontsize=40, pad=20, y=-0.2)
            plt.axis('off')

            plt.savefig(f"gradcams{os.sep}image_{i}_{class_name}.png")

        # show feature maps for the max given images
        if show_feature_map and i < max_feature_map_images:
            layers_folder = f"feature_maps{os.sep}class_{class_name}{os.sep}image_{i + 1}{os.sep}layers{os.sep}"
            prev_layers_folder = layers_folder.replace("layers" + os.sep, "")
            os.makedirs(layers_folder)

            # get feature maps for each detected target layers
            list_of_plots = {}
            valid_target_layers = []

            # Extract feature maps for each target layer and save them, if they are valid
            cnt = 0
            for j, layer in enumerate(target_layers):
                feature_maps = []

                def hook_fn(module, input, output):
                    feature_maps.append(output.detach())

                # register hook to the layer to get the feature maps
                layer.register_forward_hook(hook_fn)
                model(images)

                # if no feature maps were found, skip this layer
                if len(feature_maps) == 0:
                    layer._forward_hooks.clear()
                    continue
                feature_map_cpy = feature_maps[0].cpu().numpy()
                if feature_map_cpy.shape[-1] <=1:
                    layer._forward_hooks.clear()
                    continue

                print(f"Extracting feature maps for image {i + 1} from layer {j + 1}/{len(target_layers)}")

                # save feature maps using this function
                plots = visualise_feature_maps(feature_maps[0], f"{layers_folder}layer_{cnt + 1}.png")
                cnt += 1

                # update the list of valid target layers and the list of plots
                list_of_plots[j] = plots
                valid_target_layers.append((layer, j))  # save the layer and its index

                layer._forward_hooks.clear()

            plt.close()

            # select the smaller of chosen number of rows in the chart and number of valid target layers
            num_single_chart_layers = min(num_single_chart_layers, len(valid_target_layers))

            # pick num_single_chart_layers random layers without changing the order
            remove_layers_numbers = random.sample(valid_target_layers, max(len(valid_target_layers) - num_single_chart_layers, 0))
            valid_target_layers = [layer for layer in valid_target_layers if layer not in remove_layers_numbers]

            # put all layers in one image
            print(f"Merging all layers in one image...")
            plt.figure(figsize=(10 * num_single_chart_conv_imgs, 10 * num_single_chart_layers))
            cnt = 1

            # merge all feature map plots in one image to form a chart
            for j, layer in enumerate(valid_target_layers):
                layer, layer_index = layer
                plots = list_of_plots[layer_index]

                # pick num_single_chart_conv_imgs random features per layer without changing the order
                if num_single_chart_conv_imgs is not None:
                    plots_numbers = [random.randint(0, len(plots) - 1) for _ in range(num_single_chart_conv_imgs)]
                    plots_numbers = sorted(plots_numbers)
                    plots = [plots[i] for i in plots_numbers]
                else:
                    num_single_chart_conv_imgs = len(plots)

                # all plots in the selected layer
                for k, plot in enumerate(plots):
                    plot = normalize_feature_map(plot)

                    subplot = plt.subplot(len(valid_target_layers), num_single_chart_conv_imgs, cnt)  # (*nrows*, *ncols*, *index*)
                    cnt += 1

                    plt.imshow(plot, cmap="viridis")
                    plt.axis('off')

                    # Add "Row_j" ylabel to the first subplot of each row
                    if k == 0:
                        label_axis = subplot.twinx()
                        label_axis.set_ylabel(f"Layer_{layer_index + 1}", fontsize=40, rotation=0, labelpad=160)
                        label_axis.yaxis.set_label_position("left")
                        label_axis.yaxis.tick_left()
                        label_axis.yaxis.set_ticks([])
                        label_axis.xaxis.set_ticks([])

            plt.savefig(prev_layers_folder + f"Chart.png")
            plt.close()
            print(f"\nFeature maps and chart for image {i + 1} saved successfully!\n\n")

        # stop after max images
        if ((i > max_gradcam_images) and show_gradcam) or ((i > max_feature_map_images) and show_feature_map):
            break

    print("\nMaximum selected images completed!\n")