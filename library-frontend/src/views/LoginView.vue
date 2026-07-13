<template>
  <div :class="['login-page', { dark: isDarkMode }]">
    <header class="navbar">
      <div class="brand" @click="goHome">
        <div class="logo">
          <Library :size="24" />
        </div>
        <span>LibraryMS</span>
      </div>

      <nav class="nav-links">
        <button
          class="icon-btn"
          type="button"
          :class="{ active: !isDarkMode }"
          @click="setLightMode"
        >
          <Sun :size="18" />
        </button>
        <button
          class="icon-btn"
          type="button"
          :class="{ active: isDarkMode }"
          @click="setDarkMode"
        >
          <Moon :size="18" />
        </button>
      </nav>
    </header>
    <main class="layout">
      <div class="layout-row">  
      <motion.aside
        class="side-panel"


        :initial="{ opacity: 0, x: -16 }"
        :animate="{ opacity: 1, x: 0 }"
        :transition="{ duration: 0.4 }"
      >
       <BookOpen :size="38" />
        <p class="side-panel-title">For members &amp; guests</p>
        <ul class="side-panel-list">
          <li>Browse and search the catalogue</li>
          <li>Track your active loans</li>
          <li>Save books to your wishlist</li>
        </ul>
      </motion.aside>

      <div class="center-column">
        <motion.div
          class="login-card"
          :initial="{ opacity: 0, y: 24 }"
          :animate="{ opacity: 1, y: 0 }"
          :transition="{ duration: 0.5 }"
        >
          <div class="card-brand">
            <div class="logo">
              <Library :size="22" />
            </div>
            <span>LibraryMS</span>
          </div>

          <p class="card-eyebrow">Welcome back</p>
          <h2>Sign in to your account</h2>
          <p class="subtitle">Pick up right where you left off.</p>

          <p v-if="errorMessage" class="error-message">
            {{ errorMessage }}
          </p>

          <form @submit.prevent="handleLogin">
            <label for="login-email">Email</label>
            <div class="input-box">
              <Mail :size="18" class="input-icon" />
              <input
                id="login-email"
                v-model="email"
                type="email"
                placeholder="name@email.com"
                required
              />
            </div>
            <p v-if="email && !isEmailValid" class="field-hint">
              Please enter a valid email address (e.g. name@email.com).
            </p>

            <label for="login-password">Password</label>
            <div class="input-box">
              <Lock :size="18" class="input-icon" />
              <input
                id="login-password"
                v-model="password"
                :type="showPassword ? 'text' : 'password'"
                placeholder="••••••••"
                required
              />
              <button
                class="eye-btn"
                type="button"
                :aria-label="showPassword ? 'Hide password' : 'Show password'"
                @click="showPassword = !showPassword"
              >
                <component :is="showPassword ? EyeOff : Eye" :size="18" />
              </button>
            </div>

            <motion.button
              class="sign-btn"
              type="submit"
              :disabled="loading"
              :whileHover="{ scale: 1.01 }"
              :whilePress="{ scale: 0.99 }"
            >
              {{ loading ? "Signing in..." : "Sign in" }}
            </motion.button>

            <div class="divider">
              <span></span>
              <p>or</p>
              <span></span>
            </div>

            <motion.button
              class="guest-btn"
              type="button"
              :whileHover="{ scale: 1.01 }"
              :whilePress="{ scale: 0.99 }"
              @click="continueAsGuest"
            >
              <UserRound :size="18" />
              Continue as guest
            </motion.button>

           <p class="signup">
              Don't have an account?
              <button type="button" @click="goSignUp">Sign up</button>
            </p>
          </form>
        </motion.div>
      </div>

      <motion.aside
        class="side-panel"
        :initial="{ opacity: 0, x: 16 }"
        :animate="{ opacity: 1, x: 0 }"
        :transition="{ duration: 0.4 }"
      >
      <ShieldCheck :size="38" />
        <p class="side-panel-title">For administrators</p>
        <ul class="side-panel-list">
          <li>Manage members and their accounts</li>
          <li>Issue and track every loan</li>
          <li>View reports and analytics</li>
        </ul>
      </motion.aside>
      </div>
    </main>
  </div>
</template>

<script setup>
import { ref, computed } from "vue";
import { useRouter } from "vue-router";
import axios from "axios";
import { motion } from "motion-v";
import {
  Library,
  Sun,
  Moon,
  Mail,
  Lock,
  Eye,
  EyeOff,
  UserRound,
  BookOpen,
  ShieldCheck,
} from "@lucide/vue";

const router = useRouter();

const email = ref("");
const password = ref("");
const showPassword = ref(false);
const loading = ref(false);
const errorMessage = ref("");
const isDarkMode = ref(false);

const API_BASE_URL = "http://localhost:5239";

const emailPattern = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;

const isEmailValid = computed(() => emailPattern.test(email.value));

