window.getWindowWidth = function() {
    return window.innerWidth;
};

// Detect if device supports touch interactions
window.isTouchDevice = function() {
    return (('ontouchstart' in window) ||
            (navigator.maxTouchPoints > 0) ||
            (navigator.msMaxTouchPoints > 0));
};

// Check if device is mobile with touch
window.isMobileWithTouch = function() {
    const width = window.innerWidth;
    const isMobile = width < 768;
    const hasTouch = window.isTouchDevice();
    return isMobile && hasTouch;
};

window.focusElement = function(element) {
    if (element) {
        element.focus();
    }
};
// Focus an element by reference and select text if it's an input
window.focusElementAndSelect = function(element) {
    if (element) {
        element.focus();
        // Also select all text if it's an input
        if (element.tagName === 'INPUT' && element.type === 'text') {
            element.select();
        }
    }
};

// Copy text to clipboard
window.copyToClipboard = async function(text) {
    try {
        await navigator.clipboard.writeText(text);
        console.log('Text copied to clipboard');
    } catch (err) {
        console.error('Failed to copy text: ', err);
        // Fallback for older browsers
        const textArea = document.createElement("textarea");
        textArea.value = text;
        document.body.appendChild(textArea);
        textArea.focus();
        textArea.select();
        try {
            document.execCommand('copy');
            console.log('Text copied to clipboard (fallback)');
        } catch (err) {
            console.error('Fallback copy failed: ', err);
        }
        document.body.removeChild(textArea);
    }
};

// Check if threads overflow and apply appropriate class to sidebar
window.checkThreadOverflow = function() {
    const threadsSection = document.querySelector('.threads-scroll-section');
    const drawerCustom = document.querySelector('.drawer-custom');
    
    if (!threadsSection || !drawerCustom) return;
    
    const isOverflowing = threadsSection.scrollHeight > threadsSection.clientHeight;
    
    if (isOverflowing) {
        drawerCustom.classList.add('has-overflow');
    } else {
        drawerCustom.classList.remove('has-overflow');
    }
};

// Set up ResizeObserver to check for overflow changes
window.setupOverflowObserver = function() {
    const threadsSection = document.querySelector('.threads-scroll-section');
    if (!threadsSection) return;
    
    if (window.threadsResizeObserver) {
        window.threadsResizeObserver.disconnect();
    }
    
    window.threadsResizeObserver = new ResizeObserver(() => {
        window.checkThreadOverflow();
    });
    
    window.threadsResizeObserver.observe(threadsSection);
};

window.isChatScrolledToBottom = function() {
    const chatContainer = document.querySelector('.chat-messages-container');
    if (!chatContainer) return true;
    return chatContainer.scrollHeight - chatContainer.scrollTop - chatContainer.clientHeight < 10;
};

window.scrollChatToBottom = function() {
    const chatContainer = document.querySelector('.chat-messages-container');
    if (chatContainer) {
        chatContainer.scrollTop = chatContainer.scrollHeight;
    }
};

window.scrollChatToBottomNoAnimation = function() {
    const chatContainer = document.querySelector('.chat-messages-container');
    if (chatContainer) {
        // Save current scroll-behavior
        const oldBehavior = chatContainer.style.scrollBehavior;
        // Force instant scroll
        chatContainer.style.scrollBehavior = 'auto';
        chatContainer.scrollTop = chatContainer.scrollHeight;
        // Restore old scroll-behavior
        chatContainer.style.scrollBehavior = oldBehavior;
    }
};

window.setupChatScrollHandler = function(dotnetRef) {
    const chatContainer = document.querySelector('.chat-messages-container');
    if (!chatContainer) return;
    if (chatContainer._scrollHandler) return; // Only attach once

    chatContainer._scrollHandler = function() {
        const atBottom = (chatContainer.scrollHeight - chatContainer.scrollTop - chatContainer.clientHeight < 10);
        dotnetRef.invokeMethodAsync('SetScrolledToBottom', atBottom);
    };
    chatContainer.addEventListener('scroll', chatContainer._scrollHandler);

    // Fire once on setup to sync initial state
    chatContainer._scrollHandler();
};

