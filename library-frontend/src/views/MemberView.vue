<template>
  <div class="page">
    <AppTopbar />

    <main class="content">
      <section class="member-hero">
        <div>
          <p class="eyebrow">Member Dashboard</p>
          <h1>Welcome back, {{ displayName }}</h1>
          <p>Track your borrowed books, fines and profile information.</p>
        </div>

        <div class="member-box">
          <span>👤</span>
          <div>
            <strong>{{ email }}</strong>
            <p>Library member account</p>
          </div>
        </div>
      </section>

      <section class="stats">
        <StatCard icon="📚" title="Active Loans" :value="activeLoans.length" />
        <StatCard icon="✅" title="Returned Books" :value="returnedLoans.length" />
        <StatCard icon="💰" title="Unpaid Fines" :value="unpaidFines.length" />
        <StatCard icon="🧾" title="Paid Fines" :value="paidFines.length" />
      </section>

      <section class="quick-actions">
        <div class="action-card" @click="goMyLoans">
          <span>📚</span>
          <div>
            <h3>My Loans</h3>
            <p>View and return borrowed books.</p>
          </div>
        </div>

        <div class="action-card" @click="goMyFines">
          <span>💰</span>
          <div>
            <h3>My Fines</h3>
            <p>View unpaid fines and payment history.</p>
          </div>
        </div>

        <div class="action-card" @click="goProfile">
          <span>👤</span>
          <div>
            <h3>Profile</h3>
            <p>View your account information.</p>
          </div>
        </div>

        <div class="action-card" @click="goHome">
          <span>🏠</span>
          <div>
            <h3>Browse Library</h3>
            <p>Search books, authors and categories.</p>
          </div>
        </div>
      </section>

      <section class="recent-section">
        <h2>Current Loans</h2>

        <div v-if="activeLoans.length" class="loan-list">
          <div
            v-for="loan in activeLoans"
            :key="loan.id"
            class="loan-card"
          >
            <div>
              <h3>{{ loan.bookTitle }}</h3>
              <p>Due Date: {{ formatDate(loan.dueDate) }}</p>
            </div>

            <button @click="goMyLoans">View</button>
          </div>
        </div>

        <p v-else class="empty">
          You do not have any active loans.
        </p>
      </section>
    </main>
  </div>
</template>

<script setup>
import { computed, onMounted, ref } from "vue";
import { useRouter } from "vue-router";
import axios from "axios";

import AppTopbar from "@/components/AppTopbar.vue";
import StatCard from "@/components/StatCard.vue";

const router = useRouter();
const API_BASE_URL = "http://localhost:5239";

const member = ref({});
const loans = ref([]);
const fines = ref([]);

const token = localStorage.getItem("token");
const email = localStorage.getItem("email") || "member@library.local";
const username = localStorage.getItem("username") || "Member";

const headers = {
  Authorization: `Bearer ${token}`,
};

const displayName = computed(() => {
  const firstName = member.value.firstName || "";
  const lastName = member.value.lastName || "";
  const fullName = `${firstName} ${lastName}`.trim();

  return fullName || username || "Member";
});

const myLoans = computed(() => {
  return loans.value.filter((loan) => {
    return (
      loan.memberId === member.value.id ||
      loan.memberName === displayName.value
    );
  });
});

const activeLoans = computed(() => {
  return myLoans.value.filter((loan) => !loan.isReturned && !loan.returnDate);
});

const returnedLoans = computed(() => {
  return myLoans.value.filter((loan) => loan.isReturned || loan.returnDate);
});

const myFines = computed(() => {
  return fines.value.filter((fine) => {
    return fine.memberName === displayName.value;
  });
});

const unpaidFines = computed(() => {
  return myFines.value.filter((fine) => !fine.isPaid);
});

const paidFines = computed(() => {
  return myFines.value.filter((fine) => fine.isPaid);
});

const getMemberData = async () => {
  try {
    const profileRes = await axios.get(`${API_BASE_URL}/api/members/me`, {
      headers,
    });

    member.value = profileRes.data.data || profileRes.data;

    const loanRes = await axios.get(`${API_BASE_URL}/api/loans`, {
      headers,
    });

    loans.value = loanRes.data.data || loanRes.data;

    const fineRes = await axios.get(`${API_BASE_URL}/api/fines`, {
      headers,
    });

    fines.value = fineRes.data.data || fineRes.data;
  } catch (error) {
    console.error("Member dashboard error:", error);
  }
};

