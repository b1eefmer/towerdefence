"""
Level 1 map — forest theme.
Path has scale(2,2,1), so world = parent_pos + local * 2.
parent_pos = (-0.5, 0.5).  All segments H or V only.
PPU=38 -> sprite at scale(1,1,1) fills W/PPU x H/PPU world units.
"""
from PIL import Image, ImageDraw, ImageFilter
import random, math

random.seed(7)

W, H = 1920, 1080
PPU  = 38

WX_MIN = -W / (PPU * 2)   # -25.263
WX_MAX =  W / (PPU * 2)   #  25.263
WY_MIN = -H / (PPU * 2)   # -14.211
WY_MAX =  H / (PPU * 2)   #  14.211

def w2px(wx):
    return int((wx - WX_MIN) / (WX_MAX - WX_MIN) * W)

def w2py(wy):
    return int((WY_MAX - wy) / (WY_MAX - WY_MIN) * H)

# ── Forest palette ────────────────────────────────────────────────
FOREST_BASE = ( 28,  82,  30)   # base forest floor
CANOPY_D    = ( 12,  48,  14)   # dark canopy shadow
CANOPY_M    = ( 20,  65,  22)   # canopy mid
CANOPY_L    = ( 32,  88,  35)   # canopy lighter edge
GRASS_L     = ( 50, 115,  45)   # bright grass/clearings
GRASS_HL    = ( 72, 148,  62)   # sunlit grass patch
SHRUB       = ( 18,  58,  20)   # dense undergrowth
LEAF_DRK    = ( 35,  70,  18)   # dark leaves on floor
LEAF_BRN    = ( 72,  58,  22)   # brown leaf litter
SHADOW      = ( 10,  35,  12)   # deep forest shadow

# Path — dirt/mud trail
PATH_EDGE   = ( 22,  55,  18)   # grass edge right next to trail
PATH_BDR    = ( 48,  32,  12)   # dark mud border
PATH_BASE   = (108,  78,  38)   # packed dirt base
PATH_LITE   = (140, 105,  58)   # worn lighter dirt
PATH_DRK    = ( 75,  52,  22)   # darker rut/puddle
PATH_PEBB   = (155, 140, 118)   # small pebble

# Mushrooms
MUSH_R      = (185,  40,  20)   # red cap
MUSH_RL     = (230,  80,  55)   # highlight on cap
MUSH_O      = (210, 130,  20)   # orange cap
MUSH_OL     = (245, 175,  65)   # orange highlight
MUSH_STEM   = (210, 195, 170)   # stem
MUSH_GL     = ( 80, 220, 120)   # glowing green (magic)
MUSH_GLL    = (160, 255, 190)   # glow highlight

LOG_D       = ( 62,  40,  18)   # fallen log dark
LOG_L       = ( 88,  60,  28)   # fallen log light

def rc(base, s=14):
    return tuple(max(0, min(255, b + random.randint(-s, s))) for b in base)

# ── CORRECT WAYPOINTS ─────────────────────────────────────────────
PATH1 = [
    (-0.5,  WY_MAX + 0.5),
    (-0.5,  8.5),
    (-16.5, 8.5),
    (-16.5, -7.5),
    (-8.5,  -7.5),
    (-8.5,   2.5),
    (-0.5,   2.5),
    (-0.5,  -7.5),
    ( 5.5,  -7.5),
    ( 5.5,   8.5),
    (11.5,   8.5),
    (11.5,   0.5),
    (20.5,   0.5),
    (20.5,   1.7),
]

PATH2 = [
    (-8.5, WY_MIN - 0.5),
    (-8.5,  2.5),
]

HW     = 35
HW_BDR = HW + 14

# ── Canvas ─────────────────────────────────────────────────────────
img = Image.new("RGB", (W, H), FOREST_BASE)
d   = ImageDraw.Draw(img)

