"""
Generates a 74x74 dark basalt tile sprite (PPU=100 -> 0.74 Unity units).
"""
from PIL import Image, ImageDraw
import random

random.seed(9)
S = 74

img = Image.new("RGBA", (S, S), (0, 0, 0, 0))
d   = ImageDraw.Draw(img)

BORDER = ( 18,  16,  14, 255)
BASE   = ( 45,  42,  38, 255)
DARK   = ( 28,  25,  22, 255)
LIGHT  = ( 72,  67,  60, 255)
HL     = ( 95,  88,  78, 255)
CRACK  = ( 14,  12,  10, 255)

def rc(c, s=8):
    return tuple(max(0, min(255, c[i] + random.randint(-s, s))) for i in range(3)) + (255,)

# Border
d.rectangle([0, 0, S-1, S-1], fill=BORDER)
# Base surface
d.rectangle([3, 3, S-4, S-4], fill=BASE)

# Rock texture patches (irregular)
for _ in range(16):
    px = random.randint(4, S-16)
    py = random.randint(4, S-12)
    pw = random.randint(6, 20)
    ph = random.randint(4, 14)
    col = rc(LIGHT if random.random() > 0.55 else DARK, 10)
    d.rectangle([px, py, px+pw, py+ph], fill=col)

# Diagonal crack lines (natural rock fracture)
d.line([(4, 4), (S-4, S//2)], fill=CRACK, width=1)
d.line([(S//3, 4), (S//3+8, S-4)], fill=CRACK, width=1)

# Subtle sunlit highlight (upper-left faces)
for _ in range(5):
    hx = random.randint(4, S//2)
    hy = random.randint(4, S//2)
    hw = random.randint(5, 16); hh = random.randint(3, 10)
    d.rectangle([hx, hy, hx+hw, hy+hh], fill=rc(HL, 8))

# Deep shadow lower-right
d.line([(S-4, 4), (S-4, S-4), (4, S-4)], fill=DARK, width=3)
# Bevel highlight upper-left
d.line([(3, S-4), (3, 3), (S-4, 3)], fill=rc(LIGHT, 6), width=2)

out = r'c:\Users\Will\Documents\development\towerdefence\Assets\plot_basalt.png'
img.save(out)
print(f"Saved {S}x{S} -> {out}")
