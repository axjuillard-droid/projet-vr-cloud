"""Modèle pédagogique de comparaison, sur une heure fictive à charge égale."""


SITES = {
    "france": {
        "label": "France", "climat": "18 °C · 65 % HR",
        "reseau_ms": 25, "tarif_eur_kwh": 0.10, "carbone_g_kwh": 50,
        "auxiliaires_kwh": 9, "appoint_l": 75,
    },
    "usa": {
        "label": "États-Unis", "climat": "29 °C · 35 % HR",
        "reseau_ms": 105, "tarif_eur_kwh": 0.08, "carbone_g_kwh": 380,
        "auxiliaires_kwh": 15, "appoint_l": 100,
    },
    "chine": {
        "label": "Chine", "climat": "26 °C · 75 % HR",
        "reseau_ms": 190, "tarif_eur_kwh": 0.09, "carbone_g_kwh": 530,
        "auxiliaires_kwh": 18, "appoint_l": 125,
    },
}

STAGES = ("envoi", "calcul", "refroidissement", "eau")
IT_KWH = 60
QUEUE_MS = 120
INFERENCE_MS = 700


def entry(energy, tariff, intensity):
    return {
        "kwh": round(energy, 6),
        "eur": round(energy * tariff, 6),
        "g_co2e": round(energy * intensity, 6),
    }


def comparison(stage, water_supply=None):
    level = STAGES.index(stage)
    results = []
    for key, site in SITES.items():
        tariff = site["tarif_eur_kwh"]
        intensity = site["carbone_g_kwh"]
        required_water = site["appoint_l"]
        supplied_water = required_water if water_supply is None else water_supply
        energy_by_component = {
            "serveurs": IT_KWH,
            "refroidissement": site["auxiliaires_kwh"],
            "pompe_appoint": supplied_water * 0.004,
        }
        revealed = list(energy_by_component)[:level]
        items = {
            name: entry(energy_by_component[name], tariff, intensity) for name in revealed
        }
        total_kwh = sum(energy_by_component[name] for name in revealed)
        results.append({
            "id": key,
            "label": site["label"],
            "climat": site["climat"],
            "tarif_eur_kwh": tariff,
            "carbone_g_kwh": intensity,
            "delai_ms": {
                "reseau_aller_retour": site["reseau_ms"],
                "file_attente": QUEUE_MS,
                "inference": INFERENCE_MS,
                "total": site["reseau_ms"] + QUEUE_MS + INFERENCE_MS,
            },
            "postes_reveles": items,
            "sous_total_revele": entry(total_kwh, tariff, intensity),
            "postes_a_reveler": list(energy_by_component)[level:],
            "eau": {
                "besoin_l": required_water,
                "appoint_l": supplied_water,
                "ecart_l": supplied_water - required_water,
            } if stage == "eau" else None,
        })
    return {
        "fiction": True,
        "etape": stage,
        "periode_minutes": 60,
        "joueur_pc_et_box": entry(0.07, 0.22, 50),
        "sites": results,
    }
