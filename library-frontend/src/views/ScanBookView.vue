<template>
  <div class="page">
    <AppTopbar />

    <main class="content">
      <section class="header">
        <div>
          <p class="eyebrow">Admin Management</p>
          <h1>Scan Book</h1>
          <p>Scan an ISBN barcode to auto-fill book details from Google Books.</p>
        </div>

        <button class="back-btn" @click="goAdmin">Back to Dashboard</button>
      </section>

      <p v-if="message" :class="['message', messageType]">{{ message }}</p>

      <section class="scan-card">
        <div class="scan-left">
          <div id="scanner-region" class="scanner-region"></div>

          <div class="scan-controls">
            <button v-if="!isScanning" class="primary-btn" @click="startScan">
              Start camera
            </button>
            <button v-else class="secondary-btn" @click="stopScan">
              Stop camera
            </button>
          </div>

          <div class="manual-entry">
            <label>Or enter ISBN manually</label>
            <div class="manual-row">
              <input v-model="manualIsbn" type="text" placeholder="e.g. 9780134494166" />
              <button class="primary-btn" @click="lookupIsbn(manualIsbn)">Lookup</button>
            </div>
          </div>
        </div>

        <div class="scan-right">
          <div v-if="loadingLookup" class="status-box">Looking up ISBN...</div>

          <div v-else-if="duplicateBook" class="status-box warning">
            <p>
              This ISBN already exists: <strong>{{ duplicateBook.title }}</strong>
              ({{ duplicateBook.availableCopies ?? 0 }} / {{ duplicateBook.totalCopies ?? 1 }} available).
            </p>

            <button class="primary-btn" :disabled="addingCopy" @click="addCopy">
              {{ addingCopy ? "Adding copy..." : "Add one more copy" }}
            </button>
          </div>

          <div v-else-if="notFoundWarning" class="status-box warning">
            No book was found for this ISBN. You can fill in the details manually below.
          </div>

          <form v-if="showForm" class="book-form" @submit.prevent="confirmAdd">
            <div class="form-group">
              <label>Title</label>
              <input v-model="form.title" type="text" required />
            </div>

            <div class="form-group">
              <label>ISBN</label>
              <input v-model="form.isbn" type="text" required />
            </div>

            <div class="form-group">
              <label>Publication Year</label>
              <input v-model.number="form.publicationYear" type="number" required />
            </div>

            <div class="form-group">
              <label>Author First Name</label>
              <input v-model="form.authorFirstName" type="text" required />
            </div>

            <div class="form-group">
              <label>Author Last Name</label>
              <input v-model="form.authorLastName" type="text" required />
            </div>

            <div class="form-group">
              <label>Category</label>
              <input v-model="form.categoryName" type="text" required />
            </div>

            <div class="form-group full">
              <label>Description</label>
              <textarea v-model="form.description" rows="3"></textarea>
            </div>

            <div class="form-group full">
              <label>Cover URL</label>
              <input v-model="form.coverUrl" type="text" />
              <img v-if="form.coverUrl" :src="form.coverUrl" class="cover-preview" />
            </div>

            <div class="form-actions full">
              <button class="primary-btn" type="submit" :disabled="saving">
                {{ saving ? "Adding..." : "Add Book" }}
              </button>
              <button class="secondary-btn" type="button" @click="resetForm">
                Cancel
              </button>
            </div>
          </form>
        </div>
      </section>

      <section class="table-section">
        <div class="table-header">
          <div>
            <h2>Added this session</h2>
            <p>{{ sessionBooks.length }} book(s) added so far.</p>
          </div>
        </div>

        <div class="table-card">
          <table>
            <thead>
              <tr>
                <th>Book</th>
                <th>ISBN</th>
                <th>Author</th>
                <th>Category</th>
              </tr>
            </thead>
            <tbody>
              <tr v-for="item in sessionBooks" :key="item.isbn">
                <td><strong>{{ item.title }}</strong></td>
                <td>{{ item.isbn }}</td>
                <td>{{ item.author }}</td>
                <td>{{ item.category }}</td>
              </tr>
            </tbody>
          </table>

          <p v-if="sessionBooks.length === 0" class="empty">
            No books added in this session yet.
          </p>
        </div>
      </section>
    </main>
  </div>
