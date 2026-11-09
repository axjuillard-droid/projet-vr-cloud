"""Recalculer les mesures ABBA ou l'audit du rendu depuis les CSV du player visible."""
import argparse
import csv
import json
import math
from pathlib import Path
import statistics


def distribution(values):
    ordered = sorted(values)
    if not ordered:
        raise ValueError("Aucun échantillon valide ; ne pas interpréter zéro comme une mesure.")
    return {
        "count": len(ordered),
        "medianMs": statistics.median(ordered),
        "p95Ms": ordered[math.ceil(len(ordered) * .95) - 1],
        "meanMs": statistics.mean(ordered),
    }


def summarize(folder):
    report = json.loads((folder / "performance-result.json").read_text(encoding="utf-8-sig"))
    if not report["passed"] or not report["gpuAvailable"]:
        raise ValueError("Collecte sans GPU exploitable : aucune comparaison graphique produite.")
    if (report["width"], report["height"]) != (1280, 720):
        raise ValueError("Le protocole attend une résolution de 1280×720.")
    order = [phase["msaa"] for phase in report["phases"]]
    modes = [phase.get("mode") for phase in report["phases"]]
    audit = modes == ["reference", "ao-half", "no-ao", "no-shadows", "no-copies", "no-glass", "short-frustum", "reference"]
    shadows = modes == ["four-cascades", "two-cascades", "two-cascades", "four-cascades"]
    ao = modes == ["ao-on", "ao-off", "ao-off", "ao-on"]
    explicit_modes = audit or shadows or ao
    if explicit_modes and order != [4] * len(modes):
        raise ValueError("Ce protocole exige du MSAA 4x dans toutes les phases.")
    if not explicit_modes and order != [1, 4, 4, 1]:
        raise ValueError("Ordre ABBA inattendu.")
    groups = {}
    for phase in report["phases"]:
        if phase["focusedFrames"] != phase["frames"] or not phase["scaleUnchanged"] or phase["cpuRender"]["count"] == 0:
            raise ValueError("Phase sans rendu, non focalisée ou perturbée par une pause.")
        with (folder / f"phase-{phase['order']}.csv").open(encoding="utf-8", newline="") as source:
            rows = list(csv.DictReader(source))
        fresh = [row for row in rows if row["fresh"] == "1"]
        gpu = [float(row["gpuMs"]) for row in fresh if float(row["gpuMs"]) > 0]
        if len(rows) != phase["frames"] or len(fresh) != phase["timingSamples"] or len(gpu) != phase["gpuSamples"]:
            raise ValueError("Effectifs CSV et rapport incohérents.")
        if len({row["timestamp"] for row in fresh}) != len(fresh):
            raise ValueError("Timestamps dupliqués.")
        frames = [float(row["observedMs"]) for row in rows]
        for values, key in [(frames, "observedFrame"), (gpu, "gpu")]:
            calculated = distribution(values)
            for field in ["medianMs", "p95Ms", "meanMs"]:
                if abs(calculated[field] - phase[key][field]) > .001:
                    raise ValueError("Statistiques du rapport non reproduites par les CSV.")
        group = groups.setdefault(phase["mode"] if explicit_modes else phase["msaa"], {"frame": [], "gpu": []})
        group["frame"].extend(frames)
        group["gpu"].extend(gpu)
    result = {
        "scope": "Visible Development Player only; one ABBA series, two windows per mode, fixed 1280x720; no inference to release, other hardware or Quest. Hidden run has no rendering/GPU samples and is excluded.",
        "modes": {str(mode): {key: distribution(values) for key, values in metrics.items()} for mode, metrics in groups.items()},
    }
    if audit:
        result["scope"] = "Visible Development Player, fixed MSAA 4x and 1280x720. Reference at both ends; deltas are whole-frame interventions, not additive pass timings."
        reference = result["modes"]["reference"]
        for name, variant in result["modes"].items():
            if name != "reference":
                variant["frameMedianDeltaMs"] = variant["frame"]["medianMs"] - reference["frame"]["medianMs"]
                variant["gpuMedianDeltaMs"] = variant["gpu"]["medianMs"] - reference["gpu"]["medianMs"]
        return result
    before_mode, after_mode = "1", "4"
    if ao:
        before_mode, after_mode = "ao-on", "ao-off"
        result["scope"] = "Visible Development Player, 1280x720, MSAA 4x; ABBA SSAO on/off/off/on."
    if shadows:
        before_mode, after_mode = "four-cascades", "two-cascades"
        result["scope"] = "Visible Development Player, 1280x720, MSAA 4x; ABBA four/two/two/four cascades, fixed shadow distance and resolution."
    for key in ["frame", "gpu"]:
        before = result["modes"][before_mode][key]["medianMs"]
        after = result["modes"][after_mode][key]["medianMs"]
        result[key + "MedianDeltaMs"] = after - before
        result[key + "MedianDeltaPercent"] = (after / before - 1) * 100
    return result


if __name__ == "__main__":
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("folder", type=Path, help="Dossier du player contenant JSON et phase-*.csv")
    parser.add_argument("--output", type=Path, help="JSON à écrire ; sinon afficher le résumé")
    args = parser.parse_args()
    try:
        output = json.dumps(summarize(args.folder), ensure_ascii=False, indent=2) + "\n"
    except (ValueError, KeyError, OSError) as error:
        raise SystemExit(str(error)) from error
    if args.output:
        args.output.write_text(output, encoding="utf-8")
    else:
        print(output, end="")
