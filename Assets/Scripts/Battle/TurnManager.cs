using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// Gère l'ordre des tours basé sur la vitesse/initiative de chaque BattleUnit.
/// Utilise un système de "charge de tour" : chaque unité accumule de la charge
/// proportionnellement à sa vitesse. Quand elle atteint le seuil, c'est son tour.
/// Inspiré du système ATB de Final Fantasy.
/// </summary>
public class TurnManager
{
    private class TurnEntry
    {
        public BattleUnit Unit;
        public float      Charge;
    }

    private const float TURN_THRESHOLD = 100f;

    private readonly List<TurnEntry> _entries = new();

    // ── Setup ────────────────────────────────────────────────────

    public void Register(BattleUnit unit)
    {
        _entries.Add(new TurnEntry
        {
            Unit   = unit,
            Charge = UnityEngine.Random.Range(0f, 20f), // légère variation au départ
        });
    }

    public void RegisterAll(IEnumerable<BattleUnit> units)
    {
        foreach (var u in units) Register(u);
    }

    public void Clear() => _entries.Clear();

    // ── Prochain tour ────────────────────────────────────────────

    /// <summary>
    /// Avance la simulation jusqu'à ce qu'une unité vivante soit prête à jouer.
    /// Retourne l'unité dont c'est le tour.
    /// </summary>
    public BattleUnit GetNextUnit()
    {
        // Filtre les unités mortes
        var alive = _entries.Where(e => !e.Unit.IsDead).ToList();
        if (alive.Count == 0) return null;

        // Avance les charges jusqu'à ce qu'une unité soit prête
        while (true)
        {
            foreach (var entry in alive)
                entry.Charge += entry.Unit.speed;

            var ready = alive.Where(e => e.Charge >= TURN_THRESHOLD)
                              .OrderByDescending(e => e.Charge)
                              .FirstOrDefault();

            if (ready != null)
            {
                ready.Charge -= TURN_THRESHOLD;
                return ready.Unit;
            }
        }
    }

    /// <summary>
    /// Retourne un aperçu des N prochains tours (pour affichage UI).
    /// Ne modifie pas l'état réel.
    /// </summary>
    public List<BattleUnit> PeekNextTurns(int count)
    {
        // Clone les charges pour simulation
        var sim = _entries
            .Where(e => !e.Unit.IsDead)
            .Select(e => new TurnEntry { Unit = e.Unit, Charge = e.Charge })
            .ToList();

        var result = new List<BattleUnit>();

        while (result.Count < count)
        {
            if (sim.Count == 0) break;

            foreach (var entry in sim)
                entry.Charge += entry.Unit.speed;

            var ready = sim.Where(e => e.Charge >= TURN_THRESHOLD)
                           .OrderByDescending(e => e.Charge)
                           .FirstOrDefault();

            if (ready != null)
            {
                ready.Charge -= TURN_THRESHOLD;
                result.Add(ready.Unit);
            }
        }

        return result;
    }

    /// <summary>Retire une unité morte de la liste de tours.</summary>
    public void Remove(BattleUnit unit)
    {
        _entries.RemoveAll(e => e.Unit == unit);
    }
}
