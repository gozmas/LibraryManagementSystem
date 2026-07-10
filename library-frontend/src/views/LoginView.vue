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
      <motion.section
        class="left-panel"
        :initial="{ opacity: 0, x: -30 }"
        :animate="{ opacity: 1, x: 0 }"
        :transition="{ duration: 0.5 }"
      >
        <div class="intro">
          <p class="eyebrow">Welcome to</p>
          <h1>LibraryMS</h1>
          <p class="desc">
            The catalogue, the members and every loan &mdash; kept in one
            calm, well-organized place.
          </p>

          <ul class="features">
            <motion.li
              v-for="(feature, index) in features"
              :key="feature.title"
              :initial="{ opacity: 0, y: 10 }"
              :animate="{ opacity: 1, y: 0 }"
              :transition="{ duration: 0.35, delay: 0.15 + index * 0.08 }"
            >
              <span class="feature-icon">
                <component :is="feature.icon" :size="20" />
              </span>
              <span class="feature-text">
                <strong>{{ feature.title }}</strong>
                <span>{{ feature.desc }}</span>
              </span>
            </motion.li>
          </ul>

          <div v-if="featuredBooks.length" class="featured">
            <p class="featured-label">From our shelves</p>

            <div class="featured-scroll">
              <button
                v-for="book in featuredBooks"
                :key="book.id"
                type="button"
                class="featured-card"
                @click="goToBook(book.id)"
              >
                <div class="featured-cover">
                  <img
                    v-if="book.coverUrl"
                    :src="book.coverUrl"
                    :alt="book.title"
                  />
                  <BookOpen v-else :size="22" />
                </div>

                <p class="featured-title">{{ book.title }}</p>
                <p class="featured-author">{{ book.authorName || "Unknown" }}</p>
              </button>
            </div>
          </div>
        </div>

        <div class="spines" aria-hidden="true">
          <span
            v-for="n in 7"
            :key="n"
            :class="`spine spine-${n}`"
          ></span>
        </div>
      </motion.section>

      <section class="right-panel">
        <motion.div
          class="login-card"
          :initial="{ opacity: 0, y: 24 }"
          :animate="{ opacity: 1, y: 0 }"
          :transition="{ duration: 0.5, delay: 0.15 }"
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
      </section>
    </main>
  </div>
</template>

<script setup>
import { ref, computed, onMounted } from "vue";
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
  Users,
  CalendarCheck,
  ChartColumn,
} from "@lucide/vue";

const router = useRouter();

const email = ref("");
const password = ref("");
const showPassword = ref(false);
const loading = ref(false);
const errorMessage = ref("");
const isDarkMode = ref(false);
const featuredBooks = ref([]);

const API_BASE_URL = "http://localhost:5239";

const emailPattern = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;

const isEmailValid = computed(() => emailPattern.test(email.value));

const features = [
  { title: "Browse Books", desc: "Discover and search thousands of books.", icon: BookOpen },
  { title: "Manage Members", desc: "Add and manage library members easily.", icon: Users },
  { title: "Track Loans", desc: "Borrow, return and renew books effortlessly.", icon: CalendarCheck },
  { title: "Reports & Analytics", desc: "Get insights and reports about your library.", icon: ChartColumn },
];

// Fisher-Yates: dizinin kendisini değiştirmeden rastgele bir örneklem alır.
const pickRandom = (list, count) => {
  const copy = [...list];

  for (let i = copy.length - 1; i > 0; i--) {
    const j = Math.floor(Math.random() * (i + 1));
    [copy[i], copy[j]] = [copy[j], copy[i]];
  }

  return copy.slice(0, count);
};