</template>

<script setup>
import { ref, reactive, onBeforeUnmount } from "vue";
import { useRouter } from "vue-router";
import axios from "axios";
import { Html5Qrcode, Html5QrcodeSupportedFormats } from "html5-qrcode";

import AppTopbar from "@/components/AppTopbar.vue";

const router = useRouter();

const API_BASE_URL = "http://localhost:5239";
const GOOGLE_BOOKS_URL = "https://www.googleapis.com/books/v1/volumes";

const token = localStorage.getItem("token");
const headers = { Authorization: `Bearer ${token}` };

const message = ref("");
const messageType = ref("success");

const isScanning = ref(false);
const manualIsbn = ref("");
const loadingLookup = ref(false);
const duplicateBook = ref(null);
const addingCopy = ref(false);
const notFoundWarning = ref(false);
const showForm = ref(false);
const saving = ref(false);

const sessionBooks = ref([]);

const form = reactive({
  title: "",
  isbn: "",
  publicationYear: "",
  authorFirstName: "",
  authorLastName: "",
  categoryName: "",
  description: "",
  coverUrl: "",
});

let scanner = null;

const showMessage = (text, type = "success") => {
  message.value = text;
  messageType.value = type;
};

const startScan = async () => {
  duplicateBook.value = null;
  notFoundWarning.value = false;
  showForm.value = false;

  scanner = new Html5Qrcode("scanner-region");
  isScanning.value = true;

  try {
    await scanner.start(
      { facingMode: "environment" },
      {
        fps: 10,
        qrbox: { width: 260, height: 140 },
        formatsToSupport: [Html5QrcodeSupportedFormats.EAN_13],
      },
      async (decodedText) => {
        await stopScan();
        await lookupIsbn(decodedText);
      },
      () => {}
    );
  } catch (error) {
    console.error(error);
    showMessage("Could not access the camera. Check browser permissions.", "error");
    isScanning.value = false;
  }
};

const stopScan = async () => {
  if (scanner && isScanning.value) {
    try {
      await scanner.stop();
      scanner.clear();
    } catch (error) {
      console.error(error);
    }
  }
  isScanning.value = false;
};

const resetForm = () => {
  showForm.value = false;
  duplicateBook.value = null;
  notFoundWarning.value = false;
  manualIsbn.value = "";
  Object.assign(form, {
    title: "",
    isbn: "",
    publicationYear: "",
    authorFirstName: "",
    authorLastName: "",
    categoryName: "",
    description: "",
    coverUrl: "",
  });
};

const lookupIsbn = async (isbn) => {
  const cleanIsbn = (isbn || "").trim();

  if (!cleanIsbn) {
    showMessage("Please enter or scan an ISBN.", "error");
    return;
  }

  message.value = "";
  duplicateBook.value = null;
  notFoundWarning.value = false;
  showForm.value = false;
  loadingLookup.value = true;

  try {
    const existingRes = await axios.get(`${API_BASE_URL}/api/books`);
    const existingBooks = existingRes.data.data || existingRes.data || [];
    const duplicate = existingBooks.find((b) => b.isbn === cleanIsbn);

    if (duplicate) {
      duplicateBook.value = duplicate;
      loadingLookup.value = false;
      return;
    }

   const googleRes = await axios.get(GOOGLE_BOOKS_URL, {
  params: {
    q: `isbn:${cleanIsbn}`,
    key: import.meta.env.VITE_GOOGLE_BOOKS_API_KEY,
  },
});

    const item = googleRes.data.items?.[0];

    if (!item) {
      notFoundWarning.value = true;
      form.isbn = cleanIsbn;
      showForm.value = true;
      loadingLookup.value = false;
      return;
    }

    const info = item.volumeInfo;
    const nameParts = (info.authors?.[0] || "").split(" ");
    const lastName = nameParts.pop() || "";
    const firstName = nameParts.join(" ") || "Unknown";

    form.title = info.title || "";
    form.isbn = cleanIsbn;
    form.publicationYear = info.publishedDate
      ? parseInt(info.publishedDate.slice(0, 4), 10)
      : "";
    form.authorFirstName = firstName;
    form.authorLastName = lastName || "Unknown";
    form.categoryName = info.categories?.[0] || "General";
    form.description = info.description || "";
    form.coverUrl = info.imageLinks?.thumbnail || info.imageLinks?.smallThumbnail || "";

    showForm.value = true;
  } catch (error) {
    console.error(error);
    showMessage("Lookup failed. Please try again or enter details manually.", "error");
  } finally {
    loadingLookup.value = false;
  }
};

