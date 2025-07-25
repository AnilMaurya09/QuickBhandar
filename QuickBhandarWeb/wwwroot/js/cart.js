document.addEventListener("DOMContentLoaded", () => {
    function addToCart(id, qty) { 
            // ✅ Call backend
        fetch('/Home/AddToCart', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json'
                },
                body: JSON.stringify({ productId: id, quantity: qty })
            })
                .then(res => res.json())
                .then(data => {
                    if (data.success) updateCartDisplay(); // Refresh cart UI
                    else 
                        document.getElementById("loginPopup").style.display = "block"; // Show login popup
                });
        }


    window.removeFromCart = function (productId) {
      
            // Remove from DB using Fetch API
            fetch('/Home/RemoveToCart', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json'
                },
                body: JSON.stringify({ productId: productId })
            })
                .then(response => response.json())
                .then(data => {
                    if (data.success) {
                        updateCartDisplay(); // Refresh cart UI
                    } else {
                        document.getElementById("loginPopup").style.display = "block"; // Show login popup
                    }
                })
                .catch(error => {
                    console.error('Error:', error);
                });
    };


   

    // =====================
    // ADD TO CART (BUTTON HANDLER)
    // =====================
    document.body.addEventListener("click", function (e) {
        const target = e.target.closest(".add-to-cart-btn");
        if (target) {
            e.preventDefault();
            const id = parseInt(target.dataset.id);
            const qtyInput = target.closest(".product-item").querySelector("input.quantity");
            const qty = parseInt(qtyInput?.value || 1);
            addToCart(id, qty);
            updateCartDisplay();
        }

        if (e.target.id === "checkoutBtn") {
            e.preventDefault();
            window.location.href = "/Home/Checkout";
        }
    });

    window.toggleWishlist = function (Id, btnElement) {
        fetch(`/Home/ToggleWishlist?productId=${Id}`, {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' }
        })
                .then(res => res.json())
                .then(data => {
                    if (data.success) {
                        if (data.added) {
                            btnElement.classList.add('active'); // Mark as added
                        } else {
                            btnElement.classList.remove('active'); // Removed
                        }
                    } else {
                        document.getElementById("loginPopup").style.display = "block"; // Show login popup
                    }
                });
    };

        const placeOrderBtn = document.getElementById("placeOrderBtn");
    if (placeOrderBtn) {
        placeOrderBtn.addEventListener("click", function () {
            const form = document.getElementById("checkoutForm");
            if (!form.checkValidity()) {
                form.reportValidity();
                return;
            }

            // Collect form data
            const orderData = {
                FirstName: form.querySelector('[name="FirstName"]')?.value,
                LastName: form.querySelector('[name="LastName"]')?.value,
                Email: form.querySelector('[name="Email"]')?.value,
                Phone: form.querySelector('[name="Phone"]')?.value,
                Address: form.querySelector('[name="Address"]')?.value,
                City: form.querySelector('[name="City"]')?.value,
                Zip: form.querySelector('[name="Zip"]')?.value,
                PaymentMethod: document.querySelector('input[name="Payment"]:checked')?.value,
            };
           
            fetch("/Home/PlaceOrder", {
                method: "POST",
                headers: { "Content-Type": "application/json" },
                body: JSON.stringify(orderData)
            })
                .then(res => res.json())
                .then(data => {

                    if (data.success) {
                        window.location.href = "/Home/OrderSuccess";
                    } else if (data.redirectToLogin) {
                        window.location.href = "/Home/Login";
                    } else {
                        alert(data.message || "Something went wrong!");
                    }
                })
                .catch(err => {
                    console.error(err);
                });
        });
    }

    // =====================
    // INITIALIZE CART DISPLAY
    // =====================
    updateCartDisplay();
});
let pendingCheckout = false;
function sendOTP() {
    const mobile = document.getElementById("mobileInput").value;
    if (mobile.length === 10) {
        fetch('/Auth/SendOtp', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json'
            },
            body: JSON.stringify({ mobileNumber: mobile })
        })
            .then(res => res.json())
            .then(data => {
                if (data.success) {
                    document.getElementById("otpSection").style.display = "block";
                } else {
                    alert(data.message);
                }
            });
    } else {
        alert("Enter valid 10-digit number");
    }
}

