<template>
  <MainLayout>
    <HeroSection
      :isMember="isMember"
      :isAdmin="isAdmin"
      :userEmail="displayEmail"
    />

    <section
      v-if="isAdmin && adminOverdueLoans.length"
      class="admin-warning"
      @click="goAdminLoans"
    >
      <span>⚠️</span>

      <div>
        <strong>{{ adminOverdueLoans.length }} overdue loan found</strong>
        <p>Some borrowed books passed their due date. Click to view loan details.</p>
      </div>
    </section>

    <section
      v-if="isMember && memberOverdueLoans.length"
      class="member-warning"
      @click="goMyLoans"
    >
      <span>⚠️</span>

      <div>
        <strong>{{ memberOverdueLoans.length }} overdue book found</strong>
        <p>Please return your overdue books as soon as possible.</p>
      </div>
    </section>
    <section
      v-if="isMember && memberDueSoonLoans.length"
      class="member-reminder"
      @click="goMyLoans"
    >
      <span>⏰</span>

      <div>
        <strong>{{ memberDueSoonLoans.length }} book(s) due soon</strong>
        <p>These are due within 3 days. Return them on time to avoid fines.</p>
      </div>
    </section>

    <section
      v-if="isMember && memberUnpaidFines.length"
      class="member-fine-warning"
      @click="goMyFines"
    >
      <span>💰</span>

      <div>
        <strong>{{ memberUnpaidFines.length }} unpaid fine(s) — {{ totalUnpaidFineAmount }}₺</strong>
        <p>{{ fineSummaryText }}</p>
      </div>
    </section>

    <section v-if="isMember || isAdmin" class="stats">
      <template v-if="isAdmin">
        <motion.div
          :initial="{ opacity: 0, y: 16 }"
          :animate="{ opacity: 1, y: 0 }"
          :transition="{ duration: 0.4, delay: 0 }"
          :whileHover="{ scale: 1.03 }"
        >
          <StatCard icon="📚" title="Total Books" :value="books.length" />
        </motion.div>

        <motion.div
          :initial="{ opacity: 0, y: 16 }"
          :animate="{ opacity: 1, y: 0 }"
          :transition="{ duration: 0.4, delay: 0.05 }"
          :whileHover="{ scale: 1.03 }"
        >
          <StatCard icon="✍️" title="Total Authors" :value="authors.length" />
        </motion.div>

        <motion.div
          :initial="{ opacity: 0, y: 16 }"
          :animate="{ opacity: 1, y: 0 }"
          :transition="{ duration: 0.4, delay: 0.1 }"
          :whileHover="{ scale: 1.03 }"
        >
          <StatCard icon="🏷️" title="Total Categories" :value="categories.length" />
        </motion.div>

        <motion.div
          :initial="{ opacity: 0, y: 16 }"
          :animate="{ opacity: 1, y: 0 }"
          :transition="{ duration: 0.4, delay: 0.15 }"
          :whileHover="{ scale: 1.03 }"
        >
          <StatCard icon="⏳" title="Active Loans" :value="adminActiveLoans.length" />
        </motion.div>
      </template>

      <template v-else>
        <motion.div
          :initial="{ opacity: 0, y: 16 }"
          :animate="{ opacity: 1, y: 0 }"
          :transition="{ duration: 0.4, delay: 0 }"
          :whileHover="{ scale: 1.03 }"
        >
          <StatCard icon="📖" title="Books This Month" :value="booksThisMonth" />
        </motion.div>

        <motion.div
          :initial="{ opacity: 0, y: 16 }"
          :animate="{ opacity: 1, y: 0 }"
          :transition="{ duration: 0.4, delay: 0.05 }"
          :whileHover="{ scale: 1.03 }"
        >
          <StatCard icon="📚" title="Books This Year" :value="booksThisYear" />
        </motion.div>

        <motion.div
          :initial="{ opacity: 0, y: 16 }"
          :animate="{ opacity: 1, y: 0 }"
          :transition="{ duration: 0.4, delay: 0.1 }"
          :whileHover="{ scale: 1.03 }"
        >
          <StatCard icon="🏷️" title="Favorite Category" :value="favoriteCategory" />
        </motion.div>

        <motion.div
          :initial="{ opacity: 0, y: 16 }"
          :animate="{ opacity: 1, y: 0 }"
          :transition="{ duration: 0.4, delay: 0.15 }"
          :whileHover="{ scale: 1.03 }"
        >
          <StatCard icon="⏳" title="Active Loans" :value="memberActiveLoans.length" />
        </motion.div>
      </template>
    </section>

    <LiveActivityWidget v-if="isMember || isAdmin" />

    <section class="columns">
      <motion.div
        class="panel"
        :initial="{ opacity: 0, y: 24 }"
        :animate="{ opacity: 1, y: 0 }"
        :transition="{ duration: 0.5, delay: 0 }"
      >
        <div class="panel-top">
          <div>
            <p>Books</p>
            <h2>All Books</h2>
          </div>

          <span>📚</span>
        </div>

        <SearchInput
          v-model="bookSearch"
          placeholder="Search books..."
        />

        <div class="list">
          <motion.div
            v-for="(book, index) in filteredBooks"
            :key="book.id"
            :initial="{ opacity: 0, y: 12 }"
            :animate="{ opacity: 1, y: 0 }"
            :transition="{ duration: 0.3, delay: index * 0.03 }"
          >
            <BookCard
              :book="book"
              @click="goBookDetail(book.id)"
            />
          </motion.div>

          <p v-if="filteredBooks.length === 0" class="empty">
            No books found.
          </p>
        </div>
      </motion.div>

      <motion.div
        class="panel"
        :initial="{ opacity: 0, y: 24 }"
        :animate="{ opacity: 1, y: 0 }"
        :transition="{ duration: 0.5, delay: 0.1 }"
      >
        <div class="panel-top">
          <div>
            <p>Authors</p>
            <h2>All Authors</h2>
          </div>

          <span>✍️</span>
        </div>

        <SearchInput
          v-model="authorSearch"
          placeholder="Search authors..."
        />

        <div class="list">
          <motion.div
            v-for="(author, index) in filteredAuthors"
            :key="author.id"
            :initial="{ opacity: 0, y: 12 }"
            :animate="{ opacity: 1, y: 0 }"
            :transition="{ duration: 0.3, delay: index * 0.03 }"
          >
            <AuthorCard
              :author="author"
              @click="goAuthorDetail(author.id)"
            />
          </motion.div>

          <p v-if="filteredAuthors.length === 0" class="empty">
            No authors found.
          </p>
        </div>
      </motion.div>

      <motion.div
        class="panel"
        :initial="{ opacity: 0, y: 24 }"
        :animate="{ opacity: 1, y: 0 }"
        :transition="{ duration: 0.5, delay: 0.2 }"
      >
        <div class="panel-top">
          <div>
            <p>Categories</p>
            <h2>All Categories</h2>
          </div>

          <span>🏷️</span>
        </div>

        <SearchInput
          v-model="categorySearch"
          placeholder="Search categories..."
        />

        <div class="list">
          <motion.div
            v-for="(category, index) in filteredCategories"
            :key="category.id"
            :initial="{ opacity: 0, y: 12 }"
            :animate="{ opacity: 1, y: 0 }"
            :transition="{ duration: 0.3, delay: index * 0.03 }"
          >
            <CategoryCard
              :category="category"
              @click="goCategoryDetail(category.id)"
            />
          </motion.div>

          <p v-if="filteredCategories.length === 0" class="empty">
            No categories found.
          </p>
        </div>
      </motion.div>
    </section>
  </MainLayout>
