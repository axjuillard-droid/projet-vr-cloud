"""Générateur 3D Blender du Sas de Sécurité Militaire / Industriel et du Tunnel Fibre Optique
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
# 1. SAS DE SÉCURITÉ MILITAIRE / HAUTE SÉCURITÉ DU DATA CENTER
# ══════════════════════════════════════════════════════════════════

def build_datacenter_airlock():
    bpy.ops.wm.read_factory_settings(use_empty=True)
    scene = bpy.context.scene
    scene.unit_settings.system = "METRIC"
    scene.unit_settings.scale_length = 1.0

    COLORS = {
        "Airlock_Armor": (0.08, 0.09, 0.11, 1),       # Acier sombre blindé
        "Airlock_Steel": (0.35, 0.38, 0.42, 1),       # Acier brossé structural
        "Airlock_Hazard": (0.92, 0.72, 0.08, 1),      # Jaune de sécurité industriel
        "Airlock_Glass": (0.05, 0.15, 0.22, 1),       # Verre pare-balles fumé bleuté
        "Airlock_Beacon_Amber": (0.98, 0.52, 0.05, 1),# Gyrophare d'alerte émissif
        "Airlock_Led_Green": (0.10, 0.95, 0.25, 1),   # Badgeuse accès validé
        "Airlock_Led_Red": (0.95, 0.12, 0.10, 1),     # Badgeuse refus/verrou
    }

    materials = {}
    for name, col in COLORS.items():
        mat = bpy.data.materials.new(name)
        mat.use_nodes = True
        bsdf = mat.node_tree.nodes.get("Principled BSDF")
        bsdf.inputs["Base Color"].default_value = col
        if "Armor" in name or "Steel" in name:
            bsdf.inputs["Metallic"].default_value = 0.8
            bsdf.inputs["Roughness"].default_value = 0.35
        elif "Hazard" in name:
            bsdf.inputs["Roughness"].default_value = 0.5
        elif "Glass" in name:
            bsdf.inputs["Roughness"].default_value = 0.1
        elif "Beacon" in name or "Led" in name:
            bsdf.inputs["Emission Color"].default_value = col
            bsdf.inputs["Emission Strength"].default_value = 2.5
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

    # ── Cadre principal du sas (Hauteur 2.60m, Largeur 2.40m, Profondeur 0.50m) ──
    # Montant gauche et droit
    box("Frame_Left", (-1.15, 0, 1.30), (0.30, 0.50, 2.60), "Airlock_Armor")
    box("Frame_Right", (1.15, 0, 1.30), (0.30, 0.50, 2.60), "Airlock_Armor")
    # Linteau supérieur
    box("Frame_Top", (0, 0, 2.50), (2.60, 0.50, 0.25), "Airlock_Armor")
    # Seuil en biseau
    box("Frame_Threshold", (0, 0, 0.04), (2.10, 0.60, 0.08), "Airlock_Steel")

    # ── Bandes diagonales d'avertissement de sécurité (Jaune / Noir) ──
    box("Hazard_Stripe_Top", (0, -0.26, 2.50), (2.00, 0.02, 0.14), "Airlock_Hazard")
    box("Hazard_Pillar_L", (-1.05, -0.26, 1.30), (0.08, 0.02, 2.20), "Airlock_Hazard")
    box("Hazard_Pillar_R", (1.05, -0.26, 1.30), (0.08, 0.02, 2.20), "Airlock_Hazard")

    # ── Portes blindées doubles coulissantes (Épaisseur 0.12m) ──
    # Battant gauche (x de -0.95 à -0.02)
    box("Door_Wing_Left", (-0.48, 0, 1.20), (0.92, 0.12, 2.30), "Airlock_Steel")
    # Hublot blindé gauche
    box("Door_Window_Left", (-0.48, 0, 1.55), (0.35, 0.14, 0.60), "Airlock_Glass")
    # Battant droit (x de 0.02 à 0.95)
    box("Door_Wing_Right", (0.48, 0, 1.20), (0.92, 0.12, 2.30), "Airlock_Steel")
    # Hublot blindé droit
    box("Door_Window_Right", (0.48, 0, 1.55), (0.35, 0.14, 0.60), "Airlock_Glass")

    # Barres de verrouillage mécanique centrales
    cyl("Lock_Bolt_Upper", (0, -0.07, 1.80), 0.025, 0.40, "Airlock_Armor", rot=(0, math.pi/2, 0))
    cyl("Lock_Bolt_Lower", (0, -0.07, 0.60), 0.025, 0.40, "Airlock_Armor", rot=(0, math.pi/2, 0))

    # Poignées d'urgence
    box("Handle_Left", (-0.12, -0.09, 1.15), (0.04, 0.08, 0.28), "Airlock_Hazard")
    box("Handle_Right", (0.12, -0.09, 1.15), (0.04, 0.08, 0.28), "Airlock_Hazard")

    # ── Badgeuse biométrique / RFID murale à droite ──
    box("Badge_Console", (1.20, -0.26, 1.25), (0.18, 0.10, 0.32), "Airlock_Steel")
    box("Badge_Scanner_Glass", (1.20, -0.31, 1.25), (0.12, 0.02, 0.12), "Airlock_Glass")
    # Voyants de statut RFID (Vert = autorise, Rouge = verrous)
    cyl("Led_Green", (1.23, -0.32, 1.35), 0.012, 0.02, "Airlock_Led_Green", rot=(math.pi/2, 0, 0))
    cyl("Led_Red", (1.17, -0.32, 1.35), 0.012, 0.02, "Airlock_Led_Red", rot=(math.pi/2, 0, 0))

    # ── Gyrophare d'alerte / Balise industrielle sur le linteau ──
    cyl("Beacon_Base", (0, -0.15, 2.66), 0.09, 0.08, "Airlock_Steel")
    cyl("Beacon_Light", (0, -0.15, 2.76), 0.07, 0.12, "Airlock_Beacon_Amber")

    # ── Caméra dôme de surveillance militaire ──
    cyl("Camera_Mount", (-0.85, -0.28, 2.55), 0.06, 0.05, "Airlock_Armor")
    cyl("Camera_Dome", (-0.85, -0.31, 2.50), 0.05, 0.06, "Airlock_Glass", rot=(math.pi/4, 0, 0))

    # ── Fusion par matériau pour minimiser les draw calls ──
    final_objs = []
    for mat_name, objs in mesh_groups.items():
        if not objs: continue
        bpy.ops.object.select_all(action="DESELECT")
        for o in objs: o.select_set(True)
        bpy.context.view_layer.objects.active = objs[0]
        bpy.ops.object.join()
        joined = bpy.context.object
        joined.name = f"Airlock_{mat_name}"
        final_objs.append(joined)

    export_path = MODELS_DIR / "DataCenter_Airlock.fbx"
    bpy.ops.export_scene.fbx(
        filepath=str(export_path),
        use_selection=False,
        apply_unit_scale=True,
        apply_scale_options="FBX_SCALE_ALL",
        axis_forward="-Z",
        axis_up="Y",
        bake_space_transform=True
    )
    print(f"[Blender] Sas du Data Center exporté -> {export_path}")

# ══════════════════════════════════════════════════════════════════
# 2. TUNNEL DE DONNÉES / CÂBLE FIBRE OPTIQUE IMMERSIF
# ══════════════════════════════════════════════════════════════════

def build_data_fiber_tunnel():
    bpy.ops.wm.read_factory_settings(use_empty=True)
    scene = bpy.context.scene
    scene.unit_settings.system = "METRIC"
    scene.unit_settings.scale_length = 1.0

    COLORS = {
        "Tunnel_Outer": (0.05, 0.06, 0.09, 1),       # Gaine isolante sombre
        "Tunnel_Ring_Metal": (0.40, 0.44, 0.50, 1),   # Anneaux de renfort métallo-céramique
        "Tunnel_Fiber_Core": (0.05, 0.85, 0.98, 1),   # Brins de fibre optique cyan ultra-émissifs
        "Tunnel_Pulse_Magenta": (0.95, 0.15, 0.75, 1),# Paquets lumineux pulsés magenta
    }

    materials = {}
    for name, col in COLORS.items():
        mat = bpy.data.materials.new(name)
        mat.use_nodes = True
        bsdf = mat.node_tree.nodes.get("Principled BSDF")
        bsdf.inputs["Base Color"].default_value = col
        if "Core" in name or "Pulse" in name:
            bsdf.inputs["Emission Color"].default_value = col
            bsdf.inputs["Emission Strength"].default_value = 3.5
        elif "Metal" in name:
            bsdf.inputs["Metallic"].default_value = 0.8
            bsdf.inputs["Roughness"].default_value = 0.3
        materials[name] = mat

    mesh_groups = {key: [] for key in COLORS}

    # Le tunnel mesure 20m de long suivant l'axe Z (de z=0 à z=20)
    # Diamètre intérieur : 1.8m (le joueur passe à l'intérieur en vue FPS)
    length = 20.0
    segments = 20
    radius = 1.20

    # Conduit externe octogonal sans bouchons d'extrémité
    bpy.ops.mesh.primitive_cylinder_add(vertices=12, radius=radius, depth=length, location=(0, 0, length/2), end_fill_type='NOTHING')
    outer = bpy.context.object
    outer.name = "Tunnel_Conduit"
    # Inverser les normales pour que l'intérieur soit visible
    bpy.ops.object.mode_set(mode='EDIT')
    bpy.ops.mesh.select_all(action='SELECT')
    bpy.ops.mesh.flip_normals()
    bpy.ops.object.mode_set(mode='OBJECT')
    outer.data.materials.append(materials["Tunnel_Outer"])
    mesh_groups["Tunnel_Outer"].append(outer)

    # Anneaux de synchronisation lumineux tous les 2 mètres
    for i in range(int(length / 2.0)):
        z_pos = (i + 0.5) * 2.0
        # Anneau métallique
        bpy.ops.mesh.primitive_cylinder_add(vertices=12, radius=radius - 0.05, depth=0.15, location=(0, 0, z_pos), end_fill_type='NOTHING')
        ring = bpy.context.object
        ring.data.materials.append(materials["Tunnel_Ring_Metal"])
        mesh_groups["Tunnel_Ring_Metal"].append(ring)

    # Brins de fibre optique longitudinaux (4 guides d'onde lumineux le long du tunnel)
    angles = [math.pi/4, 3*math.pi/4, 5*math.pi/4, 7*math.pi/4]
    for angle in angles:
        fx = (radius - 0.12) * math.cos(angle)
        fy = (radius - 0.12) * math.sin(angle)
        bpy.ops.mesh.primitive_cylinder_add(vertices=6, radius=0.035, depth=length, location=(fx, fy, length/2))
        fiber = bpy.context.object
        fiber.data.materials.append(materials["Tunnel_Fiber_Core"])
        mesh_groups["Tunnel_Fiber_Core"].append(fiber)

    # 4 impulsions / paquets de données sphériques lumineux dispersés
    packet_z = [3.0, 7.5, 12.0, 16.5]
    for i, pz in enumerate(packet_z):
        angle = angles[i % len(angles)]
        px = (radius - 0.12) * math.cos(angle)
        py = (radius - 0.12) * math.sin(angle)
        bpy.ops.mesh.primitive_ico_sphere_add(subdivisions=2, radius=0.08, location=(px, py, pz))
        pkt = bpy.context.object
        pkt.data.materials.append(materials["Tunnel_Pulse_Magenta"])
        mesh_groups["Tunnel_Pulse_Magenta"].append(pkt)

    # Fusion par matériau
    for mat_name, objs in mesh_groups.items():
        if not objs: continue
        bpy.ops.object.select_all(action="DESELECT")
        for o in objs: o.select_set(True)
        bpy.context.view_layer.objects.active = objs[0]
        bpy.ops.object.join()
        joined = bpy.context.object
        joined.name = f"Fiber_{mat_name}"

    export_path = MODELS_DIR / "Data_Fiber_Tunnel.fbx"
    bpy.ops.export_scene.fbx(
        filepath=str(export_path),
        use_selection=False,
        apply_unit_scale=True,
        apply_scale_options="FBX_SCALE_ALL",
        axis_forward="-Z",
        axis_up="Y",
        bake_space_transform=True
    )
    print(f"[Blender] Tunnel fibre optique exporté -> {export_path}")

if __name__ == "__main__":
    build_datacenter_airlock()
    build_data_fiber_tunnel()