# ── Forest floor base ──────────────────────────────────────────────
# Large dark canopy blobs (tree tops viewed from above)
for _ in range(90):
    x  = random.randint(-120, W+120)
    y  = random.randint(-80,  H+80)
    rx = random.randint(80, 260); ry = random.randint(60, 200)
    d.ellipse([x-rx, y-ry, x+rx, y+ry], fill=rc(CANOPY_D, 8))

for _ in range(130):
    x  = random.randint(-60, W+60)
    y  = random.randint(-40, H+40)
    rx = random.randint(55, 160); ry = random.randint(40, 120)
    d.ellipse([x-rx, y-ry, x+rx, y+ry], fill=rc(CANOPY_M, 12))

# Canopy edges / lighter rims
for _ in range(200):
    x, y = random.randint(0, W), random.randint(0, H)
    rx, ry = random.randint(28, 100), random.randint(20, 70)
    d.ellipse([x-rx, y-ry, x+rx, y+ry], fill=rc(CANOPY_L, 14))

# Grass clearings between trees
for _ in range(180):
    x, y = random.randint(0, W), random.randint(0, H)
    rx, ry = random.randint(18, 70), random.randint(12, 48)
    d.ellipse([x-rx, y-ry, x+rx, y+ry], fill=rc(GRASS_L, 16))

for _ in range(120):
    x, y = random.randint(10, W-10), random.randint(10, H-10)
    rx, ry = random.randint(10, 45), random.randint(7, 30)
    d.ellipse([x-rx, y-ry, x+rx, y+ry], fill=rc(GRASS_HL, 18))

# Leaf litter on floor
for _ in range(400):
    x, y = random.randint(0, W), random.randint(0, H)
    d.ellipse([x, y, x+random.randint(4,16), y+random.randint(3,10)],
              fill=rc(LEAF_DRK, 12))
for _ in range(200):
    x, y = random.randint(0, W), random.randint(0, H)
    d.ellipse([x, y, x+random.randint(5,18), y+random.randint(4,12)],
              fill=rc(LEAF_BRN, 14))

# Deep shadows under dense canopy
for _ in range(60):
    x, y = random.randint(0, W), random.randint(0, H)
    rx, ry = random.randint(15, 55), random.randint(10, 38)
    d.ellipse([x-rx, y-ry, x+rx, y+ry], fill=rc(SHADOW, 6))

