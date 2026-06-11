"""
Generates 4-frame walking/flying sprite sheets for all enemies.
Output saved to Assets/art/Sprites/ with _new suffix for review.
"""
from PIL import Image, ImageDraw
import os

# ── Palette ──────────────────────────────────────────────────
T   = (0,   0,   0,   0)
BK  = (10,  8,   20,  255)   # near-black outline
DP  = (21,  18,  42,  255)   # dark purple body
MP  = (42,  35,  80,  255)   # mid purple highlight
LP  = (72,  60,  120, 255)   # light purple edge
GOLD= (212, 168, 67,  255)   # sacred gold
CYN = (115, 225, 255, 255)   # ice cyan
RE  = (218, 55,  38,  255)   # red
OR  = (230, 140, 20,  255)   # orange
DG  = (38,  38,  55,  255)   # dark grey
MG  = (75,  72,  95,  255)   # mid grey
LG  = (145, 140, 165, 255)   # light grey
WHT = (220, 215, 235, 255)   # near-white

# ── Drawing helpers ──────────────────────────────────────────
def new_frame(w, h):
    img = Image.new("RGBA", (w, h), T)
    return img, ImageDraw.Draw(img)

def ell(d, cx, cy, rx, ry, fill, outline=None):
    d.ellipse([cx-rx, cy-ry, cx+rx, cy+ry], fill=fill, outline=outline)

def px(img, x, y, col):
    if 0 <= x < img.width and 0 <= y < img.height:
        img.putpixel((x, y), col)

def ln(d, x1, y1, x2, y2, col, w=1):
    d.line([(x1, y1), (x2, y2)], fill=col, width=w)

def alpha(col, a):
    return (col[0], col[1], col[2], a)

# ── Save helper ───────────────────────────────────────────────
def save_sheet(frames, path):
    w, h = frames[0].size
    sheet = Image.new("RGBA", (w * len(frames), h), T)
    for i, f in enumerate(frames):
        sheet.paste(f, (i * w, 0))
    sheet.save(path)
    print(f"  {os.path.basename(path)}  ({w*len(frames)}x{h}px)")


# ═══════════════════════════════════════════════════════════════
# 1. BASIC ENEMY — dark alien bug   32x32 × 4
# ═══════════════════════════════════════════════════════════════
def basic_frame(fi):
    img, d = new_frame(32, 32)
    dip  = 1 if fi in (1, 3) else 0
    by   = 17 + dip          # body centre y

    # Back spines
    for sx, sh in [(9, 3), (14, 4), (19, 3)]:
        ln(d, sx, by-5, sx, by-5-sh, BK)
        px(img, sx, by-5-sh, MP)

    # Legs — 3 visible, alternating stride
    foot_off = [[+3, 0, -3], [0, 0, 0], [-3, 0, +3], [0, 0, 0]][fi]
    for i, ax in enumerate([20, 15, 10]):
        ay = by + 5
        fx = ax + foot_off[i]
        fy = by + 11 + dip
        kx = (ax + fx) // 2
        ky = ay + 4
        ln(d, ax, ay, kx, ky, BK, 2)
        ln(d, kx, ky, fx, fy, BK, 2)
        px(img, fx, fy, MP)

    # Body
    ell(d, 15, by,   9, 5, DP, BK)
    ell(d, 15, by-1, 7, 3, MP)

    # Head
    ell(d, 23, 13, 5, 4, DP, BK)
    ell(d, 23, 13, 3, 2, MP)

    # Eyes
    ell(d, 21, 12, 2, 2, RE)
    ell(d, 25, 12, 2, 2, RE)
    px(img, 21, 12, (255, 100, 80, 255))
    px(img, 25, 12, (255, 100, 80, 255))

    # Antennae sway
    sw = [0, 1, 0, -1][fi]
    ln(d, 21, 10, 18+sw, 5, BK)
    ln(d, 25, 10, 28+sw, 5, BK)
    ell(d, 18+sw, 4, 2, 2, MP)
    ell(d, 28+sw, 4, 2, 2, MP)

    return img