// Kategori başına bir kitap seçer (o kategorideki kitap da rastgele
// belirlenir), ardından kategorilerin gösterim sırasını karıştırıp
// en fazla maxCount tanesini döner. Böylece şerit tek bir kategoriye
// yığılmak yerine katalogdaki çeşitliliği yansıtır.
const pickOnePerCategory = (list, maxCount) => {
  const byCategory = new Map();

  for (const book of list) {
    const key = book.categoryId ?? book.categoryName ?? "uncategorized";

    if (!byCategory.has(key)) {
      byCategory.set(key, []);
    }

    byCategory.get(key).push(book);
  }

  const onePerCategory = [...byCategory.values()].map(
    (booksInCategory) => pickRandom(booksInCategory, 1)[0]
  );

  return pickRandom(onePerCategory, maxCount);
};

// Login sayfası herkese açık olduğu için auth gerektirmeyen /api/books
// endpoint'ini kullanıyoruz; katalogdan rastgele birkaç kitap seçip
// "From our shelves" şeridinde gösteriyoruz.
const loadFeaturedBooks = async () => {
  try {
    const response = await axios.get(`${API_BASE_URL}/api/books`);
    const books = response.data.data || response.data || [];
    featuredBooks.value = pickOnePerCategory(books, 8);
  } catch (error) {
    console.error("Featured books load failed:", error);
  }
};

const goToBook = (id) => {
  router.push(`/books/${id}`);
};

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

onMounted(loadFeaturedBooks);
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
  border-radius: 22px;
  background: white;
  display: grid;
  grid-template-columns: 1.05fr 1fr;
  box-shadow: 0 24px 60px rgba(15, 23, 42, 0.08);
}

