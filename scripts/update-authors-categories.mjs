// Var olan tüm yazarlara Biography, kategorilere Description ekler.
// Kullanım: node scripts/update-authors-categories.mjs

import { readFile } from "fs/promises";

const API_BASE = "http://localhost:5239/api";
const ADMIN_EMAIL = "admin@library.com";
const ADMIN_PASSWORD = "Admin123!";

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
  const raw = await readFile(new URL("./authors-categories-data-2.json", import.meta.url));
  const { authors, categories } = JSON.parse(raw);

  const token = await login();
  const headers = {
    "Content-Type": "application/json",
    Authorization: `Bearer ${token}`,
  };

  const existingAuthors = await (await fetch(`${API_BASE}/authors`, { headers })).json();
  const existingCategories = await (await fetch(`${API_BASE}/categories`, { headers })).json();

  let authorsUpdated = 0;
  let authorsMissing = 0;

  for (const a of authors) {
    const match = existingAuthors.find(
      (x) => x.firstName === a.firstName && x.lastName === a.lastName
    );

    if (!match) {
      console.log(`✘ Author not found: ${a.firstName} ${a.lastName}`);
      authorsMissing++;
      continue;
    }

    const res = await fetch(`${API_BASE}/authors/${match.id}`, {
      method: "PUT",
      headers,
      body: JSON.stringify({
        firstName: match.firstName,
        lastName: match.lastName,
        biography: a.biography,
      }),
    });

    if (res.ok) {
      console.log(`✔ Updated author: ${a.firstName} ${a.lastName}`);
      authorsUpdated++;
    } else {
      console.log(`✘ Failed to update ${a.firstName} ${a.lastName}: ${res.status}`);
    }
  }

  let categoriesUpdated = 0;
  let categoriesMissing = 0;

  for (const c of categories) {
    const match = existingCategories.find((x) => x.name === c.name);

    if (!match) {
      console.log(`✘ Category not found: ${c.name}`);
      categoriesMissing++;
      continue;
    }

    const res = await fetch(`${API_BASE}/categories/${match.id}`, {
      method: "PUT",
      headers,
      body: JSON.stringify({
        name: match.name,
        description: c.description,
      }),
    });

    if (res.ok) {
      console.log(`✔ Updated category: ${c.name}`);
      categoriesUpdated++;
    } else {
      console.log(`✘ Failed to update ${c.name}: ${res.status}`);
    }
  }

  console.log(
    `\nDone. Authors updated: ${authorsUpdated} (missing: ${authorsMissing}), Categories updated: ${categoriesUpdated} (missing: ${categoriesMissing})`
  );
}

main().catch((err) => {
  console.error("Fatal error:", err);
  process.exit(1);
});