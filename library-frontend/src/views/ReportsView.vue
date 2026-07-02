<template>
  <div class="page">
    <AppTopbar />

    <main class="content">
      <div class="header">
        <h1>Reports</h1>
        <p>View borrowed books, overdue books and fine reports.</p>
      </div>

      <p v-if="loading" class="state">Loading reports...</p>
      <p v-else-if="message" class="state">{{ message }}</p>

      <template v-else>
        <section class="report-section">
          <h2>Borrowed Books</h2>

          <div v-if="borrowedBooks.length" class="table-card">
            <table>
              <thead>
                <tr>
                  <th>Book</th>
                  <th>Member</th>
                  <th>Borrow Date</th>
                  <th>Due Date</th>
                </tr>
              </thead>

              <tbody>
                <tr v-for="item in borrowedBooks" :key="item.loanId">
                  <td>{{ item.bookTitle }}</td>
                  <td>{{ item.memberName }}</td>
                  <td>{{ formatDate(item.borrowDate) }}</td>
                  <td>{{ formatDate(item.dueDate) }}</td>
                </tr>
              </tbody>
            </table>
          </div>

          <p v-else class="empty">No borrowed books found.</p>
        </section>

        <section class="report-section">
          <h2>Overdue Books</h2>

          <div v-if="overdueBooks.length" class="table-card">
            <table>
              <thead>
                <tr>
                  <th>Book</th>
                  <th>Member</th>
                  <th>Due Date</th>
                </tr>
              </thead>

              <tbody>
                <tr v-for="item in overdueBooks" :key="item.loanId">
                  <td>{{ item.bookTitle }}</td>
                  <td>{{ item.memberName }}</td>
                  <td>{{ formatDate(item.dueDate) }}</td>
                </tr>
              </tbody>
            </table>
          </div>

          <p v-else class="empty">No overdue books found.</p>
        </section>

        <section class="report-section">
          <h2>Fine Reports</h2>

          <div v-if="fines.length" class="table-card">
            <table>
              <thead>
                <tr>
                  <th>Book</th>
                  <th>Member</th>
                  <th>Amount</th>
                  <th>Status</th>
                </tr>
              </thead>

              <tbody>
                <tr v-for="fine in fines" :key="fine.fineId">
                  <td>{{ fine.bookTitle }}</td>
                  <td>{{ fine.memberName }}</td>
                  <td>{{ fine.amount }} ₺</td>
                  <td>
                    <span :class="['badge', fine.isPaid ? 'paid' : 'unpaid']">
                      {{ fine.isPaid ? "Paid" : "Unpaid" }}
                    </span>
                  </td>
                </tr>
              </tbody>
            </table>
          </div>

          <p v-else class="empty">No fines found.</p>
        </section>
      </template>
    </main>
  </div>
</template>

<script setup>
import { ref, onMounted } from "vue";
import axios from "axios";

import AppTopbar from "@/components/AppTopbar.vue";

const API_BASE_URL = "http://localhost:5239";

const borrowedBooks = ref([]);
const overdueBooks = ref([]);
const fines = ref([]);

const loading = ref(false);
const message = ref("");

const token = localStorage.getItem("token");

const safeLoad = async (request, target) => {
  try {
    const response = await request();
    target.value = response.data.data || response.data || [];
  } catch (error) {
    console.error("Report load failed:", error);
    target.value = [];
  }
};

const getReports = async () => {
  loading.value = true;
  message.value = "";

  const headers = {
    Authorization: `Bearer ${token}`,
  };

  await Promise.all([
    safeLoad(
      () => axios.get(`${API_BASE_URL}/api/reports/borrowed-books`, { headers }),
      borrowedBooks
    ),
    safeLoad(
      () => axios.get(`${API_BASE_URL}/api/reports/overdue-books`, { headers }),
      overdueBooks
    ),
    safeLoad(
      () => axios.get(`${API_BASE_URL}/api/reports/fines`, { headers }),
      fines
    ),
  ]);

  loading.value = false;
};

const formatDate = (date) => {
  if (!date) return "-";
  return new Date(date).toLocaleDateString();
};

onMounted(getReports);
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
  max-width: 1120px;
  margin: 0 auto;
}

.header {
  margin-bottom: 26px;
}

.header h1 {
  margin: 0;
  font-size: 40px;
  color: #0f172a;
}

.header p {
  margin-top: 8px;
  color: #64748b;
}

.report-section {
  margin-bottom: 28px;
}

.report-section h2 {
  margin: 0 0 14px;
  font-size: 24px;
  color: #0f172a;
}

.table-card {
  border-radius: 22px;
  background: white;
  border: 1px solid #e5e7eb;
  box-shadow: 0 14px 34px rgba(15, 23, 42, 0.07);
  overflow: hidden;
}

table {
  width: 100%;
  border-collapse: collapse;
}

th,
td {
  padding: 16px 18px;
  text-align: left;
  border-bottom: 1px solid #e5e7eb;
}

th {
  background: #f8fafc;
  color: #475569;
  font-size: 13px;
  text-transform: uppercase;
  letter-spacing: 0.04em;
}

td {
  color: #0f172a;
  font-weight: 600;
}

tr:last-child td {
  border-bottom: none;
}

.badge {
  display: inline-block;
  padding: 6px 11px;
  border-radius: 999px;
  font-size: 12px;
  font-weight: 900;
}

.paid {
  background: #dcfce7;
  color: #166534;
}

.unpaid {
  background: #fee2e2;
  color: #991b1b;
}

.state,
.empty {
  padding: 22px;
  border-radius: 18px;
  background: white;
  color: #64748b;
  font-weight: 700;
}

@media (max-width: 760px) {
  .table-card {
    overflow-x: auto;
  }

  table {
    min-width: 720px;
  }
}
</style>