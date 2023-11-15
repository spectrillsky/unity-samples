using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Rendering;

namespace LFG.LevelEditor
{
    public class LevelGrid : MonoBehaviour
    {
        public Vector3 _size;

        private GameObject lattice;

        private List<LineRenderer> lines = new List<LineRenderer>();


        
        void Start()
        {
            CreateLattice();
        }
        
        public void SetSize(Vector3 size)
        {
            _size = size;
            transform.localScale = new Vector3(size.x, transform.localScale.y, size.z);
            CreateLattice();
        }

        [Button]
        public void CreateLattice()
        {
            if (HasLattice()) DestroyLattice();
            if (!Controller.Settings.drawGridLattice) return;
            
            lines = new List<LineRenderer>();
            lattice = new GameObject("Lattice");
            lattice.transform.SetParent(transform);
            lattice.transform.localPosition = transform.lossyScale.y * Vector3.up;
            lattice.transform.localScale = Vector3.one;
            
            Level level = GetComponentInParent<Level>();

            Vector3 startPoint = transform.position - new Vector3(level.Size.x, 0, level.Size.z) / 2;
            
            Vector3 size = level.Size;
            int widthTicks = (int)(level.Size.x / Controller.Settings.cellSize);
            for (int i = 0; i <= widthTicks; i++)
            {
                Vector3 p0 = startPoint + Vector3.right * i * Controller.Settings.cellSize + (0.1f) * Vector3.up;
                Vector3 p1 = p0 + Vector3.forward * size.z;
                CreateLine(p0, p1);
            }
            int lengthTicks = (int)(level.Size.z / Controller.Settings.cellSize);
            for (int i = 0; i <= lengthTicks; i++)
            {
                Vector3 p0 = startPoint + Vector3.forward * Controller.Settings.cellSize * i + (0.1f) * Vector3.up;
                Vector3 p1 = p0 + Vector3.right * size.x;
                CreateLine(p0, p1, false);
            }
        }
        
        bool HasLattice()
        {
            var latticeTransform = transform.Find("Lattice");
            if (latticeTransform) lattice = latticeTransform.gameObject;
            else lattice = null;

            return lattice != null;
        }

        public void ToggleLattice(bool show)
        {
            if(HasLattice()) lattice.SetActive(show);
            else if(show) CreateLattice();
        }
        
        [Button, ShowIf("HasLattice")]
        public void DestroyLattice()
        {
            if(Application.isPlaying)
                Destroy(lattice);
            else
                DestroyImmediate(lattice);
        }

        void CreateLine(Vector3 startPoint, Vector3 endPoint, bool isMinor = false)
        {
            LineRenderer line = new GameObject("Line", typeof(LineRenderer)).GetComponent<LineRenderer>();
            line.transform.SetParent(lattice.transform);
            line.material = Controller.Settings.latticeMaterial;
            line.endColor = line.startColor = Color.black;
            line.endWidth = line.startWidth = Controller.Settings.latticeThickness;
            line.SetPosition(0, startPoint);
            line.SetPosition(1, endPoint);
            line.shadowCastingMode = ShadowCastingMode.Off;
            line.motionVectorGenerationMode = MotionVectorGenerationMode.ForceNoMotion;
            line.useWorldSpace = false;
            lines.Add(line);
        }
    }
}
