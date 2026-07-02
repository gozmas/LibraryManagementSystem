<template>
  <div class="page">
    <AppTopbar />

    <main class="content">
      <section class="header">
        <div>
          <p class="eyebrow">Admin Management</p>
          <h1>Book Management</h1>
          <p>Add, update and delete books, authors and categories.</p>
        </div>

        <button class="back-btn" @click="goAdmin">
          Back to Dashboard
        </button>
      </section>

      <p v-if="message" :class="['message', messageType]">
        {{ message }}
      </p>

      <section class="mini-management">
        <div class="mini-card">
          <div class="form-header">
            <h2>{{ isEditingAuthor ? "Update Author" : "Add New Author" }}</h2>
            <p>
              {{
                isEditingAuthor
                  ? "Edit the selected author's information."
                  : "Create an author before adding a book."
              }}
            </p>
          </div>

          <form class="mini-form" @submit.prevent="submitAuthor">
            <div class="form-group">
              <label>First Name</label>
              <input
                v-model="authorForm.firstName"
                type="text"
                placeholder="Author first name"
              />
            </div>

            <div class="form-group">
              <label>Last Name</label>
              <input
                v-model="authorForm.lastName"
                type="text"
                placeholder="Author last name"
              />
            </div>

            <div class="form-group full">
              <label>Biography</label>
              <textarea
                v-model="authorForm.biography"
                rows="3"
                placeholder="Short biography (optional)"
              ></textarea>
            </div>

            <div class="form-actions full">
              <button class="primary-btn" type="submit">
                {{ isEditingAuthor ? "Save Changes" : "Add Author" }}
              </button>

              <button
                v-if="isEditingAuthor"
                class="secondary-btn"
                type="button"
                @click="cancelAuthorEdit"
              >
                Cancel Edit
              </button>
            </div>
          </form>

          <div class="mini-table">
            <div v-for="author in authors" :key="author.id" class="mini-row">
              <div>
                <strong>{{ getAuthorName(author) }}</strong>
                <small>{{ author.biography || "No biography" }}</small>
              </div>

              <div class="mini-row-actions">
                <button class="edit-btn" @click="startEditAuthor(author)">
                  Edit
                </button>

                <button class="delete-btn" @click="deleteAuthor(author.id)">
                  Delete
                </button>
              </div>
            </div>

            <p v-if="authors.length === 0" class="empty">No authors yet.</p>
          </div>
        </div>

        <div class="mini-card">
          <div class="form-header">
            <h2>{{ isEditingCategory ? "Update Category" : "Add New Category" }}</h2>
            <p>
              {{
                isEditingCategory
                  ? "Edit the selected category's information."
                  : "Create a category before adding a book."
              }}
            </p>
          </div>

          <form class="mini-form" @submit.prevent="submitCategory">
            <div class="form-group full">
              <label>Category Name</label>
              <input
                v-model="categoryForm.name"
                type="text"
                placeholder="Category name"
              />
            </div>

            <div class="form-group full">
              <label>Description</label>
              <textarea
                v-model="categoryForm.description"
                rows="3"
                placeholder="Short description (optional)"
              ></textarea>
            </div>

            <div class="form-actions full">
              <button class="primary-btn" type="submit">
                {{ isEditingCategory ? "Save Changes" : "Add Category" }}
              </button>

              <button
                v-if="isEditingCategory"
                class="secondary-btn"
                type="button"
                @click="cancelCategoryEdit"
              >
                Cancel Edit
              </button>
            </div>
          </form>

          <div class="mini-table">
            <div v-for="category in categories" :key="category.id" class="mini-row">
              <div>
                <strong>{{ category.name }}</strong>
                <small>{{ category.description || "No description" }}</small>
              </div>

              <div class="mini-row-actions">
                <button class="edit-btn" @click="startEditCategory(category)">
                  Edit
                </button>

                <button class="delete-btn" @click="deleteCategory(category.id)">
                  Delete
                </button>
              </div>
            </div>

            <p v-if="categories.length === 0" class="empty">No categories yet.</p>
          </div>
        </div>
      </section>

      <section class="form-card">
        <div class="form-header">
          <h2>{{ isEditing ? "Update Book" : "Add New Book" }}</h2>
          <p>
            {{
              isEditing
                ? "Edit the selected book information."
                : "Create a new book record."
            }}
          </p>
        </div>

        <form class="book-form" @submit.prevent="submitBook">
          <div class="form-group">
            <label>Title</label>
            <input
              v-model="form.title"
              type="text"
              placeholder="Book title"
            />
          </div>

          <div class="form-group">
            <label>ISBN</label>
            <input
              v-model="form.isbn"
              type="text"
              placeholder="ISBN"
            />
          </div>

          <div class="form-group">
            <label>Publication Year</label>
            <input
              v-model.number="form.publicationYear"
              type="number"
              placeholder="Publication year"
            />
          </div>
          <div class="form-group">
            <label>Total Copies</label>
            <input
              v-model.number="form.totalCopies"
              type="number"
              min="1"
              placeholder="Total copies"
            />
          </div>

          <div class="form-group">
            <label>Author</label>
            <select v-model.number="form.authorId">
              <option disabled value="">Select author</option>

              <option
                v-for="author in authors"
                :key="author.id"
                :value="author.id"
              >
                {{ getAuthorName(author) }}
              </option>
            </select>
          </div>

          <div class="form-group">
            <label>Category</label>
            <select v-model.number="form.categoryId">
              <option disabled value="">Select category</option>

              <option
                v-for="category in categories"
                :key="category.id"
                :value="category.id"
              >
                {{ category.name }}
              </option>
            </select>
          </div>

          <div class="form-actions">
            <button class="primary-btn" type="submit">
              {{ isEditing ? "Save Changes" : "Add Book" }}
            </button>

            <button
              v-if="isEditing"
              class="secondary-btn"
              type="button"
              @click="cancelEdit"
            >
              Cancel Edit
            </button>
          </div>
        </form>
      </section>

      <section class="table-section">
        <div class="table-header">
          <div>
            <h2>Books</h2>
            <p>{{ books.length }} books found in the system.</p>
          </div>

          <input
            v-model="search"
            class="search"
            type="text"
            placeholder="Search books..."
          />
        </div>

        <div class="table-card">
          <table>
            <thead>
              <tr>
                <th>Book</th>
                <th>Author</th>
                <th>Category</th>
                <th>Year</th>
                <th>Copies</th>
                <th>Status</th>
                <th class="actions-column">Actions</th>
              </tr>
            </thead>

            <tbody>
              <tr v-for="book in filteredBooks" :key="book.id">
                <td>
                  <strong>{{ book.title }}</strong>
                  <small>{{ book.isbn }}</small>
                </td>

                <td>{{ book.authorName || "-" }}</td>
                <td>{{ book.categoryName || "-" }}</td>
                <td>{{ book.publicationYear || "-" }}</td>
                <td>{{ book.availableCopies ?? 0 }} / {{ book.totalCopies ?? 1 }}</td>

                <td>
                  <span
                    :class="['status', book.isAvailable ? 'available' : 'borrowed']"
                  >
                    {{ book.isAvailable ? "Available" : "Borrowed" }}
                  </span>
                </td>

                <td class="actions-column">
                  <button class="edit-btn" @click="startEdit(book)">
                    Edit
                  </button>

                  <button class="delete-btn" @click="deleteBook(book.id)">
                    Delete
                  </button>
                </td>
              </tr>
            </tbody>
          </table>

          <p v-if="filteredBooks.length === 0" class="empty">
            No books found.
          </p>
        </div>
      </section>
    </main>
  </div>
