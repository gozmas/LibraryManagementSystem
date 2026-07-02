<template>
  <div class="register-page">
    <header class="navbar">
      <div class="brand">
        <div class="logo">📚</div>
        <span>LibraryMS</span>
      </div>

      <button class="login-top-btn" @click="goLogin">
        Sign in
      </button>
    </header>

    <main class="layout">
      <section class="left-panel">
        <p class="welcome">Join LibraryMS</p>
        <h1>Create Account</h1>
        <p class="desc">
          Register as a member to borrow books, track your loans and view your fines.
        </p>

        <div class="features">
          <div class="feature">
            <span>📚</span>
            <div>
              <h3>Borrow Books</h3>
              <p>Borrow available books easily.</p>
            </div>
          </div>

          <div class="feature">
            <span>⏳</span>
            <div>
              <h3>Track Loans</h3>
              <p>View borrow dates, due dates and return status.</p>
            </div>
          </div>

          <div class="feature">
            <span>💰</span>
            <div>
              <h3>View Fines</h3>
              <p>Check unpaid and paid fine records.</p>
            </div>
          </div>
        </div>
      </section>

      <section class="right-panel">
        <div class="register-card">
          <div class="card-brand">
            <div class="logo">📚</div>
            <span>LibraryMS</span>
          </div>

          <h2>Sign up</h2>
          <p class="subtitle">Create your member account.</p>

          <p v-if="errorMessage" class="error-message">
            {{ errorMessage }}
          </p>

          <p v-if="successMessage" class="success-message">
            {{ successMessage }}
          </p>

          <form @submit.prevent="handleRegister">
            <div class="two-columns">
              <div>
                <label>First Name</label>
                <input v-model="firstName" type="text" placeholder="First name" />
              </div>

              <div>
                <label>Last Name</label>
                <input v-model="lastName" type="text" placeholder="Last name" />
              </div>
            </div>

            <label>Username</label>
            <input v-model="username" type="text" placeholder="username" />

            <label>Email</label>
            <input v-model="email" type="email" placeholder="name@email.com" />

            <label>Password</label>
            <input v-model="password" type="password" placeholder="••••••••" />

            <label>Confirm Password</label>
            <input v-model="confirmPassword" type="password" placeholder="••••••••" />

            <button class="submit-btn" type="submit" :disabled="loading">
              {{ loading ? "Creating account..." : "Create Account" }}
            </button>

            <p class="login-text">
              Already have an account?
              <button type="button" @click="goLogin">Sign in</button>
            </p>
          </form>
        </div>
      </section>
    </main>
  </div>
</template>

<script setup>
import { ref } from "vue";
import { useRouter } from "vue-router";
import axios from "axios";

const router = useRouter();

const API_BASE_URL = "http://localhost:5239";

const firstName = ref("");
const lastName = ref("");
const username = ref("");
const email = ref("");
const password = ref("");
const confirmPassword = ref("");

const loading = ref(false);
const errorMessage = ref("");
const successMessage = ref("");

const handleRegister = async () => {
  errorMessage.value = "";
  successMessage.value = "";

  if (
    !firstName.value ||
    !lastName.value ||
    !username.value ||
    !email.value ||
    !password.value ||
    !confirmPassword.value
  ) {
    errorMessage.value = "Please fill in all fields.";
    return;
  }

  if (password.value !== confirmPassword.value) {
    errorMessage.value = "Passwords do not match.";
    return;
  }

  try {
    loading.value = true;

    await axios.post(`${API_BASE_URL}/api/auth/register`, {
      firstName: firstName.value,
      lastName: lastName.value,
      username: username.value,
      email: email.value,
      password: password.value,
    });

    successMessage.value = "Account created successfully. Redirecting to login...";

    setTimeout(() => {
      router.push("/login");
    }, 900);
  } catch (error) {
    console.error(error);

    const data = error.response?.data;

    if (typeof data === "string") {
      errorMessage.value = data;
    } else if (data?.message) {
      errorMessage.value = data.message;
    } else {
      errorMessage.value = "Registration failed.";
    }
  } finally {
    loading.value = false;
  }
};

