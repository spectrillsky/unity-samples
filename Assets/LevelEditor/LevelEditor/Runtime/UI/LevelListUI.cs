using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace LFG.LevelEditor.UI
{
    public class LevelListUI : MonoBehaviour
    {
        [SerializeField] private GameObject panel;
        [SerializeField] private Button hideButton;
        [SerializeField] private Transform list;
        [SerializeField] private LevelListItem listItemPrefab;

        private bool isActive = false;
        public bool IsActive { get { return isActive; } }
        
        
        public class OnLevelSelected : UnityEvent<LevelData> { };
        public OnLevelSelected onLevelSelected = new OnLevelSelected();

        void Start()
        {
            hideButton.onClick.AddListener(Hide);
            Hide();
        }

        public void Show()
        {
            panel.SetActive(true);
            PopulateList();
            isActive = true;
        }

        public void Hide()
        {
            panel.SetActive(false);
            ClearList();
            isActive = false;
        }
        
        void PopulateList()
        {
            ClearList();
            foreach (var level in Controller.instance.GetLevels())
            {
                var item = Instantiate(listItemPrefab, list);
                item.Init(level);
                item.selectButton.onClick.AddListener(() => OnLevelItemSelected(level));
            }
        }

        void ClearList()
        {
            foreach(var item in list.GetComponentsInChildren<LevelListItem>())
                Destroy(item.gameObject);
        }

        void OnLevelItemSelected(LevelData levelData)
        {   
            onLevelSelected.Invoke(levelData);
            Hide();
        }
    }
}
