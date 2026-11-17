"""Kiểm tra Tools/LevelDesign/levels.json: cấu trúc, section, vị trí enemy, tầm nhảy, đường đi.

Chạy: python -I Tools/LevelDesign/validate_levels.py [đường_dẫn_levels.json]
Exit code 0 = không lỗi (cảnh báo vẫn in), 1 = có lỗi.
"""
import json
import math
import os
import sys
from collections import deque

# --- Số liệu di chuyển (Data/PlayerStats.asset + Rigidbody2D Player trong scene + Physics2DSettings) ---
GRAVITY = 9.81 * 4.0          # |Physics2D.gravity.y| * gravityScale 4
JUMP_V = 13.0                 # jumpForce
MOVE_SPEED = 5.0              # moveSpeed (điều khiển ngang trên không = moveSpeed)
PLAYER_H = 1.75               # CapsuleCollider2D size.y
SAFETY = 0.75                 # chỉ dùng 75% tầm tối đa

H_MAX = JUMP_V ** 2 / (2 * GRAVITY)
H_SAFE = H_MAX * SAFETY

EPS = 1e-6
SOLID = ("ground", "platform", "wall")
TARGET_COUNTS = {"Level01": 10, "Level02": 14, "Level03": 11}  # gồm cả boss (L3: ~10 + Unmaker)
BOSS_ARENA_MIN_W = {"Demon": 18, "Unmaker": 22}
BOSS_ARENA_MIN_H = {"Demon": 3.5, "Unmaker": 9}
KNOWN_PREFABS = {"MeleeEnemy", "Axeman", "Stalker", "Knight", "Demon", "Unmaker"}


def jump_reach(dy):
    """Quãng ngang (tâm) đi được khi nhảy từ mặt cao 0 tới mặt cao dy (đáp lúc rơi xuống)."""
    disc = JUMP_V ** 2 - 2 * GRAVITY * dy
    if disc < 0:
        return None
    t = (JUMP_V + math.sqrt(disc)) / GRAVITY
    return MOVE_SPEED * t


def safe_reach(dy):
    if dy > H_SAFE + EPS:
        return None
    r = jump_reach(dy)
    return None if r is None else r * SAFETY


class Report:
    def __init__(self):
        self.errors = []
        self.warnings = []
        self.info = []

    def err(self, msg):
        self.errors.append(msg)

    def warn(self, msg):
        self.warnings.append(msg)


def top(b):
    return b["y"] + b["h"]


def overlap(a1, a2, b1, b2):
    return min(a2, b2) - max(a1, b1) > EPS


def build_surfaces(blocks):
    """Mặt đứng = mặt trên block rắn, trừ đi phần bị block rắn khác đặt lên (bottom == top)."""
    surfaces = []
    for i, b in enumerate(blocks):
        if b["type"] not in SOLID:
            continue
        y = top(b)
        cuts = sorted((o["x"], o["x"] + o["w"]) for j, o in enumerate(blocks)
                      if j != i and o["type"] in SOLID and abs(o["y"] - y) < EPS
                      and overlap(o["x"], o["x"] + o["w"], b["x"], b["x"] + b["w"]))
        x = b["x"]
        end = b["x"] + b["w"]
        for c1, c2 in cuts:
            if c1 - x > EPS:
                surfaces.append({"x1": x, "x2": min(c1, end), "y": y, "block": i})
            x = max(x, c2)
        if end - x > EPS:
            surfaces.append({"x1": x, "x2": end, "y": y, "block": i})
    return surfaces


def find_surface(surfaces, x, y, margin=0.0):
    for k, s in enumerate(surfaces):
        if abs(s["y"] - y) < 1e-3 and s["x1"] + margin - EPS <= x <= s["x2"] - margin + EPS:
            return k
    return None


