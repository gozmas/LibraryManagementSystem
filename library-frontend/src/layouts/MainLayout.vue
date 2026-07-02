<template>
  <div class="page">
    <AppTopbar />

    <main class="content">
      <slot />
    </main>
  </div>
</template>

<script setup>
import AppTopbar from "@/components/AppTopbar.vue";
</script>

<style scoped>
.page {
  min-height: 100vh;
  padding: 24px;
  background:
    radial-gradient(circle at 8% 12%, #eef9e8 0%, transparent 28%),
    radial-gradient(circle at 95% 92%, #f8eaf8 0%, transparent 24%),
    #f8faf7;
  font-family: Inter, system-ui, sans-serif;
  color: #0f172a;
}

.content {
  display: grid;
  gap: 22px;
}
</style>