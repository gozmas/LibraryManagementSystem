<template>
  <Card class="author-card" @click="$emit('click')">
    <div class="card-inner">
      <div class="author-icon">✍️</div>

      <div class="author-content">
        <CardHeader class="card-header">
          <CardTitle class="title">
            {{ author.name }}
          </CardTitle>

          <CardDescription class="subtitle">
            Library Author
          </CardDescription>
        </CardHeader>

        <CardContent class="card-content">
          <p class="biography">
            {{ author.biography || "No biography available." }}
          </p>
        </CardContent>
      </div>
    </div>
  </Card>
</template>

<script setup>
import {
  Card,
  CardContent,
  CardDescription,
  CardHeader,
  CardTitle,
} from "@/components/ui/card";

defineProps({
  author: {
    type: Object,
    required: true,
  },
});

defineEmits(["click"]);
</script>

<style scoped>
.author-card {
  cursor: pointer;
  border-radius: 18px;
  background: #f8fafc;
  border: 1px solid #e2e8f0;
  transition: 0.18s ease;
}

.author-card:hover {
  transform: translateY(-3px);
  box-shadow: 0 14px 32px rgba(15, 23, 42, 0.1);
}

.card-inner {
  display: flex;
  gap: 14px;
  padding: 18px;
}

.author-icon {
  width: 50px;
  height: 50px;
  border-radius: 15px;
  background: #fff7e6;
  display: grid;
  place-items: center;
  font-size: 24px;
  flex-shrink: 0;
}

.author-content {
  flex: 1;
  min-width: 0;
}

.card-header {
  padding: 0;
}

.title {
  font-size: 18px;
  line-height: 1.25;
}

.subtitle {
  margin-top: 5px;
  color: #64748b;
  font-weight: 600;
}

.card-content {
  padding: 12px 0 0;
}

.biography {
  margin: 0;
  color: #64748b;
  font-size: 14px;
  line-height: 1.5;

  display: -webkit-box;
  -webkit-line-clamp: 3;
  line-clamp: 3;
  -webkit-box-orient: vertical;
  overflow: hidden;
}
</style>