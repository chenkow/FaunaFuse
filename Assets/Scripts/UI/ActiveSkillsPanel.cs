using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using Core;
using Managers;

namespace UI
{
    public class ActiveSkillsPanel : MonoBehaviour
    {
        [Header("Skill Button Prefab")]
        public SkillButtonUI buttonPrefab;
        public Transform buttonContainer;
        
        [Header("Selection Mode")]
        public GameObject selectionOverlay;
        public TMPro.TextMeshProUGUI selectionPromptText;
        
        private List<SkillButtonUI> activeButtons = new List<SkillButtonUI>();
        private ActiveSkillMode currentMode = ActiveSkillMode.None;
        private int selectedX = -1, selectedY = -1;
        
        public enum ActiveSkillMode 
        { 
            None, 
            SelectRow, 
            SelectColumn, 
            SelectTile, 
            SelectFirstTile, 
            SelectSecondTile 
        }
        
        private void Start()
        {
            if (selectionOverlay) selectionOverlay.SetActive(false);
            
            if (PerkManager.Instance != null)
            {
                PerkManager.Instance.OnLevelUp += RefreshButtons;
            }
        }
        
        public void RefreshButtons(int level)
        {
            // Clear existing
            foreach (var btn in activeButtons)
            {
                if (btn != null) Destroy(btn.gameObject);
            }
            activeButtons.Clear();
            
            if (PerkManager.Instance == null || buttonPrefab == null) return;
            
            // Find all one-time-use perks
            foreach (var perk in PerkManager.Instance.activePerks)
            {
                if (perk.Definition.isOneTimeUse && perk.Level > 0)
                {
                    var btn = Instantiate(buttonPrefab, buttonContainer);
                    btn.Setup(perk.Definition, perk.Level, OnSkillButtonClicked);
                    activeButtons.Add(btn);
                }
            }
        }
        
        private void OnSkillButtonClicked(PerkSO perk)
        {
            string perkName = perk.perkName.ToLower();
            
            if (perkName.Contains("shuffle"))
            {
                ExecuteShuffle(perk);
            }
            else if (perkName.Contains("row") || perkName.Contains("satir"))
            {
                EnterSelectionMode(ActiveSkillMode.SelectRow, "Select a ROW to wipe", perk);
            }
            else if (perkName.Contains("column") || perkName.Contains("sutun"))
            {
                EnterSelectionMode(ActiveSkillMode.SelectColumn, "Select a COLUMN to wipe", perk);
            }
            else if (perkName.Contains("throw") || perkName.Contains("remove"))
            {
                EnterSelectionMode(ActiveSkillMode.SelectTile, "Select an animal to remove", perk);
            }
            else if (perkName.Contains("swap"))
            {
                EnterSelectionMode(ActiveSkillMode.SelectFirstTile, "Select FIRST animal to swap", perk);
            }
        }
        
        private PerkSO pendingPerk;
        
        private void EnterSelectionMode(ActiveSkillMode mode, string prompt, PerkSO perk)
        {
            currentMode = mode;
            pendingPerk = perk;
            
            if (selectionOverlay) selectionOverlay.SetActive(true);
            if (selectionPromptText) selectionPromptText.text = prompt;
            
            // Disable normal input
            if (BoardManager.Instance) BoardManager.Instance.IsInputActive = false;
        }
        
        public void OnTileClicked(int x, int y)
        {
            if (currentMode == ActiveSkillMode.None) return;
            
            switch (currentMode)
            {
                case ActiveSkillMode.SelectRow:
                    BoardManager.Instance.WipeRow(y);
                    ConsumePerk();
                    break;
                    
                case ActiveSkillMode.SelectColumn:
                    BoardManager.Instance.WipeColumn(x);
                    ConsumePerk();
                    break;
                    
                case ActiveSkillMode.SelectTile:
                    BoardManager.Instance.RemoveTileAt(x, y);
                    ConsumePerk();
                    break;
                    
                case ActiveSkillMode.SelectFirstTile:
                    selectedX = x;
                    selectedY = y;
                    currentMode = ActiveSkillMode.SelectSecondTile;
                    if (selectionPromptText) selectionPromptText.text = "Select SECOND animal to swap";
                    return; // Don't exit yet
                    
                case ActiveSkillMode.SelectSecondTile:
                    BoardManager.Instance.SwapTiles(selectedX, selectedY, x, y);
                    ConsumePerk();
                    break;
            }
            
            ExitSelectionMode();
        }
        
        private void ConsumePerk()
        {
            // Reduce perk level or remove
            if (PerkManager.Instance != null && pendingPerk != null)
            {
                var data = PerkManager.Instance.activePerks.Find(p => p.Definition == pendingPerk);
                if (data != null)
                {
                    data.Level--;
                    if (data.Level <= 0)
                    {
                        PerkManager.Instance.activePerks.Remove(data);
                    }
                }
                RefreshButtons(PerkManager.Instance.currentLevel);
            }
        }
        
        public void CancelSelection()
        {
            ExitSelectionMode();
        }
        
        private void ExitSelectionMode()
        {
            currentMode = ActiveSkillMode.None;
            pendingPerk = null;
            selectedX = -1;
            selectedY = -1;
            
            if (selectionOverlay) selectionOverlay.SetActive(false);
            if (BoardManager.Instance) BoardManager.Instance.IsInputActive = true;
        }
        
        private void ExecuteShuffle(PerkSO perk)
        {
            if (BoardManager.Instance) BoardManager.Instance.ShuffleBoard();
            
            // Consume
            pendingPerk = perk;
            ConsumePerk();
        }
    }
}
