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

      <p v-if="issueMessage" :class="['message', issueMessageType]">
        {{ issueMessage }}
      </p>

      <section class="issue-card">
        <div class="form-header">
          <h2>Issue a Loan</h2>
          <p>Give a book directly to a member from here.</p>
        </div>

        <form class="issue-form" @submit.prevent="issueLoan">
          <div class="form-group">
            <label>Member</label>
            <select v-model.number="issueForm.memberId">
              <option disabled value="">Select member</option>

              <option
                v-for="member in members"
                :key="member.id"
                :value="member.id"
              >
                {{ member.firstName }} {{ member.lastName }}
              </option>
            </select>
          </div>

          <div class="form-group">
            <label>Book</label>
            <select v-model.number="issueForm.bookId">
              <option disabled value="">Select available book</option>

              <option
                v-for="book in availableBooks"
                :key="book.id"
                :value="book.id"
              >
                {{ book.title }}
              </option>
            </select>
          </div>

          <button class="primary-btn" type="submit" :disabled="issuing">
            {{ issuing ? "Issuing..." : "Issue Loan" }}
          </button>
        </form>
      </section>

      <section class="table-section">
        <div class="table-header">
          <div>
            <h2>All Loans</h2>
            <p>{{ loans.length }} loan records found in the system.</p>
          </div>

         <div class="search-group">
            <input
              v-model="bookSearch"
              class="search"
              type="text"
              placeholder="Search by book..."
            />

            <input
              v-model="memberSearch"
              class="search"
              type="text"
              placeholder="Search by member..."
            />
          </div>
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
                <th>Action</th>
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
                <td>
                  <button
                    v-if="!isReturned(loan)"
                    class="return-btn"
                    @click="openReturnModal(loan)"
                  >
                    Return
                  </button>
                  <span v-else class="done-label">—</span>
                </td>
              </tr>
            </tbody>
          </table>

          <p v-if="filteredLoans.length === 0" class="empty">
            No loan records found.
          </p>
        </div>
      </section>
      <div v-if="returnModalLoan" class="modal-overlay" @click.self="closeReturnModal">
        <div class="modal-card">
          <h2>Return Book</h2>
          <p class="modal-sub">
            <strong>{{ returnModalLoan.bookTitle }}</strong> — borrowed by {{ returnModalLoan.memberName }}
          </p>

          <div class="condition-group">
            <label
              v-for="option in conditionOptions"
              :key="option.value"
              :class="['condition-option', { selected: returnForm.condition === option.value }]"
            >
              <input
                type="radio"
                name="condition"
                :value="option.value"
                v-model="returnForm.condition"
              />
              <span>{{ option.label }}</span>
            </label>
          </div>

          <div class="form-group">
            <label>Note (optional)</label>
            <textarea
              v-model="returnForm.note"
              rows="3"
              placeholder="e.g. torn cover, missing pages..."
            ></textarea>
          </div>

          <p v-if="returnMessage" class="modal-message">{{ returnMessage }}</p>

          <div class="modal-actions">
            <button class="secondary-btn" type="button" @click="closeReturnModal">
              Cancel
            </button>
            <button class="primary-btn" type="button" :disabled="returning" @click="confirmReturn">
              {{ returning ? "Processing..." : "Confirm Return" }}
            </button>
          </div>
        </div>
      </div>
    </main>
  </div>
</template>

<script setup>
import { computed, onMounted, reactive, ref } from "vue";
import { useRoute, useRouter } from "vue-router";
import axios from "axios";

import AppTopbar from "@/components/AppTopbar.vue";

const route = useRoute();
const router = useRouter();
const API_BASE_URL = "http://localhost:5239";

const loans = ref([]);
const members = ref([]);
const books = ref([]);
const bookSearch = ref("");
const memberSearch = ref("");

const issuing = ref(false);
const issueMessage = ref("");
const issueMessageType = ref("success");

const issueForm = reactive({
  memberId: "",
  bookId: "",
});

const token = localStorage.getItem("token");

const headers = {
  Authorization: `Bearer ${token}`,
};

const normalize = (response) => {
  return response.data.data || response.data || [];
};

