<template>
  <section class="detail-hero">
    <div v-if="!hideIcon" class="detail-icon">{{ icon }}</div>

    <div>
      <p class="eyebrow">{{ label }}</p>
      <h1>{{ title }}</h1>
      <p class="subtitle">{{ subtitle }}</p>

      <span
        v-if="badgeText"
        class="badge"
        :class="badgeType"
      >
        {{ badgeText }}
      </span>
    </div>
  </section>
</template>

<script setup>
defineProps({
  icon: String,
  label: String,
  title: String,
  subtitle: String,
  badgeText: String,
  badgeType: String,
  hideIcon: Boolean,
});
</script>

<style scoped>
.detail-hero {
  display: flex;
  align-items: center;
  gap: 28px;
  padding-bottom: 34px;
  border-bottom: 1px solid #e5e7eb;
}

.detail-icon {
  width: 120px;
  height: 120px;
  border-radius: 28px;
  background: #eef9e8;
  display: grid;
  place-items: center;
  font-size: 58px;
}

.eyebrow {
  margin: 0 0 8px;
  color: #166534;
  font-weight: 900;
}

.detail-hero h1 {
  margin: 0;
  font-size: 48px;
}

.subtitle {
  margin: 12px 0 18px;
  color: #64748b;
  font-size: 20px;
  font-weight: 700;
}

.badge {
  display: inline-block;
  padding: 9px 14px;
  border-radius: 999px;
  font-size: 13px;
  font-weight: 900;
}

.badge.available {
  background: #dcfce7;
  color: #166534;
}

.badge.borrowed {
  background: #fee2e2;
  color: #991b1b;
}

@media (max-width: 750px) {
  .detail-hero {
    flex-direction: column;
    align-items: flex-start;
  }
}
</style>