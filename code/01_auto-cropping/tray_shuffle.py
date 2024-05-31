import numpy as np

def shuffle_list(list_of_lists):
    # Take the first list from the list_of_lists
    base_list = list_of_lists[0]
    
    # Shuffle the base list using numpy's permutation function
    shuffled_list = list(np.random.permutation(base_list))

    # Check if the shuffled list has any element in the same position as any original list
    for lst in list_of_lists:
        for original, shuffled in zip(lst, shuffled_list):
            if original == shuffled:
                # If any element is in the same position, return None
                return None
    # If no elements are in the same position, return the shuffled list
    return shuffled_list

# Define initial lists
list1 = [
    'Bch-4', 'Li-5:2', 'Ba5-1', 'TOU-J-3', 'Go-0', 'Pro-0', 'Utreht', 'Ler-1',
    'TOU-I-17', 'Lz-0', 'Uk-4', 'Sav-0', 'Wt-5', 'Ei-6', 'Udul 1-34', 'Uk-1',
    'Zdr-1', 'NC-1', 'Wil-2', 'Can-0'
]

list2 = [
    'Br-0', 'MIB-28', 'Hs-0', 'PHW-2', 'CIBC-2', 'Is-1', 'Ws-2', 'Ors-2',
    'Ede-1', 'Ba4-1', 'Ren-11', 'C24', 'Edr-1', 'Hovdala-2', 'Or-0', 'Col-0',
    'Cvi-0', 'Hsm', 'Jm-0', 'Kz-9'
]

# Initialize list_of_lists with the first list
list_of_lists = [list1]

# Loop to generate additional shuffled lists
while len(list_of_lists) < 10:
    # Attempt to generate a new shuffled list
    new_list = shuffle_list(list_of_lists)
    
    # If a valid shuffled list is generated, add it to list_of_lists
    if new_list:
        list_of_lists.append(new_list)
        
        # Reshape the new list into a 5x4 array for display
        reshaped_array = np.reshape(new_list, (5, 4))
        
        # Print the newly reshaped array
        print('\n New array: ', reshaped_array)
