<template>
  <Card class="stat-card">
    <CardContent class="content">
      <div class="icon-box">
        <span>{{ icon }}</span>
      </div>

      <div class="text">
        <p>{{ title }}</p>
        <h2>{{ value }}</h2>
      </div>
    </CardContent>
  </Card>
</template>

<script setup>
import { Card, CardContent } from "@/components/ui/card";

defineProps({
  icon: {
    type: String,
    default: "📊",
  },
  title: {
    type: String,
    default: "",
  },
  value: {
    type: [String, Number],
    default: 0,
  },
});
</script>

<style scoped>
.stat-card {
  height: 100%;
}

.content {
  display: flex;
  align-items: center;
  gap: 16px;
  padding: 24px;
}

.icon-box {
  width: 56px;
  height: 56px;
  border-radius: 14px;
  background: #ecfdf5;
  display: flex;
  align-items: center;
  justify-content: center;
  font-size: 26px;
  flex-shrink: 0;
}

.text {
  display: flex;
  flex-direction: column;
}

.text p {
  margin: 0;
  color: #64748b;
  font-size: 14px;
  font-weight: 600;
}

.text h2 {
  margin: 4px 0 0;
  font-size: 30px;
  font-weight: 700;
  color: #0f172a;
}
</style>