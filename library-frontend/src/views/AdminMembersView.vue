<template>
  <div class="page">
    <AppTopbar />

    <main class="content">
      <section class="header">
        <div>
          <p class="eyebrow">Admin Management</p>
          <h1>Member Management</h1>
          <p>View, update and delete registered library members.</p>
        </div>

        <button class="back-btn" @click="goAdmin">
          Back to Dashboard
        </button>
      </section>

      <p v-if="message" :class="['message', messageType]">
        {{ message }}
      </p>

      <section v-if="isEditing" class="form-card">
        <div class="form-header">
          <h2>Update Member</h2>
          <p>Edit selected member information.</p>
        </div>

        <form class="member-form" @submit.prevent="updateMember">
          <div class="form-group">
            <label>First Name</label>
            <input
              v-model="form.firstName"
              type="text"
              placeholder="First name"
            />
          </div>

          <div class="form-group">
            <label>Last Name</label>
            <input
              v-model="form.lastName"
              type="text"
              placeholder="Last name"
            />
          </div>

          <div class="form-group">
            <label>Email</label>
            <input
              v-model="form.email"
              type="email"
              placeholder="Email"
            />
          </div>

          <div class="form-actions">
            <button class="primary-btn" type="submit">
              Save Changes
            </button>

            <button class="secondary-btn" type="button" @click="cancelEdit">
              Cancel Edit
            </button>
          </div>
        </form>
      </section>

      <section class="summary-card">
        <div>
          <p>Total Members</p>
          <h2>{{ members.length }}</h2>
        </div>

        <span>👥</span>
      </section>

      <section class="table-section">
        <div class="table-header">
          <div>
            <h2>Members</h2>
            <p>{{ members.length }} members found in the system.</p>
          </div>

          <input
            v-model="search"
            class="search"
            type="text"
            placeholder="Search members..."
          />
        </div>

        <div class="table-card">
          <table>
            <thead>
              <tr>
                <th>Member</th>
                <th>Email</th>
                <th>Member ID</th>
                <th>User ID</th>
                <th class="actions-column">Actions</th>
              </tr>
            </thead>

            <tbody>
              <tr v-for="member in filteredMembers" :key="member.id">
                <td>
                  <strong>{{ getMemberName(member) }}</strong>
                </td>

                <td>{{ member.email || "-" }}</td>
                <td>{{ member.id }}</td>
                <td>{{ member.userId || "-" }}</td>

               <td class="actions-column">
                  <button class="view-btn" @click="viewMember(member.id)">
                    View
                  </button>

                  <button class="edit-btn" @click="startEdit(member)">
                    Edit
                  </button>

                  <button class="delete-btn" @click="deleteMember(member.id)">
                    Delete
                  </button>
                </td>
              </tr>
            </tbody>
          </table>

          <p v-if="filteredMembers.length === 0" class="empty">
            No members found.
          </p>
        </div>
      </section>
    </main>
  </div>
</template>

<script setup>
import { computed, onMounted, reactive, ref } from "vue";
import { useRouter } from "vue-router";
import axios from "axios";

import AppTopbar from "@/components/AppTopbar.vue";

const router = useRouter();

const API_BASE_URL = "http://localhost:5239";

const members = ref([]);
const search = ref("");
const message = ref("");
const messageType = ref("success");

const isEditing = ref(false);
const editingMemberId = ref(null);

const form = reactive({
  firstName: "",
  lastName: "",
  email: "",
  userId: null,
});

const getAuthHeaders = () => {
  const token = localStorage.getItem("token");

  return {
    Authorization: `Bearer ${token}`,
  };
};

const showMessage = (text, type = "success") => {
  message.value = text;
  messageType.value = type;
};

const getMembers = async () => {
  try {
    message.value = "";

    const response = await axios.get(`${API_BASE_URL}/api/members`, {
      headers: getAuthHeaders(),
    });

    members.value = response.data.data || response.data || [];
  } catch (error) {
    console.error("Members load error:", error);
    members.value = [];

    if (error.response?.status === 401) {
      showMessage("Please login as admin.", "error");
    } else if (error.response?.status === 403) {
      showMessage("You are not authorized. Please login with an admin account.", "error");
    } else {
      showMessage("Members could not be loaded.", "error");
    }
  }
};

const getMemberName = (member) => {
  const name = `${member.firstName || ""} ${member.lastName || ""}`.trim();

  return name || member.username || member.email || "-";
};

const filteredMembers = computed(() => {
  const value = search.value.toLowerCase();

  return members.value.filter((member) => {
    const name = getMemberName(member).toLowerCase();
    const email = member.email?.toLowerCase() || "";
    const id = String(member.id || "");
    const userId = String(member.userId || "");

    return (
      name.includes(value) ||
      email.includes(value) ||
      id.includes(value) ||
      userId.includes(value)
    );
  });
});

