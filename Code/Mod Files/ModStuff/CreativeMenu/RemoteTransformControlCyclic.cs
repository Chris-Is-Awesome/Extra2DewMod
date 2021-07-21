using UnityEngine;

namespace ModStuff.CreativeMenu
{
    class RemoteTransformControlCyclic : MonoBehaviour //, IUpdatable, IBaseUpdateable
    {
        public enum TransformType { POS, ROT, SCALE, ONE_AXIS_SCALE, PERMA_ROT }
        public enum InterpType { EASYINOUT, EASYIN, EASYOUT, LINEAR }

        [SerializeField] TransformType _type;
        public TransformType ControlType
        {
            get { return _type; }
            set
            {
                _type = value;
                MeshFilter meshFilter = GetComponent<MeshFilter>();
                meshFilter.mesh.Clear();
                switch (value)
                {
                    case TransformType.POS:
                        meshFilter.mesh = MechanismManager.CreateCone();
                        break;
                    case TransformType.ROT:
                        meshFilter.mesh = MechanismManager.CreateLongShiftedCube();
                        break;
                    case TransformType.SCALE:
                        meshFilter.mesh = MechanismManager.CreateCube();
                        break;
                    case TransformType.ONE_AXIS_SCALE:
                        meshFilter.mesh = MechanismManager.CreateCube();
                        break;
                }
            }
        }
        
        [SerializeField] KeyCode _toggleKey;
        [SerializeField] float _currentPos;
        [SerializeField] float _timer;
        [SerializeField] AnimationCurve _curve;

        MeshRenderer _renderer;
        [SerializeField] bool _active = true;

        void Awake()
        {
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
            if (!_active || _curve == null) return;
            
            _timer += Time.deltaTime;
            float newPos = _curve.Evaluate(_timer);
            float delta = newPos - _currentPos;
            _currentPos = newPos;
            switch (ControlType)
            {
                case TransformType.POS:
                    transform.position += transform.up * delta;
                    break;
                case TransformType.ROT:
                    transform.Rotate(Vector3.forward * delta);
                    //ModText.QuickText(Vector3.forward.ToString());
                    //transform.rotation = transform.rotation * Quaternion.Euler(Vector3.forward * delta);
                    //transform.RotateAround(Vector3.forward, rotation = transform.rotation * Quaternion.Euler(Vector3.forward * delta);
                    break;
                case TransformType.SCALE:
                    float newScale = transform.localScale.x + delta;
                    transform.localScale = new Vector3(newScale, newScale, newScale);
                    break;
                case TransformType.ONE_AXIS_SCALE:
                    float newScaleOneAxis = transform.localScale.x + delta;
                    transform.localScale = new Vector3(newScaleOneAxis, 1f, 1f);
                    break;
            }
        }

        public void SetupCurve(float target, float time, InterpType type)
        {
            if (time <= 0f) return;
            time *= 0.5f;
            _curve = new AnimationCurve();
            _curve.preWrapMode = WrapMode.PingPong;
            _curve.postWrapMode = WrapMode.PingPong;

            float tangent = target / time;
            float interp0 = 0f;
            float interp1 = 0f;
            switch (type)
            {
                case InterpType.EASYINOUT:
                    interp0 = 0f;
                    interp1 = 0f;
                    break;
                case InterpType.EASYIN:
                    interp0 = 0f;
                    interp1 = tangent;
                    break;
                case InterpType.EASYOUT:
                    interp0 = tangent;
                    interp1 = 0f;
                    break;
                case InterpType.LINEAR:
                    interp0 = tangent;
                    interp1 = tangent;
                    break;
            }
            _curve.AddKey(new Keyframe(0f, 0f, 0f, interp0));
            _curve.AddKey(new Keyframe(time, target, interp1, 0f));
        }

        public void SetupKeys(KeyCode toggle)
        {
            _toggleKey = toggle;
        }
    }
}
