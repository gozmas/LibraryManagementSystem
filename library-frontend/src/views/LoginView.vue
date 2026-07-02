<template>
  <div :class="['login-page', { dark: isDarkMode }]">
  <header class="navbar">
  <div class="brand">
    <div class="logo">📚</div>
    <span>LibraryMS</span>
  </div>

  <nav class="nav-links">
    <button class="icon-btn" type="button" @click="setLightMode">☀️</button>
    <button class="icon-btn" type="button" @click="setDarkMode">🌙</button>
  </nav>
</header>

    <main class="layout">
      <motion.section
        class="left-panel"
        :initial="{ opacity: 0, x: -40 }"
        :animate="{ opacity: 1, x: 0 }"
        :transition="{ duration: 0.5 }"
      >
        <div class="intro">
          <p class="welcome">Welcome to</p>
          <h1>LibraryMS</h1>
          <h2>Your digital library assistant.</h2>
          <p class="desc">
            Manage books, members, loans and more with an easy and modern experience.
          </p>

          <div class="features">
            <motion.div
              class="feature"
              :initial="{ opacity: 0, y: 14 }"
              :animate="{ opacity: 1, y: 0 }"
              :transition="{ duration: 0.4, delay: 0.2 }"
            >
              <span>📖</span>
              <div>
                <h3>Browse Books</h3>
                <p>Discover and search thousands of books.</p>
              </div>
            </motion.div>

            <motion.div
              class="feature"
              :initial="{ opacity: 0, y: 14 }"
              :animate="{ opacity: 1, y: 0 }"
              :transition="{ duration: 0.4, delay: 0.3 }"
            >
              <span>👥</span>
              <div>
                <h3>Manage Members</h3>
                <p>Add and manage library members easily.</p>
              </div>
            </motion.div>

            <motion.div
              class="feature"
              :initial="{ opacity: 0, y: 14 }"
              :animate="{ opacity: 1, y: 0 }"
              :transition="{ duration: 0.4, delay: 0.4 }"
            >
              <span>🗓️</span>
              <div>
                <h3>Track Loans</h3>
                <p>Borrow, return and renew books effortlessly.</p>
              </div>
            </motion.div>

            <motion.div
              class="feature"
              :initial="{ opacity: 0, y: 14 }"
              :animate="{ opacity: 1, y: 0 }"
              :transition="{ duration: 0.4, delay: 0.5 }"
            >
              <span>📊</span>
              <div>
                <h3>Reports & Analytics</h3>
                <p>Get insights and reports about your library.</p>
              </div>
            </motion.div>
          </div>
        </div>

        <div class="circle"></div>
        <div class="art">🌿 📚</div>
      </motion.section>

      <section class="right-panel">
        <motion.div
          class="login-card"
          :initial="{ opacity: 0, y: 30 }"
          :animate="{ opacity: 1, y: 0 }"
          :transition="{ duration: 0.5, delay: 0.15 }"
        >
          <div class="card-brand">
            <div class="logo">📚</div>
            <span>LibraryMS</span>
          </div>

          <h2>Welcome back</h2>
          <p class="subtitle">Sign in to your account to continue.</p>

          <p v-if="errorMessage" class="error-message">
            {{ errorMessage }}
          </p>

          <form @submit.prevent="handleLogin">
            <label>Email</label>
            <div class="input-box">
              <span>✉️</span>
              <input
                v-model="email"
                type="email"
                placeholder="name@email.com"
                required
              />
            </div>
            <p v-if="email && !isEmailValid" class="field-hint">
              Please enter a valid email address (e.g. name@email.com).
            </p>

            <label>Password</label>
            <div class="input-box">
              <span>🔒</span>
              <input
                v-model="password"
                :type="showPassword ? 'text' : 'password'"
                placeholder="••••••••"
                required
              />
              <button class="eye-btn" type="button" @click="showPassword = !showPassword">
                {{ showPassword ? "🙈" : "👁️" }}
              </button>
            </div>

            <motion.button
              class="sign-btn"
              type="submit"
              :disabled="loading"
              :whileHover="{ scale: 1.015 }"
              :whilePress="{ scale: 0.985 }"
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
              :whileHover="{ scale: 1.015 }"
              :whilePress="{ scale: 0.985 }"
              @click="continueAsGuest"
            >
              👤 Continue as guest
            </motion.button>

            <p class="signup">
              Don't have an account?
              <button type="button" @click="goSignUp">Sign up</button>
            </p>
          </form>
        </motion.div>
      </section>
    </main>
  </div>
</template>

<script setup>
import { ref, computed } from "vue";
import { useRouter } from "vue-router";
import axios from "axios";
import { motion } from "motion-v";

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
  background: #f8faf7;
  font-family: Inter, system-ui, sans-serif;
  color: #0f172a;
}

.navbar {
  height: 74px;
  padding: 0 28px;
  margin-bottom: 20px;
  border-radius: 22px;
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
  font-size: 24px;
  font-weight: 800;
  cursor: pointer;
}

.logo {
  width: 48px;
  height: 48px;
  border-radius: 14px;
  background: #111;
  display: grid;
  place-items: center;
  font-size: 24px;
}

.nav-links {
  display: flex;
  align-items: center;
  gap: 22px;
}

.nav-links button {
  border: none;
  background: transparent;
  font-size: 16px;
  font-weight: 700;
  color: #334155;
  cursor: pointer;
}

.nav-links .icon-btn {
  width: 42px;
  height: 42px;
  border-radius: 50%;
  background: #f1f5f9;
}

