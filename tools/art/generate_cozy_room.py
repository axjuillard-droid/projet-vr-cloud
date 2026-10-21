"""Génération procédurale de la chambre cocooning V2.
Nouveaux modèles : Box Internet domestique, rideaux tombants, guirlande micro-LEDs,
verre de fenêtre, coussins supplémentaires sur le lit, affiche musicale murale.
Exécution : blender --background --python tools/art/generate_cozy_room.py
"""
import bpy
import math
from pathlib import Path

ROOT = Path(__file__).resolve().parents[2]
MODELS_DIR = ROOT / "ConnectionTest/Assets/CloudVR/Models"
MODELS_DIR.mkdir(parents=True, exist_ok=True)


def reset_scene():
    bpy.ops.object.select_all(action="SELECT")
    bpy.ops.object.delete(use_global=False)
    for col in list(bpy.data.collections):
        bpy.data.collections.remove(col)
    scene = bpy.context.scene
    scene.unit_settings.system = "METRIC"
    scene.unit_settings.scale_length = 1.0


def mat(name, color, metallic=0.0, roughness=0.5, emission=0.0, emission_color=None):
    m = bpy.data.materials.new(name)
    m.use_nodes = True
    bsdf = m.node_tree.nodes.get("Principled BSDF")
    if bsdf:
        bsdf.inputs["Base Color"].default_value = color
        bsdf.inputs["Metallic"].default_value = metallic
        bsdf.inputs["Roughness"].default_value = roughness
        if emission > 0 and emission_color:
            if "Emission Color" in bsdf.inputs:
                bsdf.inputs["Emission Color"].default_value = emission_color
                bsdf.inputs["Emission Strength"].default_value = emission
    return m


def box(name, loc, dims, material, bevel=0.0):
    bpy.ops.mesh.primitive_cube_add(size=1, location=loc)
    o = bpy.context.object
    o.name = name
    o.dimensions = dims
    bpy.ops.object.transform_apply(scale=True)
    o.data.materials.append(material)
    if bevel > 0:
        mod = o.modifiers.new("Bevel", "BEVEL")
        mod.width = bevel
        mod.segments = 2
        bpy.ops.object.modifier_apply(modifier=mod.name)
    return o


def cylinder(name, loc, r, d, material, verts=12):
    bpy.ops.mesh.primitive_cylinder_add(vertices=verts, radius=r, depth=d, location=loc)
    o = bpy.context.object
    o.name = name
    bpy.ops.object.transform_apply(scale=True)
    o.data.materials.append(material)
    return o


def export_fbx(filename):
    path = str(MODELS_DIR / filename)
    bpy.ops.export_scene.fbx(
        filepath=path,
        use_selection=False,
        apply_scale_options='FBX_SCALE_ALL',
        mesh_smooth_type='FACE',
    )
    print(f"[OK] {filename} exporté.")


# ══════════════════════════════════════════════════════════════════
# 1. BOX INTERNET (Livebox / Bbox style)
# ══════════════════════════════════════════════════════════════════
def build_box_internet():
    reset_scene()

    m_white  = mat("Box_White",  (0.88, 0.87, 0.85, 1), roughness=0.35)
    m_dark   = mat("Box_Dark",   (0.08, 0.08, 0.10, 1), roughness=0.3)
    m_led_g  = mat("Box_LED_Green",  (0.15, 0.95, 0.35, 1), emission=3.5, emission_color=(0.15, 0.95, 0.35, 1))
    m_led_b  = mat("Box_LED_Blue",   (0.20, 0.55, 1.00, 1), emission=2.8, emission_color=(0.20, 0.55, 1.00, 1))
    m_led_o  = mat("Box_LED_Orange", (1.00, 0.50, 0.05, 1), emission=2.5, emission_color=(1.00, 0.50, 0.05, 1))
    m_port   = mat("Box_Port",   (0.05, 0.05, 0.05, 1), roughness=0.6)

    # Boîtier principal (28 × 9 × 3 cm)
    box("Box_Body",   (0, 0, 0.015), (0.28, 0.09, 0.03), m_white,  bevel=0.004)
    # Tranche avant légèrement en retrait (effet biseau)
    box("Box_Front",  (0, -0.041, 0.015), (0.275, 0.008, 0.028), m_dark, bevel=0.002)
    # Logo plateau supérieur
    box("Box_Logo",   (0, 0.02, 0.031), (0.06, 0.05, 0.001), m_dark)

    # Voyants LED (5 en façade)
    led_labels = [m_led_g, m_led_b, m_led_b, m_led_g, m_led_o]
    for i, lm in enumerate(led_labels):
        lx = -0.08 + i * 0.04
        box(f"Box_LED_{i}", (lx, -0.042, 0.024), (0.007, 0.002, 0.007), lm)

    # Ports LAN arrière
    for i in range(4):
        px = -0.06 + i * 0.032
        box(f"Box_Port_LAN_{i}", (px, 0.048, 0.012), (0.017, 0.005, 0.012), m_port)
    # Port WAN (fibre optique SC-APC, vert)
    box("Box_Port_WAN", (0.08, 0.048, 0.012), (0.014, 0.005, 0.014), m_led_g)
    # Bouton reset
    cylinder("Box_Reset", (-0.12, 0.046, 0.014), r=0.003, d=0.004, material=m_dark)

    export_fbx("Box_Internet.fbx")