# ═══════════════════════════════════════════════════════════════
# 2. SPEED ENEMY — dark wolf/panther   32x32 × 4
# ═══════════════════════════════════════════════════════════════
def speed_frame(fi):
    img, d = new_frame(32, 32)
    dip = 1 if fi in (1, 3) else 0
    by  = 21 + dip   # body low, close to ground

    # Tail (sweeps almost horizontally behind, slight sway)
    tw = [20, 19, 20, 19][fi]
    ln(d, 5, by-1, 2, tw,   BK, 2)
    ln(d, 2, tw,   1, tw-3, BK)

    # Legs — gallop cycle
    fl_x = [27, 23, 19, 23][fi]
    bl_x = [5,   9, 13,  9][fi]
    gy   = by + 8   # ground level

    # Far-side legs (dim)
    for ax, fx in [(22, fl_x+1), (8, bl_x+1)]:
        kx = (ax+fx)//2
        ln(d, ax, by+3, kx, by+6, DG)
        ln(d, kx, by+6, fx, gy,   DG)

    # Near-side legs
    for ax, fx in [(22, fl_x), (8, bl_x)]:
        kx = (ax+fx)//2
        ln(d, ax, by+3, kx, by+6, BK, 2)
        ln(d, kx, by+6, fx, gy,   BK, 2)

    # Motion streaks
    if fi in (0, 2):
        for sy in [by-2, by, by+2]:
            d.line([(0, sy), (4, sy)], fill=alpha(MG, 140))

    # Body (long, low, horizontal predator silhouette)
    ell(d, 16, by,   12, 5, DG, BK)
    ell(d, 16, by-1, 10, 3, MG)

    # Small haunch (flat bump, not rooster-tall)
    ell(d, 7, by-1, 4, 3, DG)

    # Head + snout reaching forward at body level
    ell(d, 26, by-1,  5, 4, DG, BK)
    ell(d, 26, by-2,  4, 3, MG)
    ell(d, 30, by,    3, 2, DG, BK)   # snout

    # Ears — small, swept back (running posture)
    d.polygon([(24, by-4), (23, by-7), (26, by-4)], fill=DG, outline=BK)

    # Eye
    ell(d, 27, by-2, 2, 2, OR)
    px(img, 27, by-2, (255, 200, 50, 255))

    return img


# ═══════════════════════════════════════════════════════════════
# 3. TANK ENEMY — armored beast   48x48 × 4
# ═══════════════════════════════════════════════════════════════
def tank_frame(fi):
    img, d = new_frame(48, 48)
    dip = 1 if fi in (1, 3) else 0
    by  = 30 + dip

    # Legs — heavy stomp (front-right lifts on f=0, back-left on f=2)
    legs = {
        0: [(35, by+6, 37, 44), (27, by+7, 27, 44), (21, by+7, 21, 44), (13, by+6, 11, 41)],
        1: [(35, by+6, 35, 44), (27, by+7, 27, 44), (21, by+7, 21, 44), (13, by+6, 13, 44)],
        2: [(35, by+6, 33, 41), (27, by+7, 27, 44), (21, by+7, 21, 44), (13, by+6, 15, 44)],
        3: [(35, by+6, 35, 44), (27, by+7, 27, 44), (21, by+7, 21, 44), (13, by+6, 13, 44)],
    }
    for ax, ay, fx, fy in legs[fi]:
        kx = (ax+fx)//2
        ky = ay + 5
        ln(d, ax, ay, kx, ky, BK, 3)
        ln(d, kx, ky, fx, fy, BK, 3)
        d.ellipse([fx-3, fy-1, fx+3, fy+2], fill=DG, outline=BK)

    # Body
    ell(d, 24, by, 15, 9, DG, BK)
    ell(d, 24, by-1, 13, 7, MG)

    # Armour plates (vertical seam lines)
    for plx in [14, 19, 24, 29, 34]:
        ln(d, plx, by-8, plx, by+8, alpha(BK, 160))

    # Crystal spines on back
    for sx, sh, col in [(12, 10, GOLD), (20, 13, CYN), (28, 11, GOLD)]:
        sy = by - 9
        d.polygon([(sx-3, sy), (sx, sy-sh), (sx+3, sy)], fill=col, outline=BK)

    # Shoulders / haunches
    ell(d, 35, by-4, 7, 7, DG, BK)
    ell(d, 11, by-4, 7, 7, DG, BK)

    # Neck
    ell(d, 39, by-10, 5, 7, DG, BK)

    # Head
    ell(d, 43, by-17, 5, 5, DG, BK)
    ell(d, 43, by-17, 4, 4, MG)

    # Horns
    d.polygon([(39, by-21), (37, by-27), (41, by-21)], fill=LG, outline=BK)
    d.polygon([(44, by-21), (46, by-27), (43, by-21)], fill=LG, outline=BK)

    # Eyes
    ell(d, 41, by-18, 2, 2, RE)
    ell(d, 45, by-18, 2, 2, RE)
    px(img, 41, by-18, (255, 80, 60, 255))
    px(img, 45, by-18, (255, 80, 60, 255))

    # Gold nose ring
    ln(d, 45, by-12, 47, by-12, GOLD, 2)

    return img


