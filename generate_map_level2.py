"""
Level 2 map — grey rocky mountains. No snow, no volcano.
Light grey gravel path for enemy contrast.
Camera ortho=10.2 -> WX: ±18.133, WY: ±10.2
"""
from PIL import Image, ImageDraw, ImageFilter
import random, math

random.seed(13)

W, H   = 1920, 1080
CAM    = 10.2
WY_MAX =  CAM
WY_MIN = -CAM
WX_MAX =  CAM * W / H   # 18.133
WX_MIN = -WX_MAX

def w2px(wx):
    return int((wx - WX_MIN) / (WX_MAX - WX_MIN) * W)
def w2py(wy):
    return int((WY_MAX - wy) / (WY_MAX - WY_MIN) * H)

# PATH2 left entry pixel position
CAVE_X = 0
CAVE_Y = w2py(-6.0)   # ≈ 857

# PATH1 top entry pixel position
TOP_X  = w2px(12.0)   # ≈ 1595

# ── Palette ────────────────────────────────────────────────────────
SKY_TOP  = (155, 168, 185)   # cool overcast grey-blue
SKY_BOT  = (118, 128, 140)   # darker horizon

MTN_FAR  = (148, 152, 160)   # distant pale ridge
MTN_MID  = ( 98, 102, 110)   # mid rock
MTN_NEAR = ( 72,  75,  80)   # near dark face
ROCK_D   = ( 42,  44,  48)   # deep shadow / crevice
ROCK_M   = (105, 108, 115)   # general rock surface
ROCK_L   = (152, 155, 162)   # lit face
ROCK_HL  = (192, 196, 205)   # brightest highlight
CRACK    = ( 28,  30,  33)   # deep crack line

CAVE_IN  = (  8,   8,  10)   # cave interior (near black)
CAVE_DEP = ( 22,  23,  26)   # slightly lighter deep shadow
CAVE_FOG = ( 55,  58,  65)   # fog/mist at cave mouth

# Path — lighter grey for contrast with dark enemies
PATH_BDR = ( 42,  44,  48)
PATH_GRV = (138, 133, 122)
PATH_L   = (168, 162, 150)
PATH_D   = ( 95,  91,  82)
PATH_STN = (182, 176, 164)

def rc(base, s=10):
    return tuple(max(0, min(255, b + random.randint(-s, s))) for b in base)

# ── WAYPOINTS ─────────────────────────────────────────────────────
PATH1 = [
    (12.0, WY_MAX + 0.5),
    (12.0,  4.0), ( 8.0,  4.0), ( 8.0,  2.0), ( 4.0,  2.0), ( 4.0,  6.0),
    ( 0.0,  6.0), ( 0.0,  2.0), (-4.0,  2.0), (-4.0,  0.0), (-8.0,  0.0),
    (-8.0,  6.0), (-14.0, 6.0), (-14.0,-6.0), (-8.0, -6.0), (-8.0, -4.0),
    (-4.0, -4.0), (-4.0, -6.0), ( 0.0, -6.0), ( 0.0, -4.0), ( 4.0, -4.0),
    ( 4.0, -6.0), ( 8.0, -6.0), ( 8.0, -4.0), (12.0, -4.0), (12.0,  0.0),
    (16.0,  0.0),
]
PATH2 = [
    (-11.5, -6.0),   # starts after cave mouth (was left edge)
    (-8.0,  -6.0),
]

HW     = 48
HW_BDR = HW + 14

# ── Sky gradient ───────────────────────────────────────────────────
img = Image.new("RGB", (W, H), SKY_TOP)
d   = ImageDraw.Draw(img)
for y in range(H):
    t = y / H
    col = tuple(int(SKY_TOP[i]*(1-t) + SKY_BOT[i]*t) for i in range(3))
    d.line([(0,y),(W,y)], fill=col)

# Overcast cloud shapes
for _ in range(14):
    cx = random.randint(0, W); cy = random.randint(20, int(H*0.32))
    cw = random.randint(120, 380); ch = random.randint(30, 80)
    col = rc((168, 175, 188), 10)
    d.ellipse([cx-cw, cy-ch, cx+cw, cy+ch], fill=col)

# ── Far mountain ridge (pale, jagged) ────────────────────────────
ridge1 = [(-100,480),(60,240),(200,330),(370,190),(540,280),(710,175),
          (870,260),(1020,185),(1180,270),(1350,195),(1520,280),(1680,200),
          (1820,290),(2020,320),(2020,510),(-100,510)]