window.cleanupChatScrollHandler = function() {
    const chatContainer = document.querySelector('.chat-messages-container');
    if (chatContainer && chatContainer._scrollHandler) {
        chatContainer.removeEventListener('scroll', chatContainer._scrollHandler);
        delete chatContainer._scrollHandler;
    }
};

// Focus on input when page loads
window.focusChatInput = function() {
    const input = document.querySelector('.message-input input');
    if (input) {
        input.focus();
    }
};

// Textarea auto-resize functions for ChatInput
window.initializeTextareaAutoResize = function(textarea) {
    if (!textarea) return;
    
    // Set initial height
    textarea.style.height = 'auto';
    textarea.style.height = Math.max(textarea.scrollHeight, 52) + 'px';
};

window.autoResizeTextarea = function(textarea) {
    if (!textarea) return;
    
    // Reset height to auto to get the correct scrollHeight
    textarea.style.height = 'auto';
    
    // Determine min and max heights based on the component
    let minHeight, maxHeight;
    
    if (textarea.classList.contains('message-editor-textarea')) {
        // MessageEditor component - larger heights
        minHeight = 140;
        maxHeight = 400;
    } else {
        // ChatInput component - original heights
        minHeight = 52;
        maxHeight = 200;
    }
    
    // Calculate new height
    const newHeight = Math.min(Math.max(textarea.scrollHeight, minHeight), maxHeight);
    textarea.style.height = newHeight + 'px';
    
    // If content exceeds max height, show scrollbar
    if (textarea.scrollHeight > maxHeight) {
        textarea.style.overflowY = 'auto';
    } else {
        textarea.style.overflowY = 'hidden';
    }
};

window.resetTextareaHeight = function(textarea) {
    if (!textarea) return;
    
    textarea.style.height = '52px';
    textarea.style.overflowY = 'hidden';
};

// Set history state for navigation
window.setHistory = function(url) {
    if (typeof history.pushState === 'function') {
        history.pushState(null, '', url);
    } else {
        console.warn('History API not supported');
    }
}

// Download file from URL
window.downloadFile = function(url, filename) {
    try {
        const link = document.createElement('a');
        link.href = url;
        link.download = filename || 'download';
        link.style.display = 'none';
        
        document.body.appendChild(link);
        link.click();
        document.body.removeChild(link);
        
        console.log('File download initiated:', filename);
    } catch (err) {
        console.error('Failed to download file:', err);
    }
}

// Trigger file input selection
window.triggerFileInput = function(elementIdOrSupportedTypes, supportedContentTypes) {
    try {
        let fileInput;
        let acceptTypes;
        
        // Support both old and new calling patterns
        if (typeof elementIdOrSupportedTypes === 'string' && elementIdOrSupportedTypes.includes('hiddenFileInput')) {
            // New pattern: triggerFileInput(elementId)
            fileInput = document.getElementById(elementIdOrSupportedTypes);
            acceptTypes = supportedContentTypes || '*/*';
        } else {
            // Old pattern: triggerFileInput(supportedContentTypes)
            fileInput = document.getElementById('hiddenFileInput');
            acceptTypes = elementIdOrSupportedTypes || '*/*';
        }
        
        if (fileInput) {
            fileInput.setAttribute('accept', acceptTypes);
            if (fileInput.click) {
                fileInput.click();
            }
        }
    } catch (err) {
        console.error('Failed to trigger file input:', err);
    }
}