function verifyOTP() {
    const mobile = document.getElementById("mobileInput").value;
    const otp = document.getElementById("otpInput").value;

    fetch('/Auth/VerifyOtp', {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json'
        },
        body: JSON.stringify({ mobileNumber: mobile, otp: otp })
    })
        .then(res => res.json())
        .then(data => {
            if (data.success) {
                if (data.role === "Admin") {
                    // ✅ Redirect Admin
                    window.location.href = data.redirectUrl; // Comes from backend
                } else {
                    // ✅ For User - Apply UI changes
                    document.getElementById("loginPopup").style.display = "none";
                    document.getElementById("userLi").style.display = "none";
                    document.getElementById("userDropdown").style.display = "block";

                    // Enable icons
                    document.getElementById("heartIcon")?.classList.remove("disabled-icon");
                    document.getElementById("cartSection")?.classList.remove("disabled-icon");
                    updateCartDisplay();
                    if (pendingCheckout) {
                        pendingCheckout = false; // Reset flag
                        window.location.href = "/Home/Checkout"; // Redirect to checkout
                    }
                }
            } else {
                alert(data.message || "Invalid OTP");
            }
        });
}

function toggleDropdown() {
    const dropdown = document.getElementById("userDropdown");
    dropdown.classList.toggle("show");
}
function logout() {
    fetch('/Auth/Logout', {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json'
        }
    })
        .then(response => {
            if (response.redirected) {
                window.location.href = response.url;
            }
        })
        .catch(err => console.error(err));
}

document.getElementById("loginIcon").addEventListener("click", function () {
    document.getElementById("loginPopup").style.display = "block";
});
function updateCartDisplay() {
    const cartBody = document.getElementById("cartBody");
    if (!cartBody) return;

    // ✅ Call API to fetch cart items from DB
    fetch('/Home/GetCartItems')
        .then(res => res.json())
        .then(data => {
            if (!data.success || data.items.length === 0) {
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

            let total = 0;
            let listItems = "";

            data.items.forEach(item => {
                total += item.price * item.quantity;
                listItems += `
                    <li class="list-group-item d-flex justify-content-between align-items-center">
                        <div>
                            <h6 class="my-0">${item.name}</h6>
                            <small class="text-muted">${item.description}</small><br>
                            <small class="text-muted">Qty: ${item.quantity}</small>
                        </div>
                        <div class="d-flex align-items-center gap-2">
                            <span class="fw-bold">₹${item.price}</span>
                            <button class="btn btn-sm btn-danger" onclick="removeFromCart(${item.id})">
                                <i class="bi bi-x-lg"></i>
                            </button>
                        </div>
                    </li>
                `;
            });

            listItems += `
                <li class="list-group-item d-flex justify-content-between fw-bold">
                    <span>Total</span>
                    <strong>₹${total}</strong>
                </li>
            `;

            cartBody.innerHTML = `
                <div class="order-md-last">
                    <h4 class="d-flex justify-content-between align-items-center mb-3">
                        <span class="text-primary">Your cart</span>
                        <span class="badge bg-primary rounded-pill">${data.items.length}</span>
                    </h4>
                    <ul class="list-group mb-3">${listItems}</ul>
                    <button class="w-100 btn btn-success btn-lg mt-3" type="button" id="checkoutBtn">
                        Proceed to Checkout
                    </button>
                </div>
            `;

            document.querySelector(".cart-total").textContent = `₹${total}`;
        })
        .catch(() => {
            cartBody.innerHTML = `<div class="alert alert-danger text-center">Failed to load cart.</div>`;
        });
}

// Close dropdown if clicked outside
window.addEventListener("click", function (e) {
    const dropdown = document.getElementById("userDropdown");
    if (!dropdown.contains(e.target)) {
        dropdown.classList.remove("show");
    }
});
function closePopup() {
    document.getElementById("loginPopup").style.display = "none";
}

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