const ensureAuthor = async (firstName, lastName) => {
  const existingRes = await axios.get(`${API_BASE_URL}/api/authors`);
  const authors = existingRes.data.data || existingRes.data || [];
  const found = authors.find(
    (a) => a.firstName === firstName && a.lastName === lastName
  );
  if (found) return found.id;

  const created = await axios.post(
    `${API_BASE_URL}/api/authors`,
    { firstName, lastName },
    { headers }
  );
  return created.data.id;
};

const ensureCategory = async (name) => {
  const existingRes = await axios.get(`${API_BASE_URL}/api/categories`);
  const categories = existingRes.data.data || existingRes.data || [];
  const found = categories.find((c) => c.name === name);
  if (found) return found.id;

  const created = await axios.post(
    `${API_BASE_URL}/api/categories`,
    { name },
    { headers }
  );
  return created.data.id;
};
const addCopy = async () => {
  if (!duplicateBook.value) return;

  addingCopy.value = true;

  try {
    const book = duplicateBook.value;

    await axios.put(
      `${API_BASE_URL}/api/books/${book.id}`,
      {
        title: book.title,
        isbn: book.isbn,
        publicationYear: book.publicationYear,
        totalCopies: (book.totalCopies || 1) + 1,
        description: book.description || null,
        coverUrl: book.coverUrl || null,
        authorId: book.authorId,
        categoryId: book.categoryId,
      },
      { headers }
    );

    sessionBooks.value.push({
      title: book.title,
      isbn: book.isbn,
      author: book.authorName || "",
      category: book.categoryName || "",
    });

    showMessage(`Added one more copy of "${book.title}".`, "success");
    resetForm();
  } catch (error) {
    console.error(error);
    showMessage("Could not add a copy.", "error");
  } finally {
    addingCopy.value = false;
  }
};
const confirmAdd = async () => {
  saving.value = true;

  try {
    const authorId = await ensureAuthor(form.authorFirstName, form.authorLastName);
    const categoryId = await ensureCategory(form.categoryName);

    await axios.post(
      `${API_BASE_URL}/api/books`,
      {
        title: form.title,
        isbn: form.isbn,
        publicationYear: Number(form.publicationYear),
        description: form.description || null,
        coverUrl: form.coverUrl || null,
        authorId,
        categoryId,
      },
      { headers }
    );

    sessionBooks.value.push({
      title: form.title,
      isbn: form.isbn,
      author: `${form.authorFirstName} ${form.authorLastName}`,
      category: form.categoryName,
    });

    showMessage(`"${form.title}" added successfully.`, "success");
    resetForm();
  } catch (error) {
    console.error(error);

    const errorMessage =
      error.response?.data?.message ||
      error.response?.data ||
      "Could not add the book.";

    showMessage(errorMessage, "error");
  } finally {
    saving.value = false;
  }
};

const goAdmin = () => {
  router.push("/admin");
};

