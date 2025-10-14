// Folder Management JavaScript
let isCreatingFolder = false;

document.addEventListener('DOMContentLoaded', function () {
    document.getElementById('newFolderBtn').addEventListener('click', createNewFolder);
});

function createNewFolder() {
    if (isCreatingFolder) return;

    isCreatingFolder = true;
    const folderId = generateGuid();

    // Hide empty state if it exists
    const emptyState = document.getElementById('emptyState');
    if (emptyState) {
        emptyState.style.display = 'none';
    }

    // Create new folder card
    const folderHtml = `
        <div class="col-md-3 mb-3 folder-item" data-folder-id="${folderId}">
            <div class="card h-100 border-primary">
                <div class="card-body">
                    <div class="d-flex align-items-center mb-2">
                        <i class="fas fa-folder text-warning me-2 fs-4"></i>
                        <div class="flex-grow-1">
                            <div class="folder-name-display d-none"></div>
                            <input type="text" class="form-control folder-name-input" 
                                   placeholder="Enter folder name" maxlength="255"
                                   onblur="saveNewFolder('${folderId}')" 
                                   onkeydown="handleNewFolderKeydown(event, '${folderId}')">
                        </div>
                    </div>
                    <small class="text-muted">Creating...</small>
                </div>
                <div class="card-footer bg-transparent">
                    <div class="btn-group w-100" role="group">
                        <button type="button" class="btn btn-outline-secondary btn-sm" disabled>
                            <i class="fas fa-edit"></i> Rename
                        </button>
                        <button type="button" class="btn btn-outline-danger btn-sm" 
                                onclick="cancelNewFolder('${folderId}')">
                            <i class="fas fa-times"></i> Cancel
                        </button>
                    </div>
                </div>
            </div>
        </div>
    `;

    // Add to container
    const container = document.getElementById('folderContainer');
    container.insertAdjacentHTML('afterbegin', folderHtml);

    // Focus on input
    const input = document.querySelector(`[data-folder-id="${folderId}"] .folder-name-input`);
    input.focus();
}

function saveNewFolder(folderId) {
    const folderElement = document.querySelector(`[data-folder-id="${folderId}"]`);
    const input = folderElement.querySelector('.folder-name-input');
    const name = input.value.trim();

    if (!name) {
        cancelNewFolder(folderId);
        return;
    }

    // Show loading
    folderElement.querySelector('.card').classList.add('opacity-50');

    fetch('/Folder/CreateFolder', {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json',
        },
        body: JSON.stringify({ name: name })
    })
        .then(response => response.json())
        .then(data => {
            if (data.success) {
                // Update the folder element with real data
                folderElement.setAttribute('data-folder-id', data.folder.id);
                folderElement.querySelector('.folder-name-display').textContent = data.folder.name;
                folderElement.querySelector('.folder-name-input').value = data.folder.name;

                // Switch to display mode
                folderElement.querySelector('.folder-name-display').classList.remove('d-none');
                folderElement.querySelector('.folder-name-input').classList.add('d-none');

                // Update footer
                const footer = folderElement.querySelector('.card-footer');
                footer.innerHTML = `
                <div class="btn-group w-100" role="group">
                    <button type="button" class="btn btn-outline-secondary btn-sm" 
                            onclick="editFolderName('${data.folder.id}')">
                        <i class="fas fa-edit"></i> Rename
                    </button>
                    <button type="button" class="btn btn-outline-danger btn-sm" 
                            onclick="deleteFolder('${data.folder.id}', '${data.folder.name}')">
                        <i class="fas fa-trash"></i> Delete
                    </button>
                </div>
            `;

                // Remove border and loading state
                folderElement.querySelector('.card').classList.remove('border-primary', 'opacity-50');

                // Update created date
                folderElement.querySelector('small').textContent = 'Created: Just now';

                showToast('Folder created successfully!', 'success');
            } else {
                showToast(data.message || 'Failed to create folder', 'error');
                cancelNewFolder(folderId);
            }
        })
        .catch(error => {
            console.error('Error:', error);
            showToast('Failed to create folder', 'error');
            cancelNewFolder(folderId);
        })
        .finally(() => {
            isCreatingFolder = false;
        });
}

function cancelNewFolder(folderId) {
    const folderElement = document.querySelector(`[data-folder-id="${folderId}"]`);
    if (folderElement) {
        folderElement.remove();
    }

    // Show empty state if no folders exist
    const remainingFolders = document.querySelectorAll('.folder-item');
    if (remainingFolders.length === 0) {
        const emptyState = document.getElementById('emptyState');
        if (emptyState) {
            emptyState.style.display = 'block';
        }
    }

    isCreatingFolder = false;
}

function handleNewFolderKeydown(event, folderId) {
    if (event.key === 'Enter') {
        event.preventDefault();
        const input = event.target;
        input.onblur = null;
        saveNewFolder(folderId);
    } else if (event.key === 'Escape') {
        event.preventDefault();
        cancelNewFolder(folderId);
    }
}

