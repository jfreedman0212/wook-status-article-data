import Toastify from '/lib/toastify.mjs';
import IMask from '/lib/imask.min.mjs';

// localize any times
window.addEventListener('htmx:load', function (event) {
    const timeComponents = event.detail.elt.querySelectorAll('time:not(.skip-localize-time)');
    timeComponents.forEach(function (time) {
        const date = new Date(time.getAttribute('datetime'));
        const formatter = new Intl.DateTimeFormat('en-US', {
            month: '2-digit',
            day: '2-digit',
            year: 'numeric'
        });
        time.textContent = formatter.format(date);
    });
});

// bind project name field event listeners
window.addEventListener('htmx:load', function (event) {
    const projectNameFields = event.detail.elt.querySelectorAll('.project-name');
    projectNameFields.forEach(function (projectNameField) {
        // listen to Cmd/Ctrl + I to toggle the project type
        projectNameField.addEventListener('keydown', function (kbdEvent) {
            if (!((kbdEvent.ctrlKey || kbdEvent.metaKey) && kbdEvent.code === 'KeyI')) return;

            // prevent it from bubbling up to the window, where it may interfere with 
            // browser-native keystrokes
            kbdEvent.preventDefault();
            kbdEvent.stopPropagation();
            const nameField = projectNameField.querySelector('.name-field');
            const typeField = projectNameField.querySelector('.type-field');
            const newValue = typeField.value === 'Category' ? 'IntellectualProperty' : 'Category';

            if (newValue === 'IntellectualProperty') {
                nameField.classList.add("italic");
            } else {
                nameField.classList.remove("italic");
            }
            typeField.value = newValue;
        });
    });
});

window.addEventListener('htmx:load', function (event) {
    const inputsToMask = event.detail.elt.querySelectorAll('input[type="text"][data-masktype]');
    for (const input of inputsToMask) {
        const maskType = input.dataset.masktype;
        if (!maskType) {
            continue;
        }
        
        switch (maskType.toLowerCase()) {
            case "24-hour-time":
                IMask(input, {
                    mask: "hh:mm",
                    blocks: {
                        hh: {
                            mask: IMask.MaskedRange,
                            from: 0,
                            to: 23,
                            minLength: 2
                        },
                        mm: {
                            mask: IMask.MaskedRange,
                            from: 0,
                            to: 59,
                            minLength: 2
                        }
                    },
                    lazy: true,
                    autofix: true
                });
        }
    }
});

window.addEventListener('htmx:beforeSwap', function (event) {
    event.detail.shouldSwap = event.detail.xhr.status === 200 || event.detail.xhr.status === 400;
});

window.addEventListener('htmx:responseError', function (event) {
    // all of the below code is for error handling, so do nothing for successful requests or validation errors
    if (event.detail.successful || event.detail.xhr.status === 400) return;

    let message = 'Your request couldn\'t be processed. Try again later.';
    let className = 'box bad';

    switch (event.detail.xhr.status) {
        case 401:
            message = 'You must log in to access that.';
            className = 'box warn';
            break;
        case 403:
            message = 'You don\'t have permission to access that.';
            className = 'box warn';
            break;
        case 404:
            message = 'The requested resource does not exist.';
            className = 'box warn';
            break;
    }
    Toastify({
        text: message,
        duration: -1,
        close: true,
        className: className,
        gravity: "top", // `top` or `bottom`
        position: "left", // `left`, `center` or `right`
        stopOnFocus: true, // Prevents dismissing of toast on hover
    }).showToast();
});

window.addEventListener('htmx:sendError', function () {
    Toastify({
        text: 'Your request could not be sent. This can happen if your internet connection isn\'t stable or if the application is down for maintenance. If your connection is stable, try again in a few moments.',
        duration: -1,
        newWindow: true,
        close: true,
        className: 'box bad',
        gravity: "top", // `top` or `bottom`
        position: "left", // `left`, `center` or `right`
        stopOnFocus: true, // Prevents dismissing of toast on hover
    }).showToast();
});