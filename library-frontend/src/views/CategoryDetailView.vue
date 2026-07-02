<template>
  <div class="detail-page">
    <header class="topbar">
      <div class="brand" @click="goHome">📚 LibraryMS</div>
      <button @click="goHome">Back to Home</button>
    </header>

    <main v-if="category" class="detail-card">
      <DetailHero
        icon="🏷️"
        label="Category Detail"
        :title="category.name"
        :subtitle="category.description || 'Book category'"
      />

      <section class="info-grid">
        <DetailInfoBox title="Category Name" :value="category.name || '-'" />
        <DetailInfoBox title="Description" :value="category.description || 'No description'" />
      </section>

      <section class="books-section">
        <h2>Books in this category</h2>

        <div v-if="books.length" class="books-grid">
          <BookCard
            v-for="book in books"
            :key="book.id"
            :book="book"
            @click="goToBook(book.id)"
          />
        </div>

        <p v-else class="empty-message">
          No books found in this category.
        </p>
      </section>
    </main>

    <main v-else class="detail-card">
      Loading category...
    </main>
  </div>
</template>

<script setup>
import { ref, onMounted } from "vue";
import { useRoute, useRouter } from "vue-router";
import axios from "axios";

import DetailHero from "@/components/DetailHero.vue";
import DetailInfoBox from "@/components/DetailInfoBox.vue";
import BookCard from "@/components/BookCard.vue";

const route = useRoute();
const router = useRouter();

const API_BASE_URL = "http://localhost:5239";

const category = ref(null);
const books = ref([]);

const getCategory = async () => {
  const response = await axios.get(`${API_BASE_URL}/api/categories/${route.params.id}`);
  category.value = response.data.data || response.data;
};

const getBooksByCategory = async () => {
  const response = await axios.get(`${API_BASE_URL}/api/books`);
  const allBooks = response.data.data || response.data;

  books.value = allBooks.filter(
    (book) => book.categoryId === Number(route.params.id)
  );
};

const goHome = () => {
  router.push("/home");
};

const goToBook = (id) => {
  router.push(`/books/${id}`);
};

onMounted(async () => {
  await getCategory();
  await getBooksByCategory();
});
</script>

<style scoped>
.detail-page {
  min-height: 100vh;
  padding: 24px;
  background: #f8faf7;
  font-family: Inter, system-ui, sans-serif;
}

.topbar {
  height: 76px;
  padding: 0 28px;
  margin-bottom: 24px;
  border-radius: 22px;
  background: white;
  box-shadow: 0 12px 32px rgba(15, 23, 42, 0.08);
  display: flex;
  align-items: center;
  justify-content: space-between;
}

.brand {
  font-size: 24px;
  font-weight: 900;
  cursor: pointer;
}

.topbar button {
  border: none;
  padding: 12px 18px;
  border-radius: 13px;
  background: #111;
  color: white;
  font-weight: 800;
  cursor: pointer;
}

.detail-card {
  max-width: 980px;
  margin: 0 auto;
  padding: 46px;
  border-radius: 28px;
  background: white;
  box-shadow: 0 24px 60px rgba(15, 23, 42, 0.1);
}

.info-grid {
  margin-top: 34px;
  display: grid;
  grid-template-columns: repeat(2, 1fr);
  gap: 18px;
}

.books-section {
  margin-top: 42px;
}

.books-section h2 {
  margin: 0 0 18px;
  font-size: 26px;
  color: #0f172a;
}

.books-grid {
  display: grid;
  grid-template-columns: repeat(2, 1fr);
  gap: 18px;
}

.empty-message {
  padding: 20px;
  border-radius: 18px;
  background: #f8fafc;
  color: #64748b;
  font-weight: 700;
}

@media (max-width: 750px) {
  .info-grid,
  .books-grid {
    grid-template-columns: 1fr;
  }
}
</style>