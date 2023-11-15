using System.Collections.Generic;
using UnityEngine;


namespace LFG.LevelEditor.UI
{
    public class LevelObjectGalleryUI : Singleton<LevelObjectGalleryUI>
    {
        [SerializeField] private Transform grid;
        [SerializeField] private GalleryItem galleryItemPrefab;

        [SerializeField] private Transform filterList;
        [SerializeField] private GalleryFilterItem galleryFilterItemPrefab;
        [SerializeField] private List<GalleryFilterItem> _filterItems = new List<GalleryFilterItem>();
        
        
        private LevelObjectType _currentFilterType;

        void Awake()
        {
            Controller.OnAction?.AddListener(OnLevelEditorAction);
        }

        void OnLevelEditorAction(Controller.Actions action, object context)
        {
            if (action == Controller.Actions.ChangeState)
            {
                Controller.State state = (Controller.State)context;
                if (state == Controller.State.Editing)
                {
                    PopulateFilters();
                    PopulateGallery();
                }
            }
        }

        public void SetFilterType(LevelObjectType type = null)
        {
            _currentFilterType = type;
            PopulateGallery();
            foreach(var filterItem in _filterItems)
                filterItem.SetSelected(type == filterItem.Type);
        }

        void ClearGallery()
        {
            foreach(var child in grid.GetComponentsInChildren<GalleryItem>())
                Destroy(child.gameObject);
        }

        void ClearFilters()
        {
            foreach(var child in filterList.GetComponentsInChildren<GalleryFilterItem>())
                Destroy(child.gameObject);
            _filterItems.Clear();
        }
        
        void PopulateFilters()
        {
            ClearFilters();
            
            //All filter
            var allFilter = Instantiate(galleryFilterItemPrefab, filterList);
            allFilter.Init();
            _filterItems.Add(allFilter);
                
            LevelObjectType[] types = Utility.GetTypes(Controller.Settings.LevelObjects);
            foreach (var type in types)
            {
                if (!type.ShowInGalleryFilters) continue;
                
                var filterItem = Instantiate(galleryFilterItemPrefab, filterList);
                filterItem.Init(type);
                _filterItems.Add(filterItem);
            }
            SetFilterType();
        }
        
        void PopulateGallery()
        {
            ClearGallery();
            List<LevelObjectProfile> lvlObjs = Controller.Settings.GetLevelObjects(_currentFilterType);
            foreach (var lvlObj in lvlObjs)
            {
                GalleryItem item = Instantiate(galleryItemPrefab, grid);
                item.Init(lvlObj);
            }
        }
    }
}