# ── Dirt path drawing ──────────────────────────────────────────────
def draw_road(wpts):
    pts = []
    for wx, wy in wpts:
        px = max(-300, min(W+300, w2px(wx)))
        py = max(-300, min(H+300, w2py(wy)))
        pts.append((px, py))

    # Grass edge band (slightly outside border — green fringe)
    for i in range(len(pts) - 1):
        x0, y0 = pts[i]; x1, y1 = pts[i+1]
        d.rectangle([min(x0,x1)-(HW_BDR+6), min(y0,y1)-(HW_BDR+6),
                     max(x0,x1)+(HW_BDR+6), max(y0,y1)+(HW_BDR+6)],
                    fill=PATH_EDGE)

    # Dark mud border
    for i in range(len(pts) - 1):
        x0, y0 = pts[i]; x1, y1 = pts[i+1]
        d.rectangle([min(x0,x1)-HW_BDR, min(y0,y1)-HW_BDR,
                     max(x0,x1)+HW_BDR, max(y0,y1)+HW_BDR], fill=PATH_BDR)

    # Packed dirt base
    for i in range(len(pts) - 1):
        x0, y0 = pts[i]; x1, y1 = pts[i+1]
        d.rectangle([min(x0,x1)-HW, min(y0,y1)-HW,
                     max(x0,x1)+HW, max(y0,y1)+HW], fill=PATH_BASE)

    # Dirt texture: random lighter/darker patches along segments
    for i in range(len(pts) - 1):
        x0, y0 = pts[i]; x1, y1 = pts[i+1]
        horiz = abs(x1-x0) > abs(y1-y0)
        seg   = abs(x1-x0) if horiz else abs(y1-y0)
        step  = 18
        steps = max(1, int(seg / step))
        for t in range(steps):
            f = (t + 0.5) / steps
            if horiz:
                cx = int(min(x0,x1) + f*seg)
                cy = (y0+y1)//2
            else:
                cx = (x0+x1)//2
                cy = int(min(y0,y1) + f*seg)
            pw = random.randint(8, 28); ph = random.randint(6, 22)
            col = rc(PATH_LITE if random.random() > 0.4 else PATH_DRK, 12)
            d.ellipse([cx-pw, cy-ph, cx+pw, cy+ph], fill=col)

    # Ruts / wheel tracks (two parallel lines along path centre)
    rut_off = HW // 3
    for i in range(len(pts) - 1):
        x0, y0 = pts[i]; x1, y1 = pts[i+1]
        horiz = abs(x1-x0) > abs(y1-y0)
        if horiz:
            for dy in [-rut_off, rut_off]:
                d.line([(min(x0,x1), (y0+y1)//2+dy),
                        (max(x0,x1), (y0+y1)//2+dy)],
                       fill=PATH_DRK, width=3)
        else:
            for dx in [-rut_off, rut_off]:
                d.line([((x0+x1)//2+dx, min(y0,y1)),
                        ((x0+x1)//2+dx, max(y0,y1))],
                       fill=PATH_DRK, width=3)

    # Scattered pebbles on path
    for i in range(len(pts) - 1):
        x0, y0 = pts[i]; x1, y1 = pts[i+1]
        horiz = abs(x1-x0) > abs(y1-y0)
        seg   = abs(x1-x0) if horiz else abs(y1-y0)
        n_peb = int(seg / 22)
        for _ in range(n_peb):
            f = random.random()
            if horiz:
                cx = int(min(x0,x1) + f*seg)
                cy = (y0+y1)//2 + random.randint(-HW+6, HW-6)
            else:
                cy = int(min(y0,y1) + f*seg)
                cx = (x0+x1)//2 + random.randint(-HW+6, HW-6)
            r = random.randint(2, 6)
            d.ellipse([cx-r, cy-r, cx+r, cy+r], fill=rc(PATH_PEBB, 20))

draw_road(PATH1)
draw_road(PATH2)

# ── Base (dark fortress in the forest) ────────────────────────────
BASE_WX, BASE_WY = 20.5, 1.7
bx, by2 = w2px(BASE_WX), w2py(BASE_WY)
BR = 52

# Clearing around base
d.ellipse([bx-BR-20, by2-BR-20, bx+BR+20, by2+BR+20], fill=rc(GRASS_L, 10))

# Log wall (outer)
d.rectangle([bx-BR, by2-BR, bx+BR, by2+BR], fill=(48, 30, 10), outline=(28, 18, 6))
# Log planks texture
for lx in range(bx-BR, bx+BR, 14):
    d.line([(lx, by2-BR), (lx, by2+BR)], fill=(38, 24, 8), width=2)
for ly in range(by2-BR, by2+BR, 14):
    d.line([(bx-BR, ly), (bx+BR, ly)], fill=(38, 24, 8), width=2)

# Inner courtyard
d.rectangle([bx-28, by2-28, bx+28, by2+28], fill=(65, 48, 22))

# Corner watchtowers (round logs)
for tx, ty in [(bx-BR, by2-BR), (bx+BR-22, by2-BR),
               (bx-BR,  by2+BR-22), (bx+BR-22, by2+BR-22)]:
    d.ellipse([tx, ty, tx+22, ty+22], fill=(55, 35, 12), outline=(28, 18, 6))
    d.ellipse([tx+3, ty+3, tx+19, ty+19], fill=(75, 52, 20))

# Gate
d.rectangle([bx-12, by2+10, bx+12, by2+BR], fill=(22, 14, 5))
d.ellipse([bx-12, by2+2, bx+12, by2+20], fill=(22, 14, 5))

# Soul orb (stolen soul — magic purple glow)
d.ellipse([bx-18, by2-22, bx+18, by2+6], fill=(90, 30, 170), outline=(190, 120, 255))
d.ellipse([bx-11, by2-17, bx+11, by2+1], fill=(155, 70, 235))
d.ellipse([bx-5,  by2-13, bx+5,  by2-4], fill=(215, 180, 255))

# ── Forest decorations (mushrooms + fallen logs) ───────────────────
random.seed(42)

# Fallen logs in off-path zones
log_spots = [
    (-22, 11), (-20, -10), (-12, 11), (-12, -10),
    (  2, 11), (  3, -10), ( 14, 11), ( 16, -10),
    (-24,  3), ( 23,  6),
]
for (lwx, lwy) in log_spots:
    lx, ly = w2px(lwx), w2py(lwy)
    ang = random.uniform(0, math.pi)
    ll  = random.randint(40, 90)
    lw  = random.randint(7, 14)
    dx  = int(math.cos(ang) * ll); dy = int(math.sin(ang) * ll)
    d.line([(lx-dx, ly-dy), (lx+dx, ly+dy)], fill=rc(LOG_D, 8), width=lw+4)
    d.line([(lx-dx, ly-dy), (lx+dx, ly+dy)], fill=rc(LOG_L, 10), width=lw)

# Mushroom clusters
shroom_spots = [
    (-23, 12, 'r', 4), (-21,  -9, 'g', 3), (-11, 11, 'o', 4),
    (-13,-10, 'r', 3), (  2,  12, 'g', 4), (  3,-11, 'o', 3),
    ( 14, 12, 'r', 4), ( 15, -11, 'g', 3), ( 22,  9, 'o', 4),
    (-24,  4, 'g', 3), ( 24,  -4, 'r', 3),
]
for (swx, swy, kind, cnt) in shroom_spots:
    sx, sy = w2px(swx), w2py(swy)
    for _ in range(cnt):
        ox = random.randint(-30, 30); oy = random.randint(-20, 20)
        mx, my = sx+ox, sy+oy
        sh = random.randint(16, 38); sw2 = random.randint(8, 18)
        # Stem
        d.rectangle([mx-3, my, mx+3, my+sh//2], fill=rc(MUSH_STEM, 14))
        # Cap
        if kind == 'r':
            cap_c, cap_l = MUSH_R, MUSH_RL
        elif kind == 'o':
            cap_c, cap_l = MUSH_O, MUSH_OL
        else:
            cap_c, cap_l = MUSH_GL, MUSH_GLL
        d.ellipse([mx-sw2, my-sh//2, mx+sw2, my+4], fill=rc(cap_c, 12), outline=rc(LOG_D,6))
        # Highlight spot
        d.ellipse([mx-sw2//3, my-sh//3, mx+sw2//3, my-sh//6], fill=rc(cap_l, 10))
        # Glowing mushrooms emit faint aura
        if kind == 'g':
            d.ellipse([mx-sw2-8, my-sh//2-8, mx+sw2+8, my+12],
                      fill=(80, 220, 120, 60) if False else rc((60,180,90),30))

# Firefly sparkles (small bright dots)
for _ in range(180):
    x, y = random.randint(0, W), random.randint(0, H)
    r = random.randint(1, 3)
    col = random.choice([(200, 255, 160), (160, 255, 200), (255, 240, 140)])
    d.ellipse([x-r, y-r, x+r, y+r], fill=col)

# ── Blur + vignette ────────────────────────────────────────────────
img = img.filter(ImageFilter.GaussianBlur(0.5))
d   = ImageDraw.Draw(img)

vign = Image.new("RGBA", (W, H), (0, 0, 0, 0))
vd   = ImageDraw.Draw(vign)
for i in range(80):
    a = int(130 * ((1 - i/80) ** 2.2))
    vd.rectangle([i*6, i*4, W-i*6, H-i*4], outline=(0, 0, 0, a))
img_rgba = img.convert("RGBA")
img_rgba.alpha_composite(vign)
img = img_rgba.convert("RGB")

out = r'c:\Users\Will\Documents\development\towerdefence\Assets\background.png'
img.save(out)
print(f"Saved {W}x{H} -> {out}")
