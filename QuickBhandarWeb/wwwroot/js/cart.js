document.addEventListener("DOMContentLoaded", () => {
    const productJsonPath = "/data/products.json"; // JSON file path

    // =====================
    // CART FUNCTIONS
    // =====================
    function getCart() {
        return JSON.parse(localStorage.getItem("cart")) || [];
    }

    function saveCart(cart) {
        localStorage.setItem("cart", JSON.stringify(cart));
    }

    function addToCart(productId) {
        let cart = getCart();
        cart.push(productId); // Allow multiple quantities
        saveCart(cart);
        updateCartDisplay();
    }

    window.removeFromCart = function (id) {
        let cart = getCart();
        const index = cart.indexOf(id);
        if (index !== -1) {
            cart.splice(index, 1); // Remove only one item (not all)
        }
        saveCart(cart);
        updateCartDisplay();
    };

    function updateCartDisplay() {
        const cart = getCart();
        const cartBody = document.getElementById("cartBody");

        if (!cartBody) return; // Skip if cart UI is not present

        if (cart.length === 0) {
            cartBody.innerHTML = `
                <div class="alert alert-info mt-5 text-center">
                    <h5>Your cart is empty 😔</h5>
                    <p>Start browsing and add some products to your cart!</p>
                    <a href="/" class="btn btn-primary">Continue Shopping</a>
                </div>
            `;
            document.querySelector(".cart-total").textContent = "₹0";
            document.querySelector(".badge.bg-primary").textContent = "0";
            return;
        }

        fetch(productJsonPath)
            .then(res => res.json())
            .then(products => {
                const cartItems = cart.map(id => products.find(p => p.Id == id)).filter(Boolean);

                let total = 0;
                let listItems = "";
                cartItems.forEach(item => {
                    total += item.Price;
                    listItems += `
                        <li class="list-group-item d-flex justify-content-between lh-sm">
                            <div>
                                <h6 class="my-0">${item.Name}</h6>
                                <small class="text-body-secondary">${item.Description}</small>
                            </div>
                            <div class="d-flex align-items-center gap-2">
                                <span class="text-body-secondary">₹${item.Price}</span>
                                <button class="btn btn-sm btn-danger" onclick="removeFromCart(${item.Id})">
                                    <i class="bi bi-x-lg"></i>
                                </button>
                            </div>
                        </li>
                    `;
                });

                listItems += `
                    <li class="list-group-item d-flex justify-content-between">
                        <span>Total</span>
                        <strong>₹${total}</strong>
                    </li>
                `;

                cartBody.innerHTML = `
                    <div class="order-md-last">
                        <h4 class="d-flex justify-content-between align-items-center mb-3">
                            <span class="text-primary">Your cart</span>
                            <span class="badge bg-primary rounded-pill">${cart.length}</span>
                        </h4>
                        <ul class="list-group mb-3">${listItems}</ul>
                        <button class="w-100 btn btn-primary btn-lg mt-3" type="button" id="checkoutBtn">Continue to checkout</button>
                    </div>
                `;

                document.querySelector(".cart-total").textContent = `₹${total}`;
            });
    }

    // =====================
    // ADD TO CART (BUTTON HANDLER)
    // =====================
    document.body.addEventListener("click", function (e) {
        const target = e.target.closest(".add-to-cart-btn");
        if (target) {
            e.preventDefault();
            const id = parseInt(target.dataset.id);
            addToCart(id);
        }

        if (e.target.id === "checkoutBtn") {
            window.location.href = "/Home/Checkout"; // Checkout Page Redirect
        }
    });

    // =====================
    // WISHLIST FUNCTIONS
    // =====================
    let wishlist = JSON.parse(localStorage.getItem('wishlist')) || [];
    const wishlistButtons = document.querySelectorAll('.btn-wishlist');

    wishlistButtons.forEach(button => {
        const id = parseInt(button.dataset.id);

        // Pre-fill heart if already in wishlist
        if (wishlist.includes(id)) {
            button.classList.add('active');
        }

        button.addEventListener('click', e => {
            e.preventDefault();

            if (wishlist.includes(id)) {
                wishlist = wishlist.filter(pid => pid !== id);
                button.classList.remove('active');
            } else {
                wishlist.push(id);
                button.classList.add('active');
            }

            localStorage.setItem('wishlist', JSON.stringify(wishlist));
        });
    });

    // Wishlist Icon Click → Navigate to Wishlist Page
    const heartIcon = document.getElementById('heartIcon');
    if (heartIcon) {
        heartIcon.addEventListener('click', () => {
            const wishlist = JSON.parse(localStorage.getItem('wishlist') || '[]');
            const url = `/Home/Wishlist?ids=${wishlist.join(',')}`;
            window.location.href = url;
        });
    }
        const placeOrderBtn = document.getElementById("placeOrderBtn");

        if (placeOrderBtn) {
            placeOrderBtn.addEventListener("click", function () {
                // ✅ Validate form before placing order
                const form = document.getElementById("checkoutForm");
                if (!form.checkValidity()) {
                    form.reportValidity();
                    return;
                }

                // ✅ Collect form data
                const orderData = {
                    firstName: form.querySelector('input[placeholder="First Name"]')?.value || '',
                    lastName: form.querySelector('input[placeholder="Last Name"]')?.value || '',
                    email: form.querySelector('input[type="email"]')?.value || '',
                    phone: form.querySelector('input[type="text"]')?.value || '',
                    address: form.querySelector('input[placeholder="Street address"]')?.value || '',
                    city: form.querySelector('input[placeholder="City"]')?.value || '',
                    zip: form.querySelector('input[placeholder="Zip Code"]')?.value || '',
                    payment: document.querySelector('input[name="payment"]:checked')?.id || 'cod',
                    cart: JSON.parse(localStorage.getItem("cart")) || []
                };

                // ✅ Save order in localStorage or send to API
                localStorage.setItem("lastOrder", JSON.stringify(orderData));

                // ✅ Clear cart after placing order
                localStorage.removeItem("cart");

                // ✅ Redirect to Order Success / Orders Page
                window.location.href = "/Home/Orders"; // Change route as per your MVC setup
            });
        }

    // =====================
    // INITIALIZE CART DISPLAY
    // =====================
    updateCartDisplay();
});