d.polygon(ridge1, fill=rc(MTN_FAR, 8))

# Mid ridge — heavier, darker
ridge2 = [(-100,550),(30,370),(190,460),(350,305),(530,420),(700,285),
          (860,390),(1010,260),(1160,360),(1320,250),(1480,350),(1640,260),
          (1800,360),(1960,310),(2020,400),(2020,590),(-100,590)]
d.polygon(ridge2, fill=rc(MTN_MID, 10))

# Near rock masses
for mass in [
    [(-100,660),(-100,850),(200,850),(290,540),(170,630),(55,500)],
    [(1700,850),(2020,850),(2020,600),(1910,440),(1790,530),(1680,480)],
    [(480,850),(630,530),(720,620),(810,850)],
    [(900,850),(1020,500),(1115,610),(1230,850)],
    [(190,850),(310,590),(410,690),(510,850)],
    [(1290,850),(1415,545),(1520,650),(1630,850)],
]:
    d.polygon(mass, fill=rc(MTN_NEAR, 8))

# Ground rock coverage
for _ in range(45):
    cx=random.randint(-80,W+80); cy=random.randint(int(H*0.52),H+60)
    rx=random.randint(65,240); ry=random.randint(30,115)
    d.ellipse([cx-rx,cy-ry,cx+rx,cy+ry], fill=rc(ROCK_M,10))

for _ in range(32):
    cx=random.randint(0,W); cy=random.randint(int(H*0.40),H)
    rx=random.randint(22,100); ry=random.randint(10,50)
    d.ellipse([cx-rx,cy-ry,cx+rx,cy+ry], fill=rc(ROCK_L,12))

# Sunlit rock highlights (upper faces)
for _ in range(28):
    cx=random.randint(0,W); cy=random.randint(int(H*0.28),int(H*0.72))
    rx=random.randint(8,48); ry=random.randint(4,22)
    d.ellipse([cx-rx,cy-ry,cx+rx,cy+ry], fill=rc(ROCK_HL,12))

# ── Rock crack lines ──────────────────────────────────────────────
for _ in range(22):
    lx=random.randint(0,W); ly=random.randint(int(H*0.3),int(H*0.85))
    angle=random.uniform(-1.0,1.0)
    llen=random.randint(35,160)
    ex=lx+int(math.sin(angle)*llen); ey=ly+int(math.cos(angle)*llen)
    d.line([(lx,ly),(ex,ey)], fill=rc(CRACK,5), width=random.randint(1,3))
    # branch crack
    if random.random()>0.5:
        blen=random.randint(15,60); bang=angle+random.uniform(0.4,0.9)
        bx=lx+int(math.sin(angle)*llen*0.5); by=ly+int(math.cos(angle)*llen*0.5)
        d.line([(bx,by),(bx+int(math.sin(bang)*blen),by+int(math.cos(bang)*blen))],
               fill=rc(CRACK,5), width=1)

# Large prominent cracks across rock faces
for lx,ly,ang,ln in [(320,620,-0.3,200),(880,580,0.4,180),(1400,640,-0.2,220),
                      (620,700,0.6,150),(1150,590,-0.5,190)]:
    ex=lx+int(math.sin(ang)*ln); ey=ly+int(math.cos(ang)*ln)
    d.line([(lx,ly),(ex,ey)], fill=CRACK, width=3)
    d.line([(lx+1,ly+1),(ex+1,ey+1)], fill=rc(ROCK_D,6), width=1)

# ── CAVE ENTRANCE (left side, PATH2 entry) ────────────────────────
CX = 80          # cave center x
CY = CAVE_Y      # matches path entry y
CW = 210         # large cave width half
CH = 165         # large cave height half

# Massive rock cliff face behind cave
d.rectangle([-10, CY-CH-120, CX+CW+120, CY+CH+120], fill=rc(MTN_NEAR,6))
d.ellipse([CX-CW-120, CY-CH-100, CX+CW+120, CY+CH+100], fill=rc(MTN_NEAR,6))

# Rough rock surround — multi-layer for depth
d.ellipse([CX-CW-70, CY-CH-60, CX+CW+70, CY+CH+60], fill=rc(ROCK_D,5))
d.ellipse([CX-CW-40, CY-CH-35, CX+CW+40, CY+CH+35], fill=rc((32,34,38),5))