onBeforeUnmount(() => {
  stopScan();
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
  max-width: 1180px;
  margin: 0 auto;
}

.header {
  padding: 32px;
  margin-bottom: 24px;
  border-radius: 28px;
  background: white;
  box-shadow: 0 18px 45px rgba(15, 23, 42, 0.08);
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: 24px;
}

.eyebrow {
  margin: 0 0 8px;
  color: #166534;
  font-weight: 900;
}

.header h1 {
  margin: 0;
  font-size: 42px;
  color: #0f172a;
}

.header p {
  margin: 10px 0 0;
  color: #64748b;
  font-size: 17px;
}

.back-btn {
  height: 48px;
  padding: 0 18px;
  border: none;
  border-radius: 15px;
  background: #111;
  color: white;
  font-weight: 900;
  cursor: pointer;
}

.message {
  padding: 16px 20px;
  margin-bottom: 20px;
  border-radius: 16px;
  font-weight: 800;
}

.message.success {
  background: #dcfce7;
  color: #166534;
}

.message.error {
  background: #fee2e2;
  color: #991b1b;
}

.scan-card {
  display: grid;
  grid-template-columns: 1fr 1fr;
  gap: 24px;
  padding: 28px;
  margin-bottom: 24px;
  border-radius: 28px;
  background: white;
  border: 1px solid #e5e7eb;
  box-shadow: 0 18px 45px rgba(15, 23, 42, 0.08);
}

.scanner-region {
  width: 100%;
  min-height: 260px;
  border-radius: 18px;
  overflow: hidden;
  background: #0f172a;
}

.scan-controls {
  margin-top: 14px;
}

.manual-entry {
  margin-top: 22px;
  padding-top: 22px;
  border-top: 1px solid #e5e7eb;
}

.manual-entry label {
  display: block;
  margin-bottom: 8px;
  color: #64748b;
  font-size: 14px;
  font-weight: 900;
}

.manual-row {
  display: flex;
  gap: 10px;
}

.manual-row input {
  flex: 1;
  height: 48px;
  padding: 0 14px;
  border-radius: 13px;
  border: 1.5px solid #cbd5e1;
  background: #f8fafc;
}

.status-box {
  padding: 16px 18px;
  border-radius: 16px;
  background: #f8fafc;
  color: #475569;
  font-weight: 700;
}

.status-box.warning {
  background: #fef3c7;
  color: #92400e;
}

.book-form {
  display: grid;
  grid-template-columns: repeat(2, 1fr);
  gap: 16px;
}

.book-form .full {
  grid-column: span 2;
}

.form-group {
  display: flex;
  flex-direction: column;
  gap: 8px;
}

.form-group label {
  color: #64748b;
  font-size: 13px;
  font-weight: 900;
}

.form-group input,
.form-group textarea {
  padding: 0 14px;
  border-radius: 13px;
  border: 1.5px solid #cbd5e1;
  background: #f8fafc;
  font-size: 14px;
  font-family: inherit;
}

.form-group input {
  height: 46px;
}

.form-group textarea {
  padding: 10px 14px;
  resize: vertical;
}

.cover-preview {
  margin-top: 10px;
  width: 90px;
  height: 130px;
  object-fit: cover;
  border-radius: 10px;
}

.form-actions {
  display: flex;
  gap: 12px;
}

.primary-btn,
.secondary-btn {
  height: 48px;
  padding: 0 18px;
  border: none;
  border-radius: 14px;
  font-weight: 900;
  cursor: pointer;
}

.primary-btn {
  background: #166534;
  color: white;
}

.primary-btn:disabled {
  opacity: 0.6;
  cursor: not-allowed;
}

.secondary-btn {
  background: #f1f5f9;
  color: #334155;
}

.table-section {
  padding: 28px;
  border-radius: 28px;
  background: white;
  border: 1px solid #e5e7eb;
  box-shadow: 0 18px 45px rgba(15, 23, 42, 0.08);
}

.table-header h2 {
  margin: 0;
  font-size: 24px;
  color: #0f172a;
}

.table-header p {
  margin: 6px 0 0;
  color: #64748b;
  font-weight: 700;
}

.table-card {
  margin-top: 18px;
  border-radius: 20px;
  border: 1px solid #e5e7eb;
  overflow: hidden;
}

table {
  width: 100%;
  border-collapse: collapse;
}

th,
td {
  padding: 14px 16px;
  text-align: left;
  border-bottom: 1px solid #e5e7eb;
}

th {
  background: #f8fafc;
  color: #475569;
  font-size: 12px;
  font-weight: 900;
  text-transform: uppercase;
}

td {
  color: #0f172a;
  font-weight: 700;
}

.empty {
  padding: 24px;
  color: #64748b;
  font-weight: 700;
}

@media (max-width: 900px) {
  .scan-card,
  .book-form {
    grid-template-columns: 1fr;
  }

  .book-form .full {
    grid-column: span 1;
  }
}
</style>