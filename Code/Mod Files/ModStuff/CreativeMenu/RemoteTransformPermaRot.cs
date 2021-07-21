using UnityEngine;

namespace ModStuff.CreativeMenu
{
    class RemoteTransformPermaRot : MonoBehaviour //, IUpdatable, IBaseUpdateable
    {
        [SerializeField] KeyCode _toggleKey;
        [SerializeField] bool _active = true;
        [SerializeField] float _rate;

        MeshRenderer _renderer;

        void Awake()
        {
            MeshFilter meshFilter = GetComponent<MeshFilter>();
            meshFilter.mesh.Clear();
            meshFilter.mesh = MechanismManager.CreateLongShiftedCube();
            _renderer = GetComponent<MeshRenderer>();
        }

        void Update()
        {
            if (MechanismManager.MeshVisibility != _renderer.enabled)
            {
                _renderer.enabled = MechanismManager.MeshVisibility;
            }

            if (MechanismManager.DoUpdate()) return;

            if (Input.GetKeyDown(_toggleKey)) _active = !_active;
            if (!_active) return;
            transform.Rotate(Vector3.forward * Time.deltaTime * _rate);
        }

        public void SetupTime(float time)
        {
            if (time <= 0f) return;
            _rate = 360f / time;
        }

        public void SetupKeys(KeyCode toggle)
        {
            _toggleKey = toggle;
        }
    }
}
