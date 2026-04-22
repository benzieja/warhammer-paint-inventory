# Warhammer Paint Inventory

A personal web app to track your Warhammer 40K / Age of Sigmar paint collection. Built with Blazor WebAssembly, hosted on Cloudflare Pages, and synced across devices via Supabase.

---

## Features

- View all 277 Citadel paints with rack location (4 racks, 12 rows × 6 columns)
- Mark paints as owned / not owned
- Filter by category (Base, Layer, Contrast, Dry, Technical, Shade)
- Search by name
- Visual rack grid view
- Wish list of missing paints with clipboard export
- Stats dashboard with progress by category
- Multi-device sync — changes on phone appear on PC and desktop app
- Login wall via Cloudflare Access (only your email can get in)

---

## Architecture

```
GitHub Repo
    │
    └── GitHub Actions (on push to main)
            │
            └── dotnet publish → Cloudflare Pages (static site)
                                        │
                                 Cloudflare Access
                                 (email login wall)
                                        │
                                  Browser runs
                                  Blazor WASM
                                        │
                                   Supabase DB
                                 (owned_paints table)
                                        │
                          Also used by desktop apps
                          (Console + WPF GUI)
```

---

## Tech Stack

| Layer | Technology |
|---|---|
| Frontend | Blazor WebAssembly (.NET 8) |
| Hosting | Cloudflare Pages (free) |
| Auth | Cloudflare Access / Zero Trust (free) |
| Database | Supabase PostgreSQL (free tier) |
| CI/CD | GitHub Actions |
| Desktop apps | .NET 8 Console + .NET 10 WPF |

---

## Project Structure

```
WarhamerPaintInventoryWeb/
├── Models/
│   ├── Paint.cs              # Paint model with CSS category colours
│   └── PaintDatabase.cs      # All 277 paints with rack positions
├── Services/
│   └── InventoryManager.cs   # Supabase REST API calls (read/write owned status)
├── Pages/
│   ├── Home.razor            # Dashboard — stats + category breakdown
│   ├── AllPaints.razor       # Full list with search, filter, owned toggle
│   ├── ByRack.razor          # Visual rack grid (click to toggle owned)
│   └── WishList.razor        # Missing paints grouped by category
├── Layout/
│   ├── MainLayout.razor      # Sidebar + main content layout
│   └── NavMenu.razor         # Navigation links
├── wwwroot/
│   └── css/app.css           # Dark Warhammer theme
└── .github/workflows/
    └── deploy.yml            # GitHub Actions build + Cloudflare deploy
```

---

## Supabase Database

Single table: `owned_paints`

```sql
CREATE TABLE owned_paints (
  name TEXT PRIMARY KEY
);

ALTER TABLE owned_paints ENABLE ROW LEVEL SECURITY;
CREATE POLICY "Allow select" ON owned_paints FOR SELECT USING (true);
CREATE POLICY "Allow insert" ON owned_paints FOR INSERT WITH CHECK (true);
CREATE POLICY "Allow delete" ON owned_paints FOR DELETE USING (true);
```

Only owned paint names are stored. The full paint list and rack locations are always built from `PaintDatabase.cs` at runtime.

---

## Deployment

### GitHub Actions (`deploy.yml`)

Triggers on every push to `main`:
1. Checks out the repo
2. Installs .NET 8
3. Runs `dotnet publish -c Release -o release`
4. Deploys `release/wwwroot` to Cloudflare Pages via `cloudflare/pages-action`

### Required GitHub Secrets

| Secret | Where to find it |
|---|---|
| `CLOUDFLARE_API_TOKEN` | Cloudflare → My Profile → API Tokens |
| `CLOUDFLARE_ACCOUNT_ID` | Cloudflare → Workers & Pages (right sidebar) |

---

## Cloudflare Access Setup

1. Cloudflare Zero Trust → Access → Applications → Add application → Self-hosted
2. Application domain: `warhammer-paint-inventory.pages.dev`
3. Policy: Allow → Emails → your email address

Anyone visiting the URL will be asked to verify their email with a one-time code. Only the allowed email address gets through.

---

## Desktop Apps

Both the console app (`WarhamerPaintInventory`) and GUI app (`WarhamerPaintInventoryGUI`) sync with the same Supabase database.

**On startup:** loads local `inventory.json` first (works offline), then fetches latest owned status from Supabase and updates the local file.

**On toggle:** saves to local file immediately, then syncs to Supabase in the background.

`IsWishListed` (GUI only) is stored locally and is not synced to Supabase.

### Building Desktop Apps

```bash
# Console
cd WarhamerPaintInventory
dotnet publish -c Release -r win-x64 --self-contained false
# Copy exe → G:\Warhammer\Release\

# GUI
cd WarhamerPaintInventoryGUI
dotnet publish -c Release -r win-x64 --self-contained false
# Copy exe → G:\Warhammer\ReleaseGUI\
```

---

## Paint Layout

- **4 racks**, each **12 rows × 6 columns = 72 slots**
- **288 total slots**, 277 paints (11 empty)
- Fill order: top-to-bottom, left-to-right, Rack 1 first
- Category order on shelves: Base → Layer → Contrast → Dry → Technical → Shade