# ══════════════════════════════════════════════════════════════════
# 2. RIDEAUX TOMBANTS (Curtains)  — deux panneaux de tissu épais
# ══════════════════════════════════════════════════════════════════
def build_curtains():
    reset_scene()

    m_fabric_dark = mat("Curtain_Fabric", (0.22, 0.16, 0.32, 1), roughness=0.85)  # Velours prune
    m_rod         = mat("Curtain_Rod",    (0.55, 0.48, 0.38, 1), metallic=0.7, roughness=0.3)
    m_ring        = mat("Curtain_Ring",   (0.60, 0.50, 0.35, 1), metallic=0.85, roughness=0.2)

    # Tringle (1.60m, posée à y=0 = mur fenêtre)
    box("Tringle", (0, 0, 2.32), (1.62, 0.025, 0.025), m_rod)
    # Embouts
    for sx in (-1, 1):
        cylinder(f"Tringle_Embout_{sx}", (sx * 0.82, 0, 2.32), r=0.02, d=0.025, material=m_rod, verts=8)

    # Anneaux de rideau (10 par panneau)
    for panel_x, sign in ((-0.55, -1), (0.55, 1)):
        n_rings = 8
        spread = 0.52
        for i in range(n_rings):
            rx = panel_x - sign * (spread / 2) + sign * (spread / n_rings) * i
            cylinder(f"Ring_{panel_x:.0f}_{i}", (rx, 0, 2.30), r=0.012, d=0.018, material=m_ring, verts=8)

    # Panneaux de tissu — subdivision verticale via loop cuts simulé par empilement
    # Panneau gauche
    box("Curtain_Left",  (-0.52, 0.02, 1.18), (0.52, 0.04, 2.28), m_fabric_dark, bevel=0.005)
    # Panneau droit
    box("Curtain_Right", ( 0.52, 0.02, 1.18), (0.52, 0.04, 2.28), m_fabric_dark, bevel=0.005)

    # Replis naturels (franges verticales au bas)
    for panel_x in (-0.52, 0.52):
        for i in range(3):
            fx = panel_x + (i - 1) * 0.10
            box(f"Repli_{panel_x:.0f}_{i}", (fx, 0.025 + i * 0.005, 0.07), (0.08, 0.01 + i * 0.003, 0.14), m_fabric_dark)

    export_fbx("Room_Curtains.fbx")


# ══════════════════════════════════════════════════════════════════
# 3. GUIRLANDE MICRO-LED (Fairy Lights) sur étagère
# ══════════════════════════════════════════════════════════════════
def build_fairy_lights():
    reset_scene()

    m_wire  = mat("FairyLight_Wire",  (0.08, 0.08, 0.08, 1), roughness=0.8)
    m_bulb  = mat("FairyLight_Bulb",  (1.0, 0.95, 0.60, 1), emission=3.0, emission_color=(1.0, 0.80, 0.40, 1))

    # Fil principal (1.10m)
    box("Wire_Main", (0, 0, 0), (1.10, 0.003, 0.003), m_wire)

    # 20 micro-ampoules répartis sur le fil
    n = 20
    for i in range(n):
        lx = -0.50 + (i / (n - 1)) * 1.00
        lz = -0.008 + math.sin(i * 1.3) * 0.006  # léger ondulation
        cylinder(f"Bulb_{i}", (lx, 0, lz - 0.006), r=0.004, d=0.010, material=m_bulb, verts=6)

    export_fbx("Fairy_Lights.fbx")


