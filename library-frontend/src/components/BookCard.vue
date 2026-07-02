<template>
  <div class="book-card" @click="$emit('click')">
    <div class="book-top">
      <img
        v-if="book.coverUrl"
        :src="book.coverUrl"
        :alt="book.title"
        class="book-cover"
      />
      <div v-else class="book-icon">📘</div>

      <div>
        <h3>{{ book.title }}</h3>
        <p>{{ book.authorName || "Unknown Author" }}</p>
      </div>
    </div>

    <div class="book-bottom">
      <div>
        <small>{{ book.categoryName || "No Category" }}</small>
        <span class="copies">
          {{ book.availableCopies ?? 0 }} / {{ book.totalCopies ?? 1 }} available
        </span>
      </div>

      <span :class="['badge', book.isAvailable ? 'available' : 'borrowed']">
        {{ book.isAvailable ? "Available" : "Borrowed" }}
      </span>
    </div>
  </div>
</template>

<script setup>
defineProps({
  book: {
    type: Object,
    required: true,
  },
});

defineEmits(["click"]);
</script>

<style scoped>
.book-card {
  padding: 16px;
  border-radius: 18px;
  background: #f8fafc;
  border: 1px solid #e2e8f0;
  cursor: pointer;
  transition: 0.18s ease;
}

.book-card:hover {
  transform: translateY(-2px);
  box-shadow: 0 10px 24px rgba(15, 23, 42, 0.08);
}

.book-top {
  display: flex;
  gap: 13px;
  align-items: flex-start;
}

.book-icon {
  width: 44px;
  height: 44px;
  border-radius: 14px;
  background: #eef9e8;
  display: grid;
  place-items: center;
  font-size: 22px;
  flex-shrink: 0;
}

.book-cover {
  width: 44px;
  height: 58px;
  border-radius: 8px;
  object-fit: cover;
  flex-shrink: 0;
}

h3 {
  margin: 0 0 6px;
  font-size: 18px;
  font-weight: 700;
  line-height: 1.2;
  color: #111827;
}

p {
  margin: 0;
  color: #64748b;
  font-size: 14px;
  font-weight: 600;
}

.book-bottom {
  display: flex;
  align-items: flex-end;
  justify-content: space-between;
  margin-top: 14px;
  gap: 12px;
}

.book-bottom > div {
  display: flex;
  flex-direction: column;
  gap: 4px;
}

small {
  color: #166534;
  font-weight: 800;
}

.copies {
  color: #64748b;
  font-size: 12px;
  font-weight: 700;
}

.badge {
  padding: 6px 10px;
  border-radius: 999px;
  font-size: 12px;
  font-weight: 900;
  white-space: nowrap;
}

.available {
  background: #dcfce7;
  color: #166534;
}

.borrowed {
  background: #fee2e2;
  color: #991b1b;
}
</style>