</template>

<script setup>
import { computed, onMounted, reactive, ref } from "vue";
import { useRouter } from "vue-router";
import axios from "axios";

import AppTopbar from "@/components/AppTopbar.vue";

const router = useRouter();

const API_BASE_URL = "http://localhost:5239";

const books = ref([]);
const authors = ref([]);
const categories = ref([]);

const search = ref("");
const message = ref("");
const messageType = ref("success");

const isEditing = ref(false);
const editingBookId = ref(null);

const isEditingAuthor = ref(false);
const editingAuthorId = ref(null);

const isEditingCategory = ref(false);
const editingCategoryId = ref(null);

const token = localStorage.getItem("token");

const headers = {
  Authorization: `Bearer ${token}`,
};

const form = reactive({
  title: "",
  isbn: "",
  publicationYear: "",
  totalCopies: 1,
  authorId: "",
  categoryId: "",
});

const authorForm = reactive({
  firstName: "",
  lastName: "",
  biography: "",
});

const categoryForm = reactive({
  name: "",
  description: "",
});

const normalize = (response) => {
  return response.data.data || response.data || [];
};

const showMessage = (text, type = "success") => {
  message.value = text;
  messageType.value = type;
};

const getAuthorName = (author) => {
  if (author.name) return author.name;

  return `${author.firstName || ""} ${author.lastName || ""}`.trim();
};

