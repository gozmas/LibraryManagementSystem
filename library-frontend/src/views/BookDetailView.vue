<template>
  <div class="detail-page">
    <header class="topbar">
      <div class="brand" @click="goHome">
        <div class="logo">📚</div>
        <span>LibraryMS</span>
      </div>

      <button @click="goHome">Back to Home</button>
    </header>

    <main v-if="book" class="detail-card">
      <div class="hero-row">
        <img
          v-if="book.coverUrl"
          :src="book.coverUrl"
          :alt="book.title"
          class="cover-image"
        />

       <DetailHero
  icon="📘"
  label="Book Detail"
  :title="book.title"
  :subtitle="book.authorName || 'Unknown author'"
  :badgeText="book.isAvailable ? 'Available' : 'Borrowed'"
  :badgeType="book.isAvailable ? 'available' : 'borrowed'"
  :hide-icon="!!book.coverUrl"
/>
      </div>

      <section v-if="book.description" class="description">
        <h2>Summary</h2>
        <p>{{ book.description }}</p>
      </section>

     <section class="info-grid">
        <DetailInfoBox title="Author" :value="book.authorName || 'Unknown'" />
        <DetailInfoBox title="Category" :value="book.categoryName || 'No category'" />
        <DetailInfoBox title="ISBN" :value="book.isbn || '-'" />
        <DetailInfoBox title="Publication Year" :value="book.publicationYear || '-'" />
        <DetailInfoBox title="Available Copies" :value="`${book.availableCopies ?? 0} / ${book.totalCopies ?? 1}`" />
      </section>

      <p v-if="message" class="message">{{ message }}</p>

      <section class="actions">
        <button
          class="borrow-btn"
          :disabled="!book.isAvailable || loading"
          @click="borrowBook"
        >
          {{ loading ? "Processing..." : book.isAvailable ? "Borrow Book" : "Currently Borrowed" }}
        </button>

        <button class="secondary-btn" @click="goHome">
          Browse Other Books
        </button>
      </section>
      <section v-if="role === 'Admin'" class="loan-history">
        <h2>Loan History (All Copies)</h2>
        <p class="loan-history-sub">Which members borrowed which copy of this book.</p>

        <div class="table-card">
          <table>
            <thead>
              <tr>
                <th>Member</th>
                <th>Copy #</th>
                <th>Borrow Date</th>
                <th>Return Date</th>
                <th>Status</th>
              </tr>
            </thead>
            <tbody>
              <tr v-for="entry in loanHistory" :key="entry.loanId">
                <td><strong>{{ entry.memberName }}</strong></td>
                <td>#{{ entry.copyNumber }}</td>
                <td>{{ formatDate(entry.borrowDate) }}</td>
                <td>{{ entry.returnDate ? formatDate(entry.returnDate) : "-" }}</td>
                <td>
                  <span :class="['status-badge', statusClass(entry.copyStatus, entry.isReturned)]">
                    {{ entry.isReturned ? entry.copyStatus : "Active" }}
                  </span>
                </td>
              </tr>
            </tbody>
          </table>

          <p v-if="loanHistory.length === 0" class="empty">
            No loan history for this book yet.
          </p>
        </div>
      </section>
    </main>

    <main v-else class="detail-card">
      <p>Loading book detail...</p>
    </main>
  </div>
</template>

<script setup>
import { ref, onMounted } from "vue";
import { useRoute, useRouter } from "vue-router";
import axios from "axios";

import DetailHero from "@/components/DetailHero.vue";
import DetailInfoBox from "@/components/DetailInfoBox.vue";

const route = useRoute();
const router = useRouter();

const API_BASE_URL = "http://localhost:5239";

const book = ref(null);
const member = ref(null);
const loanHistory = ref([]);

const formatDate = (value) => {
  if (!value) return "-";
  return new Date(value).toLocaleDateString();
};

const getLoanHistory = async () => {
  if (role !== "Admin") return;

  try {
    const response = await axios.get(
      `${API_BASE_URL}/api/loans/by-book/${route.params.id}`,
      { headers: { Authorization: `Bearer ${token}` } }
    );
    loanHistory.value = response.data.data || response.data || [];
  } catch (error) {
    console.error("Loan history load error:", error);
  }
};
const statusClass = (copyStatus, isReturned) => {
  if (!isReturned) return "active";
  if (copyStatus === "Damaged") return "damaged";
  if (copyStatus === "Lost") return "lost";
  return "returned";
};

const message = ref("");
const loading = ref(false);

const token = localStorage.getItem("token");
const role = localStorage.getItem("role");

const getBook = async () => {
  const response = await axios.get(`${API_BASE_URL}/api/books/${route.params.id}`);
  book.value = response.data.data || response.data;
};

const getCurrentMember = async () => {
  if (!token || role !== "Member") return;

  const response = await axios.get(`${API_BASE_URL}/api/members/me`, {
    headers: {
      Authorization: `Bearer ${token}`,
    },
  });

  member.value = response.data.data || response.data;
};

