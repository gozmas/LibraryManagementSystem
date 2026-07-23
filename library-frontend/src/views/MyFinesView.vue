<template>
  <div class="page">
    <AppTopbar />

    <main class="content">
      <section class="header-card">
        <div>
          <p class="eyebrow">Member Area</p>
          <h1>My Fines</h1>
          <p>View your unpaid fines and payment history safely.</p>
        </div>

        <div class="summary-wrapper">
          <div class="summary-box">
            <span class="summary-number">{{ unpaidFines.length }}</span>
            <span class="summary-label">Unpaid Fines</span>
          </div>

          <div class="summary-box amount-box">
            <span class="summary-number">{{ formatMoney(totalUnpaidAmount) }}</span>
            <span class="summary-label">Total Debt</span>
          </div>
        </div>
      </section>

      <p v-if="loading" class="state">Loading fines...</p>
      <p v-else-if="message" class="state error">{{ message }}</p>

      <template v-else>
        <section class="section">
          <div class="section-header">
            <div>
              <h2>Unpaid Fines</h2>
              <p>Fines that still need to be paid.</p>
            </div>
          </div>

          <div v-if="unpaidFines.length" class="fine-grid">
            <article v-for="fine in unpaidFines" :key="fine.id" class="fine-card">
              <div class="fine-card-top">
                <div>
                  <h2>{{ fine.bookTitle || "Library Fine" }}</h2>
                  <p>Fine ID: {{ fine.id }} • Loan ID: {{ fine.loanId }}</p>
                </div>

                <span class="status-badge unpaid">Unpaid</span>
              </div>

              <div class="fine-details">
                <div class="detail-row">
                  <span>Reason</span>
                  <strong :class="reasonClass(fine.reason)">{{ reasonLabel(fine.reason) }}</strong>
                </div>

                <div class="detail-row">
                  <span>Amount</span>
                  <strong>{{ formatMoney(fine.amount) }}</strong>
                </div>

                <div class="detail-row">
                  <span>Status</span>
                  <strong class="unpaid-text">Unpaid</strong>
                </div>
              </div>

              <button class="pay-btn" @click="payFine(fine.id)">
                Pay Fine
              </button>
            </article>
          </div>

          <div v-else class="no-debt-card">
            <span class="icon">✅</span>

            <div>
              <h3>No unpaid fines</h3>
              <p>You do not have any active debt.</p>
            </div>
          </div>
        </section>

        <section v-if="paidFines.length" class="section">
          <div class="section-header">
            <div>
              <h2>Payment History</h2>
              <p>Fines that have already been paid.</p>
            </div>
          </div>

          <div class="fine-grid">
            <article v-for="fine in paidFines" :key="fine.id" class="fine-card">
              <div class="fine-card-top">
                <div>
                  <h2>{{ fine.bookTitle || "Library Fine" }}</h2>
                  <p>Fine ID: {{ fine.id }} • Loan ID: {{ fine.loanId }}</p>
                </div>

                <span class="status-badge paid">Paid</span>
              </div>

              <div class="fine-details">
                <div class="detail-row">
                  <span>Reason</span>
                  <strong :class="reasonClass(fine.reason)">{{ reasonLabel(fine.reason) }}</strong>
                </div>

                <div class="detail-row">
                  <span>Amount</span>
                  <strong>{{ formatMoney(fine.amount) }}</strong>
                </div>

                <div class="detail-row">
                  <span>Status</span>
                  <strong class="paid-text">Paid</strong>
                </div>
              </div>
            </article>
          </div>
        </section>
      </template>
    </main>
  </div>
</template>

<script setup>
import { computed, onMounted, ref } from "vue";
import axios from "axios";

import AppTopbar from "@/components/AppTopbar.vue";

const API_BASE_URL = "http://localhost:5239";

const fines = ref([]);
const loading = ref(false);
const message = ref("");

const token = localStorage.getItem("token");
const role = localStorage.getItem("role");

const unpaidFines = computed(() => {
  return fines.value.filter((fine) => !fine.isPaid);
});

const paidFines = computed(() => {
  return fines.value.filter((fine) => fine.isPaid);
});

const totalUnpaidAmount = computed(() => {
  return unpaidFines.value.reduce((total, fine) => {
    return total + Number(fine.amount || 0);
  }, 0);
});

const getFines = async () => {
  try {
    loading.value = true;
    message.value = "";

    if (!token) {
      message.value = "Please login to view your fines.";
      return;
    }

   if (role === "Admin") {
      message.value = "Only members can view fines.";
      return;
    }

    const response = await axios.get(`${API_BASE_URL}/api/fines/my`, {
      headers: {
        Authorization: `Bearer ${token}`,
      },
    });

    fines.value = response.data.data || response.data;
  } catch (error) {
    console.error(error);
    message.value = "Fines could not be loaded.";
  } finally {
    loading.value = false;
  }
};

