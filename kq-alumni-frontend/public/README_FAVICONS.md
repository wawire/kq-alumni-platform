# Add Your Favicon Files Here

## Quick Start

I've configured the Next.js app to use favicons, but the actual image files need to be added to this `/public` directory.

## What's Already Done ✅

1. **Metadata configured** in `src/app/layout.tsx`
2. **Web manifest** created (`site.webmanifest`)
3. **Browser config** created (`browserconfig.xml`)
4. **All links and meta tags** set up correctly

## What You Need to Do

### Add These Image Files to `/public`:

```
/public
├── favicon.ico              # 32x32 or 48x48
├── favicon-16x16.png        # 16x16
├── favicon-32x32.png        # 32x32
├── apple-touch-icon.png     # 180x180
├── android-chrome-192x192.png  # 192x192
├── android-chrome-512x512.png  # 512x512
└── mstile-150x150.png       # 150x150 (optional)
```

## How to Generate Favicons

### Method 1: Online Generator (Easiest)
1. Go to: **https://realfavicongenerator.net/**
2. Upload your KQ logo or icon design
3. Customize:
   - Background color: `#DC143C` (KQ Red)
   - Or use transparent background
4. Download the generated package
5. Extract ALL files to `/public`
6. Done!

### Method 2: From Existing Logo
If you have a high-quality logo file:

1. Use any image editor (Photoshop, GIMP, Figma, etc.)
2. Create a square canvas for each size
3. Add KQ Red background: `#DC143C`
4. Center the white logo
5. Export as PNG (or ICO for favicon.ico)

Sizes needed:
- 16×16 → `favicon-16x16.png`
- 32×32 → `favicon-32x32.png` + `favicon.ico`
- 180×180 → `apple-touch-icon.png`
- 192×192 → `android-chrome-192x192.png`
- 512×512 → `android-chrome-512x512.png`
- 150×150 → `mstile-150x150.png` (optional)

### Method 3: Use ImageMagick (If Installed)
If you have a source logo file (e.g., `logo-source.png`):

```bash
cd /path/to/kq-alumni-frontend/public

# Generate all sizes
convert logo-source.png -resize 16x16 favicon-16x16.png
convert logo-source.png -resize 32x32 favicon-32x32.png
convert logo-source.png -resize 32x32 favicon.ico
convert logo-source.png -resize 180x180 apple-touch-icon.png
convert logo-source.png -resize 192x192 android-chrome-192x192.png
convert logo-source.png -resize 512x512 android-chrome-512x512.png
convert logo-source.png -resize 150x150 mstile-150x150.png
```

## Design Recommendations

### For Best Results:
- **Background**: KQ Red (#DC143C) or transparent
- **Icon**: White KQ logo or "KQ" initials
- **Style**: Simple, recognizable at small sizes
- **Padding**: Leave 10-15% margin around the icon

### Color Palette:
- Primary: `#DC143C` (KQ Red)
- Secondary: `#FFFFFF` (White)
- Background: `#DC143C` or transparent

## After Adding Files

1. **Restart the dev server** if running
2. **Clear browser cache**: Ctrl+Shift+Delete
3. **Hard refresh**: Ctrl+Shift+R (Cmd+Shift+R on Mac)
4. **Check the browser tab** - favicon should appear

## Testing Checklist

- [ ] Favicon appears in browser tab
- [ ] Favicon appears in bookmarks
- [ ] iOS: Add to home screen shows custom icon
- [ ] Android: Add to home screen shows custom icon
- [ ] Windows: Pin to taskbar shows custom tile
- [ ] PWA: Can install as standalone app

## File Locations After Adding

```
public/
├── favicon.ico                      ✓ Browser tab (legacy)
├── favicon-16x16.png                ✓ Browser tab (modern)
├── favicon-32x32.png                ✓ Browser tab (modern)
├── apple-touch-icon.png             ✓ iOS home screen
├── android-chrome-192x192.png       ✓ Android home screen
├── android-chrome-512x512.png       ✓ Android splash screen
├── mstile-150x150.png              ✓ Windows tiles
├── site.webmanifest                ✓ Already created
├── browserconfig.xml               ✓ Already created
└── FAVICON_GUIDE.md                ✓ This guide
```

## Need Help?

If you're stuck, you can:
1. Use the realfavicongenerator.net website (recommended)
2. Ask your design team for the favicon assets
3. Use the existing KQ logo from `/public/assets/logos/logo-kq.svg`

---

**Status**: ⚠️ Configuration complete, awaiting favicon image files
