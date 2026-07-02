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
  </header>
</template>

<script setup>
import { computed } from "vue";
import { useRouter } from "vue-router";

const router = useRouter();

const role = localStorage.getItem("role");

const isAdmin = computed(() => role === "Admin");
const isMember = computed(() => role === "Member" || role === "Student");

const goHome = () => router.push("/home");
const goLogin = () => router.push("/login");
const goRegister = () => router.push("/register");
const goMyLoans = () => router.push("/my-loans");
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
</style>