const startEdit = (member) => {
  message.value = "";
  messageType.value = "success";

  isEditing.value = true;
  editingMemberId.value = member.id;

  form.firstName = member.firstName || "";
  form.lastName = member.lastName || "";
  form.email = member.email || "";
  form.userId = member.userId || null;

  window.scrollTo({
    top: 0,
    behavior: "smooth",
  });
};

const cancelEdit = () => {
  isEditing.value = false;
  editingMemberId.value = null;

  form.firstName = "";
  form.lastName = "";
  form.email = "";
  form.userId = null;

  message.value = "";
};

const updateMember = async () => {
  if (!form.firstName || !form.lastName || !form.email) {
    showMessage("Please fill in first name, last name and email.", "error");
    return;
  }

  try {
    await axios.put(
      `${API_BASE_URL}/api/members/${editingMemberId.value}`,
      {
        id: editingMemberId.value,
        firstName: form.firstName,
        lastName: form.lastName,
        email: form.email,
        userId: form.userId,
      },
      {
        headers: getAuthHeaders(),
      }
    );

    showMessage("Member updated successfully.", "success");

    cancelEdit();
    await getMembers();
  } catch (error) {
    console.error("Member update error:", error);

    const errorMessage =
      error.response?.data?.message ||
      error.response?.data ||
      "Member update failed.";

    showMessage(errorMessage, "error");
  }
};

const deleteMember = async (memberId) => {
  const confirmed = confirm(
    "Are you sure you want to delete this member? Members with loan records may not be deleted."
  );

  if (!confirmed) return;

  try {
    await axios.delete(`${API_BASE_URL}/api/members/${memberId}`, {
      headers: getAuthHeaders(),
    });

    showMessage("Member deleted successfully.", "success");

    await getMembers();
  } catch (error) {
    console.error("Member delete error:", error);

    const errorMessage =
      error.response?.data?.message ||
      error.response?.data ||
      "Member delete failed. This member may have loan records.";

    showMessage(errorMessage, "error");
  }
};

const goAdmin = () => {
  router.push("/admin");
};
const viewMember = (id) => {
  router.push(`/admin/members/${id}`);
};

onMounted(getMembers);
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

.message {
  padding: 16px 20px;
  margin-bottom: 20px;
  border-radius: 16px;
  font-weight: 900;
}

.message.success {
  background: #dcfce7;
  color: #166534;
}

.message.error {
  background: #fee2e2;
  color: #991b1b;
}

.summary-card,
.form-card,
.table-section {
  padding: 28px;
  margin-bottom: 24px;
  border-radius: 28px;
  background: white;
  border: 1px solid #e5e7eb;
  box-shadow: 0 18px 45px rgba(15, 23, 42, 0.08);
}

.summary-card {
  display: flex;
  justify-content: space-between;
  align-items: center;
}

.summary-card p {
  margin: 0 0 8px;
  color: #64748b;
  font-weight: 900;
}

.summary-card h2 {
  margin: 0;
  font-size: 46px;
  color: #0f172a;
}

.summary-card span {
  width: 70px;
  height: 70px;
  border-radius: 22px;
  background: #ecfdf5;
  display: grid;
  place-items: center;
  font-size: 34px;
}

.form-header {
  padding-bottom: 20px;
  margin-bottom: 22px;
  border-bottom: 1px solid #e5e7eb;
}

.form-header h2,
.table-header h2 {
  margin: 0;
  color: #0f172a;
  font-size: 26px;
}

.form-header p,
.table-header p {
  margin: 7px 0 0;
  color: #64748b;
  font-weight: 700;
}

.member-form {
  display: grid;
  grid-template-columns: repeat(2, 1fr);
  gap: 18px;
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

.form-group input {
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

.form-group input:focus {
  border-color: #166534;
  background: white;
}

.form-actions {
  display: flex;
  gap: 12px;
  align-items: end;
}

.primary-btn,
.secondary-btn {
  height: 52px;
  padding: 0 18px;
  border: none;
  border-radius: 15px;
  font-weight: 900;
  cursor: pointer;
}

.primary-btn {
  background: #166534;
  color: white;
}

.secondary-btn {
  background: #f1f5f9;
  color: #334155;
}

.table-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  gap: 18px;
  margin-bottom: 18px;
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

tr:last-child td {
  border-bottom: none;
}

.actions-column {
  text-align: right;
  white-space: nowrap;
}
.view-btn {
  height: 38px;
  padding: 0 13px;
  border: none;
  border-radius: 12px;
  font-weight: 900;
  cursor: pointer;
  background: #e0e7ff;
  color: #3730a3;
  margin-right: 8px;
}

.edit-btn,
.delete-btn {
  height: 38px;
  padding: 0 13px;
  border: none;
  border-radius: 12px;
  font-weight: 900;
  cursor: pointer;
}

.edit-btn {
  background: #f1f5f9;
  color: #334155;
  margin-right: 8px;
}

.delete-btn {
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

  .member-form {
    grid-template-columns: 1fr;
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

  .form-actions {
    flex-direction: column;
    align-items: stretch;
  }
}
</style>