const goLogin = () => {
  router.push("/login");
};
</script>

<style scoped>
* {
  box-sizing: border-box;
}

.register-page {
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
  justify-content: space-between;
  align-items: center;
}

.brand,
.card-brand {
  display: flex;
  align-items: center;
  gap: 12px;
  font-size: 24px;
  font-weight: 900;
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

.login-top-btn {
  height: 46px;
  padding: 0 18px;
  border: none;
  border-radius: 14px;
  background: #111;
  color: white;
  font-weight: 900;
  cursor: pointer;
}

.layout {
  min-height: calc(100vh - 118px);
  border-radius: 24px;
  background: white;
  overflow: hidden;
  display: grid;
  grid-template-columns: 1fr 1fr;
  box-shadow: 0 24px 60px rgba(15, 23, 42, 0.08);
}

.left-panel {
  padding: 76px 84px;
  background: linear-gradient(135deg, #f1faec, #ffffff);
}

.welcome {
  margin: 0 0 14px;
  color: #166534;
  font-size: 26px;
  font-weight: 900;
}

.left-panel h1 {
  margin: 0;
  font-size: 70px;
  line-height: 1;
  letter-spacing: -3px;
}

.desc {
  max-width: 560px;
  margin: 24px 0 42px;
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
  gap: 18px;
  align-items: center;
}

.feature span {
  width: 62px;
  height: 62px;
  border-radius: 50%;
  background: #dff2d8;
  display: grid;
  place-items: center;
  font-size: 28px;
}

.feature h3 {
  margin: 0 0 6px;
  font-size: 20px;
}

.feature p {
  margin: 0;
  color: #64748b;
  font-size: 16px;
}

.right-panel {
  display: flex;
  justify-content: center;
  align-items: center;
  padding: 48px;
}

.register-card {
  width: 100%;
  max-width: 600px;
  padding: 44px;
  border-radius: 26px;
  border: 1px solid #e5e7eb;
  background: white;
  box-shadow: 0 28px 60px rgba(15, 23, 42, 0.13);
}

.card-brand {
  justify-content: center;
  margin-bottom: 28px;
}

.register-card h2 {
  margin: 0 0 8px;
  text-align: center;
  font-size: 38px;
}

.subtitle {
  margin: 0 0 26px;
  text-align: center;
  color: #64748b;
  font-size: 17px;
}

.two-columns {
  display: grid;
  grid-template-columns: 1fr 1fr;
  gap: 16px;
}

label {
  display: block;
  margin: 14px 0 8px;
  font-weight: 900;
  color: #0f172a;
}

input {
  width: 100%;
  height: 54px;
  padding: 0 16px;
  border-radius: 14px;
  border: 1.5px solid #cbd5e1;
  outline: none;
  font-size: 16px;
  font-weight: 700;
}

input:focus {
  border-color: #166534;
}

.submit-btn {
  width: 100%;
  height: 58px;
  margin-top: 24px;
  border: none;
  border-radius: 14px;
  background: #111;
  color: white;
  font-size: 17px;
  font-weight: 900;
  cursor: pointer;
}

.submit-btn:disabled {
  opacity: 0.7;
  cursor: not-allowed;
}

.error-message,
.success-message {
  padding: 12px 14px;
  margin-bottom: 18px;
  border-radius: 12px;
  font-weight: 900;
  text-align: center;
}

.error-message {
  background: #fee2e2;
  color: #991b1b;
}

.success-message {
  background: #dcfce7;
  color: #166534;
}

.login-text {
  margin: 24px 0 0;
  text-align: center;
  color: #64748b;
}

.login-text button {
  border: none;
  background: transparent;
  color: #166534;
  font-weight: 900;
  cursor: pointer;
}

@media (max-width: 1000px) {
  .layout {
    grid-template-columns: 1fr;
  }

  .left-panel {
    display: none;
  }
}

@media (max-width: 620px) {
  .two-columns {
    grid-template-columns: 1fr;
  }

  .register-card {
    padding: 32px 22px;
  }
}
</style>