// Handle paste events with file attachments
window.setupPasteFileUpload = function(textareaElement, dotNetRef) {
    if (!textareaElement || !dotNetRef) return;
    
    // Remove existing paste handler if it exists
    if (textareaElement._pasteHandler) {
        textareaElement.removeEventListener('paste', textareaElement._pasteHandler);
    }
    
    textareaElement._pasteHandler = async function(event) {
        try {
            const clipboardData = event.clipboardData || window.clipboardData;
            
            if (!clipboardData || !clipboardData.items) {
                return; // No clipboard data available
            }
            
            const files = [];
            
            // Check for files in clipboard
            for (let i = 0; i < clipboardData.items.length; i++) {
                const item = clipboardData.items[i];
                
                if (item.kind === 'file') {
                    const file = item.getAsFile();
                    if (file) {
                        files.push(file);
                    }
                }
            }
            
            if (files.length > 0) {
                // Prevent default paste behavior when files are detected
                event.preventDefault();
                
                // Process each file and read its data
                const fileDataArray = [];
                
                for (const file of files) {
                    try {
                        // Read file as ArrayBuffer
                        const arrayBuffer = await file.arrayBuffer();
                        const uint8Array = new Uint8Array(arrayBuffer);
                        
                        // Convert to base64 for transfer to C#
                        const base64Data = btoa(String.fromCharCode.apply(null, uint8Array));
                        
                        fileDataArray.push({
                            name: file.name || `pasted-file-${Date.now()}.${getFileExtension(file.type)}`,
                            size: file.size,
                            type: file.type || 'application/octet-stream',
                            lastModified: file.lastModified || Date.now(),
                            data: base64Data
                        });
                    } catch (error) {
                        console.error('Error reading pasted file:', error);
                    }
                }
                
                if (fileDataArray.length > 0) {
                    // Call the Blazor component method to handle pasted files
                    await dotNetRef.invokeMethodAsync('HandlePastedFiles', fileDataArray);
                }
            }
        } catch (error) {
            console.error('Error handling paste event:', error);
        }
    };
    
    textareaElement.addEventListener('paste', textareaElement._pasteHandler);
};

// Helper function to get file extension from MIME type
function getFileExtension(mimeType) {
    const mimeToExt = {
        'image/png': 'png',
        'image/jpeg': 'jpg',
        'image/gif': 'gif',
        'image/webp': 'webp',
        'image/svg+xml': 'svg',
        'text/plain': 'txt',
        'application/pdf': 'pdf',
        'application/json': 'json',
        'application/xml': 'xml',
        'text/html': 'html',
        'text/css': 'css',
        'text/javascript': 'js',
        'application/javascript': 'js'
    };
    return mimeToExt[mimeType] || 'bin';
}

// Get pasted files stored in window (no longer needed but keeping for compatibility)
window.getPastedFiles = function() {
    return [];
};

// Remove paste handler
window.removePasteFileUpload = function(textareaElement) {
    if (!textareaElement || !textareaElement._pasteHandler) return;
    
    textareaElement.removeEventListener('paste', textareaElement._pasteHandler);
    delete textareaElement._pasteHandler;
};

window.setupChatInputAutoFocus = function (element) {
    if (!element) return;
    // Store handler on element so we can remove it later
    element._chatInputAutoFocusHandler = function (e) {
        const tag = document.activeElement.tagName;
        const editable = document.activeElement.isContentEditable;
        if (
            !e.ctrlKey && !e.metaKey && !e.altKey &&
            !editable &&
            tag !== 'INPUT' && tag !== 'TEXTAREA' && tag !== 'SELECT'
        ) {
            if (e.key.length === 1) {
                element.focus();
            }
        }
    };
    document.addEventListener('keydown', element._chatInputAutoFocusHandler);
};

// Call this to remove it:
window.removeChatInputAutoFocus = function (element) {
    if (!element || !element._chatInputAutoFocusHandler) return;
    document.removeEventListener('keydown', element._chatInputAutoFocusHandler);
    delete element._chatInputAutoFocusHandler;
};

