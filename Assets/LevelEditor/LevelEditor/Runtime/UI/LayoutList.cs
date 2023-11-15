using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace LFG.LevelEditor.UI
{
    public class LayoutList : MonoBehaviour
    {
        [SerializeField] private GameObject panel;
        [SerializeField] private Button hideButton;
        [SerializeField] private Transform list;
        [SerializeField] private GameObject listItemPrefab;

        private bool isActive = false;
        public bool IsActive { get { return isActive; } }
        
        
        public class OnLayoutSelected : UnityEvent<Level> { };
        public OnLayoutSelected onLayoutSelected = new OnLayoutSelected();

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
            foreach (var layout in Controller.Settings.Layouts)
            {
                var item = Instantiate(listItemPrefab, list).GetComponent<LayoutListItem>();
                
                item.Initialize(layout);
                item.SelectButton.onClick.AddListener(() => OnItemSelected(layout)); //dont ask
                
                if(layout == Controller.Settings.DefaultLayout) item.Select();
            }
        }

        void ClearList()
        {
            foreach(var item in list.GetComponentsInChildren<LayoutListItem>())
                Destroy(item.gameObject);
        }

        void OnItemSelected(Level layout)
        {   
            onLayoutSelected.Invoke(layout);
            Hide();
        }
    }
}
