<template>
  <div class="page">
    <AppTopbar />

    <main class="content">
      <section class="header">
        <div>
          <p class="eyebrow">Member Detail</p>
          <h1>{{ memberName }}</h1>
          <p>{{ member?.email || "-" }}</p>
        </div>

        <button class="back-btn" @click="goBack">
          Back to Members
        </button>
      </section>

      <p v-if="message" :class="['message', 'error']">
        {{ message }}
      </p>

      <section class="summary-row">
        <div class="summary-card">
          <p>Member ID</p>
          <h2>{{ member?.id ?? "-" }}</h2>
        </div>
        <div class="summary-card">
          <p>Total Loans</p>
          <h2>{{ loans.length }}</h2>
        </div>
        <div class="summary-card">
          <p>Active Loans</p>
          <h2>{{ activeLoansCount }}</h2>
        </div>
      </section>

      <section class="table-section">
        <div class="table-header">
          <div>
            <h2>Loan History</h2>
            <p>Books and editions borrowed by this member.</p>
          </div>
        </div>

        <div class="table-card">
          <table>
            <thead>
              <tr>
                <th>Book</th>
                <th>Copy #</th>
                <th>Borrow Date</th>
                <th>Return Date</th>
                <th>Status</th>
              </tr>
            </thead>

            <tbody>
              <tr v-for="loan in loans" :key="loan.id">
                <td><strong>{{ loan.bookTitle }}</strong></td>
                <td>#{{ loan.copyNumber }}</td>
                <td>{{ formatDate(loan.borrowDate) }}</td>
                <td>{{ loan.returnDate ? formatDate(loan.returnDate) : "-" }}</td>
                <td>
                  <span :class="['status-badge', statusClass(loan.copyStatus, loan.isReturned)]">
                    {{ loan.isReturned ? loan.copyStatus : "Active" }}
                  </span>
                </td>
              </tr>
            </tbody>
          </table>

          <p v-if="loans.length === 0" class="empty">
            This member has no loan history yet.
          </p>
        </div>
      </section>
    </main>
  </div>
</template>

<script setup>
import { computed, onMounted, ref } from "vue";
import { useRoute, useRouter } from "vue-router";
import axios from "axios";

import AppTopbar from "@/components/AppTopbar.vue";

const route = useRoute();
const router = useRouter();

const API_BASE_URL = "http://localhost:5239";

const member = ref(null);
const loans = ref([]);
const message = ref("");

const memberName = computed(() => {
  if (!member.value) return "Member";
  const name = `${member.value.firstName || ""} ${member.value.lastName || ""}`.trim();
  return name || "Member";
});

const activeLoansCount = computed(
  () => loans.value.filter((loan) => !loan.isReturned).length
);

const getAuthHeaders = () => {
  const token = localStorage.getItem("token");
  return { Authorization: `Bearer ${token}` };
};

const formatDate = (value) => {
  if (!value) return "-";
  return new Date(value).toLocaleDateString();
};
const statusClass = (copyStatus, isReturned) => {
  if (!isReturned) return "active";
  if (copyStatus === "Damaged") return "damaged";
  if (copyStatus === "Lost") return "lost";
  return "returned";
};

const getMember = async () => {
  try {
    const response = await axios.get(
      `${API_BASE_URL}/api/members/${route.params.id}`,
      { headers: getAuthHeaders() }
    );
    member.value = response.data.data || response.data;
  } catch (error) {
    console.error("Member load error:", error);
    message.value = "Member could not be loaded.";
  }
};

const getMemberLoans = async () => {
  try {
    const response = await axios.get(
      `${API_BASE_URL}/api/loans/by-member/${route.params.id}`,
      { headers: getAuthHeaders() }
    );
    loans.value = response.data.data || response.data || [];
  } catch (error) {
    console.error("Member loans load error:", error);
    message.value = "Loan history could not be loaded.";
  }
};

const goBack = () => {
  router.push("/admin/members");
};

onMounted(async () => {
  await getMember();
  await getMemberLoans();
});
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
  font-size: 38px;
  color: #0f172a;
}

.header p {
  margin: 10px 0 0;
  color: #64748b;
  font-size: 16px;
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

.message.error {
  padding: 16px 20px;
  margin-bottom: 20px;
  border-radius: 16px;
  font-weight: 900;
  background: #fee2e2;
  color: #991b1b;
}

.summary-row {
  display: grid;
  grid-template-columns: repeat(3, 1fr);
  gap: 18px;
  margin-bottom: 24px;
}

.summary-card {
  padding: 24px;
  border-radius: 22px;
  background: white;
  border: 1px solid #e5e7eb;
  box-shadow: 0 12px 32px rgba(15, 23, 42, 0.06);
}

.summary-card p {
  margin: 0 0 8px;
  color: #64748b;
  font-weight: 900;
  font-size: 13px;
  text-transform: uppercase;
}

.summary-card h2 {
  margin: 0;
  font-size: 34px;
  color: #0f172a;
}

.table-section {
  padding: 28px;
  border-radius: 28px;
  background: white;
  border: 1px solid #e5e7eb;
  box-shadow: 0 18px 45px rgba(15, 23, 42, 0.08);
}

.table-header h2 {
  margin: 0;
  color: #0f172a;
  font-size: 26px;
}

.table-header p {
  margin: 7px 0 18px;
  color: #64748b;
  font-weight: 700;
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

tr:last-child td {
  border-bottom: none;
}

.status-badge {
  padding: 6px 12px;
  border-radius: 999px;
  font-size: 12px;
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
  padding: 24px;
  color: #64748b;
  font-weight: 700;
}

@media (max-width: 900px) {
  .summary-row {
    grid-template-columns: 1fr;
  }

  .table-card {
    overflow-x: auto;
  }

  table {
    min-width: 700px;
  }
}
</style>