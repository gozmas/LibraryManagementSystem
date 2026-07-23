<template>
  <div class="page">
    <AppTopbar />

    <main class="content">
      <section class="header-card">
        <div>
          <p class="eyebrow">Member Area</p>
          <h1>My Loans</h1>
          <p>View your borrowed books and return active loans safely.</p>
        </div>

        <div class="summary-box">
          <span class="summary-number">{{ loans.length }}</span>
          <span class="summary-label">Total Loans</span>
        </div>
      </section>

      <p v-if="loading" class="state">Loading loans...</p>
      <p v-else-if="message" class="state error">{{ message }}</p>

      <div v-else-if="loans.length" class="loan-grid">
        <article v-for="loan in loans" :key="loan.id" class="loan-card">
          <div class="loan-card-top">
            <div>
              <h2>{{ getBookTitle(loan) }}</h2>
              <p>Loan ID: {{ loan.id }}</p>
            </div>

            <span
              class="status-badge"
              :class="isLoanReturned(loan) ? 'returned' : 'active'"
            >
              {{ isLoanReturned(loan) ? "Returned" : "Active" }}
            </span>
          </div>

          <div class="loan-details">
            <div class="detail-row">
              <span>Borrow Date</span>
              <strong>{{ formatDate(loan.borrowDate) }}</strong>
            </div>

            <div class="detail-row">
              <span>Due Date</span>
              <strong>{{ formatDate(loan.dueDate) }}</strong>
            </div>

            <div class="detail-row">
              <span>Return Date</span>
              <strong>
                {{ loan.returnDate ? formatDate(loan.returnDate) : "Not returned yet" }}
              </strong>
            </div>
          </div>

          <Button
            v-if="!isLoanReturned(loan)"
            class="return-btn"
            @click="returnBook(loan.id)"
          >
            Return Book
          </Button>

          <div v-else class="returned-note">
            This book has already been returned.
          </div>
        </article>
      </div>

      <p v-else class="state">You do not have any loans yet.</p>
    </main>
  </div>
</template>

<script setup>
import { ref, onMounted } from "vue";
import axios from "axios";

import AppTopbar from "@/components/AppTopbar.vue";
import { Button } from "@/components/ui/button";

const API_BASE_URL = "http://localhost:5239";

const loans = ref([]);
const loading = ref(false);
const message = ref("");

const token = localStorage.getItem("token");
const role = localStorage.getItem("role");

const getMyLoans = async () => {
  try {
    loading.value = true;
    message.value = "";

    if (!token) {
      message.value = "Please login to view your loans.";
      return;
    }

    if (role === "Admin") {
      message.value = "Only members can view loans.";
      
      return;
    }

    const response = await axios.get(`${API_BASE_URL}/api/loans/my`, {
      headers: {
        Authorization: `Bearer ${token}`,
      },
    });

    loans.value = response.data.data || response.data;
  } catch (error) {
    console.error(error);
    message.value = "Loans could not be loaded.";
  } finally {
    loading.value = false;
  }
};

const returnBook = async (loanId) => {
  try {
    message.value = "";

    await axios.post(
      `${API_BASE_URL}/api/loans/return`,
      {
        loanId: loanId,
      },
      {
        headers: {
          Authorization: `Bearer ${token}`,
        },
      }
    );

    await getMyLoans();
  } catch (error) {
    console.error(error);

    message.value =
      error.response?.data?.message ||
      "Return book failed. You may not be allowed to return this loan.";
  }
};

const isLoanReturned = (loan) => {
  return loan.isReturned === true || !!loan.returnDate;
};

const getBookTitle = (loan) => {
  return loan.bookTitle || loan.book?.title || "Unknown Book";
};

const formatDate = (date) => {
  if (!date) return "-";

  return new Date(date).toLocaleDateString("tr-TR", {
    day: "2-digit",
    month: "short",
    year: "numeric",
  });
};

onMounted(getMyLoans);
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

.header-card {
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: 24px;
  margin-bottom: 28px;
  padding: 30px 34px;
  border-radius: 28px;
  background: rgba(255, 255, 255, 0.94);
  border: 1px solid #e2e8f0;
  box-shadow: 0 18px 45px rgba(15, 23, 42, 0.08);
}

