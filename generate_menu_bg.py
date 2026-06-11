"""
Main menu background: dark atmosphere, large glowing crystal on the right,
left side darker for button readability.
"""
from PIL import Image, ImageDraw, ImageFilter
import random, math

random.seed(42)
W, H = 1920, 1080

img = Image.new("RGB", (W, H), (8, 10, 22))
d   = ImageDraw.Draw(img)

def rc(base, s=10):
    return tuple(max(0, min(255, b + random.randint(-s, s))) for b in base)

# ── Background gradient — left darker, right slightly lighter (for crystal)
for x in range(W):
    t = x / W
    # left: very dark, right: slightly warmer dark
    r = int(8  + 18 * t)
    g = int(10 + 8  * t)
    b = int(22 + 28 * t)
    d.line([(x, 0), (x, H)], fill=(r, g, b))

# ── Subtle ground/terrain silhouette at bottom ────────────────────
terrain_pts = [(-10, H)]
x2 = -10
while x2 < W + 20:
    terrain_pts.append((x2, int(H*0.72) + random.randint(-30, 30)))
    x2 += random.randint(60, 140)
terrain_pts.append((W + 10, H))
d.polygon(terrain_pts, fill=(5, 7, 18))

# Slightly lighter ridge
ridge_pts = [(-10, H)]
x2 = -10
while x2 < W + 20:
    ridge_pts.append((x2, int(H*0.76) + random.randint(-20, 20)))
    x2 += random.randint(80, 180)
ridge_pts.append((W + 10, H))
d.polygon(ridge_pts, fill=(10, 12, 28))

# ── Stars / particles ─────────────────────────────────────────────
for _ in range(220):
    sx = random.randint(0, W)
    sy = random.randint(0, int(H * 0.70))
    sr = random.randint(1, 2)
    bri = random.randint(120, 220)
    d.ellipse([sx-sr, sy-sr, sx+sr, sy+sr], fill=(bri, bri, bri+30))

# ── Fog layers ────────────────────────────────────────────────────
for _ in range(30):
    fx = random.randint(-200, W+200)
    fy = random.randint(int(H*0.55), H)
    fw = random.randint(180, 500); fh = random.randint(30, 90)
    ovl = Image.new("RGBA", (W, H), (0,0,0,0))
    od  = ImageDraw.Draw(ovl)
    od.ellipse([fx-fw, fy-fh, fx+fw, fy+fh],
               fill=(20, 22, 48, random.randint(18, 45)))
    img = img.convert("RGBA"); img.alpha_composite(ovl); img = img.convert("RGB")
    d = ImageDraw.Draw(img)

# ── Crystal on the RIGHT ──────────────────────────────────────────
CX = int(W * 0.76)   # crystal center x
CY = int(H * 0.52)   # crystal center y

# Outer glow — large soft purple radial
for r in range(320, 0, -4):
    t   = r / 320
    col = (
        int(60  * (1-t) + 10  * t),
        int(15  * (1-t) + 8   * t),
        int(120 * (1-t) + 30  * t),
    )
    alpha = int(80 * (1-t) ** 1.4)
    ovl = Image.new("RGBA", (W, H), (0,0,0,0))
    od  = ImageDraw.Draw(ovl)
    od.ellipse([CX-r, CY-r, CX+r, CY+r], fill=col+(alpha,))
    img = img.convert("RGBA"); img.alpha_composite(ovl); img = img.convert("RGB")
    d = ImageDraw.Draw(img)

# Mid glow
for r in range(180, 0, -3):
    t   = r / 180
    col = (
        int(120 * (1-t) + 40 * t),
        int(30  * (1-t) + 10 * t),
        int(220 * (1-t) + 80 * t),
    )
    alpha = int(110 * (1-t)**1.2)
    ovl = Image.new("RGBA", (W, H), (0,0,0,0))
    od  = ImageDraw.Draw(ovl)
    od.ellipse([CX-r, CY-r, CX+r, CY+r], fill=col+(alpha,))
    img = img.convert("RGBA"); img.alpha_composite(ovl); img = img.convert("RGB")
    d = ImageDraw.Draw(img)

# Crystal body — multiple overlapping facets
def crystal_facet(pts, col, highlight=None):
    d.polygon(pts, fill=col)
    if highlight:
        d.polygon(pts, outline=highlight)