const availableBooks = computed(() => {
  return books.value.filter((book) => book.isAvailable);
});

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

const getMembers = async () => {
  try {
    const response = await axios.get(`${API_BASE_URL}/api/members`, {
      headers,
    });

    members.value = normalize(response);
  } catch (error) {
    console.error("Members load error:", error);
    members.value = [];
  }
};

const getBooks = async () => {
  try {
    const response = await axios.get(`${API_BASE_URL}/api/books`);
    books.value = normalize(response);
  } catch (error) {
    console.error("Books load error:", error);
    books.value = [];
  }
};

const issueLoan = async () => {
  if (!issueForm.memberId || !issueForm.bookId) {
    issueMessage.value = "Please select both a member and a book.";
    issueMessageType.value = "error";
    return;
  }

  try {
    issuing.value = true;

    await axios.post(
      `${API_BASE_URL}/api/loans/borrow`,
      {
        bookId: issueForm.bookId,
        memberId: issueForm.memberId,
      },
      { headers }
    );

    issueMessage.value = "Loan issued successfully.";
    issueMessageType.value = "success";

    issueForm.memberId = "";
    issueForm.bookId = "";

    await Promise.all([getLoans(), getBooks()]);
  } catch (error) {
    console.error("Issue loan error:", error);

    const errorMessage =
      error.response?.data?.message ||
      error.response?.data ||
      "Could not issue the loan.";

    issueMessage.value = errorMessage;
    issueMessageType.value = "error";
  } finally {
    issuing.value = false;
  }
};
const returnModalLoan = ref(null);
const returning = ref(false);
const returnMessage = ref("");

const conditionOptions = [
  { value: "Good", label: "Good" },
  { value: "Damaged", label: "Damaged" },
  { value: "Lost", label: "Lost" },
];

const returnForm = reactive({
  condition: "Good",
  note: "",
});

const openReturnModal = (loan) => {
  returnModalLoan.value = loan;
  returnForm.condition = "Good";
  returnForm.note = "";
  returnMessage.value = "";
};

const closeReturnModal = () => {
  returnModalLoan.value = null;
};

const confirmReturn = async () => {
  if (!returnModalLoan.value) return;

  try {
    returning.value = true;
    returnMessage.value = "";

    await axios.post(
      `${API_BASE_URL}/api/loans/return`,
      {
        loanId: returnModalLoan.value.id,
        condition: returnForm.condition,
        conditionNote: returnForm.note || null,
      },
      { headers }
    );

    closeReturnModal();
    await Promise.all([getLoans(), getBooks()]);
  } catch (error) {
    console.error("Return error:", error);

    returnMessage.value =
      error.response?.data?.message ||
      error.response?.data ||
      "Return failed.";
  } finally {
    returning.value = false;
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
  const bookValue = bookSearch.value.toLowerCase();
  const memberValue = memberSearch.value.toLowerCase();

  return loans.value.filter((loan) => {
    const book = loan.bookTitle?.toLowerCase() || "";
    const member = loan.memberName?.toLowerCase() || "";

    return book.includes(bookValue) && member.includes(memberValue);
  });
});

const formatDate = (date) => {
  if (!date) return "-";
  return new Date(date).toLocaleDateString();
};

const goAdmin = () => {
  router.push("/admin");
};