</template>

<script setup>
import { computed, onMounted, ref } from "vue";
import { useRouter } from "vue-router";
import axios from "axios";
import { motion } from "motion-v";

import MainLayout from "@/layouts/MainLayout.vue";
import HeroSection from "@/components/HeroSection.vue";
import BookCard from "@/components/BookCard.vue";
import AuthorCard from "@/components/AuthorCard.vue";
import CategoryCard from "@/components/CategoryCard.vue";
import SearchInput from "@/components/SearchInput.vue";
import StatCard from "@/components/StatCard.vue";
import LiveActivityWidget from "@/components/LiveActivityWidget.vue";

const router = useRouter();

const API_BASE_URL = "http://localhost:5239";

const role = localStorage.getItem("role") || "";
const token = localStorage.getItem("token");
const storedEmail = localStorage.getItem("email") || "";

const isAdmin = computed(() => role === "Admin");
const isMember = computed(() => role !== "Admin");

const displayEmail = computed(() => {
  if (isAdmin.value) return storedEmail || "admin@library.com";
  if (isMember.value) return storedEmail || "Member";
  return "Guest";
});

const books = ref([]);
const authors = ref([]);
const categories = ref([]);

const memberLoans = ref([]);
const adminLoans = ref([]);
const memberFines = ref([]);

const bookSearch = ref("");
const authorSearch = ref("");
const categorySearch = ref("");

const normalize = (response) => {
  return response.data.data || response.data || [];
};

const isLoanOverdue = (loan) => {
  if (loan.isReturned || loan.returnDate) return false;

  const dueDate = new Date(loan.dueDate);
  const today = new Date();

  return dueDate < today;
};

const memberActiveLoans = computed(() => {
  return memberLoans.value.filter((loan) => !loan.isReturned && !loan.returnDate);
});

const memberOverdueLoans = computed(() => {
  return memberLoans.value.filter((loan) => isLoanOverdue(loan));
});

