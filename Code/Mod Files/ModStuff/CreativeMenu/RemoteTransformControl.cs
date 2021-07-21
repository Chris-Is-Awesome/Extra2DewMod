using UnityEngine;

namespace ModStuff.CreativeMenu
{
    class RemoteTransformControl : MonoBehaviour //, IUpdatable, IBaseUpdateable
    {
        public enum TransformType { POS, ROT, SCALE, ONE_AXIS_SCALE }
        
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

        [SerializeField] bool _isMouseControlled;
        [SerializeField] KeyCode _upKey;
        [SerializeField] KeyCode _downAndHoldKey;
        [SerializeField] float _rate;

        MeshRenderer _renderer;
        Vector3 _lastMousePosition;

        void Awake()
        {
            _renderer = GetComponent<MeshRenderer>();
        }

        void Update()
        {
            if(MechanismManager.MeshVisibility != _renderer.enabled)
            {
                _renderer.enabled = MechanismManager.MeshVisibility;
            }

            if (MechanismManager.DoUpdate()) return;
            float changeAmount;
            if (_isMouseControlled)
            {
                changeAmount = Input.mousePosition.x - _lastMousePosition.x;
                _lastMousePosition = Input.mousePosition;
                if (!Input.GetKey(_downAndHoldKey)) return;
            }
            else if(Input.GetKey(_upKey))
            {
                changeAmount = 1f;
            }
            else if (Input.GetKey(_downAndHoldKey))
            {
                changeAmount = -1f;
            }
            else
            {
                return;
            }

            changeAmount *= _rate * Time.deltaTime;

            switch (ControlType)
            {
                case TransformType.POS:
                    transform.position += transform.up * changeAmount;
                    break;
                case TransformType.ROT:
                    transform.Rotate(Vector3.forward * changeAmount);
                    break;
                case TransformType.SCALE:
                    float newScale = transform.localScale.x + changeAmount;
                    transform.localScale = new Vector3(newScale, newScale, newScale);
                    break;
                case TransformType.ONE_AXIS_SCALE:
                    float newScaleOneAxis = transform.localScale.x + changeAmount;
                    transform.localScale = new Vector3(newScaleOneAxis, 1f, 1f);
                    break;
            }
        }

        public void SetupControlMode(bool useMouse, bool invertedMouse, float rate)
        {
            _isMouseControlled = useMouse;
            _rate = rate;
        }

        public void SetupKeys(KeyCode downAndHold, KeyCode up)
        {
            _upKey = up;
            _downAndHoldKey = downAndHold;
        }
    }
}