function editFolderName(folderId) {
    const folderElement = document.querySelector(`[data-folder-id="${folderId}"]`);
    const display = folderElement.querySelector('.folder-name-display');
    const input = folderElement.querySelector('.folder-name-input');

    display.classList.add('d-none');
    input.classList.remove('d-none');
    input.focus();
    input.select();
}

function saveFolderName(folderId) {
    const folderElement = document.querySelector(`[data-folder-id="${folderId}"]`);
    const display = folderElement.querySelector('.folder-name-display');
    const input = folderElement.querySelector('.folder-name-input');
    const newName = input.value.trim();
    const originalName = display.textContent;

    if (!newName || newName === originalName) {
        // Revert to original name
        input.value = originalName;
        display.classList.remove('d-none');
        input.classList.add('d-none');
        return;
    }

    // Show loading
    folderElement.querySelector('.card').classList.add('opacity-50');

    fetch('/Folder/RenameFolder', {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json',
        },
        body: JSON.stringify({ id: folderId, name: newName })
    })
        .then(response => response.json())
        .then(data => {
            if (data.success) {
                display.textContent = newName;
                showToast('Folder renamed successfully!', 'success');
            } else {
                input.value = originalName;
                showToast(data.message || 'Failed to rename folder', 'error');
            }
        })
        .catch(error => {
            console.error('Error:', error);
            input.value = originalName;
            showToast('Failed to rename folder', 'error');
        })
        .finally(() => {
            display.classList.remove('d-none');
            input.classList.add('d-none');
            folderElement.querySelector('.card').classList.remove('opacity-50');
        });
}

function handleFolderNameKeydown(event, folderId) {
    if (event.key === 'Enter') {
        event.preventDefault();
        saveFolderName(folderId);
    } else if (event.key === 'Escape') {
        event.preventDefault();
        const folderElement = document.querySelector(`[data-folder-id="${folderId}"]`);
        const display = folderElement.querySelector('.folder-name-display');
        const input = folderElement.querySelector('.folder-name-input');

        input.value = display.textContent;
        display.classList.remove('d-none');
        input.classList.add('d-none');
    }
}

function deleteFolder(folderId, folderName) {
    if (!confirm(`Are you sure you want to delete "${folderName}"? This action cannot be undone.`)) {
        return;
    }

    const folderElement = document.querySelector(`[data-folder-id="${folderId}"]`);
    folderElement.querySelector('.card').classList.add('opacity-50');

    fetch('/Folder/DeleteFolder', {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json',
        },
        body: JSON.stringify({ id: folderId })
    })
        .then(response => response.json())
        .then(data => {
            if (data.success) {
                folderElement.remove();
                showToast('Folder deleted successfully!', 'success');

                // Show empty state if no folders left
                const remainingFolders = document.querySelectorAll('.folder-item');
                if (remainingFolders.length === 0) {
                    const emptyState = document.getElementById('emptyState');
                    if (emptyState) {
                        emptyState.style.display = 'block';
                    }
                }
            } else {
                showToast(data.message || 'Failed to delete folder', 'error');
                folderElement.querySelector('.card').classList.remove('opacity-50');
            }
        })
        .catch(error => {
            console.error('Error:', error);
            showToast('Failed to delete folder', 'error');
            folderElement.querySelector('.card').classList.remove('opacity-50');
        });
}

function showToast(message, type = 'info') {
    // Create toast container if it doesn't exist
    let toastContainer = document.getElementById('toast-container');
    if (!toastContainer) {
        toastContainer = document.createElement('div');
        toastContainer.id = 'toast-container';
        toastContainer.className = 'position-fixed top-0 end-0 p-3';
        toastContainer.style.zIndex = '1055';
        document.body.appendChild(toastContainer);
    }

    // Create toast
    const toastId = `toast-${Date.now()}`;
    const bgClass = type === 'success' ? 'bg-success' : type === 'error' ? 'bg-danger' : 'bg-info';

    const toastHtml = `
        <div id="${toastId}" class="toast ${bgClass} text-white" role="alert">
            <div class="toast-body">
                ${message}
                <button type="button" class="btn-close btn-close-white float-end" data-bs-dismiss="toast"></button>
            </div>
        </div>
    `;

    toastContainer.insertAdjacentHTML('beforeend', toastHtml);

    // Show toast
    const toastElement = document.getElementById(toastId);
    const toast = new bootstrap.Toast(toastElement, { autohide: true, delay: 3000 });
    toast.show();

    // Remove from DOM after hiding
    toastElement.addEventListener('hidden.bs.toast', () => {
        toastElement.remove();
    });
}

function generateGuid() {
    return 'xxxxxxxx-xxxx-4xxx-yxxx-xxxxxxxxxxxx'.replace(/[xy]/g, function (c) {
        const r = Math.random() * 16 | 0;
        const v = c == 'x' ? r : (r & 0x3 | 0x8);
        return v.toString(16);
    });
}
