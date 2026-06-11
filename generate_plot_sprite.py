"""
Generates a 74x74 stone tile sprite (PPU=100 -> 0.74 Unity units, same as original k1_0).
"""
from PIL import Image, ImageDraw
import random

random.seed(5)
S = 74

img = Image.new("RGBA", (S, S), (0, 0, 0, 0))
d   = ImageDraw.Draw(img)

BORDER = ( 55,  50,  40, 255)   # dark mortar border
MORTAR = ( 72,  66,  52, 255)   # mortar between stones
BASE   = (128, 122, 105, 255)   # stone base
LIGHT  = (158, 152, 132, 255)   # stone highlight
DARK   = ( 92,  87,  72, 255)   # stone shadow
MOSS   = ( 52,  90,  42, 255)   # moss accent

def rc(c, s=10):
    return tuple(max(0, min(255, c[i] + random.randint(-s, s))) for i in range(3)) + (255,)

# Outer mortar border
d.rectangle([0, 0, S-1, S-1], fill=BORDER)
# Main stone surface
d.rectangle([4, 4, S-5, S-5], fill=BASE)

# Stone texture: random lighter/darker patches
for _ in range(12):
    px = random.randint(5, S-18)
    py = random.randint(5, S-14)
    pw = random.randint(8, 22)
    ph = random.randint(6, 16)
    col = rc(LIGHT if random.random() > 0.45 else DARK, 12)
    d.rectangle([px, py, px+pw, py+ph], fill=col)

# Horizontal mortar crack (divides into 2 rows)
mid_y = S // 2
d.rectangle([4, mid_y-1, S-5, mid_y+1], fill=MORTAR)

# Vertical mortar cracks — offset per row (brick pattern)
# Top row: crack at 1/2
d.rectangle([S//2-1, 4, S//2+1, mid_y-2], fill=MORTAR)
# Bottom row: crack at 1/3 and 2/3
d.rectangle([S//3-1, mid_y+2, S//3+1, S-5], fill=MORTAR)
d.rectangle([2*S//3-1, mid_y+2, 2*S//3+1, S-5], fill=MORTAR)

# Moss dots in corners and along mortar
moss_spots = [(5, 5), (S-10, 5), (5, S-10), (S-10, S-10),
              (S//2-3, 5), (5, mid_y-3)]
for mx, my in moss_spots:
    mr = random.randint(2, 5)
    d.ellipse([mx, my, mx+mr*2, my+mr], fill=rc(MOSS, 14))

# Bevel: highlight top-left edge
d.line([(4, S-5), (4, 4), (S-5, 4)], fill=LIGHT, width=2)
# Bevel: shadow bottom-right edge
d.line([(S-5, 4), (S-5, S-5), (4, S-5)], fill=DARK, width=2)

out = r'c:\Users\Will\Documents\development\towerdefence\Assets\plot_wood.png'
img.save(out)
print(f"Saved {S}x{S} -> {out}")