def jump_blocked(blocks, a, b, skip):
    """Có block rắn nằm chắn hành lang giữa hai mặt (khoảng ngang giữa chúng) không."""
    if a["x2"] <= b["x1"]:
        g1, g2 = a["x2"], b["x1"]
    elif b["x2"] <= a["x1"]:
        g1, g2 = b["x2"], a["x1"]
    else:
        return False
    if g2 - g1 < EPS:
        # sát nhau: chắn nếu có block đứng đúng mép, cao hơn mặt cao nhất
        g1, g2 = g1 - 0.05, g2 + 0.05
    hi = max(a["y"], b["y"])
    for j, o in enumerate(blocks):
        if j in skip or o["type"] not in SOLID:
            continue
        if overlap(o["x"], o["x"] + o["w"], g1, g2) and top(o) > hi + EPS and o["y"] < hi + PLAYER_H:
            return True
    return False


def build_graph(blocks, surfaces):
    n = len(surfaces)
    edges = {k: set() for k in range(n)}
    kinds = {}
    # nhảy / rơi
    for i in range(n):
        for j in range(n):
            if i == j:
                continue
            a, b = surfaces[i], surfaces[j]
            dy = b["y"] - a["y"]
            gap = max(0.0, b["x1"] - a["x2"], a["x1"] - b["x2"])
            if overlap(a["x1"], a["x2"], b["x1"], b["x2"]):
                if dy < 0:  # mặt b nằm ngay dưới a: rơi xuống từ mép a
                    edges[i].add(j)
                    kinds[(i, j)] = "drop"
                continue
            r = safe_reach(dy)
            if r is None or gap > r + EPS:
                continue
            if jump_blocked(blocks, a, b, {a["block"], b["block"]}):
                continue
            edges[i].add(j)
            if gap < EPS and abs(dy) < EPS:
                kinds[(i, j)] = "walk"
            else:
                kinds[(i, j)] = "jump gap %.2f dy %+.2f (limit %.2f)" % (gap, dy, r)
    # thang: chân trên một mặt, đỉnh ngang mặt ở mép thang
    for lad in (b for b in blocks if b["type"] == "ladder"):
        lx1, lx2 = lad["x"], lad["x"] + lad["w"]
        bottoms = [k for k, s in enumerate(surfaces)
                   if abs(s["y"] - lad["y"]) < 1e-3 and overlap(s["x1"], s["x2"], lx1, lx2)]
        tops = [k for k, s in enumerate(surfaces)
                if abs(s["y"] - top(lad)) < 1e-3 and s["x1"] - 0.3 <= lx2 and s["x2"] + 0.3 >= lx1]
        for p in bottoms:
            for q in tops:
                edges[p].add(q)
                edges[q].add(p)
                kinds[(p, q)] = kinds[(q, p)] = "ladder"
    # giếng nhảy tường: hai tường đối diện, lòng giếng 1.2..3.0, có sàn trong lòng giếng
    walls = [b for b in blocks if b["type"] == "wall"]
    for wl in walls:
        for wr in walls:
            inner1, inner2 = wl["x"] + wl["w"], wr["x"]
            width = inner2 - inner1
            if not (1.2 <= width <= 3.0):
                continue
            if not overlap(wl["y"], top(wl), wr["y"], top(wr)):
                continue
            if abs(top(wl) - top(wr)) > H_SAFE:
                continue
            floors = [k for k, s in enumerate(surfaces)
                      if overlap(s["x1"], s["x2"], inner1, inner2)
                      and s["y"] <= min(wl["y"], wr["y"]) + H_SAFE + EPS
                      and s["y"] <= max(wl["y"], wr["y"]) + EPS]
            tops = [k for k, s in enumerate(surfaces)
                    if abs(s["y"] - max(top(wl), top(wr))) < 1e-3 or abs(s["y"] - min(top(wl), top(wr))) < 1e-3]
            tops = [k for k in tops if overlap(surfaces[k]["x1"] - 0.3, surfaces[k]["x2"] + 0.3,
                                                wl["x"], wr["x"] + wr["w"])]
            for p in floors:
                for q in tops:
                    edges[p].add(q)
                    kinds[(p, q)] = "wall-jump shaft %.1f wide, %.1f high" % (width, max(top(wl), top(wr)) - surfaces[p]["y"])
    return edges, kinds


def reachable_from(edges, start):
    seen = {start: None}
    dq = deque([start])
    while dq:
        u = dq.popleft()
        for v in edges[u]:
            if v not in seen:
                seen[v] = u
                dq.append(v)
    return seen


