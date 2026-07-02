<template>
  <div class="page">
    <AppTopbar />

    <main class="content">
      <section class="admin-hero">
        <img class="admin-image" src="/admin-cat.png" alt="Admin" />

        <div>
          <h1>Welcome, <span>Admin</span></h1>
          <p>Here is an overview of your library system.</p>
        </div>
      </section>

      <section class="cards">
        <StatCard icon="📚" title="Total Books" :value="books.length" />
        <StatCard icon="✍️" title="Total Authors" :value="authors.length" />
        <StatCard icon="🏷️" title="Total Categories" :value="categories.length" />
       <div class="clickable-card" @click="goMembers">
  <StatCard icon="👥" title="Total Members" :value="members.length" />
</div>
        <StatCard icon="⏳" title="Active Loans" :value="borrowedReports.length" />
        <StatCard icon="💰" title="Total Fines" :value="fineReports.length" />
      </section>

      <section class="insight-card">
        <div>
          <h2>Need detailed insights?</h2>
          <p>Go to reports to analyze borrowed books, overdue books and fines.</p>
        </div>

  <div class="actions">
  <button class="primary" @click="goReports">Go to Reports →</button>
  <button class="secondary" @click="goBookManagement">Manage Books</button>
  <button class="secondary" @click="goLoanManagement">Manage Loans</button>
  <button class="secondary" @click="goScanBook">Scan Book</button>
  <button class="secondary" @click="goHome">Back to Home</button>
</div>
      </section>
    </main>
  </div>
</template>

<script setup>
import { onMounted, ref } from "vue";
import { useRouter } from "vue-router";
import axios from "axios";

import AppTopbar from "@/components/AppTopbar.vue";
import StatCard from "@/components/StatCard.vue";

const router = useRouter();
const API_BASE_URL = "http://localhost:5239";

const books = ref([]);
const authors = ref([]);
const categories = ref([]);
const members = ref([]);

const borrowedReports = ref([]);
const fineReports = ref([]);

const token = localStorage.getItem("token");

const headers = {
  Authorization: `Bearer ${token}`,
};

const normalize = (res) => {
  return res.data.data || res.data || [];
};

const safeLoad = async (request, target) => {
  try {
    const response = await request();
    target.value = normalize(response);
  } catch (error) {
    console.error("Admin load error:", error);
    target.value = [];
  }
};

const getAdminData = async () => {
  await Promise.all([
    safeLoad(() => axios.get(`${API_BASE_URL}/api/books`), books),
    safeLoad(() => axios.get(`${API_BASE_URL}/api/authors`), authors),
    safeLoad(() => axios.get(`${API_BASE_URL}/api/categories`), categories),
    safeLoad(() => axios.get(`${API_BASE_URL}/api/members`, { headers }), members),

    safeLoad(
      () => axios.get(`${API_BASE_URL}/api/reports/borrowed-books`, { headers }),
      borrowedReports
    ),

    safeLoad(
      () => axios.get(`${API_BASE_URL}/api/reports/fines`, { headers }),
      fineReports
    ),
  ]);
};

const goReports = () => {
  router.push("/reports");
};

const goHome = () => {
  router.push("/home");
};
const goBookManagement = () => {
  router.push("/admin/books");
};
const goLoanManagement = () => {
  router.push("/admin/loans");
};
const goScanBook = () => {
  router.push("/admin/scan");
};
const goMembers = () => {
  router.push("/admin/members");
};

onMounted(getAdminData);
</script>

<style scoped>
.page {
  min-height: 100vh;
  padding: 24px;
  background:
    radial-gradient(circle at 10% 15%, #eef9e8 0%, transparent 28%),
    radial-gradient(circle at 95% 95%, #f8eaf8 0%, transparent 24%),
    #f8faf7;
  font-family: Inter, system-ui, sans-serif;
}

.content {
  max-width: 1180px;
  margin: 0 auto;
}

.admin-hero {
  display: flex;
  align-items: center;
  gap: 34px;
  padding: 34px;
  margin-bottom: 24px;
  border-radius: 28px;
  background: white;
  box-shadow: 0 18px 45px rgba(15, 23, 42, 0.08);
}

.admin-image {
  width: 150px;
  height: 150px;
  border-radius: 26px;
  object-fit: cover;
  border: 1px solid #e5e7eb;
}

.admin-hero h1 {
  margin: 0;
  font-size: 44px;
  color: #0f172a;
}

.admin-hero h1 span {
  color: #166534;
}

.admin-hero p {
  margin-top: 10px;
  color: #64748b;
  font-size: 18px;
}

.cards {
  display: grid;
  grid-template-columns: repeat(3, 1fr);
  gap: 18px;
}

.insight-card {
  margin-top: 26px;
  padding: 26px;
  border-radius: 24px;
  background: white;
  box-shadow: 0 14px 34px rgba(15, 23, 42, 0.07);
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: 20px;
}

.insight-card h2 {
  margin: 0;
  color: #0f172a;
}

.insight-card p {
  margin: 8px 0 0;
  color: #64748b;
}

.actions {
  display: flex;
  gap: 14px;
}

button {
  height: 52px;
  padding: 0 22px;
  border: none;
  border-radius: 16px;
  font-weight: 900;
  cursor: pointer;
}

.primary {
  background: #166534;
  color: white;
}

.secondary {
  background: #f1f5f9;
  color: #334155;
}
.clickable-card :deep(*) {
  cursor: pointer;
}

.clickable-card:hover {
  transform: translateY(-3px);
}

.clickable-card:hover {
  transform: translateY(-3px);
}

@media (max-width: 900px) {
  .admin-hero,
  .insight-card {
    flex-direction: column;
    align-items: flex-start;
  }

  .cards {
    grid-template-columns: 1fr;
  }

  .actions {
    flex-direction: column;
    width: 100%;
  }

  button {
    width: 100%;
  }
  .clickable-card {
  cursor: pointer;
  transition: 0.18s ease;
}
.clickable-card {
  cursor: pointer;
  transition: 0.18s ease;
}


}
</style>