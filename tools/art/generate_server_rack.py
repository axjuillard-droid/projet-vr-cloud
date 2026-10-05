"""Baie originale CloudVR, unités métriques ; exécuter avec Blender --background.

Aucun asset, texture ou modèle tiers. Les petites pièces sont fusionnées par
matériau : sept meshes réutilisables, sans lumière/caméra ni collider exportés.
"""
import bpy
import json
import math
from pathlib import Path

ROOT = Path(__file__).resolve().parents[2]
EXPORT = ROOT / "ConnectionTest/Assets/CloudVR/Models/ServerRack_Detailed.fbx"
SOURCE = ROOT / "art/source/ServerRack.blend"
REPORT = ROOT / "art/server-rack-report.json"

bpy.ops.object.select_all(action="SELECT")
bpy.ops.object.delete(use_global=False)
scene = bpy.context.scene
scene.unit_settings.system = "METRIC"
scene.unit_settings.scale_length = 1.0

COLORS = {
    "Rack_Shell": (0.055, 0.068, 0.083, 1),
    "Rack_Equipment": (0.27, 0.30, 0.34, 1),
    "Rack_Metal": (0.48, 0.53, 0.58, 1),
    "Rack_Recess": (0.013, 0.020, 0.029, 1),
    "Rack_Cyan": (0.05, 0.62, 0.77, 1),
    "Rack_Green": (0.13, 0.75, 0.39, 1),
    "Rack_Amber": (0.95, 0.51, 0.08, 1),
}
materials, groups = {}, {key: [] for key in COLORS}
for name, color in COLORS.items():
    mat = bpy.data.materials.new(name)
    mat.diffuse_color = color
    mat.use_nodes = True
    bsdf = mat.node_tree.nodes.get("Principled BSDF")
    bsdf.inputs["Base Color"].default_value = color
    bsdf.inputs["Metallic"].default_value = 0.45 if name in ("Rack_Metal", "Rack_Equipment") else 0.15
    bsdf.inputs["Roughness"].default_value = 0.38
    materials[name] = mat


def box(name, xyz, dims, mat, bevel=0.0):
    bpy.ops.mesh.primitive_cube_add(size=1, location=xyz)
    obj = bpy.context.object
    obj.name = name
    obj.dimensions = dims
    bpy.ops.object.transform_apply(location=False, rotation=False, scale=True)
    obj.data.materials.append(materials[mat])
    if bevel:
        mod = obj.modifiers.new("Aretes", "BEVEL")
        mod.width = bevel
        mod.segments = 1
        bpy.ops.object.modifier_apply(modifier=mod.name)
    groups[mat].append(obj)
    return obj


def bolt(x, y, z):
    bpy.ops.mesh.primitive_cylinder_add(vertices=6, radius=0.006, depth=0.006,
                                      location=(x, y, z), rotation=(math.pi / 2, 0, 0))
    obj = bpy.context.object
    obj.name = "Vis"
    obj.data.materials.append(materials["Rack_Metal"])
    groups["Rack_Metal"].append(obj)


# Blender : X largeur, Y profondeur (façade -Y), Z hauteur. Pied au sol.
box("Socle", (0, 0, 0.055), (1.1, 1.0, 0.11), "Rack_Shell", 0.008)
box("Toit", (0, 0, 2.155), (1.1, 1.0, 0.09), "Rack_Shell", 0.007)
for x in (-0.525, 0.525):
    box("Panneau_lateral", (x, 0, 1.12), (0.05, 0.94, 2.03), "Rack_Shell", 0.004)
    for z in (0.27, 1.94):
        for i in range(9):
            box("Ventilation_laterale", (x * 1.052, 0.20, z + i * 0.014),
                (0.003, 0.45, 0.005), "Rack_Recess")
box("Arriere", (0, 0.46, 1.12), (1.03, 0.06, 2.03), "Rack_Recess")
box("Fond_serveurs", (0, -0.39, 1.12), (1.0, 0.035, 2.0), "Rack_Recess")
for x in (-0.477, 0.477):
    box("Rail_fixation", (x, -0.446, 1.12), (0.038, 0.038, 2.0), "Rack_Metal", 0.003)
    for i in range(28):
        box("Perforation_rail", (x, -0.468, 0.18 + i * 0.068), (0.012, 0.006, 0.012), "Rack_Recess")
    box("Cadre_avant", (x * 1.08, -0.468, 1.12), (0.042, 0.05, 2.03), "Rack_Shell", 0.004)