# ══════════════════════════════════════════════════════════════════
# 4. CAMPUS EXTÉRIEUR DATA CENTER (DataCenter_Campus.fbx)
# ══════════════════════════════════════════════════════════════════
def build_datacenter_campus():
    """Ensemble extérieur du campus : bâtiment principal, clôture, guérite,
    transformateurs HT, conteneurs groupes électrogènes, parking, portail."""
    reset_scene()

    m_concrete  = mat("Campus_Concrete",  (0.45, 0.44, 0.42, 1), roughness=0.8)
    m_facade    = mat("Campus_Facade",    (0.18, 0.20, 0.22, 1), roughness=0.55)   # Bardage métal anthracite
    m_glass     = mat("Campus_Glass",     (0.55, 0.75, 0.80, 0.35), roughness=0.05, metallic=0.1)
    m_yellow    = mat("Campus_Yellow",    (0.98, 0.82, 0.05, 1), roughness=0.5)
    m_black     = mat("Campus_Black",     (0.06, 0.06, 0.06, 1), roughness=0.5)
    m_metal     = mat("Campus_Metal",     (0.35, 0.38, 0.42, 1), metallic=0.8, roughness=0.3)
    m_logo      = mat("Campus_Logo",      (0.90, 0.92, 0.94, 1), emission=1.5, emission_color=(0.90, 0.92, 0.94, 1))
    m_fence     = mat("Campus_Fence",     (0.22, 0.24, 0.26, 1), metallic=0.6, roughness=0.4)
    m_asphalt   = mat("Campus_Asphalt",   (0.12, 0.12, 0.13, 1), roughness=0.9)
    m_generator = mat("Campus_Generator", (0.20, 0.22, 0.24, 1), roughness=0.5)
    m_trafo     = mat("Campus_Trafo",     (0.25, 0.30, 0.35, 1), roughness=0.55)
    m_led_g     = mat("Campus_LED_Green", (0.15, 0.95, 0.35, 1), emission=3.0, emission_color=(0.15, 0.95, 0.35, 1))
    m_led_r     = mat("Campus_LED_Red",   (0.95, 0.15, 0.10, 1), emission=3.0, emission_color=(0.95, 0.15, 0.10, 1))
    m_pipe      = mat("Campus_Pipe",      (0.60, 0.62, 0.65, 1), metallic=0.75, roughness=0.25)

    # ── Sol / Parking (40m × 30m) ──────────────────────────────────────
    box("Sol_Parking", (0, 0, -0.05), (40, 30, 0.10), m_asphalt)
    # Marquage au sol : bandes jaunes
    for i in range(6):
        box(f"Marquage_{i}", (-15 + i * 6, -8, 0.001), (0.15, 5, 0.002), m_yellow)

    # ── Bâtiment principal (28m × 18m × 14m) ────────────────────────────
    # Structure béton
    box("DC_Building_Core",    (0, 5, 7),    (28, 18, 14), m_concrete)
    # Façade avant bardage métal
    box("DC_Facade_Front",     (0, -4.1, 7), (28, 0.4,  14), m_facade)
    # Bandes de fenêtres horizontales (3 rangées, 5 groupes de 4)
    for row in range(3):
        fz = 3 + row * 3.5
        for col in range(5):
            fx = -11 + col * 5.5
            box(f"Fenetre_R{row}_C{col}", (fx, -4.0, fz), (3.8, 0.05, 1.6), m_glass)
    # Bandeau LED logo en haut de façade
    box("DC_Logo_Band", (0, -4.12, 12.8), (16, 0.08, 0.80), m_logo)
    # Auvent entrée principale
    box("DC_Auvent",   (0, -5.5, 5.2), (8, 3, 0.25), m_metal)

    # ── Guérite de contrôle d'accès (2.4m × 3m × 3m) ───────────────────
    box("Guerite_Body",        (-16, -10, 1.5), (2.4, 3.0, 3.0), m_concrete)
    box("Guerite_Vitres",      (-16, -11.4, 2.0), (2.2, 0.06, 1.8), m_glass)
    box("Guerite_Toit",        (-16, -10, 3.1), (2.8, 3.6, 0.15), m_metal)
    # Feux d'entrée (rouge/vert)
    cylinder("Guerite_Feu_R",  (-14.8, -11.5, 2.8), r=0.06, d=0.12, material=m_led_r, verts=8)
    cylinder("Guerite_Feu_G",  (-14.8, -11.5, 2.5), r=0.06, d=0.12, material=m_led_g, verts=8)

    # ── Barrière levante (5m) ─────────────────────────────────────────────
    box("Barriere_Montant",   (-13.5, -12, 1.2), (0.12, 0.12, 2.4), m_yellow)
    box("Barriere_Bras",      (-11.0, -12, 2.42),(5.0, 0.08, 0.08), m_yellow)
    box("Barriere_BrasNoir1", (-10.0, -12, 2.42),(0.08, 0.08, 0.4), m_black)
    box("Barriere_BrasNoir2", (-9.0, -12, 2.42), (0.08, 0.08, 0.4), m_black)

    # ── Clôture périmétrique (panneaux rigides grillagés) ─────────────────
    fence_segments = [
        # (x, y, lx, ly)
        (0, -13, 28, 0.15),   # façade avant
        (14.1, -6, 0.15, 14), # côté droit
        (-14.1, -6, 0.15, 14),# côté gauche
    ]
    for i, (fx, fy, flx, fly) in enumerate(fence_segments):
        box(f"Fence_{i}_Base",   (fx, fy, 0.75), (flx, fly, 1.5), m_fence)
        box(f"Fence_{i}_Barbelé",(fx, fy, 1.65), (flx + 0.1, fly + 0.1, 0.08), m_metal)
    # Poteaux de clôture tous les 4m
    for px in range(-14, 15, 4):
        box(f"Fence_Poteau_{px}", (px, -13, 1.0), (0.12, 0.12, 2.0), m_fence)

    # ── Transformateurs HT (côté droit du bâtiment) ───────────────────────
    for i in range(2):
        tx = 12 + i * 3.5
        box(f"Trafo_{i}_Body",  (tx, 2, 1.6), (2.8, 2.0, 3.2), m_trafo)
        box(f"Trafo_{i}_Top",   (tx, 2, 3.3), (2.9, 2.1, 0.2), m_metal)
        # Traversées isolateurs
        for j in range(3):
            cylinder(f"Trafo_{i}_Iso_{j}", (tx - 0.8 + j * 0.8, 1.1, 3.5), r=0.07, d=0.5, material=m_pipe, verts=8)
        # Grillage de protection
        box(f"Trafo_{i}_Grille", (tx, 1.0, 1.6), (3.2, 0.06, 3.4), m_fence)
        box(f"Trafo_{i}_GrR",   (tx + 1.5, 2.0, 1.6), (0.06, 2.2, 3.4), m_fence)
        box(f"Trafo_{i}_GrL",   (tx - 1.5, 2.0, 1.6), (0.06, 2.2, 3.4), m_fence)

    # ── Groupes électrogènes diesel (conteneurs Caterpillar, côté gauche) ──
    gen_colors = [m_generator, m_generator, m_generator]
    for i, gm in enumerate(gen_colors):
        gx = -14 + i * 0  # alignés en batterie
        gy = 2 + i * 4.5
        box(f"Genset_{i}_Container", (-12, gy, 1.5), (6.0, 2.4, 3.0), gm)
        box(f"Genset_{i}_Grille",    (-12, gy - 1.25, 1.5), (5.8, 0.06, 2.6), m_fence)
        # Pot d'échappement vertical inox
        cylinder(f"Genset_{i}_Exhaust", (-10.2, gy - 1.0, 3.6), r=0.12, d=1.5, material=m_pipe, verts=8)
        box(f"Genset_{i}_Chapeau",   (-10.2, gy - 1.0, 4.4), (0.30, 0.30, 0.08), m_pipe)

    # ── Caméras PTZ extérieures (4 angles) ────────────────────────────────
    cam_positions = [(-13.8, -12.8), (13.8, -12.8), (14, 1), (-14, 1)]
    for ci, (cx, cy) in enumerate(cam_positions):
        box(f"Cam_{ci}_Mât",    (cx, cy, 3.5), (0.08, 0.08, 7.0), m_metal)
        box(f"Cam_{ci}_Bras",   (cx, cy, 7.2), (0.06, 0.8, 0.06), m_metal)
        box(f"Cam_{ci}_Corps",  (cx, cy + 0.45, 7.1), (0.14, 0.22, 0.12), m_black)
        cylinder(f"Cam_{ci}_Dome", (cx, cy + 0.45, 7.0), r=0.07, d=0.14, material=m_black, verts=10)

    # ── Éclairage extérieur (mâts LED 8m) ───────────────────────────────
    for li, (lx, ly) in enumerate([(-10, -13), (10, -13), (10, 6), (-10, 6)]):
        box(f"Light_Mat_{li}",    (lx, ly, 4.0), (0.12, 0.12, 8.0), m_metal)
        box(f"Light_Head_{li}",   (lx, ly, 8.1), (1.20, 0.30, 0.12), m_metal)
        box(f"Light_LED_{li}",    (lx, ly, 8.0), (1.10, 0.25, 0.05), m_logo)

    export_fbx("DataCenter_Campus.fbx")