onMounted(async () => {
  getLoans();
  getMembers();
  await getBooks();

  // BookDetailView'daki "Issue a Loan" linkinden gelindiyse, kitap
  // önceden seçili gelsin diye query param'dan okuyoruz.
  const bookIdFromQuery = Number(route.query.bookId);

  if (bookIdFromQuery && books.value.some((book) => book.id === bookIdFromQuery)) {
    issueForm.bookId = bookIdFromQuery;
  }
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

.message {
  padding: 16px 20px;
  margin-bottom: 20px;
  border-radius: 16px;
  font-weight: 800;
}

.message.success {
  background: #dcfce7;
  color: #166534;
}

.message.error {
  background: #fee2e2;
  color: #991b1b;
}

.issue-card {
  padding: 28px;
  margin-bottom: 24px;
  border-radius: 28px;
  background: white;
  border: 1px solid #e5e7eb;
  box-shadow: 0 18px 45px rgba(15, 23, 42, 0.08);
}

.form-header h2 {
  margin: 0;
  color: #0f172a;
  font-size: 24px;
}

.form-header p {
  margin: 7px 0 18px;
  color: #64748b;
  font-weight: 700;
}

.issue-form {
  display: grid;
  grid-template-columns: 1fr 1fr auto;
  gap: 16px;
  align-items: end;
}

.form-group {
  display: flex;
  flex-direction: column;
  gap: 8px;
}

.form-group label {
  color: #64748b;
  font-size: 14px;
  font-weight: 900;
}

.form-group select {
  height: 52px;
  padding: 0 16px;
  border-radius: 15px;
  border: 1.5px solid #cbd5e1;
  background: #f8fafc;
  color: #0f172a;
  font-size: 15px;
  font-weight: 700;
  outline: none;
}

.form-group select:focus {
  border-color: #166534;
  background: white;
}

.primary-btn {
  height: 52px;
  padding: 0 22px;
  border: none;
  border-radius: 15px;
  background: #166534;
  color: white;
  font-weight: 900;
  cursor: pointer;
}

.primary-btn:disabled {
  opacity: 0.6;
  cursor: not-allowed;
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

.search-group {
  display: flex;
  gap: 12px;
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

.return-btn {
  height: 38px;
  padding: 0 14px;
  border: none;
  border-radius: 12px;
  background: #111;
  color: white;
  font-weight: 900;
  cursor: pointer;
}

.done-label {
  color: #cbd5e1;
  font-weight: 900;
}

.modal-overlay {
  position: fixed;
  inset: 0;
  background: rgba(15, 23, 42, 0.45);
  display: flex;
  align-items: center;
  justify-content: center;
  z-index: 100;
  padding: 20px;
}

.modal-card {
  width: 100%;
  max-width: 460px;
  padding: 28px;
  border-radius: 24px;
  background: white;
  box-shadow: 0 30px 70px rgba(15, 23, 42, 0.25);
}

.modal-card h2 {
  margin: 0 0 6px;
  color: #0f172a;
  font-size: 22px;
}

.modal-sub {
  margin: 0 0 20px;
  color: #64748b;
  font-weight: 700;
}

.condition-group {
  display: flex;
  gap: 10px;
  margin-bottom: 18px;
}

.condition-option {
  flex: 1;
  display: flex;
  align-items: center;
  justify-content: center;
  gap: 8px;
  height: 46px;
  border-radius: 13px;
  border: 1.5px solid #cbd5e1;
  background: #f8fafc;
  font-weight: 800;
  cursor: pointer;
  color: #334155;
}

.condition-option input {
  accent-color: #166534;
}

.condition-option.selected {
  border-color: #166534;
  background: #ecfdf5;
  color: #166534;
}

.modal-card .form-group {
  margin-bottom: 18px;
}

.modal-card textarea {
  width: 100%;
  padding: 12px 14px;
  border-radius: 14px;
  border: 1.5px solid #cbd5e1;
  background: #f8fafc;
  font-family: inherit;
  font-size: 14px;
  resize: vertical;
  outline: none;
  box-sizing: border-box;
}

.modal-card textarea:focus {
  border-color: #166534;
  background: white;
}

.modal-message {
  margin: 0 0 14px;
  padding: 12px 16px;
  border-radius: 12px;
  background: #fee2e2;
  color: #991b1b;
  font-weight: 800;
}

.modal-actions {
  display: flex;
  justify-content: flex-end;
  gap: 12px;
}

.modal-actions .secondary-btn {
  height: 48px;
  padding: 0 20px;
  border: none;
  border-radius: 14px;
  background: #f1f5f9;
  color: #334155;
  font-weight: 900;
  cursor: pointer;
}

.modal-actions .primary-btn:disabled {
  opacity: 0.6;
  cursor: not-allowed;
}

@media (max-width: 900px) {
  .header,
  .table-header {
    flex-direction: column;
    align-items: flex-start;
  }

  .issue-form {
    grid-template-columns: 1fr;
  }
  .search-group {
    flex-direction: column;
    width: 100%;
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
