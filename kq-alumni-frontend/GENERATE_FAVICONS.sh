#!/bin/bash

# Script to generate favicons from KQ logo
# Requires: ImageMagick or rsvg-convert

echo "Generating favicons from KQ logo..."

SOURCE_SVG="public/assets/logos/logo-kq.svg"
PUBLIC_DIR="public"

# Check if source logo exists
if [ ! -f "$SOURCE_SVG" ]; then
    echo "❌ Error: Source logo not found at $SOURCE_SVG"
    exit 1
fi

# Check if ImageMagick is installed
if command -v convert &> /dev/null; then
    echo "✅ ImageMagick found, generating icons..."

    # Generate PNG favicons
    convert -background "#DC143C" -gravity center -extent 32x32 "$SOURCE_SVG" "$PUBLIC_DIR/favicon-16x16.png"
    convert -background "#DC143C" -gravity center -extent 64x64 "$SOURCE_SVG" -resize 32x32 "$PUBLIC_DIR/favicon-32x32.png"
    convert -background "#DC143C" -gravity center -extent 360x360 "$SOURCE_SVG" -resize 180x180 "$PUBLIC_DIR/apple-touch-icon.png"
    convert -background "#DC143C" -gravity center -extent 384x384 "$SOURCE_SVG" -resize 192x192 "$PUBLIC_DIR/android-chrome-192x192.png"
    convert -background "#DC143C" -gravity center -extent 1024x1024 "$SOURCE_SVG" -resize 512x512 "$PUBLIC_DIR/android-chrome-512x512.png"
    convert -background "#DC143C" -gravity center -extent 300x300 "$SOURCE_SVG" -resize 150x150 "$PUBLIC_DIR/mstile-150x150.png"

    # Generate ICO file
    convert -background "#DC143C" -gravity center -extent 64x64 "$SOURCE_SVG" -resize 32x32 "$PUBLIC_DIR/favicon.ico"

    echo "✅ Favicons generated successfully!"
    echo ""
    echo "Generated files:"
    ls -lh "$PUBLIC_DIR"/*.{ico,png} 2>/dev/null | awk '{print "  - " $9 " (" $5 ")"}'

elif command -v rsvg-convert &> /dev/null; then
    echo "✅ rsvg-convert found, generating icons..."

    # Generate PNG files
    rsvg-convert -w 16 -h 16 "$SOURCE_SVG" -o "$PUBLIC_DIR/favicon-16x16.png"
    rsvg-convert -w 32 -h 32 "$SOURCE_SVG" -o "$PUBLIC_DIR/favicon-32x32.png"
    rsvg-convert -w 180 -h 180 "$SOURCE_SVG" -o "$PUBLIC_DIR/apple-touch-icon.png"
    rsvg-convert -w 192 -h 192 "$SOURCE_SVG" -o "$PUBLIC_DIR/android-chrome-192x192.png"
    rsvg-convert -w 512 -h 512 "$SOURCE_SVG" -o "$PUBLIC_DIR/android-chrome-512x512.png"
    rsvg-convert -w 150 -h 150 "$SOURCE_SVG" -o "$PUBLIC_DIR/mstile-150x150.png"

    # For .ico, we still need ImageMagick
    echo "⚠️  Note: .ico file requires ImageMagick. Using PNG as fallback."
    rsvg-convert -w 32 -h 32 "$SOURCE_SVG" -o "$PUBLIC_DIR/favicon.ico"

    echo "✅ Favicons generated successfully!"

else
    echo "❌ Error: Neither ImageMagick nor rsvg-convert found."
    echo ""
    echo "Please install one of the following:"
    echo "  Ubuntu/Debian: sudo apt-get install imagemagick"
    echo "  macOS:         brew install imagemagick"
    echo "  Fedora:        sudo dnf install ImageMagick"
    echo ""
    echo "Or use the online generator:"
    echo "  https://realfavicongenerator.net/"
    exit 1
fi

echo ""
echo "✅ Done! Restart your dev server and hard refresh (Ctrl+Shift+R)"
