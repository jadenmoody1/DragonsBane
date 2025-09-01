using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SkillEntryUI : MonoBehaviour
{
    [Header("Hook these up in the prefab")]
    public TMP_Text skillNameText;
    public TMP_Text levelText;
    public Slider xpSlider;

    public void SetSkillInfo(string skillName, int level, float currentXP, float xpToNext)
    {
        if (skillNameText) skillNameText.text = skillName;
        if (levelText) levelText.text = "Lvl " + level;

        if (xpSlider)
        {
            xpSlider.minValue = 0f;
            xpSlider.maxValue = Mathf.Max(1f, xpToNext); // avoid zero max
            xpSlider.value   = Mathf.Clamp(currentXP, 0f, xpSlider.maxValue);
        }
    }
}
