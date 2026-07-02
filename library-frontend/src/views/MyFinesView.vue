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
            <Card v-for="fine in unpaidFines" :key="fine.id" class="fine-card">
              <CardHeader class="card-header">
                <div>
                  <CardTitle class="book-title">
                    {{ fine.bookTitle || "Library Fine" }}
                  </CardTitle>

                  <CardDescription>
                    Fine ID: {{ fine.id }} • Loan ID: {{ fine.loanId }}
                  </CardDescription>
                </div>

                <Badge variant="destructive">Unpaid</Badge>
              </CardHeader>

              <CardContent class="fine-info">
                <div class="info-row">
                  <span>Amount</span>
                  <strong>{{ formatMoney(fine.amount) }}</strong>
                </div>

                <div class="info-row">
                  <span>Status</span>
                  <strong class="unpaid-text">Unpaid</strong>
                </div>

                <Button class="pay-btn" @click="payFine(fine.id)">
                  Pay Fine
                </Button>
              </CardContent>
            </Card>
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

          <div class="history-grid">
            <Card v-for="fine in paidFines" :key="fine.id" class="history-card">
              <CardHeader class="card-header">
                <div>
                  <CardTitle class="book-title">
                    {{ fine.bookTitle || "Library Fine" }}
                  </CardTitle>

                  <CardDescription>
                    Fine ID: {{ fine.id }} • Loan ID: {{ fine.loanId }}
                  </CardDescription>
                </div>

                <Badge variant="secondary">Paid</Badge>
              </CardHeader>

              <CardContent class="fine-info">
                <div class="info-row">
                  <span>Amount</span>
                  <strong>{{ formatMoney(fine.amount) }}</strong>
                </div>

                <div class="info-row">
                  <span>Status</span>
                  <strong class="paid-text">Paid</strong>
                </div>
              </CardContent>
            </Card>
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
import { Button } from "@/components/ui/button";
import { Badge } from "@/components/ui/badge";

import {
  Card,
  CardContent,
  CardDescription,
  CardHeader,
  CardTitle,
} from "@/components/ui/card";

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

    if (role !== "Member" && role !== "Student") {
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
  padding: 28px 32px;
  border-radius: 28px;
  background: rgba(255, 255, 255, 0.92);
  border: 1px solid rgba(226, 232, 240, 0.9);
  box-shadow: 0 18px 45px rgba(15, 23, 42, 0.08);
}

.eyebrow {
  margin: 0 0 8px;
  color: #16a34a;
  font-size: 13px;
  font-weight: 800;
  letter-spacing: 0.08em;
  text-transform: uppercase;
}

.header-card h1 {
  margin: 0;
  color: #0f172a;
  font-size: 38px;
  letter-spacing: -0.04em;
}

.header-card p {
  margin: 8px 0 0;
  color: #64748b;
  font-size: 16px;
}

.summary-wrapper {
  display: flex;
  gap: 14px;
}

.summary-box {
  min-width: 130px;
  padding: 18px 20px;
  border-radius: 22px;
  background: #fff7ed;
  border: 1px solid #fed7aa;
  text-align: center;
}

.amount-box {
  background: #f0fdf4;
  border-color: #bbf7d0;
}

.summary-number {
  display: block;
  color: #c2410c;
  font-size: 28px;
  font-weight: 900;
}

.amount-box .summary-number {
  color: #15803d;
  font-size: 24px;
}

.summary-label {
  display: block;
  margin-top: 4px;
  color: #9a3412;
  font-size: 13px;
  font-weight: 800;
}

.amount-box .summary-label {
  color: #166534;
}

.section {
  margin-bottom: 34px;
}

.section-header {
  display: flex;
  align-items: flex-end;
  justify-content: space-between;
  margin-bottom: 16px;
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

.fine-grid,
.history-grid {
  display: grid;
  grid-template-columns: repeat(2, minmax(0, 1fr));
  gap: 20px;
}

.fine-card,
.history-card {
  height: 100%;
  border-radius: 22px;
  border: 1px solid #e2e8f0;
  background: rgba(255, 255, 255, 0.96);
  box-shadow: 0 14px 35px rgba(15, 23, 42, 0.07);
  transition:
    transform 0.2s ease,
    box-shadow 0.2s ease;
}

.fine-card:hover,
.history-card:hover {
  transform: translateY(-3px);
  box-shadow: 0 20px 45px rgba(15, 23, 42, 0.11);
}

.card-header {
  display: flex;
  flex-direction: row;
  align-items: flex-start;
  justify-content: space-between;
  gap: 14px;
}

.book-title {
  color: #0f172a;
  font-size: 20px;
  line-height: 1.25;
}

.fine-info {
  display: flex;
  flex-direction: column;
  gap: 12px;
}

.info-row {
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: 16px;
  padding: 12px 14px;
  border-radius: 14px;
  background: #f8fafc;
  color: #475569;
}

.info-row span {
  font-size: 14px;
  font-weight: 700;
}

.info-row strong {
  color: #0f172a;
  font-size: 14px;
  text-align: right;
}

.unpaid-text {
  color: #b91c1c !important;
}

.paid-text {
  color: #15803d !important;
}

.pay-btn {
  width: 100%;
  margin-top: 6px;
  border-radius: 14px;
  font-weight: 800;
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

  .fine-grid,
  .history-grid {
    grid-template-columns: 1fr;
  }
}

@media (max-width: 560px) {
  .page {
    padding: 16px;
  }

  .summary-wrapper {
    flex-direction: column;
  }

  .summary-box {
    width: 100%;
  }
}
</style>