const memberDueSoonLoans = computed(() => {
  const today = new Date();
  const threshold = new Date();
  threshold.setDate(today.getDate() + 3);

  return memberLoans.value.filter((loan) => {
    if (loan.isReturned || loan.returnDate) return false;

    const dueDate = new Date(loan.dueDate);
    return dueDate >= today && dueDate <= threshold;
  });
});

const memberUnpaidFines = computed(() => {
  return memberFines.value.filter((fine) => !fine.isPaid);
});

const totalUnpaidFineAmount = computed(() => {
  return memberUnpaidFines.value.reduce(
    (sum, fine) => sum + (fine.amount || 0),
    0
  );
});

const fineSummaryText = computed(() => {
  const reasons = memberUnpaidFines.value.map((fine) => fine.reason);

  const parts = [];
  if (reasons.includes("Late")) parts.push("late returns");
  if (reasons.includes("Damaged")) parts.push("damaged books");
  if (reasons.includes("Lost")) parts.push("lost books");

  if (parts.length === 0) return "You have unpaid fines.";

  return `You have unpaid fines for: ${parts.join(", ")}.`;
}); 

const adminActiveLoans = computed(() => {
  return adminLoans.value.filter((loan) => !loan.isReturned && !loan.returnDate);
});

const adminOverdueLoans = computed(() => {
  return adminLoans.value.filter((loan) => isLoanOverdue(loan));
});

const booksThisMonth = computed(() => {
  const now = new Date();

  return memberLoans.value.filter((loan) => {
    if (!loan.borrowDate) return false;

    const borrowDate = new Date(loan.borrowDate);

    return (
      borrowDate.getFullYear() === now.getFullYear() &&
      borrowDate.getMonth() === now.getMonth()
    );
  }).length;
});

const booksThisYear = computed(() => {
  const now = new Date();

  return memberLoans.value.filter((loan) => {
    if (!loan.borrowDate) return false;

    const borrowDate = new Date(loan.borrowDate);

    return borrowDate.getFullYear() === now.getFullYear();
  }).length;
});

const favoriteCategory = computed(() => {
  const categoryCounts = {};

  memberLoans.value.forEach((loan) => {
    const book = books.value.find((item) => item.id === loan.bookId);

    if (!book || !book.categoryName) return;

    categoryCounts[book.categoryName] =
      (categoryCounts[book.categoryName] || 0) + 1;
  });

  const entries = Object.entries(categoryCounts);

  if (entries.length === 0) return "-";

  entries.sort((a, b) => b[1] - a[1]);

  return entries[0][0];
});

const filteredBooks = computed(() => {
  const search = bookSearch.value.toLowerCase();

  return books.value.filter((book) => {
    const title = book.title?.toLowerCase() || "";
    const author = book.authorName?.toLowerCase() || "";
    const category = book.categoryName?.toLowerCase() || "";
    const isbn = book.isbn?.toLowerCase() || "";

    return (
      title.includes(search) ||
      author.includes(search) ||
      category.includes(search) ||
      isbn.includes(search)
    );
  });
});

const filteredAuthors = computed(() => {
  const search = authorSearch.value.toLowerCase();

  return authors.value.filter((author) => {
    const name = author.name?.toLowerCase() || "";

    return name.includes(search);
  });
});

const filteredCategories = computed(() => {
  const search = categorySearch.value.toLowerCase();

  return categories.value.filter((category) => {
    const name = category.name?.toLowerCase() || "";

    return name.includes(search);
  });
});

const getData = async () => {
  try {
    const [bookRes, authorRes, categoryRes] = await Promise.all([
      axios.get(`${API_BASE_URL}/api/books`),
      axios.get(`${API_BASE_URL}/api/authors`),
      axios.get(`${API_BASE_URL}/api/categories`),
    ]);

    books.value = normalize(bookRes);

    authors.value = normalize(authorRes).map((author) => ({
      ...author,
      name:
        author.name ||
        `${author.firstName || ""} ${author.lastName || ""}`.trim(),
    }));

    categories.value = normalize(categoryRes);

    if (token && isMember.value) {
      try {
        const loanRes = await axios.get(`${API_BASE_URL}/api/loans/my`, {
          headers: {
            Authorization: `Bearer ${token}`,
          },
        });

        memberLoans.value = normalize(loanRes);
      } catch (error) {
        console.error("Member loans could not be loaded:", error);
        memberLoans.value = [];
      }
    }
    try {
        const fineRes = await axios.get(`${API_BASE_URL}/api/fines/my`, {
          headers: {
            Authorization: `Bearer ${token}`,
          },
        });

        memberFines.value = normalize(fineRes);
      } catch (error) {
        console.error("Member fines could not be loaded:", error);
        memberFines.value = [];
      }

    if (token && isAdmin.value) {
      try {
        const loanRes = await axios.get(`${API_BASE_URL}/api/loans`, {
          headers: {
            Authorization: `Bearer ${token}`,
          },
        });

        adminLoans.value = normalize(loanRes);
      } catch (error) {
        console.error("Admin loans could not be loaded:", error);
        adminLoans.value = [];
      }
    }
  } catch (error) {
    console.error("Home data error:", error);
  }
};