// =====================
// ORDER SUMMARY (Checkout Page)
// =====================
window.loadOrderSummary = async function () {
    const productJsonPath = "/data/products.json";
    const cart = JSON.parse(localStorage.getItem("cart")) || [];
    const list = document.getElementById("orderItems"); // Changed to match your view

    if (!list) return; // If this page doesn't have summary section

    if (cart.length === 0) {
        list.innerHTML = `<li class="list-group-item">No items in your order.</li>`;
        return;
    }

    const res = await fetch(productJsonPath);
    const products = await res.json();
    const cartItems = cart.map(id => products.find(p => p.Id == id)).filter(Boolean);

    let total = 0;
    let html = "";

    cartItems.forEach(item => {
        total += item.Price;
        html += `
            <li class="list-group-item d-flex justify-content-between">
                <span>${item.Name}</span>
                <strong>₹${item.Price}</strong>
            </li>
        `;
    });

    html += `
        <li class="list-group-item d-flex justify-content-between fw-bold">
            <span>Total</span>
            <strong>₹${total}</strong>
        </li>
    `;

    list.innerHTML = html;
};

window.loadOrders = async function () {
    const productJsonPath = "/data/products.json";
    const lastOrder = JSON.parse(localStorage.getItem("lastOrder")) || null;
    const cart = lastOrder && lastOrder.cart ? lastOrder.cart : [];
    const list = document.getElementById("orderSummaryList"); // Match your HTML ID

    if (!list) return; // If this page doesn't have order summary section, exit

    if (cart.length === 0) {
        list.innerHTML = `<li class="list-group-item text-center">No items in your order.</li>`;
        return;
    }

    try {
        const res = await fetch(productJsonPath);
        const products = await res.json();

        const cartItems = cart.map(id => products.find(p => p.Id == id)).filter(Boolean);

        let total = 0;
        let html = "";

        cartItems.forEach(item => {
            total += item.Price;
            html += `
                <li class="list-group-item d-flex justify-content-between">
                    <span>${item.Name}</span>
                    <strong>₹${item.Price}</strong>
                </li>
            `;
        });

        html += `
            <li class="list-group-item d-flex justify-content-between fw-bold">
                <span>Total</span>
                <strong>₹${total}</strong>
            </li>
        `;

        list.innerHTML = html;

      

    } catch (error) {
        console.error("Error loading orders:", error);
        list.innerHTML = `<li class="list-group-item text-danger">Failed to load orders.</li>`;
    }
};