# Deep interior glow — eerie blue-grey emanating from inside
for i in range(30, 0, -1):
    t = i / 30
    glow_col = (
        int(18 + 28*t),
        int(18 + 32*t),
        int(22 + 42*t),
    )
    ew = int(CW * t * 0.9); eh = int(CH * t * 0.9)
    d.ellipse([CX-ew, CY-eh, CX+ew, CY+eh], fill=glow_col)

# Absolute dark center
d.ellipse([CX-CW, CY-CH, CX+CW, CY+CH], fill=CAVE_IN)
d.ellipse([CX-int(CW*0.8), CY-int(CH*0.75), CX+int(CW*0.8), CY+int(CH*0.75)],
          fill=CAVE_IN)

# Glowing eyes deep inside — large, bright, unmissable
eye_pairs = [(-55, -22), (20, 15), (55, -12)]
for ex_off, ey_off in eye_pairs:
    for side in [-1, 1]:
        ex_x = CX + ex_off + side * 16
        ex_y = CY + ey_off
        # outer glow (large soft halo)
        for gr in range(22, 0, -1):
            t = gr / 22
            gc = (int(255*t*0.9), int(120*t*0.5), 0)
            ovl2 = Image.new("RGBA",(W,H),(0,0,0,0))
            od3  = ImageDraw.Draw(ovl2)
            od3.ellipse([ex_x-gr,ex_y-gr//2,ex_x+gr,ex_y+gr//2],
                        fill=gc+(int(180*(1-t)**0.5),))
            img=img.convert("RGBA"); img.alpha_composite(ovl2); img=img.convert("RGB")
            d=ImageDraw.Draw(img)
        # bright core
        d.ellipse([ex_x-10, ex_y-6, ex_x+10, ex_y+6], fill=(255,180,30))
        d.ellipse([ex_x-6,  ex_y-4, ex_x+6,  ex_y+4], fill=(255,230,120))
        d.ellipse([ex_x-3,  ex_y-2, ex_x+3,  ex_y+2], fill=(255,255,200))

# Large stalactites — dramatic, uneven
for ox, ht, wd in [(-130,90,22),(-85,120,28),(-38,75,18),(0,105,24),
                    (42,85,20),(88,115,26),(135,70,16)]:
    tx = CX + ox; ty = CY - CH + 8
    d.polygon([(tx,ty),(tx-wd,ty-ht),(tx+wd,ty-ht)], fill=rc(ROCK_D,6))
    d.polygon([(tx,ty),(tx,ty-ht//2),(tx-wd//3,ty-ht)], fill=rc(ROCK_M,6))
    # drip mark
    d.line([(tx,ty),(tx,ty+random.randint(8,22))], fill=rc(ROCK_D,4), width=2)

# Stalagmites at cave floor
for ox, ht, wd in [(-110,50,14),(-60,70,18),(10,45,12),(65,60,16),(120,52,13)]:
    tx = CX + ox; ty = CY + CH - 6
    d.polygon([(tx,ty),(tx-wd,ty+ht),(tx+wd,ty+ht)], fill=rc(ROCK_D,8))

# Rock rubble at floor
for _ in range(20):
    rx2 = random.randint(CX-CW+10, CX+CW+30)
    ry2 = CY + CH + random.randint(-10, 40)
    rr  = random.randint(5, 22)
    d.ellipse([rx2-rr, ry2-rr//2, rx2+rr, ry2+rr//2], fill=rc(ROCK_D,10))

# Heavy fog/mist billowing out — many layers
for _ in range(18):
    fx = random.randint(CX-CW+10, CX+CW+180)
    fy = random.randint(CY-CH+20, CY+CH-20)
    fw = random.randint(60, 160); fh = random.randint(25, 65)
    ovl = Image.new("RGBA",(W,H),(0,0,0,0))
    od  = ImageDraw.Draw(ovl)
    od.ellipse([fx-fw,fy-fh,fx+fw,fy+fh],
               fill=rc(CAVE_FOG,8)+(random.randint(45,95),))
    img=img.convert("RGBA"); img.alpha_composite(ovl); img=img.convert("RGB")
    d=ImageDraw.Draw(img)

# Prominent cracks radiating from cave opening
for ang in [-0.9,-0.5,-0.1,0.3,0.7,1.1,1.5,-1.3]:
    start_x = CX+int(math.cos(ang)*(CW+10))
    start_y = CY+int(math.sin(ang)*(CH+10))
    clen = random.randint(80,200)
    end_x = start_x+int(math.cos(ang)*clen)
    end_y = start_y+int(math.sin(ang)*clen)
    d.line([(start_x,start_y),(end_x,end_y)], fill=CRACK, width=3)
    # hairline branch
    bang = ang + random.uniform(0.3,0.7)
    bmid_x = start_x+int(math.cos(ang)*clen*0.5)
    bmid_y = start_y+int(math.sin(ang)*clen*0.5)
    d.line([(bmid_x,bmid_y),
            (bmid_x+int(math.cos(bang)*60),bmid_y+int(math.sin(bang)*60))],
           fill=CRACK, width=1)

# ── Second entry: crack/passage at top (PATH1 entry) ─────────────
# Small rocky outcrop at top edge around TOP_X
TOP_CX = TOP_X; TOP_CY = 0
d.ellipse([TOP_CX-80, TOP_CY-40, TOP_CX+80, TOP_CY+80], fill=rc(MTN_NEAR,8))
d.ellipse([TOP_CX-55, TOP_CY-15, TOP_CX+55, TOP_CY+65], fill=rc(ROCK_D,6))
d.ellipse([TOP_CX-40, TOP_CY-5, TOP_CX+40, TOP_CY+55], fill=CAVE_IN)
# crack lines from it
for ang2 in [-0.4, 0.0, 0.4, 0.8]:
    ex3=TOP_CX+int(math.sin(ang2)*70); ey3=TOP_CY+60+int(math.cos(ang2)*60)
    d.line([(TOP_CX,TOP_CY+40),(ex3,ey3)],fill=CRACK,width=2)

# ── Path (light grey volcanic gravel) ─────────────────────────────
def draw_road(wpts):
    pts=[(max(-300,min(W+300,w2px(wx))),max(-300,min(H+300,w2py(wy)))) for wx,wy in wpts]
    for i in range(len(pts)-1):
        x0,y0=pts[i]; x1,y1=pts[i+1]
        d.rectangle([min(x0,x1)-(HW_BDR+5),min(y0,y1)-(HW_BDR+5),
                     max(x0,x1)+(HW_BDR+5),max(y0,y1)+(HW_BDR+5)],
                    fill=rc(PATH_BDR,5))
    for i in range(len(pts)-1):
        x0,y0=pts[i]; x1,y1=pts[i+1]
        d.rectangle([min(x0,x1)-HW_BDR,min(y0,y1)-HW_BDR,
                     max(x0,x1)+HW_BDR,max(y0,y1)+HW_BDR],
                    fill=rc(ROCK_D,6))
    for i in range(len(pts)-1):
        x0,y0=pts[i]; x1,y1=pts[i+1]
        d.rectangle([min(x0,x1)-HW,min(y0,y1)-HW,
                     max(x0,x1)+HW,max(y0,y1)+HW],fill=PATH_GRV)
    for i in range(len(pts)-1):
        x0,y0=pts[i]; x1,y1=pts[i+1]
        horiz=abs(x1-x0)>abs(y1-y0)
        seg=abs(x1-x0) if horiz else abs(y1-y0)
        for _ in range(max(1,int(seg/11))):
            f=random.random()
            if horiz:
                cx2=int(min(x0,x1)+f*seg); cy2=(y0+y1)//2+random.randint(-HW+5,HW-5)
            else:
                cx2=(x0+x1)//2+random.randint(-HW+5,HW-5); cy2=int(min(y0,y1)+f*seg)
            pw=random.randint(5,18); ph=random.randint(3,11)
            d.ellipse([cx2-pw,cy2-ph,cx2+pw,cy2+ph],
                      fill=rc(PATH_L if random.random()>0.45 else PATH_D,10))
    rut=HW//3
    for i in range(len(pts)-1):
        x0,y0=pts[i]; x1,y1=pts[i+1]
        horiz=abs(x1-x0)>abs(y1-y0)
        if horiz:
            my=(y0+y1)//2
            for dy in [-rut,rut]:
                d.line([(min(x0,x1),my+dy),(max(x0,x1),my+dy)],fill=PATH_D,width=2)
        else:
            mx=(x0+x1)//2
            for dx in [-rut,rut]:
                d.line([(mx+dx,min(y0,y1)),(mx+dx,max(y0,y1))],fill=PATH_D,width=2)
    for i in range(len(pts)-1):
        x0,y0=pts[i]; x1,y1=pts[i+1]
        horiz=abs(x1-x0)>abs(y1-y0)
        seg=abs(x1-x0) if horiz else abs(y1-y0)
        for _ in range(max(1,int(seg/16))):
            f=random.random()
            if horiz:
                px4=int(min(x0,x1)+f*seg); py4=(y0+y1)//2+random.randint(-HW+6,HW-6)
            else:
                py4=int(min(y0,y1)+f*seg); px4=(x0+x1)//2+random.randint(-HW+6,HW-6)
            r3=random.randint(2,7)
            d.ellipse([px4-r3,py4-r3//2,px4+r3,py4+r3//2],fill=rc(PATH_STN,14))

draw_road(PATH1)
draw_road(PATH2)

# ── Eyes: 1 red pair + 1 orange pair ─────────────────────────────
eye_defs = [
    (-85, -28, (220,20,10), (255,70,40), (255,160,140)),   # red pair
    (-85,  22, (220,20,10), (255,70,40), (255,160,140)),   # red pair
]
for ex_off, ey_off, col_out, col_mid, col_in in eye_defs:
    for side in [-1, 1]:
        ex_x = CX + ex_off + side * 20
        ex_y = CY + ey_off
        for gr in range(20, 0, -1):
            t = gr / 20
            ovl2 = Image.new("RGBA",(W,H),(0,0,0,0))
            od3  = ImageDraw.Draw(ovl2)
            od3.ellipse([ex_x-gr, ex_y-gr//2, ex_x+gr, ex_y+gr//2],
                        fill=col_out+(int(160*(1-t)**0.5),))
            img=img.convert("RGBA"); img.alpha_composite(ovl2); img=img.convert("RGB")
            d=ImageDraw.Draw(img)
        d.ellipse([ex_x-9, ex_y-6, ex_x+9, ex_y+6], fill=col_out)
        d.ellipse([ex_x-6, ex_y-4, ex_x+6, ex_y+4], fill=col_mid)
        d.ellipse([ex_x-3, ex_y-2, ex_x+3, ex_y+2], fill=col_in)

# ── Base citadel ──────────────────────────────────────────────────
bx,by=w2px(16.0),w2py(0.0); BR=55
d.ellipse([bx-BR-22,by-BR-18,bx+BR+22,by+BR+18],fill=rc(ROCK_L,8))
d.rectangle([bx-BR,by-BR,bx+BR,by+BR],fill=rc(ROCK_D,5))
for lx2 in range(bx-BR,bx+BR,15):
    d.line([(lx2,by-BR),(lx2,by+BR)],fill=rc(ROCK_M,4),width=1)
for ly2 in range(by-BR,by+BR,11):
    d.line([(bx-BR,ly2),(bx+BR,ly2)],fill=rc(ROCK_M,4),width=1)
d.rectangle([bx-28,by-28,bx+28,by+28],fill=rc(MTN_NEAR,8))
for tx,ty in [(bx-BR,by-BR),(bx+BR-24,by-BR),(bx-BR,by+BR-24),(bx+BR-24,by+BR-24)]:
    d.ellipse([tx,ty,tx+24,ty+24],fill=rc(ROCK_D,6))
    d.ellipse([tx+4,ty+4,tx+20,ty+20],fill=rc(ROCK_M,8))
d.rectangle([bx-12,by+8,bx+12,by+BR],fill=rc(ROCK_D,4))
d.ellipse([bx-12,by,bx+12,by+18],fill=rc(ROCK_D,4))
d.ellipse([bx-16,by-20,bx+16,by+4],fill=(78,25,155),outline=(185,110,250))
d.ellipse([bx-9,by-15,bx+9,by+0],fill=(140,60,220))
d.ellipse([bx-4,by-11,bx+4,by-4],fill=(208,172,252))

# ── Blur + vignette ───────────────────────────────────────────────
img=img.filter(ImageFilter.GaussianBlur(0.5))
d=ImageDraw.Draw(img)
vign=Image.new("RGBA",(W,H),(0,0,0,0))
vd=ImageDraw.Draw(vign)
for i in range(80):
    a=int(150*((1-i/80)**2.2))
    vd.rectangle([i*6,i*4,W-i*6,H-i*4],outline=(0,0,0,a))
img_rgba=img.convert("RGBA"); img_rgba.alpha_composite(vign)
img=img_rgba.convert("RGB")

out=r'c:\Users\Will\Documents\development\towerdefence\Assets\background_level2.png'
img.save(out)
print(f"Saved {W}x{H} -> {out}")