.layout {
  height: calc(100vh - 118px);
  min-height: 720px;
  border-radius: 24px;
  overflow: hidden;
  background: white;
  display: grid;
  grid-template-columns: 1fr 1fr;
  box-shadow: 0 24px 60px rgba(15, 23, 42, 0.08);
}

.left-panel {
  position: relative;
  padding: 70px 80px;
  background: linear-gradient(135deg, #f1faec, #ffffff);
  overflow: hidden;
}

.intro {
  position: relative;
  z-index: 2;
}

.welcome {
  margin: 0 0 12px;
  color: #166534;
  font-size: 26px;
  font-weight: 800;
}

.left-panel h1 {
  margin: 0;
  font-size: 82px;
  line-height: 0.95;
  letter-spacing: -4px;
}

.left-panel h2 {
  margin: 24px 0 14px;
  font-size: 30px;
  color: #475569;
}

.desc {
  max-width: 560px;
  margin: 0 0 42px;
  font-size: 20px;
  line-height: 1.6;
  color: #475569;
}

.features {
  display: grid;
  gap: 26px;
}

.feature {
  display: flex;
  align-items: center;
  gap: 20px;
}

.feature > span {
  width: 64px;
  height: 64px;
  border-radius: 50%;
  background: #dff2d8;
  display: grid;
  place-items: center;
  font-size: 28px;
  flex-shrink: 0;
}

.feature h3 {
  margin: 0 0 6px;
  font-size: 20px;
}

.feature p {
  margin: 0;
  font-size: 17px;
  color: #475569;
}

.circle {
  position: absolute;
  right: -140px;
  bottom: -180px;
  width: 560px;
  height: 560px;
  border-radius: 50%;
  background: rgba(132, 181, 111, 0.2);
}

.art {
  position: absolute;
  right: 90px;
  bottom: 90px;
  font-size: 120px;
  z-index: 2;
}

.right-panel {
  display: flex;
  align-items: center;
  justify-content: center;
  padding: 60px;
}

.login-card {
  width: 100%;
  max-width: 560px;
  padding: 58px 54px;
  border-radius: 26px;
  background: white;
  border: 1px solid #e5e7eb;
  box-shadow: 0 28px 60px rgba(15, 23, 42, 0.13);
}

.card-brand {
  justify-content: center;
  margin-bottom: 42px;
}

.login-card h2 {
  margin: 0 0 10px;
  text-align: center;
  font-size: 42px;
}

.subtitle {
  margin: 0 0 28px;
  text-align: center;
  font-size: 18px;
  color: #64748b;
}

.error-message {
  padding: 12px 14px;
  margin-bottom: 22px;
  border-radius: 12px;
  background: #fee2e2;
  color: #991b1b;
  font-weight: 700;
  text-align: center;
}

label {
  display: block;
  margin-bottom: 10px;
  font-size: 17px;
  font-weight: 800;
}

.input-box {
  height: 64px;
  margin-bottom: 24px;
  padding: 0 16px;
  border: 1.5px solid #cbd5e1;
  border-radius: 14px;
  display: flex;
  align-items: center;
  gap: 14px;
}

.input-box input {
  flex: 1;
  border: 0;
  outline: 0;
  font-size: 18px;
}

.field-hint {
  margin: -16px 0 24px;
  color: #b45309;
  font-size: 14px;
  font-weight: 700;
}

.eye-btn {
  border: 0;
  background: transparent;
  cursor: pointer;
}

.sign-btn {
  width: 100%;
  height: 64px;
  border: 0;
  border-radius: 14px;
  background: #111;
  color: white;
  font-size: 18px;
  font-weight: 800;
  cursor: pointer;
}

.sign-btn:disabled {
  opacity: 0.7;
  cursor: not-allowed;
}

.divider {
  display: flex;
  align-items: center;
  gap: 16px;
  margin: 30px 0;
  color: #94a3b8;
}

.divider span {
  flex: 1;
  height: 1px;
  background: #e2e8f0;
}

.guest-btn {
  width: 100%;
  height: 60px;
  border-radius: 14px;
  border: 1.5px solid #cbd5e1;
  background: white;
  font-size: 18px;
  font-weight: 800;
  cursor: pointer;
}

.signup {
  margin: 30px 0 0;
  text-align: center;
  color: #64748b;
  font-size: 17px;
}

.signup button {
  border: none;
  background: transparent;
  color: #166534;
  font-weight: 800;
  cursor: pointer;
  font-size: 17px;
}
.login-page.dark {
  background: #0f172a;
  color: #f8fafc;
}

.login-page.dark .navbar,
.login-page.dark .layout,
.login-page.dark .login-card {
  background: #111827;
  border-color: #334155;
}

.login-page.dark .left-panel {
  background: linear-gradient(135deg, #132315, #111827);
}

.login-page.dark h1,
.login-page.dark h2,
.login-page.dark h3,
.login-page.dark .brand,
.login-page.dark .card-brand,
.login-page.dark label {
  color: #f8fafc;
}

.login-page.dark .desc,
.login-page.dark .subtitle,
.login-page.dark .feature p,
.login-page.dark .signup {
  color: #cbd5e1;
}

.login-page.dark .input-box,
.login-page.dark .guest-btn {
  background: #0f172a;
  border-color: #475569;
  color: #f8fafc;
}

.login-page.dark .input-box input {
  background: transparent;
  color: #f8fafc;
}

.login-page.dark .icon-btn {
  background: #1e293b;
}

@media (max-width: 1100px) {
  .layout {
    grid-template-columns: 1fr;
    height: auto;
  }

  .left-panel {
    display: none;
  }

  .right-panel {
    min-height: calc(100vh - 118px);
  }

  .nav-links {
    display: none;
  }
}
</style>