const handleLogin = async () => {
  errorMessage.value = "";

  if (!email.value || !password.value) {
    errorMessage.value = "Please enter email and password.";
    return;
  }

  if (!isEmailValid.value) {
    errorMessage.value = "Please enter a valid email address.";
    return;
  }

  try {
    loading.value = true;

    localStorage.clear();

    const response = await axios.post(`${API_BASE_URL}/api/auth/login`, {
      email: email.value,
      password: password.value,
    });

    const data = response.data.data || response.data;

    const token = data.token;
    const role = data.role;
    const username = data.username || "";
    const userEmail = data.email || email.value;

    if (!token || !role) {
      console.error("Login response:", response.data);
      errorMessage.value = "Login response is missing token or role.";
      return;
    }

    localStorage.setItem("token", token);
    localStorage.setItem("role", role);
    localStorage.setItem("username", username);
    localStorage.setItem("email", userEmail);

    if (role === "Admin") {
      router.push("/admin");
    } else {
      router.push("/home");
    }
  } catch (error) {
    console.error(error);
    errorMessage.value = "Invalid email or password.";
  } finally {
    loading.value = false;
  }
};

const continueAsGuest = () => {
  localStorage.clear();
  router.push("/home");
};

const goSignUp = () => {
  router.push("/register");
};

const goHome = () => {
  router.push("/home");
};

const setLightMode = () => {
  isDarkMode.value = false;
};

const setDarkMode = () => {
  isDarkMode.value = true;
};


</script>

<style scoped>
* {
  box-sizing: border-box;
}

