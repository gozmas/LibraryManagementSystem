// Kütüphane sistemine kitapları toplu olarak ekler.
// Kullanım: node scripts/seed-books.mjs [dosya-adi.json]
// Varsayılan dosya: books-seed-data.json

import { readFile } from "fs/promises";

const API_BASE = "http://localhost:5239/api";
const ADMIN_EMAIL = "admin@library.com";
const ADMIN_PASSWORD = "Admin123!";

const dataFile = process.argv[2] || "books-seed-data.json";

const CATEGORY_COLORS = {
  Classic: "8B5E3C/FFFFFF",
  Dystopian: "3D3D3D/FFFFFF",
  Fantasy: "5B3A9C/FFFFFF",
  "Science Fiction": "1B4B66/FFFFFF",
  Gothic: "2B2B2B/E5E5E5",
  Mystery: "4A2545/FFFFFF",
  Fiction: "2F6F4E/FFFFFF",
  "Historical Fiction": "6B4226/FFFFFF",
  "Magical Realism": "A64B2A/FFFFFF",
  Philosophy: "444444/FFFFFF",
  History: "7A5230/FFFFFF",
  Science: "1F5673/FFFFFF",
  Psychology: "5C3D5C/FFFFFF",
  Biography: "3A5A40/FFFFFF",
  "Software Engineering": "1E3A5F/FFFFFF",
  "Computer Science": "24435E/FFFFFF",
  Horror: "1A1A1A/D40000",
};

function buildCoverUrl(book) {
  const colors = CATEGORY_COLORS[book.category] || "555555/FFFFFF";
  const text = encodeURIComponent(book.title);
  return `https://placehold.co/300x440/${colors}?text=${text}&font=roboto`;
}

async function login() {
  const res = await fetch(`${API_BASE}/auth/login`, {
    method: "POST",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify({ email: ADMIN_EMAIL, password: ADMIN_PASSWORD }),
  });

  if (!res.ok) {
    throw new Error(`Login failed: ${res.status} ${await res.text()}`);
  }

  const data = await res.json();
  return data.data.token;
}

async function getExisting(endpoint, token) {
  const res = await fetch(`${API_BASE}/${endpoint}`, {
    headers: { Authorization: `Bearer ${token}` },
  });
  if (!res.ok) throw new Error(`Failed to fetch ${endpoint}`);
  return res.json();
}

async function ensureAuthor(book, authorMap, token) {
  const key = `${book.firstName}|${book.lastName}`;
  if (authorMap.has(key)) return authorMap.get(key);

  const res = await fetch(`${API_BASE}/authors`, {
    method: "POST",
    headers: {
      "Content-Type": "application/json",
      Authorization: `Bearer ${token}`,
    },
    body: JSON.stringify({
      firstName: book.firstName,
      lastName: book.lastName,
    }),
  });

  if (res.status === 201) {
    const created = await res.json();
    authorMap.set(key, created.id);
    return created.id;
  }

  if (res.status === 409) {
    const authors = await getExisting("authors", token);
    const found = authors.find(
      (a) => a.firstName === book.firstName && a.lastName === book.lastName
    );
    if (found) {
      authorMap.set(key, found.id);
      return found.id;
    }
  }

  throw new Error(`Could not create/find author ${key}: ${res.status}`);
}

async function ensureCategory(book, categoryMap, token) {
  if (categoryMap.has(book.category)) return categoryMap.get(book.category);

  const res = await fetch(`${API_BASE}/categories`, {
    method: "POST",
    headers: {
      "Content-Type": "application/json",
      Authorization: `Bearer ${token}`,
    },
    body: JSON.stringify({ name: book.category }),
  });

  if (res.status === 201) {
    const created = await res.json();
    categoryMap.set(book.category, created.id);
    return created.id;
  }

  if (res.status === 409) {
    const categories = await getExisting("categories", token);
    const found = categories.find((c) => c.name === book.category);
    if (found) {
      categoryMap.set(book.category, found.id);
      return found.id;
    }
  }

  throw new Error(`Could not create/find category ${book.category}: ${res.status}`);
}

async function createBook(book, authorId, categoryId, token) {
  const res = await fetch(`${API_BASE}/books`, {
    method: "POST",
    headers: {
      "Content-Type": "application/json",
      Authorization: `Bearer ${token}`,
    },
    body: JSON.stringify({
      title: book.title,
      isbn: book.isbn,
      publicationYear: book.year,
      description: book.description,
      coverUrl: buildCoverUrl(book),
      authorId,
      categoryId,
    }),
  });

  if (res.status === 201) return { ok: true };
  return { ok: false, status: res.status, body: await res.text() };
}

async function main() {
  const raw = await readFile(new URL(`./${dataFile}`, import.meta.url));
  const books = JSON.parse(raw);

  console.log(`Reading from ${dataFile}...`);
  console.log("Logging in as admin...");
  const token = await login();

  const existingAuthors = await getExisting("authors", token);
  const existingCategories = await getExisting("categories", token);

  const authorMap = new Map(
    existingAuthors.map((a) => [`${a.firstName}|${a.lastName}`, a.id])
  );
  const categoryMap = new Map(existingCategories.map((c) => [c.name, c.id]));

  let created = 0;
  let skipped = 0;

  for (const book of books) {
    try {
      const authorId = await ensureAuthor(book, authorMap, token);
      const categoryId = await ensureCategory(book, categoryMap, token);
      const result = await createBook(book, authorId, categoryId, token);

      if (result.ok) {
        console.log(`✔ Added: ${book.title}`);
        created++;
      } else {
        console.log(`✘ Skipped: ${book.title} (${result.status})`);
        skipped++;
      }
    } catch (err) {
      console.log(`✘ Error on "${book.title}": ${err.message}`);
      skipped++;
    }
  }

  console.log(`\nDone. Created: ${created}, Skipped: ${skipped}`);
}

main().catch((err) => {
  console.error("Fatal error:", err);
  process.exit(1);
});