using System;
using UnityEngine;
using ModStuff;
using System.Collections.Generic;

namespace ModStuff
{
    public class UIKeyInput : UIElement
    {
        MappedInput _input;
        UIButton _button;
        public UIButton Button { get { return _button; } }

        GuiSelectionObject trackNode;
        public delegate void OnKeyPressed(KeyCode key);
        public event OnKeyPressed onKeyPressed;

        public delegate void OnInteraction();
        public event OnInteraction onStart;
        public event OnInteraction onCancel;

        bool _waitingForPrompt;
        bool _automaticName = true;
        int _afterPromptWait;
        string _name;
        public override string UIName
        {
            set
            {
                _name = value;
                _automaticName = false;
                UpdateValue();
            }
        }
        MappedInput.ButtonEventListener charListener;

        int ignoreFirstFramesDestroyUnity;

        public void Initialize(MappedInput input, UIButton button)
        {
            _input = input;
            _button = button;

            _button.onInteraction += delegate ()
            {
                if(_afterPromptWait == 0)
                {
                    _waitingForPrompt = true;
                    UpdateValue();
                    onStart?.Invoke();
                }
            };
            
            UpdateValue();
        }

        KeyCode _key = KeyCode.Q;
        public KeyCode Key
        {
            get
            {
                return _key;
            }
            set
            {
                _key = value;
                UpdateValue();
            }
        }

        void UpdateValue()
        {
            if(_waitingForPrompt)
            {
                _button.UIName = "Press any key";
            }
            else
            {
                if (_automaticName) _button.UIName = _key.ToString();
                else _button.UIName = _name;
            }
        }

        void GotKey(KeyCode key)
        {
            if (!_waitingForPrompt)
            {
                return;
            }
            bool notEscapePressed = key != KeyCode.Escape;
            _afterPromptWait = 2;
            _waitingForPrompt = false;
            if (notEscapePressed) _key = key;
            UpdateValue();
            if (notEscapePressed) onKeyPressed?.Invoke(key);
            else onCancel?.Invoke();
        }

        void Update()
        {
            if (ignoreFirstFramesDestroyUnity > 0)
            {
                ignoreFirstFramesDestroyUnity--;
                if (ignoreFirstFramesDestroyUnity == 0)
                {
                    charListener = _input.RegisterKeyDown(new MappedInput.KeyEventFunc(GotKey), -1);
                }
            }

            if (_afterPromptWait > 0)
            {
                _afterPromptWait--;
            }
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            this.ignoreFirstFramesDestroyUnity = 2;
            this.UpdateValue();
        }

        protected void OnDisable()
        {
            ClearListeners();
            if (_waitingForPrompt) onCancel?.Invoke();
            _waitingForPrompt = false;
        }

        void ClearListeners()
        {
            if (charListener != null)
            {
                charListener.Stop();
                charListener = null;
            }
        }

        void OnDestroy()
        {
            ClearListeners();
        }
    }
}