# Serveurs, switch et panneaux de stockage volontairement génériques.
for row in range(9):
    z = 0.24 + row * 0.203
    box("Facade_serveur", (0, -0.454, z), (0.906, 0.044, 0.17), "Rack_Equipment", 0.003)
    for x in (-0.416, 0.416):
        box("Poignee", (x, -0.488, z), (0.018, 0.026, 0.112), "Rack_Metal", 0.003)
        bolt(x, -0.496, z - 0.055)
        bolt(x, -0.496, z + 0.055)
    if row == 7:
        for port in range(12):
            x = -0.31 + port * 0.047
            box("Port_reseau", (x, -0.48, z), (0.036, 0.008, 0.04), "Rack_Recess")
            box("Etat_port", (x, -0.487, z + 0.029), (0.007, 0.004, 0.004), "Rack_Green")
    elif row < 3:
        for drive in range(5):
            x = -0.31 + drive * 0.138
            box("Tiroir_disque", (x, -0.48, z), (0.118, 0.014, 0.105), "Rack_Shell", 0.002)
            box("Poignee_disque", (x, -0.492, z - 0.023), (0.082, 0.008, 0.012), "Rack_Metal")
            box("Etat_disque", (x - 0.042, -0.496, z + 0.028), (0.012, 0.003, 0.005), "Rack_Green")
    else:
        for vent in range(14):
            x = -0.32 + vent * 0.042
            box("Fente_air", (x, -0.48, z), (0.022, 0.006, 0.105), "Rack_Recess")
    box("Bouton", (0.34, -0.482, z + 0.029), (0.025, 0.009, 0.023), "Rack_Metal", 0.003)
    box("Etat", (0.34, -0.49, z - 0.026), (0.017, 0.005, 0.006), "Rack_Green")
    box("Activite", (0.369, -0.49, z - 0.026), (0.009, 0.005, 0.006), "Rack_Amber")

box("Bande_identification", (0, -0.478, 2.057), (0.9, 0.035, 0.067), "Rack_Equipment", 0.003)
box("Trait_identification", (-0.32, -0.50, 2.057), (0.22, 0.006, 0.011), "Rack_Cyan")
for x in (-0.50, 0.50):
    box("Profil_lateral", (x, -0.501, 1.10), (0.007, 0.005, 1.95), "Rack_Cyan")

# Fusion et origine commune : aucun coût par petite vis dans Unity.
merged = []
for material, pieces in groups.items():
    bpy.ops.object.select_all(action="DESELECT")
    for obj in pieces:
        obj.select_set(True)
    bpy.context.view_layer.objects.active = pieces[0]
    bpy.ops.object.join()
    obj = bpy.context.object
    obj.name = material
    scene.cursor.location = (0, 0, 0)
    bpy.ops.object.origin_set(type="ORIGIN_CURSOR")
    # Chaque groupe possède une seule matière et des UV simples par face.
    obj.data.materials.clear()
    obj.data.materials.append(materials[material])
    for polygon in obj.data.polygons:
        polygon.material_index = 0
    merged.append(obj)

for path in (EXPORT, SOURCE, REPORT):
    path.parent.mkdir(parents=True, exist_ok=True)
bpy.ops.wm.save_as_mainfile(filepath=str(SOURCE))
bpy.ops.object.select_all(action="SELECT")
bpy.ops.export_scene.fbx(filepath=str(EXPORT), use_selection=True,
                         object_types={"MESH"}, axis_forward="-Z", axis_up="Y",
                         bake_space_transform=True, apply_unit_scale=True,
                         apply_scale_options="FBX_SCALE_ALL", mesh_smooth_type="FACE",
                         use_triangles=True, bake_anim=False, add_leaf_bones=False,
                         use_custom_props=False)
rows = []
for obj in merged:
    obj.data.calc_loop_triangles()
    rows.append({"mesh": obj.name, "vertices": len(obj.data.vertices),
                 "triangles": len(obj.data.loop_triangles)})
report = {"generator": "tools/art/generate_server_rack.py", "blender": bpy.app.version_string,
          "units": "metres", "nominalDimensions": {"width": 1.1, "height": 2.2, "depth": 1.0},
          "meshes": rows, "triangles": sum(row["triangles"] for row in rows),
          "materialGroups": len(rows), "thirdPartyAssets": False,
          "fbx": str(EXPORT.relative_to(ROOT)), "source": str(SOURCE.relative_to(ROOT))}
REPORT.write_text(json.dumps(report, ensure_ascii=False, indent=2) + "\n", encoding="utf-8")
print("CLOUDVR_RACK_GENERATED", json.dumps(report))