const resetForm = () => {
  form.title = "";
  form.isbn = "";
  form.publicationYear = "";
  form.totalCopies = 1;
  form.authorId = "";
  form.categoryId = "";
};

const resetAuthorForm = () => {
  authorForm.firstName = "";
  authorForm.lastName = "";
  authorForm.biography = "";
};

const resetCategoryForm = () => {
  categoryForm.name = "";
  categoryForm.description = "";
};

const getData = async () => {
  try {
    const [bookRes, authorRes, categoryRes] = await Promise.all([
      axios.get(`${API_BASE_URL}/api/books`),
      axios.get(`${API_BASE_URL}/api/authors`),
      axios.get(`${API_BASE_URL}/api/categories`),
    ]);

    books.value = normalize(bookRes);
    authors.value = normalize(authorRes);
    categories.value = normalize(categoryRes);
  } catch (error) {
    console.error(error);
    showMessage("Book management data could not be loaded.", "error");
  }
};

const submitAuthor = async () => {
  if (!authorForm.firstName || !authorForm.lastName) {
    showMessage("Please fill in author first name and last name.", "error");
    return;
  }

  const payload = {
    firstName: authorForm.firstName,
    lastName: authorForm.lastName,
    biography: authorForm.biography || null,
  };

  try {
    if (isEditingAuthor.value) {
      await axios.put(
        `${API_BASE_URL}/api/authors/${editingAuthorId.value}`,
        payload,
        { headers }
      );

      showMessage("Author updated successfully.", "success");
    } else {
      await axios.post(`${API_BASE_URL}/api/authors`, payload, { headers });

      showMessage("Author added successfully.", "success");
    }

    resetAuthorForm();
    isEditingAuthor.value = false;
    editingAuthorId.value = null;

    await getData();
  } catch (error) {
    console.error(error);

    const errorMessage =
      error.response?.data?.message ||
      error.response?.data ||
      "Author operation failed.";

    showMessage(errorMessage, "error");
  }
};

const startEditAuthor = (author) => {
  message.value = "";

  isEditingAuthor.value = true;
  editingAuthorId.value = author.id;

  authorForm.firstName = author.firstName || "";
  authorForm.lastName = author.lastName || "";
  authorForm.biography = author.biography || "";

  window.scrollTo({ top: 0, behavior: "smooth" });
};

const cancelAuthorEdit = () => {
  isEditingAuthor.value = false;
  editingAuthorId.value = null;
  resetAuthorForm();
  message.value = "";
};

const deleteAuthor = async (authorId) => {
  const confirmed = confirm("Are you sure you want to delete this author?");

  if (!confirmed) return;

  try {
    await axios.delete(`${API_BASE_URL}/api/authors/${authorId}`, {
      headers,
    });

    showMessage("Author deleted successfully.", "success");

    await getData();
  } catch (error) {
    console.error(error);

    const errorMessage =
      error.response?.data?.message ||
      error.response?.data ||
      "Author delete failed.";

    showMessage(errorMessage, "error");
  }
};

const submitCategory = async () => {
  if (!categoryForm.name) {
    showMessage("Please enter category name.", "error");
    return;
  }

  const payload = {
    name: categoryForm.name,
    description: categoryForm.description || null,
  };

  try {
    if (isEditingCategory.value) {
      await axios.put(
        `${API_BASE_URL}/api/categories/${editingCategoryId.value}`,
        payload,
        { headers }
      );

      showMessage("Category updated successfully.", "success");
    } else {
      await axios.post(`${API_BASE_URL}/api/categories`, payload, { headers });

      showMessage("Category added successfully.", "success");
    }

    resetCategoryForm();
    isEditingCategory.value = false;
    editingCategoryId.value = null;

    await getData();
  } catch (error) {
    console.error(error);

    const errorMessage =
      error.response?.data?.message ||
      error.response?.data ||
      "Category operation failed.";

    showMessage(errorMessage, "error");
  }
};

const startEditCategory = (category) => {
  message.value = "";

  isEditingCategory.value = true;
  editingCategoryId.value = category.id;

  categoryForm.name = category.name || "";
  categoryForm.description = category.description || "";

  window.scrollTo({ top: 0, behavior: "smooth" });
};