# ═══════════════════════════════════════════════════════════════
# 4. FLYING ENEMY — dark bat creature   32x32 × 4
# ═══════════════════════════════════════════════════════════════
def flying_frame(fi):
    img, d = new_frame(32, 32)
    body_y   = 17
    wing_tip = [4, 10, 16, 22][fi]   # wing tip y per frame (up→down)

    # Far wing (darker)
    d.polygon([
        (20, body_y),
        (28, wing_tip + 3),
        (26, body_y + 4),
    ], fill=(15, 12, 30, 255), outline=BK)

    # Near wing
    near = [
        (12, body_y - 1),
        (2,  wing_tip),
        (5,  wing_tip + 6),
        (10, body_y + 4),
    ]
    d.polygon(near, fill=DP, outline=BK)
    ln(d, 12, body_y-1,  2, wing_tip,     alpha(LP, 130))
    ln(d, 12, body_y-1,  5, wing_tip+6,   alpha(LP,  80))

    # Body
    ell(d, 16, body_y,   6, 5, DP, BK)
    ell(d, 16, body_y-1, 4, 3, MP)

    # Eyes (glowing cyan)
    ell(d, 14, body_y-1, 2, 2, CYN)
    ell(d, 18, body_y-1, 2, 2, CYN)
    px(img, 14, body_y-1, WHT)
    px(img, 18, body_y-1, WHT)

    # Mouth + fang
    ln(d, 14, body_y+2, 18, body_y+2, BK)
    px(img, 16, body_y+3, RE)

    # Claws
    ln(d, 15, body_y+4, 14, body_y+7, BK)
    ln(d, 17, body_y+4, 18, body_y+7, BK)
    for cx2, cy2 in [(13, body_y+8), (15, body_y+8), (17, body_y+8), (19, body_y+8)]:
        px(img, cx2, cy2, BK)

    return img


# ── Main ─────────────────────────────────────────────────────
if __name__ == "__main__":
    out = os.path.join(os.path.dirname(os.path.abspath(__file__)),
                       "Assets", "art", "Sprites")
    os.makedirs(out, exist_ok=True)

    print("Generating enemy sprites…")
    save_sheet([basic_frame(f)  for f in range(4)], os.path.join(out, "basic_enemy_new.png"))
    save_sheet([speed_frame(f)  for f in range(4)], os.path.join(out, "speedN_new.png"))
    save_sheet([tank_frame(f)   for f in range(4)], os.path.join(out, "tank1_new.png"))
    save_sheet([flying_frame(f) for f in range(4)], os.path.join(out, "basicN_new.png"))
    print("Done — review sprites in Assets/art/Sprites/ before replacing originals.")
