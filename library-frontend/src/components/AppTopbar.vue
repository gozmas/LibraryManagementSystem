<template>
  <header class="topbar">
    <div class="brand" @click="goHome">
      <div class="logo">📚</div>
      <span>LibraryMS</span>
    </div>

    <nav>
      <button class="nav-btn active" @click="goHome">
        <span>🏠</span>
        Home
      </button>

      <template v-if="isAdmin">
        <button class="nav-btn" @click="goReports">
          <span>📊</span>
          Reports
        </button>

        <button class="nav-btn admin-btn" @click="goAdmin">
          <span>🛡️</span>
          Admin
        </button>

        <button class="logout-btn" @click="logout">
          <span>↪</span>
          Logout
        </button>
      </template>

      <template v-else-if="isMember">
        <button class="nav-btn" @click="goMyLoans">
          <span>📚</span>
          My Loans
        </button>

        <button class="nav-btn" @click="goMyWishlist">
          <span>❤️</span>
          My Wishlist
        </button>

        <button class="nav-btn" @click="goMyFines">
          <span>💰</span>
          My Fines
        </button>

        <button class="nav-btn" @click="goProfile">
          <span>👤</span>
          Profile
        </button>

        <button class="logout-btn" @click="logout">
          <span>↪</span>
          Logout
        </button>
      </template>

      <template v-else>
        <button class="nav-btn" @click="goLogin">
          <span>🔐</span>
          Login
        </button>

        <button class="register-btn" @click="goRegister">
          <span>✨</span>
          Register
        </button>
      </template>
    </nav>

    <div v-if="toasts.length" class="toast-stack">
      <div v-for="toast in toasts" :key="toast.id" class="toast">
        <span class="toast-icon">📗</span>

        <div class="toast-body">
          <p class="toast-title">{{ toast.bookTitle }} is available!</p>
          <p class="toast-sub">A copy just came back — grab it before it's gone.</p>
        </div>

        <button class="toast-action" @click="goToWishlistFromToast(toast.id)">
          View
        </button>

        <button class="toast-close" @click="dismissToast(toast.id)">
          ✕
        </button>
      </div>
    </div>
  </header>
</template>

<script setup>
import { computed, onBeforeUnmount, onMounted, ref } from "vue";
import { useRouter } from "vue-router";
import * as signalR from "@microsoft/signalr";

const router = useRouter();

const role = localStorage.getItem("role");

const isAdmin = computed(() => role === "Admin");
const isMember = computed(() => role === "Member" || role === "Student");

const goHome = () => router.push("/home");
const goLogin = () => router.push("/login");
const goRegister = () => router.push("/register");
const goMyLoans = () => router.push("/my-loans");
const goMyWishlist = () => router.push("/my-wishlist");
const goMyFines = () => router.push("/my-fines");
const goProfile = () => router.push("/profile");
const goReports = () => router.push("/reports");
const goAdmin = () => router.push("/admin");

const logout = () => {
  localStorage.removeItem("token");
  localStorage.removeItem("role");
  localStorage.removeItem("email");
  localStorage.removeItem("username");

  router.push("/login");
};

// ---------- Wishlist canlı bildirimleri ----------
// Sadece member/student için, sayfa açıkken bu hub bağlantısı kuruluyor.
// Backend, bir kitap tekrar müsait olduğunda LoanService.ReturnBookAsync
// içinden Clients.User(...) ile sadece o kitabı wishlist'inde bulunduran
// kullanıcıya "WishlistBookAvailable" event'i gönderiyor.
const API_BASE_URL = "http://localhost:5239";
const toasts = ref([]);
let toastCounter = 0;
let connection = null;

const dismissToast = (id) => {
  toasts.value = toasts.value.filter((toast) => toast.id !== id);
};

const goToWishlistFromToast = (id) => {
  dismissToast(id);
  router.push("/my-wishlist");
};

const startWishlistNotifications = async () => {
  const token = localStorage.getItem("token");

  if (!token || !isMember.value) return;

  connection = new signalR.HubConnectionBuilder()
    .withUrl(`${API_BASE_URL}/hubs/loan`, {
      accessTokenFactory: () => token,
    })
    .withAutomaticReconnect()
    .build();

  connection.on("WishlistBookAvailable", (data) => {
    toastCounter += 1;
    const id = toastCounter;

    toasts.value.push({ id, bookTitle: data.bookTitle });

    setTimeout(() => dismissToast(id), 8000);
  });

  try {
    await connection.start();
  } catch (error) {
    console.error("Wishlist notification connection failed:", error);
  }
};

