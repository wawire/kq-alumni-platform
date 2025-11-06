# Favicon Implementation Guide

## Required Favicon Files

Place the following files in the `/public` directory:

### Essential Files:
1. **favicon.ico** (48x48 or 32x32) - Legacy browsers
2. **favicon-16x16.png** - Small browser tabs
3. **favicon-32x32.png** - Standard browser tabs
4. **apple-touch-icon.png** (180x180) - iOS home screen
5. **android-chrome-192x192.png** - Android home screen
6. **android-chrome-512x512.png** - Android splash screen
7. **mstile-150x150.png** - Windows tiles

### Generated Files (Already Created):
- ✅ `site.webmanifest` - PWA manifest
- ✅ `browserconfig.xml` - Windows tile configuration

## How to Generate Favicons

### Option 1: Use Online Generator (Recommended)
1. Visit: https://realfavicongenerator.net/
2. Upload your logo (preferably SVG or high-res PNG)
3. Customize colors (use KQ Red: #DC143C)
4. Download the generated package
5. Extract all files to `/public` directory

### Option 2: Use Existing Logo
If you have the KQ logo, you can convert it:

```bash
# Using ImageMagick (if available)
convert logo-kq.svg -resize 16x16 favicon-16x16.png
convert logo-kq.svg -resize 32x32 favicon-32x32.png
convert logo-kq.svg -resize 180x180 apple-touch-icon.png
convert logo-kq.svg -resize 192x192 android-chrome-192x192.png
convert logo-kq.svg -resize 512x512 android-chrome-512x512.png
convert logo-kq.svg -resize 32x32 favicon.ico
```

### Option 3: Design Custom Icons
Create custom icons with:
- Background: KQ Red (#DC143C)
- Icon: White KQ logo or initials
- Padding: 10-15% around the icon

## Implementation Status

✅ **Next.js Metadata Configured** (`app/layout.tsx`)
  - Favicon links added
  - Apple touch icon configured
  - Android icons configured
  - Web manifest linked
  - Theme colors set
  - Open Graph tags added
  - Twitter card configured

✅ **Manifest Files Created**
  - `site.webmanifest` - PWA configuration
  - `browserconfig.xml` - Windows tiles

⚠️ **Missing**: Actual icon image files
  - Need to add PNG/ICO files to `/public`
  - Use one of the methods above to generate

## File Checklist

Place these files in `/public`:

- [ ] favicon.ico
- [ ] favicon-16x16.png
- [ ] favicon-32x32.png
- [ ] apple-touch-icon.png
- [ ] android-chrome-192x192.png
- [ ] android-chrome-512x512.png
- [ ] mstile-150x150.png (optional)
- [x] site.webmanifest
- [x] browserconfig.xml

## Testing

After adding the favicon files:

1. **Clear browser cache** (Ctrl+Shift+Delete)
2. **Hard refresh** (Ctrl+Shift+R)
3. **Check browser tab** - Should show favicon
4. **Test on mobile** - Add to home screen
5. **Check PWA** - Install as app

## Expected Result

Once files are added:
- ✅ Browser tabs show KQ logo
- ✅ iOS home screen has icon
- ✅ Android home screen has icon
- ✅ Windows tiles have icon
- ✅ PWA installable with proper icons

## Colors Used

- **Theme Color**: #DC143C (KQ Red)
- **Background**: #FFFFFF (White)
- **Tile Color**: #DC143C (KQ Red)

---

**Next Steps:**
1. Generate/add the actual PNG and ICO files
2. Test in browser
3. Verify on mobile devices
4. Check PWA installation
