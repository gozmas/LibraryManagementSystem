<template>
  <div class="page">
    <AppTopbar />

    <main class="content">
      <section class="profile-hero">
        <div class="avatar">👤</div>

        <div>
          <p class="eyebrow">Member Profile</p>
          <h1>{{ fullName }}</h1>
          <p>Update your account information and password.</p>
        </div>
      </section>

      <p v-if="loading" class="state">Loading profile...</p>

      <template v-else>
        <p v-if="message" :class="['message', messageType]">
          {{ message }}
        </p>

        <section class="profile-card">
          <div class="profile-header">
            <div>
              <h2>Account Information</h2>
              <p>Edit your personal and login information.</p>
            </div>

            <span class="role-badge">
              {{ role || "Member" }}
            </span>
          </div>

          <form class="profile-form" @submit.prevent="updateProfile">
            <div class="form-group">
              <label>First Name</label>
              <input v-model="profileForm.firstName" type="text" />
            </div>

            <div class="form-group">
              <label>Last Name</label>
              <input v-model="profileForm.lastName" type="text" />
            </div>

            <div class="form-group">
              <label>Username</label>
              <input v-model="profileForm.username" type="text" />
            </div>

            <div class="form-group">
              <label>Email</label>
              <input v-model="profileForm.email" type="email" />
            </div>

            <button class="primary-btn" type="submit" :disabled="savingProfile">
              {{ savingProfile ? "Saving..." : "Save Profile" }}
            </button>
          </form>
        </section>

        <section class="profile-card">
          <div class="profile-header">
            <div>
              <h2>Change Password</h2>
              <p>Update your password securely.</p>
            </div>
          </div>

          <form class="profile-form" @submit.prevent="changePassword">
            <div class="form-group">
              <label>Current Password</label>
              <input v-model="passwordForm.currentPassword" type="password" />
            </div>

            <div class="form-group">
              <label>New Password</label>
              <input v-model="passwordForm.newPassword" type="password" />
            </div>

            <div class="form-group">
              <label>Confirm New Password</label>
              <input v-model="passwordForm.confirmNewPassword" type="password" />
            </div>

            <button class="secondary-btn" type="submit" :disabled="savingPassword">
              {{ savingPassword ? "Changing..." : "Change Password" }}
            </button>
          </form>
        </section>
      </template>
    </main>
  </div>
</template>

<script setup>
import { computed, onMounted, reactive, ref } from "vue";
import axios from "axios";

import AppTopbar from "@/components/AppTopbar.vue";

const API_BASE_URL = "http://localhost:5239";

const loading = ref(false);
const savingProfile = ref(false);
const savingPassword = ref(false);

const message = ref("");
const messageType = ref("success");

const token = localStorage.getItem("token");
const role = localStorage.getItem("role");

const profileForm = reactive({
  firstName: "",
  lastName: "",
  username: "",
  email: "",
});

const passwordForm = reactive({
  currentPassword: "",
  newPassword: "",
  confirmNewPassword: "",
});

const fullName = computed(() => {
  const name = `${profileForm.firstName} ${profileForm.lastName}`.trim();
  return name || profileForm.username || profileForm.email || "Member Profile";
});

const headers = {
  Authorization: `Bearer ${token}`,
};

const showMessage = (text, type = "success") => {
  message.value = text;
  messageType.value = type;
};

const getProfile = async () => {
  if (!token) {
    showMessage("Please login to view your profile.", "error");
    return;
  }

  try {
    loading.value = true;
    message.value = "";

    const response = await axios.get(`${API_BASE_URL}/api/members/me`, {
      headers,
    });

    const data = response.data.data || response.data;

    profileForm.firstName = data.firstName || "";
    profileForm.lastName = data.lastName || "";
    profileForm.username = data.username || "";
    profileForm.email = data.email || "";
  } catch (error) {
    console.error(error);
    showMessage("Profile could not be loaded.", "error");
  } finally {
    loading.value = false;
  }
};

