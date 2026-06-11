"""
Preview of 3 plot color variants on the forest background.
Generates plot_preview.png showing each style side-by-side.
"""
from PIL import Image, ImageDraw, ImageFont
import random

W, H = 1200, 500
TILE = 68    # plot tile pixel size (matches in-game scale)
GAP  = 6     # gap between tiles
COLS = 6
ROWS = 5

FOREST_BG  = (28,  82,  30)
CANOPY     = (18,  60,  20)
GRASS_L    = (50, 115,  45)

random.seed(99)

def draw_forest_bg(d, x0, y0, x1, y1):
    d.rectangle([x0, y0, x1, y1], fill=FOREST_BG)
    for _ in range(80):
        cx = random.randint(x0, x1)
        cy = random.randint(y0, y1)
        rx, ry = random.randint(15, 55), random.randint(10, 38)
        c = random.choice([CANOPY, GRASS_L, (22, 72, 25), (35, 95, 38)])
        d.ellipse([cx-rx, cy-ry, cx+rx, cy+ry], fill=c)

# ── Tile drawing functions ────────────────────────────────────────
def draw_mossy_stone(d, tx, ty):
    # Base: grey-green stone
    BASE  = (88, 102, 72)
    DARK  = (60,  72, 48)
    LIGHT = (112, 128, 92)
    MOSS  = (48,  90, 42)
    r = random.randint
    d.rectangle([tx, ty, tx+TILE, ty+TILE], fill=DARK)
    d.rectangle([tx+3, ty+3, tx+TILE-3, ty+TILE-3], fill=BASE)
    # stone texture: a few lighter patches
    for _ in range(4):
        px = tx + r(6, TILE-18); py = ty + r(6, TILE-14)
        pw = r(8, 20); ph = r(6, 14)
        d.rectangle([px, py, px+pw, py+ph], fill=LIGHT)
    # moss blotches
    for _ in range(3):
        mx = tx + r(5, TILE-10); my = ty + r(5, TILE-10)
        mr = r(3, 8)
        d.ellipse([mx-mr, my-mr, mx+mr, my+mr], fill=MOSS)
    # bevel highlight top-left
    d.line([(tx+3, ty+TILE-3), (tx+3, ty+3), (tx+TILE-3, ty+3)],
           fill=LIGHT, width=2)
    # bevel shadow bottom-right
    d.line([(tx+TILE-3, ty+3), (tx+TILE-3, ty+TILE-3), (tx+3, ty+TILE-3)],
           fill=DARK, width=2)

def draw_dark_earth(d, tx, ty):
    # Dark earth with grass tufts
    BASE  = (62,  88, 48)
    DARK  = (38,  58, 28)
    LIGHT = (78, 112, 60)
    TUFT  = (55, 130, 50)
    r = random.randint
    d.rectangle([tx, ty, tx+TILE, ty+TILE], fill=DARK)
    d.rectangle([tx+3, ty+3, tx+TILE-3, ty+TILE-3], fill=BASE)
    # irregular earth texture
    for _ in range(5):
        px = tx + r(4, TILE-16); py = ty + r(4, TILE-12)
        pw = r(6, 18); ph = r(4, 12)
        d.ellipse([px, py, px+pw, py+ph],
                  fill=(r(50,72), r(80,102), r(40,60)))
    # grass tufts at edges
    for ex, ey in [(tx+r(5,TILE-10), ty+4), (tx+r(5,TILE-10), ty+TILE-6),
                   (tx+4, ty+r(5,TILE-10)), (tx+TILE-6, ty+r(5,TILE-10))]:
        d.line([(ex, ey), (ex-2, ey-6)], fill=TUFT, width=2)
        d.line([(ex, ey), (ex+2, ey-6)], fill=TUFT, width=2)

def draw_wooden(d, tx, ty):
    # Wooden platform / planks
    BASE  = (118, 88, 48)
    DARK  = ( 72, 52, 24)
    LIGHT = (148, 115, 68)
    PLANK = ( 95, 68, 32)
    r = random.randint
    d.rectangle([tx, ty, tx+TILE, ty+TILE], fill=DARK)
    d.rectangle([tx+3, ty+3, tx+TILE-3, ty+TILE-3], fill=BASE)
    # horizontal plank lines
    plank_h = TILE // 3
    for i in range(1, 3):
        py = ty + 3 + i * plank_h
        d.line([(tx+3, py), (tx+TILE-3, py)], fill=PLANK, width=2)
    # vertical grain lines (subtle)
    for i in range(2, TILE-2, 10):
        d.line([(tx+i, ty+3), (tx+i+1, ty+TILE-3)],
               fill=(r(105,130), r(78,95), r(40,55)), width=1)
    # bevel
    d.line([(tx+3, ty+TILE-3), (tx+3, ty+3), (tx+TILE-3, ty+3)],
           fill=LIGHT, width=2)
    d.line([(tx+TILE-3, ty+3), (tx+TILE-3, ty+TILE-3), (tx+3, ty+TILE-3)],
           fill=DARK, width=2)
    # nail dots
    for nx, ny in [(tx+8, ty+8), (tx+TILE-9, ty+8),
                   (tx+8, ty+TILE-9), (tx+TILE-9, ty+TILE-9)]:
        d.ellipse([nx-2, ny-2, nx+2, ny+2], fill=DARK)

variants = [
    ("Замшелый камень", draw_mossy_stone),
    ("Тёмная земля",    draw_dark_earth),
    ("Деревянные помосты", draw_wooden),
]

img = Image.new("RGB", (W, H), (15, 40, 15))
d   = ImageDraw.Draw(img)

PAD   = 30
LABEL = 36
panel_w = (W - PAD * 4) // 3

for vi, (name, draw_fn) in enumerate(variants):
    px0 = PAD + vi * (panel_w + PAD)
    py0 = PAD + LABEL
    px1 = px0 + panel_w
    py1 = H - PAD

    # Forest background
    draw_forest_bg(d, px0, py0, px1, py1)

    # Grid of tiles
    grid_w = COLS * (TILE + GAP) - GAP
    grid_h = ROWS * (TILE + GAP) - GAP
    ox = px0 + (panel_w - grid_w) // 2
    oy = py0 + (py1 - py0 - grid_h) // 2

    random.seed(vi * 1000)
    for row in range(ROWS):
        for col in range(COLS):
            tx = ox + col * (TILE + GAP)
            ty = oy + row * (TILE + GAP)
            draw_fn(d, tx, ty)

    # Panel border
    d.rectangle([px0, py0, px1, py1], outline=(200, 200, 200), width=2)

    # Label
    d.rectangle([px0, PAD, px1, py0], fill=(20, 55, 20))
    # Draw text manually with a rough font fallback
    try:
        font = ImageFont.truetype("arial.ttf", 18)
    except Exception:
        font = ImageFont.load_default()
    # center text in label area
    bbox = d.textbbox((0, 0), name, font=font)
    tw = bbox[2] - bbox[0]
    tx_c = px0 + (panel_w - tw) // 2
    d.text((tx_c, PAD + 8), name, fill=(220, 220, 180), font=font)

out = r'c:\Users\Will\Documents\development\towerdefence\plot_preview.png'
img.save(out)
print(f"Saved -> {out}")