# ══════════════════════════════════════════════════════════════════
# 5. SAS MANTRAP UNITAIRE BIOMÉTRIQUE
# ══════════════════════════════════════════════════════════════════
def build_mantrap():
    reset_scene()

    m_steel      = mat("Mantrap_Steel",   (0.30, 0.33, 0.36, 1), metallic=0.85, roughness=0.25)
    m_glass_b    = mat("Mantrap_Glass",   (0.45, 0.65, 0.75, 0.35), roughness=0.05, metallic=0.05)
    m_floor      = mat("Mantrap_Floor",   (0.18, 0.22, 0.18, 1), roughness=0.4)
    m_led_g      = mat("Mantrap_LED_G",   (0.15, 0.95, 0.35, 1), emission=4.0, emission_color=(0.15, 0.95, 0.35, 1))
    m_led_r      = mat("Mantrap_LED_R",   (0.95, 0.15, 0.10, 1), emission=4.0, emission_color=(0.95, 0.15, 0.10, 1))
    m_scanner    = mat("Mantrap_Scanner", (0.05, 0.55, 0.90, 1), emission=1.8, emission_color=(0.05, 0.55, 0.90, 1))
    m_warn       = mat("Mantrap_Warn",    (0.95, 0.75, 0.05, 1), roughness=0.4)
    m_dark       = mat("Mantrap_Dark",    (0.05, 0.05, 0.06, 1), roughness=0.5)
    m_scale      = mat("Mantrap_Scale",   (0.40, 0.45, 0.50, 1), metallic=0.7, roughness=0.3)

    sas_r = 0.90   # rayon interne du sas cylindrique
    sas_h = 2.60   # hauteur

    # Murs cylindriques en verre blindé (anneau)
    bpy.ops.mesh.primitive_cylinder_add(vertices=24, radius=sas_r + 0.06, depth=sas_h, location=(0, 0, sas_h / 2))
    outer = bpy.context.object
    outer.name = "Mantrap_Wall_Outer"
    outer.data.materials.append(m_steel)

    bpy.ops.mesh.primitive_cylinder_add(vertices=24, radius=sas_r, depth=sas_h + 0.01, location=(0, 0, sas_h / 2))
    inner = bpy.context.object
    inner.name = "Mantrap_Wall_Inner"
    inner.data.materials.append(m_glass_b)

    # Sol avec dalle de pesée (balance dynamique)
    box("Mantrap_Floor",   (0, 0, 0.01), (sas_r * 1.8, sas_r * 1.8, 0.02), m_floor)
    box("Scale_Plate",     (0, 0, 0.025), (0.80, 0.80, 0.015), m_scale)
    # Capteurs de poids (4 coins)
    for sx in (-0.35, 0.35):
        for sy in (-0.35, 0.35):
            cylinder(f"Scale_Sensor_{sx}_{sy}", (sx, sy, 0.02), r=0.03, d=0.012, material=m_dark, verts=6)

    # Toit en acier
    bpy.ops.mesh.primitive_cylinder_add(vertices=24, radius=sas_r + 0.07, depth=0.08, location=(0, 0, sas_h + 0.04))
    roof = bpy.context.object
    roof.name = "Mantrap_Roof"
    roof.data.materials.append(m_steel)

    # ── Porte d'entrée (avant, -Y) ────────────────────────────────────────
    # Cadre
    box("Door_Entry_Frame",  (0, -(sas_r + 0.03), sas_h / 2), (1.10, 0.12, sas_h), m_steel)
    # Battant
    box("Door_Entry_Panel",  (0, -(sas_r - 0.02), sas_h / 2), (0.95, 0.06, sas_h - 0.20), m_glass_b)
    # Poignée
    box("Door_Entry_Handle", (0.42, -(sas_r + 0.06), sas_h / 2 - 0.10), (0.025, 0.14, 0.025), m_steel)
    # Bande d'avertissement
    box("Door_Entry_Warn",   (0, -(sas_r + 0.09), 0.06), (1.05, 0.09, 0.12), m_warn)
    # Diode statut accès
    cylinder("Door_Entry_LED", (0.50, -(sas_r + 0.08), sas_h - 0.15), r=0.025, d=0.04, material=m_led_r, verts=8)

    # ── Porte de sortie (arrière, +Y) ─────────────────────────────────────
    box("Door_Exit_Frame",   (0, (sas_r + 0.03), sas_h / 2), (1.10, 0.12, sas_h), m_steel)
    box("Door_Exit_Panel",   (0, (sas_r - 0.02), sas_h / 2), (0.95, 0.06, sas_h - 0.20), m_glass_b)
    box("Door_Exit_Handle",  (0.42, (sas_r + 0.06), sas_h / 2 - 0.10), (0.025, 0.14, 0.025), m_steel)
    box("Door_Exit_Warn",    (0, (sas_r + 0.09), 0.06), (1.05, 0.09, 0.12), m_warn)
    cylinder("Door_Exit_LED", (0.50, (sas_r + 0.08), sas_h - 0.15), r=0.025, d=0.04, material=m_led_g, verts=8)

    # ── Terminal biométrique (lecteur rétinien + badge RFID) ─────────────
    box("BioTerm_Body",    (-sas_r + 0.10, 0, 1.30), (0.22, 0.08, 0.36), m_dark, bevel=0.008)
    box("BioTerm_Screen",  (-sas_r + 0.15, 0, 1.30), (0.16, 0.03, 0.26), m_scanner)
    box("BioTerm_RFID",    (-sas_r + 0.14, 0, 1.10), (0.12, 0.03, 0.08), m_dark)
    cylinder("BioTerm_Iris",(-sas_r + 0.16, 0, 1.45), r=0.030, d=0.04, material=m_scanner, verts=12)
    # LED statut du terminal
    cylinder("BioTerm_LED_R", (-sas_r + 0.14, 0, 1.62), r=0.012, d=0.03, material=m_led_r, verts=6)
    cylinder("BioTerm_LED_G", (-sas_r + 0.14, 0, 1.55), r=0.012, d=0.03, material=m_led_g, verts=6)

    # ── Bandeau lumineux au plafond (annulaire vert) ───────────────────────
    bpy.ops.mesh.primitive_torus_add(major_radius=0.80, minor_radius=0.035, major_segments=24, minor_segments=6, location=(0, 0, 2.48))
    torus = bpy.context.object
    torus.name = "Mantrap_Ceiling_Ring"
    torus.data.materials.append(m_led_g)

    # ── Caméra intérieure de surveillance ─────────────────────────────────
    box("Cam_Body",    (0, 0, 2.45), (0.10, 0.14, 0.08), m_dark)
    cylinder("Cam_Lens", (0, -0.08, 2.45), r=0.025, d=0.06, material=m_scanner, verts=8)

    export_fbx("Mantrap_Airlock.fbx")