const cancelCategoryEdit = () => {
  isEditingCategory.value = false;
  editingCategoryId.value = null;
  resetCategoryForm();
  message.value = "";
};

const deleteCategory = async (categoryId) => {
  const confirmed = confirm("Are you sure you want to delete this category?");

  if (!confirmed) return;

  try {
    await axios.delete(`${API_BASE_URL}/api/categories/${categoryId}`, {
      headers,
    });

    showMessage("Category deleted successfully.", "success");

    await getData();
  } catch (error) {
    console.error(error);

    const errorMessage =
      error.response?.data?.message ||
      error.response?.data ||
      "Category delete failed.";

    showMessage(errorMessage, "error");
  }
};

const filteredBooks = computed(() => {
  const value = search.value.toLowerCase();

  return books.value.filter((book) => {
    const title = book.title?.toLowerCase() || "";
    const author = book.authorName?.toLowerCase() || "";
    const category = book.categoryName?.toLowerCase() || "";
    const isbn = book.isbn?.toLowerCase() || "";

    return (
      title.includes(value) ||
      author.includes(value) ||
      category.includes(value) ||
      isbn.includes(value)
    );
  });
});

const submitBook = async () => {
  if (
    !form.title ||
    !form.isbn ||
    !form.publicationYear ||
    !form.authorId ||
    !form.categoryId
  ) {
    showMessage("Please fill in all book fields.", "error");
    return;
  }

  const payload = {
    title: form.title,
    isbn: form.isbn,
    publicationYear: Number(form.publicationYear),
    totalCopies: Number(form.totalCopies) || 1,
    authorId: Number(form.authorId),
    categoryId: Number(form.categoryId),
  };

  try {
    if (isEditing.value) {
      await axios.put(
        `${API_BASE_URL}/api/books/${editingBookId.value}`,
        payload,
        { headers }
      );

      showMessage("Book updated successfully.", "success");
    } else {
      await axios.post(`${API_BASE_URL}/api/books`, payload, { headers });

      showMessage("Book added successfully.", "success");
    }

    resetForm();
    isEditing.value = false;
    editingBookId.value = null;

    await getData();
  } catch (error) {
    console.error(error);

    const errorMessage =
      error.response?.data?.message ||
      error.response?.data ||
      "Book operation failed.";

    showMessage(errorMessage, "error");
  }
};

const startEdit = (book) => {
  message.value = "";
  messageType.value = "success";

  isEditing.value = true;
  editingBookId.value = book.id;

  form.title = book.title || "";
  form.isbn = book.isbn || "";
  form.publicationYear = book.publicationYear || "";
  form.totalCopies = book.totalCopies || 1;
  form.authorId = book.authorId || "";
  form.categoryId = book.categoryId || "";

  window.scrollTo({
    top: 0,
    behavior: "smooth",
  });
};

const cancelEdit = () => {
  isEditing.value = false;
  editingBookId.value = null;
  resetForm();
  message.value = "";
};

const deleteBook = async (bookId) => {
  const confirmed = confirm("Are you sure you want to delete this book?");

  if (!confirmed) return;

  try {
    await axios.delete(`${API_BASE_URL}/api/books/${bookId}`, {
      headers,
    });

    showMessage("Book deleted successfully.", "success");

    await getData();
  } catch (error) {
    console.error(error);

    const errorMessage =
      error.response?.data?.message ||
      error.response?.data ||
      "Book delete failed.";

    showMessage(errorMessage, "error");
  }
};

const goAdmin = () => {
  router.push("/admin");
};

onMounted(getData);
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

.mini-management {
  display: grid;
  grid-template-columns: repeat(2, 1fr);
  gap: 24px;
  margin-bottom: 24px;
  align-items: start;
}

.mini-card,
.form-card,
.table-section {
  padding: 28px;
  border-radius: 28px;
  background: white;
  border: 1px solid #e5e7eb;
  box-shadow: 0 18px 45px rgba(15, 23, 42, 0.08);
}

.form-card,
.table-section {
  margin-bottom: 24px;
}

.form-header {
  padding-bottom: 20px;
  margin-bottom: 22px;
  border-bottom: 1px solid #e5e7eb;
}

.form-header h2,
.table-header h2 {
  margin: 0;
  color: #0f172a;
  font-size: 26px;
}

.form-header p,
.table-header p {
  margin: 7px 0 0;
  color: #64748b;
  font-weight: 700;
}

.mini-form,
.book-form {
  display: grid;
  grid-template-columns: repeat(2, 1fr);
  gap: 18px;
}