.eyebrow {
  margin: 0 0 8px;
  color: #64748b;
  font-size: 13px;
  font-weight: 900;
  letter-spacing: 0.1em;
  text-transform: uppercase;
}

.header-card h1 {
  margin: 0;
  color: #0f172a;
  font-size: 38px;
  letter-spacing: -0.04em;
}

.header-card p {
  margin: 10px 0 0;
  color: #64748b;
  font-size: 16px;
  font-weight: 600;
}

.summary-box {
  min-width: 130px;
  padding: 18px 20px;
  border-radius: 22px;
  background: #f0fdf4;
  border: 1px solid #bbf7d0;
  text-align: center;
}

.summary-number {
  display: block;
  color: #15803d;
  font-size: 34px;
  font-weight: 950;
}

.summary-label {
  display: block;
  margin-top: 4px;
  color: #166534;
  font-size: 13px;
  font-weight: 900;
}

.loan-grid {
  display: grid;
  grid-template-columns: repeat(2, minmax(0, 1fr));
  gap: 22px;
}

.loan-card {
  padding: 24px;
  border-radius: 26px;
  background: rgba(255, 255, 255, 0.96);
  border: 1px solid #e2e8f0;
  box-shadow: 0 16px 38px rgba(15, 23, 42, 0.08);
  transition:
    transform 0.2s ease,
    box-shadow 0.2s ease,
    border-color 0.2s ease;
}

.loan-card:hover {
  transform: translateY(-4px);
  border-color: #cbd5e1;
  box-shadow: 0 24px 52px rgba(15, 23, 42, 0.12);
}

.loan-card-top {
  display: flex;
  align-items: flex-start;
  justify-content: space-between;
  gap: 18px;
  margin-bottom: 22px;
}

.loan-card-top h2 {
  margin: 0;
  color: #0f172a;
  font-size: 23px;
  line-height: 1.25;
  letter-spacing: -0.03em;
}

.loan-card-top p {
  margin: 6px 0 0;
  color: #64748b;
  font-size: 14px;
  font-weight: 700;
}

.status-badge {
  flex-shrink: 0;
  padding: 7px 12px;
  border-radius: 999px;
  font-size: 12px;
  font-weight: 900;
  line-height: 1;
}

.status-badge.active {
  background: #dcfce7;
  color: #166534;
  border: 1px solid #bbf7d0;
}

.status-badge.returned {
  background: #f1f5f9;
  color: #475569;
  border: 1px solid #cbd5e1;
}

.loan-details {
  display: flex;
  flex-direction: column;
  gap: 12px;
}

.detail-row {
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: 18px;
  padding: 14px 16px;
  border-radius: 16px;
  background: #f8fafc;
}

.detail-row span {
  color: #475569;
  font-size: 14px;
  font-weight: 800;
}

.detail-row strong {
  color: #0f172a;
  font-size: 14px;
  font-weight: 900;
  text-align: right;
}

.return-btn {
  width: 100%;
  height: 48px;
  margin-top: 18px;
  border-radius: 16px;
  font-size: 14px;
  font-weight: 900;
  cursor: pointer;
}

.returned-note {
  margin-top: 18px;
  padding: 14px 16px;
  border-radius: 16px;
  background: #f1f5f9;
  color: #64748b;
  font-size: 14px;
  font-weight: 800;
  text-align: center;
}

.state {
  padding: 24px;
  border-radius: 20px;
  background: white;
  color: #64748b;
  font-weight: 800;
  box-shadow: 0 14px 35px rgba(15, 23, 42, 0.06);
}

.error {
  color: #b91c1c;
  background: #fff1f2;
}

@media (max-width: 860px) {
  .header-card {
    flex-direction: column;
    align-items: flex-start;
  }

  .summary-box {
    width: 100%;
  }

  .loan-grid {
    grid-template-columns: 1fr;
  }
}

@media (max-width: 560px) {
  .page {
    padding: 16px;
  }

  .header-card {
    padding: 24px;
  }

  .loan-card {
    padding: 20px;
  }

  .loan-card-top {
    flex-direction: column;
  }

  .status-badge {
    align-self: flex-start;
  }
}
</style>