const updateProfile = async () => {
  try {
    savingProfile.value = true;
    message.value = "";

    const response = await axios.put(
      `${API_BASE_URL}/api/members/me`,
      {
        firstName: profileForm.firstName,
        lastName: profileForm.lastName,
        username: profileForm.username,
        email: profileForm.email,
      },
      {
        headers,
      }
    );

    const updated = response.data.data || response.data;

    localStorage.setItem("email", updated.email || profileForm.email);
    localStorage.setItem("username", updated.username || profileForm.username);

    showMessage("Profile updated successfully.", "success");
  } catch (error) {
    console.error(error);

    const errorMessage =
      error.response?.data || "Profile update failed.";

    showMessage(errorMessage, "error");
  } finally {
    savingProfile.value = false;
  }
};

const changePassword = async () => {
  if (passwordForm.newPassword !== passwordForm.confirmNewPassword) {
    showMessage("New password and confirmation password do not match.", "error");
    return;
  }

  try {
    savingPassword.value = true;
    message.value = "";

    await axios.put(
      `${API_BASE_URL}/api/members/me/change-password`,
      {
        currentPassword: passwordForm.currentPassword,
        newPassword: passwordForm.newPassword,
        confirmNewPassword: passwordForm.confirmNewPassword,
      },
      {
        headers,
      }
    );

    passwordForm.currentPassword = "";
    passwordForm.newPassword = "";
    passwordForm.confirmNewPassword = "";

    showMessage("Password changed successfully.", "success");
  } catch (error) {
    console.error(error);

    const errorMessage =
      error.response?.data || "Password change failed.";

    showMessage(errorMessage, "error");
  } finally {
    savingPassword.value = false;
  }
};

onMounted(getProfile);
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
  max-width: 1050px;
  margin: 0 auto;
}

.profile-hero {
  padding: 34px;
  margin-bottom: 26px;
  border-radius: 28px;
  background: white;
  box-shadow: 0 18px 45px rgba(15, 23, 42, 0.08);
  display: flex;
  align-items: center;
  gap: 24px;
}

.avatar {
  width: 92px;
  height: 92px;
  border-radius: 26px;
  background: #ecfdf5;
  display: grid;
  place-items: center;
  font-size: 42px;
  flex-shrink: 0;
}

.eyebrow {
  margin: 0 0 8px;
  color: #166534;
  font-weight: 900;
}

.profile-hero h1 {
  margin: 0;
  font-size: 42px;
  color: #0f172a;
}

.profile-hero p {
  margin: 10px 0 0;
  color: #64748b;
  font-size: 17px;
}

.profile-card {
  padding: 30px;
  margin-bottom: 24px;
  border-radius: 28px;
  background: white;
  border: 1px solid #e5e7eb;
  box-shadow: 0 18px 45px rgba(15, 23, 42, 0.08);
}

.profile-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  gap: 18px;
  padding-bottom: 22px;
  margin-bottom: 24px;
  border-bottom: 1px solid #e5e7eb;
}

.profile-header h2 {
  margin: 0;
  font-size: 26px;
  color: #0f172a;
}

.profile-header p {
  margin: 6px 0 0;
  color: #64748b;
  font-weight: 700;
}

.role-badge {
  padding: 9px 14px;
  border-radius: 999px;
  background: #dcfce7;
  color: #166534;
  font-size: 13px;
  font-weight: 900;
}

.profile-form {
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

.primary-btn,
.secondary-btn {
  height: 54px;
  border: none;
  border-radius: 16px;
  font-weight: 900;
  cursor: pointer;
  align-self: end;
}

.primary-btn {
  background: #166534;
  color: white;
}

.secondary-btn {
  background: #111;
  color: white;
}

.primary-btn:disabled,
.secondary-btn:disabled {
  opacity: 0.6;
  cursor: not-allowed;
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

.state {
  padding: 24px;
  border-radius: 18px;
  background: white;
  color: #64748b;
  font-weight: 700;
}

@media (max-width: 760px) {
  .profile-hero {
    flex-direction: column;
    align-items: flex-start;
  }

  .profile-form {
    grid-template-columns: 1fr;
  }

  .profile-header {
    flex-direction: column;
    align-items: flex-start;
  }
}
</style>