const formatDate = (date) => {
  if (!date) return "-";
  return new Date(date).toLocaleDateString();
};

const goHome = () => router.push("/home");
const goMyLoans = () => router.push("/my-loans");
const goMyFines = () => router.push("/my-fines");
const goProfile = () => router.push("/profile");

onMounted(getMemberData);
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

.member-hero {
  padding: 34px 38px;
  border-radius: 28px;
  background: white;
  box-shadow: 0 18px 45px rgba(15, 23, 42, 0.08);
  display: flex;
  justify-content: space-between;
  align-items: center;
  gap: 24px;
  margin-bottom: 24px;
}

.eyebrow {
  margin: 0 0 8px;
  color: #166534;
  font-weight: 900;
}

.member-hero h1 {
  margin: 0;
  font-size: 42px;
  color: #0f172a;
}

.member-hero p {
  margin: 10px 0 0;
  color: #64748b;
  font-size: 17px;
}

.member-box {
  display: flex;
  align-items: center;
  gap: 16px;
  padding: 18px 20px;
  border-radius: 18px;
  background: #f8fafc;
  border: 1px solid #e2e8f0;
}

.member-box span {
  font-size: 32px;
}

.member-box strong {
  display: block;
  max-width: 260px;
  overflow: hidden;
  text-overflow: ellipsis;
  white-space: nowrap;
}

.member-box p {
  margin: 4px 0 0;
  font-size: 14px;
}

.stats {
  display: grid;
  grid-template-columns: repeat(4, 1fr);
  gap: 18px;
  margin-bottom: 24px;
}

.quick-actions {
  display: grid;
  grid-template-columns: repeat(4, 1fr);
  gap: 18px;
  margin-bottom: 28px;
}

.action-card {
  padding: 22px;
  border-radius: 24px;
  background: white;
  border: 1px solid #e5e7eb;
  box-shadow: 0 14px 34px rgba(15, 23, 42, 0.07);
  display: flex;
  gap: 16px;
  cursor: pointer;
  transition: 0.18s ease;
}

.action-card:hover {
  transform: translateY(-3px);
  box-shadow: 0 18px 42px rgba(15, 23, 42, 0.1);
}

.action-card span {
  width: 54px;
  height: 54px;
  border-radius: 16px;
  background: #ecfdf5;
  display: grid;
  place-items: center;
  font-size: 26px;
  flex-shrink: 0;
}

.action-card h3 {
  margin: 0;
  color: #0f172a;
}

.action-card p {
  margin: 7px 0 0;
  color: #64748b;
  font-size: 14px;
}

.recent-section {
  padding: 26px;
  border-radius: 24px;
  background: white;
  box-shadow: 0 14px 34px rgba(15, 23, 42, 0.07);
}

.recent-section h2 {
  margin: 0 0 18px;
  color: #0f172a;
}

.loan-list {
  display: grid;
  gap: 14px;
}

.loan-card {
  padding: 18px;
  border-radius: 18px;
  background: #f8fafc;
  border: 1px solid #e2e8f0;
  display: flex;
  justify-content: space-between;
  align-items: center;
}

.loan-card h3 {
  margin: 0;
  color: #0f172a;
}

.loan-card p {
  margin: 6px 0 0;
  color: #64748b;
}

.loan-card button {
  height: 42px;
  padding: 0 16px;
  border: none;
  border-radius: 13px;
  background: #111;
  color: white;
  font-weight: 800;
  cursor: pointer;
}

.empty {
  padding: 20px;
  border-radius: 18px;
  background: #f8fafc;
  color: #64748b;
  font-weight: 700;
}

@media (max-width: 1000px) {
  .stats,
  .quick-actions {
    grid-template-columns: 1fr 1fr;
  }

  .member-hero {
    flex-direction: column;
    align-items: flex-start;
  }
}

@media (max-width: 650px) {
  .stats,
  .quick-actions {
    grid-template-columns: 1fr;
  }

  .loan-card {
    flex-direction: column;
    align-items: flex-start;
    gap: 14px;
  }
}
</style>