const payFine = async (fineId) => {
  try {
    message.value = "";

    await axios.put(`${API_BASE_URL}/api/fines/${fineId}/pay`, null, {
      headers: {
        Authorization: `Bearer ${token}`,
      },
    });

    await getFines();
  } catch (error) {
    console.error(error);

    message.value =
      error.response?.data?.message ||
      "Fine payment failed. You may not be allowed to pay this fine.";
  }
};

const formatMoney = (amount) => {
  return `${Number(amount || 0).toLocaleString("tr-TR", {
    minimumFractionDigits: 2,
    maximumFractionDigits: 2,
  })} ₺`;
};

const reasonLabel = (reason) => {
  if (reason === "Damaged") return "Damaged Book";
  if (reason === "Lost") return "Lost Book";
  return "Late Return";
};

const reasonClass = (reason) => {
  if (reason === "Damaged") return "reason-damaged";
  if (reason === "Lost") return "reason-lost";
  return "reason-late";
};

onMounted(getFines);
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

.summary-wrapper {
  display: flex;
  gap: 14px;
}

.summary-box {
  min-width: 130px;
  padding: 18px 20px;
  border-radius: 22px;
  background: #f0fdf4;
  border: 1px solid #bbf7d0;
  text-align: center;
}

.amount-box {
  background: #fff7ed;
  border-color: #fed7aa;
}

.summary-number {
  display: block;
  color: #15803d;
  font-size: 34px;
  font-weight: 950;
}

.amount-box .summary-number {
  color: #c2410c;
  font-size: 26px;
}

.summary-label {
  display: block;
  margin-top: 4px;
  color: #166534;
  font-size: 13px;
  font-weight: 900;
}

.amount-box .summary-label {
  color: #9a3412;
}

.section {
  margin-bottom: 34px;
}

.section-header {
  display: flex;
  align-items: flex-end;
  justify-content: space-between;
  margin-bottom: 16px;
  padding-left: 24px;
}

.section-header h2 {
  margin: 0;
  color: #0f172a;
  font-size: 24px;
  letter-spacing: -0.03em;
}

.section-header p {
  margin: 6px 0 0;
  color: #64748b;
  font-size: 14px;
  font-weight: 600;
}

.fine-grid {
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(380px, 480px));
  gap: 22px;
}

.fine-card {
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

.fine-card:hover {
  transform: translateY(-4px);
  border-color: #cbd5e1;
  box-shadow: 0 24px 52px rgba(15, 23, 42, 0.12);
}

.fine-card-top {
  display: flex;
  align-items: flex-start;
  justify-content: space-between;
  gap: 18px;
  margin-bottom: 22px;
}

.fine-card-top h2 {
  margin: 0;
  color: #0f172a;
  font-size: 23px;
  line-height: 1.25;
  letter-spacing: -0.03em;
}

.fine-card-top p {
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

.status-badge.unpaid {
  background: #fee2e2;
  color: #991b1b;
  border: 1px solid #fecaca;
}

.status-badge.paid {
  background: #f1f5f9;
  color: #475569;
  border: 1px solid #cbd5e1;
}

.fine-details {
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

.unpaid-text {
  color: #b91c1c !important;
}

.paid-text {
  color: #15803d !important;
}

.reason-late {
  color: #b45309 !important;
}

.reason-damaged {
  color: #b91c1c !important;
}

.reason-lost {
  color: #6b21a8 !important;
}

.pay-btn {
  width: 100%;
  height: 48px;
  margin-top: 18px;
  border: none;
  border-radius: 16px;
  background: #111;
  color: white;
  font-size: 14px;
  font-weight: 900;
  cursor: pointer;
}

.no-debt-card {
  display: flex;
  align-items: center;
  gap: 18px;
  padding: 28px;
  border-radius: 24px;
  background: white;
  border: 1px solid #dcfce7;
  box-shadow: 0 14px 34px rgba(15, 23, 42, 0.07);
}

.icon {
  width: 58px;
  height: 58px;
  display: grid;
  place-items: center;
  flex-shrink: 0;
  border-radius: 18px;
  background: #dcfce7;
  font-size: 28px;
}

.no-debt-card h3 {
  margin: 0;
  color: #166534;
  font-size: 22px;
}

.no-debt-card p {
  margin: 6px 0 0;
  color: #64748b;
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

  .summary-wrapper {
    width: 100%;
  }

  .summary-box {
    flex: 1;
  }

  .fine-grid {
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

  .summary-wrapper {
    flex-direction: column;
  }

  .summary-box {
    width: 100%;
  }

  .fine-card {
    padding: 20px;
  }

  .fine-card-top {
    flex-direction: column;
  }

  .status-badge {
    align-self: flex-start;
  }
}
</style>