.login-page {
  min-height: 100vh;
  padding: 24px;
  background:
    radial-gradient(circle at 10% 15%, #eef9e8 0%, transparent 28%),
    radial-gradient(circle at 95% 95%, #f8eaf8 0%, transparent 24%),
    #f8faf7;
  font-family: Inter, system-ui, sans-serif;
  color: #0f172a;
}

.navbar {
  height: 74px;
  padding: 0 26px;
  margin-bottom: 20px;
  border-radius: 20px;
  background: white;
  box-shadow: 0 12px 32px rgba(15, 23, 42, 0.08);
  display: flex;
  align-items: center;
  justify-content: space-between;
}

.brand,
.card-brand {
  display: flex;
  align-items: center;
  gap: 12px;
  font-size: 21px;
  font-weight: 800;
  cursor: pointer;
}

.logo {
  width: 42px;
  height: 42px;
  border-radius: 13px;
  background: #111;
  color: white;
  display: grid;
  place-items: center;
  flex-shrink: 0;
}

.nav-links {
  display: flex;
  align-items: center;
  gap: 8px;
  padding: 4px;
  border-radius: 999px;
  background: #f1f5f9;
}

.icon-btn {
  width: 38px;
  height: 38px;
  border-radius: 50%;
  border: none;
  background: transparent;
  color: #64748b;
  display: grid;
  place-items: center;
  cursor: pointer;
  transition: 0.15s ease;
}

.icon-btn.active {
  background: white;
  color: #166534;
  box-shadow: 0 4px 12px rgba(15, 23, 42, 0.1);
}

/* ---------- Layout ---------- */

.layout {
  min-height: calc(100vh - 118px);
  display: flex;
  align-items: center;
  justify-content: center;
}

.layout-row {
  display: flex;
  align-items: stretch;
  justify-content: center;
  gap: 40px;
  width: 100%;
  max-width: 1600px;
}
.center-column {
  flex-shrink: 0;
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
}

/* ---------- Side panels (member/guest + admin) ---------- */

.side-panel {
  flex: 1;
  max-width: 680px;
  padding: 56px 56px;
  border-radius: 22px;
  background: linear-gradient(160deg, #15803d 0%, #22c55e 100%);
  color: white;
  display: flex;
  flex-direction: column;
  justify-content: center;
  box-shadow: 0 30px 70px rgba(21, 128, 61, 0.2);
}

.side-panel svg {
  color: #86efac;
  margin-bottom: 16px;
}

.side-panel-title {
  margin: 0 0 20px;
  font-size: 27px;
  font-weight: 800;
  color: white;
}

.side-panel-list {
  list-style: none;
  margin: 0;
  padding: 0;
  display: grid;
  gap: 13px;
}

.side-panel-list li {
  position: relative;
  padding-left: 20px;
  font-size: 18.5px;
  line-height: 1.65;
  color: rgba(255, 255, 255, 0.85);
}

.side-panel-list li::before {
  content: "";
  position: absolute;
  left: 0;
  top: 7px;
  width: 5px;
  height: 5px;
  border-radius: 50%;
  background: #86efac;
}

/* ---------- Login card ---------- */




.login-card {
  position: relative;
  width: 100%;
  max-width: 580px;
  padding: 60px 56px 52px;
  border-radius: 24px;
  background: white;
  box-shadow: 0 30px 70px rgba(15, 23, 42, 0.1);
  border: 1px solid #f1f5f9;
  overflow: hidden;
}

.login-card::before {
  content: "";
  position: absolute;
  top: 0;
  left: 0;
  right: 0;
  height: 5px;
  background: linear-gradient(90deg, #14532d 0%, #166534 45%, #65a30d 100%);
}

.card-brand {
  justify-content: center;
  margin-bottom: 36px;
}

.card-eyebrow {
  margin: 0 0 6px;
  text-align: center;
  color: #166534;
  font-size: 13px;
  font-weight: 800;
  text-transform: uppercase;
  letter-spacing: 0.08em;
}

.login-card h2 {
  margin: 0 0 10px;
  text-align: center;
  font-size: 32px;
  font-weight: 800;
  letter-spacing: -0.01em;
}

.subtitle {
  margin: 0 0 34px;
  text-align: center;
  font-size: 16.5px;
  color: #64748b;
}

.error-message {
  padding: 12px 14px;
  margin-bottom: 20px;
  border-radius: 12px;
  background: #fee2e2;
  color: #991b1b;
  font-weight: 700;
  font-size: 14px;
  text-align: center;
}

label {
  display: block;
  margin-bottom: 8px;
  font-size: 15px;
  font-weight: 750;
}

.input-box {
  height: 58px;
  margin-bottom: 22px;
  padding: 0 16px;
  border: 1.5px solid #e2e8f0;
  border-radius: 13px;
  display: flex;
  align-items: center;
  gap: 12px;
  transition: 0.15s ease;
}

.input-box:focus-within {
  border-color: #166534;
  box-shadow: 0 0 0 3px rgba(22, 101, 52, 0.12);
}

.input-icon {
  color: #94a3b8;
  flex-shrink: 0;
}

.input-box input {
  flex: 1;
  min-width: 0;
  border: 0;
  outline: 0;
  background: transparent;
  font-size: 16px;
  color: inherit;
  font-family: inherit;
}

.field-hint {
  margin: -12px 0 20px;
  color: #b45309;
  font-size: 13px;
  font-weight: 700;
}

.eye-btn {
  border: 0;
  background: transparent;
  color: #94a3b8;
  display: grid;
  place-items: center;
  cursor: pointer;
  flex-shrink: 0;
}

.sign-btn {
  width: 100%;
  height: 58px;
  border: 0;
  border-radius: 13px;
  background: #111;
  color: white;
  font-size: 16px;
  font-weight: 750;
  cursor: pointer;
}

.sign-btn:disabled {
  opacity: 0.65;
  cursor: not-allowed;
}

.divider {
  display: flex;
  align-items: center;
  gap: 14px;
  margin: 26px 0;
  color: #94a3b8;
  font-size: 13px;
}

.divider span {
  flex: 1;
  height: 1px;
  background: #e2e8f0;
}

.guest-btn {
  width: 100%;
  height: 54px;
  border-radius: 13px;
  border: 1.5px solid #e2e8f0;
  background: white;
  color: #334155;
  font-size: 16px;
  font-weight: 750;
  cursor: pointer;
  display: flex;
  align-items: center;
  justify-content: center;
  gap: 10px;
}

.signup {
  margin: 26px 0 0;
  text-align: center;
  color: #64748b;
  font-size: 14px;
}

.signup button {
  border: none;
  background: transparent;
  color: #166534;
  font-weight: 800;
  cursor: pointer;
  font-size: 14px;
}

/* ---------- Dark mode ---------- */

.login-page.dark {
  background: #0f172a;
  color: #f8fafc;
}
.login-page.dark .navbar,
.login-page.dark .login-card {
  background: #111827;
  border-color: #334155;
}

.login-page.dark .nav-links {
  background: #1e293b;
}

.login-page.dark .icon-btn.active {
  background: #0f172a;
}

.login-page.dark h2,
.login-page.dark .brand,
.login-page.dark .card-brand,
.login-page.dark label {
  color: #f8fafc;
}

.login-page.dark .subtitle,
.login-page.dark .signup {
  color: #94a3b8;
}

.login-page.dark .input-box {
  background: #0f172a;
  border-color: #334155;
}

.login-page.dark .input-box:focus-within {
  border-color: #4ade80;
  box-shadow: 0 0 0 3px rgba(74, 222, 128, 0.15);
}

.login-page.dark .guest-btn {
  background: #0f172a;
  border-color: #334155;
  color: #f8fafc;
}

/* ---------- Responsive ---------- */

@media (max-width: 1100px) {
  .layout {
    min-height: auto;
  }

  .side-panel {
    display: none;
  }
}

@media (max-width: 480px) {
  .login-page {
    padding: 14px;
  }

  .navbar {
    padding: 0 16px;
  }

  .login-card {
    padding: 32px 24px;
  }
}
</style>