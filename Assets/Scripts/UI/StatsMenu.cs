using UnityEngine;

public class StatsMenu : MonoBehaviour
{
    [Header("Assign the UI panel to toggle")]
    public GameObject statsPanel;

    [Header("Keybinding")]
    public KeyCode toggleKey = KeyCode.Tab;

    private void Start()
    {
        if (statsPanel != null) statsPanel.SetActive(false);
    }

    private void Update()
    {
        if (Input.GetKeyDown(toggleKey) && statsPanel != null)
            statsPanel.SetActive(!statsPanel.activeSelf);
    }
}