onMounted(startWishlistNotifications);

onBeforeUnmount(() => {
  if (connection) {
    connection.stop();
  }
});
</script>

<style scoped>
.topbar {
  min-height: 82px;
  padding: 14px 26px;
  margin-bottom: 24px;
  border-radius: 24px;
  background: rgba(255, 255, 255, 0.96);
  box-shadow: 0 14px 38px rgba(15, 23, 42, 0.09);
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: 20px;
}

.brand {
  display: flex;
  align-items: center;
  gap: 14px;
  font-size: 25px;
  font-weight: 900;
  color: #0f172a;
  cursor: pointer;
  white-space: nowrap;
}

.logo {
  width: 52px;
  height: 52px;
  border-radius: 16px;
  background: #111;
  display: grid;
  place-items: center;
  font-size: 25px;
}

nav {
  display: flex;
  align-items: center;
  gap: 12px;
  flex-wrap: wrap;
  justify-content: flex-end;
}

button {
  border: none;
  cursor: pointer;
  font-family: inherit;
}

.nav-btn,
.register-btn,
.logout-btn {
  min-height: 48px;
  padding: 0 18px;
  border-radius: 15px;
  font-size: 15px;
  font-weight: 850;
  display: flex;
  align-items: center;
  gap: 8px;
  transition: 0.18s ease;
}

.nav-btn {
  background: #f8fafc;
  color: #334155;
  border: 1px solid #e2e8f0;
}

.nav-btn:hover {
  background: #ecfdf5;
  color: #166534;
  transform: translateY(-1px);
}

.nav-btn.active {
  background: #111;
  color: white;
}

.admin-btn {
  background: #ecfdf5;
  color: #166534;
  border: 1px solid #86efac;
}

.register-btn {
  background: #166534;
  color: white;
}

.logout-btn {
  background: #dc2626;
  color: white;
  box-shadow: 0 10px 22px rgba(220, 38, 38, 0.18);
}

.logout-btn:hover,
.register-btn:hover {
  transform: translateY(-1px);
  opacity: 0.92;
}

@media (max-width: 760px) {
  .topbar {
    flex-direction: column;
    align-items: flex-start;
  }

  nav {
    justify-content: flex-start;
  }
}

/* ---------- Wishlist toast bildirimleri ---------- */

.toast-stack {
  position: fixed;
  top: 24px;
  right: 24px;
  z-index: 999;
  display: flex;
  flex-direction: column;
  gap: 12px;
  max-width: 360px;
}

.toast {
  display: flex;
  align-items: flex-start;
  gap: 12px;
  padding: 16px 16px;
  border-radius: 18px;
  background: white;
  border: 1px solid #bbf7d0;
  box-shadow: 0 18px 40px rgba(15, 23, 42, 0.15);
  animation: toast-in 0.25s ease;
}

@keyframes toast-in {
  from {
    opacity: 0;
    transform: translateY(-8px);
  }
  to {
    opacity: 1;
    transform: translateY(0);
  }
}

.toast-icon {
  font-size: 20px;
  line-height: 1;
  margin-top: 2px;
}

.toast-body {
  flex: 1;
  min-width: 0;
}

.toast-title {
  margin: 0;
  color: #0f172a;
  font-size: 14px;
  font-weight: 800;
}

.toast-sub {
  margin: 4px 0 0;
  color: #64748b;
  font-size: 12.5px;
  font-weight: 600;
  line-height: 1.4;
}

.toast-action {
  flex-shrink: 0;
  height: 30px;
  padding: 0 12px;
  border: none;
  border-radius: 10px;
  background: #166534;
  color: white;
  font-size: 12px;
  font-weight: 800;
  cursor: pointer;
}

.toast-close {
  flex-shrink: 0;
  border: none;
  background: transparent;
  color: #94a3b8;
  font-size: 13px;
  cursor: pointer;
  padding: 2px;
}

@media (max-width: 480px) {
  .toast-stack {
    left: 16px;
    right: 16px;
    top: 16px;
    max-width: none;
  }
}
</style>