# ══════════════════════════════════════════════════════════════════
# 6. GRANDE SALLE BLANCHE MONUMENTALE (White Space Hall)
# ══════════════════════════════════════════════════════════════════
def build_white_space():
    """Salle informatique monumentale : 40m × 18m × 5m, confinement allée froide,
    plancher surélevé, Busways suspendus, 32 baies 42U, CRAC en périphérie."""
    reset_scene()

    m_floor_tile = mat("WS_FloorTile",   (0.60, 0.62, 0.64, 1), metallic=0.4, roughness=0.25)
    m_floor_perf = mat("WS_FloorPerf",   (0.25, 0.28, 0.32, 1), metallic=0.5, roughness=0.35)  # Dalles perforées
    m_ceiling    = mat("WS_Ceiling",     (0.85, 0.86, 0.88, 1), roughness=0.7)
    m_rack_body  = mat("WS_RackBody",    (0.10, 0.11, 0.13, 1), metallic=0.6, roughness=0.3)
    m_rack_door  = mat("WS_RackDoor",    (0.08, 0.09, 0.11, 1), metallic=0.7, roughness=0.15)
    m_rack_led_g = mat("WS_RackLED_G",   (0.12, 0.95, 0.35, 1), emission=2.5, emission_color=(0.12, 0.95, 0.35, 1))
    m_rack_led_b = mat("WS_RackLED_B",   (0.05, 0.55, 1.00, 1), emission=2.0, emission_color=(0.05, 0.55, 1.00, 1))
    m_cac_glass  = mat("WS_CAC_Glass",   (0.55, 0.80, 0.85, 0.25), roughness=0.04, metallic=0.0)
    m_cac_frame  = mat("WS_CAC_Frame",   (0.20, 0.22, 0.25, 1), metallic=0.6, roughness=0.3)
    m_busway     = mat("WS_Busway",      (0.68, 0.50, 0.12, 1), metallic=0.3, roughness=0.5)   # Cuivre/jaune
    m_crac       = mat("WS_CRAC",        (0.20, 0.22, 0.26, 1), roughness=0.4)
    m_duct       = mat("WS_Duct",        (0.58, 0.60, 0.65, 1), metallic=0.5, roughness=0.35)
    m_label      = mat("WS_Label",       (1.0, 1.0, 1.0, 1), roughness=0.9)
    m_cold_air   = mat("WS_ColdAirGlow", (0.55, 0.85, 1.0, 0.12), roughness=0.02)

    # ── Faux plancher surélevé (40m × 18m, élevé de 0.80m) ──────────────
    # Piédroits de support (tous les 0.60m)
    for ix in range(-19, 20, 3):
        for iy in range(-8, 9, 3):
            box(f"Pedroit_{ix}_{iy}", (ix * 0.6, iy * 0.6, 0.38), (0.05, 0.05, 0.76), m_rack_body)

    # Dalles du plancher (2 types alternés)
    for ix in range(-20, 21):
        for iy in range(-9, 10):
            cx, cy = ix * 0.60, iy * 0.60
            # Dalles perforées dans les allées froides (ix pair)
            is_perf = (ix % 4 == 0)
            dm = m_floor_perf if is_perf else m_floor_tile
            box(f"Dalle_{ix}_{iy}", (cx, cy, 0.78), (0.595, 0.595, 0.04), dm)

    # Lumière bleutée de sous-plancher (allées froides)
    for ix in range(-18, 20, 4):
        box(f"UnderFloor_Glow_{ix}", (ix * 0.6, 0, 0.40), (0.55, 17.5, 0.01), m_cold_air)

    # ── Plafond ──────────────────────────────────────────────────────────
    box("WS_Ceiling", (0, 0, 5.0), (40, 18, 0.20), m_ceiling)

    # ── Murs (simplifiés) ───────────────────────────────────────────────
    box("WS_Wall_N", (0,  9.1, 2.5), (40, 0.20, 5.0), m_ceiling)
    box("WS_Wall_S", (0, -9.1, 2.5), (40, 0.20, 5.0), m_ceiling)
    box("WS_Wall_E", (20.1, 0, 2.5), (0.20, 18, 5.0), m_ceiling)
    box("WS_Wall_W", (-20.1, 0, 2.5), (0.20, 18, 5.0), m_ceiling)

    # ── 32 Baies serveurs 42U (8 rangées × 4 colonnes) ──────────────────
    rack_w, rack_d, rack_h = 0.60, 1.10, 2.00   # 600×1100×2000 mm (42U)
    col_positions = [-12, -4, 4, 12]             # 4 colonnes (x)
    row_positions = list(range(-7, 8, 2))         # 8 rangées (y) → 7 gaps réels

    for ri, ry in enumerate(row_positions):
        for ci, rx in enumerate(col_positions):
            rid = f"Rack_{ri}_{ci}"
            # Corps de baie
            box(f"{rid}_Body", (rx, ry * 0.9, 0.80 + rack_h / 2), (rack_w - 0.02, rack_d - 0.02, rack_h), m_rack_body)
            # Porte avant vitrée
            box(f"{rid}_Door", (rx, ry * 0.9 - rack_d / 2, 0.80 + rack_h / 2), (rack_w - 0.04, 0.02, rack_h - 0.04), m_rack_door)
            # Bande LED façade
            box(f"{rid}_LED",  (rx, ry * 0.9 - rack_d / 2 - 0.01, 0.80 + rack_h - 0.15), (rack_w - 0.10, 0.008, 0.06), m_rack_led_g)
            # Patch panel bleu (haut)
            box(f"{rid}_Patch", (rx, ry * 0.9 - rack_d / 2 - 0.01, 0.80 + rack_h - 0.30), (rack_w - 0.10, 0.008, 0.035), m_rack_led_b)
            # Étiquette blanche
            box(f"{rid}_Label", (rx, ry * 0.9 - rack_d / 2 - 0.015, 0.80 + rack_h - 0.55), (0.10, 0.003, 0.06), m_label)

    # ── Confinement allée froide : toitures polycarbonate + portes ────────
    for ci_pair in range(2):   # 2 allées froides (entre col 0-1 et col 2-3)
        acx = (-4 + ci_pair * 16)  # centrée entre les 2 colonnes
        # Toit CAC transparent (longueur de la salle)
        box(f"CAC_Roof_{ci_pair}", (acx, 0, 2.82), (4.5, 15.5, 0.06), m_cac_glass)
        # Cadres du toit CAC
        box(f"CAC_Frame_L_{ci_pair}", (acx - 2.18, 0, 2.82), (0.08, 15.5, 0.08), m_cac_frame)
        box(f"CAC_Frame_R_{ci_pair}", (acx + 2.18, 0, 2.82), (0.08, 15.5, 0.08), m_cac_frame)
        # Portes coulissantes translucides (aux 2 extrémités de l'allée)
        for sign in (-1, 1):
            box(f"CAC_Door_{ci_pair}_{sign}", (acx, sign * 7.7, 1.88), (4.30, 0.04, 2.12), m_cac_glass)
            box(f"CAC_DoorFrame_{ci_pair}_{sign}", (acx, sign * 7.7, 1.88), (4.38, 0.08, 2.20), m_cac_frame)

    # ── Busways suspendus au plafond (chemins de câbles horizontaux) ──────
    for by in range(-8, 9, 2):
        box(f"Busway_H_{by}", (0, by * 0.55, 4.85), (38, 0.12, 0.10), m_busway)
    # Verticaux le long des rangées
    for bx in [-13.5, -4.5, 3.5, 11.5]:
        box(f"Busway_V_{bx:.0f}", (bx, 0, 4.85), (0.10, 17, 0.10), m_busway)
    # Cordons de descente vers les racks
    for ci, rx in enumerate(col_positions):
        for ri in range(len(row_positions)):
            ry = row_positions[ri] * 0.9
            box(f"PDU_Drop_{ci}_{ri}", (rx, ry, 2.85), (0.04, 0.04, 2.10), m_busway)

    # ── CRAC (Computer Room Air Conditioner) en périphérie ───────────────
    crac_positions = [
        (-19, -7), (-19, 0), (-19, 7),
        ( 19, -7), ( 19, 0), ( 19, 7),
    ]
    for crx, cry in crac_positions:
        box(f"CRAC_{crx}_{cry}_Body",  (crx, cry, 0.80 + 1.0), (0.90, 0.60, 2.0), m_crac)
        box(f"CRAC_{crx}_{cry}_Grille",(crx, cry - 0.32, 1.30), (0.85, 0.04, 0.90), m_rack_body)
        box(f"CRAC_{crx}_{cry}_LED",   (crx, cry - 0.32, 2.60), (0.80, 0.02, 0.06), m_rack_led_b)
        # Gaines de soufflage sous plancher
        box(f"CRAC_{crx}_{cry}_Duct",  (crx, cry, 0.40), (0.30, 0.30, 0.80), m_duct)

    # ── Éclairage de la salle (réglettes LED suspendues) ──────────────────
    m_light_strip = mat("WS_LightStrip", (0.95, 0.95, 1.0, 1), emission=2.0, emission_color=(0.85, 0.90, 1.0, 1))
    for lx in range(-18, 20, 4):
        box(f"Light_Strip_{lx}", (lx, 0, 4.90), (0.08, 17, 0.04), m_light_strip)

    export_fbx("WhiteSpace_Hall.fbx")


