<template>
  <div class="page">
    <AppTopbar />

    <main class="content">
      <section class="header-card">
        <div>
          <p class="eyebrow">Member Area</p>
          <h1>My Wishlist</h1>
          <p>Books you want to read — get notified the moment a copy is free.</p>
        </div>

        <div class="summary-wrapper">
          <div class="summary-box">
            <span class="summary-number">{{ wishlist.length }}</span>
            <span class="summary-label">Wishlisted</span>
          </div>

          <div class="summary-box available-box">
            <span class="summary-number">{{ availableCount }}</span>
            <span class="summary-label">Available Now</span>
          </div>
        </div>
      </section>

      <p v-if="loading" class="state">Loading wishlist...</p>
      <p v-else-if="message" class="state error">{{ message }}</p>

      <template v-else>
        <div v-if="wishlist.length" class="wishlist-grid">
          <article
            v-for="item in wishlist"
            :key="item.id"
            class="wishlist-card"
          >
            <div class="cover">
              <img v-if="item.coverUrl" :src="item.coverUrl" :alt="item.bookTitle" />
              <BookOpen v-else :size="26" />
            </div>

            <div class="wishlist-body">
              <div class="wishlist-top">
                <div>
                  <h2 @click="goToBook(item.bookId)">{{ item.bookTitle }}</h2>
                  <p>{{ item.authorName || "Unknown author" }}</p>
                </div>

                <span :class="['status-badge', item.isAvailable ? 'available' : 'unavailable']">
                  {{ item.isAvailable ? "Available" : "Borrowed" }}
                </span>
              </div>

              <p class="copies">
                {{ item.availableCopies }} / {{ item.totalCopies }} copies available
              </p>

              <div class="wishlist-actions">
                <button
                  v-if="item.isAvailable"
                  class="borrow-btn"
                  @click="goToBook(item.bookId)"
                >
                  Go Borrow
                </button>

                <button
                  class="remove-btn"
                  :disabled="removingId === item.bookId"
                  @click="removeFromWishlist(item.bookId)"
                >
                  {{ removingId === item.bookId ? "Removing..." : "Remove" }}
                </button>
              </div>
            </div>
          </article>
        </div>

        <div v-else class="empty-card">
          <Heart :size="28" />

          <div>
            <h3>Your wishlist is empty</h3>
            <p>
              Browse the catalogue and add books you'd like to read — we'll
              let you know live when a copy becomes available.
            </p>
          </div>
        </div>
      </template>
    </main>
  </div>
</template>

<script setup>
import { computed, onBeforeUnmount, onMounted, ref } from "vue";
import { useRouter } from "vue-router";
import axios from "axios";
import * as signalR from "@microsoft/signalr";
import { BookOpen, Heart } from "@lucide/vue";

import AppTopbar from "@/components/AppTopbar.vue";

const router = useRouter();

const API_BASE_URL = "http://localhost:5239";

const wishlist = ref([]);
const loading = ref(false);
const message = ref("");
const removingId = ref(null);
let connection = null;

const token = localStorage.getItem("token");
const role = localStorage.getItem("role");

const availableCount = computed(() => {
  return wishlist.value.filter((item) => item.isAvailable).length;
});

const getWishlist = async () => {
  try {
    loading.value = true;
    message.value = "";

    if (!token) {
      message.value = "Please login to view your wishlist.";
      return;
    }

    if (role === "Admin") {
      message.value = "Only members can use a wishlist.";
      return;
    }

    const response = await axios.get(`${API_BASE_URL}/api/wishlist/my`, {
      headers: { Authorization: `Bearer ${token}` },
    });

    wishlist.value = response.data.data || response.data || [];
  } catch (error) {
    console.error(error);
    message.value = "Wishlist could not be loaded.";
  } finally {
    loading.value = false;
  }
};

const removeFromWishlist = async (bookId) => {
  removingId.value = bookId;

  try {
    await axios.delete(`${API_BASE_URL}/api/wishlist/${bookId}`, {
      headers: { Authorization: `Bearer ${token}` },
    });

    wishlist.value = wishlist.value.filter((item) => item.bookId !== bookId);
  } catch (error) {
    console.error("Remove from wishlist failed:", error);
  } finally {
    removingId.value = null;
  }
};

const goToBook = (bookId) => {
  router.push(`/books/${bookId}`);
};

// Wishlist'teki kitaplardan biri başkası tarafından borrow/return
// edildiğinde, sayfa yenilenmeden liste güncellensin diye LoanHub'ı
// dinliyoruz. Bu, LiveActivityWidget'ın da kullandığı aynı "herkese açık"
// BookStatusChanged event'i — burada sadece wishlist'te olan kitap
// eşleşirse ilgili satırı güncelliyoruz.
const startLiveUpdates = async () => {
  if (!token || role === "Admin") return;

  connection = new signalR.HubConnectionBuilder()
    .withUrl(`${API_BASE_URL}/hubs/loan`, {
      accessTokenFactory: () => token,
    })
    .withAutomaticReconnect()
    .build();

  connection.on("BookStatusChanged", (data) => {
    const item = wishlist.value.find((entry) => entry.bookId === data.bookId);

    if (!item) return;

    item.isAvailable = data.availableCopies > 0;
    item.availableCopies = data.availableCopies;
    item.totalCopies = data.totalCopies;
  });

  try {
    await connection.start();
  } catch (error) {
    console.error("Wishlist live update connection failed:", error);
  }
};

onMounted(async () => {
  await getWishlist();
  await startLiveUpdates();
});

onBeforeUnmount(() => {
  if (connection) {
    connection.stop();
  }
});
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
  max-width: 1120px;
  margin: 0 auto;
}

