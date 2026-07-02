<template>
  <section class="hero">
    <div>
      <p class="eyebrow">
        {{ isAdmin ? "Admin Area" : isMember ? "Member Area" : "Guest Area" }}
      </p>

      <h1>Explore the library</h1>

      <p>
        Search books, authors and categories from one clean dashboard.
      </p>
    </div>

    <div class="user-box">
      <span>👋</span>

      <div>
        <strong>
          {{ isMember || isAdmin ? userEmail : "Welcome, Guest" }}
        </strong>

        <p>
          {{
            isAdmin
              ? "Signed in as admin"
              : isMember
              ? "Signed in as member"
              : "Browsing as guest"
          }}
        </p>
      </div>
    </div>
  </section>
</template>

<script setup>
defineProps({
  isMember: {
    type: Boolean,
    default: false,
  },
  isAdmin: {
    type: Boolean,
    default: false,
  },
  userEmail: {
    type: String,
    default: "",
  },
});
</script>

<style scoped>
.hero {
  padding: 34px 38px;
  border-radius: 26px;
  background: white;
  box-shadow: 0 18px 45px rgba(15, 23, 42, 0.08);
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: 24px;
  margin-bottom: 22px;
}

.eyebrow {
  margin: 0 0 8px;
  color: #166534;
  font-weight: 900;
}

.hero h1 {
  margin: 0;
  font-size: 44px;
  letter-spacing: -1.5px;
  color: #0f172a;
}

.hero p {
  margin: 10px 0 0;
  color: #64748b;
  font-size: 18px;
}

.user-box {
  display: flex;
  align-items: center;
  gap: 16px;
  padding: 18px 20px;
  border-radius: 18px;
  background: #f8fafc;
  border: 1px solid #e2e8f0;
}

.user-box span {
  font-size: 32px;
}

.user-box strong {
  display: block;
  max-width: 260px;
  font-size: 16px;
  overflow: hidden;
  text-overflow: ellipsis;
  white-space: nowrap;
}

.user-box p {
  margin: 4px 0 0;
  font-size: 14px;
}

@media (max-width: 760px) {
  .hero {
    flex-direction: column;
    align-items: flex-start;
  }
}
</style>