# ══════════════════════════════════════════════════════════════════
# 7. LOCAL TECHNIQUE TGBT / UPS (Tableau Électrique + Onduleurs)
# ══════════════════════════════════════════════════════════════════
def build_electrical_room():
    reset_scene()

    m_panel  = mat("Elec_Panel",  (0.15, 0.20, 0.18, 1), roughness=0.4)
    m_copper = mat("Elec_Copper", (0.72, 0.45, 0.20, 1), metallic=0.8, roughness=0.25)
    m_wire   = mat("Elec_Wire",   (0.05, 0.05, 0.05, 1), roughness=0.7)
    m_ups    = mat("Elec_UPS",    (0.14, 0.16, 0.20, 1), roughness=0.4)
    m_batt   = mat("Elec_Batt",   (0.10, 0.10, 0.12, 1), roughness=0.5)
    m_led_g  = mat("Elec_LED_G",  (0.15, 0.95, 0.35, 1), emission=3.0, emission_color=(0.15, 0.95, 0.35, 1))
    m_led_r  = mat("Elec_LED_R",  (0.95, 0.15, 0.10, 1), emission=3.0, emission_color=(0.95, 0.15, 0.10, 1))
    m_led_o  = mat("Elec_LED_O",  (0.98, 0.55, 0.05, 1), emission=2.5, emission_color=(0.98, 0.55, 0.05, 1))
    m_warn   = mat("Elec_Warn",   (0.98, 0.82, 0.05, 1), roughness=0.5)
    m_floor  = mat("Elec_Floor",  (0.30, 0.30, 0.32, 1), roughness=0.7)
    m_busbar = mat("Elec_Busbar", (0.85, 0.60, 0.20, 1), metallic=0.9, roughness=0.2)

    # Sol
    box("Elec_Floor", (0, 0, -0.02), (20, 12, 0.04), m_floor)

    # ── TGBT Voie A (gauche) ─────────────────────────────────────────────
    # 6 armoires de distribution de 0.80m × 0.60m × 2.10m
    for i in range(6):
        ax = -8.5 + i * 0.85
        box(f"TGBT_A_Cabinet_{i}", (ax, -4.5, 1.05), (0.80, 0.60, 2.10), m_panel, bevel=0.005)
        box(f"TGBT_A_Door_{i}",    (ax, -4.82, 1.05), (0.75, 0.04, 2.04), m_panel)
        # Indicateurs sur les portes
        cylinder(f"TGBT_A_LED_G_{i}", (ax - 0.30, -4.85, 1.80), r=0.018, d=0.03, material=m_led_g, verts=8)
        cylinder(f"TGBT_A_LED_R_{i}", (ax - 0.20, -4.85, 1.80), r=0.018, d=0.03, material=m_led_r, verts=8)
        # Afficheur numérique
        box(f"TGBT_A_Display_{i}", (ax + 0.12, -4.85, 1.85), (0.24, 0.02, 0.10), m_led_o)
        # Bandes d'avertissement
        box(f"TGBT_A_Warn_{i}", (ax, -4.85, 0.12), (0.75, 0.04, 0.14), m_warn)

    # Jeu de barres busbar Voie A (cuivre nu)
    box("BusBar_A_L1", (-5.5, -4.1, 2.05), (4.8, 0.05, 0.08), m_busbar)
    box("BusBar_A_L2", (-5.5, -4.0, 2.05), (4.8, 0.05, 0.08), m_busbar)
    box("BusBar_A_L3", (-5.5, -3.9, 2.05), (4.8, 0.05, 0.08), m_busbar)
    box("BusBar_A_PE", (-5.5, -3.75, 2.05), (4.8, 0.05, 0.06), m_copper)

    # ── TGBT Voie B (droite) ─────────────────────────────────────────────
    for i in range(6):
        bx = -8.5 + i * 0.85
        box(f"TGBT_B_Cabinet_{i}", (bx, 4.5, 1.05), (0.80, 0.60, 2.10), m_panel, bevel=0.005)
        box(f"TGBT_B_Door_{i}",    (bx, 4.82, 1.05), (0.75, 0.04, 2.04), m_panel)
        cylinder(f"TGBT_B_LED_G_{i}", (bx - 0.30, 4.85, 1.80), r=0.018, d=0.03, material=m_led_g, verts=8)
        cylinder(f"TGBT_B_LED_R_{i}", (bx - 0.20, 4.85, 1.80), r=0.018, d=0.03, material=m_led_r, verts=8)
        box(f"TGBT_B_Display_{i}", (bx + 0.12, 4.85, 1.85), (0.24, 0.02, 0.10), m_led_o)
        box(f"TGBT_B_Warn_{i}", (bx, 4.85, 0.12), (0.75, 0.04, 0.14), m_warn)

    box("BusBar_B_L1", (-5.5, 4.1, 2.05), (4.8, 0.05, 0.08), m_busbar)
    box("BusBar_B_L2", (-5.5, 4.0, 2.05), (4.8, 0.05, 0.08), m_busbar)
    box("BusBar_B_L3", (-5.5, 3.9, 2.05), (4.8, 0.05, 0.08), m_busbar)

    # ── Onduleurs UPS (4 armoires, 160 kVA chacune) ──────────────────────
    for i in range(4):
        ux = 3 + i * 2.20
        box(f"UPS_A_{i}", (ux, -4.5, 1.10), (1.90, 0.80, 2.20), m_ups, bevel=0.005)
        box(f"UPS_A_Front_{i}", (ux, -4.92, 1.10), (1.85, 0.04, 2.14), m_panel)
        box(f"UPS_A_Screen_{i}", (ux - 0.40, -4.94, 1.80), (0.55, 0.02, 0.28), m_led_o)
        cylinder(f"UPS_A_LED_{i}", (ux + 0.65, -4.93, 2.05), r=0.020, d=0.03, material=m_led_g, verts=8)

        box(f"UPS_B_{i}", (ux, 4.5, 1.10), (1.90, 0.80, 2.20), m_ups, bevel=0.005)
        box(f"UPS_B_Front_{i}", (ux, 4.92, 1.10), (1.85, 0.04, 2.14), m_panel)
        box(f"UPS_B_Screen_{i}", (ux - 0.40, 4.94, 1.80), (0.55, 0.02, 0.28), m_led_o)
        cylinder(f"UPS_B_LED_{i}", (ux + 0.65, 4.93, 2.05), r=0.020, d=0.03, material=m_led_g, verts=8)

    # ── Batteries au lithium-ion (rack de batteries derrière les UPS) ─────
    for i in range(8):
        bax = 3 + (i // 4) * 4.5 + (i % 4) * 1.10
        bay_y = -2.0 if i < 4 else 2.0
        box(f"Batt_Rack_{i}", (bax, bay_y, 1.05), (1.0, 0.70, 2.10), m_batt, bevel=0.003)
        box(f"Batt_LED_{i}",  (bax - 0.35, bay_y - 0.37 * (1 if i < 4 else -1), 2.00), (0.55, 0.03, 0.06), m_led_g)

    # ── Câblerie principale ────────────────────────────────────────────────
    # Chemins de câble aériens
    for cy in (-1.5, -0.5, 0.5, 1.5):
        box(f"Cable_Tray_{cy}", (-5, cy, 2.55), (17.5, 0.40, 0.06), m_wire)

    export_fbx("Electrical_Room.fbx")


if __name__ == "__main__":
    print("=== DÉBUT GÉNÉRATION ASSETS COCOONING + DATA CENTER COMPLET ===")
    build_box_internet()
    build_curtains()
    build_fairy_lights()
    build_datacenter_campus()
    build_mantrap()
    build_white_space()
    build_electrical_room()
    print("=== TOUS LES MODÈLES ONT ÉTÉ GÉNÉRÉS AVEC SUCCÈS ===")