window.getHighlightedHtml = function(code, language) {
    if (typeof hljs === 'undefined') {
        console.warn('Highlight.js is not loaded');
        return code;
    }
    
    try {
        // Auto-detect language
        const result = hljs.highlightAuto(code);
        return result.value;
    } catch (error) {
        console.warn('Error highlighting code:', error);
        // Return the original code if highlighting fails
        return code;
    }
}

window.addClickOutsideHandler = function(elementId, dotNetRef) {
    const element = document.getElementById(elementId);
    if (!element) return;

    const clickHandler = function(event) {
        // Check if the click is outside the element
        if (!element.contains(event.target)) {
            // Add a small delay to ensure the click event on the button has been processed
            setTimeout(() => {
                dotNetRef.invokeMethodAsync('HandleClickOutside');
            }, 0);
        }
    };

    // Store the handler on the element so we can remove it later
    element._clickOutsideHandler = clickHandler;
    document.addEventListener('click', clickHandler);
};

window.removeClickOutsideHandler = function(elementId) {
    const element = document.getElementById(elementId);
    if (!element || !element._clickOutsideHandler) return;

    document.removeEventListener('click', element._clickOutsideHandler);
    delete element._clickOutsideHandler;
};

// Store Blazor component references
window._blazorComponents = window._blazorComponents || {};

// Function to register a component reference
window.registerBlazorComponent = function(name, dotNetRef) {
    window._blazorComponents[name] = dotNetRef;
};

// Function to unregister a component reference
window.unregisterBlazorComponent = function(name) {
    delete window._blazorComponents[name];
};

// Document click handler
window.handleDocumentClick = function(event) {
    const reasoningWrapper = document.getElementById('reasoning-selector-wrapper');
    const reasoningDropdown = document.querySelector('.reasoning-dropdown');
    
    if (reasoningWrapper && reasoningDropdown) {
        // Check if the click is outside both the wrapper and dropdown
        if (!reasoningWrapper.contains(event.target) && !reasoningDropdown.contains(event.target)) {
            // Find the Blazor component and invoke the close method
            const dotNetRef = window._blazorComponents?.reasoningMenu;
            if (dotNetRef) {
                dotNetRef.invokeMethodAsync('HandleDocumentClick');
            }
        }
    }
};

// Add document click handler
window.addDocumentClickHandler = function() {
    document.addEventListener('click', window.handleDocumentClick);
};

// Remove document click handler
window.removeDocumentClickHandler = function() {
    document.removeEventListener('click', window.handleDocumentClick);
};


window.renderLatex = function(content) {
    if (typeof katex === 'undefined') {
        console.warn('LaTeX is not loaded');
        return;
    }
    // Render the LateX string as HTML
    try {
        var html = katex.renderToString(content, {
            throwOnError: false,
            output: 'html',
            displayMode: false
        });
        // Insert the rendered HTML into the page
        return html;
    } catch (error) {
        console.error('Error rendering LaTeX:', error);
    }
}

window.registerChatHistoryShortcuts = function(dotNetRef) {
    // Detect platform - use userAgent as primary method, platform as fallback
    const isMac = navigator.userAgent.indexOf('Mac') !== -1 || 
                  (navigator.platform && navigator.platform.indexOf('Mac') !== -1);
    
    function handler(e) {
        const isK = (e.key === 'k' || e.key === 'K');
        const isB = (e.key === 'b' || e.key === 'B');
        
        if (isMac) {
            // On Mac: only Cmd+K and Cmd+B work
            if (e.metaKey && isK) {
                e.preventDefault();
                dotNetRef.invokeMethodAsync('OpenSearchShortcut');
            }
            if (e.metaKey && isB) {
                e.preventDefault();
                dotNetRef.invokeMethodAsync('NewChatShortcut');
            }
        } else {
            // On Windows/Linux: only Ctrl+K and Ctrl+B work
            if (e.ctrlKey && isK) {
                e.preventDefault();
                dotNetRef.invokeMethodAsync('OpenSearchShortcut');
            }
            if (e.ctrlKey && isB) {
                e.preventDefault();
                dotNetRef.invokeMethodAsync('NewChatShortcut');
            }
        }
    }
    window.__chatHistoryShortcutHandler = handler;
    window.addEventListener('keydown', handler);
}

