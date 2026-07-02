<template>
  <div class="page">
    <AppTopbar />

    <main class="content">
      <section class="header">
        <div>
          <p class="eyebrow">Admin Management</p>
          <h1>Loan Management</h1>
          <p>View which books are borrowed, who borrowed them, and due dates.</p>
        </div>

        <button class="back-btn" @click="goAdmin">
          Back to Dashboard
        </button>
      </section>

      <section v-if="overdueLoans.length" class="alert-card">
        <span>⚠️</span>

        <div>
          <h2>{{ overdueLoans.length }} overdue loan found</h2>
          <p>These books passed their due dates and should be returned.</p>
        </div>
      </section>

      <section class="table-section">
        <div class="table-header">
          <div>
            <h2>All Loans</h2>
            <p>{{ loans.length }} loan records found in the system.</p>
          </div>

          <input
            v-model="search"
            class="search"
            type="text"
            placeholder="Search by book or member..."
          />
        </div>

        <div class="table-card">
          <table>
            <thead>
              <tr>
                <th>Book</th>
                <th>Member</th>
                <th>Borrow Date</th>
                <th>Due Date</th>
                <th>Return Date</th>
                <th>Status</th>
              </tr>
            </thead>

            <tbody>
              <tr
                v-for="loan in filteredLoans"
                :key="loan.id"
                :class="{ overdueRow: isOverdue(loan) }"
              >
                <td>
                  <strong>{{ loan.bookTitle }}</strong>
                  <small>Loan ID: {{ loan.id }}</small>
                </td>

                <td>{{ loan.memberName }}</td>
                <td>{{ formatDate(loan.borrowDate) }}</td>
                <td>{{ formatDate(loan.dueDate) }}</td>
                <td>{{ loan.returnDate ? formatDate(loan.returnDate) : "-" }}</td>

                <td>
                 <span
  v-if="isReturned(loan)"
  class="badge returned"
>
  Returned
</span>

                  <span
                    v-else-if="isOverdue(loan)"
                    class="badge overdue"
                  >
                    Overdue
                  </span>

                  <span
                    v-else
                    class="badge active"
                  >
                    Active
                  </span>
                </td>
              </tr>
            </tbody>
          </table>

          <p v-if="filteredLoans.length === 0" class="empty">
            No loan records found.
          </p>
        </div>
      </section>
    </main>
  </div>
</template>

<script setup>
import { computed, onMounted, ref } from "vue";
import { useRouter } from "vue-router";
import axios from "axios";

import AppTopbar from "@/components/AppTopbar.vue";

const router = useRouter();
const API_BASE_URL = "http://localhost:5239";

const loans = ref([]);
const search = ref("");

const token = localStorage.getItem("token");

const headers = {
  Authorization: `Bearer ${token}`,
};

const getLoans = async () => {
  try {
    const response = await axios.get(`${API_BASE_URL}/api/loans`, {
      headers,
    });

    loans.value = response.data.data || response.data || [];
  } catch (error) {
    console.error("Admin loans load error:", error);
    loans.value = [];
  }
};

const isOverdue = (loan) => {
  if (isReturned(loan)) return false;

  const today = new Date();
  const dueDate = new Date(loan.dueDate);

  return dueDate < today;
};
const isReturned = (loan) => {
  return loan.isReturned || loan.returnDate;
};

const overdueLoans = computed(() => {
  return loans.value.filter((loan) => isOverdue(loan));
});

const filteredLoans = computed(() => {
  const value = search.value.toLowerCase();

  return loans.value.filter((loan) => {
    const book = loan.bookTitle?.toLowerCase() || "";
    const member = loan.memberName?.toLowerCase() || "";

    return book.includes(value) || member.includes(value);
  });
});

const formatDate = (date) => {
  if (!date) return "-";
  return new Date(date).toLocaleDateString();
};

const goAdmin = () => {
  router.push("/admin");
};

onMounted(getLoans);
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

.header {
  padding: 32px;
  margin-bottom: 24px;
  border-radius: 28px;
  background: white;
  box-shadow: 0 18px 45px rgba(15, 23, 42, 0.08);
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: 24px;
}

.eyebrow {
  margin: 0 0 8px;
  color: #166534;
  font-weight: 900;
}

.header h1 {
  margin: 0;
  font-size: 42px;
  color: #0f172a;
}

.header p {
  margin: 10px 0 0;
  color: #64748b;
  font-size: 17px;
}

.back-btn {
  height: 48px;
  padding: 0 18px;
  border: none;
  border-radius: 15px;
  background: #111;
  color: white;
  font-weight: 900;
  cursor: pointer;
}

.alert-card {
  padding: 24px;
  margin-bottom: 24px;
  border-radius: 24px;
  background: #fee2e2;
  color: #991b1b;
  display: flex;
  align-items: center;
  gap: 18px;
  box-shadow: 0 14px 34px rgba(15, 23, 42, 0.07);
}

.alert-card span {
  font-size: 34px;
}

.alert-card h2 {
  margin: 0;
}

.alert-card p {
  margin: 6px 0 0;
  font-weight: 700;
}

.table-section {
  padding: 28px;
  border-radius: 28px;
  background: white;
  border: 1px solid #e5e7eb;
  box-shadow: 0 18px 45px rgba(15, 23, 42, 0.08);
}

.table-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  gap: 18px;
  margin-bottom: 18px;
}

.table-header h2 {
  margin: 0;
  color: #0f172a;
  font-size: 26px;
}

.table-header p {
  margin: 7px 0 0;
  color: #64748b;
  font-weight: 700;
}

.search {
  width: 320px;
  height: 46px;
  padding: 0 15px;
  border-radius: 14px;
  border: 1.5px solid #cbd5e1;
  background: #f8fafc;
  font-weight: 700;
  outline: none;
}

.search:focus {
  border-color: #166534;
  background: white;
}

.table-card {
  border-radius: 22px;
  border: 1px solid #e5e7eb;
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
  font-weight: 900;
  text-transform: uppercase;
}

td {
  color: #0f172a;
  font-weight: 700;
}

td small {
  display: block;
  margin-top: 4px;
  color: #64748b;
  font-weight: 600;
}

tr:last-child td {
  border-bottom: none;
}

.overdueRow {
  background: #fff7f7;
}

.badge {
  padding: 6px 11px;
  border-radius: 999px;
  font-size: 12px;
  font-weight: 900;
}

.badge.active {
  background: #dbeafe;
  color: #1d4ed8;
}

.badge.returned {
  background: #dcfce7;
  color: #166534;
}

.badge.overdue {
  background: #fee2e2;
  color: #991b1b;
}

.empty {
  padding: 24px;
  color: #64748b;
  font-weight: 700;
}

@media (max-width: 900px) {
  .header,
  .table-header {
    flex-direction: column;
    align-items: flex-start;
  }

  .search {
    width: 100%;
  }

  .table-card {
    overflow-x: auto;
  }

  table {
    min-width: 850px;
  }
}
</style>