.mini-form .full,
.form-actions.full {
  grid-column: span 2;
}

.form-group {
  display: flex;
  flex-direction: column;
  gap: 8px;
}

.form-group label {
  color: #64748b;
  font-size: 14px;
  font-weight: 900;
}

.form-group input,
.form-group select,
.form-group textarea {
  padding: 0 16px;
  border-radius: 15px;
  border: 1.5px solid #cbd5e1;
  background: #f8fafc;
  color: #0f172a;
  font-size: 15px;
  font-weight: 700;
  outline: none;
  font-family: inherit;
}

.form-group input,
.form-group select {
  height: 52px;
}

.form-group textarea {
  padding: 12px 16px;
  resize: vertical;
}

.form-group input:focus,
.form-group select:focus,
.form-group textarea:focus {
  border-color: #166534;
  background: white;
}

.form-actions {
  display: flex;
  gap: 12px;
  align-items: end;
}

.primary-btn,
.secondary-btn {
  height: 52px;
  padding: 0 18px;
  border: none;
  border-radius: 15px;
  font-weight: 900;
  cursor: pointer;
}

.primary-btn {
  background: #166534;
  color: white;
}

.secondary-btn {
  background: #f1f5f9;
  color: #334155;
}

.mini-table {
  margin-top: 22px;
  padding-top: 22px;
  border-top: 1px solid #e5e7eb;
  display: grid;
  gap: 10px;
  max-height: 260px;
  overflow-y: auto;
}

.mini-row {
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: 14px;
  padding: 12px 14px;
  border-radius: 14px;
  background: #f8fafc;
  border: 1px solid #e5e7eb;
}

.mini-row strong {
  display: block;
  color: #0f172a;
  font-size: 15px;
}

.mini-row small {
  display: block;
  margin-top: 3px;
  color: #64748b;
  font-weight: 600;
  max-width: 320px;
  overflow: hidden;
  text-overflow: ellipsis;
  white-space: nowrap;
}

.mini-row-actions {
  display: flex;
  gap: 8px;
  flex-shrink: 0;
}

.table-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  gap: 18px;
  margin-bottom: 18px;
}

.search {
  width: 320px;
  height: 46px;
  padding: 0 15px;
  border-radius: 14px;
  border: 1.5px solid #cbd5e1;
  background: #f8fafc;
  font-weight: 700;
  outline: none;
}

.search:focus {
  border-color: #166534;
  background: white;
}

.table-card {
  border-radius: 22px;
  border: 1px solid #e5e7eb;
  overflow: hidden;
}

table {
  width: 100%;
  border-collapse: collapse;
}

th,
td {
  padding: 16px 18px;
  text-align: left;
  border-bottom: 1px solid #e5e7eb;
}

th {
  background: #f8fafc;
  color: #475569;
  font-size: 13px;
  font-weight: 900;
  text-transform: uppercase;
}

td {
  color: #0f172a;
  font-weight: 700;
}

td small {
  display: block;
  margin-top: 4px;
  color: #64748b;
  font-weight: 600;
}

tr:last-child td {
  border-bottom: none;
}

.status {
  padding: 6px 11px;
  border-radius: 999px;
  font-size: 12px;
  font-weight: 900;
}

.status.available {
  background: #dcfce7;
  color: #166534;
}

.status.borrowed {
  background: #fee2e2;
  color: #991b1b;
}

.actions-column {
  text-align: right;
  white-space: nowrap;
}

.edit-btn,
.delete-btn {
  height: 38px;
  padding: 0 13px;
  border: none;
  border-radius: 12px;
  font-weight: 900;
  cursor: pointer;
}

.edit-btn {
  background: #f1f5f9;
  color: #334155;
  margin-right: 8px;
}

.delete-btn {
  background: #fee2e2;
  color: #991b1b;
}

.empty {
  padding: 24px;
  color: #64748b;
  font-weight: 700;
}

@media (max-width: 900px) {
  .header,
  .table-header {
    flex-direction: column;
    align-items: flex-start;
  }

  .mini-management,
  .mini-form,
  .book-form {
    grid-template-columns: 1fr;
  }

  .mini-form .full,
  .form-actions.full {
    grid-column: span 1;
  }

  .search {
    width: 100%;
  }

  .table-card {
    overflow-x: auto;
  }

  table {
    min-width: 900px;
  }

  .form-actions {
    flex-direction: column;
    align-items: stretch;
  }
}
</style>