.left-panel {
  position: relative;
  padding: 64px 64px 0;
  background: linear-gradient(160deg, #eef6e9 0%, #ffffff 65%);
  overflow: hidden;
  display: flex;
  flex-direction: column;
  justify-content: space-between;
  min-height: 100%;
}

.intro {
  position: relative;
  z-index: 2;
  max-width: 480px;
  padding-bottom: 48px;
}

.eyebrow {
  margin: 0 0 10px;
  color: #166534;
  font-size: 14px;
  font-weight: 800;
  text-transform: uppercase;
  letter-spacing: 0.08em;
}

.left-panel h1 {
  margin: 0;
  font-size: 52px;
  font-weight: 800;
  line-height: 1;
  letter-spacing: -0.02em;
}

.desc {
  max-width: 420px;
  margin: 20px 0 40px;
  font-size: 17px;
  line-height: 1.65;
  color: #475569;
}

.features {
  list-style: none;
  margin: 0;
  padding: 0;
  display: grid;
  gap: 20px;
}

.features li {
  display: flex;
  align-items: flex-start;
  gap: 16px;
}

.feature-icon {
  width: 44px;
  height: 44px;
  border-radius: 13px;
  background: #dff2d8;
  color: #166534;
  display: grid;
  place-items: center;
  flex-shrink: 0;
}

.feature-text {
  display: flex;
  flex-direction: column;
  gap: 3px;
  padding-top: 2px;
}

.feature-text strong {
  font-size: 16px;
  font-weight: 750;
}

.feature-text span:last-child {
  font-size: 14px;
  color: #64748b;
  line-height: 1.4;
}

/* ---------- Featured books strip ---------- */

.featured {
  margin-top: 32px;
}

.featured-label {
  margin: 0 0 12px;
  font-size: 13px;
  font-weight: 800;
  text-transform: uppercase;
  letter-spacing: 0.06em;
  color: #64748b;
}

.featured-scroll {
  display: flex;
  align-items: flex-start;
  gap: 12px;
  overflow-x: auto;
  padding-bottom: 6px;
  scrollbar-width: thin;
}

.featured-card {
  flex: 0 0 96px;
  display: flex;
  flex-direction: column;
  align-items: flex-start;
  border: none;
  background: none;
  padding: 0;
  text-align: left;
  cursor: pointer;
  font-family: inherit;
}

.featured-cover {
  width: 96px;
  height: 132px;
  flex-shrink: 0;
  border-radius: 10px;
  background: #dff2d8;
  color: #166534;
  overflow: hidden;
  display: grid;
  place-items: center;
  box-shadow: 0 8px 18px rgba(15, 23, 42, 0.1);
  margin-bottom: 8px;
}

.featured-cover img {
  width: 100%;
  height: 100%;
  object-fit: cover;
}

.featured-title {
  margin: 0;
  font-size: 13px;
  font-weight: 750;
  line-height: 1.3;
  color: #0f172a;
  display: -webkit-box;
  -webkit-line-clamp: 2;
  -webkit-box-orient: vertical;
  overflow: hidden;
}

.featured-author {
  margin: 3px 0 0;
  font-size: 12px;
  color: #64748b;
}

/* Signature element: a row of overlapping "book spines" grounding the
   panel in its actual subject matter instead of a generic decorative
   blob/circle. Pure CSS, no external assets. */
.spines {
  position: relative;
  z-index: 1;
  display: flex;
  align-items: flex-end;
  gap: 6px;
  height: 130px;
  margin: 0 -64px 0;
  padding: 0 40px;
}

.spine {
  flex: 1;
  border-radius: 6px 6px 0 0;
  opacity: 0.9;
}

.spine-1 { height: 68%; background: #14532d; }
.spine-2 { height: 92%; background: #166534; }
.spine-3 { height: 55%; background: #4d7c0f; }
.spine-4 { height: 100%; background: #0f172a; }
.spine-5 { height: 72%; background: #65a30d; }
.spine-6 { height: 84%; background: #166534; }
.spine-7 { height: 60%; background: #14532d; }

/* ---------- Right panel / card ---------- */

.right-panel {
  display: flex;
  align-items: center;
  justify-content: center;
  padding: 56px;
}

.login-card {
  width: 100%;
  max-width: 440px;
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
  font-size: 28px;
  font-weight: 800;
  letter-spacing: -0.01em;
}

.subtitle {
  margin: 0 0 30px;
  text-align: center;
  font-size: 15px;
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
  font-size: 14px;
  font-weight: 750;
}

.input-box {
  height: 54px;
  margin-bottom: 20px;
  padding: 0 14px;
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
  font-size: 15px;
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
  height: 54px;
  border: 0;
  border-radius: 13px;
  background: #111;
  color: white;
  font-size: 15px;
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
  height: 50px;
  border-radius: 13px;
  border: 1.5px solid #e2e8f0;
  background: white;
  color: #334155;
  font-size: 15px;
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
.login-page.dark .layout {
  background: #111827;
  border-color: #334155;
}

.login-page.dark .nav-links {
  background: #1e293b;
}

.login-page.dark .icon-btn.active {
  background: #0f172a;
}

.login-page.dark .left-panel {
  background: linear-gradient(160deg, #132315 0%, #111827 70%);
}

.login-page.dark h1,
.login-page.dark h2,
.login-page.dark .brand,
.login-page.dark .card-brand,
.login-page.dark label {
  color: #f8fafc;
}

.login-page.dark .feature-text strong {
  color: #f8fafc;
}

.login-page.dark .desc,
.login-page.dark .subtitle,
.login-page.dark .feature-text span:last-child,
.login-page.dark .signup {
  color: #94a3b8;
}

.login-page.dark .feature-icon {
  background: #1e293b;
  color: #4ade80;
}

.login-page.dark .featured-label {
  color: #94a3b8;
}

.login-page.dark .featured-cover {
  background: #1e293b;
  color: #4ade80;
}

.login-page.dark .featured-title {
  color: #f8fafc;
}

.login-page.dark .featured-author {
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
    grid-template-columns: 1fr;
    min-height: auto;
  }

  .left-panel {
    display: none;
  }

  .right-panel {
    padding: 40px 24px;
  }
}

@media (max-width: 480px) {
  .login-page {
    padding: 14px;
  }

  .navbar {
    padding: 0 16px;
  }

  .right-panel {
    padding: 24px 16px;
  }
}
</style>