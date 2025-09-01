using UnityEngine;
using System.Collections.Generic;

public class StatsUI : MonoBehaviour
{
    [Header("References")]
    public RPGStats rpgStats;              // Drag your Player (with RPGStats) here
    public Transform skillsContainer;      // The parent that will hold the rows
    public GameObject skillEntryPrefab;    // The prefab with SkillEntryUI

    private readonly Dictionary<string, SkillEntryUI> entries = new();

    private void OnEnable()
    {
        BuildList();
        Subscribe();
        RefreshAll();
    }

    private void OnDisable()
    {
        Unsubscribe();
        ClearList();
    }

    private void BuildList()
    {
        if (rpgStats == null || skillsContainer == null || skillEntryPrefab == null) return;

        ClearList();

        // Iterate over your skills array in RPGStats
        foreach (var skill in rpgStats.skills)
        {
            if (skill == null) continue;

            var go = Instantiate(skillEntryPrefab, skillsContainer);
            var ui = go.GetComponent<SkillEntryUI>();
            if (ui == null)
            {
                Debug.LogError("SkillEntry prefab is missing SkillEntryUI component.");
                Destroy(go);
                continue;
            }

            // Initialize UI once
            ui.SetSkillInfo(skill.skillName, skill.level, skill.currentXP, skill.xpToNextLevel);
            entries[skill.skillName] = ui;
        }
    }

    private void ClearList()
    {
        foreach (Transform child in skillsContainer)
            Destroy(child.gameObject);
        entries.Clear();
    }

    private void Subscribe()
    {
        if (rpgStats == null) return;

        foreach (var skill in rpgStats.skills)
        {
            if (skill == null) continue;

            // Capture local reference to use inside the lambda
            var s = skill;
            s.OnSkillChanged.AddListener((level, currentXP, xpToNext) =>
            {
                if (entries.TryGetValue(s.skillName, out var ui))
                {
                    ui.SetSkillInfo(s.skillName, level, currentXP, xpToNext);
                }
            });
        }
    }

    private void Unsubscribe()
    {
        if (rpgStats == null) return;

        foreach (var skill in rpgStats.skills)
        {
            if (skill == null) continue;
            // Remove all listeners added here (optional: keep if you use persistent listeners elsewhere)
            skill.OnSkillChanged.RemoveAllListeners();
        }
    }

    private void RefreshAll()
    {
        if (rpgStats == null) return;

        foreach (var skill in rpgStats.skills)
        {
            if (skill == null) continue;
            if (entries.TryGetValue(skill.skillName, out var ui))
            {
                ui.SetSkillInfo(skill.skillName, skill.level, skill.currentXP, skill.xpToNextLevel);
            }
        }
    }
}
