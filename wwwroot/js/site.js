// ===== SCROLL TO TOP BUTTON =====
$(document).ready(function () {
    // Create scroll to top button
    if ($('#scrollToTop').length === 0) {
        $('body').append('<button id="scrollToTop"><i class="bi bi-arrow-up"></i></button>');
    }

    // Show/hide scroll to top button
    $(window).scroll(function () {
        if ($(this).scrollTop() > 300) {
            $('#scrollToTop').addClass('show');
        } else {
            $('#scrollToTop').removeClass('show');
        }
    });

    // Scroll to top on click
    $('#scrollToTop').click(function () {
        $('html, body').animate({ scrollTop: 0 }, 800);
        return false;
    });

    // ===== SMOOTH SCROLL FOR ANCHOR LINKS =====
    $('a[href*="#"]:not([href="#"])').click(function () {
        if (location.pathname.replace(/^\//, '') == this.pathname.replace(/^\//, '') && location.hostname == this.hostname) {
            var target = $(this.hash);
            target = target.length ? target : $('[name=' + this.hash.slice(1) + ']');
            if (target.length) {
                $('html, body').animate({
                    scrollTop: target.offset().top - 70
                }, 1000);
                return false;
            }
        }
    });

    // ===== AUTO HIDE ALERTS =====
    $('.alert:not(.alert-permanent)').each(function () {
        var alert = $(this);
        setTimeout(function () {
            alert.fadeOut('slow', function () {
                alert.remove();
            });
        }, 5000);
    });

    // ===== FORM VALIDATION FEEDBACK =====
    $('form').on('submit', function () {
        var form = $(this);
        if (form[0].checkValidity() === false) {
            event.preventDefault();
            event.stopPropagation();
        }
        form.addClass('was-validated');
    });

    // ===== LOADING OVERLAY =====
    window.showLoading = function () {
        if ($('.spinner-overlay').length === 0) {
            $('body').append('<div class="spinner-overlay"><div class="spinner-border-custom"></div></div>');
        }
        $('.spinner-overlay').fadeIn();
    };

    window.hideLoading = function () {
        $('.spinner-overlay').fadeOut();
    };

    // ===== CONFIRM DIALOG =====
    $('[data-confirm]').click(function (e) {
        var message = $(this).data('confirm');
        if (!confirm(message)) {
            e.preventDefault();
            return false;
        }
    });

    // ===== TOOLTIP INITIALIZATION =====
    var tooltipTriggerList = [].slice.call(document.querySelectorAll('[data-bs-toggle="tooltip"]'));
    var tooltipList = tooltipTriggerList.map(function (tooltipTriggerEl) {
        return new bootstrap.Tooltip(tooltipTriggerEl);
    });

    // ===== POPOVER INITIALIZATION =====
    var popoverTriggerList = [].slice.call(document.querySelectorAll('[data-bs-toggle="popover"]'));
    var popoverList = popoverTriggerList.map(function (popoverTriggerEl) {
        return new bootstrap.Popover(popoverTriggerEl);
    });

    // ===== ANIMATED COUNTERS =====
    $('.stat-card h2').each(function () {
        var $this = $(this);
        var countTo = parseInt($this.text().replace(/,/g, ''));

        if (!isNaN(countTo) && countTo > 0) {
            $({ countNum: 0 }).animate({
                countNum: countTo
            }, {
                duration: 2000,
                easing: 'swing',
                step: function () {
                    $this.text(Math.floor(this.countNum).toLocaleString('vi-VN'));
                },
                complete: function () {
                    $this.text(this.countNum.toLocaleString('vi-VN'));
                }
            });
        }
    });

    // ===== ANIMATE ON SCROLL =====
    function animateOnScroll() {
        $('.card, .stat-card').each(function () {
            var elementTop = $(this).offset().top;
            var elementBottom = elementTop + $(this).outerHeight();
            var viewportTop = $(window).scrollTop();
            var viewportBottom = viewportTop + $(window).height();

            if (elementBottom > viewportTop && elementTop < viewportBottom) {
                $(this).css({
                    'opacity': '1',
                    'transform': 'translateY(0)'
                });
            }
        });
    }

    // Initial animation
    $('.card, .stat-card').css({
        'opacity': '0',
        'transform': 'translateY(30px)',
        'transition': 'all 0.6s ease'
    });

    $(window).on('scroll resize', animateOnScroll);
    animateOnScroll(); // Run on page load

    // ===== IMAGE LAZY LOADING =====
    if ('loading' in HTMLImageElement.prototype) {
        const images = document.querySelectorAll('img[loading="lazy"]');
        images.forEach(img => {
            img.src = img.dataset.src;
        });
    }

    // ===== CARD HOVER EFFECT =====
    $('.hover-card').hover(
        function () {
            $(this).find('.card-img-top').css('transform', 'scale(1.1)');
        },
        function () {
            $(this).find('.card-img-top').css('transform', 'scale(1)');
        }
    );

    $('.hover-card .card-img-top').css('transition', 'transform 0.3s ease');
});

// ===== NUMBER FORMAT =====
function formatCurrency(number) {
    return new Intl.NumberFormat('vi-VN', {
        style: 'currency',
        currency: 'VND'
    }).format(number);
}

// ===== DATE FORMAT =====
function formatDate(date) {
    return new Date(date).toLocaleDateString('vi-VN', {
        year: 'numeric',
        month: '2-digit',
        day: '2-digit'
    });
}

// ===== COPY TO CLIPBOARD =====
function copyToClipboard(text) {
    navigator.clipboard.writeText(text).then(function () {
        alert('Đã sao chép: ' + text);
    }, function (err) {
        console.error('Could not copy text: ', err);
    });
}
// ===== TOAST NOTIFICATION =====
window.showToast = function (message, type = 'success') {
    var toastEl = document.getElementById('liveToast');
    var toastHeader = toastEl.querySelector('.toast-header');
    var toastBody = toastEl.querySelector('.toast-body');

    // Set color based on type
    toastHeader.className = 'toast-header';
    var icon = 'bi-check-circle';

    switch (type) {
        case 'success':
            toastHeader.classList.add('bg-success', 'text-white');
            icon = 'bi-check-circle';
            break;
        case 'error':
            toastHeader.classList.add('bg-danger', 'text-white');
            icon = 'bi-x-circle';
            break;
        case 'warning':
            toastHeader.classList.add('bg-warning', 'text-dark');
            icon = 'bi-exclamation-triangle';
            break;
        case 'info':
            toastHeader.classList.add('bg-info', 'text-white');
            icon = 'bi-info-circle';
            break;
    }

    toastHeader.querySelector('i').className = `bi ${icon} me-2`;
    toastBody.textContent = message;

    var toast = new bootstrap.Toast(toastEl);
    toast.show();
};

// Show toast from TempData
$(document).ready(function () {
    @if (TempData["Success"] != null) {
        <text>
            showToast('@TempData["Success"]', 'success');
        </text>
    }

    @if (TempData["Error"] != null) {
        <text>
            showToast('@TempData["Error"]', 'error');
        </text>
    }
});