const goBookDetail = (id) => {
  router.push(`/books/${id}`);
};

const goAuthorDetail = (id) => {
  router.push(`/authors/${id}`);
};

const goCategoryDetail = (id) => {
  router.push(`/categories/${id}`);
};

const goMyLoans = () => {
  router.push("/my-loans");
};
const goMyFines = () => {
  router.push("/my-fines");
};

const goAdminLoans = () => {
  router.push("/admin/loans");
};

onMounted(getData);
</script>

<style scoped>
* {
  box-sizing: border-box;
}

.stats {
  display: grid;
  grid-template-columns: repeat(4, 1fr);
  gap: 18px;
  margin-bottom: 22px;
}

.admin-warning,
.member-warning {
  padding: 20px 24px;
  margin-bottom: 22px;
  border-radius: 22px;
  background: #fee2e2;
  color: #991b1b;
  box-shadow: 0 14px 34px rgba(15, 23, 42, 0.07);
  display: flex;
  align-items: center;
  gap: 16px;
  cursor: pointer;
  transition: 0.18s ease;
}

.admin-warning:hover,
.member-warning:hover {
  transform: translateY(-2px);
  box-shadow: 0 18px 42px rgba(15, 23, 42, 0.1);
}

.admin-warning span,
.member-warning span {
  font-size: 32px;
}

.admin-warning strong,
.member-warning strong {
  font-size: 18px;
}

.admin-warning p,
.member-warning p {
  margin: 4px 0 0;
  font-weight: 700;
}
.member-reminder {
  padding: 20px 24px;
  margin-bottom: 22px;
  border-radius: 22px;
  background: #fef3c7;
  color: #92400e;
  box-shadow: 0 14px 34px rgba(15, 23, 42, 0.07);
  display: flex;
  align-items: center;
  gap: 16px;
  cursor: pointer;
  transition: 0.18s ease;
}

.member-reminder:hover {
  transform: translateY(-2px);
  box-shadow: 0 18px 42px rgba(15, 23, 42, 0.1);
}

.member-reminder span {
  font-size: 32px;
}

.member-reminder strong {
  font-size: 18px;
}

.member-reminder p {
  margin: 4px 0 0;
  font-weight: 700;
}

.member-fine-warning {
  padding: 20px 24px;
  margin-bottom: 22px;
  border-radius: 22px;
  background: #ede9fe;
  color: #5b21b6;
  box-shadow: 0 14px 34px rgba(15, 23, 42, 0.07);
  display: flex;
  align-items: center;
  gap: 16px;
  cursor: pointer;
  transition: 0.18s ease;
}

.member-fine-warning:hover {
  transform: translateY(-2px);
  box-shadow: 0 18px 42px rgba(15, 23, 42, 0.1);
}

.member-fine-warning span {
  font-size: 32px;
}

.member-fine-warning strong {
  font-size: 18px;
}

.member-fine-warning p {
  margin: 4px 0 0;
  font-weight: 700;
}

.columns {
  display: grid;
  grid-template-columns: repeat(3, 1fr);
  gap: 22px;
}

.panel {
  height: calc(100vh - 310px);
  min-height: 560px;
  padding: 24px;
  border-radius: 26px;
  background: white;
  border: 1px solid #e5e7eb;
  box-shadow: 0 18px 45px rgba(15, 23, 42, 0.08);
  display: flex;
  flex-direction: column;
}

.panel-top {
  display: flex;
  align-items: center;
  justify-content: space-between;
  margin-bottom: 18px;
}

.panel-top p {
  margin: 0 0 5px;
  color: #166534;
  font-weight: 900;
  font-size: 14px;
}

.panel-top h2 {
  margin: 0;
  font-size: 28px;
  color: #0f172a;
}

.panel-top > span {
  width: 54px;
  height: 54px;
  border-radius: 18px;
  background: #f1f5f9;
  display: grid;
  place-items: center;
  font-size: 26px;
}

.list {
  flex: 1;
  overflow-y: auto;
  padding-right: 5px;
  display: grid;
  align-content: start;
  gap: 14px;
}

.empty {
  margin: 24px 0;
  text-align: center;
  color: #94a3b8;
  font-weight: 700;
}

@media (max-width: 1150px) {
  .columns,
  .stats {
    grid-template-columns: 1fr;
  }

  .panel {
    height: auto;
    min-height: 420px;
  }
}
</style>