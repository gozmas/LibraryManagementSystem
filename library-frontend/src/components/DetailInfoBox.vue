<template>
  <div class="info-box">
    <p>{{ title }}</p>
    <strong>{{ value }}</strong>
  </div>
</template>

<script setup>
defineProps({
  title: String,
  value: [String, Number]
})
</script>

<style scoped>
.info-box {
  padding: 22px;
  border-radius: 20px;
  background: #f8fafc;
  border: 1px solid #e2e8f0;
}

.info-box p {
  margin: 0 0 8px;
  color: #64748b;
  font-weight: 800;
}

.info-box strong {
  font-size: 20px;
}
</style>