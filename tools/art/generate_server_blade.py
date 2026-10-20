"""Générateur 3D Blender du Serveur Rack 2U Extractible et de la Porte de Baie Ouvrable.
Exécuté en headless avec Blender 4.5.2 LTS.
Unités métriques, zéro asset externe, matériaux PBR émissifs optimisés URP.
"""
import bpy
import math
from pathlib import Path

ROOT = Path(__file__).resolve().parents[2]
MODELS_DIR = ROOT / "ConnectionTest/Assets/CloudVR/Models"
MODELS_DIR.mkdir(parents=True, exist_ok=True)

# ══════════════════════════════════════════════════════════════════
# 1. SERVEUR RACK 2U EXTRACTIBLE AVEC COMPOSANTS INTERNES
# ══════════════════════════════════════════════════════════════════

def build_server_blade():
    bpy.ops.wm.read_factory_settings(use_empty=True)
    scene = bpy.context.scene
    scene.unit_settings.system = "METRIC"
    scene.unit_settings.scale_length = 1.0

    COLORS = {
        "Blade_Chassis": (0.12, 0.14, 0.17, 1),      # Châssis acier galvanisé sombre
        "Blade_Rails": (0.55, 0.60, 0.66, 1),        # Rails télescopiques acier brossé
        "Blade_PCB": (0.04, 0.28, 0.12, 1),          # Carte mère verte industrielle
        "Blade_Heatsink": (0.75, 0.42, 0.18, 1),     # Cuivre / Aluminium radiateurs CPU
        "Blade_RAM": (0.15, 0.18, 0.22, 1),          # Barrettes mémoires DDR5 sombres
        "Blade_Fans": (0.08, 0.08, 0.09, 1),         # Ventilateurs d'extraction noirs
        "Blade_LED_Green": (0.12, 0.95, 0.35, 1),    # Diodes d'état saines
        "Blade_LED_Cyan": (0.05, 0.85, 0.98, 1),     # Diodes réseau / bus
        "Blade_LED_Amber": (0.98, 0.55, 0.08, 1),    # Diodes activité disque
    }

    materials = {}
    for name, col in COLORS.items():
        mat = bpy.data.materials.new(name)
        mat.use_nodes = True
        bsdf = mat.node_tree.nodes.get("Principled BSDF")
        bsdf.inputs["Base Color"].default_value = col
        if "LED" in name:
            bsdf.inputs["Emission Color"].default_value = col
            bsdf.inputs["Emission Strength"].default_value = 3.0
        elif "Heatsink" in name or "Rails" in name:
            bsdf.inputs["Metallic"].default_value = 0.85
            bsdf.inputs["Roughness"].default_value = 0.30
        elif "Chassis" in name:
            bsdf.inputs["Metallic"].default_value = 0.65
            bsdf.inputs["Roughness"].default_value = 0.45
        materials[name] = mat

    mesh_groups = {key: [] for key in COLORS}

    def box(name, loc, size, mat_name):
        bpy.ops.mesh.primitive_cube_add(size=1, location=loc)
        obj = bpy.context.object
        obj.name = name
        obj.dimensions = size
        bpy.ops.object.transform_apply(location=False, rotation=False, scale=True)
        obj.data.materials.append(materials[mat_name])
        mesh_groups[mat_name].append(obj)
        return obj

    def cyl(name, loc, r, h, mat_name, rot=(0,0,0)):
        bpy.ops.mesh.primitive_cylinder_add(radius=r, depth=h, location=loc, rotation=rot)
        obj = bpy.context.object
        obj.name = name
        bpy.ops.object.transform_apply(location=False, rotation=False, scale=True)
        obj.data.materials.append(materials[mat_name])
        mesh_groups[mat_name].append(obj)
        return obj

    # Dimensions : Largeur 0.44m (19"), Hauteur 0.086m (2U), Profondeur 0.72m
    # Origine du serveur au centre arrière (0, 0, 0), s'étend vers Y négatif (façade à y = -0.72)
    # ── Fond et parois du châssis ──
    box("Chassis_Bottom", (0, -0.36, 0.005), (0.44, 0.72, 0.008), "Blade_Chassis")
    box("Chassis_Wall_Left", (-0.218, -0.36, 0.043), (0.006, 0.72, 0.080), "Blade_Chassis")
    box("Chassis_Wall_Right", (0.218, -0.36, 0.043), (0.006, 0.72, 0.080), "Blade_Chassis")
    box("Chassis_Back", (0, -0.003, 0.043), (0.44, 0.006, 0.080), "Blade_Chassis")

    # ── Glissières télescopiques sur les côtés ──
    box("Rail_L", (-0.224, -0.36, 0.043), (0.008, 0.70, 0.035), "Blade_Rails")
    box("Rail_R", (0.224, -0.36, 0.043), (0.008, 0.70, 0.035), "Blade_Rails")

    # ── Façade avant 2U avec oreilles de fixation et poignées ──
    box("Front_Faceplate", (0, -0.723, 0.043), (0.48, 0.012, 0.088), "Blade_Chassis")
    # Poignées de préhension latérales en acier brossé
    box("Handle_Left", (-0.232, -0.745, 0.043), (0.016, 0.032, 0.070), "Blade_Rails")
    box("Handle_Right", (0.232, -0.745, 0.043), (0.016, 0.032, 0.070), "Blade_Rails")

    # 8 baies de disques SSD NVMe hot-swap en façade
    for d in range(8):
        dx = -0.16 + (d % 4) * 0.082
        dz = 0.024 if d < 4 else 0.062
        box(f"Drive_Tray_{d}", (dx, -0.730, dz), (0.076, 0.008, 0.032), "Blade_Chassis")
        # LED d'activité NVMe
        cyl(f"LED_Drive_{d}", (dx + 0.028, -0.733, dz), 0.003, 0.004, "Blade_LED_Green", rot=(math.pi/2, 0, 0))
        cyl(f"LED_Act_{d}", (dx + 0.020, -0.733, dz), 0.002, 0.004, "Blade_LED_Amber", rot=(math.pi/2, 0, 0))

    # ── Carte mère industrielle PCB ──
    box("Motherboard_PCB", (0, -0.32, 0.012), (0.42, 0.58, 0.004), "Blade_PCB")

    # ── Deux sockets CPU imposants avec radiateurs cuivre/aluminium ──
    for cpu, cy in enumerate([-0.30, -0.44]):
        # Base radiateur
        box(f"CPU_Heatsink_Base_{cpu}", (0, cy, 0.045), (0.13, 0.11, 0.065), "Blade_Heatsink")
        # Ailettes de refroidissement
        for a in range(7):
            ay = cy - 0.045 + a * 0.015
            box(f"Heatsink_Fin_{cpu}_{a}", (0, ay, 0.046), (0.128, 0.003, 0.063), "Blade_Heatsink")

    # ── 16 barrettes de RAM DDR5 réparties autour des CPUs ──
    for side in [-1, 1]:
        for slot in range(8):
            ry = -0.22 - slot * 0.038
            rx = side * 0.135
            box(f"RAM_Stick_{side}_{slot}", (rx, ry, 0.032), (0.012, 0.030, 0.038), "Blade_RAM")
            # Petit indicateur cyan sur chaque barrette
            cyl(f"LED_RAM_{side}_{slot}", (rx, ry, 0.052), 0.002, 0.003, "Blade_LED_Cyan")

    # ── 4 modules de ventilateurs d'extraction hot-swap à l'arrière des disques ──
    for f in range(4):
        fx = -0.15 + f * 0.10
        box(f"Fan_Module_{f}", (fx, -0.66, 0.045), (0.08, 0.06, 0.065), "Blade_Fans")
        cyl(f"Fan_Rotor_{f}", (fx, -0.66, 0.045), 0.028, 0.055, "Blade_Rails", rot=(math.pi/2, 0, 0))

    # ── Deux alimentations redondantes à l'arrière (PSU 1 & PSU 2) ──
    box("PSU_1", (-0.14, -0.06, 0.042), (0.10, 0.11, 0.065), "Blade_Chassis")
    box("PSU_2", (0.14, -0.06, 0.042), (0.10, 0.11, 0.065), "Blade_Chassis")
    cyl("PSU_Handle_1", (-0.14, 0.002, 0.042), 0.006, 0.04, "Blade_Rails", rot=(0, math.pi/2, 0))
    cyl("PSU_Handle_2", (0.14, 0.002, 0.042), 0.006, 0.04, "Blade_Rails", rot=(0, math.pi/2, 0))

    # ── Fusion par matériau pour minimiser les draw calls ──
    for mat_name, objs in mesh_groups.items():
        if not objs: continue
        bpy.ops.object.select_all(action="DESELECT")
        for o in objs: o.select_set(True)
        bpy.context.view_layer.objects.active = objs[0]
        bpy.ops.object.join()
        joined = bpy.context.object
        joined.name = f"Blade_{mat_name}"

    export_path = MODELS_DIR / "Server_Blade_Detailed.fbx"
    bpy.ops.export_scene.fbx(
        filepath=str(export_path),
        use_selection=False,
        apply_unit_scale=True,
        apply_scale_options="FBX_SCALE_ALL",
        axis_forward="-Z",
        axis_up="Y",
        bake_space_transform=True
    )
    print(f"[Blender] Serveur Blade 2U exporté -> {export_path}")

