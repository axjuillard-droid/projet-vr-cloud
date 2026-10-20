"""Génération procédurale de mobilier réaliste pour la Chambre étudiante.
Modèles originaux optimisés pour Unity URP / Meta Quest (low/mid poly, fusionnés par matière).
Exécution : blender --background --python tools/art/generate_room_furniture.py
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
    scene = bpy.context.scene
    scene.unit_settings.system = "METRIC"
    scene.unit_settings.scale_length = 1.0

def make_material(name, color, metallic=0.0, roughness=0.5, emission=0.0, emission_color=None):
    mat = bpy.data.materials.new(name)
    mat.diffuse_color = color
    mat.use_nodes = True
    bsdf = mat.node_tree.nodes.get("Principled BSDF")
    if bsdf:
        bsdf.inputs["Base Color"].default_value = color
        bsdf.inputs["Metallic"].default_value = metallic
        bsdf.inputs["Roughness"].default_value = roughness
        if emission > 0 and emission_color:
            # Blender 4.0+ Principled BSDF emission inputs
            if "Emission Color" in bsdf.inputs:
                bsdf.inputs["Emission Color"].default_value = emission_color
                bsdf.inputs["Emission Strength"].default_value = emission
            elif "Emission" in bsdf.inputs:
                bsdf.inputs["Emission"].default_value = emission_color
    return mat

def create_box(name, location, dimensions, material, bevel=0.0):
    bpy.ops.mesh.primitive_cube_add(size=1, location=location)
    obj = bpy.context.object
    obj.name = name
    obj.dimensions = dimensions
    bpy.ops.object.transform_apply(location=False, rotation=False, scale=True)
    obj.data.materials.append(material)
    if bevel > 0:
        mod = obj.modifiers.new("Bevel", "BEVEL")
        mod.width = bevel
        mod.segments = 1
        bpy.ops.object.modifier_apply(modifier=mod.name)
    return obj

def create_cylinder(name, location, radius, depth, material, vertices=16):
    bpy.ops.mesh.primitive_cylinder_add(vertices=vertices, radius=radius, depth=depth, location=location)
    obj = bpy.context.object
    obj.name = name
    bpy.ops.object.transform_apply(location=False, rotation=False, scale=True)
    obj.data.materials.append(material)
    return obj

def merge_by_material(prefix):
    objs = [o for o in bpy.context.scene.objects if o.type == 'MESH']
    mat_map = {}
    for o in objs:
        m = o.data.materials[0].name if o.data.materials else "Default"
        mat_map.setdefault(m, []).append(o)
    
    merged_objs = []
    for mat_name, group in mat_map.items():
        if not group:
            continue
        bpy.ops.object.select_all(action='DESELECT')
        for o in group:
            o.select_set(True)
        bpy.context.view_layer.objects.active = group[0]
        if len(group) > 1:
            bpy.ops.object.join()
        res = bpy.context.view_layer.objects.active
        res.name = f"{prefix}_{mat_name}"
        merged_objs.append(res)
    return merged_objs

# ══════════════════════════════════════════════════════════════════
# 1. MODÈLE DU LIT ÉTUDIANT (Bed_Student.fbx)
# ══════════════════════════════════════════════════════════════════
def build_bed():
    reset_scene()
    
    mat_wood = make_material("Bed_Wood", (0.35, 0.22, 0.14, 1.0), roughness=0.6)
    mat_metal = make_material("Bed_Metal", (0.10, 0.10, 0.12, 1.0), metallic=0.7, roughness=0.35)
    mat_sheet = make_material("Bed_Sheet", (0.85, 0.86, 0.88, 1.0), roughness=0.8)
    mat_duvet = make_material("Bed_Duvet", (0.16, 0.28, 0.42, 1.0), roughness=0.75)  # Bleu pétrole
    mat_pillow = make_material("Bed_Pillow", (0.92, 0.92, 0.90, 1.0), roughness=0.9)
    
    # Cadre de lit (2.05m x 1.0m x 0.35m)
    # 4 pieds
    for x in (-0.46, 0.46):
        for y in (-0.98, 0.98):
            create_box("Pied", (x, y, 0.15), (0.06, 0.06, 0.30), mat_wood)
    
    # Longerons et traverses
    create_box("Longeron_G", (-0.46, 0, 0.26), (0.04, 2.00, 0.12), mat_wood)
    create_box("Longeron_D", (0.46, 0, 0.26), (0.04, 2.00, 0.12), mat_wood)
    create_box("Traverse_Pieds", (0, -0.98, 0.26), (0.96, 0.04, 0.12), mat_wood)
    
    # Tête de lit (+Y)
    create_box("Tete_Lit", (0, 0.98, 0.52), (0.98, 0.05, 0.65), mat_wood, bevel=0.01)
    
    # Sommier intérieur
    create_box("Sommier", (0, 0, 0.22), (0.90, 1.94, 0.04), mat_metal)
    
    # Matelas (1.90m x 0.90m x 0.20m)
    create_box("Matelas", (0, 0, 0.35), (0.90, 1.92, 0.20), mat_sheet, bevel=0.02)
    
    # Couette (couvre les 2/3 inférieurs du lit, légèrement débordante)
    create_box("Couette_Principale", (0, -0.25, 0.46), (0.94, 1.40, 0.06), mat_duvet, bevel=0.03)
    # Rabat de couette
    create_box("Couette_Rabat", (0, 0.46, 0.47), (0.92, 0.18, 0.04), mat_sheet, bevel=0.02)
    
    # Oreiller (au fond, vers tête de lit)
    create_box("Oreiller", (0, 0.72, 0.49), (0.65, 0.40, 0.12), mat_pillow, bevel=0.04)

    merge_by_material("Bed")
    out_path = MODELS_DIR / "Bed_Student.fbx"
    bpy.ops.export_scene.fbx(filepath=str(out_path), use_selection=False, apply_scale_options='FBX_SCALE_ALL')
    print("Bed_Student.fbx généré avec succès !")

# ══════════════════════════════════════════════════════════════════
# 2. MODÈLE DU BUREAU & SETUP PC GAMER (Desk_Setup.fbx)
# ══════════════════════════════════════════════════════════════════
def build_desk_setup():
    reset_scene()
    
    mat_wood = make_material("Desk_Wood", (0.40, 0.26, 0.16, 1.0), roughness=0.45)
    mat_metal = make_material("Desk_Metal", (0.08, 0.08, 0.09, 1.0), metallic=0.8, roughness=0.3)
    mat_plastic = make_material("PC_Plastic", (0.12, 0.12, 0.14, 1.0), roughness=0.4)
    mat_mat = make_material("Desk_Mousepad", (0.15, 0.16, 0.20, 1.0), roughness=0.9)
    mat_led = make_material("PC_RGB_Cyan", (0.05, 0.85, 0.95, 1.0), emission=2.5, emission_color=(0.05, 0.85, 0.95, 1.0))
    mat_led_red = make_material("PC_RGB_Red", (0.95, 0.20, 0.10, 1.0), emission=2.0, emission_color=(0.95, 0.20, 0.10, 1.0))
    mat_mug = make_material("Desk_Ceramic", (0.92, 0.92, 0.92, 1.0), roughness=0.2)
    
    # --- Plateau Bureau (1.60m x 0.80m x 0.04m) ---
    create_box("Plateau", (0, 0, 0.73), (1.60, 0.80, 0.04), mat_wood, bevel=0.008)
    
    # Pieds en "U" métalliques modernes
    for x in (-0.72, 0.72):
        create_box(f"Pied_V1_{x}", (x, -0.34, 0.355), (0.05, 0.05, 0.71), mat_metal)
        create_box(f"Pied_V2_{x}", (x, 0.34, 0.355), (0.05, 0.05, 0.71), mat_metal)
        create_box(f"Pied_Bas_{x}", (x, 0, 0.02), (0.05, 0.74, 0.04), mat_metal)
        create_box(f"Pied_Haut_{x}", (x, 0, 0.70), (0.05, 0.74, 0.02), mat_metal)
    
    # Traverse arrière de renfort
    create_box("Traverse_Fond", (0, 0.32, 0.45), (1.39, 0.03, 0.05), mat_metal)
    
    # --- Sous-main / Tapis de souris XXL (0.80m x 0.40m) ---
    create_box("Mousepad", (0, -0.10, 0.752), (0.80, 0.36, 0.004), mat_mat)
    
    # --- Clavier avec touches et châssis ---
    create_box("Clavier_Base", (-0.05, -0.12, 0.760), (0.44, 0.14, 0.012), mat_plastic, bevel=0.003)
    create_box("Clavier_Touches", (-0.05, -0.12, 0.768), (0.42, 0.12, 0.006), mat_led)
    
    # --- Souris ergonomique ---
    create_box("Souris", (0.28, -0.12, 0.762), (0.07, 0.12, 0.025), mat_plastic, bevel=0.008)
    create_box("Souris_LED", (0.28, -0.15, 0.772), (0.02, 0.03, 0.004), mat_led)
    
    # --- Tour PC Gamer posée à droite du plateau (ou au sol, ici sur plateau côté +X) ---
    pc_x, pc_y, pc_z = 0.62, 0.08, 0.75 + 0.24
    create_box("PC_Chassis", (pc_x, pc_y, pc_z), (0.22, 0.45, 0.44), mat_plastic, bevel=0.005)
    # Vitre latérale gauche illuminée
    create_box("PC_Vitre", (pc_x - 0.105, pc_y, pc_z), (0.008, 0.40, 0.38), mat_metal)
    # Bandeau LED RGB façade avant
    create_box("PC_LED_Front", (pc_x, pc_y - 0.226, pc_z), (0.015, 0.006, 0.36), mat_led)
    # Ventilateur RGB intérieur visible
    create_cylinder("PC_Fan_RGB", (pc_x - 0.04, pc_y + 0.16, pc_z + 0.08), radius=0.055, depth=0.02, material=mat_led_red)
    
    # --- Tasse à café posée sur le bureau ---
    create_cylinder("Tasse_Corps", (-0.52, -0.05, 0.79), radius=0.04, depth=0.08, material=mat_mug)
    create_box("Tasse_Anse", (-0.57, -0.05, 0.79), (0.025, 0.02, 0.05), mat_mug)
    
    # --- Chaise de bureau ergonomique ---
    # Pied en étoile à 5 branches
    chair_x, chair_y = 0.0, -0.58
    create_cylinder("Chaise_Pied_Centre", (chair_x, chair_y, 0.10), radius=0.04, depth=0.15, material=mat_metal)
    for i in range(5):
        angle = i * (2 * math.pi / 5)
        bx = chair_x + math.cos(angle) * 0.24
        by = chair_y + math.sin(angle) * 0.24
        create_box(f"Chaise_Branche_{i}", (bx, by, 0.06), (0.04, 0.04, 0.03), mat_metal)
        # Roulette
        create_cylinder(f"Chaise_Roulette_{i}", (bx, by, 0.03), radius=0.025, depth=0.02, material=mat_plastic)
    # Vérin central
    create_cylinder("Chaise_Verin", (chair_x, chair_y, 0.28), radius=0.025, depth=0.22, material=mat_metal)
    # Assise galbée
    create_box("Chaise_Assise", (chair_x, chair_y, 0.44), (0.48, 0.48, 0.08), mat_plastic, bevel=0.02)
    # Dossier ergonomique
    create_box("Chaise_Dossier", (chair_x, chair_y - 0.22, 0.72), (0.44, 0.06, 0.52), mat_plastic, bevel=0.015)
    # Accoudoirs
    for s in (-1, 1):
        ax = chair_x + s * 0.26
        create_box(f"Accoudoir_Tige_{s}", (ax, chair_y - 0.05, 0.52), (0.03, 0.04, 0.16), mat_metal)
        create_box(f"Accoudoir_Haut_{s}", (ax, chair_y - 0.05, 0.60), (0.06, 0.22, 0.025), mat_plastic, bevel=0.005)

    merge_by_material("DeskSetup")
    out_path = MODELS_DIR / "Desk_Setup.fbx"
    bpy.ops.export_scene.fbx(filepath=str(out_path), use_selection=False, apply_scale_options='FBX_SCALE_ALL')
    print("Desk_Setup.fbx généré avec succès !")

# ══════════════════════════════════════════════════════════════════
# 3. MODÈLE DU DÉCOR DE CHAMBRE (Room_Decor.fbx)
# ══════════════════════════════════════════════════════════════════
def build_room_decor():
    reset_scene()
    
    mat_wood = make_material("Decor_Wood", (0.48, 0.32, 0.20, 1.0), roughness=0.5)
    mat_metal = make_material("Decor_Metal", (0.12, 0.12, 0.14, 1.0), metallic=0.7, roughness=0.3)
    mat_carpet = make_material("Decor_Carpet", (0.75, 0.72, 0.68, 1.0), roughness=0.95)
    mat_plant = make_material("Decor_Plant", (0.15, 0.55, 0.22, 1.0), roughness=0.6)
    mat_pot = make_material("Decor_Pot", (0.70, 0.38, 0.25, 1.0), roughness=0.7)
    mat_book1 = make_material("Decor_Book_Blue", (0.15, 0.35, 0.65, 1.0), roughness=0.6)
    mat_book2 = make_material("Decor_Book_Red", (0.75, 0.20, 0.18, 1.0), roughness=0.6)
    mat_book3 = make_material("Decor_Book_Yellow", (0.85, 0.65, 0.15, 1.0), roughness=0.6)
    mat_poster = make_material("Decor_Poster_Frame", (0.05, 0.05, 0.05, 1.0), roughness=0.3)
    
    # --- Grand tapis de sol (2.20m x 1.60m x 0.008m) ---
    create_box("Tapis_Sol", (0, 0, 0.004), (2.20, 1.60, 0.008), mat_carpet)
    
    # --- Étagère murale suspendue (1.20m x 0.22m x 0.03m) ---
    create_box("Etagere_Planche", (0, 0, 1.60), (1.20, 0.22, 0.03), mat_wood, bevel=0.005)
    for x in (-0.50, 0.50):
        create_box(f"Etagere_Support_{x}", (x, 0.08, 1.70), (0.03, 0.04, 0.24), mat_metal)
    
    # Livres sur l'étagère
    create_box("Livre_1", (-0.42, 0.01, 1.72), (0.04, 0.16, 0.22), mat_book1)
    create_box("Livre_2", (-0.37, 0.01, 1.71), (0.035, 0.15, 0.20), mat_book2)
    create_box("Livre_3", (-0.32, 0.01, 1.73), (0.05, 0.17, 0.23), mat_book3)
    # Deux livres empilés à plat
    create_box("Livre_Plat_1", (-0.18, 0.01, 1.63), (0.16, 0.20, 0.03), mat_book2)
    create_box("Livre_Plat_2", (-0.18, 0.01, 1.66), (0.15, 0.19, 0.03), mat_book1)
    
    # Petite plante en pot
    create_cylinder("Pot_Plante", (0.35, 0.01, 1.66), radius=0.06, depth=0.10, material=mat_pot)
    for angle_deg in (0, 72, 144, 216, 288):
        rad = math.radians(angle_deg)
        lx = 0.35 + math.cos(rad) * 0.04
        ly = 0.01 + math.sin(rad) * 0.04
        create_box(f"Feuille_{angle_deg}", (lx, ly, 1.73), (0.04, 0.04, 0.08), mat_plant, bevel=0.005)

    # --- Lampe de bureau moderne sur le bureau (gauche) ---
    create_cylinder("Lampe_Base", (-0.62, 0.15, 0.76), radius=0.07, depth=0.015, material=mat_metal)
    create_cylinder("Lampe_Tige", (-0.62, 0.15, 0.90), radius=0.008, depth=0.28, material=mat_metal)
    create_box("Lampe_AbatJour", (-0.62, 0.10, 1.05), (0.12, 0.14, 0.10), mat_metal, bevel=0.01)

    # --- Cadre de poster mural (décalé à droite de l'étagère) ---
    create_box("Poster_Cadre", (0.95, 0.02, 1.85), (0.64, 0.02, 0.84), mat_poster, bevel=0.008)
    create_box("Poster_Fond", (0.95, 0.025, 1.85), (0.60, 0.01, 0.80), mat_book1)

    merge_by_material("Decor")
    out_path = MODELS_DIR / "Room_Decor.fbx"
    bpy.ops.export_scene.fbx(filepath=str(out_path), use_selection=False, apply_scale_options='FBX_SCALE_ALL')
    print("Room_Decor.fbx généré avec succès !")

if __name__ == "__main__":
    print("=== DÉBUT GÉNÉRATION MOBILIER CHAMBRE ===")
    build_bed()
    build_desk_setup()
    build_room_decor()
    print("=== TOUS LES MODÈLES ONT ÉTÉ GÉNÉRÉS AVEC SUCCÈS ===")