def path_to(seen, goal):
    p = []
    while goal is not None:
        p.append(goal)
        goal = seen[goal]
    return p[::-1]


def check_level(lv, rep):
    lid = lv["id"]
    P = lid + ": "
    for key in ("id", "name", "playerSpawn", "exit", "cameraBounds", "sections", "blocks", "spawnZones", "boss"):
        if key not in lv:
            rep.err(P + "thiếu key '%s'" % key)
            return
    secs = lv["sections"]
    blocks = lv["blocks"]

    # --- section liền nhau ---
    for a, b in zip(secs, secs[1:]):
        if abs(a["maxX"] - b["minX"]) > EPS:
            rep.err(P + "section %s/%s không liền nhau (%.2f vs %.2f)" % (a["id"], b["id"], a["maxX"], b["minX"]))
    for s in secs:
        w = s["maxX"] - s["minX"]
        if not (20 <= w <= 36):
            rep.warn(P + "section %s rộng %.1f (mong ~25-35)" % (s["id"], w))
    lmin, lmax = secs[0]["minX"], secs[-1]["maxX"]
    cb = lv["cameraBounds"]
    if cb["minX"] > lmin + EPS or cb["maxX"] < lmax - EPS:
        rep.warn(P + "cameraBounds không phủ hết dải section")

    def in_section(x1, x2):
        return any(s["minX"] - EPS <= x1 and x2 <= s["maxX"] + EPS for s in secs)

    def point_in_section(x):
        return lmin - EPS <= x <= lmax + EPS

    # --- block ---
    for i, b in enumerate(blocks):
        tag = "%s #%d (x %.2f)" % (b["type"], i, b["x"])
        if b["type"] not in SOLID + ("ladder",):
            rep.err(P + "type lạ " + tag)
        if b["w"] <= 0 or b["h"] <= 0:
            rep.err(P + "kích thước <= 0 " + tag)
        if not in_section(b["x"], b["x"] + b["w"]):
            if b["type"] == "ground" and lmin - EPS <= b["x"] and b["x"] + b["w"] <= lmax + EPS:
                rep.warn(P + "ground vắt qua 2 section " + tag)
            else:
                rep.err(P + "block không nằm trọn trong section " + tag)
        if b["type"] == "platform" and not (0.5 - EPS <= b["h"] <= 0.8 + EPS):
            rep.err(P + "platform h ngoài 0.5-0.8 " + tag)
        if b["type"] == "wall" and b["w"] < 1 - EPS:
            rep.err(P + "wall w < 1 " + tag)
        if b["type"] == "ground" and b["h"] < 0.5 - EPS:
            rep.err(P + "ground dày < 0.5 " + tag)
        if b["type"] == "ladder" and abs(b["w"] - 1.2) > EPS:
            rep.err(P + "ladder w != 1.2 " + tag)
    # chồng lấn khối rắn
    for i, a in enumerate(blocks):
        for j in range(i + 1, len(blocks)):
            b = blocks[j]
            if a["type"] in SOLID and b["type"] in SOLID and \
                    overlap(a["x"], a["x"] + a["w"], b["x"], b["x"] + b["w"]) and \
                    overlap(a["y"], top(a), b["y"], top(b)):
                rep.err(P + "block rắn chồng nhau #%d và #%d" % (i, j))

    surfaces = build_surfaces(blocks)
    # khoảng trống trên đầu: block rắn treo thấp hơn chiều cao player trên một mặt đứng
    for s in surfaces:
        for j, o in enumerate(blocks):
            if o["type"] in SOLID and overlap(o["x"], o["x"] + o["w"], s["x1"], s["x2"]) and \
                    s["y"] + EPS < o["y"] < s["y"] + PLAYER_H + 0.25:
                rep.err(P + "mặt y=%.2f [%.2f,%.2f] bị block #%d treo thấp (đáy %.2f), player không chui lọt"
                        % (s["y"], s["x1"], s["x2"], j, o["y"]))

    # --- spawn / exit ---
    sx, sy = lv["playerSpawn"]
    s_spawn = find_surface(surfaces, sx, sy)
    if s_spawn is None:
        rep.err(P + "playerSpawn không đứng trên mặt nào")
    goals = []
    if lv["exit"] is not None:
        ex, ey = lv["exit"]
        k = find_surface(surfaces, ex, ey)
        if k is None:
            rep.err(P + "exit không đứng trên mặt nào")
        else:
            goals.append(("exit", k))
        if not point_in_section(ex):
            rep.err(P + "exit ngoài section")

    # --- enemy ---
    counts = {}
    all_enemies = []
    zones = lv["spawnZones"]
    for z in zones:
        if not in_section(z["x"], z["x"] + z["w"]):
            rep.warn(P + "zone %s vắt qua ranh section" % z["id"])
        for e in z["enemies"]:
            counts[e["prefab"]] = counts.get(e["prefab"], 0) + 1
            all_enemies.append((z["id"], e))
            if e["prefab"] not in KNOWN_PREFABS or e["prefab"] in ("Demon", "Unmaker"):
                rep.err(P + "prefab không hợp lệ trong zone: %s" % e["prefab"])
            if not (z["x"] - EPS <= e["x"] <= z["x"] + z["w"] + EPS and z["y"] - EPS <= e["y"] <= z["y"] + z["h"] + EPS):
                rep.err(P + "%s %s (%.2f,%.2f) nằm ngoài zone" % (z["id"], e["prefab"], e["x"], e["y"]))
            k = find_surface(surfaces, e["x"], e["y"], margin=0.4)
            if k is None:
                rep.err(P + "%s %s (%.2f,%.2f) không đứng trên mặt block" % (z["id"], e["prefab"], e["x"], e["y"]))
            elif e["prefab"] == "Stalker":
                w = surfaces[k]["x2"] - surfaces[k]["x1"]
                if w < 6 - EPS:
                    rep.warn(P + "Stalker ở x %.1f: mặt đứng chỉ rộng %.1f (< 6)" % (e["x"], w))
            if s_spawn is not None and abs(e["x"] - sx) < 12 - EPS:
                rep.err(P + "%s %s cách spawn %.1f (< 12)" % (z["id"], e["prefab"], abs(e["x"] - sx)))
    for i, a in enumerate(zones):
        for b in zones[i + 1:]:
            if overlap(a["x"], a["x"] + a["w"], b["x"], b["x"] + b["w"]) and overlap(a["y"], a["y"] + a["h"], b["y"], b["y"] + b["h"]):
                rep.err(P + "zone %s và %s chồng nhau" % (a["id"], b["id"]))
    if all_enemies:
        first = min(e["x"] for _, e in all_enemies)
        rep.info.append(P + "enemy đầu tiên ở x %.1f (cách spawn %.1f)" % (first, first - sx))

    # --- boss ---
    boss = lv["boss"]
    if boss is not None:
        name = boss["prefab"]
        counts[name] = counts.get(name, 0) + 1
        k = find_surface(surfaces, boss["arenaMinX"] + 0.01, boss["y"])
        if k is None or surfaces[k]["x2"] < boss["arenaMaxX"] - EPS:
            rep.err(P + "sàn arena không phẳng liền từ arenaMinX tới arenaMaxX")
        else:
            goals.append(("boss arena", k))
            if not (boss["arenaMinX"] <= boss["x"] <= boss["arenaMaxX"]) or abs(boss["y"] - surfaces[k]["y"]) > EPS:
                rep.err(P + "boss không đứng trên sàn arena")
        w = boss["arenaMaxX"] - boss["arenaMinX"]
        if w < BOSS_ARENA_MIN_W.get(name, 0) - EPS:
            rep.err(P + "arena %s rộng %.1f < %.1f" % (name, w, BOSS_ARENA_MIN_W[name]))
        hneed = BOSS_ARENA_MIN_H.get(name, 0)
        for j, o in enumerate(blocks):
            if o["type"] in SOLID and overlap(o["x"], o["x"] + o["w"], boss["arenaMinX"], boss["arenaMaxX"]) and \
                    boss["y"] + EPS < o["y"] < boss["y"] + hneed:
                rep.err(P + "block #%d chắn chiều cao arena (cần %.1f)" % (j, hneed))
        if not (boss["arenaMinX"] + 1 <= boss["triggerX"] <= boss["arenaMinX"] + 3):
            rep.warn(P + "triggerX %.1f không gần arenaMinX+2" % boss["triggerX"])
        if any(e["x"] >= boss["arenaMinX"] - EPS for _, e in all_enemies):
            rep.err(P + "có enemy thường trong arena boss")
        if lv["exit"] is not None and lv["exit"][0] <= boss["arenaMaxX"]:
            rep.warn(P + "exit nằm trong / trước arena")
        rep.info.append(P + "arena %s: %.1f..%.1f (rộng %.1f), trigger %.1f"
                        % (name, boss["arenaMinX"], boss["arenaMaxX"], w, boss["triggerX"]))

    total = sum(counts.values())
    target = TARGET_COUNTS.get(lid)
    rep.info.append(P + "enemy (gồm boss) = %d %s, mục tiêu ~%s" % (total, dict(sorted(counts.items())), target))
    if target is not None and abs(total - target) > 2:
        rep.warn(P + "số enemy %d lệch mục tiêu ~%d" % (total, target))

    # --- đường đi spawn -> exit / arena ---
    if s_spawn is None:
        return
    edges, kinds = build_graph(blocks, surfaces)
    seen = reachable_from(edges, s_spawn)
    for label, g in goals:
        if g not in seen:
            rep.err(P + "không tới được %s từ spawn với tầm nhảy 75%%" % label)
            continue
        steps = path_to(seen, g)
        rep.info.append(P + "đường spawn -> %s qua %d mặt:" % (label, len(steps)))
        for a, b in zip(steps, steps[1:]):
            sa, sb = surfaces[a], surfaces[b]
            rep.info.append("      [%.1f..%.1f]@%.1f -> [%.1f..%.1f]@%.1f : %s"
                            % (sa["x1"], sa["x2"], sa["y"], sb["x1"], sb["x2"], sb["y"], kinds.get((a, b), "?")))
    for z in zones:
        for e in z["enemies"]:
            k = find_surface(surfaces, e["x"], e["y"], margin=0.4)
            if k is not None and k not in seen:
                rep.warn(P + "%s %s đứng trên mặt player không tới được" % (z["id"], e["prefab"]))