# ══════════════════════════════════════════════════════════════════
# 2. PORTE DE BAIE SERVEUR OUVRABLE AVEC POIGNÉE INDUSTRIELLE
# ══════════════════════════════════════════════════════════════════

def build_rack_door():
    bpy.ops.wm.read_factory_settings(use_empty=True)
    scene = bpy.context.scene
    scene.unit_settings.system = "METRIC"
    scene.unit_settings.scale_length = 1.0

    COLORS = {
        "Door_Frame": (0.08, 0.09, 0.11, 1),       # Cadre noir mat
        "Door_Handle": (0.55, 0.60, 0.66, 1),      # Poignée quart de tour acier brossé
        "Door_Mesh": (0.15, 0.20, 0.26, 0.65),     # Grillage perforé ventilé
        "Door_Badge": (0.05, 0.85, 0.98, 1),      # Écusson cyan de baie
    }

    materials = {}
    for name, col in COLORS.items():
        mat = bpy.data.materials.new(name)
        mat.use_nodes = True
        bsdf = mat.node_tree.nodes.get("Principled BSDF")
        bsdf.inputs["Base Color"].default_value = col
        if "Badge" in name:
            bsdf.inputs["Emission Color"].default_value = col
            bsdf.inputs["Emission Strength"].default_value = 2.5
        elif "Handle" in name:
            bsdf.inputs["Metallic"].default_value = 0.85
            bsdf.inputs["Roughness"].default_value = 0.25
        materials[name] = mat

    mesh_groups = {key: [] for key in COLORS}

    def box(name, loc, size, mat_name):
        bpy.ops.mesh.primitive_cube_add(size=1, location=loc)
        obj = bpy.context.object
        obj.name = name
        obj.dimensions = size
        bpy.ops.object.transform_apply(location=False, rotation=False, scale=True)
        obj.data.materials.append(materials[mat_name])
        mesh_groups[mat_name].append(obj)
        return obj

    def cyl(name, loc, r, h, mat_name, rot=(0,0,0)):
        bpy.ops.mesh.primitive_cylinder_add(radius=r, depth=h, location=loc, rotation=rot)
        obj = bpy.context.object
        obj.name = name
        bpy.ops.object.transform_apply(location=False, rotation=False, scale=True)
        obj.data.materials.append(materials[mat_name])
        mesh_groups[mat_name].append(obj)
        return obj

    # Dimensions : Largeur 0.94m, Hauteur 2.02m, Profondeur 0.04m
    # Pivot de rotation placé sur les charnières à gauche (x = -0.47)
    # ── Cadre extérieur de la porte ──
    box("Frame_L", (-0.45, 0, 1.01), (0.04, 0.04, 2.02), "Door_Frame")
    box("Frame_R", (0.45, 0, 1.01), (0.04, 0.04, 2.02), "Door_Frame")
    box("Frame_Top", (0, 0, 2.00), (0.94, 0.04, 0.04), "Door_Frame")
    box("Frame_Bottom", (0, 0, 0.02), (0.94, 0.04, 0.04), "Door_Frame")

    # ── Panneau grillagé perforé central ──
    box("Perforated_Mesh", (0, 0, 1.01), (0.86, 0.008, 1.92), "Door_Mesh")

    # ── Poignée quart de tour industrielle pivotante à droite ──
    box("Lock_Housing", (0.42, 0.025, 1.00), (0.05, 0.03, 0.22), "Door_Frame")
    cyl("Handle_Lever", (0.42, 0.045, 1.00), 0.012, 0.16, "Door_Handle")
    cyl("Keyhole", (0.42, 0.046, 1.08), 0.008, 0.01, "Door_Frame", rot=(math.pi/2, 0, 0))

    # ── Écusson d'identification de baie ──
    box("Badge_Plate", (0, 0.022, 1.88), (0.24, 0.006, 0.08), "Door_Frame")
    box("Badge_Led", (0, 0.026, 1.88), (0.20, 0.003, 0.04), "Door_Badge")

    # ── Charnières mécaniques à gauche ──
    for hz in [0.25, 1.00, 1.75]:
        cyl(f"Hinge_{hz}", (-0.47, 0.01, hz), 0.012, 0.06, "Door_Handle")

    # Fusion par matériau
    for mat_name, objs in mesh_groups.items():
        if not objs: continue
        bpy.ops.object.select_all(action="DESELECT")
        for o in objs: o.select_set(True)
        bpy.context.view_layer.objects.active = objs[0]
        bpy.ops.object.join()
        joined = bpy.context.object
        joined.name = f"RackDoor_{mat_name}"

    export_path = MODELS_DIR / "Server_Rack_Door.fbx"
    bpy.ops.export_scene.fbx(
        filepath=str(export_path),
        use_selection=False,
        apply_unit_scale=True,
        apply_scale_options="FBX_SCALE_ALL",
        axis_forward="-Z",
        axis_up="Y",
        bake_space_transform=True
    )
    print(f"[Blender] Porte de baie serveur exportée -> {export_path}")

if __name__ == "__main__":
    build_server_blade()
    build_rack_door()
