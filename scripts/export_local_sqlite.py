#!/usr/bin/env python3
"""
Exporta una base SQLite de FinOptima a CSV para preparar su migración.
No exporta PasswordHash ni tokens. Los CSV contienen datos financieros privados.
Uso:
  python export_local_sqlite.py "C:\\ruta\\finoptima.db" "C:\\ruta\\exportacion"
"""
import csv
import json
import os
import sqlite3
import sys
import zipfile
from datetime import datetime, timezone

TABLES = {
    "Users": "SELECT Id, FullName, Email, CreatedAt, Active, Role FROM Users ORDER BY Id",
    "Envelopes": "SELECT Id, UserId, Name, Type, TargetPercent, MinPercent, MaxPercent, Balance, Description, Active FROM Envelopes ORDER BY Id",
    "Transactions": "SELECT Id, UserId, EnvelopeId, Type, Category, Description, Amount, Date FROM Transactions ORDER BY Id",
    "Goals": "SELECT Id, UserId, Name, TargetAmount, CurrentAmount, Deadline, Priority FROM Goals ORDER BY Id",
    "Debts": "SELECT Id, UserId, Creditor, Principal, Balance, InterestRate, MinimumPayment, DueDate FROM Debts ORDER BY Id",
    "Plans": "SELECT Id, UserId, Month, Income, Allocated, CreatedAt FROM Plans ORDER BY Id",
    "AuditLog": "SELECT Id, AdminUserId, Action, Details, CreatedAt FROM AuditLog ORDER BY Id",
}

def main():
    if len(sys.argv) != 3:
        print("Uso: python export_local_sqlite.py <finoptima.db> <carpeta_salida>")
        return 2
    db_path, output_dir = sys.argv[1], sys.argv[2]
    if not os.path.isfile(db_path):
        print("No se encontró la base de datos:", db_path)
        return 2
    os.makedirs(output_dir, exist_ok=True)
    stamp = datetime.now().strftime("%Y%m%d_%H%M%S")
    bundle_dir = os.path.join(output_dir, "FinOptima_Export_" + stamp)
    os.makedirs(bundle_dir, exist_ok=True)
    conn = sqlite3.connect(f"file:{os.path.abspath(db_path)}?mode=ro", uri=True)
    try:
        integrity = conn.execute("PRAGMA integrity_check").fetchone()[0]
        if integrity.lower() != "ok":
            raise RuntimeError("La base SQLite no superó integrity_check: " + str(integrity))
        exported = {}
        for table, query in TABLES.items():
            exists = conn.execute(
                "SELECT 1 FROM sqlite_master WHERE type='table' AND name=?", (table,)
            ).fetchone()
            if not exists:
                continue
            cur = conn.execute(query)
            headers = [d[0] for d in cur.description]
            rows = cur.fetchall()
            path = os.path.join(bundle_dir, table + ".csv")
            with open(path, "w", newline="", encoding="utf-8-sig") as f:
                writer = csv.writer(f)
                writer.writerow(headers)
                writer.writerows(rows)
            exported[table] = {"rows": len(rows), "file": table + ".csv"}
        manifest = {
            "created_utc": datetime.now(timezone.utc).isoformat(),
            "source_file": os.path.basename(db_path),
            "integrity_check": integrity,
            "tables": exported,
            "password_hashes_exported": False,
            "note": "Los usuarios deben establecer nuevas contraseñas al migrar a Supabase Auth."
        }
        with open(os.path.join(bundle_dir, "manifest.json"), "w", encoding="utf-8") as f:
            json.dump(manifest, f, ensure_ascii=False, indent=2)
    finally:
        conn.close()

    zip_path = bundle_dir + ".zip"
    with zipfile.ZipFile(zip_path, "w", zipfile.ZIP_DEFLATED) as archive:
        for name in os.listdir(bundle_dir):
            archive.write(os.path.join(bundle_dir, name), arcname=name)
    print("Exportación creada:", zip_path)
    print("ADVERTENCIA: contiene datos financieros privados. Guárdala de forma segura y no la subas a GitHub.")
    return 0

if __name__ == "__main__":
    raise SystemExit(main())
