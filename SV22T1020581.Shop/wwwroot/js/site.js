// TechZone Shop — Global JavaScript Utilities
// =============================================

// ── Formatters ──────────────────────────────────────────────
function fmt(n) {
    if (n === null || n === undefined || n === '') return '—';
    return new Intl.NumberFormat('vi-VN', { style: 'currency', currency: 'VND', maximumFractionDigits: 0 }).format(n);
}

// ── Toast Notifications ──────────────────────────────────────
function toast(msg, type = '') {
    const c = document.getElementById('toastContainer');
    if (!c) return;
    const t = document.createElement('div');
    t.className = 'toast' + (type ? ' ' + type : '');
    t.textContent = msg;
    c.appendChild(t);
    // Trigger reflow
    void t.offsetWidth;
    t.classList.add('show');
    setTimeout(() => {
        if (t.parentNode) {
            t.classList.remove('show');
            setTimeout(() => { if (t.parentNode) t.remove(); }, 350);
        }
    }, 3000);
}

// ── Page Navigation ──────────────────────────────────────────
function showPage(page) {
    const pages = {
        home: '/', catalog: '/Product', cart: '/Cart',
        orders: '/Order/MyOrders', login: '/Account/Login',
        register: '/Account/Register', profile: '/Account/Profile'
    };
    if (pages[page]) { window.location.href = pages[page]; }
}

// ── Star Rating ──────────────────────────────────────────────
function renderStars(rating) {
    const full = Math.floor(rating || 0);
    const half = (rating % 1) >= 0.5;
    const empty = 5 - full - (half ? 1 : 0);
    return '★'.repeat(full) + (half ? '½' : '') + '☆'.repeat(empty) +
        '<span style="color:var(--grey);margin-left:4px;">(' + (rating || '') + ')</span>';
}

// ── Cart Actions ────────────────────────────────────────────
function addToCart(productId, qty = 1) {
    qty = parseInt(qty) || 1;
    fetch('/Product/AddToCart', {
        method: 'POST',
        headers: {
            'Content-Type': 'application/x-www-form-urlencoded',
            'RequestVerificationToken': getCSRFToken()
        },
        body: `productID=${productId}&quantity=${qty}`
    })
    .then(r => r.json())
    .then(res => {
        if (res.success) {
            updateCartBadge(res.totalQuantity);
            toast(res.message || 'Đã thêm vào giỏ hàng!', 'success');
        } else {
            toast(res.message || 'Có lỗi xảy ra', 'error');
        }
    })
    .catch(() => toast('Có lỗi xảy ra. Vui lòng thử lại.', 'error'));
}

function updateCartBadge(qty) {
    const badge = document.getElementById('cartBadge');
    if (!badge) return;
    qty = parseInt(qty) || 0;
    badge.textContent = qty;
    badge.style.display = qty > 0 ? 'flex' : 'none';
}

function getCSRFToken() {
    const input = document.querySelector('input[name="__RequestVerificationToken"]');
    return input ? input.value : '';
}

// ── Product Quantity (Detail page) ──────────────────────────
function changeQty(delta) {
    const input = document.getElementById('detailQtyInput');
    if (!input) return;
    let val = parseInt(input.value) || 1;
    val = Math.max(1, val + delta);
    input.value = val;
}

function addToCartFromDetail() {
    const input = document.getElementById('detailQtyInput');
    const productId = document.getElementById('productIdValue');
    const qty = parseInt(input ? input.value : 1) || 1;
    const pid = parseInt(productId ? productId.value : 0);
    if (pid > 0) addToCart(pid, qty);
}

function buyNow() {
    const input = document.getElementById('detailQtyInput');
    const productId = document.getElementById('productIdValue');
    const qty = parseInt(input ? input.value : 1) || 1;
    const pid = parseInt(productId ? productId.value : 0);
    if (pid > 0) {
        addToCart(pid, qty);
        setTimeout(() => { window.location.href = '/Cart'; }, 500);
    }
}

// ── Init ──────────────────────────────────────────────────
document.addEventListener('DOMContentLoaded', function () {
    // Update cart badge from server
    fetch('/Cart/GetCartCount')
        .then(r => r.json())
        .then(res => { if (res.count >= 0) updateCartBadge(res.count); })
        .catch(() => {});

    // Auto-dismiss alerts after 5s
    setTimeout(() => {
        document.querySelectorAll('.alert[data-auto-dismiss]').forEach(el => {
            el.style.opacity = '0';
            el.style.transition = 'opacity 0.3s';
            setTimeout(() => { if (el.parentNode) el.remove(); }, 350);
        });
    }, 5000);
});