window.unregisterChatHistoryShortcuts = function() {
    if (window.__chatHistoryShortcutHandler) {
        window.removeEventListener('keydown', window.__chatHistoryShortcutHandler);
        window.__chatHistoryShortcutHandler = null;
    }
}

// Register visual feedback for keyboard shortcuts
window.registerShortcutVisual = function(shortcut, element) {
    if (!element) return;

    const isMac = isMacPlatform();
    let platformShortcut = shortcut;
    if (isMac && shortcut.startsWith('ctrl+')) {
        platformShortcut = shortcut.replace('ctrl+', 'meta+');
    } else if (!isMac && shortcut.startsWith('cmd+')) {
        platformShortcut = shortcut.replace('cmd+', 'ctrl+');
    }
    const keys = platformShortcut.toLowerCase().split('+').map(k => k.trim());
    let animationTimeout = null;

    function matchesShortcut(e, keyArr) {
        return keyArr.every(key => {
            if (key === 'ctrl') return e.ctrlKey;
            if (key === 'shift') return e.shiftKey;
            if (key === 'alt') return e.altKey;
            if (key === 'meta') return e.metaKey;
            return e.key.toLowerCase() === key;
        });
    }

    function onKeyDown(e) {
        // Only animate on the first keydown, not on repeats
        if (matchesShortcut(e, keys) && !e.repeat) {
            if (animationTimeout) {
                clearTimeout(animationTimeout);
                animationTimeout = null;
            }
            if (element && element.style) {
                element.style.opacity = '0.6';
                element.style.transform = 'scale(0.92)';
                element.style.background = 'rgba(180, 180, 180, 0.3)';
                element.style.borderColor = 'rgba(100, 100, 100, 0.25)';
                element.style.color = 'var(--mud-palette-text-primary, #333)';
            }
            animationTimeout = setTimeout(() => {
                if (element && element.style) {
                    element.style.opacity = '';
                    element.style.transform = '';
                    element.style.background = '';
                    element.style.borderColor = '';
                    element.style.color = '';
                }
                animationTimeout = null;
            }, 200);
        }
    }

    window.addEventListener('keydown', onKeyDown);

    // Cleanup
    return function() {
        window.removeEventListener('keydown', onKeyDown);
        if (animationTimeout) {
            clearTimeout(animationTimeout);
            animationTimeout = null;
        }
        if (element && element.style) {
            element.style.opacity = '';
            element.style.transform = '';
            element.style.background = '';
            element.style.borderColor = '';
            element.style.color = '';
        }
    };
};

// Safe platform detection function for Blazor interop
window.isMacPlatform = function() {
    return navigator.userAgent.indexOf('Mac') !== -1 || 
           (navigator.platform && navigator.platform.indexOf('Mac') !== -1);
};

// Window resize handler for MainLayout
let mainLayoutResizeHandler = null;

window.registerResizeHandler = function(dotNetObjectReference) {
    // Clean up existing handler if any
    if (mainLayoutResizeHandler) {
        window.removeEventListener('resize', mainLayoutResizeHandler);
    }
    
    // Create new handler
    mainLayoutResizeHandler = function() {
        const width = window.innerWidth;
        dotNetObjectReference.invokeMethodAsync('OnWindowResized', width);
    };
    
    // Register the handler
    window.addEventListener('resize', mainLayoutResizeHandler);
};

window.unregisterResizeHandler = function() {
    if (mainLayoutResizeHandler) {
        window.removeEventListener('resize', mainLayoutResizeHandler);
        mainLayoutResizeHandler = null;
    }
};