def main():
    path = sys.argv[1] if len(sys.argv) > 1 else os.path.join(os.path.dirname(os.path.abspath(__file__)), "levels.json")
    rep = Report()
    if hasattr(sys.stdout, "reconfigure"):
        sys.stdout.reconfigure(encoding="utf-8")  # -I bỏ qua PYTHONIOENCODING; console Windows mặc định cp125x
    with open(path, encoding="utf-8") as f:
        data = json.load(f)
    print("JSON parse OK: %s" % path)
    print("Tầm nhảy: g=%.2f, v0=%.1f -> H_max=%.3f (75%%: %.3f); gap phẳng max=%.3f (75%%: %.3f)"
          % (GRAVITY, JUMP_V, H_MAX, H_SAFE, jump_reach(0), jump_reach(0) * SAFETY))
    if data.get("version") != 1:
        rep.err("version != 1")
    ids = set()
    for lv in data.get("levels", []):
        if lv.get("id") in ids:
            rep.err("trùng id level %s" % lv.get("id"))
        ids.add(lv.get("id"))
        check_level(lv, rep)
        secs = lv.get("sections", [])
        if secs:
            print("%s: dài %.1f, %d section, %d block, %d zone"
                  % (lv["id"], secs[-1]["maxX"] - secs[0]["minX"], len(secs), len(lv["blocks"]), len(lv["spawnZones"])))
    print("Levels kiểm: %d" % len(ids))
    for m in rep.info:
        print("  " + m)
    for m in rep.warnings:
        print("WARN  " + m)
    for m in rep.errors:
        print("ERROR " + m)
    print("Kết quả: %d lỗi, %d cảnh báo" % (len(rep.errors), len(rep.warnings)))
    return 1 if rep.errors else 0


if __name__ == "__main__":
    sys.exit(main())