.header-card {
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: 24px;
  margin-bottom: 28px;
  padding: 30px 34px;
  border-radius: 28px;
  background: rgba(255, 255, 255, 0.94);
  border: 1px solid #e2e8f0;
  box-shadow: 0 18px 45px rgba(15, 23, 42, 0.08);
}

.eyebrow {
  margin: 0 0 8px;
  color: #64748b;
  font-size: 13px;
  font-weight: 900;
  letter-spacing: 0.1em;
  text-transform: uppercase;
}

.header-card h1 {
  margin: 0;
  color: #0f172a;
  font-size: 38px;
  letter-spacing: -0.04em;
}

.header-card p {
  margin: 10px 0 0;
  color: #64748b;
  font-size: 16px;
  font-weight: 600;
}

.summary-wrapper {
  display: flex;
  gap: 14px;
}

.summary-box {
  min-width: 130px;
  padding: 18px 20px;
  border-radius: 22px;
  background: #f0fdf4;
  border: 1px solid #bbf7d0;
  text-align: center;
}

.available-box {
  background: #eff6ff;
  border-color: #bfdbfe;
}

.summary-number {
  display: block;
  color: #15803d;
  font-size: 34px;
  font-weight: 950;
}

.available-box .summary-number {
  color: #1d4ed8;
}

.summary-label {
  display: block;
  margin-top: 4px;
  color: #166534;
  font-size: 13px;
  font-weight: 900;
}

.available-box .summary-label {
  color: #1e40af;
}

.wishlist-grid {
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(360px, 1fr));
  gap: 22px;
}

.wishlist-card {
  display: flex;
  gap: 18px;
  padding: 22px;
  border-radius: 26px;
  background: rgba(255, 255, 255, 0.96);
  border: 1px solid #e2e8f0;
  box-shadow: 0 16px 38px rgba(15, 23, 42, 0.08);
  transition:
    transform 0.2s ease,
    box-shadow 0.2s ease,
    border-color 0.2s ease;
}

.wishlist-card:hover {
  transform: translateY(-4px);
  border-color: #cbd5e1;
  box-shadow: 0 24px 52px rgba(15, 23, 42, 0.12);
}

.cover {
  width: 76px;
  height: 106px;
  flex-shrink: 0;
  border-radius: 12px;
  background: #dff2d8;
  color: #166534;
  overflow: hidden;
  display: grid;
  place-items: center;
}

.cover img {
  width: 100%;
  height: 100%;
  object-fit: cover;
}

.wishlist-body {
  flex: 1;
  min-width: 0;
  display: flex;
  flex-direction: column;
}

.wishlist-top {
  display: flex;
  align-items: flex-start;
  justify-content: space-between;
  gap: 12px;
}

.wishlist-top h2 {
  margin: 0;
  color: #0f172a;
  font-size: 19px;
  line-height: 1.25;
  letter-spacing: -0.02em;
  cursor: pointer;
}

.wishlist-top h2:hover {
  text-decoration: underline;
}

.wishlist-top p {
  margin: 4px 0 0;
  color: #64748b;
  font-size: 13px;
  font-weight: 700;
}

.status-badge {
  flex-shrink: 0;
  padding: 6px 11px;
  border-radius: 999px;
  font-size: 11px;
  font-weight: 900;
  line-height: 1;
}

.status-badge.available {
  background: #dcfce7;
  color: #166534;
  border: 1px solid #bbf7d0;
}

.status-badge.unavailable {
  background: #fee2e2;
  color: #991b1b;
  border: 1px solid #fecaca;
}

.copies {
  margin: 10px 0 0;
  color: #475569;
  font-size: 13px;
  font-weight: 700;
}

.wishlist-actions {
  display: flex;
  gap: 10px;
  margin-top: 16px;
}

.borrow-btn {
  flex: 1;
  height: 42px;
  border: none;
  border-radius: 13px;
  background: #111;
  color: white;
  font-size: 13px;
  font-weight: 900;
  cursor: pointer;
}

.remove-btn {
  height: 42px;
  padding: 0 16px;
  border: 1.5px solid #e2e8f0;
  border-radius: 13px;
  background: white;
  color: #991b1b;
  font-size: 13px;
  font-weight: 900;
  cursor: pointer;
}

.remove-btn:disabled {
  opacity: 0.6;
  cursor: not-allowed;
}

.empty-card {
  display: flex;
  align-items: center;
  gap: 18px;
  padding: 28px;
  border-radius: 24px;
  background: white;
  border: 1px solid #dcfce7;
  color: #166534;
  box-shadow: 0 14px 34px rgba(15, 23, 42, 0.07);
}

.empty-card h3 {
  margin: 0;
  color: #166534;
  font-size: 22px;
}

.empty-card p {
  margin: 6px 0 0;
  color: #64748b;
  max-width: 460px;
}

.state {
  padding: 24px;
  border-radius: 20px;
  background: white;
  color: #64748b;
  font-weight: 800;
  box-shadow: 0 14px 35px rgba(15, 23, 42, 0.06);
}

.error {
  color: #b91c1c;
  background: #fff1f2;
}

@media (max-width: 860px) {
  .header-card {
    flex-direction: column;
    align-items: flex-start;
  }

  .summary-wrapper {
    width: 100%;
  }

  .summary-box {
    flex: 1;
  }

  .wishlist-grid {
    grid-template-columns: 1fr;
  }
}

@media (max-width: 560px) {
  .page {
    padding: 16px;
  }

  .header-card {
    padding: 24px;
  }

  .summary-wrapper {
    flex-direction: column;
  }

  .summary-box {
    width: 100%;
  }

  .wishlist-card {
    flex-direction: column;
  }

  .cover {
    width: 100%;
    height: 160px;
  }
}
</style>