# Main crystal shape (large, centered)
CW = 95; CH = 240
# Main body
d.polygon([
    (CX, CY - CH),
    (CX - CW, CY - CH//4),
    (CX - CW//2, CY + CH//2),
    (CX, CY + CH),
    (CX + CW//2, CY + CH//2),
    (CX + CW, CY - CH//4),
], fill=(90, 20, 180))

# Left facet (darker)
d.polygon([
    (CX, CY - CH),
    (CX - CW, CY - CH//4),
    (CX - CW//2, CY + CH//2),
    (CX, CY + CH//3),
], fill=(55, 10, 130))

# Right facet (lighter, lit)
d.polygon([
    (CX, CY - CH),
    (CX + CW, CY - CH//4),
    (CX + CW//2, CY + CH//2),
    (CX, CY + CH//3),
], fill=(140, 50, 240))

# Inner glow core
d.polygon([
    (CX, CY - CH + 40),
    (CX - CW//3, CY - CH//5),
    (CX, CY + CH//4),
    (CX + CW//3, CY - CH//5),
], fill=(200, 140, 255))

# Bright highlight streak
d.polygon([
    (CX - 12, CY - CH + 20),
    (CX - 30, CY - CH//3),
    (CX - 8, CY - CH//4),
    (CX + 5, CY - CH + 30),
], fill=(230, 200, 255))

# Top spike glow
d.ellipse([CX-25, CY-CH-20, CX+25, CY-CH+20], fill=(220, 180, 255))
d.ellipse([CX-10, CY-CH-8, CX+10, CY-CH+8], fill=(255, 240, 255))

# Bottom base glow on ground
for r2 in range(90, 0, -5):
    t = r2/90
    ovl = Image.new("RGBA", (W, H), (0,0,0,0))
    od  = ImageDraw.Draw(ovl)
    od.ellipse([CX-r2*2, CY+CH-20-r2//3, CX+r2*2, CY+CH+20+r2//3],
               fill=(100, 30, 200, int(60*(1-t)**1.5)))
    img = img.convert("RGBA"); img.alpha_composite(ovl); img = img.convert("RGB")
    d = ImageDraw.Draw(img)

# Small orbiting crystal shards
for ang, dist, sc in [(0.5, 200, 0.4), (2.1, 170, 0.3), (3.8, 220, 0.35),
                       (1.3, 250, 0.25), (4.9, 190, 0.3)]:
    sx = CX + int(math.cos(ang) * dist)
    sy = CY + int(math.sin(ang) * dist * 0.6)
    sw = int(28*sc); sh = int(70*sc)
    d.polygon([
        (sx, sy-sh), (sx-sw, sy), (sx, sy+sh//2), (sx+sw, sy)
    ], fill=rc((110, 40, 200), 20))
    d.polygon([
        (sx, sy-sh), (sx+sw, sy), (sx, sy+sh//3)
    ], fill=rc((170, 90, 255), 15))

# Sparkle points around crystal
for _ in range(18):
    ang2 = random.uniform(0, math.pi*2)
    dist2 = random.randint(100, 300)
    sx2 = CX + int(math.cos(ang2)*dist2)
    sy2 = CY + int(math.sin(ang2)*dist2*0.7)
    sr2 = random.randint(2, 5)
    d.ellipse([sx2-sr2, sy2-sr2, sx2+sr2, sy2+sr2], fill=(220, 180, 255))

# ── Left-side shadow (for button readability) ─────────────────────
ovl = Image.new("RGBA", (W, H), (0,0,0,0))
od  = ImageDraw.Draw(ovl)
for bx in range(0, int(W*0.55), 4):
    t = 1 - bx / (W*0.55)
    alpha = int(140 * t**0.7)
    od.line([(bx, 0), (bx, H)], fill=(0, 0, 0, alpha))
img = img.convert("RGBA"); img.alpha_composite(ovl); img = img.convert("RGB")
d = ImageDraw.Draw(img)

# ── Top vignette ──────────────────────────────────────────────────
for vy in range(0, int(H*0.25), 3):
    t = 1 - vy/(H*0.25)
    alpha = int(100 * t**1.5)
    ovl = Image.new("RGBA", (W, H), (0,0,0,0))
    od  = ImageDraw.Draw(ovl)
    od.line([(0, vy), (W, vy)], fill=(0, 0, 0, alpha))
    img = img.convert("RGBA"); img.alpha_composite(ovl); img = img.convert("RGB")
    d = ImageDraw.Draw(img)

# ── Slight blur for atmosphere ────────────────────────────────────
img = img.filter(ImageFilter.GaussianBlur(0.8))

out = r'c:\Users\Will\Documents\development\towerdefence\Assets\menu_background.png'
img.save(out)
print(f"Saved {W}x{H} -> {out}")