const borrowBook = async () => {
  if (!token) {
    message.value = "Please login to borrow books.";
    return;
  }

  if (role !== "Member") {
    message.value = "Only members can borrow books.";
    return;
  }

  if (!member.value) {
    message.value = "Member profile could not be found.";
    return;
  }

  try {
    loading.value = true;

    await axios.post(
      `${API_BASE_URL}/api/loans/borrow`,
      {
        bookId: book.value.id,
        memberId: member.value.id,
      },
      {
        headers: {
          Authorization: `Bearer ${token}`,
        },
      }
    );

    message.value = "Book borrowed successfully.";
    await getBook();
  } catch (error) {
    console.error(error);
    message.value = error.response?.data?.message || "Borrow failed.";
  } finally {
    loading.value = false;
  }
};

const goHome = () => {
  router.push("/home");
};

onMounted(async () => {
  await getBook();
  await getCurrentMember();
  await getLoanHistory();
});
</script>

<style scoped>
.detail-page {
  min-height: 100vh;
  padding: 24px;
  background:
    radial-gradient(circle at 10% 15%, #eef9e8 0%, transparent 28%),
    radial-gradient(circle at 95% 95%, #f8eaf8 0%, transparent 24%),
    #f8faf7;
  font-family: Inter, system-ui, sans-serif;
  color: #0f172a;
}

.topbar {
  height: 76px;
  padding: 0 28px;
  margin-bottom: 24px;
  border-radius: 22px;
  background: white;
  box-shadow: 0 12px 32px rgba(15, 23, 42, 0.08);
  display: flex;
  align-items: center;
  justify-content: space-between;
}

.brand {
  display: flex;
  align-items: center;
  gap: 12px;
  font-size: 24px;
  font-weight: 900;
  cursor: pointer;
}

.logo {
  width: 48px;
  height: 48px;
  border-radius: 14px;
  background: #111;
  display: grid;
  place-items: center;
  font-size: 23px;
}

.topbar button {
  border: none;
  padding: 12px 18px;
  border-radius: 13px;
  background: #111;
  color: white;
  font-weight: 800;
  cursor: pointer;
}

.detail-card {
  max-width: 960px;
  margin: 0 auto;
  padding: 42px;
  border-radius: 28px;
  background: white;
  box-shadow: 0 24px 60px rgba(15, 23, 42, 0.1);
}

.hero-row {
  display: flex;
  gap: 28px;
  align-items: flex-start;
}

.cover-image {
  width: 140px;
  height: 200px;
  border-radius: 14px;
  object-fit: cover;
  flex-shrink: 0;
  box-shadow: 0 10px 26px rgba(15, 23, 42, 0.15);
}

.description {
  margin-top: 30px;
  padding: 22px;
  border-radius: 18px;
  background: #f8fafc;
  border: 1px solid #e5e7eb;
}

.description h2 {
  margin: 0 0 10px;
  font-size: 18px;
  color: #0f172a;
}

.description p {
  margin: 0;
  color: #475569;
  line-height: 1.6;
}

.info-grid {
  margin-top: 24px;
  display: grid;
  grid-template-columns: repeat(2, 1fr);
  gap: 18px;
}

.message {
  margin-top: 24px;
  font-weight: 800;
  color: #166534;
}

.actions {
  display: flex;
  gap: 14px;
  margin-top: 34px;
}

.borrow-btn,
.secondary-btn {
  height: 58px;
  padding: 0 22px;
  border: none;
  border-radius: 15px;
  font-size: 17px;
  font-weight: 900;
  cursor: pointer;
}

.borrow-btn {
  background: #111;
  color: white;
}

.borrow-btn:disabled {
  opacity: 0.55;
  cursor: not-allowed;
}

.secondary-btn {
  background: #f1f5f9;
  color: #334155;
}

.loan-history {
  margin-top: 34px;
  padding-top: 28px;
  border-top: 1px solid #e5e7eb;
}

.loan-history h2 {
  margin: 0 0 6px;
  color: #0f172a;
  font-size: 22px;
}

.loan-history-sub {
  margin: 0 0 18px;
  color: #64748b;
  font-weight: 700;
}

.table-card {
  border-radius: 18px;
  border: 1px solid #e5e7eb;
  overflow: hidden;
}

.table-card table {
  width: 100%;
  border-collapse: collapse;
}

.table-card th,
.table-card td {
  padding: 14px 16px;
  text-align: left;
  border-bottom: 1px solid #e5e7eb;
}

.table-card th {
  background: #f8fafc;
  color: #475569;
  font-size: 12px;
  font-weight: 900;
  text-transform: uppercase;
}

.table-card td {
  color: #0f172a;
  font-weight: 700;
}

.table-card tr:last-child td {
  border-bottom: none;
}

.status-badge {
  padding: 5px 11px;
  border-radius: 999px;
  font-size: 11px;
  font-weight: 900;
}

.status-badge.active {
  background: #fef3c7;
  color: #92400e;
}

.status-badge.returned {
  background: #dcfce7;
  color: #166534;
}

.status-badge.damaged {
  background: #fee2e2;
  color: #991b1b;
}

.status-badge.lost {
  background: #f3f4f6;
  color: #374151;
}

.empty {
  padding: 20px;
  color: #64748b;
  font-weight: 700;
}

@media (max-width: 750px) {
  .hero-row {
    flex-direction: column;
  }

  .cover-image {
    width: 120px;
    height: 172px;
  }

  .info-grid {
    grid-template-columns: 1fr;
  }

  .actions {
    flex-direction: column;
  }

  .table-card {
    overflow-x: auto;
  }

  .table-card table {
    min-width: 600px;
  }
}
</style>