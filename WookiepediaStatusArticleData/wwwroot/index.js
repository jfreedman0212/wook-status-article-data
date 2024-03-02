window.addEventListener('htmx:responseError', function (event) {
    let message = 'Your request couldn\'t be processed. Try again later.';
    let close = false;
    let className = 'box bad';
    
    switch (event.detail.xhr.status) {
        case 401:
            message = 'You must log in to access that.';
            close = true;
            className = 'box warn';
            break;
        case 403:
            message = 'You don\'t have permission to access that.';
            close = true;
            className = 'box warn';
            break;
        case 404:
            message = 'The requested resource does not exist.';
            close = true;
            className = 'box warn';
            break;
    }
    Toastify({
        text: message,
        duration: 3000,
        newWindow: true,
        close: close,
        className: className,
        gravity: "top", // `top` or `bottom`
        position: "left", // `left`, `center` or `right`
        stopOnFocus: true, // Prevents dismissing of toast on hover
    }).showToast();
});