// Sistemdeki TÜM kitaplara (kapağı olsun olmasın) kapak fotoğrafı ekler/günceller.
// Kullanım: node scripts/backfill-covers.mjs

const API_BASE = "http://localhost:5239/api";
const ADMIN_EMAIL = "admin@library.com";
const ADMIN_PASSWORD = "Admin123!";

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
  const colors = CATEGORY_COLORS[book.categoryName] || "555555/FFFFFF";
  const text = encodeURIComponent(book.title);
  return `https://placehold.co/300x440/${colors}?text=${text}&font=roboto`;
}

async function login() {
  const res = await fetch(`${API_BASE}/auth/login`, {
    method: "POST",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify({ email: ADMIN_EMAIL, password: ADMIN_PASSWORD }),
  });

  if (!res.ok) throw new Error(`Login failed: ${res.status}`);
  const data = await res.json();
  return data.data.token;
}

async function main() {
  const token = await login();
  const headers = {
    "Content-Type": "application/json",
    Authorization: `Bearer ${token}`,
  };

  const books = await (await fetch(`${API_BASE}/books`, { headers })).json();

  console.log(`Found ${books.length} books.`);

  let updated = 0;
  let skipped = 0;

  for (const book of books) {
    if (book.coverUrl) {
      console.log(`- Skipping (already has cover): ${book.title}`);
      skipped++;
      continue;
    }

    const res = await fetch(`${API_BASE}/books/${book.id}`, {
      method: "PUT",
      headers,
      body: JSON.stringify({
        title: book.title,
        isbn: book.isbn,
        publicationYear: book.publicationYear,
        description: book.description,
        coverUrl: buildCoverUrl(book),
        authorId: book.authorId,
        categoryId: book.categoryId,
      }),
    });

    if (res.ok) {
      console.log(`✔ Cover added: ${book.title}`);
      updated++;
    } else {
      console.log(`✘ Failed: ${book.title} (${res.status})`);
      skipped++;
    }
  }

  console.log(`\nDone. Updated: ${updated}, Skipped: ${skipped}`);
}

main().catch((err) => {
  console.error("Fatal error:", err);
  process.exit(1);
});